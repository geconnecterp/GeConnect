using gc.infraestructura.Dtos.DocManager;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.infraestructura.ViewModels;
using gc.sitio.core.Servicios.Contratos.DocManager;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion.DocManager
{
    public class DocManagerServicio : IDocManagerServicio
    {
        public List<MenuRoot> GeneraArbolArchivos(List<Reporte> reportes)
        {
            List<MenuRoot> arbol = new List<MenuRoot>();
            MenuRoot root = new MenuRoot
            {
                id = "00",
                text = "Archivos",
                icon = "bx bx-file",
                state = new Estado { disabled = true, opened = true, selected = false },
                children = new List<MenuRoot>()
            };
            ;
            foreach (var rep in reportes)
            {
                int contador = 0;
                foreach (var arch in rep.Titulos)
                {
                    root.children.Add(CargaArchivo(rep.Id, arch, contador));
                    contador++;
                }

            }
            arbol.Add(root);
            return arbol;
        }

        private MenuRoot CargaArchivo(int idm, string titulo, int orden)
        {
            //hay qeu tener en cuenta que si llegan a ser mas archivos en el mismo 
            //reporte, se deberia agregar un nuevo tipo de seleccion para el titulo del archivo.

            var id = $"{idm}{orden + 1}";

            var archivo = new MenuRoot
            {
                id = id,
                text = titulo,
                state = new Estado { disabled = true, opened = true, selected = false },
                data = new MenuRootData { archivoB64 = string.Empty }
            };

            return archivo;
        }

        /// <summary>
        /// Se devuelve la ruta del archivo generado
        /// </summary>
        /// <typeparam name="T">tipo de dato a utilizar</typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public void GenerarArchivoPDF<T>(PrintRequestDto<T> request, out MemoryStream ms, List<string> titulos, float[] anchos,bool datosCliente)
        {
            PdfWriter writer = null;
            Document pdf = null;
            ms = new MemoryStream();
            try
            {
                var rCab = request.Cabecera;
                var rPie = request.Pie;

                string nnFile = rCab.TituloDocumento;

                #region Instancia del PDF y Generación de Cabecera
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, nnFile, HojaSize.A4, true);

                // Agregar el evento de pie de página
                writer.PageEvent = new CustomPdfPageEventHelper(rPie.Observaciones);

                var logo = HelperPdf.CargaLogo(rCab.Logo, 20, pdf.PageSize.Height - 10, 20);
                #region Se definen las fuentes a utilizar
                var titulo = HelperPdf.DefineFontWithStyle("Arial", 12, Font.BOLD, 0, 0, 0);
                var subtitulo = HelperPdf.DefineFontWithStyle("Arial", 10, Font.NORMAL, 0, 0, 0);
                var normal = HelperPdf.DefineFontWithStyle("Arial", 8, Font.NORMAL, 0, 0, 0);
                var chico = HelperPdf.DefineFontWithStyle("Arial", 6, Font.NORMAL, 0, 0, 0);
                #endregion
                #endregion

                #region Definición de la cabecera
                HeaderFooter cabecera = HelperPdf.GeneraCabeceraListadoTipo01(rCab, titulo, subtitulo, normal, chico, logo);
                pdf.Header = cabecera;
                #endregion
                #region Definición de Pie de página

                #endregion

                pdf.Open();

                #region Carga del listado
                //la primera hoja tiene los datos del cliente. Luego el listado de datos
                //cargamos los datos del cliente
                if (datosCliente)
                {
                    var tablaEnc = HelperPdf.GeneraTabla(4, [20f, 40f, 20f, 20f], 100, 10, 20);
                    HelperPdf.CargarDatosCliente(pdf, request.Cuerpo, subtitulo, tablaEnc);
                }
                
                        //List<string> titulos = new List<string> { "Descripcion", "Cuota", "Est.", "Fecha Comp.", "Fecha Vto", "Importe" };
                        //float[] anchos = [50f, 10f, 10f, 10f, 10f, 10f];
                        HelperPdf.GeneraCabeceraLista(pdf, titulos, anchos, normal);

                        HelperPdf.GenerarListadoDatos(pdf, request.Cuerpo, anchos, normal);
                

                #endregion
                //var parrafo = HelperPdf.GeneraParrafo("Texto Prueba", normal, Element.ALIGN_JUSTIFIED, 10, 10);
                //pdf.Add(parrafo);
                pdf.Close();

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public DocumentManagerViewModel InicializaObjeto(string titulo, AppModulo _modulo)
        {
            return new DocumentManagerViewModel()
            {
                Id = _modulo.Id,
                Titulo = titulo,
                ShowExportOption = _modulo.Export,
                ShowPrintOption = _modulo.Print,
                ShowEmailOption = _modulo.Email,
                ShowWhatsAppOption = _modulo.Whatsapp,
            };
        }

        public List<MenuRoot> MarcarConsultaRealizada(List<MenuRoot> reportes, AppReportes consulta, int orden, string archB64)
        {
            //debo buscar el reporte a modificar
            var id = $"{(int)consulta}{orden}";

            var repo = reportes[0].children.First(x => x.id == id);
            repo.state.selected = true;
            repo.state.disabled = false;
            repo.data.archivoB64 = archB64;
            reportes[0].children.Remove(reportes[0].children.First(x => x.id == id));
            reportes[0].children.Add(repo);
            reportes[0].children = reportes[0].children.OrderBy(x => x.id).ToList();
            return reportes;
        }
    }
}
