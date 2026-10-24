using ClosedXML.Excel;
using gc.api.core.Servicios.Libros;
using gc.api.infra.Datos.Implementacion;
using gc.infraestructura.Dtos.Libros;
using System.Data;

public static class PercepcionDdjjChecks
{
    public static object?[] Fila(string imp) => CertificadosPercepcionDdjjArchivos.Columnas(imp).Select(c => c.Tipo == typeof(decimal) ? (object?)(c.Campo == "ali" ? 2.5m : c.Campo == "percepcion_lh" ? null : -1234567890.12m)
        : c.Tipo == typeof(DateTime) ? new DateTime(2025,11,1) : c.Campo == "razon_social" ? "=1+1" : c.Campo is "cuit" or "ib_nro" ? "00123456789" : "0001-00000001").ToArray();
    private static async IAsyncEnumerable<object?[]> Filas(string imp,int cantidad=1,bool falla=false)
    {
        await Task.Yield();
        for(var n=0;n<cantidad;n++)yield return Fila(imp);
        if(falla)throw new IOException("Lectura incompleta sintética");
    }
    public static async Task<int> Run(string[] args)
    {
        var checks=0;
        void Check(bool ok,string text){if(!ok)throw new Exception("FAIL: "+text);checks++;Console.WriteLine("OK: "+text);}
        async Task Reject<T>(Func<Task> fn,string text) where T:Exception {try{await fn();}catch(T){Check(true,text);return;}throw new Exception("FAIL: "+text);}
        var hoy=IvaVentasFiltro.HoyArgentina();
        foreach(var imp in new[]{"IB","IV"})
        {
            var p=new CertificadosPercepcionDdjjPedido{Impuesto=imp,Periodo=hoy.Year,Mes=1};
            var pars=CertificadosPercepcionDatos.ParametrosDdjj(p);
            Check(pars.Length==2 && pars[0].ParameterName=="@periodo" && pars[1].ParameterName=="@mes" && (string)pars[1].Value=="01","DDJJ sólo período/mes y mes con dos dígitos "+imp);
            Check(pars[0].SqlDbType==SqlDbType.VarChar && pars[0].Size==4 && pars[1].Size==2,"Contrato parámetros SQL DDJJ "+imp);
            Check(CertificadosPercepcionDatos.SpDdjj(imp)=="dbo.SPGECO_Imp_AP_DDJJ_"+imp,"SP AP específico, no AR "+imp);
            Check(p.Nombre==$"DDJJ_Percepciones_{imp}_{hoy.Year}-01.xlsx" && p.ContentType.Contains("spreadsheetml"),"Nombre y formato Excel "+imp);
            var cols=CertificadosPercepcionDdjjArchivos.Columnas(imp);
            Check(cols.Length==(imp=="IB"?13:11),"Cantidad de columnas DDJJ "+imp);
            using(var tabla=new DataTable())
            {
                foreach(var c in cols)tabla.Columns.Add(c.Campo,c.Tipo);
                using(var reader=tabla.CreateDataReader())CertificadosPercepcionDdjjArchivos.ValidarColumnas(reader,imp);
                Check(true,"Metadatos válidos incluso sin filas "+imp);
                tabla.Columns[0].ColumnName="alias_erroneo";
                await Reject<InvalidOperationException>(()=>{using var reader=tabla.CreateDataReader();CertificadosPercepcionDdjjArchivos.ValidarColumnas(reader,imp);return Task.CompletedTask;},"Alias cambiado rechazado "+imp);
            }
            using var destino=new MemoryStream();
            await CertificadosPercepcionDdjjArchivos.Escribir(destino,imp,Filas(imp,6001),default); destino.Position=0;
            using var libro=new XLWorkbook(destino);var hoja=libro.Worksheets.Single();
            Check(hoja.LastRowUsed()!.RowNumber()==6002 && hoja.LastColumnUsed()!.ColumnNumber()==cols.Length,"Exporta 6001 filas completas y conserva duplicados "+imp);
            Check(hoja.Cell(2,1).GetString()=="=1+1" && !hoja.Cell(2,1).HasFormula,"Texto del origen no ejecuta fórmulas "+imp);
            Check(hoja.Cell(2,2).DataType==XLDataType.Text && hoja.Cell(2,2).GetString()=="00123456789","CUIT textual con ceros "+imp);
            var b=Array.FindIndex(cols,c=>c.Campo=="base")+1;var fecha=Array.FindIndex(cols,c=>c.Campo=="fecha")+1;
            Check(hoja.Cell(2,b).DataType==XLDataType.Number && hoja.Cell(2,b).GetValue<decimal>()==-1234567890.12m,"Importes numéricos exactos y negativos "+imp);
            Check(hoja.Cell(2,fecha).GetDateTime()==new DateTime(2025,11,1),"Fecha tipada "+imp);
            Check(hoja.SheetView.SplitRow==1 && hoja.AutoFilter.IsEnabled && hoja.Cell(1,1).Style.Font.FontColor==XLColor.White,"Golden, autofiltro y encabezado inmóvil "+imp);
            if(imp=="IB")
            {
                Check(hoja.Cell(1,4).GetString()=="CUIT (2C)" && hoja.Cell(2,4).GetString()=="00123456789","ib_nro conserva CUIT repetido y encabezado claro");
                Check(hoja.Cell(2,11).IsEmpty(),"LH nulo queda vacío, no cero");
            }
            if(args.Length==2 && args[0]=="--export-samples")
            {
                Directory.CreateDirectory(args[1]);
                using var sample=new MemoryStream();await CertificadosPercepcionDdjjArchivos.Escribir(sample,imp,Filas(imp,3),default);
                await File.WriteAllBytesAsync(Path.Combine(args[1],"DDJJ_Percepciones_"+imp+"_sintetica.xlsx"),sample.ToArray());
            }
            using var vacio=new MemoryStream();
            await Reject<CertificadosPercepcionDdjjException>(()=>CertificadosPercepcionDdjjArchivos.Escribir(vacio,imp,Filas(imp,0),default),"Sin datos no entrega archivo vacío "+imp);
            Check(vacio.Length==0,"No escribió un Excel incompleto vacío "+imp);
            await Reject<IOException>(()=>CertificadosPercepcionDdjjArchivos.Escribir(vacio,imp,Filas(imp,1,true),default),"Lectura interrumpida rechazada "+imp);
            Check(vacio.Length==0,"No entrega filas parciales "+imp);
            using var cancel=new CancellationTokenSource();cancel.Cancel();
            await Reject<OperationCanceledException>(()=>CertificadosPercepcionDdjjArchivos.Escribir(vacio,imp,Filas(imp),cancel.Token),"Cancelación DDJJ "+imp);
        }
        await Reject<ArgumentException>(()=>{CertificadosPercepcionDatos.SpDdjj("GA");return Task.CompletedTask;},"GA no permitido en AP");
        return checks;
    }
}
