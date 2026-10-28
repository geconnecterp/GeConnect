using gc.api.core.Contratos.Servicios;
using gc.api.core.Servicios.Libros;
using gc.api.core.Servicios.Reportes;
using gc.api.infra.Datos.Implementacion;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Implementacion.Libros;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Reflection;
using System.Security.Claims;

public static class SufridosPorCuitChecks
{
    public static async Task<int> Run(string salida, EmpresaGeco empresa)
    {
        var checks = 0;
        void Check(bool ok, string texto) { if (!ok) throw new Exception("FAIL: " + texto); checks++; Console.WriteLine("OK: " + texto); }
        async Task Reject<T>(Func<Task> action, string texto) where T : Exception
        { try { await action(); } catch (T) { Check(true, texto); return; } throw new Exception("FAIL: " + texto); }
        CertificadosSufridosFiltro Filtro() => new() { Impuesto = "IB", Periodo = IvaVentasFiltro.HoyArgentina().Year, Mes = 2, AgrupadoPorCuit = true, SepararCierre = true };
        var filtro = Filtro(); filtro.Validar();
        var p = CertificadosSufridosDatos.Parametros(filtro);
        Check(p.Length == 8 && (string)p[0].Value == "IB" && (string)p[2].Value == "02" && !filtro.SepararCierre, "Agrupado: ocho parámetros y sin separación inventada");
        Check(CertificadosSufridosDatos.SpPorCuit == "dbo.SPGECO_Imp_Reten_Percep_x_Cuit", "Nombre exacto x_Cuit del archivo entregado");
        DataTable Tabla()
        {
            var t = new DataTable();
            foreach (var prop in typeof(CertificadoSufridoPorCuitFila).GetProperties()) t.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            t.Rows.Add("IB", "P", "Percepción", "00123456789", "<b>Texto literal</b>", DBNull.Value, 9999999999999.99m, int.MaxValue);
            return t;
        }
        using (var t = Tabla()) using (var r = t.CreateDataReader())
        {
            Check(t.Columns.Count == 8, "Agrupado: ocho alias independientes del DTO de detalle");
            var map = CertificadosSufridosDatos.CrearMapeadorPorCuit(r); r.Read(); var fila = map(r);
            Check(fila.cm_cuit == "00123456789" && fila.percepcion == 9999999999999.99m && fila.percepcion_cant == int.MaxValue && fila.cm_domicilio == null, "Agrupado: conserva ceros, precisión, COUNT int y NULL");
        }
        using (var t = Tabla()) { t.Columns["percepcion_cant"]!.ColumnName = "cantidad"; using var r = t.CreateDataReader(); await Reject<ArgumentException>(() => { CertificadosSufridosDatos.CrearMapeadorPorCuit(r); return Task.CompletedTask; }, "Agrupado: cambio de alias rechazado"); }
        using (var t = Tabla()) { t.Columns.Add("base", typeof(decimal)); using var r = t.CreateDataReader(); await Reject<InvalidOperationException>(() => { CertificadosSufridosDatos.CrearMapeadorPorCuit(r); return Task.CompletedTask; }, "Agrupado: columna extra no se materializa como detalle"); }
        using (var t = Tabla()) { t.Rows.Clear(); t.Columns.Remove("percepcion_cant"); t.Columns.Add("percepcion_cant", typeof(long)); using var r = t.CreateDataReader(); await Reject<InvalidOperationException>(() => { CertificadosSufridosDatos.CrearMapeadorPorCuit(r); return Task.CompletedTask; }, "Agrupado: COUNT_BIG diferente del contrato rechazado"); }
        var consulta = new CertificadosSufridosConsulta { AgrupadoPorCuit = true, ImpuestoTexto = "Ingresos Brutos", FilasPorCuit = [
            new() { imp_id = "IB", tipo = "P", tipo_desc = "Percepción", cm_cuit = "00123456789", cm_nombre = "<b>literal</b>", percepcion = 10.21m, percepcion_cant = int.MaxValue },
            new() { imp_id = "IB", tipo = "R", tipo_desc = "Retención", cm_cuit = "00123456789", percepcion = -0.01m, percepcion_cant = int.MaxValue },
            new() { imp_id = "IB", tipo = "P", tipo_desc = "Percepción", cm_cuit = "00987654321", percepcion = 0.10m, percepcion_cant = 2 }
        ] };
        var grupos = consulta.GruposPorCuit();
        Check(grupos.Count == 2 && grupos[0].Filas.Count == 2 && grupos[1].Filas.Count == 1, "Agrupado: agrupa por impuesto/tipo/descripción, conserva cada fila del SP");
        Check(CertificadosSufridosConsulta.SumarPorCuit(consulta.FilasPorCuit) == 10.30m, "Agrupado: el importe con LH se suma una sola vez y conserva negativos");
        Check(CertificadosSufridosConsulta.CantidadPorCuit(consulta.FilasPorCuit) == 4294967296L, "Agrupado: registros de origen se suman como long sin desborde Int32");
        var json = JObject.FromObject(CertificadosSufridosPresentacion.Crear(consulta, filtro));
        Check(json["columnas"]!.Count() == 8 && json["filas"]!.Count() == 3 && json["totalBase"]!.Type == JTokenType.Null && json["totalLH"]!.Type == JTokenType.Null,
            "Agrupado: no inventa base ni desglose de LH");
        Check(json["totalImporte"]!.ToString() == "10,30" && json["registrosOrigen"]!.ToString() == "4.294.967.296" && !(bool)json["incompletos"]!, "Agrupado: totales y cantidades exactos formateados en servidor");
        Check(json["filas"]![0]!["celdas"]![1]!.ToString() == "<b>literal</b>" && json["filas"]![0]!["celdas"]![0]!.ToString() == "00123456789", "Agrupado: texto y CUIT literales");
        Check(!json["columnas"]!.Any(c => new[] { "Base", "Fecha", "Corte", "Alícuota" }.Any(s => c["titulo"]!.ToString().Contains(s))), "Agrupado: sólo columnas informadas por el SP");
        var restored = JsonConvert.DeserializeObject<CertificadosSufridosConsulta>(JsonConvert.SerializeObject(consulta))!;
        Check(restored.AgrupadoPorCuit && restored.Filas.Count == 0 && restored.FilasPorCuit[0].percepcion == 10.21m, "Agrupado: JSON mantiene el modo y contrato independiente");
        await Reject<InvalidOperationException>(() => { CertificadosSufridosPresentacion.Crear(consulta, new() { Impuesto = "IB", Periodo = filtro.Periodo, Mes = 2 }); return Task.CompletedTask; }, "Agrupado: respuesta de modo incorrecto rechazada");
        consulta.Filas.Add(new()); await Reject<InvalidOperationException>(() => { consulta.ValidarModo(filtro); return Task.CompletedTask; }, "Agrupado: mezcla de contratos rechazada"); consulta.Filas.Clear();
        consulta.FilasPorCuit.Add(new() { imp_id = "IB", tipo = "B", tipo_desc = "SIRCREB", percepcion = null, percepcion_cant = null });
        json = JObject.FromObject(CertificadosSufridosPresentacion.Crear(consulta, filtro));
        Check((bool)json["incompletos"]! && json["totalImporte"]!.ToString() == "No disponible" && json["registrosOrigen"]!.ToString() == "No disponible", "Agrupado: NULL no genera un total parcial ni conteo ficticio");
        var vacia = new CertificadosSufridosConsulta { AgrupadoPorCuit = true, ImpuestoTexto = "IB" };
        json = JObject.FromObject(CertificadosSufridosPresentacion.Crear(vacia, filtro));
        Check((int)json["total"]! == 0 && json["totalImporte"]!.ToString() == "0,00" && json["registrosOrigen"]!.ToString() == "0", "Agrupado: vacío sin filas ni grupos ficticios");
        await Reject<ArgumentException>(() => { CertificadosSufridosReporte.Pdf(vacia, filtro, empresa); return Task.CompletedTask; }, "Agrupado: no imprime consulta vacía");
        var datos = new DatosFake { Agrupadas = consulta.FilasPorCuit };
        var usuarios = DispatchProxy.Create<IApiUsuarioServicio, UsuarioFake>(); var proxy = (UsuarioFake)(object)usuarios;
        var svc = new CertificadosSufridosServicio(datos, usuarios);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user", "prueba"), new Claim("AdmId", "0001#Sucursal"), new Claim("perfiles", "[{\"perfil_id\":\"P1\"}]")], "test"));
        proxy.Item = "imp_ap";
        await Reject<UnauthorizedAccessException>(() => svc.Consultar(filtro, user, "P1", "C", default), "Agrupado: exige imp_ret_per_sufridas antes de leer el SP");
        Check(datos.Calls == 0 && datos.AgrupadasCalls == 0, "Agrupado: sin acceso no usa ninguno de los SP");
        proxy.Item = "imp_ret_per_sufridas"; var resultado = await svc.Consultar(filtro, user, "P1", "C", default);
        Check(resultado.AgrupadoPorCuit && resultado.Filas.Count == 0 && resultado.FilasPorCuit.Count == 4 && datos.Calls == 0 && datos.AgrupadasCalls == 1, "Agrupado: servicio selecciona sólo el SP x_Cuit");
        datos.FallaAgrupada = true;
        await Reject<IOException>(() => svc.Consultar(filtro, user, "P1", "C", default), "Agrupado: lectura fallida no produce respuesta parcial");
        using (var cancel = new CancellationTokenSource()) { cancel.Cancel(); await Reject<OperationCanceledException>(() => svc.Consultar(filtro, user, "P1", "C", cancel.Token), "Agrupado: cancelación propagada"); }
        var muestra = new CertificadosSufridosConsulta { AgrupadoPorCuit = true, ImpuestoTexto = "Ingresos Brutos" };
        for (var i = 0; i < 180; i++) muestra.FilasPorCuit.Add(new() { imp_id = "IB", tipo = i < 100 ? "P" : "R", tipo_desc = i < 100 ? "Percepción" : "Retención",
            cm_cuit = $"00{i:000000000}", cm_nombre = "AGENTE SINTÉTICO AGRUPADO POR CUIT", cm_domicilio = "DOMICILIO SINTÉTICO DE PRUEBA", percepcion = i == 0 ? -0.01m : 30.21m, percepcion_cant = 3 });
        var bytes = CertificadosSufridosReporte.Pdf(muestra, filtro, empresa);
        File.WriteAllBytes(Path.Combine(salida, "sufridos-IB-por-CUIT-sintetico.pdf"), bytes);
        File.WriteAllText(Path.Combine(salida, "consulta-por-CUIT-sintetica.json"), JsonConvert.SerializeObject(CertificadosSufridosPresentacion.Crear(muestra, filtro)));
        Check(bytes.Take(5).SequenceEqual("%PDF-"u8.ToArray()) && filtro.NombrePdf.EndsWith("_por_CUIT.pdf"), "Agrupado: PDF completo y nombre que identifica el modo");
        return checks;
    }
}
