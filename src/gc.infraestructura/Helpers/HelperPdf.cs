//// Proyecto base .NET 8 MVC - Reporte Gerencial con iTextSharp y gráficos

//using gc.infraestructura.Dtos.DocManager;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using Microsoft.AspNetCore.Mvc;
//using SixLabors.ImageSharp.Advanced;
//using System.ComponentModel;
//using System.Globalization;
//using System.IO;
//using DrawingColor = System.Drawing.Color;
//using DrawingBitmap = System.Drawing.Bitmap;
//using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;

//namespace gc.infraestructura.Helpers
//{
//    public static class EstilosPdf
//    {
//        public static iTextSharp.text.Font TituloPrincipal => FontFactory.GetFont("Arial", 16, iTextSharp.text.Font.BOLD, BaseColor.Black);
//        public static iTextSharp.text.Font Subtitulo => FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD, BaseColor.DarkGray);
//        public static iTextSharp.text.Font TextoNormal => FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.NORMAL, BaseColor.Black);
//        public static iTextSharp.text.Font TextoChico => FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.Gray);
//    }

//    public static class HelperPdf
//    {
//        public static Document GenerarInstanciaAndInit(ref PdfWriter writer, string fileName, HojaSize pagina = HojaSize.A4, bool esVertical = true)
//        {
//            Document doc = new Document(ObtenerHoja(pagina, esVertical), 50, 50, 50, 20);
//            writer = PdfWriter.GetInstance(doc, File.Create(fileName));
//            return doc;
//        }

//        public static Document GenerarInstanciaAndInit(ref PdfWriter writer, out MemoryStream mStream, string nombreArchivo, HojaSize pagina = HojaSize.A4, bool esVertical = true)
//        {
//            Document doc = new Document(ObtenerHoja(pagina, esVertical), 20, 20, 15, 50);
//            mStream = new MemoryStream();
//            writer = PdfWriter.GetInstance(doc, mStream);
//            return doc;
//        }

//        public static void VerificaDirTemp()
//        {
//            if (!Directory.Exists(@"c:\temp"))
//            {
//                Directory.CreateDirectory(@"c:\temp");
//            }
//        }

//        private static iTextSharp.text.Rectangle ObtenerHoja(HojaSize pagina, bool esVertical)
//        {
//            return pagina switch
//            {
//                HojaSize.A3 => esVertical ? PageSize.A3 : PageSize.A3.Rotate(),
//                HojaSize.A5 => esVertical ? PageSize.A5 : PageSize.A5.Rotate(),
//                HojaSize.A6 => esVertical ? PageSize.A6 : PageSize.A6.Rotate(),
//                _ => esVertical ? PageSize.A4 : PageSize.A4.Rotate()
//            };
//        }

//        public static void InsertarGrafico(Document doc, DrawingBitmap grafico, float ancho = 300, float alto = 300, int alineacion = Element.ALIGN_CENTER)
//        {
//            using var ms = new MemoryStream();
//            grafico.Save(ms, DrawingImageFormat.Png);
//            var img = iTextSharp.text.Image.GetInstance(ms.ToArray());
//            img.ScaleToFit(ancho, alto);
//            img.Alignment = alineacion;
//            doc.Add(img);
//        }

//        public static void VerificarEspacioYAgregarSalto(Document doc, float alturaNecesaria)
//        {
//            float posicionVertical = doc.Top - doc.TopMargin;
//            float espacioDisponible = posicionVertical - doc.BottomMargin;
//            if (espacioDisponible < alturaNecesaria)
//            {
//                doc.NewPage();
//            }
//        }
//    }

//    public class PieDePagina : PdfPageEventHelper
//    {
//        private readonly string _texto;

//        public PieDePagina(string texto)
//        {
//            _texto = texto;
//        }

//        public override void OnEndPage(PdfWriter writer, Document document)
//        {
//            PdfPTable tabla = new PdfPTable(2)
//            {
//                TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin
//            };
//            tabla.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

//            PdfPCell celdaTexto = new PdfPCell(new Phrase(_texto, EstilosPdf.TextoChico))
//            {
//                Border = iTextSharp.text.Rectangle.NO_BORDER,
//                HorizontalAlignment = Element.ALIGN_LEFT
//            };
//            tabla.AddCell(celdaTexto);

//            PdfPCell celdaNum = new PdfPCell(new Phrase($"Página {writer.PageNumber}", EstilosPdf.TextoChico))
//            {
//                Border = iTextSharp.text.Rectangle.NO_BORDER,
//                HorizontalAlignment = Element.ALIGN_RIGHT
//            };
//            tabla.AddCell(celdaNum);

//            tabla.WriteSelectedRows(0, -1, document.LeftMargin, document.BottomMargin - 10, writer.DirectContent);
//        }
//    }

//    public enum HojaSize
//    {
//        A1, A2, A3, A4, A5, A6
//    }
//}




using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte;
using gc.infraestructura.Dtos.Consultas.ReporteFinanciero;
using gc.infraestructura.Dtos.DocManager;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.EntidadesComunes.Options;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Microsoft.Win32;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static gc.infraestructura.Helpers.GridHelper;



namespace gc.infraestructura.Helpers
{
	public static class HelperPdf
	{
		/// <summary>
		/// Genera un documento A4
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="nombreArchivo"></param>
		/// <returns></returns>         
		public static Document GenerarInstanciaAndInit(ref PdfWriter writer, out MemoryStream mStream, string nombreArchivo, HojaSize pagina = HojaSize.A4, bool esVertical = true, float mgLeft = 20f, float mgRight = 20f, float mgTop = 15f, float mgBot = 50f)
		{
			Document doc = new Document(ObtenerHoja(pagina, esVertical), mgLeft, mgRight, mgTop, mgBot);
			mStream = new MemoryStream();
			writer = PdfWriter.GetInstance(doc, mStream);
			return doc;
		}

		public static Document GenerarInstanciaAndInit(ref PdfWriter writer, out MemoryStream mStream, string nombreArchivo, HojaSize pagina = HojaSize.A4, bool esVertical = true)
		{
			return GenerarInstanciaAndInit(ref writer, out mStream, nombreArchivo, pagina, esVertical, 20f, 20f, 15f, 50f);
			//Document doc = new Document(ObtenerHoja(pagina, esVertical), 20, 20, 15, 50);
			//mStream = new MemoryStream();
			//writer = PdfWriter.GetInstance(doc, mStream);
			//return doc;
		}

		public static Document GenerarInstanciaAndInit(ref PdfWriter writer, out MemoryStream mStream,
			HojaSize pagina = HojaSize.A4, bool esVertical = true,
			float mgLeft = 20f, float mgRight = 20f, float mgTop = 15f, float mgBot = 50f)
		{
			Document doc = new Document(ObtenerHoja(pagina, esVertical), mgLeft, mgRight, mgTop, mgBot);
			mStream = new MemoryStream();
			writer = PdfWriter.GetInstance(doc, mStream);
			return doc;
		}

		public static Document GenerarInstanciaAndInit(ref PdfWriter writer, out MemoryStream mStream,
			HojaSize pagina = HojaSize.A4, bool esVertical = true)
		{
			return GenerarInstanciaAndInit(ref writer, out mStream, pagina, esVertical, 20, 20, 15, 50);

		}

		public static Document GenerarInstanciaAndInit(ref PdfWriter writer, HojaSize pagina = HojaSize.A4, bool esVertical = true)
		{
			Document doc = new Document(ObtenerHoja(pagina, esVertical), 20, 20, 15, 50);
			var mStream = new MemoryStream();
			writer = PdfWriter.GetInstance(doc, mStream);
			return doc;
		}

		public static Document GenerarInstanciaAndInit(ref PdfWriter writer, string fileName, HojaSize pagina = HojaSize.A4, bool esVertical = true)
		{
			Document doc = new Document(ObtenerHoja(pagina, esVertical), 50, 50, 50, 20);
			writer = PdfWriter.GetInstance(doc, File.Create(fileName));
			return doc;
		}

		public static void VerificaDirTemp()
		{
			if (!Directory.Exists(@"c:\temp"))
			{
				Directory.CreateDirectory(@"c:\temp");
			}
		}

		private static Rectangle ObtenerHoja(HojaSize pagina, bool esVertical)
		{
			switch (pagina)
			{
				case HojaSize.A3:
					if (esVertical)
					{
						return PageSize.A3;
					}
					else
					{
						return PageSize.A3.Rotate();
					}
				case HojaSize.A5:
					if (esVertical)
					{
						return PageSize.A5;
					}
					else
					{
						return PageSize.A5.Rotate();
					}
				case HojaSize.A6:
					if (esVertical)
					{
						return PageSize.A6;
					}
					else
					{
						return PageSize.A6.Rotate();
					}
				default:
					if (esVertical)
					{
						return PageSize.A4;
					}
					else
					{
						return PageSize.A4.Rotate();
					}
			}

		}

		public static void GenerarRecibosEnA4(ref MemoryStream memory)
		{
			/// error itextsharp c# PdfReader PDF startxref not found
			/// antes tenia memory getBuffer
			PdfReader reader = new PdfReader(memory.ToArray());
			Document destino = new Document(PageSize.A4.Rotate(), 10, 10, 10, 0);


			MemoryStream msDestino = new MemoryStream();
			PdfWriter writer = PdfWriter.GetInstance(destino, msDestino);

			destino.Open();

			PdfImportedPage page;
			PdfPTable tabla = new PdfPTable(2);
			tabla.WidthPercentage = 100;
			for (int i = 1; i <= reader.NumberOfPages; i++)
			{
				page = writer.GetImportedPage(reader, i);
				tabla.AddCell(Image.GetInstance(page));
				destino.Add(tabla);
			}
			destino.Close();
			//return msDestino;
			memory = msDestino;
		}



		/// <summary>
		/// Los estilos pueden ser BOLD = 1;
		///                        BOLDITALIC = 3;
		///                        COURIER = 0;
		///                        DEFAULTSIZE = 12;
		///                        HELVETICA = 1;
		///                        ITALIC = 2;
		///                        NORMAL = 0;
		///                        STRIKETHRU = 8;
		///                        SYMBOL = 3;
		///                        TIMES_ROMAN = 2;
		///                        UNDEFINED = -1;
		///                        UNDERLINE = 4;
		///                        ZAPFDINGBATS = 4;
		/// </summary>
		/// <param name="nnFont">el nombre de la fuente "arial" "courier new", etc. </param>
		/// <param name="size">tamaño de la fuente</param>
		/// <param name="estilo">es un valor numerico que define el estilo de la fuente</param>
		/// <param name="r">valor numérico Red del RGB</param>
		/// <param name="g">valor numérico Green del RGB</param>
		/// <param name="b">valor numérico Blue del RGB</param>
		/// <returns></returns>
		public static Font DefineFontWithStyle(string nnFont, int size, int estilo, int r, int g, int b)
		{
			var font = FontFactory.GetFont(nnFont, size, estilo);
			font.Color = new BaseColor(r, g, b);
			return font;
		}

		public static Font DefineFontWithStyleIncrustada(string nnFont, int size, int estilo, int r, int g, int b)
		{
			var color = new BaseColor(r, g, b);
			var font = FontFactory.GetFont(
				nnFont,
				BaseFont.CP1252,
				BaseFont.EMBEDDED, // fuerza que se incruste la font
				size,
				estilo,
				color
			);
			return font;
		}

		// Sobrecarga: carga la fuente desde archivo en la carpeta Fonts
		public static Font DefineFontWithStyleFromFile(string fileName, int size, int estilo, int r, int g, int b)
		{
			// Ruta absoluta al archivo de la fuente
			var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "Fonts", fileName);

			// Alias para la fuente (sin extensión)
			var alias = Path.GetFileNameWithoutExtension(fontPath);

			// Registramos la fuente con un alias legible
			FontFactory.Register(fontPath, alias);

			// Reutilizamos el método original
			return DefineFontWithStyle(alias, size, estilo, r, g, b);
		}

		/// <summary>
		/// Define una imagen como logo para ser ubicada en una posición absoluta (x,y) 
		/// y con un tamaño definido por un porcentaje
		/// </summary>
		/// <param name="pathImagen">ruta para accedera a la imagen</param>
		/// <param name="x">posición X</param>
		/// <param name="y">posición Y</param>
		/// <param name="sizePorcent">porcentaje del tamaño de la imagen a insertar en el documento</param>
		/// <returns></returns>
		public static Image CargaLogo(string pathImagen, float x, float y, float sizePorcent)
		{
			Image logo = Image.GetInstance(pathImagen);
			logo.SetAbsolutePosition(x, y);
			logo.ScalePercent(sizePorcent);
			return logo;
		}

		/// <summary>
		/// Generación del Encabezado del Documento
		/// </summary>
		/// <param name="texto">Texto a presentar en el encabezado</param>
		/// <param name="fuente">La fuente a presentar en el encabezado</param>
		/// <param name="hasLogo">si va a presentar un logo debe tener el valor True.</param>
		/// <param name="logo">Ruta para acceder el logo o imagen en la cabecera</param>
		/// <returns></returns>
		public static HeaderFooter GeneraCabecera(string texto, Font fuente, bool hasLogo, Image logo)
		{

			Paragraph parrafo = new Paragraph(texto, fuente)
			{
				Alignment = Element.ALIGN_CENTER,
				SpacingBefore = 20,
				SpacingAfter = 20
			};
			if (hasLogo)
			{
				parrafo.Add(logo);
			}

			HeaderFooter header = new HeaderFooter(parrafo, false)
			{
				Alignment = Element.ALIGN_CENTER,
				BorderWidth = 0,
				BorderWidthBottom = 1
			};

			return header;
		}

		public static HeaderFooter GeneraCabecera(string texto, string texto2, Font fuente, bool hasLogo, Image logo)
		{
			Paragraph parrafo = new Paragraph(texto, fuente)
			{
				new Chunk("\n"),
				new Paragraph(texto2, fuente)
			};
			parrafo.Alignment = Element.ALIGN_CENTER;
			parrafo.SpacingBefore = 50;
			parrafo.SpacingAfter = 20;
			if (hasLogo)
			{
				parrafo.Add(logo);
			}

			HeaderFooter header = new HeaderFooter(parrafo, false)
			{
				Alignment = Element.ALIGN_CENTER,
				BorderWidth = 0,
				BorderWidthBottom = 1
			};

			return header;
		}
		public static HeaderFooter GeneraCabeceraListadoTipo01(DatosCabeceraDto rCab, Font titulo, Font subtitulo, Font normal, Font chico, Image? logo)
		{
			PdfPTable tabla = GeneraTabla(4, [10f, 20f, 50f, 20f], 100, 10, 20);

			// Columna 1: Logo
			PdfPCell celdaLogo = GeneraCelda(logo, false);
			tabla.AddCell(celdaLogo);

			// Columna 2: Datos apilados y título
			PdfPTable subTabla = new PdfPTable(1);
			subTabla.WidthPercentage = 100;

			// Datos apilados
			subTabla.AddCell(CrearCeldaTexto(rCab.NombreEmpresa, chico));
			subTabla.AddCell(CrearCeldaTexto($"CUIT: {rCab.CUIT} s:{rCab.Sucursal}", chico));
			subTabla.AddCell(CrearCeldaTexto($"IIBB: {rCab.IIBB}", chico));
			subTabla.AddCell(CrearCeldaTexto($"Dirección: {rCab.Direccion}", chico));

			PdfPCell celdaSubTabla = new PdfPCell(subTabla)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaSubTabla);
			//  Columna 3: Título del documento
			PdfPCell celdaTitulo = new PdfPCell(new Phrase(rCab.TituloDocumento, titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 10f
			};
			tabla.AddCell(celdaTitulo);

			// Columna 4: Fecha y hora
			string fechaHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
			PdfPCell celdaFechaHora = new PdfPCell(new Phrase(fechaHora, chico))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaFechaHora);

			// Convertir la tabla en un Phrase
			Phrase phrase = new Phrase();
			phrase.Add(tabla);

			// Crear el HeaderFooter con el Phrase que contiene la tabla
			HeaderFooter header = new HeaderFooter(phrase, false)
			{
				Alignment = Element.ALIGN_TOP,
				BorderWidth = 0,
				//BorderWidthBottom = 1,   

			};

			return header;
		}
		public static PdfPCell CrearCeldaTexto(string texto, Font fuente)
		{
			PdfPCell celda = new PdfPCell(new Phrase(texto, fuente))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			return celda;
		}
		public static HeaderFooter GeneraCabecera(string razonsocial, string cuit, string iibb, string direccion, string sucursal, Font fuente1, string titulo, Font fuenteTit, Font normal, Font chica)
		{
			return GeneraCabecera(razonsocial, cuit, iibb, direccion, sucursal, fuente1, titulo, fuenteTit, normal, chica, false);
		}
		public static HeaderFooter GeneraCabecera(string razonsocial, string cuit, string iibb, string direccion, string sucursal, Font fuente1,
			string titulo, Font fuenteTit, Font normal, Font chica, bool hasLogo, Image? logo = null)
		{
			PdfPTable tabla = HelperPdf.GeneraTabla(3, [40f, 50f, 10f], 100, 10, 20);
			tabla.DefaultCell.Border = 0;//Rectangle.NO_BORDER;

			var parrafo = HelperPdf.GeneraParrafo(razonsocial, fuente1, Element.ALIGN_CENTER, 10, 10);
			var celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.Black, Element.ALIGN_CENTER);
			celda.Border = Rectangle.NO_BORDER;
			tabla.AddCell(celda);

			parrafo = HelperPdf.GeneraParrafo(cuit, normal, Element.ALIGN_CENTER, 10, 10);
			celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.Black, Element.ALIGN_CENTER);
			celda.Border = Rectangle.NO_BORDER;
			tabla.AddCell(celda);

			parrafo = HelperPdf.GeneraParrafo(iibb, normal, Element.ALIGN_CENTER, 10, 10);
			celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.Black, Element.ALIGN_CENTER);
			celda.Border = Rectangle.NO_BORDER;
			tabla.AddCell(celda);

			//Paragraph parrafo = new Paragraph(titulo, fuenteTit)
			//{
			//    Alignment = Element.ALIGN_CENTER,
			//    SpacingBefore = 20,
			//    SpacingAfter = 20
			//};

			//if (hasLogo)
			//{
			//    parrafo.Add(logo);
			//}


			////TITULO 
			//parrafo = HelperPdf.GeneraParrafo(titulo, fuenteTit, Element.ALIGN_CENTER, 10, 10);
			//var celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.White, Element.ALIGN_CENTER);
			//celda.Border = 0; // Rectangle.NO_BORDER;
			//tabla.AddCell(celda);

			////cargo la fecha y hora
			//parrafo = HelperPdf.GeneraParrafo(DateTime.Today.ToString("dd/MM/yyyy"), normal, Element.ALIGN_CENTER, 10, 10);
			//tabla.AddCell(parrafo);

			var frase = new Phrase();
			frase.Add(tabla);

			//gen
			HeaderFooter header = new HeaderFooter(frase, false)
			{
				Alignment = Element.ALIGN_CENTER,
				BorderWidth = 0,
				BorderWidthBottom = 1
			};
			return header;
		}

		/// <summary>
		/// Crea un parrafo con un texto que se ingresa por parametro, espeficando la fuente
		/// del texto y el tipo de alineacion
		/// </summary>
		/// <param name="texto">Texto que tendrá el parrafo</param>
		/// <param name="fuente">Fuente del texto</param>
		/// <param name="alineacion">Alineación del Texto.
		/// ALIGN_BOTTOM = 6;
		/// ALIGN_CENTER = 1;
		/// ALIGN_JUSTIFIED = 3;
		/// ALIGN_JUSTIFIED_ALL = 8;
		/// ALIGN_LEFT = 0;
		/// ALIGN_MIDDLE = 5;
		/// ALIGN_RIGHT = 2;
		/// ALIGN_TOP = 4;
		/// 
		/// </param>
		/// <param name="espaciadoAnterior"></param>
		/// <param name="espaciadoPosterior"></param>
		/// <returns>devuelve un Parrafo</returns>
		public static Paragraph GeneraParrafo(string texto, Font fuente, int alineacion, float espaciadoAnterior, float espaciadoPosterior, bool especificaColor = false, BaseColor? color = null)
		{
			if (especificaColor)
			{
				fuente.Color = color;
			}
			var parrafo = new Paragraph(texto, fuente)
			{
				SpacingBefore = espaciadoAnterior,
				SpacingAfter = espaciadoPosterior,
				Alignment = alineacion
			};

			return parrafo;
		}

		public static Chunk GeneraAgregadoDeParrafo(string texto, Font fuente)
		{
			return new Chunk(texto, fuente);
		}

		/// <summary>
		/// Genera una tabla
		/// </summary>
		/// <param name="numeroColumnas"></param>
		/// <param name="anchosDeColumna"></param>
		/// <param name="anchoTabla"></param>
		/// <param name="espaciadoAnterior"></param>
		/// <param name="espaciadoPosterior"></param>
		/// <returns></returns>
		public static PdfPTable GeneraTabla(int numeroColumnas, float[] anchosDeColumna, float anchoTabla, float espaciadoAnterior, float espaciadoPosterior)
		{
			PdfPTable tabla = new PdfPTable(numeroColumnas);
			//se define el ancho de cada columna de la tabla. Por cada columna se define el ancho y el tamaño de la pagina.
			tabla.SetWidthPercentage(anchosDeColumna, PageSize.A4);
			//se define el ancho de la tabla en la hoja
			tabla.WidthPercentage = anchoTabla;
			tabla.SpacingBefore = espaciadoAnterior;
			tabla.SpacingAfter = espaciadoPosterior;

			return tabla;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parrafo"></param>
		/// <param name="hasBackground">Si se asigna true significa que se definira el color background con el color enviado.</param>
		/// <param name="bkg">Define un color para el fondo</param>
		/// <param name="alineacion"></param>
		/// <returns></returns>
		public static PdfPCell GeneraCelda(iTextSharp.text.Paragraph parrafo, bool hasBackground, BaseColor bkg, int alineacion)
		{
			var celda = new PdfPCell(parrafo);
			celda.HorizontalAlignment = alineacion;
			if (hasBackground)
			{
				celda.BackgroundColor = bkg;
			}

			return celda;
		}

		public static PdfPCell GeneraCelda(Image? logo, bool fit = true)
		{
			if (logo == null)
			{
				return new PdfPCell();
			}
			PdfPCell celdaLogo = new PdfPCell(logo, fit)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			return celdaLogo;
		}

		public static PdfPCell GeneraCelda(Phrase prase, bool hasBackground, BaseColor bkg, int alineacion)
		{
			var celda = new PdfPCell(prase);
			celda.HorizontalAlignment = alineacion;
			if (hasBackground)
			{
				celda.BackgroundColor = bkg;
			}

			return celda;
		}



		private static PdfPTable CargarCabeceraDeLista(List<string> columnas, float[] anchos, Font normal)
		{
			var tabla = GeneraTabla(columnas.Count, anchos, 100, 10, 10);

			foreach (var item in columnas)
			{
				var parrafo = GeneraParrafo(item, normal, Element.ALIGN_CENTER, 10, 10);
				var celda = GeneraCelda(parrafo, true, BaseColor.White, Element.ALIGN_CENTER);
				tabla.AddCell(celda);
			}
			return tabla;
		}

		public static void CargarDatosCliente<T>(Document pdf, DatosCuerpoDto<T> cuerpo, Font subtitulo, PdfPTable tablaEnc)
		{
			// FILA 1
			Paragraph parrafo = GeneraParrafo($"Cta. Comercial:", subtitulo, Element.ALIGN_RIGHT, 10, 10);
			var celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.Black, Element.ALIGN_CENTER);
			celda.Border = Rectangle.NO_BORDER;
			tablaEnc.AddCell(celda);

			parrafo = GeneraParrafo(cuerpo.CtaId, subtitulo, Element.ALIGN_LEFT, 10, 10);
			celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.Black, Element.ALIGN_LEFT);
			celda.Border = Rectangle.NO_BORDER;
			tablaEnc.AddCell(celda);

			parrafo = GeneraParrafo($"CUIT:", subtitulo, Element.ALIGN_RIGHT, 10, 10);
			celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.Black, Element.ALIGN_LEFT);
			celda.Border = Rectangle.NO_BORDER;
			tablaEnc.AddCell(celda);

			parrafo = GeneraParrafo(cuerpo.CUIT, subtitulo, Element.ALIGN_LEFT, 10, 10);
			celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.Black, Element.ALIGN_CENTER);
			celda.Border = Rectangle.NO_BORDER;
			tablaEnc.AddCell(celda);

			// FILA 2
			parrafo = GeneraParrafo($"Razón Social:", subtitulo, Element.ALIGN_RIGHT, 10, 10);
			celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.Black, Element.ALIGN_CENTER);
			celda.Border = Rectangle.NO_BORDER;
			tablaEnc.AddCell(celda);

			parrafo = GeneraParrafo(cuerpo.RazonSocial, subtitulo, Element.ALIGN_LEFT, 10, 10);
			celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.Black, Element.ALIGN_LEFT);
			celda.Border = Rectangle.NO_BORDER;
			tablaEnc.AddCell(celda);

			parrafo = GeneraParrafo($"Contacto:", subtitulo, Element.ALIGN_RIGHT, 10, 10);
			celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.Black, Element.ALIGN_CENTER);
			celda.Border = Rectangle.NO_BORDER;
			tablaEnc.AddCell(celda);

			parrafo = GeneraParrafo(cuerpo.Contacto, subtitulo, Element.ALIGN_LEFT, 10, 10);
			celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.Black, Element.ALIGN_LEFT);
			celda.Border = Rectangle.NO_BORDER;
			tablaEnc.AddCell(celda);

			// FILA 3
			parrafo = GeneraParrafo($"Domicilio:", subtitulo, Element.ALIGN_RIGHT, 10, 10);
			celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.Black, Element.ALIGN_CENTER);
			celda.Border = Rectangle.NO_BORDER;
			tablaEnc.AddCell(celda);

			parrafo = GeneraParrafo(cuerpo.Domicilio, subtitulo, Element.ALIGN_LEFT, 10, 10);
			celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.Black, Element.ALIGN_CENTER);
			celda.Border = Rectangle.NO_BORDER;
			tablaEnc.AddCell(celda);

			pdf.Add(tablaEnc);
		}

		public static void GeneraCabeceraListado(Document pdf, List<string> titulos,
			List<float> columnasAncho, Font titulo, Font subtitulo, Font normal, Font chico)
		{
			PdfPTable tabla = GeneraTabla(titulos.Count, columnasAncho.ToArray(), 100, 10, 10);
			Paragraph parrafo;
			PdfPCell celda;
			foreach (var txt in titulos)
			{
				parrafo = GeneraParrafo(txt, normal, Element.ALIGN_CENTER, 10, 10);
				celda = GeneraCelda(parrafo, false, BaseColor.White, Element.ALIGN_CENTER);
				tabla.AddCell(celda);
			}

		}

		public static void GeneraCabeceraLista(Document pdf, List<string> titulos, float[] anchos, Font normal, float espaciadoAnterior = 0, float espaciadoPosterior = 0)
		{
			PdfPTable tabla = GeneraTabla(titulos.Count, anchos, 100, espaciadoAnterior, espaciadoPosterior);
			Paragraph parrafo;
			PdfPCell celda;
			foreach (var txt in titulos)
			{
				parrafo = GeneraParrafo(txt, normal, Element.ALIGN_CENTER, 10, 10, true, BaseColor.White);

				celda = GeneraCelda(parrafo, true, BaseColor.Black, Element.ALIGN_CENTER);
				tabla.AddCell(celda);
			}
			pdf.Add(tabla);
		}

		/// <summary>
		/// Genera un listado PDF con agrupamiento jerárquico múltiple
		/// </summary>
		/// <typeparam name="T">Tipo de entidad a listar</typeparam>
		/// <param name="pdf">Documento PDF destino</param>
		/// <param name="lista">Lista de datos a procesar</param>
		/// <param name="campos">Nombres de campos a mostrar en las columnas</param>
		/// <param name="titulos">Títulos de las columnas</param>
		/// <param name="anchos">Anchos relativos de cada columna</param>
		/// <param name="nivelesAgrupamiento">Lista ordenada de niveles de agrupamiento (del más general al más específico)</param>
		/// <param name="fuente">Fuente para datos normales</param>
		/// <param name="fuenteNegrita">Fuente para headers y totales</param>
		/// <param name="totalesPorCampo">Totales generales (opcional)</param>
		/// <param name="autoCalcularTotales">Si debe calcular totales automáticamente</param>
		/// <param name="camposTotalizables">Campos que deben sumarse (opcional)</param>
		/// <param name="mostrarCabecera">Si debe mostrar el encabezado de columnas</param>
		public static void GenerarListadoAgrupado<T>(
			Document pdf,
			List<T> lista,
			List<string> campos,
			List<string> titulos,
			float[] anchos,
			List<NivelAgrupamiento> nivelesAgrupamiento,
			Font fuente,
			Font fuenteNegrita,
			Dictionary<string, decimal>? totalesPorCampo = null,
			bool autoCalcularTotales = true,
			List<string>? camposTotalizables = null,
			bool mostrarCabecera = true)
		{
			if (lista == null || !lista.Any() || campos == null || campos.Count == 0)
				return;

			if (nivelesAgrupamiento == null || !nivelesAgrupamiento.Any())
				throw new ArgumentException("Debe especificar al menos un nivel de agrupamiento", nameof(nivelesAgrupamiento));

			var cultura = new CultureInfo("es-ES");
			var propsDict = TypeDescriptor.GetProperties(typeof(T))
										  .Cast<PropertyDescriptor>()
										  .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

			// Validar que todos los campos de agrupamiento existan
			foreach (var nivel in nivelesAgrupamiento)
			{
				if (!propsDict.ContainsKey(nivel.CampoGrupo))
					throw new ArgumentException($"El campo de agrupamiento '{nivel.CampoGrupo}' no existe en el tipo {typeof(T).Name}");

				if (!propsDict.ContainsKey(nivel.CampoDescripcion))
					throw new ArgumentException($"El campo de descripción '{nivel.CampoDescripcion}' no existe en el tipo {typeof(T).Name}");
			}

			var tabla = GeneraTabla(campos.Count, anchos, 100, 0, 10);

			// Mostrar encabezado de columnas si corresponde
			if (mostrarCabecera)
			{
				AgregarEncabezadoColumnas(tabla, titulos, campos.Count, fuenteNegrita);
			}

			// Procesar agrupamiento jerárquico
			ProcesarAgrupamientoJerarquico(
				tabla,
				lista,
				campos,
				nivelesAgrupamiento,
				propsDict,
				fuente,
				fuenteNegrita,
				cultura,
				autoCalcularTotales,
				camposTotalizables
			);

			// Agregar totales generales si corresponde
			if (totalesPorCampo != null && totalesPorCampo.Count > 0)
			{
				AgregarFilaTotalesGenerales(tabla, campos, totalesPorCampo, fuenteNegrita, cultura);
			}
			else if (autoCalcularTotales)
			{
				var totalesCalculados = CalcularTotales(lista, campos, camposTotalizables, propsDict);
				if (totalesCalculados.Count > 0)
				{
					AgregarFilaTotalesGenerales(tabla, campos, totalesCalculados, fuenteNegrita, cultura);
				}
			}

			pdf.Add(tabla);
		}

		public static void GenerarListadoDatos<T>(Document pdf, DatosCuerpoDto<T> cuerpo, float[] anchos, Font normal)
		{
			int alig;
			CultureInfo cultura = new CultureInfo("es-ES");
			Type entidad = typeof(T);
			PropertyDescriptorCollection propiedades = TypeDescriptor.GetProperties(entidad);

			PdfPTable tabla = GeneraTabla(propiedades.Count, anchos, 100, 0, 10);

			Paragraph parrafo;
			PdfPCell celda;
			foreach (T elemento in cuerpo.Datos)
			{
				foreach (PropertyDescriptor prop in propiedades)
				{
					var valor = prop.GetValue(elemento);
					if (valor == null)
					{
						valor = string.Empty;
					}
					if (decimal.TryParse(valor.ToString(), NumberStyles.Number, cultura, out decimal resultado))
					{
						alig = Element.ALIGN_RIGHT;
					}
					//trato de identificar si es una fecha
					else if (valor.ToString()?.ToDateTimeOrNull() != null)
					{
						alig = Element.ALIGN_CENTER;
					}
					//si es un string y tiene un solo caracter lo considero char
					else if (valor.ToString().Length == 1)
					{
						alig = Element.ALIGN_CENTER;
					}
					else
					{
						alig = Element.ALIGN_LEFT;
					}

					parrafo = GeneraParrafo(valor.ToString() ?? "", normal, alig, 10, 10, true, BaseColor.Black);

					celda = GeneraCelda(parrafo, true, BaseColor.White, alig);
					tabla.AddCell(celda);
				}
			}
			pdf.Add(tabla);
		}

		public static bool ObtenerBooleanoSeguro(object valor)
		{
			return valor is bool resultado && resultado;
		}

		public static void GenerarListadoDesdeLista<T>(
			Document pdf,
			List<T> lista,
			List<string> campos,
			float[] anchos,
			Font fuente,
			bool incluirHoraEnFechas = false,
			bool agregarFilaTotal = false,
			Dictionary<string, decimal>? totalesPorCampo = null,
			bool formatearBooleanos = false,
			BooleanDisplayFormat formatoBooleano = BooleanDisplayFormat.SiNo,
			bool valorExitoEsTrue = true,
			bool anioEnCuatroDigitos = true,
			bool estableceColorCamposBooleanos = false)
		{
			if (lista == null || lista.Count == 0 || campos == null || campos.Count == 0)
				return;

			var cultura = new CultureInfo("es-ES");
			var propsDict = TypeDescriptor.GetProperties(typeof(T))
										  .Cast<PropertyDescriptor>()
										  .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

			PdfPTable tabla = GeneraTabla(campos.Count, anchos, 100, 0, 10);

			foreach (var item in lista)
			{
				foreach (var campo in campos)
				{
					if (!propsDict.TryGetValue(campo, out var prop))
					{
						tabla.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.BOTTOM_BORDER });
						continue;
					}

					var valorObj = prop.GetValue(item);
					string valorTexto = string.Empty;
					int alineacion;
					BaseColor? colorTexto = null;

					// Detectar y formatear valores booleanos
					if (formatearBooleanos && valorObj is bool valorBooleano)
					{
						bool representaExito = (valorBooleano && valorExitoEsTrue) || (!valorBooleano && !valorExitoEsTrue);

						switch (formatoBooleano)
						{
							case BooleanDisplayFormat.SiNo:
								valorTexto = ObtenerBooleanoSeguro(valorObj) ? "SI" : "NO";
								break;
							case BooleanDisplayFormat.XOk:
								valorTexto = representaExito ? "OK" : "X";
								break;
							case BooleanDisplayFormat.CheckX:
								// Usamos símbolos Unicode para check (✓) y X (✗)
								if (representaExito)
									valorTexto = ObtenerBooleanoSeguro(valorObj) ? "✓" : "✗";
								else
									valorTexto = ObtenerBooleanoSeguro(valorObj) ? "✓" : "";
								break;
							case BooleanDisplayFormat.TrueFalse:
								valorTexto = valorBooleano ? "True" : "False";
								break;
						}

						// Asignar colores según el valor (verde para éxito, rojo para error)
						if (estableceColorCamposBooleanos)
							colorTexto = representaExito ? new BaseColor(0, 128, 0) : BaseColor.Red; // Verde o Rojo
						else
							colorTexto = null;
						alineacion = Element.ALIGN_CENTER;
					}
					else if (valorObj is DateTime dt)
					{
						if (anioEnCuatroDigitos)
							valorTexto = incluirHoraEnFechas ? dt.ToString("dd/MM/yyyy HH:mm") : dt.ToString("dd/MM/yyyy");
						else
							valorTexto = incluirHoraEnFechas ? dt.ToString("dd/MM/yy HH:mm") : dt.ToString("dd/MM/yy");
						alineacion = Element.ALIGN_CENTER;
					}
					else if (valorObj is decimal or double or float)
					{
						valorTexto = Convert.ToDecimal(valorObj).ToString("N2", cultura);
						alineacion = Element.ALIGN_RIGHT;
					}
					else
					{
						valorTexto = valorObj?.ToString() ?? "";
						alineacion = valorTexto.Length == 1 ? Element.ALIGN_CENTER : Element.ALIGN_LEFT;
					}

					var parrafo = GeneraParrafo(valorTexto, fuente, alineacion, 5, 5, colorTexto != null, colorTexto ?? BaseColor.Black);
					var celda = new PdfPCell(parrafo)
					{
						Border = Rectangle.BOTTOM_BORDER,
						BorderColorBottom = BaseColor.Black,
						HorizontalAlignment = alineacion
					};
					tabla.AddCell(celda);
				}
			}

			// Agregar fila total si corresponde (código existente sin cambios)
			if (agregarFilaTotal && totalesPorCampo != null && totalesPorCampo.Count > 0)
			{
				var fuenteNegrita = new Font(fuente);
				fuenteNegrita.SetStyle(Font.BOLD);
				int idxPrimerTotal = campos.FindIndex(c => totalesPorCampo.ContainsKey(c));

				for (int i = 0; i < campos.Count; i++)
				{
					string campo = campos[i];
					PdfPCell celda;

					if (totalesPorCampo.TryGetValue(campo, out var total))
					{
						string valorFormateado = total.ToString("N2", cultura);
						var parrafo = GeneraParrafo(valorFormateado, fuenteNegrita, Element.ALIGN_RIGHT, 5, 5, true, BaseColor.Black);
						celda = new PdfPCell(parrafo)
						{
							HorizontalAlignment = Element.ALIGN_RIGHT,
							BackgroundColor = BaseColor.LightGray,
							Border = Rectangle.BOTTOM_BORDER,
							BorderColorBottom = BaseColor.Black
						};
					}
					else if (i == idxPrimerTotal - 1) // Agregar "Total" antes de último total si no hay campo a totalizar antes
					{
						var parrafo = GeneraParrafo("Total:", fuenteNegrita, Element.ALIGN_RIGHT, 5, 5, true, BaseColor.Black);
						celda = new PdfPCell(parrafo)
						{
							HorizontalAlignment = Element.ALIGN_RIGHT,
							BackgroundColor = BaseColor.LightGray,
							Border = Rectangle.BOTTOM_BORDER,
							BorderColorBottom = BaseColor.Black
						};
					}
					else
					{
						celda = new PdfPCell(new Phrase(""))
						{
							BackgroundColor = BaseColor.LightGray,
							Border = Rectangle.BOTTOM_BORDER,
							BorderColorBottom = BaseColor.Black
						};
					}

					tabla.AddCell(celda);
				}
			}

			pdf.Add(tabla);
		}

		// Enumeración para definir los formatos de visualización de booleanos
		public enum BooleanDisplayFormat
		{
			SiNo,     // Muestra "SI" o "NO"
			XOk,      // Muestra "OK" o "X"
			CheckX,   // Muestra símbolos de check (✓) y X (✗)
			TrueFalse // Muestra "True" o "False" (por defecto)
		}



		public static HeaderFooter GenerarPie(DatosPieDto rPie, Font chico)
		{
			throw new NotImplementedException();
		}

		public static Font FontTituloBigBoldPredeterminado()
		{
			return DefineFontWithStyle("Arial", 14, Font.BOLD, 0, 0, 0);
		}

		public static Font FontTituloBigPredeterminado()
		{
			return DefineFontWithStyle("Arial", 14, Font.NORMAL, 0, 0, 0);
		}

		public static Font FontTituloPredeterminado()
		{
			return DefineFontWithStyle("Arial", 12, Font.BOLD, 0, 0, 0);
		}

		public static Font FontSubtituloPredeterminado(bool bold = false)
		{
			return DefineFontWithStyle("Arial", 10, bold ? Font.BOLD : Font.NORMAL, 0, 0, 0);
		}

		public static Font FontNormalPredeterminado(bool bold = false)
		{
			return DefineFontWithStyle("Arial", 8, bold ? Font.BOLD : Font.NORMAL, 0, 0, 0);
		}

		public static Font FontChicoPredeterminado(bool bold = false)
		{
			return DefineFontWithStyle("Arial", 6, bold ? Font.BOLD : Font.NORMAL, 0, 0, 0);
		}

		public static Font FontSuperChicoPredeterminado(bool bold = false)
		{
			return DefineFontWithStyle("Arial", 5, bold ? Font.BOLD : Font.NORMAL, 0, 0, 0);
		}

		public static Font FontMedioPredeterminado(bool bold = false)
		{
			return DefineFontWithStyle("Arial", 7, bold ? Font.BOLD : Font.NORMAL, 0, 0, 0);
		}

		public static PdfPCell CeldaSinBorde(string texto, Font fuente, int alineacion)
		{
			var celda = HelperPdf.GeneraCelda(HelperPdf.GeneraParrafo(texto, fuente, alineacion, 5, 5), false, BaseColor.White, alineacion);
			celda.Border = Rectangle.NO_BORDER;
			return celda;
		}

		public static void CargaDatosPresupuesto(Document pdf, PresupuestoDto presup, Font fuenteEtiqueta, Font fuenteValor)
		{
			PdfPTable tabla = GeneraTabla(2, [20f, 80f], 100, 10, 0);
			//FILA 1
			tabla.AddCell(CeldaSinBorde("Presupuesto N°: ", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(presup.pre_id, fuenteValor, Element.ALIGN_LEFT));
			//fila 2 
			tabla.AddCell(CeldaSinBorde("Cliente: ", fuenteEtiqueta, Element.ALIGN_RIGHT));
			var cli = !string.IsNullOrEmpty(presup.cta_id) ?
				$"{presup.cta_id}-{presup.cta_denominacion}" :
				$"{presup.pre_nombre}";
			tabla.AddCell(CeldaSinBorde(cli, fuenteValor, Element.ALIGN_LEFT));
			pdf.Add(tabla);

			//generamos una nueva tabla sin espacio con la tabla anterior
			tabla = GeneraTabla(4, [20f, 30f, 20f, 30f], 100, 0, 0);

			//fila3
			tabla.AddCell(CeldaSinBorde("Domicilio: ", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(presup.pre_domicilio, fuenteValor, Element.ALIGN_LEFT));
			tabla.AddCell(CeldaSinBorde("Registrado: ", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(presup.usu_apellidoynombre, fuenteValor, Element.ALIGN_LEFT));

			//fila4
			tabla.AddCell(CeldaSinBorde("Vigencia Desde: ", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(presup.pre_vigencia_desde.ToShortDateString(), fuenteValor, Element.ALIGN_LEFT));
			tabla.AddCell(CeldaSinBorde("Vigencia Hasta: ", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(presup.pre_vigencia_hasta.ToShortDateString(), fuenteValor, Element.ALIGN_LEFT));

			pdf.Add(tabla);

			tabla = GeneraTabla(6, [20f, 10f, 15f, 20f, 15f, 20f], 100, 0, 10);
			//fila5
			tabla.AddCell(CeldaSinBorde("Sucursal: ", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(presup.adm_nombre, fuenteValor, Element.ALIGN_LEFT));
			tabla.AddCell(CeldaSinBorde("Forma de Pago: ", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(presup.pre_obs_pago, fuenteValor, Element.ALIGN_LEFT));
			tabla.AddCell(CeldaSinBorde("Entrega: ", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(presup.pre_obs_entrega, fuenteValor, Element.ALIGN_LEFT));

			pdf.Add(tabla);
		}



		public static void CargarTablaClienteProveedor(Document pdf, CuentaDto cuenta, Font fuenteEtiqueta, Font fuenteValor)
		{
			PdfPTable tabla = GeneraTabla(4, [20f, 30f, 20f, 30f], 100, 10, 10);


			// FILA 1
			tabla.AddCell(CeldaSinBorde("Cta. Comercial:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(cuenta.Cta_Id, fuenteValor, Element.ALIGN_LEFT));
			tabla.AddCell(CeldaSinBorde("CUIT:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(cuenta.Cta_Documento, fuenteValor, Element.ALIGN_LEFT));

			// FILA 2
			tabla.AddCell(CeldaSinBorde("Razón Social:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(cuenta.Cta_Denominacion, fuenteValor, Element.ALIGN_LEFT));
			tabla.AddCell(CeldaSinBorde("Contacto:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(cuenta.Cta_Te, fuenteValor, Element.ALIGN_LEFT));

			// FILA 3
			string domicilioCompleto = $"{cuenta.Cta_Domicilio} {cuenta.Cta_Localidad} CP: {cuenta.Cta_Cpostal}";
			tabla.AddCell(CeldaSinBorde("Domicilio:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(domicilioCompleto, fuenteValor, Element.ALIGN_LEFT));

			// SALDO con signo y color si es negativo
			string saldoFormateado = cuenta.Monto.ToString("+#,##0.00;-#,##0.00", new CultureInfo("es-AR"));
			BaseColor colorSaldo = cuenta.Monto < 0 ? BaseColor.Red : BaseColor.Black;

			var fuenteSaldo = new Font(fuenteValor);
			fuenteSaldo.Color = colorSaldo;

			tabla.AddCell(CeldaSinBorde(cuenta.MontoEtiqueta, fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(saldoFormateado, fuenteSaldo, Element.ALIGN_RIGHT));

			pdf.Add(tabla);
		}

		public static void CargarTablaProveedor(Document pdf, CuentaDto cuenta, Font fuenteEtiqueta, Font fuenteValor)
		{
			PdfPTable tabla = GeneraTabla(2, [10f, 90f], 100, 10, 10);


			// FILA 1
			tabla.AddCell(CeldaSinBorde("Proveedor:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde($"{cuenta.Cta_Denominacion} - ({cuenta.Cta_Id})", fuenteValor, Element.ALIGN_LEFT));

			// FILA 2
			tabla.AddCell(CeldaSinBorde("Tipo. Doc.:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde($"{cuenta.Tdoc_Desc} N°: {cuenta.Cta_Documento}", fuenteValor, Element.ALIGN_LEFT));

			// FILA 3
			string domicilioCompleto = $"{cuenta.Cta_Domicilio} {cuenta.Cta_Localidad} CP: {cuenta.Cta_Cpostal}";
			tabla.AddCell(CeldaSinBorde("Domicilio:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(domicilioCompleto, fuenteValor, Element.ALIGN_LEFT));

			pdf.Add(tabla);
		}

		public static void CargarTablaAgenteDeRetencion1Col(Document pdf, CertificadosDto certi, Font fuenteEtiqueta, Font fuenteValor, Font titulo, bool mostrarInscripcion = false)
		{
			PdfPTable tabla = GeneraTabla(1, [100f], 100, 10, 10);
			tabla.AddCell(CeldaSinBorde("Datos del Agente de Retención:", titulo, Element.ALIGN_LEFT));
			tabla.AddCell(CeldaSinBorde("         ", titulo, Element.ALIGN_LEFT));

			// FILA 1 - Razon Social
			tabla.AddCell(CeldaSinBorde($"RAZÓN SOCIAL: {certi.emp_razon_social}", fuenteValor, Element.ALIGN_LEFT));

			// FILA 2 - DOMICILIO
			tabla.AddCell(CeldaSinBorde($"DOMICILIO: {certi.emp_domicilio}", fuenteValor, Element.ALIGN_LEFT));

			if (mostrarInscripcion)
			{
				// FILA 3 - Nro de Inscripción de Ingresos Brutos
				tabla.AddCell(CeldaSinBorde($"N° INSCRIPCIÓN INGRESOS BRUTOS: {certi.emp_ib_nro}", fuenteValor, Element.ALIGN_LEFT));
			}

			// FILA 4 - CUIT
			tabla.AddCell(CeldaSinBorde($"NÚMERO DE CUIT: {certi.emp_cuit}", fuenteValor, Element.ALIGN_LEFT));

			pdf.Add(tabla);
		}

		public static void CargarTablaAgenteDeRetencion2Col(Document pdf, CertificadosDto certi, Font fuenteEtiqueta, Font fuenteValor, Font titulo, bool mostrarInscripcion = false)
		{
			PdfPTable tablaTitulo = GeneraTabla(1, [100f], 100, 10, 10);
			tablaTitulo.AddCell(CeldaSinBorde("Datos del Agente de Retención:", titulo, Element.ALIGN_LEFT));
			pdf.Add(tablaTitulo);

			PdfPTable tabla = GeneraTabla(2, [25f, 75f], 100, 10, 10);

			// FILA 1 - Razon Social
			tabla.AddCell(CeldaSinBorde("RAZÓN SOCIAL:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde($"{certi.emp_razon_social}", fuenteValor, Element.ALIGN_LEFT));

			// FILA 2 - DOMICILIO
			string domicilioCompleto = $"{certi.emp_domicilio}";
			tabla.AddCell(CeldaSinBorde("DOMICILIO:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(domicilioCompleto, fuenteValor, Element.ALIGN_LEFT));

			if (mostrarInscripcion)
			{
				// FILA 3 - Nro de Inscripción de Ingresos Brutos
				tabla.AddCell(CeldaSinBorde("N° INSCRIPCIÓN INGRESOS BRUTOS:", fuenteEtiqueta, Element.ALIGN_RIGHT));
				tabla.AddCell(CeldaSinBorde($"{certi.emp_ib_nro}", fuenteValor, Element.ALIGN_LEFT));
			}

			// FILA 4 - CUIT
			tabla.AddCell(CeldaSinBorde("NÚMERO DE CUIT:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde($"{certi.emp_cuit}", fuenteValor, Element.ALIGN_LEFT));

			pdf.Add(tabla);
		}

		public static void CargarTablaCertificado(Document pdf, Certificado certi, Font fuenteEtiqueta, Font fuenteValor, Font titulo)
		{
			PdfPTable tabla = GeneraTabla(2, [80f, 20f], 100, 10, 10);

			// FILA 1 - Numero de Certificado
			tabla.AddCell(CeldaSinBorde("Número de Certificado: ", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde($"{certi.id}", fuenteValor, Element.ALIGN_LEFT));

			// FILA 2 - Fecha
			tabla.AddCell(CeldaSinBorde("Fecha: ", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.fecha.ToString("dd/MM/yyyy HH:mm"), fuenteValor, Element.ALIGN_LEFT));

			pdf.Add(tabla);
		}

		public static void CargarTablaCertificadoIIBBDetalle(Document pdf, CertRetenIBDto certi, Font fuenteEtiqueta, Font fuenteValor, Font titulo)
		{
			PdfPTable tabla = GeneraTabla(2, [25f, 75f], 100, 10, 10);

			// FILA 1 - Numero de Certificado
			tabla.AddCell(CeldaSinBorde("Número de Cuenta:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde($"{certi.cta_id}", fuenteValor, Element.ALIGN_LEFT));

			// FILA 2 - Razón Social
			tabla.AddCell(CeldaSinBorde("Razón Social:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.cib_raz_soc, fuenteValor, Element.ALIGN_LEFT));

			// FILA 3 - Domicilio
			tabla.AddCell(CeldaSinBorde("Domicilio:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.cib_domicilio, fuenteValor, Element.ALIGN_LEFT));

			// FILA 4 - N° Inscripción de IIBB
			tabla.AddCell(CeldaSinBorde("N° Inscripción de IIBB:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.cib_nro_ins, fuenteValor, Element.ALIGN_LEFT));

			// FILA 5 - Nro CUIT
			tabla.AddCell(CeldaSinBorde("Número de CUIT:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.cib_cuit, fuenteValor, Element.ALIGN_LEFT));

			// FILA 6 - Orden de Pago
			tabla.AddCell(CeldaSinBorde("Orden de Pago:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.op_compte, fuenteValor, Element.ALIGN_LEFT));

			// FILA 7 - Base Imponible
			tabla.AddCell(CeldaSinBorde("Base Imponible:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.cib_base.ToString("N2"), fuenteValor, Element.ALIGN_LEFT));

			// FILA 8 - Importe de Retencion IB
			tabla.AddCell(CeldaSinBorde("Importe de Retencion IB:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde($"{certi.cib_reten:N2}     (Alicuota: {certi.cib_ali})", fuenteValor, Element.ALIGN_LEFT));

			// FILA 9 - Importe de Retencion LH
			tabla.AddCell(CeldaSinBorde("Importe de Retencion LH:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde($"{certi.cib_reten_lh:N2}     (Alicuota: {certi.cib_ali_lh})", fuenteValor, Element.ALIGN_LEFT));

			// FILA 10 - Total Retenido
			var total = certi.cib_reten + certi.cib_reten_lh;
			tabla.AddCell(CeldaSinBorde("Total Retenido:", fuenteValor, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde($"{total:N2}", fuenteValor, Element.ALIGN_LEFT));

			// FILA 11 - Total en texto
			tabla.AddCell(CeldaSinBorde("", fuenteValor, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(HelperGen.EnLetras(total.ToString()), fuenteValor, Element.ALIGN_LEFT));

			pdf.Add(tabla);
		}

		public static void CargarTablaCertificadoIVADetalle(Document pdf, CertRetenIVADto certi, Font fuenteEtiqueta, Font fuenteValor, Font titulo)
		{
			PdfPTable tabla = GeneraTabla(2, [20f, 80f], 100, 10, 10);

			// FILA 1 - Numero de Certificado
			tabla.AddCell(CeldaSinBorde("Número de Cuenta:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde($"{certi.cta_id}", fuenteValor, Element.ALIGN_LEFT));

			// FILA 2 - Nro CUIT
			tabla.AddCell(CeldaSinBorde("Número de CUIT:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.civa_cuit, fuenteValor, Element.ALIGN_LEFT));

			// FILA 3 - Razón Social
			tabla.AddCell(CeldaSinBorde("Razón Social:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.civa_raz_soc, fuenteValor, Element.ALIGN_LEFT));

			// FILA 4 - Domicilio
			tabla.AddCell(CeldaSinBorde("Domicilio:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.civa_domicilio, fuenteValor, Element.ALIGN_LEFT));

			// FILA 5 - Impusto
			tabla.AddCell(CeldaSinBorde("Impuesto:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde("IVA", fuenteValor, Element.ALIGN_LEFT));

			// FILA 6 - Orden de Pago
			tabla.AddCell(CeldaSinBorde("Orden de Pago:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.op_compte, fuenteValor, Element.ALIGN_LEFT));

			// FILA 7 - Base Imponible
			tabla.AddCell(CeldaSinBorde("Base Imponible:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.civa_base.ToString("N2"), fuenteValor, Element.ALIGN_LEFT));

			// FILA 8 - Importe de Retencion IB
			tabla.AddCell(CeldaSinBorde("Importe de Retencion:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde($"{certi.civa_reten:N2}", fuenteValor, Element.ALIGN_LEFT));

			pdf.Add(tabla);
		}

		public static void CargarTablaCertificadoGanDetalle(Document pdf, CertRetenGananDto certi, Font fuenteEtiqueta, Font fuenteValor, Font titulo)
		{
			PdfPTable tabla = GeneraTabla(2, [20f, 80f], 100, 10, 10);

			// FILA 1 - Numero de Certificado
			tabla.AddCell(CeldaSinBorde("Número de Cuenta:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde($"{certi.cta_id}", fuenteValor, Element.ALIGN_LEFT));

			// FILA 2 - Nro CUIT
			tabla.AddCell(CeldaSinBorde("Número de CUIT:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.cgan_cuit, fuenteValor, Element.ALIGN_LEFT));

			// FILA 3 - Razón Social
			tabla.AddCell(CeldaSinBorde("Razón Social:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.cgan_raz_soc, fuenteValor, Element.ALIGN_LEFT));

			// FILA 4 - Domicilio
			tabla.AddCell(CeldaSinBorde("Domicilio:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.cgan_domicilio, fuenteValor, Element.ALIGN_LEFT));

			// FILA 5 - Impuesto
			tabla.AddCell(CeldaSinBorde("Impuesto:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde("GANANCIAS", fuenteValor, Element.ALIGN_LEFT));

			// FILA 6 - Regimen
			tabla.AddCell(CeldaSinBorde("Régimen:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.rgan_desc, fuenteValor, Element.ALIGN_LEFT));

			// FILA 7 - Orden de Pago
			tabla.AddCell(CeldaSinBorde("Orden de Pago:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.op_compte, fuenteValor, Element.ALIGN_LEFT));

			// FILA 8 - Base Imponible
			tabla.AddCell(CeldaSinBorde("Base Imponible:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(certi.cgan_base.ToString("N2"), fuenteValor, Element.ALIGN_LEFT));

			// FILA 9 - Importe de Retencion IB
			tabla.AddCell(CeldaSinBorde("Importe de Retencion:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde($"{certi.cgan_reten:N2}", fuenteValor, Element.ALIGN_LEFT));

			// FILA 10 - Total en texto
			tabla.AddCell(CeldaSinBorde("", fuenteValor, Element.ALIGN_RIGHT));
			tabla.AddCell(CeldaSinBorde(HelperGen.EnLetras(certi.cgan_reten.ToString()), fuenteValor, Element.ALIGN_LEFT));

			pdf.Add(tabla);
		}

		public static void CargarSeccionFirmaParaCertificadoDeRetencion(Document pdf, Font fuenteEtiqueta, Font fuenteValor, Font titulo, bool mostrarCargo, float bottom, float top)
		{
			PdfPTable tabla = GeneraTabla(1, [100f], 100, 10, 10);
			tabla.AddCell(CeldaSinBorde($"San Juan, {DateTime.Now.ToString("dd/MM/yyyy")}", fuenteEtiqueta, Element.ALIGN_LEFT));
			tabla.AddCell(CeldaSinBorde($"         ", fuenteEtiqueta, Element.ALIGN_LEFT));
			tabla.AddCell(CeldaSinBorde($"         ", fuenteEtiqueta, Element.ALIGN_LEFT));
			tabla.AddCell(CeldaSinBorde($"         ", fuenteEtiqueta, Element.ALIGN_LEFT));
			tabla.AddCell(CeldaSinBorde($"         ", fuenteEtiqueta, Element.ALIGN_LEFT));
			tabla.AddCell(CeldaSinBorde($"         ", fuenteEtiqueta, Element.ALIGN_LEFT));
			if (mostrarCargo)
			{
				tabla.AddCell(CeldaSinBorde($"                             Por Café América Mayorista S.A. Autorizado", fuenteValor, Element.ALIGN_LEFT));
				tabla.AddCell(CeldaSinBorde($"    CARGO: ...........................................................", fuenteEtiqueta, Element.ALIGN_LEFT));
			}
			else
			{
				tabla.AddCell(CeldaSinBorde($"         ", fuenteEtiqueta, Element.ALIGN_LEFT));
				tabla.AddCell(CeldaSinBorde($"                             Por Café América Mayorista S.A. Autorizado", fuenteValor, Element.ALIGN_LEFT));
			}
			tabla.AddCell(CeldaSinBorde($"         ", fuenteEtiqueta, Element.ALIGN_LEFT));
			tabla.AddCell(CeldaSinBorde($"Declaro bajo juramento que los datos consignados en la presente constancia son fiel expresión de la verdad.", fuenteEtiqueta, Element.ALIGN_LEFT));
			pdf.Add(tabla);

			var rect = new Rectangle(300, 200, 23, 100)
			{
				Border = Rectangle.BOX,
				BorderWidth = 1,
				BorderColor = new BaseColor(0, 0, 0),
				Bottom = bottom,
				Top = top
			};

			pdf.Add(rect);
		}

		public static void CargarSeccionCopiaParaCertificadoDeRetencion(Document pdf, PdfWriter writer)
		{
			PdfContentByte cb = writer.DirectContent;
			float MargenInferior = 15;
			float footerY = pdf.BottomMargin - MargenInferior;
			PdfPTable tabla = GeneraTabla(1, [100f], 100, 10, 10);

			float llx = 20; // esquina inferior izquierda X - Borde izquierdo
			float lly = 50; // esquina inferior izquierda Y - Borde Inferior
			float urx = pdf.PageSize.Width - 20; // esquina superior derecha X
			float ury = 65; // esquina superior derecha Y - Borde superior

			Rectangle rect = new(llx, lly, urx, ury)
			{
				Border = Rectangle.BOX,
				BorderWidth = 1,
				BorderColor = new BaseColor(0, 0, 0)
			};

			cb.Rectangle(rect);
			cb.Stroke();

			BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
			cb.SetFontAndSize(baseFont, 8);
			cb.BeginText();

			// Posicionamos el texto dentro del rectángulo
			string texto = "COPIA ORIGINAL PARA CAFÉ AMÉRICA";
			float x = (llx + (urx * 7 / 12)) / 2;
			float y = ((lly + ury) - 5) / 2;

			cb.ShowTextAligned(Element.ALIGN_RIGHT, texto, x, y, 0);
			cb.EndText();
		}

		public static void CargarSeccionPieOrdenDePagoProveedor(Document pdf, PdfWriter writer, List<ConsOrdPagoDetExtendDto> regs)
		{
			PdfContentByte cb = writer.DirectContent;
			float MargenInferior = 15;
			float footerY = pdf.BottomMargin - MargenInferior;
			PdfPTable tabla = GeneraTabla(1, [100f], 100, 10, 10);

			float llx = 20; // esquina inferior izquierda X - Borde izquierdo
			float lly = 50; // esquina inferior izquierda Y - Borde Inferior
			float urx = pdf.PageSize.Width - 20; // esquina superior derecha X
			float ury = 120; // esquina superior derecha Y - Borde superior
			float leading = 14; // espacio entre líneas

			Rectangle rect = new(llx, lly, urx, ury)
			{
				Border = Rectangle.BOX,
				BorderWidth = 1,
				BorderColor = new BaseColor(0, 0, 0)
			};

			cb.Rectangle(rect);
			cb.Stroke();

			BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
			BaseFont baseFontBold = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
			cb.SetFontAndSize(baseFont, 9);
			cb.BeginText();

			string texto = $"Son Pesos: ";
			float x = llx;
			float y = (lly + ury + 80) / 2;
			cb.ShowTextAligned(Element.ALIGN_LEFT, texto, x, y, 0);
			string textoBold = $"{HelperGen.EnLetras(regs.Where(x => x.Grupo.Equals("2") || x.Grupo.Equals("3") || x.Grupo.Equals("4")).Sum(y => y.Cc_importe).ToString())}";
			cb.SetFontAndSize(baseFontBold, 9);
			cb.ShowTextAligned(Element.ALIGN_LEFT, textoBold, x + 50, y, 0);
			cb.EndText();

			baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
			cb.SetFontAndSize(baseFont, 9);
			cb.BeginText();

			// Posicionamos el texto dentro del rectángulo
			string texto1 = "Este comprobante de pago, es suficiente recibo de cancelación y aceptación de todos los conceptos en él incluidos.";
			string texto2 = "Recibimos CONFORME los valores y remitos detallados.";
			x = ((24 * llx) + urx) / 2;
			y = (lly + ury + 40) / 2;

			cb.ShowTextAligned(Element.ALIGN_RIGHT, texto1, x, y, 0);
			cb.ShowTextAligned(Element.ALIGN_RIGHT, texto2, (x / 2) + 7, y - leading, 0);
			cb.EndText();

			// Suponiendo que ya tenés el objeto cb y el documento abierto
			float margenDerecho = 50f;
			float largoLinea = 150f;
			y = 60f; // Altura desde el borde inferior

			float xFin = pdf.PageSize.Width - margenDerecho;
			float xInicio = xFin - largoLinea;

			cb.SetLineWidth(1f);
			cb.SetColorStroke(BaseColor.Black);
			cb.MoveTo(xInicio, y);
			cb.LineTo(xFin, y);
			cb.Stroke();

		}

		public static void CargarTablaConceptosCancelados(Document pdf, List<ConsOrdPagoDetExtendDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			List<string> _campos = new List<string> { "Descripcion", "Importe" };
			List<string> _titulosTabla = new List<string>
			{
				"Concepto",
				"Comprobante",
				"CuentaGastoRelacionada",
				"Fecha",
				"Importe"
			};
			float[] _anchosTitulosTabla = new float[] { 30f, 20f, 20f, 10f, 20f };

			// FILA 1 - Título
			PdfPTable tablaTitulo = GeneraTabla(1, new float[] { 100f }, 100, 10, 0);

			PdfPCell celdaTitulo = new PdfPCell(
				new Phrase("Conceptos Cancelados", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				PaddingBottom = -2f
			};

			tablaTitulo.AddCell(celdaTitulo);
			pdf.Add(tablaTitulo);

			// Separador con Paragraph (opción 3)
			Paragraph separador = new Paragraph();
			LineSeparator linea = new LineSeparator(1f, 100f, BaseColor.Black, Element.ALIGN_LEFT, 0);
			separador.Add(new Chunk(linea));
			separador.SpacingBefore = 2f;  // espacio debajo del título
			separador.SpacingAfter = 4f;   // espacio encima de la cabecera
			pdf.Add(separador);

			// FILA 2 - Cabecera de la tabla
			HelperPdf.GeneraCabeceraLista(pdf, _titulosTabla, _anchosTitulosTabla, HelperPdf.FontNormalPredeterminado(true));

			// FILA 3 - Datos
			var regsAux = regs
				.Where(x => x.Grupo.Equals("1"))
				.Select(x => new
				{
					Concepto = x.Concepto,
					Comprobante = x.Cm_compte,
					CuentaGastoRelacionada = x.Ctag_motivo,
					Fecha = x.Cc_fecha_carga.ToString("dd/MM/yyyy"),
					Importe = x.Cc_importe
				})
				.ToList();

			// FILA 3 - Datos con alineación por columna
			PdfPTable tablaDatos = new PdfPTable(_titulosTabla.Count);
			tablaDatos.WidthPercentage = 100;
			tablaDatos.SetWidths(_anchosTitulosTabla);

			// Alineaciones por columna
			int[] alineaciones = new int[]
			{
				Element.ALIGN_LEFT,   // Concepto
				Element.ALIGN_CENTER, // Comprobante
				Element.ALIGN_LEFT,   // CuentaGastoRelacionada
				Element.ALIGN_CENTER, // Fecha
				Element.ALIGN_RIGHT   // Importe
			};

			foreach (var item in regsAux)
			{
				object[] valores = new object[]
				{
					item.Concepto,
					item.Comprobante,
					item.CuentaGastoRelacionada,
					item.Fecha,
					item.Importe.ToString("N2")
				};

				for (int i = 0; i < valores.Length; i++)
				{
					PdfPCell celda = new PdfPCell(new Phrase(valores[i].ToString(), fuenteEtiqueta))
					{
						HorizontalAlignment = alineaciones[i],
						VerticalAlignment = Element.ALIGN_MIDDLE,
						PaddingTop = 2f,
						PaddingBottom = 2f
					};

					tablaDatos.AddCell(celda);
				}
			}

			pdf.Add(tablaDatos);

			// FILA 4 - Total
			PdfPTable tablaTotal = GeneraTabla(1, new float[] { 100f }, 100, 0, 10);

			decimal totalConceptosCancelados = regs
				.Where(x => x.Grupo.Equals("1"))
				.Sum(y => y.Op_importe);

			PdfPCell celdaTotal = new PdfPCell(
				new Phrase($"Total Conceptos Cancelados: {totalConceptosCancelados}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};

			tablaTotal.AddCell(celdaTotal);
			pdf.Add(tablaTotal);
		}

		//public static void CargarTablaConceptosCancelados(Document pdf, List<ConsOrdPagoDetExtendDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		//{
		//	List<string> _campos = ["Descripcion", "Importe",];
		//	List<string> _titulosTabla = ["Concepto", "Comprobante", "CuentaGastoRelacionada", "Fecha", "Importe",];
		//	float[] _anchosTitulosTabla = [30f, 15f, 15, 20, 20f];
		//	PdfPTable tablaTitulo = GeneraTabla(1, [100f], 100, 10, 0);

		//	// FILA 1
		//	PdfPCell celdaTitulo = new PdfPCell(new Phrase("Conceptos Cancelados", HelperPdf.FontNormalPredeterminado(true)))
		//	{
		//		Border = Rectangle.NO_BORDER,
		//		HorizontalAlignment = Element.ALIGN_LEFT,
		//		VerticalAlignment = Element.ALIGN_MIDDLE,
		//		PaddingTop = 0f,
		//		PaddingBottom = -2f
		//	};
		//	tablaTitulo.AddCell(celdaTitulo);
		//	pdf.Add(tablaTitulo);

		//	Chunk linebreak = new Chunk(new LineSeparator(1f, 17f, BaseColor.Black, Element.ALIGN_LEFT, 3));
		//	pdf.Add(linebreak);

		//	// FILA 2
		//	HelperPdf.GeneraCabeceraLista(pdf, _titulosTabla, _anchosTitulosTabla, HelperPdf.FontNormalPredeterminado(true));

		//	// FILA 3
		//	//hago el modelo de dato aca ya que necesito los datos de la cuenta
		//	var regsAux = regs.Where(x => x.Grupo.Equals("1")).Select(x => new
		//	{
		//		x.Concepto,
		//		Comprobante = x.Cm_compte,
		//		CuentaGastoRelacionada = x.Ctag_motivo,
		//		Fecha = x.Cc_fecha_carga.ToString("dd/MM/yyyy"),
		//		Importe = x.Cc_importe
		//	}).ToList();
		//	HelperPdf.GenerarListadoDesdeLista(pdf, regsAux, _titulosTabla, _anchosTitulosTabla, fuenteEtiqueta);

		//	// FILA 4
		//	PdfPTable tablaTotal = GeneraTabla(1, [100f], 100, 0, 10);
		//	PdfPCell celdaTotal = new PdfPCell(new Phrase($"Total Conceptos Cancelados: {regs.Where(x => x.Grupo.Equals("1")).Sum(y => y.Op_importe)}", HelperPdf.FontNormalPredeterminado(true)))
		//	{
		//		Border = Rectangle.NO_BORDER,
		//		HorizontalAlignment = Element.ALIGN_RIGHT,
		//		VerticalAlignment = Element.ALIGN_MIDDLE,
		//		PaddingTop = 0f
		//	};

		//	tablaTotal.AddCell(celdaTotal);
		//	pdf.Add(tablaTotal);
		//}

		public static void CargarTablaFormaDePago(Document pdf, List<ConsOrdPagoDetExtendDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			PdfPTable tablaTitulo = GeneraTabla(1, [100f], 100, 10, 0);
			List<string> _campos = ["Concepto", "Importe",];
			float[] _anchosTitulosTabla = [80f, 20f];
			// FILA 1
			PdfPCell celdaTitulo = new PdfPCell(new Phrase("Forma de Pago", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				PaddingBottom = 0f
			};
			tablaTitulo.AddCell(celdaTitulo);

			PdfPCell celdaVacia = new PdfPCell(new Phrase("", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				PaddingBottom = 5f
			};
			tablaTitulo.AddCell(celdaVacia);
			pdf.Add(tablaTitulo);

			// FILA 2
			// hago el modelo de dato 
			var regsAux = regs.Where(x => x.Grupo.Equals("2") || x.Grupo.Equals("3")).Select(x => new
			{
				x.Concepto,
				Importe = x.Cc_importe
			}).ToList();
			HelperPdf.GenerarListadoDesdeLista(pdf, regsAux, _campos, _anchosTitulosTabla, fuenteEtiqueta);
		}

		public static void CargarTablaValoresEntregados(Document pdf, List<ConsOrdPagoDetExtendDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			PdfPTable tablaTitulo = GeneraTabla(1, [100f], 100, 10, 0);
			List<string> _campos = ["Concepto", "Importe",];
			float[] _anchosTitulosTabla = [80f, 20f];
			// FILA 1
			PdfPCell celdaTitulo = new PdfPCell(new Phrase("Valores Entregados", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				PaddingBottom = 0f
			};
			tablaTitulo.AddCell(celdaTitulo);
			//pdf.Add(tablaTitulo);

			PdfPCell celdaVacia = new PdfPCell(new Phrase("", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				PaddingBottom = 5f
			};
			tablaTitulo.AddCell(celdaVacia);
			pdf.Add(tablaTitulo);

			// FILA 2
			// hago el modelo de dato 
			var regsAux = regs.Where(x => x.Grupo.Equals("4")).Select(x => new
			{
				x.Concepto,
				Importe = x.Cc_importe
			}).ToList();
			HelperPdf.GenerarListadoDesdeLista(pdf, regsAux, _campos, _anchosTitulosTabla, fuenteEtiqueta);
		}

		public static void CargarTablaTotalValoresCancelatorios(Document pdf, List<ConsOrdPagoDetExtendDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			PdfPTable tablaTotal = GeneraTabla(1, [100f], 100, 0, 10);
			PdfPCell celdaTotal = new PdfPCell(new Phrase($"Total Valores Cancelatorios: {regs.Where(x => x.Grupo.Equals("2") || x.Grupo.Equals("3") || x.Grupo.Equals("4")).Sum(y => y.Cc_importe)}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};

			tablaTotal.AddCell(celdaTotal);
			pdf.Add(tablaTotal);
		}

		public static void CargarTablaConceptosOrdenesDePago(Document pdf, List<OrdenDePagoConsultaDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			List<string> _campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe",];
			List<string> _titulosTabla = ["Nro", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe",];
			float[] _anchosTitulosTabla = [10f, 20f, 10, 20, 10f, 20, 10];
			PdfPTable tablaTitulo = GeneraTabla(1, [100f], 100, 10, 0);

			// FILA 1
			HelperPdf.GeneraCabeceraLista(pdf, _titulosTabla, _anchosTitulosTabla, HelperPdf.FontNormalPredeterminado(true));

			// FILA 2
			var regsAux = regs.Select(x => new
			{
				Nro = x.op_compte,
				Tipo = x.opt_desc,
				Fecha = x.op_fecha.ToString("dd/MM/yyyy"),
				Proveedor = x.cta_denominacion,
				Anulada = x.op_anulada_desc,
				Usuario = x.usu_apellidoynombre,
				Importe = x.op_importe
			}).ToList();
			HelperPdf.GenerarListadoDesdeLista(pdf, regsAux, _titulosTabla, _anchosTitulosTabla, fuenteEtiqueta);

			// FILA 3
			PdfPTable tablaTotal = GeneraTabla(1, [100f], 100, 0, 10);
			PdfPCell celdaTotal = new PdfPCell(new Phrase($"Total Ordenes de Pago: {regs.Sum(y => y.op_importe).ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				BackgroundColor = BaseColor.LightGray
			};

			tablaTotal.AddCell(celdaTotal);
			pdf.Add(tablaTotal);
		}

		public static void CargarTablaDatosDeProveedorEnOrdenDeCompra(Document pdf, OrdenDeCompraDto oc, CuentaDto cta, Font fuenteEtiqueta, Font fuenteValor, Font titulo)
		{
			PdfPTable tabla = GeneraTabla(2, [85f, 15f], 100, 10, 10);
			PdfPTable tablaDatos = GeneraTabla(4, [20f, 45f, 15f, 20f], 100, 10, 10);

			// FILA 1
			tablaDatos.AddCell(CeldaSinBorde("Proveedor:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde($"{cta.Cta_Id} {cta.Cta_Denominacion}", fuenteValor, Element.ALIGN_LEFT));
			tablaDatos.AddCell(CeldaSinBorde("Email:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde(cta.Cta_Email, fuenteValor, Element.ALIGN_LEFT));

			// FILA 2
			tablaDatos.AddCell(CeldaSinBorde("Fecha generación:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde(oc.Oc_Fecha.ToString("dd/MM/yyyy"), fuenteValor, Element.ALIGN_LEFT));
			tablaDatos.AddCell(CeldaSinBorde("Fecha entrega:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde(oc.Oc_Entrega_Fecha.ToString("dd/MM/yyyy"), fuenteValor, Element.ALIGN_LEFT));

			// FILA 3 
			tablaDatos.AddCell(CeldaSinBorde("Pago Ant.:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde(oc.Oc_Pago_Ant == 'S' ? "SI" : "NO", fuenteValor, Element.ALIGN_LEFT));
			tablaDatos.AddCell(CeldaSinBorde("Vto. Pago Ant.:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde(oc.Oc_Pago_Ant_Vto == null ? "" : oc.Oc_Pago_Ant_Vto.Value.ToString("dd/MM/yyyy"), fuenteValor, Element.ALIGN_LEFT));

			// FILA 4 
			tablaDatos.AddCell(CeldaSinBorde("Flete:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde("No Pago", fuenteValor, Element.ALIGN_LEFT));
			tablaDatos.AddCell(CeldaSinBorde("Dep. Entrega:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde("Santa Lucia", fuenteValor, Element.ALIGN_LEFT));

			PdfPCell celdaSubTabla = new PdfPCell(tablaDatos)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaSubTabla);

			//tabla.AddCell(tablaDatos);
			tabla.AddCell(CeldaSinBorde($"Generado por: {oc.Usu_Apellidoynombre}", fuenteEtiqueta, Element.ALIGN_RIGHT));

			pdf.Add(tabla);
		}

		public static void CargarTablaDatosDeDetalleEnOrdenDeCompra(Document pdf, OrdenDeCompraDto reg, List<OrdenDeCompraDetalleDto> regs, List<OrdenDeCompraConceptoDto> resumen, Font fuenteValor, Font fuenteEtiqueta, PdfWriter writer)
		{
			// 1) Tabla de detalle
			GenerarDetalleDeOrdenDeCompra(pdf, regs, fuenteValor, fuenteEtiqueta);

			// FILA 3
			// 2) Construimos las tablas del resumen (pero NO las agregamos todavía)
			PdfPTable tablaTotal = GeneraTabla(1, new float[] { 100f }, 100, 0, 10);
			tablaTotal.SpacingBefore = 10f; // ← ESPACIO ENTRE DETALLE Y RESUMEN

			// Gravados
			var listaTemp = new List<int>() { 3, 4, 5 };
			var gravados = resumen.Where(y => listaTemp.Contains(y.Orden)).Sum(x => x.Importe);
			PdfPCell celdaGravado = new(new Phrase($"Gravados: {gravados.ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};
			tablaTotal.AddCell(celdaGravado);

			// No Gravados
			listaTemp = new List<int>() { 2, 6, 7, 8 };
			var noGravados = resumen.Where(y => listaTemp.Contains(y.Orden)).Sum(x => x.Importe);
			PdfPCell celdaNoGravado = new(new Phrase($"No Gravados: {noGravados.ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};
			tablaTotal.AddCell(celdaNoGravado);

			// Flete
			PdfPCell celdaFlete = new(new Phrase($"Flete: {resumen.Where(x => x.Orden == 3).First().Importe.ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};
			tablaTotal.AddCell(celdaFlete);

			// IVA Flete
			PdfPCell celdaIVAFlete = new(new Phrase($"IVA Flete: {resumen.Where(x => x.Orden == 4).First().Importe.ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};
			tablaTotal.AddCell(celdaIVAFlete);

			// Subtotal de la orden de compra
			PdfPCell celdaSubTotal = new(new Phrase($"SUBTOTAL: ................. {resumen.Where(x => x.Orden == 1).First().Importe.ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};
			tablaTotal.AddCell(celdaSubTotal);

			// Impuestos Internos
			PdfPCell celdaIN = new(new Phrase($"Impuestos Internos: {resumen.Where(x => x.Orden == 2).First().Importe.ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};
			tablaTotal.AddCell(celdaIN);

			// IVA 21%
			PdfPCell celdaIva21 = new(new Phrase($"I.V.A. (21.00%): {resumen.Where(x => x.Orden == 5).First().Importe.ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};
			tablaTotal.AddCell(celdaIva21);

			// Total de la orden de compra
			PdfPCell celdaTotal = new(new Phrase($"TOTAL ................. {resumen.Where(x => x.Orden == 100).First().Importe.ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				BackgroundColor = BaseColor.LightGray
			};
			tablaTotal.AddCell(celdaTotal);

			PdfPTable tablaObservaciones = GeneraTabla(1, new float[] { 100f }, 100, 0, 10);
			tablaObservaciones.AddCell(CeldaSinBorde($"Observaciones: {reg.Oc_Observaciones}", fuenteValor, Element.ALIGN_LEFT));

			// 3) Medimos espacio disponible
			float espacioDisponible = writer.GetVerticalPosition(true);

			// 4) Medimos cuánto ocupa el resumen
			float alturaResumen =
				tablaTotal.CalculateHeights(true) +
				tablaObservaciones.CalculateHeights(true);

			// 5) Decidimos si entra o no
			bool entra = espacioDisponible - alturaResumen > 20f;

			if (!entra)
				pdf.NewPage();

			// 6) Ahora sí agregamos el resumen

			pdf.Add(tablaTotal);
			pdf.Add(tablaObservaciones);
		}

		private static void GenerarDetalleDeOrdenDeCompra(Document pdf, List<OrdenDeCompraDetalleDto> lista, Font fuenteValor, Font fuenteEtiqueta)
		{
			if (lista == null || lista.Count == 0)
			{
				Paragraph sinDatos = new Paragraph("No hay detalles de la orden de compra.", fuenteEtiqueta);
				pdf.Add(sinDatos);
				return;
			}

			// Definición de columnas
			string[] columnas = new[]
			{
				"Código", "Descripción", "Cod. Prov", "P. Lista",
				"Dto1", "Dto2", "Dto3", "Dto4", "Dto Pago",
				"BxB", "Cant", "Bonif", "P. Costo",
				"Cant Total", "Total"
			};

			// Cantidad de columnas
			PdfPTable tabla = new PdfPTable(columnas.Length);
			tabla.WidthPercentage = 100;

			// Anchos relativos (ajustalos si querés)
			tabla.SetWidths(new float[]
			{
				7f, 31f, 7f, 10f,
				6f, 6f, 6f, 6f, 8f,
				6f, 6f, 6f, 10f,
				8f, 12f
			});

			// ============================
			// Encabezados
			// ============================
			foreach (var col in columnas)
			{
				PdfPCell celda = new PdfPCell(new Phrase(col, fuenteEtiqueta));
				celda.HorizontalAlignment = Element.ALIGN_CENTER;
				celda.BackgroundColor = new BaseColor(240, 200, 0);
				celda.Padding = 4;
				tabla.AddCell(celda);
			}
			// Repetir encabezado en cada página
			tabla.HeaderRows = 1;

			// ============================
			// Filas
			// ============================
			foreach (var x in lista)
			{
				// Mapeo directo según tu especificación
				var valores = new List<string>
				{
					x.p_id,
					x.p_desc?.Trim() ?? "",
					x.p_id_prov,
					GridHelper.FormatearPrecio(x.ocd_plista, TipoPrecio.Lista),
					x.ocd_dto1.ToString("N2"),
					x.ocd_dto2.ToString("N2"),
					x.ocd_dto3.ToString("N2"),
					x.ocd_dto4.ToString("N2"),
					x.ocd_dto_pa.ToString("N2"),
					x.ocd_unidad_x_bulto.ToString(),
					x.ocd_cantidad.ToString(),
					x.ocd_bonificacion.ToString(),
					GridHelper.FormatearPrecio(x.ocd_pcosto, TipoPrecio.Costo),
					(x.ocd_cantidad + x.ocd_bonificacion).ToString(),
					GridHelper.FormatearPrecio(x.ocd_pcosto_tot, TipoPrecio.Costo)
				};

				foreach (var val in valores)
				{
					PdfPCell celda = new PdfPCell(new Phrase(val, fuenteValor));
					celda.HorizontalAlignment = Element.ALIGN_RIGHT;
					celda.Padding = 3;

					// Alineación especial para texto
					if (!decimal.TryParse(val, out _))
						celda.HorizontalAlignment = Element.ALIGN_LEFT;

					tabla.AddCell(celda);
				}
			}

			// Agregar tabla al PDF
			pdf.Add(tabla);
		}

		public static void CargarTablaDatosDeAcuseDeTransferencia_Encabezado(Document pdf, FinancieroTraRepoDDto fTra, Font fuenteEtiqueta, Font fuenteValor, Font titulo)
		{
			PdfPTable tabla = GeneraTabla(1, [100f], 100, 10, 10);
			PdfPTable tablaDatos = GeneraTabla(4, [20f, 45f, 15f, 20f], 100, 10, 10);

			// FILA 0
			tablaDatos.AddCell(CeldaSinBorde(string.Empty, fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde(string.Empty, fuenteValor, Element.ALIGN_LEFT));
			tablaDatos.AddCell(CeldaSinBorde(string.Empty, fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde(string.Empty, fuenteValor, Element.ALIGN_LEFT));

			// FILA 1
			tablaDatos.AddCell(CeldaSinBorde("Tipo de Transferencia:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde(fTra.ttra_desc, fuenteValor, Element.ALIGN_LEFT));
			tablaDatos.AddCell(CeldaSinBorde("Fec. Registro:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde(fTra.tra_fecha.ToString("dd/MM/yyyy"), fuenteValor, Element.ALIGN_LEFT));

			// FILA 2
			tablaDatos.AddCell(CeldaSinBorde("Concepto:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde(fTra.tra_concepto.ToUpper(), fuenteValor, Element.ALIGN_LEFT));
			tablaDatos.AddCell(CeldaSinBorde(string.Empty, fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde(string.Empty, fuenteValor, Element.ALIGN_LEFT));

			// FILA 3 
			tablaDatos.AddCell(CeldaSinBorde("Fec. Movimiento:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde(fTra.tra_fecha_movi.ToString("dd/MM/yyyy"), fuenteValor, Element.ALIGN_LEFT));
			tablaDatos.AddCell(CeldaSinBorde("Registrado por:", fuenteEtiqueta, Element.ALIGN_RIGHT));
			tablaDatos.AddCell(CeldaSinBorde(fTra.usu_apellidoynombre, fuenteValor, Element.ALIGN_LEFT));

			PdfPCell celdaSubTabla = new PdfPCell(tablaDatos)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};

			tabla.AddCell(celdaSubTabla);

			pdf.Add(tabla);
		}

		public static void CargarTablaDatosDeAcuseDeTransferencia_Origen(Document pdf, List<FinancieroTraRepoDDto> registros, Font fuenteEtiqueta, Font fuenteValor, Font titulo)
		{
			// FILA 1 - TITULO DE SECCION
			PdfPTable tabla = GeneraTabla(1, [100f], 100, 10, 10);
			PdfPCell celdaTitulo = new PdfPCell(new Phrase("Origen de Transferencias", HelperPdf.FontSubtituloPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				PaddingBottom = 0f
			};
			tabla.AddCell(celdaTitulo);
			pdf.Add(tabla);

			//FILA 2 - TABLA
			List<string> _titulosTabla = ["Código", "Denominación", "Concepto", "Monto",];
			float[] _anchosTitulosTabla = [10f, 30f, 40f, 20f];
			List<string> _campos = ["ctaf_id", "ctaf_denominacion", "concepto", "fc_importe",];

			HelperPdf.GeneraCabeceraLista(pdf, _titulosTabla, _anchosTitulosTabla, HelperPdf.FontNormalPredeterminado(true));

			var regsAux = registros.Select(x => new
			{
				x.ctaf_id,
				x.ctaf_denominacion,
				x.concepto,
				x.fc_importe
			}).ToList();
			HelperPdf.GenerarListadoDesdeLista(pdf, regsAux, _campos, _anchosTitulosTabla, fuenteEtiqueta);

			// FILA 3
			PdfPTable tablaTotal = GeneraTabla(1, [100f], 100, 0, 10);
			PdfPCell celdaTotal = new PdfPCell(new Phrase($"Total Egreso Cuentas Origen: {registros.Sum(y => y.fc_importe).ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};
			tablaTotal.AddCell(celdaTotal);
			pdf.Add(tablaTotal);
		}

		public static void CargarTablaDatosDeAcuseDeTransferencia_Destino(Document pdf, List<FinancieroTraRepoDDto> registros, Font fuenteEtiqueta, Font fuenteValor, Font titulo)
		{
			// FILA 1 - TITULO DE SECCION
			PdfPTable tabla = GeneraTabla(1, [100f], 100, 10, 10);
			PdfPCell celdaTitulo = new PdfPCell(new Phrase("Destino de Transferencias", HelperPdf.FontSubtituloPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				PaddingBottom = 0f
			};
			tabla.AddCell(celdaTitulo);
			pdf.Add(tabla);

			//FILA 2 - TABLA
			List<string> _titulosTabla = ["Código", "Denominación", "Concepto", "Monto",];
			float[] _anchosTitulosTabla = [10f, 30f, 40f, 20f];
			List<string> _campos = ["ctaf_id", "ctaf_denominacion", "concepto", "fc_importe",];

			HelperPdf.GeneraCabeceraLista(pdf, _titulosTabla, _anchosTitulosTabla, HelperPdf.FontNormalPredeterminado(true));

			var regsAux = registros.Select(x => new
			{
				x.ctaf_id,
				x.ctaf_denominacion,
				x.concepto,
				x.fc_importe
			}).ToList();
			HelperPdf.GenerarListadoDesdeLista(pdf, regsAux, _campos, _anchosTitulosTabla, fuenteEtiqueta);

			// FILA 3
			PdfPTable tablaTotal = GeneraTabla(1, [100f], 100, 0, 10);
			PdfPCell celdaTotal = new PdfPCell(new Phrase($"Total Ingreso Cuentas Destino: {registros.Sum(y => y.fc_importe).ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};
			tablaTotal.AddCell(celdaTotal);
			pdf.Add(tablaTotal);
		}

		public static void CargarTablaDatosDeAcuseDeTransferencia_Ctag(Document pdf, List<FinancieroTraRepoCtagDto> registros, Font fuenteEtiqueta, Font fuenteValor, Font titulo)
		{
			// FILA 1 - TITULO DE SECCION
			PdfPTable tabla = GeneraTabla(1, [100f], 100, 10, 10);
			PdfPCell celdaTitulo = new PdfPCell(new Phrase("Gastos por Transferencias", HelperPdf.FontSubtituloPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				PaddingBottom = 0f
			};
			tabla.AddCell(celdaTitulo);
			pdf.Add(tabla);

			//FILA 2 - TABLA
			List<string> _titulosTabla = ["Código", "Denominación", "Tipo Comprobante", "Comprobante", "Número", "Importe",];
			float[] _anchosTitulosTabla = [10f, 30f, 10f, 20f, 20f, 10f];
			List<string> _campos = ["ctag_id", "ctag_denominacion", "tco_id", "tco_desc", "cm_compte", "cm_importe",];

			HelperPdf.GeneraCabeceraLista(pdf, _titulosTabla, _anchosTitulosTabla, HelperPdf.FontNormalPredeterminado(true));

			var regsAux = registros.Select(x => new
			{
				x.ctag_id,
				x.ctag_denominacion,
				x.tco_id,
				x.tco_desc,
				x.cm_compte,
				x.cm_importe,
			}).ToList();
			HelperPdf.GenerarListadoDesdeLista(pdf, regsAux, _campos, _anchosTitulosTabla, fuenteEtiqueta);

			// FILA 3
			PdfPTable tablaTotal = GeneraTabla(1, [100f], 100, 0, 10);
			PdfPCell celdaTotal = new PdfPCell(new Phrase($"Total: {registros.Sum(y => y.cm_importe).ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};
			tablaTotal.AddCell(celdaTotal);
			pdf.Add(tablaTotal);
		}

		public static void CargarTablaDatosDeAcuseDeTransferencia_Total(Document pdf, List<FinancieroTraRepoDDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			//PdfPCell celdaTotal = new PdfPCell(new Phrase($"Total Egreso Cuentas Origen: {registros.Sum(y => y.fc_importe).ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			PdfPTable tablaTotal = GeneraTabla(1, [100f], 100, 0, 10);
			PdfPCell celdaTotal = new PdfPCell(new Phrase($"Total de Ingreso en Cuentas Destino y Gastos Asociados: {regs.Where(x => x.grupo.Equals(1)).Sum(y => y.fc_importe).ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};

			tablaTotal.AddCell(celdaTotal);
			pdf.Add(tablaTotal);
		}

		public static void CargarTablaMovimientosFinancieros(Document pdf, List<MovimientoFinancieroListaDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			List<string> _campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe",];
			List<string> _titulosTabla = ["Nro", "Fecha", "Tipo", "Concepto", "Anulada", "Usuario", "Importe",];
			float[] _anchosTitulosTabla = [10f, 20f, 10, 20, 10f, 20, 10];
			PdfPTable tablaTitulo = GeneraTabla(1, [100f], 100, 10, 0);

			// FILA 1
			HelperPdf.GeneraCabeceraLista(pdf, _titulosTabla, _anchosTitulosTabla, HelperPdf.FontNormalPredeterminado(true));

			// FILA 2
			var regsAux = regs.Select(x => new
			{
				Nro = x.tra_compte,
				Fecha = x.tra_fecha,
				Tipo = x.ttra_desc,
				Concepto = x.tra_concepto,
				Anulada = x.strAnulada,
				Usuario = x.usu_apellidoynombre,
				Importe = x.tra_importe
			}).ToList();
			HelperPdf.GenerarListadoDesdeLista(pdf, regsAux, _titulosTabla, _anchosTitulosTabla, fuenteEtiqueta);

			// FILA 3
			PdfPTable tablaTotal = GeneraTabla(1, [100f], 100, 0, 10);
			PdfPCell celdaTotal = new(new Phrase($"Total Ordenes de Pago: {regs.Sum(y => y.tra_importe).ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				BackgroundColor = BaseColor.LightGray
			};

			tablaTotal.AddCell(celdaTotal);
			pdf.Add(tablaTotal);
		}

		public static void CargarTablaExtractoBancarioFinancieros(Document pdf, List<FinancieroBcoExtractoDto> regs, Font fuenteEtiqueta,Font fuenteValor)
		{
			// Definición de columnas
			float[] anchos = { 5f, 5f, 25f, 25f, 10f, 10f, 10f, 5f, 5f };
			string[] titulos = {
				"Fecha Movi", "Cod. Movi", "Origen", "Concepto",
				"Debe", "Haber", "Saldo", "Conciliado", "Cierre"
			};

			// Crear tabla con 9 columnas
			PdfPTable tabla = new PdfPTable(9);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(anchos);

			// ============================
			// CABECERA (se repetirá en cada hoja)
			// ============================
			foreach (var titulo in titulos)
			{
				PdfPCell celda = new PdfPCell(new Phrase(titulo, fuenteValor))
				{
					BackgroundColor = BaseColor.LightGray,
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					Padding = 5
				};
				tabla.AddCell(celda);
			}

			// Marcar la cabecera como repetible
			tabla.HeaderRows = 1;

			// ============================
			// CONTENIDO
			// ============================
			foreach (var x in regs)
			{
				tabla.AddCell(new PdfPCell(new Phrase(
					x.ext_fecha.ToString("dd/MM/yy"), fuenteEtiqueta))
				{ HorizontalAlignment = Element.ALIGN_CENTER });

				tabla.AddCell(new PdfPCell(new Phrase(
					x.extr_id.ToString(), fuenteEtiqueta))
				{ HorizontalAlignment = Element.ALIGN_CENTER });

				tabla.AddCell(new PdfPCell(new Phrase(
					x.extr_desc ?? "", fuenteEtiqueta))
				{ HorizontalAlignment = Element.ALIGN_LEFT });

				tabla.AddCell(new PdfPCell(new Phrase(
					x.ext_concepto ?? "", fuenteEtiqueta))
				{ HorizontalAlignment = Element.ALIGN_LEFT });

				tabla.AddCell(new PdfPCell(new Phrase(
					x.ext_debe.ToString("N2"), fuenteEtiqueta))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				tabla.AddCell(new PdfPCell(new Phrase(
					x.ext_haber.ToString("N2"), fuenteEtiqueta))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				tabla.AddCell(new PdfPCell(new Phrase(
					x.ext_saldo.ToString("N2"), fuenteEtiqueta))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				tabla.AddCell(new PdfPCell(new Phrase(
					x.strConciliado ? "SI" : "NO", fuenteEtiqueta))
				{ HorizontalAlignment = Element.ALIGN_CENTER });

				tabla.AddCell(new PdfPCell(new Phrase(
					x.strCierre ? "SI" : "NO", fuenteEtiqueta))
				{ HorizontalAlignment = Element.ALIGN_CENTER });
			}

			// Agregar tabla al PDF
			pdf.Add(tabla);
		}


		//public static void CargarTablaExtractoBancarioFinancieros(Document pdf, List<FinancieroBcoExtractoDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		//{
		//	List<string> _campos = ["Fecha", "Codigo", "Origen", "Concepto", "Debe", "Haber", "Saldo", "strConciliado", "strCierre",];
		//	List<string> _titulosTabla = ["Fecha Movi", "Cod. Movi", "Origen", "Concepto", "Debe", "Haber", "Saldo", "Conciliado", "Cierre",];
		//	float[] _anchosTitulosTabla = [5, 5, 25, 25, 10, 10, 10, 5, 5];
		//	PdfPTable tablaTitulo = GeneraTabla(1, [100f], 100, 10, 0);

		//	// FILA 1
		//	HelperPdf.GeneraCabeceraLista(pdf, _titulosTabla, _anchosTitulosTabla, HelperPdf.FontNormalPredeterminado(true));

		//	// FILA 2
		//	var regsAux = regs.Select(x => new
		//	{
		//		Fecha = x.ext_fecha,
		//		Codigo = x.extr_id,
		//		Origen = x.extr_desc,
		//		Concepto = x.ext_concepto,
		//		Debe = x.ext_debe,
		//		Haber = x.ext_haber,
		//		Saldo = x.ext_saldo,
		//		strConciliado = x.strConciliado,
		//		strCierre = x.strCierre
		//	}).ToList();
		//	HelperPdf.GenerarListadoDesdeLista(pdf, regsAux, _campos, _anchosTitulosTabla, fuenteEtiqueta, false, false, null, true, BooleanDisplayFormat.SiNo, false, false);
		//}

		public static void CargarTablaCtaCteFinancieros(Document pdf, List<FinancieroBcoCtaCteDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			// Definición de columnas
			float[] anchos = { 10f, 6f, 7f, 7f, 41f, 12f, 12f, 5f };
			string[] titulos = { "Movimiento", "Fecha", "Vencimiento", "Percibido", "Concepto", "Debe", "Haber", "Conc." };

			// Crear tabla con 8 columnas
			PdfPTable tabla = new PdfPTable(8);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(anchos);

			// ============================
			// CABECERA (se repetirá en cada hoja)
			// ============================
			foreach (var titulo in titulos)
			{
				PdfPCell celda = new PdfPCell(new Phrase(titulo, fuenteValor))
				{
					BackgroundColor = BaseColor.LightGray,
					HorizontalAlignment = Element.ALIGN_CENTER,
					Padding = 5
				};
				tabla.AddCell(celda);
			}

			// Marcar la cabecera como repetible
			tabla.HeaderRows = 1;

			// ============================
			// CONTENIDO
			// ============================
			foreach (var x in regs)
			{
				tabla.AddCell(new PdfPCell(new Phrase(x.dia_movi.ToString(), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(x.cf_fecha.ToString("dd/MM/yy"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(x.fecha_cheque?.ToString("dd/MM/yy") ?? "", fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(x.cf_fecha_concilia?.ToString("dd/MM/yy") ?? "", fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(x.cf_concepto, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_LEFT });
				tabla.AddCell(new PdfPCell(new Phrase(x.cf_debe.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(x.cf_haber.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(x.strConciliado ? "SI" : "NO", fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
			}

			// Agregar tabla al PDF
			pdf.Add(tabla);
		}


		//public static void CargarTablaCtaCteFinancieros(Document pdf, List<FinancieroBcoCtaCteDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		//{
		//	List<string> _campos = ["Movimiento", "Fecha", "Vencimiento", "Percibido", "Concepto", "Debe", "Haber", "strConciliado",];
		//	List<string> _titulosTabla = ["Movimiento", "Fecha", "Vencimiento", "Percibido", "Concepto", "Debe", "Haber", "Conciliado",];
		//	float[] _anchosTitulosTabla = [10, 6, 7, 7, 30, 15, 15, 10];
		//	PdfPTable tablaTitulo = GeneraTabla(1, [100f], 100, 10, 0);

		//	// FILA 1
		//	HelperPdf.GeneraCabeceraLista(pdf, _titulosTabla, _anchosTitulosTabla, HelperPdf.FontNormalPredeterminado(true));

		//	// FILA 2
		//	var regsAux = regs.Select(x => new
		//	{
		//		Movimiento = x.dia_movi,
		//		Fecha = x.cf_fecha,
		//		Vencimiento = x.fecha_cheque,
		//		Percibido = x.cf_fecha_concilia,
		//		Concepto = x.cf_concepto,
		//		Debe = x.cf_debe,
		//		Haber = x.cf_haber,
		//		strConciliado = x.strConciliado,
		//	}).ToList();
		//	HelperPdf.GenerarListadoDesdeLista(pdf, regsAux, _campos, _anchosTitulosTabla, fuenteEtiqueta, false, false, null, true, BooleanDisplayFormat.SiNo, false, false);
		//}

		public static void CargarTablaLibroBancoResumenFinancieros(Document pdf, List<FinancieroBcoLibroResumenDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			List<string> _campos = ["Descripcion", "Saldo", "H1", "H2",];
			float[] _anchoCampoTabla = [75, 25, 0, 0];
			PdfPTable tablaTitulo = new(3)
			{
				WidthPercentage = 100
			};
			tablaTitulo.SetWidths(new float[] { 49, 2, 49 }); // espacio central del 4%

			// CELDA #1
			var regsAux = ObtenerGrillaCuentaFinanciera(regs, TipoGrillaCuentaFinanciera.CuentaFinanciera).Select(x => new
			{
				Descripcion = x.descripcion,
				Saldo = x.saldo,
				H1 = x.es_header_1,
				H2 = x.es_header_2
			}).ToList();
			PdfPTable tablaInterna = HelperPdf.GenerarListadoDesdeLista(regsAux, _campos, _anchoCampoTabla, fuenteEtiqueta, true, true, null, true, BooleanDisplayFormat.SiNo, false, true);
			PdfPCell celdaConTabla = new(tablaInterna)
			{
				Border = Rectangle.NO_BORDER,
				Padding = 5,
			};
			tablaTitulo.AddCell(celdaConTabla);


			//Celda #2 (espacio en blanco)
			tablaTitulo.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });


			//Celda #3
			var regsAux2 = ObtenerGrillaCuentaFinanciera(regs, TipoGrillaCuentaFinanciera.CuentaBanco).Select(x => new
			{
				Descripcion = x.descripcion,
				Saldo = x.saldo,
				H1 = x.es_header_1,
				H2 = x.es_header_2
			}).ToList();
			PdfPTable tablaInterna2 = HelperPdf.GenerarListadoDesdeLista(regsAux2, _campos, _anchoCampoTabla, fuenteEtiqueta, true, true, null, true, BooleanDisplayFormat.SiNo, false, true);
			PdfPCell celdaConTabla2 = new(tablaInterna2)
			{
				Border = Rectangle.NO_BORDER,
				Padding = 5,
			};
			tablaTitulo.AddCell(celdaConTabla2);
			pdf.Add(tablaTitulo);
		}


		public static void CargarTablaVencimientoDeChequesEmitidos(Document pdf, List<FinancieroBcoVencChequeEmitidoListaDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			List<string> _campos = ["che_fecha_emi", "che_nro", "che_anombre", "che_estado_desc", "che_importe",];
			List<string> _titulosTabla = ["Fecha Emi.", "N° Cheque", "A Nombre", "Estado", "Importe",];
			float[] _anchosTitulosTabla = [15, 15, 30, 20, 20];
			PdfPTable tablaTitulo = GeneraTabla(1, [100f], 100, 10, 0);

			var reportePorFecha = regs.GroupBy(r => r.che_fecha.Date)
										.Select(g => new
										{
											Fecha = g.Key,
											Cheques = g.ToList(),
											TotalPendiente = g.Where(x => x.che_estado == 'C').Sum(x => x.che_importe),
											TotalEmitidos = g.Sum(x => x.che_importe)
										}).OrderBy(g => g.Fecha).ToList();

			foreach (var grupo in reportePorFecha)
			{
				// FILA 0 TITULO
				PdfPTable tablaSubTitulo = GeneraTabla(1, [100f], 100, 0, 10);
				PdfPCell celdaSubTitulo = new(new Phrase($"Vencimiento: {grupo.Fecha:dd/MM/yyyy}", HelperPdf.FontNormalPredeterminado(true)))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_LEFT,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					PaddingTop = 0f,
					PaddingBottom = 0f
				};
				tablaSubTitulo.AddCell(celdaSubTitulo);
				pdf.Add(tablaSubTitulo);

				// FILA 1 CABEZERA
				HelperPdf.GeneraCabeceraLista(pdf, _titulosTabla, _anchosTitulosTabla, HelperPdf.FontNormalPredeterminado(true), 0, 0);

				// FILA 2 CUERPO DE LA TABLA
				var regsAuxCheques = grupo.Cheques.Select(x => new
				{
					x.che_fecha_emi,
					x.che_nro,
					x.che_anombre,
					x.che_estado_desc,
					x.che_importe,
				}).ToList();
				HelperPdf.GenerarListadoDesdeLista(pdf, regsAuxCheques, _campos, _anchosTitulosTabla, fuenteEtiqueta);

				// FILA 3 TOTALES
				PdfPTable tablaSubTotal = GeneraTabla(2, [50f, 50f], 100, 0, 10);
				PdfPCell celdaSubTotal = new(new Phrase($"Total Pendiente: {grupo.TotalPendiente.ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					PaddingTop = 0f
				};
				tablaSubTotal.AddCell(celdaSubTotal);
				celdaSubTotal = new(new Phrase($"Total Emitidos: {grupo.TotalEmitidos.ToString("C", ForzarObtenerFormatoMonetario())}", HelperPdf.FontNormalPredeterminado(true)))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					PaddingTop = 0f
				};
				tablaSubTotal.AddCell(celdaSubTotal);
				pdf.Add(tablaSubTotal);

				// Espaciador entre grupos
				PdfPTable espaciador = new PdfPTable(1)
				{
					TotalWidth = 100f
				};
				espaciador.DefaultCell.Border = Rectangle.NO_BORDER;
				espaciador.DefaultCell.FixedHeight = 10f; // Altura del espacio
				espaciador.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
				espaciador.AddCell("");
				pdf.Add(espaciador);

			}
		}

		public static void CargarTablaChequesEmitidosPropios(Document pdf, List<FinancieroBcoVencChequeEmitidoListaDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			List<string> _campos = ["che_op_tra", "op_compte", "che_fecha_emi", "che_fecha", "che_nro", "che_anombre", "che_estado_desc", "che_importe",];
			List<string> _titulosTabla = ["Tipo", "Comprobante", "Fec. Emi.", "Fec. Vto.", "N° Cheque", "Cheque a Nombre de", "Estado", "Importe",];
			float[] _anchosTitulosTabla = [5, 12, 10, 10, 10, 35, 8, 10];

			var reportePorCtaf = regs
									.GroupBy(x => new { x.ctaf_id, x.ctaf_denominacion })
									.Select(g => new
									{
										g.Key.ctaf_id,
										g.Key.ctaf_denominacion,
										Cheques = g.ToList()
									})
									.ToList();

			foreach (var grupo in reportePorCtaf)
			{
				// FILA 0 TITULO
				PdfPTable tablaSubTitulo = GeneraTabla(1, [100f], 100, 0, 10);
				PdfPCell celdaSubTitulo = new(new Phrase($"{grupo.ctaf_denominacion} ({grupo.ctaf_id})", HelperPdf.FontNormalPredeterminado(true)))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_LEFT,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					MinimumHeight = 18f,
					BackgroundColor = BaseColor.LightGray
				};
				tablaSubTitulo.AddCell(celdaSubTitulo);
				pdf.Add(tablaSubTitulo);

				// FILA 1 CABEZERA
				HelperPdf.GeneraCabeceraLista(pdf, _titulosTabla, _anchosTitulosTabla, HelperPdf.FontNormalPredeterminado(true), 0, 0);

				// CUERPO
				HelperPdf.GenerarListadoDesdeLista(pdf, grupo.Cheques, _campos, _anchosTitulosTabla, fuenteEtiqueta);
				// Espaciador entre grupos
				PdfPTable espaciador = new PdfPTable(1)
				{
					TotalWidth = 100f
				};
				espaciador.DefaultCell.Border = Rectangle.NO_BORDER;
				espaciador.DefaultCell.FixedHeight = 10f; // Altura del espacio
				espaciador.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
				espaciador.AddCell("");
				pdf.Add(espaciador);
			}
		}

		public static void CargarTablaProyeccionFinanciera(Document pdf, List<ProyFinanDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			var semanas = regs
							.GroupBy(x => new { x.semana })
							.OrderBy(g => g.Key.semana);
			//var semanas = regs
			//				.GroupBy(x => new { x.semana, x.desde, x.hasta, x.leyendaSemana })
			//				.OrderBy(g => g.Key.desde);

			foreach (var semana in semanas)
			{
				// Título de la semana
				var fecDesde = regs.Where(x => x.semana == semana.Key.semana).Min(y => y.desde);
				var fecHasta = regs.Where(x => x.semana == semana.Key.semana).Max(y => y.desde);
				var tituloSemana = new Paragraph($"Semana del {fecDesde:dd/MM/yyyy} al {fecHasta:dd/MM/yyyy}", fuenteEtiqueta);
				tituloSemana.SpacingBefore = 10f;
				tituloSemana.SpacingAfter = 5f;
				pdf.Add(tituloSemana);

				// Tabla con columnas
				var table = new PdfPTable(10);
				table.WidthPercentage = 100;
				table.SetWidths(new float[] { 12f, 12f, 12f, 12f, 12f, 12f, 12f, 12f, 12f, 12f });

				// Encabezados
				string[] headers = {
					"",
					"Cheq. Emitidos + Transf. Bco.",
					"Cheq. Emitidos No Entregados",
					"Obligaciones a Pagar",
					"Proy. Otros Gastos",
					"Total Proy. Egresos",
					"Cheq. en Cartera",
					"Valores al Cobro",
					"Documentos a Cobrar",
					"Total Proy. Ingresos"
				};

				foreach (var h in headers)
				{
					if (h != "")
					{
						var cell = new PdfPCell(new Phrase(h, fuenteEtiqueta))
						{
							BackgroundColor = BaseColor.LightGray,
							HorizontalAlignment = Element.ALIGN_CENTER,
							Padding = 4
						};
						table.AddCell(cell);
					}
					else
					{
						var cell = new PdfPCell(new Phrase(h, fuenteEtiqueta))
						{
							BackgroundColor = BaseColor.White,
							HorizontalAlignment = Element.ALIGN_CENTER,
							Padding = 4
						};
						table.AddCell(cell);
					}
				}

				var total_cheque_emit_mas_trans_bco = 0.00M;
				var total_che_emi_nent = 0.00M;
				var total_apagar = 0.00M;
				var total_proy_gastos = 0.00M;
				var total_total_proy_egresos = 0.00M;
				var total_che_cartera = 0.00M;
				var total_che_depo = 0.00M;
				var total_valores_alcobro = 0.00M;
				var total_total_proy_ingresos = 0.00M;
				// Filas por día
				foreach (var item in regs.Where(x => x.semana == semana.Key.semana).ToList())
				{
					table.AddCell(new PdfPCell(new Phrase($"{item.desde:dd/MM/yyyy}", fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					table.AddCell(new PdfPCell(new Phrase(item.cheque_emit_mas_trans_bco.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					total_cheque_emit_mas_trans_bco += item.cheque_emit_mas_trans_bco;
					table.AddCell(new PdfPCell(new Phrase(item.che_emi_nent.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					total_che_emi_nent += item.che_emi_nent;
					table.AddCell(new PdfPCell(new Phrase(item.apagar.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					total_apagar += item.apagar;
					table.AddCell(new PdfPCell(new Phrase(item.proy_gastos.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					total_proy_gastos += item.proy_gastos;
					table.AddCell(new PdfPCell(new Phrase(item.total_proy_egresos.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
					total_total_proy_egresos += item.total_proy_egresos;
					table.AddCell(new PdfPCell(new Phrase(item.che_cartera.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					total_che_cartera += item.che_cartera;
					table.AddCell(new PdfPCell(new Phrase(item.che_depo.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					total_che_depo += item.che_depo;
					table.AddCell(new PdfPCell(new Phrase(item.valores_alcobro.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					total_valores_alcobro += item.valores_alcobro;
					table.AddCell(new PdfPCell(new Phrase(item.total_proy_ingresos.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
					total_total_proy_ingresos += item.total_proy_ingresos;
				}

				//Totales
				table.AddCell(new PdfPCell(new Phrase(string.Empty, fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				table.AddCell(new PdfPCell(new Phrase(total_cheque_emit_mas_trans_bco.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
				table.AddCell(new PdfPCell(new Phrase(total_che_emi_nent.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
				table.AddCell(new PdfPCell(new Phrase(total_apagar.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
				table.AddCell(new PdfPCell(new Phrase(total_proy_gastos.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
				table.AddCell(new PdfPCell(new Phrase(total_total_proy_egresos.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
				table.AddCell(new PdfPCell(new Phrase(total_che_cartera.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
				table.AddCell(new PdfPCell(new Phrase(total_che_depo.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
				table.AddCell(new PdfPCell(new Phrase(total_valores_alcobro.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
				table.AddCell(new PdfPCell(new Phrase(total_total_proy_ingresos.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });

				pdf.Add(table);
			}
		}

		public static void AgregarEncabezadoFinanciero(Document doc, ProyFinanDto datos, Font fontTitulo, Font fontTexto)
		{
			var fontTit = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
			var fontEtiqueta = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.NORMAL);
			var fontValor = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.BOLD, BaseColor.DarkGray);
			var linea = new LineSeparator(0.5f, 100f, BaseColor.Black, Element.ALIGN_CENTER, -2);

			// Línea superior
			doc.Add(new Chunk(linea));
			doc.Add(new Paragraph(" "));

			var tabla = new PdfPTable(2)
			{
				WidthPercentage = 100
			};
			tabla.SetWidths(new float[] { 50f, 50f });

			// Columna izquierda
			var columnaIzquierda = new PdfPTable(1)
			{
				WidthPercentage = 100
			};

			columnaIzquierda.AddCell(CeldaEtiquetaValor("Saldos Bancarios Disponibles (Según Extractos):", datos.saldo_bco, fontEtiqueta, fontValor));
			columnaIzquierda.AddCell(CeldaEtiquetaValor("Saldos Bancarios en Descubierto (Según Extractos):", datos.saldo_bco_rojo, fontEtiqueta, fontValor));

			// Columna derecha
			var columnaDerecha = new PdfPTable(1)
			{
				WidthPercentage = 100
			};

			columnaDerecha.AddCell(CeldaEtiquetaValor("Valores al Cobros no Acreditados:", datos.valores_alcobro_v, fontEtiqueta, fontValor));
			columnaDerecha.AddCell(CeldaEtiquetaValor("Documentos a Cobrar Vencidos hace 30 días:", datos.acobrar_mes_ant, fontEtiqueta, fontValor));
			columnaDerecha.AddCell(CeldaEtiquetaValor("Proyección de Ventas Diarias:", datos.proy_vtas, fontEtiqueta, fontValor));

			// Insertar las dos columnas en la tabla principal
			tabla.AddCell(new PdfPCell(columnaIzquierda) { Border = Rectangle.NO_BORDER });
			tabla.AddCell(new PdfPCell(columnaDerecha) { Border = Rectangle.NO_BORDER });

			doc.Add(tabla);

			// Línea inferior
			doc.Add(new Chunk(linea));
			doc.Add(new Paragraph(" "));
		}

		// Helper para construir celda con etiqueta y valor
		private static PdfPCell CeldaEtiquetaValor(string etiqueta, decimal valor, Font fontEtiqueta, Font fontValor)
		{
			var frase = new Phrase
			{
				new Chunk(etiqueta + " ", fontEtiqueta),
				new Chunk(valor.ToString("N2"), fontValor)
			};

			return new PdfPCell(frase)
			{
				Border = Rectangle.NO_BORDER,
				PaddingBottom = 6f, // separación vertical entre campos
				HorizontalAlignment = Element.ALIGN_RIGHT // Alinea todo el contenido a la derecha
			};
		}

		public static void CargarTablaSaldosEnCuenta(Document pdf, List<SaldoDeCuentaDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			var fontEtiqueta = FontFactory.GetFont(FontFactory.HELVETICA, 10, Font.BOLD);

			var tiposDeCuenta = regs
							  .GroupBy(x => new { x.tcf_id, x.tcf_desc })
							  .OrderBy(g => g.Key.tcf_id);

			foreach (var tipo in tiposDeCuenta)
			{
				// Título del grupo
				var titulo = new Paragraph(tipo.Key.tcf_desc, fontEtiqueta)
				{
					SpacingBefore = 10f,
					SpacingAfter = 5f
				};
				pdf.Add(titulo);

				// Tabla
				var tabla = new PdfPTable(4)
				{
					WidthPercentage = 100
				};
				tabla.SetWidths(new float[] { 15f, 45f, 20f, 20f });

				// Encabezados
				string[] headers = { "Código", "Medio de Pago / Cuenta Financiera", "Cuenta Cble", "Saldo" };
				foreach (var h in headers)
				{
					var celda = new PdfPCell(new Phrase(h, fuenteEtiqueta))
					{
						BackgroundColor = BaseColor.LightGray,
						HorizontalAlignment = Element.ALIGN_CENTER,
						Padding = 4
					};
					tabla.AddCell(celda);
				}

				decimal totalGrupo = 0;

				foreach (var item in tipo)
				{
					tabla.AddCell(new PdfPCell(new Phrase(item.ctaf_id, fuenteValor)) { HorizontalAlignment = Element.ALIGN_LEFT });
					tabla.AddCell(new PdfPCell(new Phrase(item.ctaf_denominacion, fuenteValor)) { HorizontalAlignment = Element.ALIGN_LEFT });
					tabla.AddCell(new PdfPCell(new Phrase(item.ccb_id, fuenteValor)) { HorizontalAlignment = Element.ALIGN_LEFT });
					tabla.AddCell(new PdfPCell(new Phrase(item.cf_saldo.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });

					totalGrupo += item.cf_saldo;
				}

				// Fila de total
				var celdaTotalLabel = new PdfPCell(new Phrase("Total " + tipo.Key.tcf_desc + ":", fontEtiqueta))
				{
					Colspan = 3,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					PaddingTop = 6f,
					PaddingBottom = 6f,
					BackgroundColor = new BaseColor(240, 240, 240)
				};
				var celdaTotalValor = new PdfPCell(new Phrase(totalGrupo.ToString("N2"), fuenteValor))
				{
					HorizontalAlignment = Element.ALIGN_RIGHT,
					PaddingTop = 6f,
					PaddingBottom = 6f,
					BackgroundColor = new BaseColor(240, 240, 240)
				};

				tabla.AddCell(celdaTotalLabel);
				tabla.AddCell(celdaTotalValor);

				pdf.Add(tabla);

			}
		}

		public static void CargarTablaFlujoDeIngreso(Document pdf, List<FlujoDeIngresoDto> lista, Font fuenteEtiqueta, Font fuenteValor)
		{
			var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
			var fontHeaderRevision = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Red);
			var fontHeaderCartera = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Black);
			var fontHeaderAlCobro = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Green);
			var fontHeaderAcreditado = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.Blue);

			var fontValorRevision = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.Red);
			var fontValorCartera = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.Black);
			var fontValorAlCobro = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.Green);
			var fontValorAcreditado = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.Blue);

			var fontTotal = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);

			pdf.Add(new Paragraph(" "));

			var tabla = new PdfPTable(6) { WidthPercentage = 100 };
			tabla.SetWidths([30f, 15f, 15f, 15f, 15f, 15f]);

			// Fila 1: encabezado agrupado
			tabla.AddCell(new PdfPCell(new Phrase("Concepto", fuenteEtiqueta)) { Rowspan = 2, BackgroundColor = BaseColor.LightGray, HorizontalAlignment = Element.ALIGN_CENTER });
			tabla.AddCell(new PdfPCell(new Phrase("Ingresos", fuenteEtiqueta)) { Rowspan = 2, BackgroundColor = BaseColor.LightGray, HorizontalAlignment = Element.ALIGN_CENTER });
			tabla.AddCell(new PdfPCell(new Phrase("Estado de Valores", fuenteEtiqueta)) { Colspan = 4, BackgroundColor = BaseColor.LightGray, HorizontalAlignment = Element.ALIGN_CENTER });

			// Fila 2: subencabezados
			tabla.AddCell(new PdfPCell(new Phrase("En Revisión", fontHeaderRevision)) { BackgroundColor = BaseColor.LightGray, HorizontalAlignment = Element.ALIGN_CENTER });
			tabla.AddCell(new PdfPCell(new Phrase("Cartera", fontHeaderCartera)) { BackgroundColor = BaseColor.LightGray, HorizontalAlignment = Element.ALIGN_CENTER });
			tabla.AddCell(new PdfPCell(new Phrase("Al Cobro", fontHeaderAlCobro)) { BackgroundColor = BaseColor.LightGray, HorizontalAlignment = Element.ALIGN_CENTER });
			tabla.AddCell(new PdfPCell(new Phrase("Acreditado", fontHeaderAcreditado)) { BackgroundColor = BaseColor.LightGray, HorizontalAlignment = Element.ALIGN_CENTER });


			// Totales
			decimal totalIngreso = 0, totalRevision = 0, totalCartera = 0, totalAlCobro = 0, totalAcreditado = 0;

			foreach (var item in lista)
			{
				tabla.AddCell(new PdfPCell(new Phrase(item.medio_de_pago, fuenteValor)) { HorizontalAlignment = Element.ALIGN_LEFT });
				tabla.AddCell(new PdfPCell(new Phrase(item.ingreso.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(item.revision.ToString("N2"), fontValorRevision)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(item.cartera.ToString("N2"), fontValorCartera)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(item.alcobro.ToString("N2"), fontValorAlCobro)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(item.acreditado.ToString("N2"), fontValorAcreditado)) { HorizontalAlignment = Element.ALIGN_RIGHT });

				totalIngreso += item.ingreso;
				totalRevision += item.revision;
				totalCartera += item.cartera;
				totalAlCobro += item.alcobro;
				totalAcreditado += item.acreditado;
			}

			// Fila de totales
			var fondoTotal = new BaseColor(230, 230, 230);
			tabla.AddCell(new PdfPCell(new Phrase("Total", fontTotal)) { BackgroundColor = fondoTotal, HorizontalAlignment = Element.ALIGN_RIGHT });
			tabla.AddCell(new PdfPCell(new Phrase(totalIngreso.ToString("N2"), fontTotal)) { BackgroundColor = fondoTotal, HorizontalAlignment = Element.ALIGN_RIGHT });
			tabla.AddCell(new PdfPCell(new Phrase(totalRevision.ToString("N2"), fontTotal)) { BackgroundColor = fondoTotal, HorizontalAlignment = Element.ALIGN_RIGHT });
			tabla.AddCell(new PdfPCell(new Phrase(totalCartera.ToString("N2"), fontTotal)) { BackgroundColor = fondoTotal, HorizontalAlignment = Element.ALIGN_RIGHT });
			tabla.AddCell(new PdfPCell(new Phrase(totalAlCobro.ToString("N2"), fontTotal)) { BackgroundColor = fondoTotal, HorizontalAlignment = Element.ALIGN_RIGHT });
			tabla.AddCell(new PdfPCell(new Phrase(totalAcreditado.ToString("N2"), fontTotal)) { BackgroundColor = fondoTotal, HorizontalAlignment = Element.ALIGN_RIGHT });

			pdf.Add(tabla);
		}

		public static void CargarTablaFlujoDeEgresos(Document pdf, List<ProyeccionDeGastoDto> lista, Font fuenteEtiqueta, Font fuenteValor)
		{
			// Ordenar y calcular acumulado incremental
			decimal acumulado = 0.00M;
			var listaOrdenada = lista.OrderBy(x => x.fecha).ThenBy(x => x.orden).ToList();
			foreach (var item in listaOrdenada)
			{
				acumulado += item.importe;
				item.acumulado = acumulado;
			}

			// Crear tabla con 4 columnas
			var tabla = new PdfPTable(4) { WidthPercentage = 100 };
			tabla.SetWidths(new float[] { 15f, 45f, 20f, 20f });

			// Encabezados
			string[] headers = { "FECHA", "CONCEPTO", "IMPORTE", "ACUMULADO" };
			foreach (var h in headers)
			{
				var celda = new PdfPCell(new Phrase(h, fuenteEtiqueta))
				{
					BackgroundColor = BaseColor.LightGray,
					HorizontalAlignment = Element.ALIGN_CENTER,
					Padding = 5
				};
				tabla.AddCell(celda);
			}

			// Filas de datos
			foreach (var item in listaOrdenada)
			{
				tabla.AddCell(new PdfPCell(new Phrase(item.fecha.ToString("dd/MM/yyyy"), fuenteValor)) { Padding = 4, HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(item.concepto, fuenteValor)) { Padding = 4 });
				tabla.AddCell(new PdfPCell(new Phrase(item.importe.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
				tabla.AddCell(new PdfPCell(new Phrase(item.acumulado.ToString("N2"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
			}

			// Agregar tabla al documento
			pdf.Add(tabla);
		}


		public static void CargarTablaLibroBancoDetalle(Document pdf, List<FinancieroBcoLibroDto> regs, string fHasta, DateTime fHastaDate, Font fuenteEtiqueta, Font fuenteNormal, Font fuenteValor)
		{
			BaseColor azul = new(0x00, 0x7B, 0xFF);   // #007BFF
			BaseColor rojo = new(0xB2, 0x22, 0x22);   // #B22222
			Font fuenteAzul = new(fuenteNormal.BaseFont, fuenteNormal.Size, fuenteNormal.Style, azul);
			Font fuenteRoja = new(fuenteNormal.BaseFont, fuenteNormal.Size, fuenteNormal.Style, rojo);

			var item = regs.First();
			var saldo_bco = item.saldo_bco > 0 ? item.saldo_bco.ToString("C", ForzarObtenerFormatoMonetario()).Trim() : $"({(-1 * item.saldo_bco).ToString("C", ForzarObtenerFormatoMonetario()).Trim()})";
			var saldo_bco_che = item.saldo_bco_che > 0 ? item.saldo_bco_che.ToString("C", ForzarObtenerFormatoMonetario()).Trim() : $"({(-1 * item.saldo_bco_che).ToString("C", ForzarObtenerFormatoMonetario()).Trim()})";
			var saldo_pendiente = item.saldo_pendiente > 0 ? item.saldo_pendiente.ToString("C", ForzarObtenerFormatoMonetario()).Trim() : $"({(-1 * item.saldo_pendiente).ToString("C", ForzarObtenerFormatoMonetario()).Trim()})";
			var conciliado_m_ant = item.conciliado_m_ant > 0 ? item.conciliado_m_ant.ToString("C", ForzarObtenerFormatoMonetario()).Trim() : $"({(-1 * item.conciliado_m_ant).ToString("C", ForzarObtenerFormatoMonetario()).Trim()})";
			var conciliado_m_sig = item.conciliado_m_sig > 0 ? item.conciliado_m_sig.ToString("C", ForzarObtenerFormatoMonetario()).Trim() : $"({(-1 * item.conciliado_m_sig).ToString("C", ForzarObtenerFormatoMonetario()).Trim()})";
			var conciliado_m_pos = item.conciliado_m_pos != null ? item.conciliado_m_pos.Value > 0 ? item.conciliado_m_pos.Value.ToString("C", ForzarObtenerFormatoMonetario()).Trim() : "0" : $"({(0).ToString("C", ForzarObtenerFormatoMonetario()).Trim()})";


			PdfPTable tablaResumen2 = new(3)
			{
				WidthPercentage = 100
			};
			tablaResumen2.SetWidths(new float[] { 50f, 20f, 30f });
			AgregarFilaResumen(tablaResumen2, $"Saldo Libro Banco al {fHastaDate:dd/MM/yyyy}", saldo_bco, fuenteNormal);
			AgregarFilaResumen(tablaResumen2, $"Saldo Libro Banco al {fHastaDate:dd/MM/yyyy} (Con Cheques Entregados)", saldo_bco_che, fuenteNormal);
			AgregarFilaResumen(tablaResumen2, $"Cheques Pendientes de Entrega al {fHastaDate:dd/MM/yyyy}", saldo_pendiente, fuenteNormal);

			AgregarFilaResumen(tablaResumen2, $"Saldo Conciliado en Lib. Bco. Mes Anterior al {fHastaDate:MMyyyy}", conciliado_m_ant, fuenteNormal);
			AgregarFilaResumen(tablaResumen2, $"Saldo Conciliado en Lib. Bco. Mes Siguiente al {fHastaDate:MMyyyy}", conciliado_m_sig, fuenteNormal);
			AgregarFilaResumen(tablaResumen2, $"Saldo Conciliado en Lib. Bco. Mes Siguiente Posterior al {fHastaDate:MMyyyy}", conciliado_m_pos, fuenteNormal);
			pdf.Add(tablaResumen2);


			// Espaciador entre grupos
			PdfPTable espaciador = new PdfPTable(1)
			{
				TotalWidth = 100f
			};
			espaciador.DefaultCell.Border = Rectangle.NO_BORDER;
			espaciador.DefaultCell.FixedHeight = 10f; // Altura del espacio
			espaciador.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
			espaciador.AddCell("");
			pdf.Add(espaciador);



			// ===============================
			//  Primera grilla tipo '0'
			//  Formato igual al HTML: 3 columnas
			// ===============================

			PdfPTable tablaTipo0 = new PdfPTable(3)
			{
				WidthPercentage = 90
			};

			// Ajustá los anchos según tu HTML
			tablaTipo0.SetWidths(new float[] { 70f, 15f, 15f });
			tablaTipo0.HorizontalAlignment = Element.ALIGN_LEFT;
			foreach (var reg in regs.Where(x => x.tipo == '0'))
			{
				string concepto = reg.concepto ?? "";
				int idx = concepto.LastIndexOf(" ");

				string textoFijo = "Saldo Extracto al";
				string fecha = "";

				if (idx > -1)
					fecha = concepto.Substring(idx + 1);

				string importeFormateado = reg.importe < 0
					? $"({Math.Abs(reg.importe):N2})"
					: $"{reg.importe:N2}";

				// Columna 1: texto fijo
				tablaTipo0.AddCell(new PdfPCell(new Phrase(textoFijo, fuenteNormal))
				{
					HorizontalAlignment = Element.ALIGN_RIGHT
				});

				// Columna 2: fecha
				tablaTipo0.AddCell(new PdfPCell(new Phrase(fecha, fuenteNormal))
				{
					HorizontalAlignment = Element.ALIGN_CENTER
				});

				// Columna 3: importe
				tablaTipo0.AddCell(new PdfPCell(new Phrase(importeFormateado, fuenteNormal))
				{
					HorizontalAlignment = Element.ALIGN_RIGHT
				});
			}

			pdf.Add(tablaTipo0);


			// Segunda grilla, grupo tipo '1' (movimientos extracto no Conciliados)
			PdfPTable tablaSubTitulo = GeneraTabla(1, [100f], 100, 0, 10);
			PdfPCell celdaSubTitulo = new(new Phrase($"Movimiento Extracto - no Conciliados"))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				PaddingBottom = 0f
			};
			tablaSubTitulo.AddCell(celdaSubTitulo);
			pdf.Add(tablaSubTitulo);

			// Encabezados
			PdfPTable tabla = HelperPdf.GeneraTabla(3, [70f, 15f, 15f], 90, 0, 0);
			tabla.HorizontalAlignment = Element.ALIGN_LEFT;
			string[] headers = { "Concepto", "Fecha Vto.", "Importe" };
			foreach (var header in headers)
			{
				PdfPCell celda = new(new Phrase(header, fuenteValor))
				{
					BackgroundColor = BaseColor.LightGray,
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					Padding = 5
				};
				tabla.AddCell(celda);
			}
			pdf.Add(tabla);

			tabla = HelperPdf.GeneraTabla(3, [70f, 15f, 15f], 90, 0, 0);
			tabla.HorizontalAlignment = Element.ALIGN_LEFT;
			foreach (var reg in regs.Where(x => x.tipo == '1'))
			{
				tabla.AddCell(new PdfPCell(new Phrase(reg.concepto, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_LEFT });
				tabla.AddCell(new PdfPCell(new Phrase(reg.fecha_vto.Value.ToString("dd/MM/yyyy"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(reg.importe.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
			}
			pdf.Add(tabla);

			// Tercera grilla, grupo tipo '2' (movimientos libro banco no Conciliados)
			PdfPTable tablaSubTitulo2 = GeneraTabla(1, [100f], 100, 10, 10);
			PdfPCell celdaSubTitulo2 = new(new Phrase($"Movimientos Libro Banco con vto al {fHastaDate:dd/MM/yyyy} - no Conciliados"))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				PaddingBottom = 0f
			};
			tablaSubTitulo2.AddCell(celdaSubTitulo2);
			pdf.Add(tablaSubTitulo2);

			tabla = HelperPdf.GeneraTabla(5, [50, 13, 14, 13, 10], 100, 0, 0);
			string[] headers2 = { "Concepto", "Fecha Reg.", "Fecha Vto.", "Importe", "Estado" };
			foreach (var header in headers2)
			{
				PdfPCell celda = new(new Phrase(header, fuenteValor))
				{
					BackgroundColor = BaseColor.LightGray,
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					Padding = 5
				};
				tabla.AddCell(celda);
			}
			pdf.Add(tabla);

			tabla = HelperPdf.GeneraTabla(5, [50, 13, 14, 13, 10], 100, 0, 10);
			foreach (var reg in regs.Where(x => x.tipo == '2'))
			{
				tabla.AddCell(new PdfPCell(new Phrase(reg.concepto, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_LEFT });
				tabla.AddCell(new PdfPCell(new Phrase(reg.fecha.Value.ToString("dd/MM/yyyy"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(reg.fecha_vto.Value.ToString("dd/MM/yyyy"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(reg.importe.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				string estado = (reg.strEstado) ? "SI" : "NO";
				tabla.AddCell(new PdfPCell(new Phrase(estado, fuenteEtiqueta))
				{
					HorizontalAlignment = Element.ALIGN_CENTER
				});
			}
			pdf.Add(tabla);
		}


		private static void AgregarFilaResumen(PdfPTable pdf, string etiqueta, string valor, Font fuente)
		{
			PdfPCell celdaEtiqueta = new(new Phrase(etiqueta, fuente))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				PaddingTop = 2f,
				PaddingBottom = 2f
			};

			// Celda vacía en el medio (sin puntos)
			PdfPCell celdaSeparador = new(new Phrase("", fuente))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingTop = 2f,
				PaddingBottom = 2f
			};

			PdfPCell celdaValor = new(new Phrase(valor, fuente))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				PaddingTop = 2f,
				PaddingBottom = 2f
			};

			pdf.AddCell(celdaEtiqueta);
			pdf.AddCell(celdaSeparador);
			pdf.AddCell(celdaValor);
		}


		public static void CargarAnticiposDeEmpleados(Document pdf, List<AnticipoDetalleDto> lista, Font fuenteEtiqueta, Font fuenteValor, EmpresaGeco _empresaGeco, ReporteSolicitudDto solicitud)
		{
			var logo = HelperPdf.CargaLogo(solicitud.LogoPath, 20, pdf.PageSize.Height - 10, 20);
			var linea = new LineSeparator(0.5f, 100f, BaseColor.Black, Element.ALIGN_CENTER, -2);
			for (int i = 0; i < lista.Count; i += 2)
			{
				if (i > 0) pdf.NewPage();

				solicitud.Titulo = $"Vale anticipo N° : {lista[i].an_compte}";
				solicitud.SubTitulo = $"Fecha : {lista[i].an_fecha.ToString("dd/MM/yyyy")}";

				pdf.Add(GeneraCabeceraPDF2(solicitud, HelperPdf.FontChicoPredeterminado(), HelperPdf.FontTituloPredeterminado(), logo, _empresaGeco));

				AgregarAnticipo(pdf, lista[i], fuenteEtiqueta, fuenteValor);

				pdf.Add(new Paragraph(" ", fuenteValor));

				if (i + 1 < lista.Count)
				{
					pdf.Add(new Paragraph(" "));
					pdf.Add(new Paragraph(" "));
					pdf.Add(new Paragraph(" "));
					pdf.Add(new Paragraph(" "));

					solicitud.Titulo = $"Vale anticipo N° : {lista[i + 1].an_compte}";
					solicitud.SubTitulo = $"Fecha : {lista[i + 1].an_fecha.ToString("dd/MM/yyyy")}";

					pdf.Add(GeneraCabeceraPDF2(solicitud, HelperPdf.FontChicoPredeterminado(), HelperPdf.FontTituloPredeterminado(), logo, _empresaGeco));

					AgregarAnticipo(pdf, lista[i + 1], fuenteEtiqueta, fuenteValor);
				}
			}

		}

		private static PdfPTable GeneraCabeceraPDF2(ReporteSolicitudDto solicitud, Font chico, Font titulo, Image? logo, EmpresaGeco _empresaGeco)
		{
			PdfPTable contenedor = new PdfPTable(1)
			{
				WidthPercentage = 100
			};

			PdfPTable tabla = HelperPdf.GeneraTabla(3, [10f, 30f, 50f], 100, 10, 20);

			// Columna 1: Logo
			PdfPCell celdaLogo;
			if (logo == null)
			{
				celdaLogo = new PdfPCell(new Paragraph("CA", titulo));
			}
			else
			{
				celdaLogo = HelperPdf.GeneraCelda(logo, false);
			}
			tabla.AddCell(celdaLogo);

			// Columna 2: Datos apilados y título
			PdfPTable subTabla = new(1)
			{
				WidthPercentage = 100
			};

			// Datos apilados
			subTabla.AddCell(HelperPdf.CrearCeldaTexto(_empresaGeco.Nombre, chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"{_empresaGeco.Responsabilidad} Ini.Act:{_empresaGeco.InicioActividades.ToShortDateString()}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"CUIT: {_empresaGeco.CUIT} IB:{_empresaGeco.IngresosBrutos}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"{_empresaGeco.Direccion}, {_empresaGeco.Localidad}", chico));

			PdfPCell celdaSubTabla = new(subTabla)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaSubTabla);

			// Columna 3: Título del informe
			PdfPCell celdaTitulo = new PdfPCell(new Phrase(solicitud.Titulo, titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 2f
			};
			PdfPCell celdaSubTitulo = new();
			if (!string.IsNullOrEmpty(solicitud.SubTitulo))
			{
				// Título del informe
				celdaSubTitulo = new PdfPCell(new Phrase(solicitud.SubTitulo, titulo))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					PaddingTop = 10f
				};
			}
			PdfPTable subTablaC3 = new(1);
			subTablaC3.WidthPercentage = 100;
			//subTablaC3.AddCell(HelperPdf.CrearCeldaTexto(string.Empty, chico));
			subTablaC3.SpacingBefore = 0f;
			subTablaC3.SpacingAfter = 0f;
			subTablaC3.AddCell(celdaTitulo);
			if (!string.IsNullOrEmpty(solicitud.SubTitulo))
			{
				subTablaC3.AddCell(HelperPdf.CrearCeldaTexto(string.Empty, chico));
				subTablaC3.AddCell(celdaSubTitulo);
			}

			PdfPCell celdaSubTablaC3 = new PdfPCell(subTablaC3)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaSubTablaC3);

			PdfPCell celdaContenedora = new PdfPCell(tabla)
			{
				Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
				BorderWidthTop = 0.8f,
				BorderWidthBottom = 0.8f,
				BorderColorTop = BaseColor.Black,
				BorderColorBottom = BaseColor.Black,
				PaddingTop = 1f,
				PaddingBottom = 1f
			};
			contenedor.AddCell(celdaContenedora);

			return contenedor;
		}

		private static void AgregarAnticipo(Document doc, AnticipoDetalleDto dto, Font fuenteEtiqueta, Font fuenteValor)
		{
			var tabla = new PdfPTable(1) { WidthPercentage = 100 };
			var fuenteSubtitulo = HelperPdf.FontSubtituloPredeterminado();
			var fuenteTitulo = HelperPdf.FontTituloPredeterminado();

			tabla.AddCell(Celda("Vale Anticipo / Descuento Personal", fuenteTitulo, Element.ALIGN_CENTER));
			tabla.AddCell(Celda(" ", fuenteValor));
			tabla.AddCell(Celda($"          Concepto: {dto.ant_desc.ToUpper()}         {dto.an_concepto.ToUpper()}", fuenteTitulo));
			tabla.AddCell(Celda($"          Beneficiario: {dto.cta_denominacion.ToUpper()}                Legajo N°: {dto.cta_emp_legajo}", fuenteTitulo));
			tabla.AddCell(Celda($"          Vale por la cantidad de $ {dto.cv_importe:N2}.- (Pesos — {ConvertirImporteEnTexto(dto.cv_importe)})", fuenteTitulo));
			tabla.AddCell(Celda(" ", fuenteValor));
			tabla.AddCell(Celda(" ", fuenteValor));
			tabla.AddCell(Celda($"          {dto.cv_concepto}", fuenteTitulo));
			tabla.AddCell(Celda(" ", fuenteValor));
			tabla.AddCell(Celda(" ", fuenteValor));
			tabla.AddCell(Celda(" ", fuenteValor));
			// 🖊️ Bloque de firma con línea y texto centrado
			float anchoFirma = 120f; // mm
			PdfPTable tablaFirma = new PdfPTable(1)
			{
				TotalWidth = anchoFirma,
				LockedWidth = true,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				SpacingBefore = 10f,
				SpacingAfter = 5f
			};

			// Línea para firma
			var celdaLinea = new PdfPCell(new Phrase(" ", fuenteValor))
			{
				Border = Rectangle.BOTTOM_BORDER,
				BorderWidthBottom = 0.8f,
				FixedHeight = 18f,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 2f
			};
			tablaFirma.AddCell(celdaLinea);

			// Texto debajo de la línea
			var celdaTexto = new PdfPCell(new Phrase("Recibí Conforme", fuenteValor))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingTop = 2f
			};
			tablaFirma.AddCell(celdaTexto);

			// Agregar tabla de firma como celda dentro de tabla principal
			var celdaContenedoraFirma = new PdfPCell(tablaFirma)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT
			};
			tabla.AddCell(celdaContenedoraFirma);

			// Pie de impresión
			tabla.AddCell(Celda($"Fecha de impresión: {DateTime.Now:dd/MM/yyyy HH:mm}", fuenteValor, Element.ALIGN_LEFT));

			doc.Add(tabla);
		}

		private static PdfPCell Celda(string texto, Font fuente, int alineacion = Element.ALIGN_LEFT)
		{
			return new PdfPCell(new Phrase(texto, fuente))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = alineacion,
				PaddingBottom = 4f
			};
		}

		private static string ConvertirImporteEnTexto(decimal importe)
		{
			int parteEntera = (int)Math.Floor(importe);
			int parteDecimal = (int)((importe - parteEntera) * 100);
			return $"{NumeroEnLetras(parteEntera)} con {parteDecimal:00}/100";
		}

		public static string NumeroEnLetras(int numero)
		{
			if (numero == 0) return "cero";

			string[] unidades = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve" };
			string[] especiales = { "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve" };
			string[] decenas = { "", "", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
			string[] centenas = { "", "ciento", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos" };

			StringBuilder resultado = new StringBuilder();

			if (numero == 100) return "cien";

			int millones = numero / 1000000;
			int miles = (numero % 1000000) / 1000;
			int resto = numero % 1000;

			if (millones > 0)
			{
				if (millones == 1)
					resultado.Append("un millón ");
				else
					resultado.Append($"{NumeroEnLetras(millones)} millones ");
			}

			if (miles > 0)
			{
				if (miles == 1)
					resultado.Append("mil ");
				else
					resultado.Append($"{NumeroEnLetras(miles)} mil ");
			}

			if (resto > 0)
			{
				int centena = resto / 100;
				int decena = (resto % 100) / 10;
				int unidad = resto % 10;

				if (centena > 0)
					resultado.Append($"{centenas[centena]} ");

				int dosDigitos = resto % 100;

				if (dosDigitos < 10)
					resultado.Append(unidades[unidad]);
				else if (dosDigitos < 20)
					resultado.Append(especiales[dosDigitos - 10]);
				else
				{
					resultado.Append(decenas[decena]);
					if (unidad > 0)
						resultado.Append($" y {unidades[unidad]}");
				}
			}

			return resultado.ToString().Trim();
		}

		public static void CargarAnticiposDetalle(Document pdf, List<AnticipoDetalleDto> lista, Font fuenteEtiqueta, Font fuenteValor)
		{
			if (lista == null || !lista.Any()) return;

			Font fuenteTotal = new Font(fuenteValor.BaseFont, fuenteValor.Size + 1, Font.BOLD);

			PdfPTable tabla = new(7)
			{
				WidthPercentage = 100
			};
			tabla.SetWidths([5f, 15f, 10f, 30f, 10f, 15f, 15f]);

			// Encabezados
			string[] headers = { "Item", "Código", "Legajo", "Razón Social", "Cuota", "Fecha Vto.", "Anti./Dto." };
			foreach (var header in headers)
			{
				PdfPCell celda = new(new Phrase(header, fuenteValor))
				{
					BackgroundColor = BaseColor.LightGray,
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					Padding = 5
				};
				tabla.AddCell(celda);
			}

			// Filas
			int item = 1;
			decimal total = 0;
			foreach (var anticipo in lista)
			{
				tabla.AddCell(new PdfPCell(new Phrase(anticipo.an_item.ToString(), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(anticipo.cta_id, fuenteEtiqueta)));
				tabla.AddCell(new PdfPCell(new Phrase(anticipo.cta_emp_legajo.ToString(), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(anticipo.cta_denominacion, fuenteEtiqueta)));
				tabla.AddCell(new PdfPCell(new Phrase($"{anticipo.cm_compte_cuota}/{anticipo.cm_compte_cuota_tot}", fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(anticipo.an_fecha.ToString("dd/MM/yyyy"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase(anticipo.cv_importe.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });

				total += anticipo.cv_importe;
				item++;
			}

			// Celdas vacías para centrar visualmente el totalizador
			for (int i = 0; i <= 4; i++)
			{
				PdfPCell celdaVacia = new PdfPCell(new Phrase(""))
				{
					Border = Rectangle.NO_BORDER
				};
				tabla.AddCell(celdaVacia);
			}

			// Celda "Total:"
			PdfPCell celdaTotalLabel = new PdfPCell(new Phrase("Total:", fuenteValor))
			{
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				Border = Rectangle.TOP_BORDER,
				PaddingTop = 6f,
				PaddingBottom = 4f,
				BackgroundColor = new BaseColor(230, 230, 230) // Gris suave
			};
			tabla.AddCell(celdaTotalLabel);

			// Celda con el valor
			PdfPCell celdaTotalValor = new PdfPCell(new Phrase(total.ToString("N2"), fuenteValor))
			{
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				Border = Rectangle.BOX,
				BorderWidth = 0.5f,
				BorderColor = BaseColor.Black, // Color del borde
				PaddingTop = 6f,
				PaddingBottom = 4f,
				BackgroundColor = new BaseColor(230, 230, 230)
			};
			tabla.AddCell(celdaTotalValor);

			pdf.Add(tabla);

		}

		private static string GenerarSeparadorPunteado(string etiqueta, string valor, int totalLongitud = 90)
		{
			int longitudEtiqueta = etiqueta.Length;
			int longitudValor = valor.Length;
			int puntosNecesarios = Math.Max(3, totalLongitud - longitudEtiqueta - longitudValor);

			return new string('.', puntosNecesarios);
		}

		private static PdfPCell GeneraLineaResumen(string etiqueta, string valor, Font fuente, BaseColor colorFondo)
		{
			string puntos = new('.', 60); // Ajustable según ancho
			string linea = $"{etiqueta} {puntos} {valor}";

			return new PdfPCell(new Phrase(linea, fuente))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 2f,
				PaddingBottom = 2f,
				BackgroundColor = colorFondo
			};
		}

		private static PdfPCell GeneraLineaResumen(string etiqueta, string valor, Font fuente, int cantPuntos)
		{
			string linea = $"{etiqueta} {new string('.', cantPuntos)} {valor}";

			return new PdfPCell(new Phrase(linea, fuente))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				PaddingTop = 2f,
				PaddingBottom = 2f
			};
		}

		private static PdfPCell GeneraCeldaTexto(string texto, string valor)
		{
			return new PdfPCell(new Phrase($"{texto} {valor}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				BackgroundColor = BaseColor.LightGray
			};
		}

		private static PdfPCell AplicarEstiloCelda(PdfPCell celda, string claseCss, Font fuente)
		{
			switch (claseCss)
			{
				case "destacado-header-1":
					celda.BackgroundColor = new BaseColor(211, 208, 71);
					celda.Phrase.Font = new Font(fuente.BaseFont, fuente.Size, Font.BOLD);
					break;
				case "destacado-header-2":
					celda.BackgroundColor = new BaseColor(96, 165, 243);
					celda.Phrase.Font = new Font(fuente.BaseFont, fuente.Size, Font.BOLD);
					break;
				case "no-destacado":
					celda.BackgroundColor = BaseColor.White;
					break;
			}
			return celda;
		}

		public static void CargarLiquidacionDeHaberesDeEmpleados(Document pdf, List<LiqEmpleadoDetalleParaReporteDto> lista, Font fuenteEtiqueta, Font fuenteValor)
		{
			var agrupadoPorEmpleado = lista.GroupBy(x => x.cta_id);
			bool esPrimeraPagina = true;
			BaseColor colorEncabezado = new BaseColor(230, 230, 230); // Gris claro

			foreach (var grupo in agrupadoPorEmpleado)
			{
				var primer = grupo.First();

				// Nueva hoja por empleado
				if (!esPrimeraPagina)
					pdf.NewPage();
				else
					esPrimeraPagina = false;

				// Tabla de anticipos/documentos
				var tabla = new PdfPTable(5) { WidthPercentage = 100 };
				tabla.SetWidths(new float[] { 30, 10, 15, 20, 25 });

				// Espacio visual entre encabezado y títulos
				var celdaEspaciadora = new PdfPCell(new Phrase(" "))
				{
					Colspan = 5,
					Border = Rectangle.NO_BORDER,
					FixedHeight = 10f
				};
				tabla.AddCell(celdaEspaciadora);

				// Encabezado del empleado
				var celdaEncabezado = new PdfPCell(new Phrase($"Empleado: {primer.cta_denominacion} (Legajo: {primer.cta_emp_legajo} Cód. Cuenta: {primer.cta_id})", fuenteValor))
				{
					Colspan = 5,
					HorizontalAlignment = Element.ALIGN_CENTER,
					PaddingBottom = 10f,
					BackgroundColor = colorEncabezado
				};
				tabla.AddCell(celdaEncabezado);

				// Encabezados
				AgregarCelda(tabla, "Concepto", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Cuota", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Fecha Vto.", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Débito Ori.", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Dto. Sueldo", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);

				decimal totalDto = 0;

				foreach (var item in grupo)
				{
					AgregarCelda(tabla, item.concepto, fuenteValor, Element.ALIGN_LEFT);
					AgregarCelda(tabla, item.cm_compte_cuota.ToString(), fuenteValor, Element.ALIGN_CENTER);
					AgregarCelda(tabla, item.cv_fecha_vto.ToString("dd/MM/yyyy"), fuenteValor, Element.ALIGN_CENTER);
					AgregarCelda(tabla, FormatearDecimal(item.cv_importe_ori), fuenteValor, Element.ALIGN_RIGHT);
					AgregarCelda(tabla, FormatearDecimal(item.dto), fuenteValor, Element.ALIGN_RIGHT);

					totalDto += item.dto;
				}

				// Total
				var celdaTotal = new PdfPCell(new Phrase("Total Descuento:", HelperPdf.FontNormalPredeterminado())) { Colspan = 4, HorizontalAlignment = Element.ALIGN_RIGHT };
				tabla.AddCell(celdaTotal);
				tabla.AddCell(new PdfPCell(new Phrase(FormatearDecimal(totalDto), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = colorEncabezado });

				pdf.Add(tabla);
			}
		}

		private static void AgregarCelda(PdfPTable tabla, string texto, Font fuente, int Align = 0, bool esEncabezado = false, BaseColor? fondo = null)
		{
			var celda = new PdfPCell(new Phrase(texto, fuente))
			{
				HorizontalAlignment = Align,
				Padding = 4f
			};
			if (esEncabezado && fondo != null)
				celda.BackgroundColor = fondo;

			tabla.AddCell(celda);
		}

		private static string FormatearDecimal(decimal valor)
		{
			return valor.ToString("#,##0.00", CultureInfo.InvariantCulture); // coma miles, punto decimal
		}

		public static void CargarVencimientoPorTipoDeComprobante(Document pdf, List<VencimientoListaDto> lista, Font fuenteEtiqueta, Font fuenteValor)
		{
			var agrupadoPorEmpleado = lista.GroupBy(x => x.cta_id);
			//bool esPrimeraPagina = true;
			BaseColor colorEncabezado = new(230, 230, 230); // Gris claro

			foreach (var grupo in agrupadoPorEmpleado)
			{
				var primer = grupo.First();

				// Tabla de anticipos/documentos
				var tabla = new PdfPTable(6) { WidthPercentage = 100 };
				tabla.SetWidths(new float[] { 50, 7, 7, 7, 10, 19 });

				// Espacio visual entre encabezado y títulos
				var celdaEspaciadora = new PdfPCell(new Phrase(" "))
				{
					Colspan = 6,
					Border = Rectangle.NO_BORDER,
					FixedHeight = 10f
				};
				tabla.AddCell(celdaEspaciadora);

				// Encabezado del empleado
				var celdaEncabezado = new PdfPCell(new Phrase($"({primer.cta_id}) {primer.cta_denominacion}", fuenteValor))
				{
					Colspan = 6,
					HorizontalAlignment = Element.ALIGN_LEFT,
					PaddingBottom = 10f,
					BackgroundColor = colorEncabezado
				};
				tabla.AddCell(celdaEncabezado);

				// Encabezados
				AgregarCelda(tabla, "Descripción", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Est", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Cuota", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Días Atr.", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Vencimiento", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Importe", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);

				decimal totalImporte = 0;

				foreach (var item in grupo)
				{
					AgregarCelda(tabla, item.comprobante, fuenteEtiqueta, Element.ALIGN_LEFT);
					AgregarCelda(tabla, item.cv_estado, fuenteEtiqueta, Element.ALIGN_CENTER);
					AgregarCelda(tabla, item.cm_compte_cuota.ToString(), fuenteEtiqueta, Element.ALIGN_CENTER);
					AgregarCelda(tabla, item.atraso.ToString(), fuenteEtiqueta, Element.ALIGN_CENTER);
					AgregarCelda(tabla, item.cv_fecha_vto.ToString("dd/MM/yy"), fuenteEtiqueta, Element.ALIGN_CENTER);
					AgregarCelda(tabla, FormatearDecimal(item.cv_importe), fuenteEtiqueta, Element.ALIGN_RIGHT);

					totalImporte += item.cv_importe;
				}

				// Total
				var celdaTotal = new PdfPCell(new Phrase($"Total de '{primer.cta_denominacion}':", fuenteValor)) { Colspan = 5, HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = colorEncabezado };
				tabla.AddCell(celdaTotal);
				tabla.AddCell(new PdfPCell(new Phrase(FormatearDecimal(totalImporte), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = colorEncabezado });

				pdf.Add(tabla);
			}
		}

		public static void CargarCertificadosNoRetencionNoPercepcion(Document pdf, List<CertificadoListaDto> lista, Font fuenteEtiqueta, Font fuenteValor)
		{
			var agrupadoPorGrupo = lista.GroupBy(x => x.grupo);
			BaseColor colorEncabezado = new(230, 230, 230); // Gris claro

			foreach (var grupo in agrupadoPorGrupo)
			{
				var primer = grupo.First();

				// Tabla de certificados
				var tabla = new PdfPTable(8) { WidthPercentage = 100 };
				tabla.SetWidths(new float[] { 10, 20, 5, 10, 25, 10, 10, 10 });

				// Espacio visual entre encabezado y títulos
				var celdaEspaciadora = new PdfPCell(new Phrase(" "))
				{
					Colspan = 8,
					Border = Rectangle.NO_BORDER,
					FixedHeight = 10f
				};
				tabla.AddCell(celdaEspaciadora);

				// Encabezado del empleado
				var celdaEncabezado = new PdfPCell(new Phrase($"{primer.grupo_des}", HelperPdf.FontSubtituloPredeterminado()))
				{
					Colspan = 8,
					HorizontalAlignment = Element.ALIGN_CENTER,
					PaddingBottom = 10f,
					BackgroundColor = colorEncabezado
				};
				tabla.AddCell(celdaEncabezado);

				// Encabezados
				AgregarCelda(tabla, "Cuenta", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Denominación", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "T. Doc.", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Documento", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Domicilio", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Teléfono", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Celular", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);
				AgregarCelda(tabla, "Vigencia", fuenteValor, Element.ALIGN_CENTER, true, colorEncabezado);

				foreach (var item in grupo)
				{
					AgregarCelda(tabla, item.cta_id, fuenteEtiqueta, Element.ALIGN_CENTER);
					AgregarCelda(tabla, item.cta_denominacion, fuenteEtiqueta, Element.ALIGN_LEFT);
					AgregarCelda(tabla, item.tdoc_desc, fuenteEtiqueta, Element.ALIGN_CENTER);
					AgregarCelda(tabla, item.cta_documento, fuenteEtiqueta, Element.ALIGN_CENTER);
					AgregarCelda(tabla, item.cta_domicilio, fuenteEtiqueta, Element.ALIGN_LEFT);
					AgregarCelda(tabla, item.cta_te, fuenteEtiqueta, Element.ALIGN_LEFT);
					AgregarCelda(tabla, item.cta_celu, fuenteEtiqueta, Element.ALIGN_LEFT);
					AgregarCelda(tabla, item.cert_vto == null ? "" : item.cert_vto.Value.ToString("dd/MM/yyyy"), fuenteEtiqueta, Element.ALIGN_CENTER);
				}
				pdf.Add(tabla);
			}
		}

		/// <summary>
		/// Genera e inserta en el <paramref name="pdf"/> un listado de productos con su stock,
		/// agrupado según el valor de <paramref name="agrupador"/>. Si <paramref name="lista"/> es nulo o vacío
		/// la función retorna sin modificar el documento.
		/// </summary>
		/// <param name="pdf">Documento destino donde se agregará la(s) tabla(s). No se cierra ni se abre el documento aquí.</param>
		/// <param name="lista">Colección de <see cref="ProductoStkDto"/> que se representarán en el informe.</param>
		/// <param name="agrupador">
		/// Determina el criterio de agrupamiento:
		/// 1 = Sector (usa <c>sec_id</c> / <c>sec_desc</c>),
		/// 2 = Grupo de Rubros (usa <c>rubg_id</c> / <c>rubg_desc</c>),
		/// 3 = Rubro (usa <c>rub_id</c> / <c>rub_desc</c>),
		/// 4 = Proveedor (usa <c>cta_id</c> / <c>cta_denominacion</c>).
		/// Si no coincide ninguno, se genera un único grupo (sin agrupamiento).
		/// </param>
		/// <param name="fuenteEtiqueta">Fuente usada para los encabezados y títulos de grupo.</param>
		/// <param name="fuenteValor">Fuente usada para los valores/filas de detalle.</param>
		/// <remarks>
		/// - Formatea fechas con el patrón <c>dd/MM/yyyy</c>.
		/// - Las tablas usan anchos fijos y están preparadas para A4; ajustar si se necesita otro tamaño de página.
		/// - No realiza operaciones de I/O ni cierra el <paramref name="pdf"/>; quien llama debe encargarse de ello.
		/// - Si necesita otros criterios de agrupamiento, extienda la lógica de selección con nuevos casos en <paramref name="agrupador"/>.
		/// </remarks>
		/// <example>
		/// // Ejemplo de uso:
		/// var lista = servicio.ObtenerProductosStock(...);
		/// HelperPdf.CargarProductosParaRptDeStk(doc, lista, 4, HelperPdf.FontNormalPredeterminado(true), HelperPdf.FontNormalPredeterminado());
		/// </example>
		public static void CargarProductosParaRptDeStk(Document pdf, List<ProductoStkDto> lista, int agrupador, Font fuenteEtiqueta, Font fuenteValor)
		{
			if (lista == null || !lista.Any())
				return;
			Func<ProductoStkDto, string> agrupadorKeySelector = null;
			Func<ProductoStkDto, string> agrupadorDescSelector = null;
			string tituloAgrupador = null;

			switch (agrupador)
			{
				case 1:
					agrupadorKeySelector = x => x.sec_id;
					agrupadorDescSelector = x => x.sec_desc;
					tituloAgrupador = "Sector";
					break;
				case 2:
					agrupadorKeySelector = x => x.rubg_id;
					agrupadorDescSelector = x => x.rubg_desc;
					tituloAgrupador = "Grupo de Rubros";
					break;
				case 3:
					agrupadorKeySelector = x => x.rub_id;
					agrupadorDescSelector = x => x.rub_desc;
					tituloAgrupador = "Rubro";
					break;
				case 4:
					agrupadorKeySelector = x => x.cta_id;
					agrupadorDescSelector = x => x.cta_denominacion;
					tituloAgrupador = "Proveedor";
					break;
			}

			IEnumerable<IGrouping<string, ProductoStkDto>> grupos;

			if (agrupadorKeySelector != null)
			{
				grupos = lista.GroupBy(agrupadorKeySelector);
			}
			else
			{
				// Sin agrupamiento: todo en un solo grupo ficticio
				grupos = new List<IGrouping<string, ProductoStkDto>> { new AgrupacionSinGrupo(lista) };
			}

			foreach (var grupo in grupos)
			{
				PdfPTable tabla = new(11)
				{
					WidthPercentage = 100
				};
				tabla.SetWidths([6f, 10f, 30f, 5f, 7f, 7f, 7f, 7f, 6f, 7f, 8f]);

				// 👇 Si hay agrupador, agregamos fila de título como parte de la tabla
				if (agrupadorKeySelector != null)
				{
					string descripcionGrupo = agrupadorDescSelector(grupo.First());
					PdfPCell celdaGrupo = new(new Phrase($"{tituloAgrupador}: {descripcionGrupo}", fuenteValor))
					{
						Colspan = 11, // ocupa todas las columnas
						HorizontalAlignment = Element.ALIGN_CENTER,
						BackgroundColor = BaseColor.Yellow, // opcional, para destacar
						PaddingTop = 5f,
						PaddingBottom = 5f
					};

					tabla.AddCell(celdaGrupo);
				}

				// Encabezados
				string[] encabezados = { "Código", "Cód. Barra", "Descripción", "Cod. Prov.", "Ult. Mov.", "Ult. Recep.", "Ref. P.", "Unidad Pres.", "Conteo", "Stock", "Diferencia" };
				foreach (var encabezado in encabezados)
				{
					PdfPCell celda = new(new Phrase(encabezado, fuenteValor))
					{
						BackgroundColor = BaseColor.LightGray,
						HorizontalAlignment = Element.ALIGN_CENTER
					};
					tabla.AddCell(celda);
				}
				// 👇 ESTA LÍNEA ES LA CLAVE
				tabla.HeaderRows = 2;

				foreach (var producto in grupo)
				{
					tabla.AddCell(new PdfPCell(new Phrase(producto.p_id, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(producto.p_id_barrado, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(producto.p_desc, fuenteEtiqueta)));
					tabla.AddCell(new PdfPCell(new Phrase(producto.p_id_prov, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tabla.AddCell(new PdfPCell(new Phrase(producto.stk_ult_mov?.ToString("dd/MM/yyyy") ?? "", fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(producto.rp_fecha?.ToString("dd/MM/yyyy") ?? "", fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(producto.rp_dias.ToString(), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tabla.AddCell(new PdfPCell(new Phrase(producto.p_unidad_pres.ToString(), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tabla.AddCell(new PdfPCell(new Phrase(string.Empty, fuenteEtiqueta)));
					if (producto.PermiteDecimales)
						tabla.AddCell(new PdfPCell(new Phrase(producto.stk.ToString("0.00"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					else
						tabla.AddCell(new PdfPCell(new Phrase(producto.stk.ToString("0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tabla.AddCell(new PdfPCell(new Phrase(string.Empty, fuenteEtiqueta)));
				}

				pdf.Add(tabla);
			}

		}

		public static void CargarProductosParaRptDeStkValor(Document pdf, List<ProductoStkDto> lista, int agrupador, Font fuenteEtiqueta, Font fuenteValor)
		{
			if (lista == null || !lista.Any())
				return;

			decimal totalValorizado = 0;

			switch (agrupador)
			{
				case 0:
					PdfPTable tabla = new(6)
					{
						WidthPercentage = 100
					};
					tabla.SetWidths([10f, 50f, 10f, 10f, 10f, 10f]);

					// Encabezados
					string[] encabezados = { "Código", "Producto", "Stock", "Precio Costo", "Valorización", "%/Total" };
					foreach (var encabezado in encabezados)
					{
						PdfPCell celda = new(new Phrase(encabezado, fuenteValor))
						{
							BackgroundColor = BaseColor.LightGray,
							HorizontalAlignment = Element.ALIGN_CENTER
						};
						tabla.AddCell(celda);
					}

					foreach (var producto in lista)
					{
						var valStr = string.Empty;
						var val = producto.stk_val ?? 0;
						if (val < 0)
							valStr = $"({Math.Abs(val).ToString("0.000")})";
						else
							valStr = val.ToString("0.000");
						tabla.AddCell(new PdfPCell(new Phrase(producto.p_id, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
						tabla.AddCell(new PdfPCell(new Phrase(producto.p_desc, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_LEFT });
						if (producto.PermiteDecimales)
							tabla.AddCell(new PdfPCell(new Phrase(producto.stk.ToString("0.00"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
						else
							tabla.AddCell(new PdfPCell(new Phrase(producto.stk.ToString("0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });

						tabla.AddCell(new PdfPCell(new Phrase(GridHelper.FormatearPrecio(producto.p_pcosto, TipoPrecio.Costo), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
						tabla.AddCell(new PdfPCell(new Phrase(valStr, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
						tabla.AddCell(new PdfPCell(new Phrase(producto.porc_rub?.ToString("N6"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					}
					totalValorizado = lista.Sum(x => x.stk_val ?? 0);
					// Fila de total
					PdfPCell celdaTotalTitulo = new PdfPCell(new Phrase("Total Valorizado", fuenteEtiqueta))
					{
						Colspan = 4, // ocupa las primeras 5 columnas
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LightGray
					};
					tabla.AddCell(celdaTotalTitulo);

					// Celda con el total
					PdfPCell celdaTotalValor = new PdfPCell(new Phrase(totalValorizado.ToString("0.000"), fuenteEtiqueta))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LightGray
					};
					tabla.AddCell(celdaTotalValor);

					// celda vacía para la columna "Costo"
					tabla.AddCell(new PdfPCell(new Phrase("", fuenteEtiqueta))
					{
						BackgroundColor = BaseColor.LightGray
					});

					pdf.Add(tabla);
					break;
				case 1:
					PdfPTable tablaS = new(4)
					{
						WidthPercentage = 100
					};
					tablaS.SetWidths([10f, 60f, 20f, 10f]);

					// Encabezados
					string[] encabezadosS = { "Código", "Sector", "Costo", "%/Total" };
					foreach (var encabezado in encabezadosS)
					{
						PdfPCell celda = new(new Phrase(encabezado, fuenteValor))
						{
							BackgroundColor = BaseColor.LightGray,
							HorizontalAlignment = Element.ALIGN_CENTER
						};
						tablaS.AddCell(celda);
					}

					foreach (var producto in lista)
					{
						var valStr = string.Empty;
						var val = producto.stk_val ?? 0;
						if (val < 0)
							valStr = $"({Math.Abs(val).ToString("0.000")})";
						else
							valStr = val.ToString("0.000");
						tablaS.AddCell(new PdfPCell(new Phrase(producto.sec_id, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
						tablaS.AddCell(new PdfPCell(new Phrase(producto.sec_desc, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_LEFT });
						tablaS.AddCell(new PdfPCell(new Phrase(valStr, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
						tablaS.AddCell(new PdfPCell(new Phrase(producto.porc_total?.ToString("0.000"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					}
					totalValorizado = lista.Sum(x => x.stk_val ?? 0);
					PdfPCell celdaTotalTituloS = new PdfPCell(new Phrase("Total Valorizado", fuenteEtiqueta))
					{
						Colspan = 2,
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LightGray
					};
					tablaS.AddCell(celdaTotalTituloS);

					// celda con el total
					PdfPCell celdaTotalValorS = new PdfPCell(new Phrase(totalValorizado.ToString("0.000"), fuenteEtiqueta))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LightGray
					};
					tablaS.AddCell(celdaTotalValorS);

					// celda vacía para la columna "Costo"
					tablaS.AddCell(new PdfPCell(new Phrase("", fuenteEtiqueta))
					{
						BackgroundColor = BaseColor.LightGray
					});

					pdf.Add(tablaS);
					break;
				case 2:
					PdfPTable tablaRG = new(4)
					{
						WidthPercentage = 100
					};
					tablaRG.SetWidths([10f, 60f, 20f, 10f]);

					// Encabezados
					string[] encabezadosRG = { "Código", "Rubro Grupo", "Costo", "%/Total" };
					foreach (var encabezado in encabezadosRG)
					{
						PdfPCell celda = new(new Phrase(encabezado, fuenteValor))
						{
							BackgroundColor = BaseColor.LightGray,
							HorizontalAlignment = Element.ALIGN_CENTER
						};
						tablaRG.AddCell(celda);
					}

					foreach (var producto in lista)
					{
						var valStr = string.Empty;
						var val = producto.stk_val ?? 0;
						if (val < 0)
							valStr = $"({Math.Abs(val).ToString("0.000")})";
						else
							valStr = val.ToString("0.000");
						tablaRG.AddCell(new PdfPCell(new Phrase(producto.rubg_id, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
						tablaRG.AddCell(new PdfPCell(new Phrase(producto.rubg_desc, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_LEFT });
						tablaRG.AddCell(new PdfPCell(new Phrase(valStr, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
						tablaRG.AddCell(new PdfPCell(new Phrase(producto.porc_total?.ToString("0.000"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					}
					totalValorizado = lista.Sum(x => x.stk_val ?? 0);
					// Fila de total
					PdfPCell celdaTotalTituloRG = new PdfPCell(new Phrase("Total Valorizado", fuenteEtiqueta))
					{
						Colspan = 2, // ocupa las primeras 5 columnas
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LightGray
					};
					tablaRG.AddCell(celdaTotalTituloRG);

					// Celda con el total
					PdfPCell celdaTotalValorRG = new PdfPCell(new Phrase(totalValorizado.ToString("0.000"), fuenteEtiqueta))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LightGray
					};
					tablaRG.AddCell(celdaTotalValorRG);

					// celda vacía para la columna "Costo"
					tablaRG.AddCell(new PdfPCell(new Phrase("", fuenteEtiqueta))
					{
						BackgroundColor = BaseColor.LightGray
					});

					pdf.Add(tablaRG);
					break;
				case 3:
					PdfPTable tablaR = new(4)
					{
						WidthPercentage = 100
					};
					tablaR.SetWidths([10f, 60f, 20f, 10f]);

					// Encabezados
					string[] encabezadosR = { "Código", "Rubro", "Costo", "%/Total" };
					foreach (var encabezado in encabezadosR)
					{
						PdfPCell celda = new(new Phrase(encabezado, fuenteValor))
						{
							BackgroundColor = BaseColor.LightGray,
							HorizontalAlignment = Element.ALIGN_CENTER
						};
						tablaR.AddCell(celda);
					}

					foreach (var producto in lista)
					{
						var valStr = string.Empty;
						var val = producto.stk_val ?? 0;
						if (val < 0)
							valStr = $"({Math.Abs(val).ToString("0.000")})";
						else
							valStr = val.ToString("0.000");
						tablaR.AddCell(new PdfPCell(new Phrase(producto.rub_id, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
						tablaR.AddCell(new PdfPCell(new Phrase(producto.rub_desc, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_LEFT });
						tablaR.AddCell(new PdfPCell(new Phrase(valStr, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
						tablaR.AddCell(new PdfPCell(new Phrase(producto.porc_total?.ToString("0.000"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					}
					totalValorizado = lista.Sum(x => x.stk_val ?? 0);
					// Fila de total
					PdfPCell celdaTotalTituloR = new PdfPCell(new Phrase("Total Valorizado", fuenteEtiqueta))
					{
						Colspan = 2, // ocupa las primeras 5 columnas
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LightGray
					};
					tablaR.AddCell(celdaTotalTituloR);

					// Celda con el total
					PdfPCell celdaTotalValorR = new PdfPCell(new Phrase(totalValorizado.ToString("0.000"), fuenteEtiqueta))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LightGray
					};
					tablaR.AddCell(celdaTotalValorR);

					// celda vacía para la columna "Costo"
					tablaR.AddCell(new PdfPCell(new Phrase("", fuenteEtiqueta))
					{
						BackgroundColor = BaseColor.LightGray
					});

					pdf.Add(tablaR);
					break;
				case 4:
					PdfPTable tablaP = new(4)
					{
						WidthPercentage = 100
					};
					tablaP.SetWidths([10f, 60f, 20f, 10f]);

					// Encabezados
					string[] encabezadosP = { "Código", "Proveedor", "Costo", "%/Total" };
					foreach (var encabezado in encabezadosP)
					{
						PdfPCell celda = new(new Phrase(encabezado, fuenteValor))
						{
							BackgroundColor = BaseColor.LightGray,
							HorizontalAlignment = Element.ALIGN_CENTER
						};
						tablaP.AddCell(celda);
					}

					foreach (var producto in lista)
					{
						var valStr = string.Empty;
						var val = producto.stk_val ?? 0;
						if (val < 0)
							valStr = $"({Math.Abs(val).ToString("0.000")})";
						else
							valStr = val.ToString("0.000");
						tablaP.AddCell(new PdfPCell(new Phrase(producto.rub_id, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
						tablaP.AddCell(new PdfPCell(new Phrase(producto.rub_desc, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_LEFT });
						tablaP.AddCell(new PdfPCell(new Phrase(valStr, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
						tablaP.AddCell(new PdfPCell(new Phrase(producto.porc_total?.ToString("0.000"), fuenteValor)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					}
					totalValorizado = lista.Sum(x => x.stk_val ?? 0);
					// Fila de total
					PdfPCell celdaTotalTituloP = new PdfPCell(new Phrase("Total Valorizado", fuenteEtiqueta))
					{
						Colspan = 2, // ocupa las primeras 5 columnas
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LightGray
					};
					tablaP.AddCell(celdaTotalTituloP);

					// Celda con el total
					PdfPCell celdaTotalValorP = new PdfPCell(new Phrase(totalValorizado.ToString("0.000"), fuenteEtiqueta))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LightGray
					};
					tablaP.AddCell(celdaTotalValorP);

					// celda vacía para la columna "Costo"
					tablaP.AddCell(new PdfPCell(new Phrase("", fuenteEtiqueta))
					{
						BackgroundColor = BaseColor.LightGray
					});

					pdf.Add(tablaP);
					break;
			}

		}

		public static void CargarProductosParaRptDeStkCompensado(Document pdf, List<ProductoStkCompensadoDto> lista, Font fuenteEtiqueta, Font fuenteValor)
		{
			if (lista == null || !lista.Any())
				return;
			Func<ProductoStkCompensadoDto, string> agrupadorKeySelector = null;
			Func<ProductoStkCompensadoDto, string> agrupadorDescSelector = null;
			string tituloAgrupador = null;

			agrupadorKeySelector = x => x.rub_id;
			agrupadorDescSelector = x => x.rub_desc;
			tituloAgrupador = "Rubro";

			IEnumerable<IGrouping<string, ProductoStkCompensadoDto>> grupos;

			if (agrupadorKeySelector != null)
			{
				grupos = lista.GroupBy(agrupadorKeySelector);
			}
			else
			{
				// Sin agrupamiento: todo en un solo grupo ficticio
				grupos = new List<IGrouping<string, ProductoStkCompensadoDto>> { new AgrupacionSinGrupoComp(lista) };
			}

			foreach (var grupo in grupos)
			{
				PdfPTable tabla = new(7)
				{
					WidthPercentage = 100
				};
				tabla.SetWidths([10f, 10f, 40f, 10f, 10f, 10f, 10f]);

				// 👇 Si hay agrupador, agregamos fila de título como parte de la tabla
				if (agrupadorKeySelector != null)
				{
					string descripcionGrupo = agrupadorDescSelector(grupo.First());
					PdfPCell celdaGrupo = new(new Phrase($"{tituloAgrupador}: {descripcionGrupo}", fuenteValor))
					{
						Colspan = 11, // ocupa todas las columnas
						HorizontalAlignment = Element.ALIGN_CENTER,
						BackgroundColor = BaseColor.Yellow, // opcional, para destacar
						PaddingTop = 5f,
						PaddingBottom = 5f
					};

					tabla.AddCell(celdaGrupo);
				}

				// Encabezados
				string[] encabezados = { "Código", "Barrado", "Descripción", "Stock +", "Stock -", "Diferencia", "Compensa" };
				foreach (var encabezado in encabezados)
				{
					PdfPCell celda = new(new Phrase(encabezado, fuenteValor))
					{
						BackgroundColor = BaseColor.LightGray,
						HorizontalAlignment = Element.ALIGN_CENTER
					};
					tabla.AddCell(celda);
				}
				// 👇 ESTA LÍNEA ES LA CLAVE
				tabla.HeaderRows = 2;

				foreach (var producto in grupo)
				{
					tabla.AddCell(new PdfPCell(new Phrase(producto.p_id, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(producto.p_id_barrado, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(producto.p_desc, fuenteEtiqueta)));
					if (producto.PermiteDecimales)
					{
						tabla.AddCell(new PdfPCell(new Phrase(producto.stk_positivo.ToString("0.00"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
						tabla.AddCell(new PdfPCell(new Phrase(producto.st_negativo.ToString("0.00"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
						tabla.AddCell(new PdfPCell(new Phrase(producto.stk_diferencia.ToString("0.00"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
						tabla.AddCell(new PdfPCell(new Phrase(producto.stk_diferencia.ToString("0.00"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					}
					else
					{
						tabla.AddCell(new PdfPCell(new Phrase(producto.stk_positivo.ToString("0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
						tabla.AddCell(new PdfPCell(new Phrase(producto.st_negativo.ToString("0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
						tabla.AddCell(new PdfPCell(new Phrase(producto.stk_diferencia.ToString("0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
						tabla.AddCell(new PdfPCell(new Phrase(producto.stk_diferencia.ToString("0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					}
				}

				pdf.Add(tabla);
			}

		}

		public static void CargarRepoInvValPorSec(Document pdf, List<InvRepoValPorSecDto> lista, Font fuenteEtiqueta, Font fuenteValor)
		{
			if (lista == null || lista.Count == 0)
			{
				Paragraph sinDatos = new Paragraph("No se encontraron datos", fuenteEtiqueta);
				sinDatos.Alignment = Element.ALIGN_CENTER;
				pdf.Add(sinDatos);
				return;
			}

			BaseColor amarilloPastel = new BaseColor(255, 245, 200);

			// Tabla con 9 columnas
			PdfPTable tabla = new PdfPTable(9);
			tabla.WidthPercentage = 100;

			// Anchos proporcionales
			tabla.SetWidths(new float[] { 15, 15, 15, 10, 10, 10, 10, 10, 10 });

			tabla.HeaderRows = 2;

			// ============================
			// CABECERA NIVEL 1
			// ============================
			PdfPCell c1 = new(new Phrase("Sectores", fuenteValor))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c1);

			PdfPCell c2 = new(new Phrase("Prod. Rubro", fuenteValor))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c2);

			PdfPCell c3 = new(new Phrase("Prod. con Conteo", fuenteValor))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c3);

			PdfPCell c4 = new(new Phrase("Cantidades", fuenteValor))
			{
				Colspan = 3,
				HorizontalAlignment = Element.ALIGN_CENTER,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c4);

			PdfPCell c5 = new(new Phrase("Valorización", fuenteValor))
			{
				Colspan = 3,
				HorizontalAlignment = Element.ALIGN_CENTER,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c5);

			// ============================
			// CABECERA NIVEL 2
			// ============================
			tabla.AddCell(new PdfPCell(new Phrase("Stk", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });
			tabla.AddCell(new PdfPCell(new Phrase("Conteo", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });
			tabla.AddCell(new PdfPCell(new Phrase("Dif.", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });

			tabla.AddCell(new PdfPCell(new Phrase("Stk", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });
			tabla.AddCell(new PdfPCell(new Phrase("Conteo", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });
			tabla.AddCell(new PdfPCell(new Phrase("Dif.", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });

			// ============================
			// FILAS DE DATOS
			// ============================
			decimal totalProdRubro = 0;
			decimal totalProdConConteo = 0;
			decimal totalStkCant = 0;
			decimal totalPlaniCant = 0;
			decimal totalStkVal = 0;
			decimal totalPlaniVal = 0;

			foreach (var item in lista)
			{
				decimal difCant = item.stk_cant - item.plani_cant;
				decimal difVal = item.stk_val - item.plani_val;

				totalProdRubro += item.prod_sec;
				totalProdConConteo += item.prod_sec_cont;
				totalStkCant += item.stk_cant;
				totalPlaniCant += item.plani_cant;
				totalStkVal += item.stk_val;
				totalPlaniVal += item.plani_val;

				tabla.AddCell(new PdfPCell(new Phrase(item.sec_desc, fuenteEtiqueta)));
				tabla.AddCell(new PdfPCell(new Phrase(item.prod_sec.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(item.prod_sec_cont.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });

				tabla.AddCell(new PdfPCell(new Phrase(item.stk_cant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(item.plani_cant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(difCant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });

				tabla.AddCell(new PdfPCell(new Phrase(item.stk_val.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(item.plani_val.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(difVal.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
			}

			// ============================
			// FILA DE TOTALES
			// ============================
			decimal totalDifCant = totalStkCant - totalPlaniCant;
			decimal totalDifVal = totalStkVal - totalPlaniVal;

			PdfPCell totalCell = new PdfPCell(new Phrase("TOTAL", fuenteValor));
			totalCell.Colspan = 1;
			totalCell.HorizontalAlignment = Element.ALIGN_RIGHT;
			totalCell.BackgroundColor = BaseColor.LightGray;
			tabla.AddCell(totalCell);

			tabla.AddCell(new PdfPCell(new Phrase(totalProdRubro.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalProdConConteo.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });

			tabla.AddCell(new PdfPCell(new Phrase(totalStkCant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalPlaniCant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalDifCant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });

			tabla.AddCell(new PdfPCell(new Phrase(totalStkVal.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalPlaniVal.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalDifVal.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });

			// Agregar tabla al PDF
			pdf.Add(tabla);

		}

		public static void CargarRepoInvValPorRub(Document pdf, List<InvRepoValPorRubDto> lista, Font fuenteEtiqueta, Font fuenteValor)
		{
			if (lista == null || lista.Count == 0)
			{
				Paragraph sinDatos = new Paragraph("No se encontraron datos", fuenteEtiqueta);
				sinDatos.Alignment = Element.ALIGN_CENTER;
				pdf.Add(sinDatos);
				return;
			}

			BaseColor amarilloPastel = new BaseColor(255, 245, 200);

			// Tabla con 9 columnas
			PdfPTable tabla = new PdfPTable(9);
			tabla.WidthPercentage = 100;

			// Anchos proporcionales
			tabla.SetWidths(new float[] { 15, 15, 15, 10, 10, 10, 10, 10, 10 });

			tabla.HeaderRows = 2;

			// ============================
			// CABECERA NIVEL 1
			// ============================
			PdfPCell c1 = new(new Phrase("Rubros", fuenteValor))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c1);

			PdfPCell c2 = new(new Phrase("Prod. Rubro", fuenteValor))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c2);

			PdfPCell c3 = new(new Phrase("Prod. con Conteo", fuenteValor))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c3);

			PdfPCell c4 = new(new Phrase("Cantidades", fuenteValor))
			{
				Colspan = 3,
				HorizontalAlignment = Element.ALIGN_CENTER,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c4);

			PdfPCell c5 = new(new Phrase("Valorización", fuenteValor))
			{
				Colspan = 3,
				HorizontalAlignment = Element.ALIGN_CENTER,
				BackgroundColor = amarilloPastel
			};
			tabla.AddCell(c5);

			// ============================
			// CABECERA NIVEL 2
			// ============================
			tabla.AddCell(new PdfPCell(new Phrase("Stk", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });
			tabla.AddCell(new PdfPCell(new Phrase("Conteo", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });
			tabla.AddCell(new PdfPCell(new Phrase("Dif.", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });

			tabla.AddCell(new PdfPCell(new Phrase("Stk", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });
			tabla.AddCell(new PdfPCell(new Phrase("Conteo", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });
			tabla.AddCell(new PdfPCell(new Phrase("Dif.", fuenteValor)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = amarilloPastel });

			// ============================
			// FILAS DE DATOS
			// ============================
			decimal totalProdRubro = 0;
			decimal totalProdConConteo = 0;
			decimal totalStkCant = 0;
			decimal totalPlaniCant = 0;
			decimal totalStkVal = 0;
			decimal totalPlaniVal = 0;

			foreach (var item in lista)
			{
				decimal difCant = item.stk_cant - item.plani_cant;
				decimal difVal = item.stk_val - item.plani_val;

				totalProdRubro += item.prod_rub;
				totalProdConConteo += item.prod_rub_cont;
				totalStkCant += item.stk_cant;
				totalPlaniCant += item.plani_cant;
				totalStkVal += item.stk_val;
				totalPlaniVal += item.plani_val;

				tabla.AddCell(new PdfPCell(new Phrase(item.rub_desc, fuenteEtiqueta)));
				tabla.AddCell(new PdfPCell(new Phrase(item.prod_rub.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(item.prod_rub_cont.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });

				tabla.AddCell(new PdfPCell(new Phrase(item.stk_cant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(item.plani_cant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(difCant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });

				tabla.AddCell(new PdfPCell(new Phrase(item.stk_val.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(item.plani_val.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				tabla.AddCell(new PdfPCell(new Phrase(difVal.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
			}

			// ============================
			// FILA DE TOTALES
			// ============================
			decimal totalDifCant = totalStkCant - totalPlaniCant;
			decimal totalDifVal = totalStkVal - totalPlaniVal;

			PdfPCell totalCell = new PdfPCell(new Phrase("TOTAL", fuenteValor));
			totalCell.Colspan = 1;
			totalCell.HorizontalAlignment = Element.ALIGN_RIGHT;
			totalCell.BackgroundColor = BaseColor.LightGray;
			tabla.AddCell(totalCell);

			tabla.AddCell(new PdfPCell(new Phrase(totalProdRubro.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalProdConConteo.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });

			tabla.AddCell(new PdfPCell(new Phrase(totalStkCant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalPlaniCant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalDifCant.ToString("N0"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });

			tabla.AddCell(new PdfPCell(new Phrase(totalStkVal.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalPlaniVal.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });
			tabla.AddCell(new PdfPCell(new Phrase(totalDifVal.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = BaseColor.LightGray });

			// Agregar tabla al PDF
			pdf.Add(tabla);

		}

		public static void CargarRepoConteoPorUsu(Document pdf, List<InvRepoConteosPorUsuDto> lista, Font fuenteEtiqueta, Font fuenteValor)
		{
			if (lista == null || lista.Count == 0)
			{
				pdf.Add(new Paragraph("No se encontraron datos", fuenteEtiqueta));
				return;
			}

			// Agrupar por planilla
			var grupos = lista
				.GroupBy(x => new { x.carga_nro, x.carga_des })
				.OrderBy(g => g.Key.carga_nro);

			BaseColor amarilloPastel = new BaseColor(255, 245, 200);
			Font fuenteTitulo = new Font(fuenteEtiqueta.BaseFont, 14, Font.BOLD);
			bool primera = true;

			foreach (var grupo in grupos)
			{
				// Nueva hoja por cada planilla excepto la primera
				if (!primera)
					pdf.NewPage();
				primera = false;

				// ============================================================
				// TABLA COMPLETA (TÍTULO + CABECERA + DATOS)
				// ============================================================
				PdfPTable tabla = new PdfPTable(7);
				tabla.WidthPercentage = 100;
				tabla.SetWidths(new float[] { 8, 12, 35, 10, 10, 12, 13 });

				// Título + cabecera deben repetirse
				tabla.HeaderRows = 3;

				// ============================================================
				// FILA 1: TÍTULO (se repite en cada página)
				// ============================================================
				string titulo = $"{grupo.Key.carga_des} ({grupo.Key.carga_nro})";

				PdfPCell celdaTitulo = new PdfPCell(new Phrase(titulo, fuenteTitulo))
				{
					Colspan = 7,
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_LEFT,
					PaddingBottom = 6f
				};

				// Subrayado
				celdaTitulo.CellEvent = new SubrayadoCellEvent();

				tabla.AddCell(celdaTitulo);

				// ============================================================
				// FILA 2 y 3: CABECERA REAL (igual al HTML)
				// ============================================================

				// CABECERA FILA 1
				PdfPCell c1 = new PdfPCell(new Phrase("Código", fuenteValor))
				{
					Rowspan = 2,
					Colspan = 2,
					BackgroundColor = amarilloPastel,
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE
				};
				tabla.AddCell(c1);

				PdfPCell c2 = new PdfPCell(new Phrase("Producto", fuenteValor))
				{
					Rowspan = 2,
					BackgroundColor = amarilloPastel,
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE
				};
				tabla.AddCell(c2);

				PdfPCell c3 = new PdfPCell(new Phrase("UP", fuenteValor))
				{
					Rowspan = 2,
					BackgroundColor = amarilloPastel,
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE
				};
				tabla.AddCell(c3);

				PdfPCell c4 = new PdfPCell(new Phrase("Stock Inventariado", fuenteValor))
				{
					Colspan = 3,
					BackgroundColor = amarilloPastel,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
				tabla.AddCell(c4);

				// CABECERA FILA 2
				tabla.AddCell(new PdfPCell(new Phrase("Bulto", fuenteValor)) { BackgroundColor = amarilloPastel, HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase("Cant. Suelta", fuenteValor)) { BackgroundColor = amarilloPastel, HorizontalAlignment = Element.ALIGN_CENTER });
				tabla.AddCell(new PdfPCell(new Phrase("Cant. Total", fuenteValor)) { BackgroundColor = amarilloPastel, HorizontalAlignment = Element.ALIGN_CENTER });

				// ============================================================
				// FILAS DE DATOS
				// ============================================================
				foreach (var item in grupo)
				{
					tabla.AddCell(new PdfPCell(new Phrase(item.p_id, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(item.p_id_barrado, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(item.p_desc, fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_LEFT });
					tabla.AddCell(new PdfPCell(new Phrase(item.p_unidad_pres.ToString(), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(item.invd_bulto.ToString(), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tabla.AddCell(new PdfPCell(new Phrase(item.invd_unidad_suelta.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tabla.AddCell(new PdfPCell(new Phrase(item.invd_cantidad.ToString("N2"), fuenteEtiqueta)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				}

				pdf.Add(tabla);
			}
		}

		public static void CargarRepoValorDetalle(Document pdf, List<InvRepoValorDetalleDto> lista, Font fChico, Font fNormal, Font fTitulo)
		{
			if (lista == null || lista.Count == 0)
			{
				pdf.Add(new Paragraph("No se encontraron datos", fNormal));
				return;
			}

			BaseColor amarilloPastel = new BaseColor(255, 245, 200);

			var grupos = lista
				.GroupBy(x => new { x.sec_id, x.sec_desc, x.rub_id, x.rub_desc })
				.OrderBy(g => g.Key.sec_id)
				.ThenBy(g => g.Key.rub_id);

			bool primera = true;

			foreach (var grupo in grupos)
			{
				if (!primera)
					pdf.NewPage();
				primera = false;

				var datos = grupo.ToList();

				int prodDelRubro = datos.Count;
				int prodConAjuste = datos.Count(x => x.ps_ajuste == 'S');
				decimal prodConConteo = prodDelRubro == 0 ? 0 : (prodConAjuste * 100m / prodDelRubro);

				decimal valorProdSinConteo = datos.Where(y => y.conteo1 == 0)
												  .Sum(x => x.ps_stk * x.p_costo);

				decimal valorProdConConteo = datos.Where(y => y.conteo1 > 0)
												  .Sum(x => (x.ps_conteo - x.ps_stk) * x.p_costo);

				decimal stkTotal = datos.Sum(x => x.ps_stk);
				decimal conteoTotal = datos.Sum(x => x.ps_conteo);
				decimal difTotal = stkTotal - conteoTotal;

				decimal stkTotalVal = datos.Sum(x => x.ps_stk * x.p_costo);
				decimal conteoTotalVal = datos.Sum(x => x.ps_conteo * x.p_costo);
				decimal difTotalVal = stkTotalVal - conteoTotalVal;

				// ============================================================
				// TABLA PRINCIPAL DEL GRUPO (título + cabecera + datos)
				// ============================================================
				PdfPTable tabla = new PdfPTable(9);
				tabla.WidthPercentage = 100;

				// Anchos equivalentes al <colgroup>
				tabla.SetWidths(new float[] { 10, 45, 10, 7, 7, 7, 7, 7, 7 });

				// ============================================================
				// FILA AGRUPADORA (colspan=9)
				// ============================================================
				PdfPCell celdaAgr = new PdfPCell();
				celdaAgr.Colspan = 9;
				celdaAgr.BackgroundColor = new BaseColor(230, 230, 230);
				celdaAgr.Padding = 6f;

				// Construimos el contenido del agrupador
				PdfPTable tAgr = new PdfPTable(2);
				tAgr.WidthPercentage = 100;
				tAgr.SetWidths(new float[] { 60, 40 });

				// ------------------ Columna izquierda (título + datos)
				PdfPTable tIzq = new PdfPTable(1);
				tIzq.WidthPercentage = 100;

				PdfPCell t1 = new PdfPCell(new Phrase(
					$"Sector: {grupo.Key.sec_desc} - Rubro: {grupo.Key.rub_desc}", fTitulo));
				t1.Border = Rectangle.NO_BORDER;
				tIzq.AddCell(t1);

				PdfPCell t2 = new PdfPCell(new Phrase(
					$"Prod. del Rubro: {prodDelRubro}\n" +
					$"Prod. con Conteo: {prodConConteo:N2}%\n" +
					$"Valoriza Prod. sin Conteo: {valorProdSinConteo:N2}\n" +
					$"Valoriza Dif. Prod. con Conteo: {valorProdConConteo:N2}", fNormal));
				t2.Border = Rectangle.NO_BORDER;
				tIzq.AddCell(t2);

				// ------------------ Columna derecha (mini‑tablas)
				PdfPTable tDer = new PdfPTable(2);
				tDer.WidthPercentage = 100;
				tDer.SetWidths(new float[] { 50, 50 });

				// Mini tabla Cantidades
				PdfPTable tCant = new PdfPTable(3);
				tCant.WidthPercentage = 100;
				tCant.AddCell(CeldaMiniHeader("Cantidades", 3, fNormal, amarilloPastel));
				tCant.AddCell(CeldaMini("Stk", fNormal, amarilloPastel));
				tCant.AddCell(CeldaMini("Conteo", fNormal, amarilloPastel));
				tCant.AddCell(CeldaMini("Dif", fNormal, amarilloPastel));
				tCant.AddCell(CeldaMini(stkTotal.ToString("N2"), fChico));
				tCant.AddCell(CeldaMini(conteoTotal.ToString("N2"), fChico));
				tCant.AddCell(CeldaMini(difTotal.ToString("N2"), fChico));

				// Mini tabla Valorización
				PdfPTable tVal = new PdfPTable(3);
				tVal.WidthPercentage = 100;
				tVal.AddCell(CeldaMiniHeader("Valorización", 3, fNormal, amarilloPastel));
				tVal.AddCell(CeldaMini("Stk", fNormal, amarilloPastel));
				tVal.AddCell(CeldaMini("Conteo", fNormal, amarilloPastel));
				tVal.AddCell(CeldaMini("Dif", fNormal, amarilloPastel));
				tVal.AddCell(CeldaMini(stkTotalVal.ToString("N2"), fChico));
				tVal.AddCell(CeldaMini(conteoTotalVal.ToString("N2"), fChico));
				tVal.AddCell(CeldaMini(difTotalVal.ToString("N2"), fChico));

				tDer.AddCell(Wrap(tCant));
				tDer.AddCell(Wrap(tVal));

				// Armamos la fila agrupadora
				tAgr.AddCell(Wrap(tIzq));
				tAgr.AddCell(Wrap(tDer));

				celdaAgr.AddElement(tAgr);
				tabla.AddCell(celdaAgr);

				// ============================================================
				// CABECERA DEL DETALLE (2 filas)
				// ============================================================
				BaseColor dorado = new BaseColor(186, 134, 11);

				tabla.AddCell(CeldaHeader("Código", fNormal, amarilloPastel, 2));
				tabla.AddCell(CeldaHeader("Descripción", fNormal, amarilloPastel, 2));
				tabla.AddCell(CeldaHeader("Ajuste", fNormal, amarilloPastel, 2));

				tabla.AddCell(CeldaHeader("Cantidades", fNormal, amarilloPastel, 1, 3));
				tabla.AddCell(CeldaHeader("Valorización", fNormal, amarilloPastel, 1, 3));

				tabla.AddCell(CeldaHeader("Stk", fNormal, amarilloPastel));
				tabla.AddCell(CeldaHeader("Conteo", fNormal, amarilloPastel));
				tabla.AddCell(CeldaHeader("Dif", fNormal, amarilloPastel));

				tabla.AddCell(CeldaHeader("Stk", fNormal, amarilloPastel));
				tabla.AddCell(CeldaHeader("Conteo", fNormal, amarilloPastel));
				tabla.AddCell(CeldaHeader("Dif", fNormal, amarilloPastel));

				// ============================================================
				// FILAS DE PRODUCTOS
				// ============================================================
				bool alt = true;

				foreach (var item in datos)
				{
					var difCant = item.ps_stk - item.ps_conteo;
					var stkVal = item.ps_stk * item.p_costo;
					var conVal = item.ps_conteo * item.p_costo;
					var difVal = stkVal - conVal;

					BaseColor fondo = alt ? new BaseColor(245, 245, 245) : BaseColor.White;
					alt = !alt;

					tabla.AddCell(CeldaDato(item.p_id, fChico, fondo));
					tabla.AddCell(CeldaDato(item.p_des, fChico, fondo));
					tabla.AddCell(CeldaDato(item.ps_ajuste == 'S' ? "✔" : "✘", fChico, fondo, Element.ALIGN_CENTER));

					tabla.AddCell(CeldaDato(item.ps_stk.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));
					tabla.AddCell(CeldaDato(item.ps_conteo.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));
					tabla.AddCell(CeldaDato(difCant.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));

					tabla.AddCell(CeldaDato(stkVal.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));
					tabla.AddCell(CeldaDato(conVal.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));
					tabla.AddCell(CeldaDato(difVal.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));
				}

				pdf.Add(tabla);
			}
		}

		public static void CargarRepoStkVsConteo(Document pdf, List<InvRepoStkVsConteoDto> lista, Font fChico, Font fNormal, Font fTitulo)
		{
			if (lista == null || lista.Count == 0)
			{
				pdf.Add(new Paragraph("No se encontraron datos", fNormal));
				return;
			}
			BaseColor amarilloPastel = new(255, 245, 200);
			// Tipo de inventario (se repite en todos los registros)
			var tipo = lista.First();
			bool incluyeGrupo2 = tipo.invt_id == 'D'; // si es 'D', agregar columna extra

			// ============================================================
			// TÍTULO DEL REPORTE
			// ============================================================
			//Paragraph titulo = new Paragraph($"Reporte: {tipo.invt_desc}", fTitulo);
			//titulo.SpacingAfter = 10f;
			//pdf.Add(titulo);

			// ============================================================
			// DEFINICIÓN DE COLUMNAS
			// ============================================================
			int columnas = incluyeGrupo2 ? 7 : 6;

			PdfPTable tabla = new(columnas)
			{
				WidthPercentage = 100
			};

			if (incluyeGrupo2)
			{
				tabla.SetWidths(new float[] { 10, 40, 10, 10, 10, 10, 10 });
			}
			else
			{
				tabla.SetWidths(new float[] { 10, 50, 10, 10, 10, 10 });
			}

			tabla.HeaderRows = 1;

			// ============================================================
			// CABECERA
			// ============================================================
			tabla.AddCell(CeldaHeader("Código", fNormal, amarilloPastel));
			tabla.AddCell(CeldaHeader("Descripción", fNormal, amarilloPastel));
			tabla.AddCell(CeldaHeader("Dif. a Ajustar", fNormal, amarilloPastel));
			tabla.AddCell(CeldaHeader("Aplico Ajuste", fNormal, amarilloPastel));
			tabla.AddCell(CeldaHeader("Stk", fNormal, amarilloPastel));
			tabla.AddCell(CeldaHeader("Conteo Grupo 1", fNormal, amarilloPastel));

			if (incluyeGrupo2)
				tabla.AddCell(CeldaHeader("Grupo Conteo 2", fNormal, amarilloPastel));

			// ============================================================
			// FILAS DE DATOS
			// ============================================================
			bool alt = true;

			foreach (var item in lista)
			{
				BaseColor fondo = alt ? new BaseColor(245, 245, 245) : BaseColor.White;
				alt = !alt;

				decimal difAjustar = item.ps_conteo - item.ps_stk;

				tabla.AddCell(CeldaDato(item.p_id, fChico, fondo));
				tabla.AddCell(CeldaDato(item.p_des, fChico, fondo));
				tabla.AddCell(CeldaDato(difAjustar.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));

				// Checkbox
				string chk = item.ps_ajuste == 'S' ? "✔" : "✘";
				tabla.AddCell(CeldaDato(chk, fChico, fondo, Element.ALIGN_CENTER));
				tabla.AddCell(CeldaDato(GridHelper.FormatearDato(item.ps_stk, GridHelper.FormatDato.Monto, item.PermiteDecimales), fChico, fondo, Element.ALIGN_RIGHT));
				tabla.AddCell(CeldaDato(item.conteo1.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));

				if (incluyeGrupo2)
					tabla.AddCell(CeldaDato(item.conteo2.ToString("N2"), fChico, fondo, Element.ALIGN_RIGHT));

			}

			pdf.Add(tabla);
		}

		public static void CargarRepoPedidoDeCliente(Document pdf, List<PedidoProductoDto> registros, Font chico, Font normalBold)
		{
			if (registros == null || registros.Count == 0)
				return;
			var cab = registros.First();

			// ============================
			// CABECERA DEL PEDIDO (2x2)
			// ============================

			PdfPTable cabecera = new PdfPTable(new float[] { 15f, 35f, 15f, 35f });
			cabecera.WidthPercentage = 100;
			cabecera.SpacingAfter = 5f;

			PdfPCell Celda(string texto, Font font, bool bold = false)
			{
				return new PdfPCell(new Phrase(texto, bold ? normalBold : font))
				{
					Border = Rectangle.NO_BORDER,
					Padding = 2f
				};
			}

			// Fila 1
			cabecera.AddCell(Celda("Cliente:", normalBold, true));
			cabecera.AddCell(Celda($"({cab.cta_id}) {cab.cta_denominacion}", chico));
			cabecera.AddCell(Celda("Fecha:", normalBold, true));
			cabecera.AddCell(Celda(cab.pc_fecha.ToString("dd/MM/yy"), chico));

			// Fila 2
			cabecera.AddCell(Celda("Vendedor:", normalBold, true));
			cabecera.AddCell(Celda(cab.ve_nombre, chico));
			cabecera.AddCell(Celda("Repartidor:", normalBold, true));
			cabecera.AddCell(Celda(cab.rp_nombre, chico));

			// Fila 3
			cabecera.AddCell(Celda("Estado:", normalBold, true));
			cabecera.AddCell(Celda(cab.pce_desc, chico));
			cabecera.AddCell(Celda("Reparto N°:", normalBold, true));
			cabecera.AddCell(Celda(cab.cm_compte, chico));

			// Fila 4
			cabecera.AddCell(Celda("Factura:", normalBold, true));
			cabecera.AddCell(Celda(cab.facturado, chico));
			cabecera.AddCell(Celda("Obs.:", normalBold, true));
			cabecera.AddCell(Celda(cab.pc_obs, chico));

			pdf.Add(cabecera);

			// ============================
			// SEPARADOR
			// ============================
			PdfPTable separador = new PdfPTable(1);
			separador.WidthPercentage = 100;

			PdfPCell linea = new PdfPCell()
			{
				BorderWidthBottom = 1f,
				BorderWidthTop = 0,
				BorderWidthLeft = 0,
				BorderWidthRight = 0,
				Padding = 2f
			};

			separador.AddCell(linea);
			pdf.Add(separador);


			// ============================
			// DETALLE POR RUBROS
			// ============================
			var grupos = registros
				.GroupBy(x => new { x.rub_id, x.rub_desc })
				.OrderBy(g => g.Key.rub_id);

			decimal totalPedido = 0m;
			decimal totalEntregado = 0m;

			foreach (var grupo in grupos)
			{
				// Título del Rubro
				Paragraph titulo = new(
					$"Rubros: {grupo.Key.rub_desc}",
					normalBold
				);
				titulo.SpacingBefore = 10f;
				titulo.SpacingAfter = 5f;
				pdf.Add(titulo);

				// ============================
				// TABLA IZQUIERDA (Pedido)
				// ============================
				PdfPTable tablaPedido = new(new float[] { 12f, 48f, 10f, 15f, 15f }); // 5 columnas
				tablaPedido.WidthPercentage = 100;
				tablaPedido.SplitLate = false;
				tablaPedido.SplitRows = false;
				tablaPedido.KeepTogether = true;

				AgregarCeldaHeader(tablaPedido, "Código", chico);
				AgregarCeldaHeader(tablaPedido, "Descripción", chico);
				AgregarCeldaHeader(tablaPedido, "Cant. Ped.", chico);
				AgregarCeldaHeader(tablaPedido, "Precio Vta.", chico);
				AgregarCeldaHeader(tablaPedido, "Total", chico);

				foreach (var item in grupo)
				{
					decimal totalLinea = item.pcd_pedida * item.pcd_pvta;
					totalPedido += totalLinea;

					tablaPedido.AddCell(new PdfPCell(new Phrase(item.p_id, chico)));
					tablaPedido.AddCell(new PdfPCell(new Phrase(item.p_desc, chico)));
					tablaPedido.AddCell(new PdfPCell(new Phrase(item.pcd_pedida.ToString("0.##"), chico)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tablaPedido.AddCell(new PdfPCell(new Phrase(item.pcd_pvta.ToString("0.00"), chico)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tablaPedido.AddCell(new PdfPCell(new Phrase(totalLinea.ToString("0.00"), chico)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				}

				// ============================
				// TABLA DERECHA (Entregado)
				// ============================
				PdfPTable tablaEntregado = new([60f, 40f]);
				tablaEntregado.WidthPercentage = 100;
				tablaEntregado.SplitLate = false;
				tablaEntregado.SplitRows = false;
				tablaEntregado.KeepTogether = true;

				AgregarCeldaHeader(tablaEntregado, "Cant. Ent.", chico);
				AgregarCeldaHeader(tablaEntregado, "Total", chico);

				foreach (var item in grupo)
				{
					decimal totalEnt = item.pcd_enviada * item.pcd_pvta;
					totalEntregado += totalEnt;

					tablaEntregado.AddCell(new PdfPCell(new Phrase(item.pcd_enviada.ToString("0.##"), chico)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tablaEntregado.AddCell(new PdfPCell(new Phrase(totalEnt.ToString("0.00"), chico)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				}

				// ============================
				// CONTENEDOR (lado a lado)
				// ============================
				PdfPTable contenedor = new PdfPTable(new float[] { 70f, 30f });
				contenedor.WidthPercentage = 100;
				contenedor.SpacingBefore = 5f;
				contenedor.KeepTogether = true;

				// Celda izquierda → tablaPedido
				PdfPCell celdaPedido = new(tablaPedido)
				{
					Border = Rectangle.NO_BORDER,
					Padding = 0
				};
				contenedor.AddCell(celdaPedido);

				// Celda derecha → tablaEntregado
				PdfPCell celdaEntregado = new(tablaEntregado)
				{
					Border = Rectangle.NO_BORDER,
					PaddingLeft = 10f   // separación visual
				};
				contenedor.AddCell(celdaEntregado);

				pdf.Add(contenedor);

			}

			// ============================
			// TOTALES FINALES
			// ============================
			PdfPTable tablaTotales = new PdfPTable(new float[] { 70f, 30f });
			tablaTotales.WidthPercentage = 100;
			tablaTotales.SpacingBefore = 10f;

			// Celda izquierda → Total Pedido
			PdfPCell celdaTotalPedido = new PdfPCell(new Phrase($"Total: {totalPedido:0.00}", normalBold))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				PaddingRight = 10f
			};
			tablaTotales.AddCell(celdaTotalPedido);

			// Celda derecha → Total Entregado
			PdfPCell celdaTotalEntregado = new PdfPCell(new Phrase($"Total: {totalEntregado:0.00}", normalBold))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT
			};
			tablaTotales.AddCell(celdaTotalEntregado);

			pdf.Add(tablaTotales);
		}

		public static void CargarRepoHojaDeRutaDeOrdenDeReparto(Document pdf, List<PedidoEnOrdenDeRepartoDto> registros, Font chico, Font normal, Font normalBold)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================
			// ENCABEZADO GENERAL DE LA OR
			// ============================
			var or = registros.First();

			// ============================
			// AGRUPAR POR CLIENTE
			// ============================
			var grupos = registros
				.GroupBy(x => new
				{
					x.cta_id,
					x.cta_denominacion,
					x.cta_domicilio,
					x.cta_te,
					x.cta_celu
				})
				.OrderBy(g => g.Key.cta_denominacion);

			decimal totalGeneral = 0;

			foreach (var grupo in grupos)
			{
				// ============================================================
				// SEPARADOR ANTES DEL CLIENTE (ANCHO COMPLETO)
				// ============================================================
				AgregarSeparador(pdf, chico);

				// ============================================================
				// TABLA DE CLIENTE (4 CELDAS)
				// ============================================================
				PdfPTable tablaCliente = new PdfPTable(new float[] { 20, 30, 30, 20 });
				tablaCliente.WidthPercentage = 100;

				tablaCliente.AddCell(new PdfPCell(new Phrase(
					$"Cliente: ({grupo.Key.cta_id})", normalBold))
				{
					Border = Rectangle.NO_BORDER
				});

				tablaCliente.AddCell(new PdfPCell(new Phrase(
					grupo.Key.cta_denominacion, normalBold))
				{
					Border = Rectangle.NO_BORDER
				});

				tablaCliente.AddCell(new PdfPCell(new Phrase(
					$"Domi: {grupo.Key.cta_domicilio}", normalBold))
				{
					Border = Rectangle.NO_BORDER
				});

				string telefono = $"{grupo.Key.cta_te}".Trim();
				if (!string.IsNullOrWhiteSpace(grupo.Key.cta_celu))
					telefono += $" / {grupo.Key.cta_celu}";

				tablaCliente.AddCell(new PdfPCell(new Phrase(
					$"Tel: {telefono}", normalBold))
				{
					Border = Rectangle.NO_BORDER
				});

				pdf.Add(tablaCliente);

				pdf.Add(new Paragraph(" ", chico)); // pequeño espacio

				// ============================================================
				// TABLA DE PEDIDOS (3 CELDAS + SANGRÍA)
				// ============================================================
				PdfPTable tablaPedidos = new PdfPTable(new float[] { 25, 55, 20 });
				tablaPedidos.WidthPercentage = 100;

				foreach (var ped in grupo)
				{
					tablaPedidos.AddCell(new PdfPCell(new Phrase(
						$"    Pedido N°: {ped.pc_compte}", normalBold))   // sangría con espacios
					{
						Border = Rectangle.NO_BORDER
					});

					tablaPedidos.AddCell(new PdfPCell(new Phrase(
						$"Comprobante: {ped.tco_desc} {ped.cm_compte}", normalBold))
					{
						Border = Rectangle.NO_BORDER
					});

					var precio = ped.pc_precio_tot.ToString("N2");
					tablaPedidos.AddCell(new PdfPCell(new Phrase(
						$"Importe: {precio}", normalBold))
					{
						Border = Rectangle.NO_BORDER,
						HorizontalAlignment = Element.ALIGN_RIGHT
					});

					totalGeneral += ped.pc_precio_tot;
				}

				pdf.Add(tablaPedidos);

				// ============================================================
				// SEPARADOR ENTRE GRUPOS (ANCHO COMPLETO)
				// ============================================================
				AgregarSeparador(pdf, chico);
			}

			// ============================
			// TOTAL GENERAL DE LA OR
			// ============================
			PdfPTable tablaTotal = new PdfPTable(1);
			tablaTotal.WidthPercentage = 100;

			tablaTotal.AddCell(new PdfPCell(new Phrase(
				$"TOTAL ORDEN DE REPARTO: {totalGeneral:N2}", normalBold))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				PaddingTop = 10
			});

			pdf.Add(tablaTotal);

		}

		public static void CargarRepoHojaDeProductoDeOrdenDeReparto(Document pdf, List<OrdenDeRepartoDetalleDto> registros, Font chico, Font normal, Font normalBold)
		{
			if (registros == null || registros.Count == 0)
				return;

			// Agrupar por rubro
			var grupos = registros
				.GroupBy(x => new { x.rub_id, x.rub_desc })
				.OrderBy(g => g.Key.rub_desc);

			foreach (var grupo in grupos)
			{
				// ============================================================
				// SEPARADOR DE ANCHO COMPLETO
				// ============================================================
				PdfPTable sep = new PdfPTable(1);
				sep.WidthPercentage = 100;

				sep.AddCell(new PdfPCell(new Phrase(" ", chico))
				{
					Border = Rectangle.BOTTOM_BORDER,
					BorderWidthBottom = 1f,
					PaddingBottom = 5
				});

				pdf.Add(sep);

				// ============================================================
				// TÍTULO DEL RUBRO
				// ============================================================
				Font fontRubro = new Font(normalBold.BaseFont, normalBold.Size + 2, Font.BOLD);

				PdfPTable tablaTitulo = new PdfPTable(1);
				tablaTitulo.WidthPercentage = 100;

				tablaTitulo.AddCell(new PdfPCell(new Phrase(
					$"Rubros: {grupo.Key.rub_desc}", fontRubro))
				{
					Border = Rectangle.NO_BORDER,
					PaddingTop = 4,
					PaddingBottom = 6
				});

				pdf.Add(tablaTitulo);

				// ============================================================
				// TABLA DE PRODUCTOS (VISIBLE, ENCABEZADO GRIS)
				// ============================================================
				PdfPTable tabla = new PdfPTable(new float[] { 15, 65, 20 });
				tabla.WidthPercentage = 100;

				BaseColor grisSuave = new BaseColor(230, 230, 230);

				// Encabezados visibles
				tabla.AddCell(CeldaHeaderVisible("Código", normalBold, grisSuave));
				tabla.AddCell(CeldaHeaderVisible("Descripción", normalBold, grisSuave));
				tabla.AddCell(CeldaHeaderVisible("Cant. Enviada", normalBold, grisSuave, Element.ALIGN_RIGHT));

				// Filas de productos
				foreach (var item in grupo)
				{
					tabla.AddCell(CeldaDatoVisible(item.p_id, normal));
					tabla.AddCell(CeldaDatoVisible(item.p_desc, normal));

					string cantidad = item.PermiteDecimales
						? item.pcd_enviada.ToString("N2")
						: ((int)item.pcd_enviada).ToString();

					tabla.AddCell(CeldaDatoVisible(cantidad, normal, Element.ALIGN_RIGHT));
				}

				pdf.Add(tabla);
			}


		}

		public static void CargarRepoPedidoInterno(Document pdf, List<PIDetalleDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			var reg0 = registros.First();

			// ================================
			// ENCABEZADO MANUAL
			// ================================
			AgregarEncabezado(pdf, reg0, normal, normalBold);

			// ================================
			// TÍTULO
			// ================================
			Paragraph tit = new Paragraph("Detalle de Productos Solicitados", titulo);
			tit.Alignment = Element.ALIGN_CENTER;
			tit.SpacingAfter = 10f;
			pdf.Add(tit);

			// ================================
			// TABLA PRINCIPAL
			// ================================
			PdfPTable tabla = new PdfPTable(4);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 10f, 55f, 15f, 20f });

			// Encabezados
			AgregarCeldaHeader(tabla, "Código", normalBold);
			AgregarCeldaHeader(tabla, "Descripción", normalBold);
			AgregarCeldaHeader(tabla, "Ref. Prov.", normalBold);
			AgregarCeldaHeader(tabla, "Código de Barras", normalBold);

			// ================================
			// AGRUPADOR ÚNICO POR RUBRO
			// ================================
			string grupoActual = "";

			foreach (var item in registros
				.OrderBy(x => x.rub_id)
				.ThenBy(x => x.p_id))
			{
				// Detectar salto de página
				if (writerFitsNewPage(pdf, tabla))
				{
					pdf.NewPage();
					AgregarEncabezado(pdf, reg0, normal, normalBold);

					Paragraph titulo2 = new Paragraph("Detalle de Productos Solicitados", normalBold);
					titulo2.Alignment = Element.ALIGN_CENTER;
					titulo2.SpacingAfter = 10f;
					pdf.Add(titulo2);

					// Reimprimir encabezados
					AgregarCeldaHeader(tabla, "Código", normalBold);
					AgregarCeldaHeader(tabla, "Descripción", normalBold);
					AgregarCeldaHeader(tabla, "Ref. Prov.", normalBold);
					AgregarCeldaHeader(tabla, "Código de Barras", normalBold);
					AgregarCeldaHeader(tabla, "Bultos Aprox.", normalBold);
				}

				// ---- ÚNICO AGRUPADOR ----
				string grupo = $"{item.rub_desc} ({item.rub_id})";

				if (grupo != grupoActual)
				{
					PdfPCell celdaGrupo = new(new Phrase(grupo, normalBold))
					{
						Colspan = 4,
						BackgroundColor = new BaseColor(230, 230, 230),
						Padding = 5,
						HorizontalAlignment = Element.ALIGN_CENTER
					};
					tabla.AddCell(celdaGrupo);

					grupoActual = grupo;
				}

				// ---- Fila de producto ----
				AgregarCelda(tabla, item.p_id, chico, Element.ALIGN_CENTER);
				AgregarCelda(tabla, item.p_desc, chico, Element.ALIGN_LEFT);
				AgregarCelda(tabla, item.p_id_prov ?? "", chico, Element.ALIGN_RIGHT);
				AgregarCelda(tabla, item.p_id_barrado, chico, Element.ALIGN_CENTER);
			}

			pdf.Add(tabla);
		}

		public static void CargarRepoAnalisisDeVentaMensual(Document pdf, List<AnaVtaMesDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			pdf.SetPageSize(PageSize.A4.Rotate());
			pdf.SetMargins(20f, 20f, 20f, 20f);

			PdfPTable tabla = new(9)
			{
				WidthPercentage = 100
			};
			tabla.SetWidths([
				1.2f, // Mes
				2.0f, // Facturación
				2.0f, // Fact. Ac.
				2.2f, // Dif. Mes Ant.
				2.2f, // Dif. Mes/Año Ant.
				2.0f, // Costo
				2.0f, // Rentabilidad
				2.0f, // Rent. Ac.
				2.2f  // Vta. CtaCte.
			]);

			void AddHeader(string texto)
			{
				PdfPCell c = new(new Phrase(texto, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(230, 230, 230),
					Padding = 4
				};
				tabla.AddCell(c);
			}

			AddHeader("Mes");
			AddHeader("Facturación");
			AddHeader("Fact. Ac.");
			AddHeader("Dif. Mes Ant.");
			AddHeader("Dif. Mes/Año Ant.");
			AddHeader("Costo");
			AddHeader("Rentabilidad");
			AddHeader("Rent. Ac.");
			AddHeader("Vta. CtaCte.");


			BaseColor ColorPorcentaje(decimal valor)
			{
				if (valor > 0) return new BaseColor(201, 228, 255); // celeste
				if (valor < 0) return new BaseColor(255, 224, 224); // rojo suave
				return BaseColor.White;
			}

			foreach (var r in registros)
			{
				// Mes
				tabla.AddCell(new PdfPCell(new Phrase($"{r.periodo}-{r.mes:00}", normal))
				{ HorizontalAlignment = Element.ALIGN_CENTER });

				// Facturación
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Facturación acumulada
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion_acu.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Dif. Mes Ant. (porcentaje + valor)
				{
					PdfPTable mini = new PdfPTable(2);
					mini.WidthPercentage = 100;
					mini.SetWidths(new float[] { 1f, 1f });

					PdfPCell porc = new PdfPCell(new Phrase($"{r.mes_ant_dif_porc}%", chico))
					{
						BackgroundColor = ColorPorcentaje(r.mes_ant_dif_porc),
						HorizontalAlignment = Element.ALIGN_LEFT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					PdfPCell val = new PdfPCell(new Phrase(r.mes_ant_dif.ToString("N2"), chico))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					mini.AddCell(porc);
					mini.AddCell(val);

					PdfPCell cont = new PdfPCell(mini);
					tabla.AddCell(cont);
				}

				// Dif. Mes/Año Ant. (igual que arriba)
				{
					PdfPTable mini = new PdfPTable(2);
					mini.WidthPercentage = 100;
					mini.SetWidths(new float[] { 1f, 1f });

					PdfPCell porc = new PdfPCell(new Phrase($"{r.per_ant_dif_porc}%", chico))
					{
						BackgroundColor = ColorPorcentaje(r.per_ant_dif_porc),
						HorizontalAlignment = Element.ALIGN_LEFT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					PdfPCell val = new PdfPCell(new Phrase(r.per_ant_dif.ToString("N2"), chico))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					mini.AddCell(porc);
					mini.AddCell(val);

					PdfPCell cont = new PdfPCell(mini);
					tabla.AddCell(cont);
				}

				// Costo
				tabla.AddCell(new PdfPCell(new Phrase(r.co_costo.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Rentabilidad
				tabla.AddCell(new PdfPCell(new Phrase(r.rentabilidad.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Rentabilidad acumulada
				tabla.AddCell(new PdfPCell(new Phrase(r.rentabilidad_acu.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Vta. CtaCte. (porcentaje + valor)
				{
					PdfPTable mini = new PdfPTable(2);
					mini.WidthPercentage = 100;
					mini.SetWidths(new float[] { 1f, 1f });

					PdfPCell porc = new PdfPCell(new Phrase($"{r.ctacte_dif_porc}%", chico))
					{
						BackgroundColor = ColorPorcentaje(r.ctacte_dif_porc),
						HorizontalAlignment = Element.ALIGN_LEFT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					PdfPCell val = new PdfPCell(new Phrase(r.ctacte_dif.ToString("N2"), chico))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					mini.AddCell(porc);
					mini.AddCell(val);

					PdfPCell cont = new PdfPCell(mini);
					tabla.AddCell(cont);
				}
			}

			pdf.Add(tabla);

		}

		public static void CargarRepoAnalisisDeVentaDiario(Document pdf, List<AnaVtaMesDetalleDiarioDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			// Página apaisada
			pdf.SetPageSize(PageSize.A4.Rotate());
			pdf.SetMargins(20f, 20f, 20f, 20f);

			// ============================
			// ENCABEZADO REPETIBLE
			// ============================
			PdfPTable header = new PdfPTable(1);
			header.WidthPercentage = 100;

			PdfPCell tituloCell = new PdfPCell(new Phrase("Análisis de Venta Diario", tituloBig))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 5
			};
			header.AddCell(tituloCell);

			PdfPCell subCell = new PdfPCell(new Phrase("Detalle por día", titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 10
			};
			header.AddCell(subCell);

			header.HeaderRows = 2;
			pdf.Add(header);

			// ============================
			// TABLA PRINCIPAL
			// ============================
			PdfPTable tabla = new PdfPTable(11)
			{
				WidthPercentage = 100
			};

			tabla.SetWidths(new float[] {
				1.4f, // Día
				2.0f, // Facturación
				2.0f, // Fact. Ac.
				1.8f, // % Acu MA
				2.0f, // Costo
				2.0f, // Rentabilidad
				2.0f, // Rent. Ac.
				2.2f, // Vta CtaCte (mini tabla)
				2.0f, // Fact MA
				2.0f, // Fact MI
				2.0f  // Fact Dif
			});

			// ============================
			// ENCABEZADOS
			// ============================
			void AddHeader(string texto)
			{
				PdfPCell c = new PdfPCell(new Phrase(texto, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(230, 230, 230),
					Padding = 4
				};
				tabla.AddCell(c);
			}

			AddHeader("Día");
			AddHeader("Facturación");
			AddHeader("Fact. Ac.");
			AddHeader("% Acu. MA");
			AddHeader("Costo");
			AddHeader("Rentabilidad");
			AddHeader("Rent. Ac.");
			AddHeader("Vta. CtaCte.");
			AddHeader("Fact. MA");
			AddHeader("Fact. MI");
			AddHeader("Fact. Dif.");

			// ============================
			// COLORES PARA PORCENTAJES
			// ============================
			BaseColor ColorPorcentaje(decimal valor)
			{
				if (valor > 0) return new BaseColor(201, 228, 255); // celeste
				if (valor < 0) return new BaseColor(255, 224, 224); // rojo suave
				return BaseColor.White;
			}

			// ============================
			// FILAS
			// ============================
			foreach (var r in registros)
			{
				decimal fac_may_acu_porc = r.fac_may_acu_porc * 100;
				decimal ctacte_dif_porc = r.ctacte_dif_porc * 100;

				// Día
				tabla.AddCell(new PdfPCell(new Phrase(r.dia.ToString("dd/MM/yyyy"), normal))
				{ HorizontalAlignment = Element.ALIGN_CENTER });

				// Facturación
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Facturación acumulada
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion_acu.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// % Acu MA
				tabla.AddCell(new PdfPCell(new Phrase($"{r.fac_may_acu_porc}%", normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = ColorPorcentaje(r.fac_may_acu_porc), });

				// Costo
				tabla.AddCell(new PdfPCell(new Phrase(r.co_costo.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Rentabilidad
				tabla.AddCell(new PdfPCell(new Phrase(r.rentabilidad.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Rentabilidad acumulada
				tabla.AddCell(new PdfPCell(new Phrase(r.rentabilidad_acu.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Vta. CtaCte. (porcentaje + valor)
				{
					PdfPTable mini = new PdfPTable(2);
					mini.WidthPercentage = 100;
					mini.SetWidths(new float[] { 1f, 1f });

					PdfPCell porc = new PdfPCell(new Phrase($"{ctacte_dif_porc:N2}%", chico))
					{
						BackgroundColor = ColorPorcentaje(ctacte_dif_porc),
						HorizontalAlignment = Element.ALIGN_LEFT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					PdfPCell val = new PdfPCell(new Phrase(r.ctacte_dif.ToString("N2"), chico))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					mini.AddCell(porc);
					mini.AddCell(val);

					tabla.AddCell(new PdfPCell(mini));
				}

				// Fact MA
				tabla.AddCell(new PdfPCell(new Phrase(r.co_fac_may.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Fact MI
				tabla.AddCell(new PdfPCell(new Phrase(r.co_fac_min.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Fact Dif
				tabla.AddCell(new PdfPCell(new Phrase(r.co_fac_dis.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });
			}

			pdf.Add(tabla);
		}

		public static void CargarRepoAnalisisDeVentaHora(Document pdf, List<AnaVtaMesDetalleHoraDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			// Página apaisada
			pdf.SetPageSize(PageSize.A4.Rotate());
			pdf.SetMargins(20f, 20f, 20f, 20f);

			// ============================
			// ENCABEZADO REPETIBLE
			// ============================
			PdfPTable header = new PdfPTable(1);
			header.WidthPercentage = 100;

			PdfPCell tituloCell = new PdfPCell(new Phrase("Análisis de Venta por Hora", tituloBig))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 5
			};
			header.AddCell(tituloCell);

			PdfPCell subCell = new PdfPCell(new Phrase("Detalle por franja horaria", titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 10
			};
			header.AddCell(subCell);

			header.HeaderRows = 2;
			pdf.Add(header);

			// ============================
			// TABLA PRINCIPAL
			// ============================
			PdfPTable tabla = new PdfPTable(16)
			{
				WidthPercentage = 100
			};

			tabla.SetWidths(new float[] {
				1.6f, // Día
				1.4f,1.4f,1.4f,1.4f,1.4f,1.4f,1.4f,1.4f,
				1.4f,1.4f,1.4f,1.4f,1.4f,1.4f,1.4f // 6-8 ... 21-22
			});

			// ============================
			// ENCABEZADOS
			// ============================
			void AddHeader(string texto)
			{
				PdfPCell c = new PdfPCell(new Phrase(texto, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(230, 230, 230),
					Padding = 4
				};
				tabla.AddCell(c);
			}

			AddHeader("Día");
			AddHeader("6 a 8");
			AddHeader("8 a 9");
			AddHeader("9 a 10");
			AddHeader("10 a 11");
			AddHeader("11 a 12");
			AddHeader("12 a 13");
			AddHeader("13 a 14");
			AddHeader("14 a 15");
			AddHeader("15 a 16");
			AddHeader("16 a 17");
			AddHeader("17 a 18");
			AddHeader("18 a 19");
			AddHeader("19 a 20");
			AddHeader("20 a 21");
			AddHeader("21 a 22");

			// ============================
			// FUNCIÓN MINI‑CELDA (FA / OP)
			// ============================
			PdfPCell CeldaDual(decimal fa, decimal op)
			{
				PdfPTable mini = new PdfPTable(1);
				mini.WidthPercentage = 100;

				PdfPCell c1 = new PdfPCell(new Phrase(fa.ToString("N2"), chico))
				{
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Padding = 2,
					Border = Rectangle.NO_BORDER
				};

				PdfPCell c2 = new PdfPCell(new Phrase(op.ToString("N0"), chico))
				{
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Padding = 2,
					Border = Rectangle.NO_BORDER
				};

				// Línea divisoria entre FA y OP
				c1.BorderWidthBottom = 0.5f;
				c1.BorderColorBottom = new BaseColor(200, 200, 200);

				mini.AddCell(c1);
				mini.AddCell(c2);

				return new PdfPCell(mini)
				{
					Padding = 0
				};
			}

			// ============================
			// FILAS
			// ============================
			foreach (var r in registros)
			{
				// Día
				tabla.AddCell(new PdfPCell(new Phrase(r.dia.ToString("dd/MM/yyyy"), normal))
				{ HorizontalAlignment = Element.ALIGN_CENTER });

				// 6 a 8
				tabla.AddCell(CeldaDual(r.fa_6a8, r.op_6a8));
				tabla.AddCell(CeldaDual(r.fa_8a9, r.op_8a9));
				tabla.AddCell(CeldaDual(r.fa_9a10, r.op_9a10));
				tabla.AddCell(CeldaDual(r.fa_10a11, r.op_10a11));
				tabla.AddCell(CeldaDual(r.fa_11a12, r.op_11a12));
				tabla.AddCell(CeldaDual(r.fa_12a13, r.op_12a13));
				tabla.AddCell(CeldaDual(r.fa_13a14, r.op_13a14));
				tabla.AddCell(CeldaDual(r.fa_14a15, r.op_14a15));
				tabla.AddCell(CeldaDual(r.fa_15a16, r.op_15a16));
				tabla.AddCell(CeldaDual(r.fa_16a17, r.op_16a17));
				tabla.AddCell(CeldaDual(r.fa_17a18, r.op_17a18));
				tabla.AddCell(CeldaDual(r.fa_18a19, r.op_18a19));
				tabla.AddCell(CeldaDual(r.fa_19a20, r.op_19a20));
				tabla.AddCell(CeldaDual(r.fa_20a21, r.op_20a21));
				tabla.AddCell(CeldaDual(r.fa_21a22, r.op_21a22));
			}

			pdf.Add(tabla);
		}

		public static void CargarRepoAnalisisDeVentaSucursal(Document pdf, List<AnaVtaMesDetalleSucursalDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			// Página vertical (no apaisada)
			pdf.SetPageSize(PageSize.A4);
			pdf.SetMargins(20f, 20f, 20f, 20f);

			// ============================
			// ENCABEZADO REPETIBLE
			// ============================
			PdfPTable header = new PdfPTable(1);
			header.WidthPercentage = 100;

			PdfPCell tituloCell = new PdfPCell(new Phrase("Análisis de Venta por Sucursal", tituloBig))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 5
			};
			header.AddCell(tituloCell);

			PdfPCell subCell = new PdfPCell(new Phrase("Detalle por sucursal", titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 10
			};
			header.AddCell(subCell);

			header.HeaderRows = 2;
			pdf.Add(header);

			// ============================
			// TABLA PRINCIPAL
			// ============================
			PdfPTable tabla = new PdfPTable(6)
			{
				WidthPercentage = 100
			};

			tabla.SetWidths(new float[] {
				3.0f, // Sucursal
				2.0f, // Facturación
				1.6f, // Porcentaje
				2.0f, // Cta Cte
				2.0f, // Cobranza
				2.0f  // Rentabilidad
			});

			// ============================
			// COLORES PARA PORCENTAJES
			// ============================
			BaseColor ColorPorcentaje(decimal valor)
			{
				if (valor > 0) return new BaseColor(201, 228, 255); // celeste
				if (valor < 0) return new BaseColor(255, 224, 224); // rojo suave
				return BaseColor.White;
			}

			// ============================
			// ENCABEZADOS
			// ============================
			void AddHeader(string texto)
			{
				PdfPCell c = new PdfPCell(new Phrase(texto, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(230, 230, 230),
					Padding = 4
				};
				tabla.AddCell(c);
			}

			AddHeader("Sucursal");
			AddHeader("Facturación");
			AddHeader("Porcentaje");
			AddHeader("Cta. Cte.");
			AddHeader("Cobranza");
			AddHeader("Rentabilidad");

			// ============================
			// FILAS
			// ============================
			foreach (var r in registros)
			{
				// Sucursal
				tabla.AddCell(new PdfPCell(new Phrase(r.adm_nombre, normal))
				{ HorizontalAlignment = Element.ALIGN_LEFT });

				// Facturación
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Porcentaje
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion_porc.ToString("N2") + "%", normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = ColorPorcentaje(r.co_facturacion_porc), });

				// Cta. Cte.
				tabla.AddCell(new PdfPCell(new Phrase(r.co_ctacte.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Cobranza
				tabla.AddCell(new PdfPCell(new Phrase(r.co_cobranza.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Rentabilidad
				tabla.AddCell(new PdfPCell(new Phrase(r.rentabilidad.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });
			}

			pdf.Add(tabla);
		}

		public static void CargarRepoAnalisisDeVentaCierre(Document pdf, List<AnaVtaMesDetalleCierreDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			// Página apaisada
			pdf.SetPageSize(PageSize.A4.Rotate());
			pdf.SetMargins(20f, 20f, 20f, 20f);

			// ============================
			// ENCABEZADO REPETIBLE
			// ============================
			PdfPTable header = new PdfPTable(1);
			header.WidthPercentage = 100;

			PdfPCell tituloCell = new PdfPCell(new Phrase("Análisis de Venta - Cierres", tituloBig))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 5
			};
			header.AddCell(tituloCell);

			PdfPCell subCell = new PdfPCell(new Phrase("Detalle de cierres por sucursal", titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 10
			};
			header.AddCell(subCell);

			header.HeaderRows = 2;
			pdf.Add(header);

			// ============================
			// TABLA PRINCIPAL
			// ============================
			PdfPTable tabla = new PdfPTable(11)
			{
				WidthPercentage = 100
			};

			tabla.SetWidths(new float[] {
				3.0f, // Sucursal
				1.4f, // Proceso
				1.4f, // Cierre
				2.0f, // Facturación
				2.0f, // Fact. Dif.
				2.0f, // Cta Cte
				2.0f, // Cobranza
				2.0f, // Cob. Dif.
				2.0f, // Devoluciones y NC
				2.0f, // Créditos Usados
				2.0f  // A Rendir
			});

			// ============================
			// ENCABEZADOS
			// ============================
			void AddHeader(string texto)
			{
				PdfPCell c = new PdfPCell(new Phrase(texto, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(230, 230, 230),
					Padding = 4
				};
				tabla.AddCell(c);
			}

			AddHeader("Sucursal");
			AddHeader("Proceso");
			AddHeader("Cierre");
			AddHeader("Facturación");
			AddHeader("Fact. Dif.");
			AddHeader("Cta. Cte.");
			AddHeader("Cobranza");
			AddHeader("Cob. Dif.");
			AddHeader("Devoluciones y NC");
			AddHeader("Créditos Usados");
			AddHeader("A Rendir");

			// ============================
			// FILAS
			// ============================
			foreach (var r in registros)
			{
				// Sucursal
				tabla.AddCell(new PdfPCell(new Phrase(r.adm_nombre, normal))
				{ HorizontalAlignment = Element.ALIGN_LEFT });

				// Proceso
				tabla.AddCell(new PdfPCell(new Phrase(r.caja_nro_proceso.ToString(), normal))
				{ HorizontalAlignment = Element.ALIGN_CENTER });

				// Cierre
				tabla.AddCell(new PdfPCell(new Phrase(r.caja_nro_cierre.ToString(), normal))
				{ HorizontalAlignment = Element.ALIGN_CENTER });

				// Facturación
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Facturación Diferida
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion_dif.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Cta. Cte.
				tabla.AddCell(new PdfPCell(new Phrase(r.co_ctacte.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Cobranza
				tabla.AddCell(new PdfPCell(new Phrase(r.co_cobranza.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Cobranza Diferida
				tabla.AddCell(new PdfPCell(new Phrase(r.co_cobranza_dif.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Devoluciones y NC
				tabla.AddCell(new PdfPCell(new Phrase(r.co_nota_credito.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Créditos Usados
				tabla.AddCell(new PdfPCell(new Phrase(r.co_creditos_usados.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// A Rendir
				tabla.AddCell(new PdfPCell(new Phrase(r.a_rendir.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });
			}

			pdf.Add(tabla);
		}

		public static void CargarRepoAnalisisDeVentaAnual(Document pdf, List<AnaVtaMesDetalleAnualDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			// Página vertical
			pdf.SetPageSize(PageSize.A4);
			pdf.SetMargins(20f, 20f, 20f, 20f);

			// ============================
			// ENCABEZADO REPETIBLE
			// ============================
			PdfPTable header = new PdfPTable(1);
			header.WidthPercentage = 100;

			PdfPCell tituloCell = new PdfPCell(new Phrase("Análisis de Venta Anual", tituloBig))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 5
			};
			header.AddCell(tituloCell);

			PdfPCell subCell = new PdfPCell(new Phrase("Detalle por año", titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 10
			};
			header.AddCell(subCell);

			header.HeaderRows = 2;
			pdf.Add(header);

			// ============================
			// TABLA PRINCIPAL
			// ============================
			PdfPTable tabla = new PdfPTable(4)
			{
				WidthPercentage = 100
			};

			tabla.SetWidths(new float[] {
				1.4f, // Año
				2.2f, // Facturación
				2.2f, // Dif. Año Anterior
				2.2f  // Rentabilidad
			});

			// ============================
			// ENCABEZADOS
			// ============================
			void AddHeader(string texto)
			{
				PdfPCell c = new PdfPCell(new Phrase(texto, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(230, 230, 230),
					Padding = 4
				};
				tabla.AddCell(c);
			}

			AddHeader("Año");
			AddHeader("Facturación");
			AddHeader("Dif. Año Anterior");
			AddHeader("Rentabilidad");

			// ============================
			// FILAS
			// ============================
			foreach (var r in registros)
			{
				// Año
				tabla.AddCell(new PdfPCell(new Phrase(r.periodo.ToString(), normal))
				{ HorizontalAlignment = Element.ALIGN_CENTER });

				// Facturación
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Diferencia Año Anterior
				tabla.AddCell(new PdfPCell(new Phrase(r.diferencia.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Rentabilidad
				tabla.AddCell(new PdfPCell(new Phrase(r.co_costo.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });
			}

			pdf.Add(tabla);
		}

		public static void CargarRepoAnalisisDeValoresDeVentaMensual(Document pdf, List<AnaValDeVtaMesDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || !registros.Any())
			{
				pdf.Add(new Paragraph("No hay datos para mostrar", normalBold));
				return;
			}

			// Título
			Paragraph tituloPar = new Paragraph("Análisis de Valores de Venta - Mensual", tituloBig);
			tituloPar.Alignment = Element.ALIGN_CENTER;
			tituloPar.SpacingAfter = 10f;
			pdf.Add(tituloPar);

			// Definición de columnas
			float[] widths = { 1f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f };
			PdfPTable tabla = new PdfPTable(widths);
			tabla.WidthPercentage = 100;

			// Encabezados
			string[] headers = {
				"Mes",
				"Fact. + Cob.",
				"Cta. Cte.",
				"Cta. Cte. Dist.",
				"Efectivo",
				"Tarjetas",
				"Transf. Bco.",
				"Mutuales",
				"Vales",
				"Otros"
			};

			foreach (var h in headers)
			{
				PdfPCell celda = new PdfPCell(new Phrase(h, normalBold));
				celda.HorizontalAlignment = Element.ALIGN_CENTER;
				celda.BackgroundColor = new BaseColor(230, 230, 230);
				celda.Padding = 4;
				tabla.AddCell(celda);
			}

			// Filas
			foreach (var item in registros)
			{
				tabla.AddCell(new PdfPCell(new Phrase($"{item.periodo}-{item.mes}", HelperPdf.FontChicoPredeterminado())));

				// Facturación + Cobranzas
				tabla.AddCell(CeldaSoloMonto(item.co_facturacion + item.co_cobranza, HelperPdf.FontChicoPredeterminado()));

				// Cta. Cte.
				tabla.AddCell(CeldaPorcMonto(item.co_ctacte_porc, item.co_ctacte, chico, HelperPdf.FontChicoPredeterminado()));

				// Cta. Cte. Dist.
				tabla.AddCell(CeldaPorcMonto(item.co_ctacte_dist_porc, item.co_ctacte_dist, chico, HelperPdf.FontChicoPredeterminado()));

				// Efectivo
				tabla.AddCell(CeldaPorcMonto(item.efectivos_porc, item.efectivos, chico, HelperPdf.FontChicoPredeterminado()));

				// Tarjetas
				tabla.AddCell(CeldaPorcMonto(item.tarjetas_porc, item.tarjetas, chico, HelperPdf.FontChicoPredeterminado()));

				// Transferencias
				tabla.AddCell(CeldaPorcMonto(item.bco_transf_porc, item.bco_transf, chico, HelperPdf.FontChicoPredeterminado()));

				// Mutuales
				tabla.AddCell(CeldaPorcMonto(item.mutuales_porc, item.mutuales, chico, HelperPdf.FontChicoPredeterminado()));

				// Vales
				tabla.AddCell(CeldaPorcMonto(item.vales_porc, item.vales, chico, HelperPdf.FontChicoPredeterminado()));

				// Otros
				tabla.AddCell(CeldaPorcMonto(item.otros_porc, item.otros, chico, HelperPdf.FontChicoPredeterminado()));
			}

			pdf.Add(tabla);
		}

		//public static void CargarRepoAnalisisDeValoresDeVentaDiario(Document pdf, List<AnaValDeVtaDetDiarioDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig, int tabId)
		//{
		//	if (registros == null || !registros.Any())
		//	{
		//		pdf.Add(new Paragraph("No hay datos para mostrar", normalBold));
		//		return;
		//	}

		//	// Título
		//	Paragraph tituloPar = new Paragraph("Análisis de Valores de Venta - Diario", tituloBig);
		//	tituloPar.Alignment = Element.ALIGN_CENTER;
		//	tituloPar.SpacingAfter = 10f;
		//	pdf.Add(tituloPar);

		//	// Definición de columnas
		//	float[] widths = { 1.6f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f };
		//	PdfPTable tabla = new PdfPTable(widths);
		//	tabla.WidthPercentage = 100;

		//	// Encabezados
		//	string[] headers = {
		//		"Día",
		//		"Fact. + Cob.",
		//		"Cta. Cte.",
		//		"Cta. Cte. Dist.",
		//		"Efectivo",
		//		"Tarjetas",
		//		"Transf. Bco.",
		//		"Mutuales",
		//		"Vales",
		//		"Otros"
		//	};

		//	foreach (var h in headers)
		//	{
		//		PdfPCell celda = new PdfPCell(new Phrase(h, normalBold));
		//		celda.HorizontalAlignment = Element.ALIGN_CENTER;
		//		celda.BackgroundColor = new BaseColor(230, 230, 230);
		//		celda.Padding = 4;
		//		tabla.AddCell(celda);
		//	}

		//	// Filas
		//	foreach (var item in registros)
		//	{
		//		PdfPCell celdaDia = new PdfPCell(new Phrase(item.dia.ToString("dd/MM/yyyy"), normal));
		//		celdaDia.HorizontalAlignment = Element.ALIGN_CENTER;
		//		celdaDia.VerticalAlignment = Element.ALIGN_MIDDLE;
		//		celdaDia.Padding = 4;
		//		tabla.AddCell(celdaDia);

		//		// Facturación + Cobranzas
		//		tabla.AddCell(CeldaSoloMonto(item.co_facturacion + item.co_cobranza, normal));

		//		// Cta. Cte.
		//		tabla.AddCell(CeldaPorcMonto(item.co_ctacte_porc, item.co_ctacte, chico, normal));

		//		// Cta. Cte. Dist.
		//		tabla.AddCell(CeldaPorcMonto(item.co_ctacte_dist_porc, item.co_ctacte_dist, chico, normal));

		//		// Efectivo
		//		tabla.AddCell(CeldaDual(
		//			item.efectivos_porc, item.efectivos,
		//			item.efectivos_cant_porc, item.efectivos_cant,
		//			chico, normal));

		//		// Tarjetas
		//		tabla.AddCell(CeldaDual(
		//			item.tarjetas_porc, item.tarjetas,
		//			item.tarjetas_cant_porc, item.tarjetas_cant,
		//			chico, normal));

		//		// Transferencias
		//		tabla.AddCell(CeldaDual(
		//			item.bco_transf_porc, item.bco_transf,
		//			item.bco_transf_cant_porc, item.bco_transf_cant,
		//			chico, normal));

		//		// Mutuales
		//		tabla.AddCell(CeldaDual(
		//			item.mutuales_porc, item.mutuales,
		//			item.mutuales_cant_porc, item.mutuales_cant,
		//			chico, normal));

		//		// Vales
		//		tabla.AddCell(CeldaDual(
		//			item.vales_porc, item.vales,
		//			item.vales_cant_porc, item.vales_cant,
		//			chico, normal));

		//		// Otros
		//		tabla.AddCell(CeldaDual(
		//			item.otros_porc, item.otros,
		//			item.otros_cant_porc, item.otros_cant,
		//			chico, normal));
		//	}

		//	pdf.Add(tabla);
		//}

		public static void CargarRepoAnalisisDeValoresDeVentaDiario(
				Document pdf,
				List<AnaValDeVtaDetDiarioDto> registros,
				Font chico,
				Font normal,
				Font normalBold,
				Font titulo,
				Font tituloBig,
				int tabId
			)
		{
			if (registros == null || !registros.Any())
			{
				pdf.Add(new Paragraph("No hay datos para mostrar", normalBold));
				return;
			}

			// Título
			string tituloTexto = tabId == 1
				? "Análisis de Valores de Venta - Diario (Importes)"
				: "Análisis de Valores de Venta - Diario (Cantidades)";

			Paragraph tituloPar = new Paragraph(tituloTexto, tituloBig);
			tituloPar.Alignment = Element.ALIGN_CENTER;
			tituloPar.SpacingAfter = 10f;
			pdf.Add(tituloPar);

			if (tabId == 1)
				DibujarTablaImportes(pdf, registros, chico, normal, normalBold);
			else
				DibujarTablaCantidades(pdf, registros, chico, normal, normalBold);
		}



		public static void CargarRepoAnalisisDeValoresDeVentaPV(Document pdf, List<AnaValDeVtaDetPVDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || !registros.Any())
			{
				pdf.Add(new Paragraph("No hay datos para mostrar", normalBold));
				return;
			}

			// Título
			Paragraph tituloPar = new Paragraph("Análisis de Valores de Venta - Detalle por Punto de Venta", tituloBig);
			tituloPar.Alignment = Element.ALIGN_CENTER;
			tituloPar.SpacingAfter = 10f;
			pdf.Add(tituloPar);

			// Definición de columnas
			float[] widths = { 1.6f, 2.5f, 1.4f, 2f, 2f, 2f, 2f, 2f, 2f };
			PdfPTable tabla = new PdfPTable(widths);
			tabla.WidthPercentage = 100;

			// Encabezados
			string[] headers = {
				"Día",
				"Suc.",
				"PV",
				"Efectivo",
				"Tarjetas",
				"Transf. Bco.",
				"Mutual",
				"Vales",
				"Otros"
			};

			foreach (var h in headers)
			{
				PdfPCell celda = new PdfPCell(new Phrase(h, normalBold));
				celda.HorizontalAlignment = Element.ALIGN_CENTER;
				celda.BackgroundColor = new BaseColor(230, 230, 230);
				celda.Padding = 4;
				tabla.AddCell(celda);
			}

			// Filas
			foreach (var item in registros)
			{
				// Día
				tabla.AddCell(new PdfPCell(new Phrase(item.dia.ToString("dd/MM/yyyy"), normal)));

				// Sucursal
				PdfPCell suc = new PdfPCell(new Phrase(item.adm_nombre, normal));
				suc.HorizontalAlignment = Element.ALIGN_LEFT;
				tabla.AddCell(suc);

				// PV
				PdfPCell pv = new PdfPCell(new Phrase(item.caja_id, normal));
				pv.HorizontalAlignment = Element.ALIGN_CENTER;
				tabla.AddCell(pv);

				// Efectivo
				tabla.AddCell(CeldaSoloMonto(item.efectivos, normal));

				// Tarjetas
				tabla.AddCell(CeldaSoloMonto(item.tarjetas, normal));

				// Transferencias
				tabla.AddCell(CeldaSoloMonto(item.bco_transf, normal));

				// Mutual
				tabla.AddCell(CeldaSoloMonto(item.mutuales, normal));

				// Vales
				tabla.AddCell(CeldaSoloMonto(item.vales, normal));

				// Otros
				tabla.AddCell(CeldaSoloMonto(item.otros, normal));
			}

			pdf.Add(tabla);
		}

		public static void CargarRepoAnalisisDeValoresDeVentaCB(Document pdf, List<AnaValDeVtaDetCBDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || !registros.Any())
			{
				pdf.Add(new Paragraph("No hay datos para mostrar", normalBold));
				return;
			}

			// Ordenar como en el HTML
			registros = registros.OrderBy(x => x.caja_nro_proceso).ToList();

			// Título
			Paragraph tituloPar = new Paragraph("Análisis de Valores de Venta - Cashback", tituloBig);
			tituloPar.Alignment = Element.ALIGN_CENTER;
			tituloPar.SpacingAfter = 10f;
			pdf.Add(tituloPar);

			// Definición de columnas
			float[] widths = { 1.6f, 2.5f, 2.5f, 3.5f, 2f, 2f };
			PdfPTable tabla = new PdfPTable(widths);
			tabla.WidthPercentage = 100;

			// Encabezados
			string[] headers = {
				"Día",
				"Suc.",
				"Medio de Pago",
				"Datos",
				"CashBack",
				"Importe"
			};

			foreach (var h in headers)
			{
				PdfPCell celda = new PdfPCell(new Phrase(h, normalBold));
				celda.HorizontalAlignment = Element.ALIGN_CENTER;
				celda.BackgroundColor = new BaseColor(230, 230, 230);
				celda.Padding = 4;
				tabla.AddCell(celda);
			}

			string grupoAnterior = null;

			// Filas
			foreach (var item in registros)
			{
				string grupoActual = item.caja_nro_proceso;

				// Si cambia el grupo → insertar fila de encabezado
				if (grupoActual != grupoAnterior)
				{
					PdfPCell header = new PdfPCell(
						new Phrase(
							$"Proceso N° {grupoActual} - {item.caja_habilitacion:dd/MM/yyyy}",
							normalBold
						)
					);

					header.Colspan = 6;
					header.BackgroundColor = new BaseColor(220, 220, 220);
					header.HorizontalAlignment = Element.ALIGN_CENTER;
					header.Padding = 5;
					tabla.AddCell(header);

					grupoAnterior = grupoActual;
				}

				// Día
				tabla.AddCell(new PdfPCell(new Phrase(item.rb_fecha_valor.ToString("dd/MM/yyyy"), normal)));

				// Sucursal
				PdfPCell suc = new PdfPCell(new Phrase(item.adm_nombre, normal));
				suc.HorizontalAlignment = Element.ALIGN_LEFT;
				tabla.AddCell(suc);

				// Medio de Pago
				PdfPCell mp = new PdfPCell(new Phrase(item.ins_desc, normal));
				mp.HorizontalAlignment = Element.ALIGN_LEFT;
				tabla.AddCell(mp);

				// Datos (dato1 + dato2 + dato3)
				string datos = $"{item.rb_dato1_valor} {item.rb_dato2_valor} {item.rb_dato3_valor}".Trim();
				PdfPCell datosCell = new PdfPCell(new Phrase(datos, normal));
				datosCell.HorizontalAlignment = Element.ALIGN_LEFT;
				tabla.AddCell(datosCell);

				// Cashback
				tabla.AddCell(CeldaSoloMonto(item.cashback, normal));

				// Importe
				tabla.AddCell(CeldaSoloMonto(item.rb_importe, normal));
			}

			pdf.Add(tabla);
		}

		public static void CargarRepoSorteoCompteLista(Document pdf, List<SorteoComptesDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || !registros.Any())
			{
				Paragraph sinDatos = new Paragraph("No hay datos para mostrar.", normalBold);
				sinDatos.Alignment = Element.ALIGN_CENTER;
				pdf.Add(sinDatos);
				return;
			}

			// ---------------------------
			// TÍTULO
			// ---------------------------
			Paragraph tituloReporte = new Paragraph("Análisis de Valores de Venta - Comprobantes del Sorteo", tituloBig);
			tituloReporte.Alignment = Element.ALIGN_CENTER;
			tituloReporte.SpacingAfter = 12f;
			pdf.Add(tituloReporte);

			// ---------------------------
			// TABLA
			// ---------------------------
			PdfPTable tabla = new PdfPTable(7);
			tabla.WidthPercentage = 100;
			tabla.HeaderRows = 1;

			// Anchos proporcionales
			tabla.SetWidths(new float[] { 12f, 18f, 14f, 26f, 10f, 10f, 20f });

			// ---------------------------
			// ENCABEZADOS
			// ---------------------------
			AgregarCeldaHeader(tabla, "N° Asignado", normalBold);
			AgregarCeldaHeader(tabla, "Tipo", normalBold);
			AgregarCeldaHeader(tabla, "N° Comprobante", normalBold);
			AgregarCeldaHeader(tabla, "Cliente", normalBold);
			AgregarCeldaHeader(tabla, "Proceso", normalBold);
			AgregarCeldaHeader(tabla, "Cierre", normalBold);
			AgregarCeldaHeader(tabla, "Sucursal", normalBold);

			// ---------------------------
			// FILAS
			// ---------------------------
			bool alt = true;

			foreach (var item in registros)
			{
				BaseColor bg = alt ? new BaseColor(245, 245, 245) : BaseColor.White;
				alt = !alt;

				AgregarCelda(tabla, item.so_nro, normal, Element.ALIGN_CENTER, bg);
				AgregarCelda(tabla, item.tco_desc, normal, Element.ALIGN_LEFT, bg);
				AgregarCelda(tabla, item.cm_compte, normal, Element.ALIGN_CENTER, bg);
				AgregarCelda(tabla, item.cta_denominacion, normal, Element.ALIGN_LEFT, bg);
				AgregarCelda(tabla, item.caja_nro_proceso, normal, Element.ALIGN_CENTER, bg);
				AgregarCelda(tabla, item.caja_nro_cierre, normal, Element.ALIGN_CENTER, bg);
				AgregarCelda(tabla, item.adm_nombre, normal, Element.ALIGN_LEFT, bg);
			}

			pdf.Add(tabla);
		}

		public static void CargarRepoSorteoAnalisisProdLista(Document pdf, List<SorteoAnalisisProdDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || !registros.Any())
			{
				Paragraph sinDatos = new Paragraph("No hay datos para mostrar.", normalBold);
				sinDatos.Alignment = Element.ALIGN_CENTER;
				pdf.Add(sinDatos);
				return;
			}

			// ---------------------------
			// TÍTULO
			// ---------------------------
			Paragraph tituloReporte = new Paragraph("Análisis de Productos del Sorteo", tituloBig);
			tituloReporte.Alignment = Element.ALIGN_CENTER;
			tituloReporte.SpacingAfter = 12f;
			pdf.Add(tituloReporte);

			// ---------------------------
			// TABLA
			// ---------------------------
			PdfPTable tabla = new PdfPTable(9);
			tabla.WidthPercentage = 100;
			tabla.HeaderRows = 1;

			// Anchos proporcionales
			tabla.SetWidths(new float[] { 14f, 10f, 26f, 14f, 22f, 10f, 14f, 12f, 14f });

			// ---------------------------
			// ENCABEZADOS
			// ---------------------------
			AgregarCeldaHeader(tabla, "Sucursal", normalBold);
			AgregarCeldaHeader(tabla, "ID", normalBold);
			AgregarCeldaHeader(tabla, "Descripción", normalBold);
			AgregarCeldaHeader(tabla, "EAN", normalBold);
			AgregarCeldaHeader(tabla, "Proveedor", normalBold);
			AgregarCeldaHeader(tabla, "Cant. Comptes", normalBold);
			AgregarCeldaHeader(tabla, "Importe Comptes", normalBold);
			AgregarCeldaHeader(tabla, "Cant. Productos", normalBold);
			AgregarCeldaHeader(tabla, "Importe Productos", normalBold);

			// ---------------------------
			// FILAS
			// ---------------------------
			bool alt = true;

			foreach (var item in registros)
			{
				BaseColor bg = alt ? new BaseColor(245, 245, 245) : BaseColor.White;
				alt = !alt;

				AgregarCelda(tabla, item.adm_nombre, normal, Element.ALIGN_CENTER, bg);
				AgregarCelda(tabla, item.p_id, normal, Element.ALIGN_CENTER, bg);
				AgregarCelda(tabla, item.p_desc, normal, Element.ALIGN_LEFT, bg);
				AgregarCelda(tabla, item.p_id_barrado, normal, Element.ALIGN_CENTER, bg);
				AgregarCelda(tabla, item.cta_denominacion, normal, Element.ALIGN_LEFT, bg);

				AgregarCelda(tabla, item.cant_comptes.ToString("N0"), normal, Element.ALIGN_RIGHT, bg);
				AgregarCelda(tabla, item.importe_comptes.ToString("N2"), normal, Element.ALIGN_RIGHT, bg);

				AgregarCelda(tabla, item.cant_prod.ToString("N0"), normal, Element.ALIGN_RIGHT, bg);
				AgregarCelda(tabla, item.importe_prod.ToString("N2"), normal, Element.ALIGN_RIGHT, bg);
			}

			pdf.Add(tabla);
		}

		// ======================================================
		// HELPERS
		// ======================================================

		private static void AgregarCeldaHeader(PdfPTable tabla, string texto, Font font)
		{
			PdfPCell celda = new PdfPCell(new Phrase(texto, font));
			celda.HorizontalAlignment = Element.ALIGN_CENTER;
			celda.VerticalAlignment = Element.ALIGN_MIDDLE;
			celda.BackgroundColor = new BaseColor(230, 230, 230);
			celda.Padding = 4f;
			tabla.AddCell(celda);
		}

		private static void AgregarCelda(PdfPTable tabla, string texto, Font font, int align, BaseColor bg)
		{
			PdfPCell celda = new PdfPCell(new Phrase(texto ?? "", font));
			celda.HorizontalAlignment = align;
			celda.VerticalAlignment = Element.ALIGN_MIDDLE;
			celda.BackgroundColor = bg;
			celda.Padding = 4f;
			tabla.AddCell(celda);
		}


		private static void DibujarTablaImportes(Document pdf, List<AnaValDeVtaDetDiarioDto> registros, Font chico, Font normal, Font normalBold)
		{
			float[] widths = { 1.6f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f };
			PdfPTable tabla = new PdfPTable(widths);
			tabla.WidthPercentage = 100;

			string[] headers = {
				"Día",
				"Fact. + Cob.",
				"Cta. Cte.",
				"Cta. Cte. Dist.",
				"Efectivo",
				"Tarjetas",
				"Transf. Bco.",
				"Mutuales",
				"Vales",
				"Otros"
			};

			foreach (var h in headers)
			{
				PdfPCell celda = new PdfPCell(new Phrase(h, normalBold));
				celda.HorizontalAlignment = Element.ALIGN_CENTER;
				celda.BackgroundColor = new BaseColor(230, 230, 230);
				celda.Padding = 4;
				tabla.AddCell(celda);
			}

			foreach (var item in registros)
			{
				tabla.AddCell(CeldaDia(item.dia, normal));
				tabla.AddCell(CeldaSoloMonto(item.co_facturacion + item.co_cobranza, normal));
				tabla.AddCell(CeldaPorcMonto(item.co_ctacte_porc, item.co_ctacte, chico, normal));
				tabla.AddCell(CeldaPorcMonto(item.co_ctacte_dist_porc, item.co_ctacte_dist, chico, normal));
				tabla.AddCell(CeldaPorcMonto(item.efectivos_porc, item.efectivos, chico, normal));
				tabla.AddCell(CeldaPorcMonto(item.tarjetas_porc, item.tarjetas, chico, normal));
				tabla.AddCell(CeldaPorcMonto(item.bco_transf_porc, item.bco_transf, chico, normal));
				tabla.AddCell(CeldaPorcMonto(item.mutuales_porc, item.mutuales, chico, normal));
				tabla.AddCell(CeldaPorcMonto(item.vales_porc, item.vales, chico, normal));
				tabla.AddCell(CeldaPorcMonto(item.otros_porc, item.otros, chico, normal));
			}

			pdf.Add(tabla);
		}

		private static void DibujarTablaCantidades(Document pdf, List<AnaValDeVtaDetDiarioDto> registros, Font chico, Font normal, Font normalBold)
		{
			float[] widths = { 1.6f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f };
			PdfPTable tabla = new PdfPTable(widths);
			tabla.WidthPercentage = 100;

			string[] headers = {
				"Día",
				"Fact. + Cob.",
				"Cta. Cte.",
				"Cta. Cte. Dist.",
				"Efectivo",
				"Tarjetas",
				"Transf. Bco.",
				"Mutuales",
				"Vales",
				"Otros"
			};

			foreach (var h in headers)
			{
				PdfPCell celda = new PdfPCell(new Phrase(h, normalBold));
				celda.HorizontalAlignment = Element.ALIGN_CENTER;
				celda.BackgroundColor = new BaseColor(230, 230, 230);
				celda.Padding = 4;
				tabla.AddCell(celda);
			}

			foreach (var item in registros)
			{
				tabla.AddCell(CeldaDia(item.dia, normal));

				// Las primeras 3 columnas no tienen cantidades
				tabla.AddCell(CeldaSoloMonto(item.co_facturacion + item.co_cobranza, normal));
				tabla.AddCell(CeldaPorcMonto(item.co_ctacte_porc, item.co_ctacte, chico, normal));
				tabla.AddCell(CeldaPorcMonto(item.co_ctacte_dist_porc, item.co_ctacte_dist, chico, normal));

				tabla.AddCell(CeldaPorcCant(item.efectivos_cant_porc, item.efectivos_cant, chico, normal));
				tabla.AddCell(CeldaPorcCant(item.tarjetas_cant_porc, item.tarjetas_cant, chico, normal));
				tabla.AddCell(CeldaPorcCant(item.bco_transf_cant_porc, item.bco_transf_cant, chico, normal));
				tabla.AddCell(CeldaPorcCant(item.mutuales_cant_porc, item.mutuales_cant, chico, normal));
				tabla.AddCell(CeldaPorcCant(item.vales_cant_porc, item.vales_cant, chico, normal));
				tabla.AddCell(CeldaPorcCant(item.otros_cant_porc, item.otros_cant, chico, normal));
			}

			pdf.Add(tabla);
		}

		private static PdfPCell CeldaDia(DateTime fecha, Font normal)
		{
			PdfPCell celda = new PdfPCell(new Phrase(fecha.ToString("dd/MM/yyyy"), normal));
			celda.HorizontalAlignment = Element.ALIGN_CENTER;
			celda.VerticalAlignment = Element.ALIGN_MIDDLE;
			celda.Padding = 4;
			return celda;
		}

		private static PdfPCell CeldaPorcCant(
			decimal porcCant,
			decimal cantidad,
			Font chico,
			Font normal
		)
		{
			// Tabla interna con 2 columnas
			PdfPTable inner = new PdfPTable(2);
			inner.WidthPercentage = 100;
			inner.SetWidths(new float[] { 1f, 1f });

			// Porcentaje (columna izquierda)
			PdfPCell c1 = new PdfPCell(new Phrase($"{porcCant}%", chico));
			c1.Border = Rectangle.NO_BORDER;
			c1.HorizontalAlignment = Element.ALIGN_LEFT;
			c1.Padding = 0;
			inner.AddCell(c1);

			// Cantidad (columna derecha)
			PdfPCell c2 = new PdfPCell(new Phrase($"{cantidad}", normal));
			c2.Border = Rectangle.NO_BORDER;
			c2.HorizontalAlignment = Element.ALIGN_RIGHT;
			c2.Padding = 0;
			inner.AddCell(c2);

			// Celda contenedora
			PdfPCell celda = new PdfPCell(inner);
			celda.Padding = 4;
			celda.HorizontalAlignment = Element.ALIGN_CENTER;
			celda.VerticalAlignment = Element.ALIGN_MIDDLE;

			return celda;
		}


		private static PdfPCell CeldaVacia()
		{
			PdfPCell celda = new PdfPCell(new Phrase(""));
			celda.Padding = 4;
			celda.HorizontalAlignment = Element.ALIGN_CENTER;
			celda.VerticalAlignment = Element.ALIGN_MIDDLE;
			return celda;
		}


		private static PdfPCell CeldaMonto(decimal monto, Font normal)
		{
			PdfPCell c = new PdfPCell();
			c.AddElement(new Phrase(GridHelper.FormatearPrecio(monto, TipoPrecio.Venta), normal));
			c.HorizontalAlignment = Element.ALIGN_RIGHT;
			c.Padding = 4;
			return c;
		}


		private static PdfPCell CeldaDual(decimal porc, decimal monto, decimal porcCant, int cant, Font chico, Font normal)
		{
			PdfPCell cell = new PdfPCell();
			cell.Padding = 4;

			PdfPTable interno = new PdfPTable(2);
			interno.WidthPercentage = 100;
			interno.SetWidths(new float[] { 1f, 1f });

			// -------------------------
			// LÍNEA 1: % + MONTO
			// -------------------------
			BaseColor bg1 = porc > 0 ? new BaseColor(201, 228, 255) :
							porc < 0 ? new BaseColor(255, 224, 224) :
									   BaseColor.White;

			BaseColor fg1 = porc > 0 ? new BaseColor(0, 74, 133) :
							porc < 0 ? new BaseColor(161, 0, 0) :
									   new BaseColor(102, 102, 102);

			PdfPCell cPorc = new PdfPCell(new Phrase(porc.ToString("0.##") + "%", chico));
			cPorc.BackgroundColor = bg1;
			cPorc.Phrase.Font.Color = fg1;
			cPorc.Border = Rectangle.NO_BORDER;
			cPorc.HorizontalAlignment = Element.ALIGN_LEFT;

			PdfPCell cMonto = new PdfPCell(new Phrase(GridHelper.FormatearPrecio(monto, TipoPrecio.Venta), normal));
			cMonto.Border = Rectangle.NO_BORDER;
			cMonto.HorizontalAlignment = Element.ALIGN_RIGHT;

			interno.AddCell(cPorc);
			interno.AddCell(cMonto);

			// Línea divisoria (simula border-bottom)
			PdfPCell divider = new PdfPCell(new Phrase(""));
			divider.Colspan = 2;
			divider.BorderWidthBottom = 0.5f;
			divider.BorderColorBottom = new BaseColor(200, 200, 200);
			divider.Border = Rectangle.BOTTOM_BORDER;
			divider.Padding = 0;
			interno.AddCell(divider);

			// -------------------------
			// LÍNEA 2: % CANT + CANTIDAD
			// -------------------------
			BaseColor bg2 = porcCant > 0 ? new BaseColor(201, 228, 255) :
							porcCant < 0 ? new BaseColor(255, 224, 224) :
										   BaseColor.White;

			BaseColor fg2 = porcCant > 0 ? new BaseColor(0, 74, 133) :
							porcCant < 0 ? new BaseColor(161, 0, 0) :
										   new BaseColor(102, 102, 102);

			PdfPCell cPorcCant = new PdfPCell(new Phrase(porcCant.ToString("0.##") + "%", chico));
			cPorcCant.BackgroundColor = bg2;
			cPorcCant.Phrase.Font.Color = fg2;
			cPorcCant.Border = Rectangle.NO_BORDER;
			cPorcCant.HorizontalAlignment = Element.ALIGN_LEFT;

			PdfPCell cCant = new PdfPCell(new Phrase(cant.ToString(), normal));
			cCant.Border = Rectangle.NO_BORDER;
			cCant.HorizontalAlignment = Element.ALIGN_RIGHT;

			interno.AddCell(cPorcCant);
			interno.AddCell(cCant);

			cell.AddElement(interno);

			return cell;
		}


		private static PdfPCell CeldaSoloMonto(decimal monto, Font normal)
		{
			//PdfPCell c = new PdfPCell();
			//c.AddElement(new Phrase(GridHelper.FormatearPrecio(monto, TipoPrecio.Venta), normal));
			//c.HorizontalAlignment = Element.ALIGN_RIGHT;
			//c.Padding = 4;
			//return c;
			PdfPCell c = new PdfPCell(new Phrase(GridHelper.FormatearPrecio(monto, TipoPrecio.Venta), normal));

			c.HorizontalAlignment = Element.ALIGN_RIGHT;
			c.VerticalAlignment = Element.ALIGN_MIDDLE; // ← clave
			c.Padding = 4;

			return c;
		}

		private static PdfPCell CeldaPorcMonto(decimal porc, decimal monto, Font chico, Font normal)
		{
			PdfPCell cell = new PdfPCell();
			cell.Padding = 1;

			PdfPTable interno = new PdfPTable(2);
			interno.WidthPercentage = 100;
			interno.SetWidths(new float[] { 30f, 70f });

			// Colores
			BaseColor bg = porc > 0 ? new BaseColor(201, 228, 255) :
							porc < 0 ? new BaseColor(255, 224, 224) :
									   BaseColor.White;

			BaseColor fg = porc > 0 ? new BaseColor(0, 74, 133) :
							porc < 0 ? new BaseColor(161, 0, 0) :
									   new BaseColor(102, 102, 102);

			// Porcentaje
			PdfPCell cPorc = new PdfPCell(new Phrase(porc.ToString("0.##") + "%", chico));
			cPorc.BackgroundColor = bg;
			cPorc.Phrase.Font.Color = fg;
			cPorc.Border = Rectangle.NO_BORDER;
			cPorc.HorizontalAlignment = Element.ALIGN_LEFT;

			// Monto
			PdfPCell cMonto = new PdfPCell(new Phrase(GridHelper.FormatearPrecio(monto, TipoPrecio.Venta), normal));
			cMonto.Border = Rectangle.NO_BORDER;
			cMonto.HorizontalAlignment = Element.ALIGN_RIGHT;

			interno.AddCell(cPorc);
			interno.AddCell(cMonto);

			cell.AddElement(interno);

			return cell;
		}



		private static void AgregarEncabezado(Document pdf, PIDetalleDto reg, Font normal, Font bold)
		{
			PdfPTable header = new(4)
			{
				WidthPercentage = 100
			};
			header.SetWidths([20f, 30f, 20f, 30f]);

			header.AddCell(new PdfPCell(new Phrase("Fecha Pedido:", bold))
			{
				Border = 0,
				HorizontalAlignment = Element.ALIGN_RIGHT
			});
			header.AddCell(new PdfPCell(new Phrase(reg.pi_fecha.ToString("dd/MM/yyyy"), normal))
			{
				Border = 0,
				HorizontalAlignment = Element.ALIGN_LEFT
			});

			header.AddCell(new PdfPCell(new Phrase("Solicitado Por:", bold))
			{
				Border = 0,
				HorizontalAlignment = Element.ALIGN_RIGHT
			});
			header.AddCell(new PdfPCell(new Phrase(reg.usu_apellidoynombre, normal))
			{
				Border = 0,
				HorizontalAlignment = Element.ALIGN_LEFT
			});

			header.SpacingAfter = 10f;

			pdf.Add(header);
		}

		private static bool writerFitsNewPage(Document pdf, PdfPTable tabla)
		{
			return tabla.TotalHeight > (pdf.PageSize.Height - pdf.TopMargin - pdf.BottomMargin - 100);
		}


		private static PdfPCell CeldaHeaderVisible(string texto, Font font, BaseColor fondo, int align = Element.ALIGN_LEFT)
		{
			return new PdfPCell(new Phrase(texto, font))
			{
				BackgroundColor = fondo,
				Border = Rectangle.BOX,
				PaddingTop = 4,
				PaddingBottom = 4,
				HorizontalAlignment = align
			};
		}

		private static PdfPCell CeldaDatoVisible(string texto, Font font, int align = Element.ALIGN_LEFT)
		{
			return new PdfPCell(new Phrase(texto, font))
			{
				Border = Rectangle.BOX,
				PaddingTop = 3,
				PaddingBottom = 3,
				HorizontalAlignment = align
			};
		}



		private static void AgregarSeparador(Document pdf, Font chico)
		{
			PdfPTable sep = new PdfPTable(1);
			sep.WidthPercentage = 100;

			PdfPCell cellSep = new PdfPCell(new Phrase(" ", chico))
			{
				Border = Rectangle.BOTTOM_BORDER,
				BorderWidthBottom = 1f,
				PaddingTop = 4,
				PaddingBottom = 4
			};

			sep.AddCell(cellSep);
			pdf.Add(sep);
		}

		//private static void AgregarCeldaHeader(PdfPTable tabla, string texto, Font font)
		//{
		//	PdfPCell celda = new PdfPCell(new Phrase(texto, font));
		//	celda.BackgroundColor = new BaseColor(230, 230, 230);
		//	celda.HorizontalAlignment = Element.ALIGN_CENTER;
		//	celda.Padding = 4f;
		//	tabla.AddCell(celda);
		//}


		private static PdfPCell CeldaHeader(string texto, Font f, int rowspan = 1, int colspan = 1)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, f));
			c.BackgroundColor = new BaseColor(186, 134, 11);
			c.HorizontalAlignment = Element.ALIGN_CENTER;
			c.VerticalAlignment = Element.ALIGN_MIDDLE;
			c.Rowspan = rowspan;
			c.Colspan = colspan;
			return c;
		}

		private static PdfPCell CeldaHeader(string texto, Font f, BaseColor color, int rowspan = 1, int colspan = 1)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, f));
			c.BackgroundColor = color;
			c.HorizontalAlignment = Element.ALIGN_CENTER;
			c.VerticalAlignment = Element.ALIGN_MIDDLE;
			c.Rowspan = rowspan;
			c.Colspan = colspan;
			return c;
		}

		private static PdfPCell CeldaDato(string texto, Font f, BaseColor fondo, int align = Element.ALIGN_LEFT)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, f));
			c.BackgroundColor = fondo;
			c.HorizontalAlignment = align;
			return c;
		}
		public static PdfPCell CeldaMini(string texto, Font fuente)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, fuente));
			c.HorizontalAlignment = Element.ALIGN_CENTER;
			return c;
		}

		private static PdfPCell CeldaMini(string texto, Font f, BaseColor color)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, f));
			c.HorizontalAlignment = Element.ALIGN_CENTER;
			c.BackgroundColor = color;
			return c;
		}

		private static PdfPCell CeldaMiniHeader(string texto, int colspan, Font f, BaseColor color)
		{
			PdfPCell c = new PdfPCell(new Phrase(texto, f));
			c.Colspan = colspan;
			c.HorizontalAlignment = Element.ALIGN_CENTER;
			c.BackgroundColor = color;
			return c;
		}

		private static PdfPCell Wrap(PdfPTable t)
		{
			PdfPCell c = new PdfPCell();
			c.AddElement(t);
			c.Border = Rectangle.NO_BORDER;
			return c;
		}


		public class SubrayadoCellEvent : IPdfPCellEvent
		{
			public void CellLayout(PdfPCell cell, Rectangle rect, PdfContentByte[] canvas)
			{
				PdfContentByte cb = canvas[PdfPTable.LINECANVAS];
				cb.SetLineWidth(0.8f);
				cb.MoveTo(rect.Left, rect.Bottom + 2);
				cb.LineTo(rect.Right, rect.Bottom + 2);
				cb.Stroke();
			}
		}



		// Clase auxiliar para simular un grupo sin agrupamiento
		private class AgrupacionSinGrupoComp : IGrouping<string, ProductoStkCompensadoDto>
		{
			private readonly IEnumerable<ProductoStkCompensadoDto> _items;
			public AgrupacionSinGrupoComp(IEnumerable<ProductoStkCompensadoDto> items) => _items = items;
			public string Key => "Sin agrupamiento";
			public IEnumerator<ProductoStkCompensadoDto> GetEnumerator() => _items.GetEnumerator();
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();
		}

		// Clase auxiliar para simular un grupo sin agrupamiento
		private class AgrupacionSinGrupo : IGrouping<string, ProductoStkDto>
		{
			private readonly IEnumerable<ProductoStkDto> _items;
			public AgrupacionSinGrupo(IEnumerable<ProductoStkDto> items) => _items = items;
			public string Key => "Sin agrupamiento";
			public IEnumerator<ProductoStkDto> GetEnumerator() => _items.GetEnumerator();
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();
		}


		public static PdfPTable GenerarListadoDesdeLista<T>(
			List<T> lista,
			List<string> campos,
			float[] anchoColumnas,
			Font fuente,
			bool mostrarCabecera = true,
			bool mostrarBordes = true,
			string titulo = null,
			bool ajustarAncho = true,
			BooleanDisplayFormat formatoBooleano = BooleanDisplayFormat.SiNo,
			bool mostrarTotales = false,
			bool aplicarFormatoMoneda = false
		)
		{
			PdfPTable tabla = new PdfPTable(campos.Count);
			tabla.WidthPercentage = 100;

			if (ajustarAncho && anchoColumnas != null)
				tabla.SetWidths(anchoColumnas);

			if (!string.IsNullOrEmpty(titulo))
			{
				PdfPCell celdaTitulo = new PdfPCell(new Phrase(titulo, fuente))
				{
					Colspan = campos.Count,
					HorizontalAlignment = Element.ALIGN_CENTER,
					Border = Rectangle.NO_BORDER,
					PaddingBottom = 5f
				};
				tabla.AddCell(celdaTitulo);
			}

			if (mostrarCabecera)
			{
				foreach (var campo in campos)
				{
					PdfPCell celdaCabecera = new PdfPCell(new Phrase(campo, fuente))
					{
						BackgroundColor = BaseColor.LightGray,
						HorizontalAlignment = Element.ALIGN_CENTER
					};
					tabla.AddCell(celdaCabecera);
				}
			}

			foreach (var item in lista)
			{
				for (int i = 0; i < campos.Count; i++)
				{
					var h1 = item.GetType().GetProperty(campos[2])?.GetValue(item, null);
					var h2 = item.GetType().GetProperty(campos[3])?.GetValue(item, null);
					var campo = campos[i];
					object valor = item.GetType().GetProperty(campo)?.GetValue(item, null);
					string texto = FormatearValor(valor, formatoBooleano, aplicarFormatoMoneda);

					PdfPCell celda = new(new Phrase(texto, fuente))
					{
						HorizontalAlignment = i == 1 ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT
					};

					string claseFila = (bool)h1 ? "destacado-header-1" : (bool)h2 ? "destacado-header-2" : "no-destacado";
					celda = AplicarEstiloCelda(celda, claseFila, fuente);
					tabla.AddCell(celda);
				}

			}

			return tabla;
		}
		private static string FormatearValor(object valor, BooleanDisplayFormat formatoBooleano, bool aplicarFormatoMoneda)
		{
			if (valor == null) return "";

			if (valor is bool b)
				return formatoBooleano == BooleanDisplayFormat.SiNo ? (b ? "Sí" : "No") : b.ToString();

			if (aplicarFormatoMoneda && valor is decimal d)
				return d.ToString("C", new CultureInfo("es-AR"));

			return valor.ToString();
		}

		public static void GenerarListadoAgrupado<T>(
					Document pdf,
					List<T> lista,
					List<string> campos,
					List<string> titulos,
					float[] anchos,
					string campoGrupo,
					string campoGrupoDescripcion,
					Font fuente,
					Font fuenteNegrita)
		{
			GenerarListadoAgrupado(
				pdf,
				lista,
				campos,
				titulos,
				anchos,
				campoGrupo,
				campoGrupoDescripcion,
				fuente,
				fuenteNegrita,
				totalesPorCampo: null,
				autoCalcularTotales: true,
				camposTotalizables: null
			);
		}


		public static void GenerarListadoAgrupado<T>(
	 Document pdf,
	 List<T> lista,
	 List<string> campos,
	 List<string> titulos,
	 float[] anchos,
	 string campoGrupo,
	 string campoGrupoDescripcion,
	 Font fuente,
	 Font fuenteNegrita,
	 Dictionary<string, decimal>? totalesPorCampo = null,
	 bool autoCalcularTotales = true,
	 List<string>? camposTotalizables = null)
		{
			if (lista == null || !lista.Any() || campos == null || campos.Count == 0) return;

			var cultura = new CultureInfo("es-ES");
			var propsDict = TypeDescriptor.GetProperties(typeof(T))
										  .Cast<PropertyDescriptor>()
										  .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

			var tabla = GeneraTabla(campos.Count, anchos, 100, 0, 10);

			string grupoActual = string.Empty;
			bool alternar = false;

			foreach (var item in lista)
			{
				string valorGrupo = propsDict[campoGrupo].GetValue(item)?.ToString() ?? "";
				string valorGrupoDescripcion = propsDict[campoGrupoDescripcion].GetValue(item)?.ToString() ?? "";

				if (!valorGrupo.Equals(grupoActual, StringComparison.OrdinalIgnoreCase))
				{
					grupoActual = valorGrupo;

					PdfPCell celdaGrupo = new PdfPCell(new Phrase(valorGrupoDescripcion, fuenteNegrita))
					{
						Colspan = campos.Count,
						Border = Rectangle.NO_BORDER,
						BackgroundColor = BaseColor.LightGray,
						PaddingTop = 5,
						PaddingBottom = 5
					};
					tabla.AddCell(celdaGrupo);
				}

				foreach (var campo in campos)
				{
					var prop = propsDict[campo];
					var valor = prop.GetValue(item);

					string texto = string.Empty;
					int alineacion = Element.ALIGN_LEFT;

					if (valor == null)
					{
						texto = "-";
					}
					else if (valor is DateTime dt)
					{
						texto = dt.ToString("dd/MM/yyyy");
						alineacion = Element.ALIGN_CENTER;
					}
					else if (valor is decimal or double or float)
					{
						texto = Convert.ToDecimal(valor).ToString("N2", cultura);
						alineacion = Element.ALIGN_RIGHT;
					}
					else
					{
						texto = valor.ToString() ?? string.Empty;
						alineacion = texto.Length == 1 ? Element.ALIGN_CENTER : Element.ALIGN_LEFT;
					}

					var parrafo = GeneraParrafo(texto, fuente, alineacion, 3, 3, true, BaseColor.Black);
					var celda = new PdfPCell(parrafo)
					{
						HorizontalAlignment = alineacion,
						Border = Rectangle.NO_BORDER,
						BackgroundColor = alternar ? BaseColor.White : new BaseColor(245, 245, 245)
					};

					tabla.AddCell(celda);
				}

				alternar = !alternar;
			}

			if (autoCalcularTotales && (totalesPorCampo == null || totalesPorCampo.Count == 0))
			{
				totalesPorCampo = new Dictionary<string, decimal>();

				foreach (var campo in campos)
				{
					if (!propsDict.ContainsKey(campo)) continue;

					if (camposTotalizables != null && !camposTotalizables.Contains(campo)) continue;

					var tipo = propsDict[campo].PropertyType;
					if (tipo == typeof(decimal) || tipo == typeof(double) || tipo == typeof(float))
					{
						decimal suma = lista.Sum(item =>
						{
							var val = propsDict[campo].GetValue(item);
							return val != null ? Convert.ToDecimal(val) : 0;
						});
						totalesPorCampo[campo] = suma;
					}
				}
			}

			if (totalesPorCampo != null && totalesPorCampo.Count > 0)
			{
				int idxPrimerTotal = campos.FindIndex(c => totalesPorCampo.ContainsKey(c));

				for (int i = 0; i < campos.Count; i++)
				{
					PdfPCell celda;

					if (totalesPorCampo.TryGetValue(campos[i], out var total))
					{
						string valorFormateado = total.ToString("N2", cultura);
						var parrafo = GeneraParrafo(valorFormateado, fuenteNegrita, Element.ALIGN_RIGHT, 5, 5, true, BaseColor.Black);
						celda = new PdfPCell(parrafo)
						{
							HorizontalAlignment = Element.ALIGN_RIGHT,
							BackgroundColor = BaseColor.LightGray,
							Border = Rectangle.BOTTOM_BORDER,
							BorderColorBottom = BaseColor.Black
						};
					}
					else if (i == idxPrimerTotal - 1)
					{
						var parrafo = GeneraParrafo("Total:", fuenteNegrita, Element.ALIGN_RIGHT, 5, 5, true, BaseColor.Black);
						celda = new PdfPCell(parrafo)
						{
							HorizontalAlignment = Element.ALIGN_RIGHT,
							BackgroundColor = BaseColor.LightGray,
							Border = Rectangle.BOTTOM_BORDER,
							BorderColorBottom = BaseColor.Black
						};
					}
					else
					{
						celda = new PdfPCell(new Phrase(""))
						{
							BackgroundColor = BaseColor.LightGray,
							Border = Rectangle.BOTTOM_BORDER,
							BorderColorBottom = BaseColor.Black
						};
					}

					tabla.AddCell(celda);
				}
			}

			pdf.Add(tabla);
		}


		public static void PresentarDatosCuentaTablaMarco(Document pdf, CuentaDto cuenta, Font fuenteEtiqueta, Font fuenteValor)
		{
			PdfPTable tabla = new PdfPTable(4);
			tabla.SetWidths(new float[] { 25f, 25f, 25f, 25f });
			tabla.WidthPercentage = 100;
			tabla.SpacingBefore = 10;
			tabla.SpacingAfter = 10;
			tabla.DefaultCell.Border = Rectangle.NO_BORDER;

			BaseColor grisFondo = new BaseColor(240, 240, 240);

			// Fila 1
			tabla.AddCell(CeldaEtiqueta("Proveedor:", fuenteEtiqueta, grisFondo));
			tabla.AddCell(CeldaDato($"({cuenta.Cta_Id}) {cuenta.Cta_Denominacion}", fuenteValor));
			tabla.AddCell(CeldaEtiqueta("Fecha Recepción:", fuenteEtiqueta, grisFondo));
			tabla.AddCell(CeldaDato(DateTime.Today.ToString("dd/MM/yyyy"), fuenteValor));

			// Fila 2
			tabla.AddCell(CeldaEtiqueta("Comprobante:", fuenteEtiqueta, grisFondo));
			tabla.AddCell(CeldaDato("Factura Nº 0001-00008964", fuenteValor));
			tabla.AddCell(CeldaEtiqueta("Fecha Comprobante:", fuenteEtiqueta, grisFondo));
			tabla.AddCell(CeldaDato("20/08/2020", fuenteValor));

			// Fila 3
			tabla.AddCell(CeldaEtiqueta("Sucursal de Descarga:", fuenteEtiqueta, grisFondo));
			tabla.AddCell(CeldaDato("Santa Lucia", fuenteValor));
			tabla.AddCell(CeldaEtiqueta("Depósito:", fuenteEtiqueta, grisFondo));
			tabla.AddCell(CeldaDato("Salon de Ventas Santa L.", fuenteValor));

			// Encapsular la tabla en una celda contenedora con borde
			PdfPTable tablaContenedora = new PdfPTable(1);
			tablaContenedora.WidthPercentage = 100;
			var celdaContenedor = new PdfPCell(tabla)
			{
				Border = Rectangle.BOX,
				Padding = 5
			};
			tablaContenedora.AddCell(celdaContenedor);

			pdf.Add(tablaContenedora);
		}

		// Helpers

		private static PdfPCell CeldaEtiqueta(string texto, Font fuente, BaseColor fondo)
		{
			return new PdfPCell(new Phrase(texto, fuente))
			{
				Border = Rectangle.NO_BORDER,
				BackgroundColor = fondo,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				Padding = 4
			};
		}

		private static PdfPCell CeldaDato(string texto, Font fuente)
		{
			return new PdfPCell(new Phrase(texto, fuente))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				Padding = 4
			};
		}

		public static void ConfigurarPieDePaginaPersonalizado(PdfWriter writer, string textoPersonalizado = "")
		{
			CustomPdfPageEventHelper evento = new CustomPdfPageEventHelper(textoPersonalizado);
			writer.PageEvent = evento;
		}
		public static void ConfigurarPieDePaginaPersonalizado(PdfWriter writer, string textoPersonalizado = "", float margenInferior = 15)
		{
			CustomPdfPageEventHelper evento = new CustomPdfPageEventHelper(textoPersonalizado)
			{
				MargenInferior = margenInferior
			};
			writer.PageEvent = evento;
		}

		private static NumberFormatInfo ForzarObtenerFormatoMonetario()
		{
			CultureInfo culturaArgentina = CultureInfo.CreateSpecificCulture("es-AR");
			NumberFormatInfo formatoPersonalizado = culturaArgentina.NumberFormat.Clone() as NumberFormatInfo;
			formatoPersonalizado.CurrencySymbol = "$";                // Cambia el símbolo
			formatoPersonalizado.CurrencyDecimalSeparator = ",";         // Separador decimal
			formatoPersonalizado.CurrencyGroupSeparator = ".";           // Separador de miles
			formatoPersonalizado.CurrencyDecimalDigits = 2;              // Cantidad de decimales
			formatoPersonalizado.CurrencyNegativePattern = 1;            // Muestra negativos como "-ARS$ 1.234,56"
			return formatoPersonalizado;
		}

		private static void ProcesarAgrupamientoJerarquico<T>(
	PdfPTable tabla,
	List<T> lista,
	List<string> campos,
	List<NivelAgrupamiento> nivelesAgrupamiento,
	Dictionary<string, PropertyDescriptor> propsDict,
	Font fuente,
	Font fuenteNegrita,
	CultureInfo cultura,
	bool autoCalcularTotales,
	List<string>? camposTotalizables)
		{
			// Ordenar lista por todos los niveles de agrupamiento
			var listaOrdenada = OrdenarPorNiveles(lista, nivelesAgrupamiento, propsDict);

			// Diccionario para rastrear valores actuales de cada nivel
			var valoresActuales = new Dictionary<int, string>();

			// Diccionario para acumular subtotales por nivel
			var subtotalesPorNivel = new Dictionary<int, Dictionary<string, decimal>>();

			bool alternar = false;

			foreach (var item in listaOrdenada)
			{
				// Verificar cambios en cada nivel de agrupamiento
				bool cambioEnAlgunNivel = false;
				int nivelCambio = -1;

				for (int i = 0; i < nivelesAgrupamiento.Count; i++)
				{
					var nivel = nivelesAgrupamiento[i];
					string valorActual = propsDict[nivel.CampoGrupo].GetValue(item)?.ToString() ?? "";

					if (!valoresActuales.ContainsKey(i) ||
						!valorActual.Equals(valoresActuales[i], StringComparison.OrdinalIgnoreCase))
					{
						// Hubo cambio en este nivel
						if (cambioEnAlgunNivel == false)
						{
							nivelCambio = i;
							cambioEnAlgunNivel = true;
						}

						// Si cambió un nivel superior, mostrar subtotales de niveles inferiores
						if (i < nivelesAgrupamiento.Count - 1 && valoresActuales.ContainsKey(i))
						{
							MostrarSubtotalesNivelesInferiores(
								tabla,
								campos,
								nivelesAgrupamiento,
								subtotalesPorNivel,
								i,
								fuenteNegrita,
								cultura
							);
						}

						// Actualizar valor actual de este nivel
						valoresActuales[i] = valorActual;

						// Limpiar valores de niveles inferiores
						for (int j = i + 1; j < nivelesAgrupamiento.Count; j++)
						{
							valoresActuales.Remove(j);
							subtotalesPorNivel.Remove(j);
						}

						// Mostrar header de grupo para este nivel
						MostrarHeaderGrupo(tabla, item, nivel, propsDict, campos.Count, fuenteNegrita);

						// Inicializar acumulador de subtotales para este nivel
						if (nivel.MostrarSubtotal)
						{
							subtotalesPorNivel[i] = new Dictionary<string, decimal>();
						}

						// Resetear alternancia de color
						alternar = false;
					}
				}

				// Agregar fila de datos
				AgregarFilaDatos(tabla, item, campos, propsDict, fuente, cultura, alternar);
				alternar = !alternar;

				// Acumular en subtotales de cada nivel
				if (autoCalcularTotales)
				{
					AcumularSubtotales(item, campos, camposTotalizables, propsDict,
									 subtotalesPorNivel, nivelesAgrupamiento);
				}
			}

			// Mostrar subtotales finales de todos los niveles
			MostrarSubtotalesNivelesInferiores(
				tabla,
				campos,
				nivelesAgrupamiento,
				subtotalesPorNivel,
				-1, // Mostrar todos los niveles
				fuenteNegrita,
				cultura
			);
		}

		private static List<T> OrdenarPorNiveles<T>(
	List<T> lista,
	List<NivelAgrupamiento> niveles,
	Dictionary<string, PropertyDescriptor> propsDict)
		{
			IOrderedEnumerable<T>? query = null;

			for (int i = 0; i < niveles.Count; i++)
			{
				var campo = niveles[i].CampoGrupo;
				var prop = propsDict[campo];

				if (i == 0)
				{
					query = lista.OrderBy(item => prop.GetValue(item)?.ToString() ?? "");
				}
				else
				{
					query = query!.ThenBy(item => prop.GetValue(item)?.ToString() ?? "");
				}
			}

			return query?.ToList() ?? lista;
		}

		private static void MostrarHeaderGrupo<T>(
			PdfPTable tabla,
			T item,
			NivelAgrupamiento nivel,
			Dictionary<string, PropertyDescriptor> propsDict,
			int colspan,
			Font fuenteNegrita)
		{
			string descripcion = propsDict[nivel.CampoDescripcion].GetValue(item)?.ToString() ?? "";

			// Determinar color de fondo basado en nivel jerárquico
			BaseColor colorFondo = nivel.ColorFondo ?? ObtenerColorPorNivel(nivel.Nivel);

			// Calcular indentación visual según nivel
			string indentacion = new string(' ', nivel.Nivel * 2);

			PdfPCell celdaGrupo = new PdfPCell(new Phrase($"{indentacion}{descripcion}", fuenteNegrita))
			{
				Colspan = colspan,
				Border = Rectangle.NO_BORDER,
				BackgroundColor = colorFondo,
				PaddingTop = 5 + (nivel.Nivel * 2),
				PaddingBottom = 5 + (nivel.Nivel * 2),
				PaddingLeft = 5 + (nivel.Nivel * 5)
			};

			tabla.AddCell(celdaGrupo);
		}

		private static BaseColor ObtenerColorPorNivel(int nivel)
		{
			// Degradado de grises según profundidad
			return nivel switch
			{
				0 => new BaseColor(200, 200, 200), // Gris oscuro
				1 => new BaseColor(220, 220, 220), // Gris medio
				2 => new BaseColor(235, 235, 235), // Gris claro
				_ => BaseColor.LightGray
			};
		}

		private static void AgregarFilaDatos<T>(
			PdfPTable tabla,
			T item,
			List<string> campos,
			Dictionary<string, PropertyDescriptor> propsDict,
			Font fuente,
			CultureInfo cultura,
			bool alternar)
		{
			foreach (var campo in campos)
			{
				var prop = propsDict[campo];
				var valor = prop.GetValue(item);

				string texto = FormatearValor(valor, cultura);
				int alineacion = DeterminarAlineacion(valor);

				var parrafo = HelperPdf.GeneraParrafo(texto, fuente, alineacion, 3, 3, true, BaseColor.Black);
				var celda = new PdfPCell(parrafo)
				{
					HorizontalAlignment = alineacion,
					Border = Rectangle.NO_BORDER,
					BackgroundColor = alternar ? BaseColor.White : new BaseColor(245, 245, 245)
				};

				tabla.AddCell(celda);
			}
		}

		private static void AcumularSubtotales<T>(
			T item,
			List<string> campos,
			List<string>? camposTotalizables,
			Dictionary<string, PropertyDescriptor> propsDict,
			Dictionary<int, Dictionary<string, decimal>> subtotalesPorNivel,
			List<NivelAgrupamiento> niveles)
		{
			for (int nivelIdx = 0; nivelIdx < niveles.Count; nivelIdx++)
			{
				if (!niveles[nivelIdx].MostrarSubtotal)
					continue;

				if (!subtotalesPorNivel.ContainsKey(nivelIdx))
					subtotalesPorNivel[nivelIdx] = new Dictionary<string, decimal>();

				foreach (var campo in campos)
				{
					if (!propsDict.ContainsKey(campo))
						continue;

					if (camposTotalizables != null && !camposTotalizables.Contains(campo))
						continue;

					var tipo = propsDict[campo].PropertyType;
					if (tipo == typeof(decimal) || tipo == typeof(double) || tipo == typeof(float))
					{
						var valor = propsDict[campo].GetValue(item);
						decimal valorDecimal = valor != null ? Convert.ToDecimal(valor) : 0;

						if (!subtotalesPorNivel[nivelIdx].ContainsKey(campo))
							subtotalesPorNivel[nivelIdx][campo] = 0;

						subtotalesPorNivel[nivelIdx][campo] += valorDecimal;
					}
				}
			}
		}

		private static void MostrarSubtotalesNivelesInferiores(
			PdfPTable tabla,
			List<string> campos,
			List<NivelAgrupamiento> niveles,
			Dictionary<int, Dictionary<string, decimal>> subtotalesPorNivel,
			int nivelDesde,
			Font fuenteNegrita,
			CultureInfo cultura)
		{
			// Mostrar subtotales de niveles inferiores al que cambió
			for (int i = niveles.Count - 1; i > nivelDesde; i--)
			{
				if (!niveles[i].MostrarSubtotal)
					continue;

				if (!subtotalesPorNivel.ContainsKey(i))
					continue;

				string etiqueta = niveles[i].EtiquetaSubtotal ?? $"Subtotal Nivel {i}:";
				AgregarFilaSubtotal(tabla, campos, subtotalesPorNivel[i], etiqueta,
								  fuenteNegrita, cultura, niveles[i].Nivel);

				// Limpiar acumuladores
				subtotalesPorNivel.Remove(i);
			}
		}

		private static void AgregarFilaSubtotal(
			PdfPTable tabla,
			List<string> campos,
			Dictionary<string, decimal> subtotales,
			string etiqueta,
			Font fuenteNegrita,
			CultureInfo cultura,
			int nivel)
		{
			int idxPrimerTotal = campos.FindIndex(c => subtotales.ContainsKey(c));
			BaseColor colorFondo = new BaseColor(240 - (nivel * 10), 240 - (nivel * 10), 240 - (nivel * 10));

			for (int i = 0; i < campos.Count; i++)
			{
				PdfPCell celda;

				if (subtotales.TryGetValue(campos[i], out var total))
				{
					string valorFormateado = total.ToString("N2", cultura);
					var parrafo = HelperPdf.GeneraParrafo(valorFormateado, fuenteNegrita,
														Element.ALIGN_RIGHT, 5, 5, true, BaseColor.Black);
					celda = new PdfPCell(parrafo)
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = colorFondo,
						Border = Rectangle.TOP_BORDER,
						BorderColorTop = BaseColor.Black,
						BorderWidthTop = 0.5f
					};
				}
				else if (i == idxPrimerTotal - 1)
				{
					var parrafo = HelperPdf.GeneraParrafo(etiqueta, fuenteNegrita,
														Element.ALIGN_RIGHT, 5, 5, true, BaseColor.Black);
					celda = new PdfPCell(parrafo)
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = colorFondo,
						Border = Rectangle.TOP_BORDER,
						BorderColorTop = BaseColor.Black,
						BorderWidthTop = 0.5f
					};
				}
				else
				{
					celda = new PdfPCell(new Phrase(""))
					{
						BackgroundColor = colorFondo,
						Border = Rectangle.TOP_BORDER,
						BorderColorTop = BaseColor.Black,
						BorderWidthTop = 0.5f
					};
				}

				tabla.AddCell(celda);
			}
		}

		private static void AgregarEncabezadoColumnas(
			PdfPTable tabla,
			List<string> titulos,
			int numColumnas,
			Font fuenteNegrita)
		{

			foreach (var titulo in titulos)
			{
				PdfPCell celda = new PdfPCell(new Phrase(titulo, fuenteNegrita))
				{
					BackgroundColor = BaseColor.White,
					HorizontalAlignment = Element.ALIGN_CENTER,
					Border = Rectangle.BOX,
					Padding = 5
				};
				tabla.AddCell(celda);
			}
		}

		private static void AgregarFilaTotalesGenerales(
			PdfPTable tabla,
			List<string> campos,
			Dictionary<string, decimal> totales,
			Font fuenteNegrita,
			CultureInfo cultura)
		{
			int idxPrimerTotal = campos.FindIndex(c => totales.ContainsKey(c));

			for (int i = 0; i < campos.Count; i++)
			{
				PdfPCell celda;

				if (totales.TryGetValue(campos[i], out var total))
				{
					string valorFormateado = total.ToString("N2", cultura);
					var parrafo = HelperPdf.GeneraParrafo(valorFormateado, fuenteNegrita,
														Element.ALIGN_RIGHT, 5, 5, true, BaseColor.Black);
					celda = new PdfPCell(parrafo)
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = new BaseColor(200, 200, 200),
						Border = Rectangle.BOX,
						BorderWidth = 1f
					};
				}
				else if (i == idxPrimerTotal - 1)
				{
					var parrafo = HelperPdf.GeneraParrafo("TOTAL GENERAL:", fuenteNegrita,
														Element.ALIGN_RIGHT, 5, 5, true, BaseColor.Black);
					celda = new PdfPCell(parrafo)
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						BackgroundColor = new BaseColor(200, 200, 200),
						Border = Rectangle.BOX,
						BorderWidth = 1f
					};
				}
				else
				{
					celda = new PdfPCell(new Phrase(""))
					{
						BackgroundColor = new BaseColor(200, 200, 200),
						Border = Rectangle.BOX,
						BorderWidth = 1f
					};
				}

				tabla.AddCell(celda);
			}
		}

		private static Dictionary<string, decimal> CalcularTotales<T>(
			List<T> lista,
			List<string> campos,
			List<string>? camposTotalizables,
			Dictionary<string, PropertyDescriptor> propsDict)
		{
			var totales = new Dictionary<string, decimal>();

			foreach (var campo in campos)
			{
				if (!propsDict.ContainsKey(campo))
					continue;

				if (camposTotalizables != null && !camposTotalizables.Contains(campo))
					continue;

				var tipo = propsDict[campo].PropertyType;
				if (tipo == typeof(decimal) || tipo == typeof(double) || tipo == typeof(float))
				{
					decimal suma = lista.Sum(item =>
					{
						var val = propsDict[campo].GetValue(item);
						return val != null ? Convert.ToDecimal(val) : 0;
					});
					totales[campo] = suma;
				}
			}

			return totales;
		}

		private static string FormatearValor(object? valor, CultureInfo cultura)
		{
			if (valor == null)
				return "-";

			if (valor is DateTime dt)
				return dt.ToString("dd/MM/yyyy");

			if (valor is decimal or double or float)
				return Convert.ToDecimal(valor).ToString("N2", cultura);

			return valor.ToString() ?? string.Empty;
		}

		private static int DeterminarAlineacion(object? valor)
		{
			if (valor == null)
				return Element.ALIGN_LEFT;

			if (valor is DateTime)
				return Element.ALIGN_CENTER;

			if (valor is decimal or double or float)
				return Element.ALIGN_RIGHT;

			string texto = valor.ToString() ?? "";
			return texto.Length == 1 ? Element.ALIGN_CENTER : Element.ALIGN_LEFT;
		}

		private static List<LibroBancoResumenDto> ObtenerGrillaCuentaFinanciera(List<FinancieroBcoLibroResumenDto> lista, TipoGrillaCuentaFinanciera tipoGrilla)
		{
			var listaCuentaFin = new List<LibroBancoResumenDto>();
			if (lista == null || lista.Count == 0)
				return listaCuentaFin;

			var itemFinan = lista.First();
			var item = new LibroBancoResumenDto();

			if (tipoGrilla == TipoGrillaCuentaFinanciera.CuentaFinanciera)
			{
				item = new LibroBancoResumenDto
				{
					descripcion = "Saldo Estado de Cuenta Financiera al Cierre",
					saldo = $"({(itemFinan.saldo_sis).ToString("C", ForzarObtenerFormatoMonetario()).Trim()})",
					es_fuente_negrita = true,
					background = "#D3D047",
					es_header_1 = true
				};
				listaCuentaFin.Add(item);
			}
			else
			{
				item = new LibroBancoResumenDto
				{
					descripcion = "Saldo Estado de Cuenta Banco al Cierre",
					saldo = $"{(itemFinan.saldo_ext).ToString("C", ForzarObtenerFormatoMonetario()).Trim()}",
					es_fuente_negrita = true,
					background = "#D3D047",
					es_header_1 = true
				};
				listaCuentaFin.Add(item);
			}
			var mas = itemFinan.cheques_sis + itemFinan.transferencias_h_sis + itemFinan.creditos_ext;
			item = new LibroBancoResumenDto { descripcion = (tipoGrilla == TipoGrillaCuentaFinanciera.CuentaFinanciera ? "Mas" : "Menos"), saldo = (tipoGrilla == TipoGrillaCuentaFinanciera.CuentaFinanciera ? $"{mas.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}" : $"({mas.ToString("C", ForzarObtenerFormatoMonetario()).Trim()})"), es_fuente_negrita = true, background = "#60A5F3", es_header_2 = true };
			listaCuentaFin.Add(item);
			item = new LibroBancoResumenDto { descripcion = "Cheques emitidos no conciliados en el Sistema", saldo = $"{itemFinan.cheques_sis.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}", es_fuente_negrita = false, background = "" };
			listaCuentaFin.Add(item);
			item = new LibroBancoResumenDto { descripcion = "Transferencias hacia bancos (extracciones, retiros) no conciliados en el Sistema", saldo = $"{itemFinan.transferencias_h_sis.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}", es_fuente_negrita = false, background = "" };
			listaCuentaFin.Add(item);
			item = new LibroBancoResumenDto { descripcion = "Créditos realizadios por el banco (Perc., Imp., Ret., Com., etc.) no conciliados en Extracto", saldo = $"{itemFinan.creditos_ext.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}", es_fuente_negrita = false, background = "" };
			listaCuentaFin.Add(item);
			var menos = itemFinan.depositos_sis + itemFinan.transferencias_d_sis + itemFinan.debitos_ext;
			item = new LibroBancoResumenDto { descripcion = tipoGrilla == TipoGrillaCuentaFinanciera.CuentaFinanciera ? "Menos" : "Mas", saldo = (tipoGrilla == TipoGrillaCuentaFinanciera.CuentaFinanciera ? $"({menos.ToString("C", ForzarObtenerFormatoMonetario()).Trim()})" : $"{menos.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}"), es_fuente_negrita = true, background = "#60A5F3", es_header_2 = true };
			listaCuentaFin.Add(item);
			item = new LibroBancoResumenDto { descripcion = "Cheques de terceros depositados no conciliados en Sistema", saldo = $"{itemFinan.depositos_sis.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}", es_fuente_negrita = false, background = "" };
			listaCuentaFin.Add(item);
			item = new LibroBancoResumenDto { descripcion = "Transferencias desde otros bancos (depósitos) pendientes no conciliados en el Sistema", saldo = $"{itemFinan.transferencias_d_sis.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}", es_fuente_negrita = false, background = "" };
			listaCuentaFin.Add(item);
			item = new LibroBancoResumenDto { descripcion = "Débitos realizadios por el banco (Int., Dev. de Perc., Dev. de Int., Dev. de Ret., Dev. de Com.) no conciliados en Extracto", saldo = $"{itemFinan.debitos_ext.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}", es_fuente_negrita = false, background = "" };
			listaCuentaFin.Add(item);
			var subTotal = mas - menos;
			if (tipoGrilla == TipoGrillaCuentaFinanciera.CuentaFinanciera)
			{
				item = new LibroBancoResumenDto
				{
					descripcion = "SubTotal",
					saldo = subTotal < 0 ? $"{(-1 * subTotal).ToString("C", ForzarObtenerFormatoMonetario()).Trim()}" : $"{subTotal.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}",
					es_fuente_negrita = true,
					background = "#60A5F3",
					es_header_2 = true
				};
				listaCuentaFin.Add(item);

				var saldo = itemFinan.saldo_sis + subTotal;
				if (saldo < 0) saldo *= -1;
				item = new LibroBancoResumenDto
				{
					descripcion = "Saldo Cuenta Banco al Cierre",
					saldo = $"{saldo.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}",
					es_fuente_negrita = true,
					background = "#D3D047",
					es_header_1 = true
				};
				listaCuentaFin.Add(item);
			}
			else
			{
				item = new LibroBancoResumenDto
				{
					descripcion = "SubTotal",
					saldo = subTotal < 0 ? $"({(-1 * subTotal).ToString("C", ForzarObtenerFormatoMonetario()).Trim()})" : $"({subTotal.ToString("C", ForzarObtenerFormatoMonetario()).Trim()})",
					es_fuente_negrita = true,
					background = "#60A5F3",
					es_header_2 = true
				};
				listaCuentaFin.Add(item);

				var saldo = subTotal - itemFinan.saldo_ext;
				if (saldo < 0) saldo *= -1;
				item = new LibroBancoResumenDto
				{
					descripcion = "Saldo Estado de Cuenta Financiera al Cierre",
					saldo = $"({saldo.ToString("C", ForzarObtenerFormatoMonetario()).Trim()})",
					es_fuente_negrita = true,
					background = "#D3D047",
					es_header_1 = true
				};
				listaCuentaFin.Add(item);
			}
			return listaCuentaFin;
		}

		enum TipoGrillaCuentaFinanciera
		{
			CuentaFinanciera = 1,
			CuentaBanco = 2
		}
	}

	public enum HojaSize
	{
		A1, A2, A3, A4, A5, A6
	}


	public class EventoConTabla : PdfPageEventHelper
	{
		PdfPTable tabla;

		public EventoConTabla(PdfPTable tablaARepetir)
		{
			tabla = tablaARepetir;
		}

		public override void OnEndPage(PdfWriter writer, Document document)
		{
			// Posición debajo del header
			float yPos = document.Top - 60;
			tabla.WriteSelectedRows(0, -1, document.LeftMargin, yPos, writer.DirectContent);
		}
	}

	public class CustomPdfPageEventHelper : PdfPageEventHelper
	{
		private readonly string _footerText;
		private PdfTemplate _totalPages;
		private BaseFont _baseFont;
		public float MargenInferior { get; set; } = 15;

		public CustomPdfPageEventHelper(string footerText)
		{
			_footerText = footerText;
		}

		public override void OnOpenDocument(PdfWriter writer, Document document)
		{
			try
			{
				// Aumentamos el ancho del template para asegurar que el número quepa
				_totalPages = writer.DirectContent.CreateTemplate(50, 20);
				_baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
			}
			catch (Exception)
			{
				_baseFont = BaseFont.CreateFont();
			}
		}

		public override void OnEndPage(PdfWriter writer, Document document)
		{
			PdfContentByte cb = writer.DirectContent;
			float pageWidth = document.PageSize.Width;

			// Calcular posición Y para el pie de página
			float footerY = document.BottomMargin - MargenInferior;

			// Dibujar línea horizontal
			cb.SetLineWidth(0.5f);
			cb.MoveTo(document.LeftMargin, footerY + 15);
			cb.LineTo(pageWidth - document.RightMargin, footerY + 15);
			cb.Stroke();

			// Fuente para el pie de página
			Font footerFont = new Font(_baseFont, 8, Font.NORMAL);

			// CAMBIO PRINCIPAL: Ajustamos el ancho de la última celda para dar más espacio
			PdfPTable footerTable = new PdfPTable(3);
			footerTable.TotalWidth = pageWidth - document.LeftMargin - document.RightMargin;
			footerTable.SetWidths(new float[] { 35f, 20f, 45f }); // Damos más espacio a la tercera celda
			footerTable.DefaultCell.Border = Rectangle.NO_BORDER;

			// Fecha de impresión (izquierda)
			string currentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
			PdfPCell dateCell = new PdfPCell(new Phrase($"Fecha de Impresión: {currentDate}", footerFont))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				PaddingTop = 3
			};
			footerTable.AddCell(dateCell);

			// Texto personalizado (centro)
			PdfPCell textCell = new PdfPCell(new Phrase(_footerText, footerFont))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingTop = 3
			};
			footerTable.AddCell(textCell);

			// Combinar texto estático con el número de página actual y un template para el total
			PdfPCell pageNumberCell = new PdfPCell()
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				PaddingTop = 3
			};

			// Añadir el texto fijo y el número de página actual
			Phrase pagePhrase = new Phrase($"Página {writer.PageNumber} de ", footerFont);

			// Añadir el template para el número total de páginas
			pagePhrase.Add(new Chunk(Image.GetInstance(_totalPages), 0, 0, true));

			pageNumberCell.Phrase = pagePhrase;
			footerTable.AddCell(pageNumberCell);

			// Dibujar la tabla del pie de página
			footerTable.WriteSelectedRows(0, -1, document.LeftMargin, footerY + 3, cb);

			//// Guardamos el número de página actual para el cierre del documento
			//cb.SaveState();
			//cb.RestoreState();
		}

		public override void OnCloseDocument(PdfWriter writer, Document document)
		{
			// Escribir el número total de páginas en el template
			_totalPages.BeginText();
			_totalPages.SetFontAndSize(_baseFont, 8);
			_totalPages.SetTextMatrix(0, 0);
			_totalPages.ShowText((writer.PageNumber - 1).ToString());
			_totalPages.EndText();
		}
	}

	public class WatermarkPageEvent : PdfPageEventHelper
	{
		private readonly string _texto;
		private readonly BaseFont _font;

		public WatermarkPageEvent(string texto)
		{
			_texto = texto;
			_font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, BaseFont.EMBEDDED);
		}

		public override void OnEndPage(PdfWriter writer, Document document)
		{
			PdfContentByte canvas = writer.DirectContentUnder;
			canvas.SaveState();

			PdfGState gState = new PdfGState();
			gState.FillOpacity = 0.15f; // Transparencia
			canvas.SetGState(gState);

			canvas.BeginText();
			canvas.SetColorFill(BaseColor.LightGray);
			canvas.SetFontAndSize(_font, 80);

			// Coordenadas para que quede cruzado
			float x = document.PageSize.Width / 2;
			float y = document.PageSize.Height / 2;

			canvas.ShowTextAligned(
				Element.ALIGN_CENTER,
				_texto,
				x,
				y,
				45 // Rotación diagonal
			);

			canvas.EndText();
			canvas.RestoreState();
		}
	}


	/// <summary>
	/// Representa un nivel de agrupamiento en un reporte PDF
	/// </summary>
	public class NivelAgrupamiento
	{
		/// <summary>
		/// Nombre del campo por el cual agrupar (debe existir en el tipo T)
		/// </summary>
		public string CampoGrupo { get; set; } = string.Empty;

		/// <summary>
		/// Campo que contiene la descripción a mostrar en el header del grupo
		/// </summary>
		public string CampoDescripcion { get; set; } = string.Empty;

		/// <summary>
		/// Nivel de jerarquía (0 = raíz, 1 = subnivel, etc.)
		/// </summary>
		public int Nivel { get; set; }

		/// <summary>
		/// Color de fondo para el header de este nivel (opcional)
		/// </summary>
		public BaseColor? ColorFondo { get; set; }

		/// <summary>
		/// Indica si debe mostrar subtotal para este nivel
		/// </summary>
		public bool MostrarSubtotal { get; set; }

		/// <summary>
		/// Texto a mostrar antes del subtotal (ej: "Subtotal Proveedor:")
		/// </summary>
		public string? EtiquetaSubtotal { get; set; }
	}
}
