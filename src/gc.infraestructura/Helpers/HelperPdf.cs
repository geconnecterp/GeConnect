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
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.DocManager;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Microsoft.Win32;
using System.ComponentModel;
using System.Globalization;


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
		public static Document GenerarInstanciaAndInit(ref PdfWriter writer, out MemoryStream mStream, string nombreArchivo, HojaSize pagina = HojaSize.A4, bool esVertical = true)
		{
			Document doc = new Document(ObtenerHoja(pagina, esVertical), 20, 20, 15, 50);
			mStream = new MemoryStream();
			writer = PdfWriter.GetInstance(doc, mStream);
			return doc;
		}

		public static Document GenerarInstanciaAndInit(ref PdfWriter writer, out MemoryStream mStream, HojaSize pagina = HojaSize.A4, bool esVertical = true)
		{
			Document doc = new Document(ObtenerHoja(pagina, esVertical), 20, 20, 15, 50);
			mStream = new MemoryStream();
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
				VerticalAlignment = Element.ALIGN_BOTTOM
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

		public static void GeneraCabeceraLista(Document pdf, List<string> titulos, float[] anchos, Font normal)
		{
			PdfPTable tabla = GeneraTabla(titulos.Count, anchos, 100, 10, 0);
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
	bool valorExitoEsTrue = true)
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
								valorTexto = representaExito ? "SI" : "NO";
								break;
							case BooleanDisplayFormat.XOk:
								valorTexto = representaExito ? "OK" : "X";
								break;
							case BooleanDisplayFormat.CheckX:
								// Usamos símbolos Unicode para check (✓) y X (✗)
								valorTexto = representaExito ? "✓" : "✗";
								break;
							case BooleanDisplayFormat.TrueFalse:
								valorTexto = valorBooleano ? "True" : "False";
								break;
						}

						// Asignar colores según el valor (verde para éxito, rojo para error)
						colorTexto = representaExito ? new BaseColor(0, 128, 0) : BaseColor.Red; // Verde o Rojo
						alineacion = Element.ALIGN_CENTER;
					}
					else if (valorObj is DateTime dt)
					{
						valorTexto = incluirHoraEnFechas ? dt.ToString("dd/MM/yyyy HH:mm") : dt.ToString("dd/MM/yyyy");
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

		public static PdfPCell CeldaSinBorde(string texto, Font fuente, int alineacion)
		{
			var celda = HelperPdf.GeneraCelda(HelperPdf.GeneraParrafo(texto, fuente, alineacion, 5, 5), false, BaseColor.White, alineacion);
			celda.Border = Rectangle.NO_BORDER;
			return celda;
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

		public static void CargarSeccionFirmaParaCertificadoDeRetencion(Document pdf, Font fuenteEtiqueta, Font fuenteValor, Font titulo, bool mostrarCargo, float bottom, float top )
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

		public static void CargarTablaConceptosCancelados(Document pdf, List<ConsOrdPagoDetExtendDto> regs, Font fuenteEtiqueta, Font fuenteValor)
		{
			List<string> _campos = ["Descripcion", "Importe",];
			List<string> _titulosTabla = ["Concepto", "Comprobante", "Ctag_motivo", "Cc_fecha_carga", "Cc_importe",];
			float[] _anchosTitulosTabla = [30f, 15f, 15, 20, 20f];
			PdfPTable tablaTitulo = GeneraTabla(1, [100f], 100, 10, 0);

			// FILA 1
			PdfPCell celdaTitulo = new PdfPCell(new Phrase("Conceptos Cancelados", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f,
				PaddingBottom = -2f
			};
			tablaTitulo.AddCell(celdaTitulo);
			pdf.Add(tablaTitulo);

			Chunk linebreak = new Chunk(new LineSeparator(1f, 17f, BaseColor.Black, Element.ALIGN_LEFT, -4));
			pdf.Add(linebreak);

			// FILA 2
			HelperPdf.GeneraCabeceraLista(pdf, _titulosTabla, _anchosTitulosTabla, HelperPdf.FontNormalPredeterminado(true));

			// FILA 3
			//hago el modelo de dato aca ya que necesito los datos de la cuenta
			var regsAux = regs.Where(x => x.Grupo.Equals("1")).Select(x => new
			{
				x.Concepto,
				Comprobante = x.Cm_compte,
				x.Ctag_motivo,
				Cc_fecha_carga = x.Cc_fecha_carga.ToString("dd/MM/yyyy"),
				x.Cc_importe
			}).ToList();
			HelperPdf.GenerarListadoDesdeLista(pdf, regsAux, _titulosTabla, _anchosTitulosTabla, fuenteEtiqueta);
			//HelperPdf.GenerarListadoAgrupado(pdf, regs, _campos, _titulos, anchos, "Grupo", "GrDesc", chico, HelperPdf.FontSubtituloPredeterminado(), null, false, null);

			// FILA 4
			PdfPTable tablaTotal = GeneraTabla(1, [100f], 100, 0, 10);
			PdfPCell celdaTotal = new PdfPCell(new Phrase($"Total Conceptos Cancelados: {regs.Where(x => x.Grupo.Equals("1")).Sum(y => y.Op_importe)}", HelperPdf.FontNormalPredeterminado(true)))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 0f
			};

			tablaTotal.AddCell(celdaTotal);
			pdf.Add(tablaTotal);
		}

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

			//Chunk linebreak = new Chunk(new LineSeparator(1f, 17f, BaseColor.Black, Element.ALIGN_LEFT, -4));
			//pdf.Add(linebreak);

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

			//Chunk linebreak = new Chunk(new LineSeparator(1f, 17f, BaseColor.Black, Element.ALIGN_LEFT, -4));
			//pdf.Add(linebreak);

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
	}

	public enum HojaSize
	{
		A1, A2, A3, A4, A5, A6
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

}
