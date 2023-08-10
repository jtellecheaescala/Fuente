﻿using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
using Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using Tecnologistica;


/// <summary>
/// Summary description for HandlersEtiquetasApple
/// </summary>
public class HandlersEtiquetasApple
{
    private Log log;
    private SqlConnection connLog;
    private Globals.staticValues.SeveridadesClass severidades;
    private string MODO_OBTENCION_ARCHIVO;
    private Globals.staticValues gvalues;
    private PageSize pageSize;
    private Archivo archivoPdf = new Archivo();
    private string ruta;
    private PdfWriter writer;
    private PdfDocument pdf;
    private Document document;
    private string size;
    private string format;
    private string etiqueta;


    public HandlersEtiquetasApple(Log _log, SqlConnection _connLog, Globals.staticValues.SeveridadesClass _severidades, string _MODO_OBTENCION_ARCHIVO, Globals.staticValues _gvalues, string _size, string _etiqueta, string _format)
    {
        this.log = _log;
        this.connLog = _connLog;
        this.severidades = _severidades;
        this.MODO_OBTENCION_ARCHIVO = _MODO_OBTENCION_ARCHIVO;
        this.gvalues = _gvalues;
        this.size = _size;
        this.etiqueta = _etiqueta;
        this.format = _format;
        InicializarHandler();
    }

    public void InicializarHandler()
    {
        try
        {

            ruta = String.Format(@"{0}\{1}_{2}.pdf", gvalues.PathOut, etiqueta, DateTime.Now.ToString("yyyyMMdd-HHmmssffff"));
            log.GrabarLogs(connLog, severidades.MsgSoporte1, "INFO", String.Format("Inicializando handler etiquetas BULTOS_DHL_APPLE. Generando ruta: {0}", ruta));

            writer = new PdfWriter(ruta);
            pdf = new PdfDocument(writer);

            if (size.ToUpper().Equals("A4"))
            {
                pageSize = PageSize.A4;
            }
            else
            {
                // new PageSize(170, 85):  representa: a 60 mm x 30 mm
                pageSize = new PageSize(170, 85);
            }

            document = new Document(pdf, pageSize);
            document.SetMargins(0, 0, 0, 0);

            if (!Directory.Exists(gvalues.PathOut))
            {
                Directory.CreateDirectory(gvalues.PathOut);
            }

        }
        catch (Exception ex)
        {
            log.GrabarLogs(connLog, severidades.MsgSoporte1, "ERROR", String.Format("Error al inicializar handler etiquetas Apple: {0}, Detalle: {1}", ex.Message, ex.StackTrace));

            throw ex;
        }
    }

    public Archivo GenerarPDFBultosDHLViajesApple(EtiquetaBultoViajeDHLApple etiqueta, out string message)
    {
        message = null;
        iText.Layout.Element.Image codigoQR = null;
        ImageData logo = null;

        try
        {
            try
            {
                log.GrabarLogs(connLog, severidades.NovedadesEjecucion, "Notificacion", "Generando logo para etiqueta VIAJES_DHL_APPLE para viaje: " + etiqueta.Nro_Viaje);
                logo = ImageDataFactory.Create(gvalues.PathLogo);
            }
            catch (Exception ex)
            {
                log.GrabarLogs(connLog, severidades.MsgSoporte1, "ERROR", "No se pudo generar logo para etiqueta VIAJES_DHL_APPLE para el viaje: " + etiqueta.Nro_Viaje + ". Detalles: " + ex.Message);
                throw new Exception("No se pudo generar el logo para la etiqueta");
            }

            try
            {
                log.GrabarLogs(connLog, severidades.NovedadesEjecucion, "Notificacion", "Generando codigo QR para etiqueta VIAJES_DHL_APPLE para viaje: " + etiqueta.Nro_Viaje);
                codigoQR = new ImageHelper().CrearCodigoQRIText(etiqueta.Nro_Viaje, 330, 330);
            }
            catch (Exception ex)
            {
                log.GrabarLogs(connLog, severidades.MsgSoporte1, "ERROR", "No se pudo generar codigo de QR para etiqueta VIAJES_DHL_APPLE para el viaje: " + etiqueta.Nro_Viaje + ". Detalles: " + ex.Message);
                throw ex;
            }


            log.GrabarLogs(connLog, severidades.NovedadesEjecucion, "Notificacion", "Generando etiquetas VIAJES_DHL_APPLE para Viaje: " + etiqueta.Nro_Viaje + " - Tamaño: " + size + " - Formato: " + format);

            float xDisplacement = 14;
            float yDisplacement = 402;
            Border border = new SolidBorder(ColorConstants.BLACK, 1);

            Table tMainBorder = new Table(1).SetFixedPosition(xDisplacement, yDisplacement, 567);
            tMainBorder.AddCell(new Cell().SetHeight(425).SetWidth(567).SetBorder(new SolidBorder(ColorConstants.RED, 1)));
            document.Add(tMainBorder);

            var p1 = new Paragraph("RUTAS").SetFixedPosition(xDisplacement+15,yDisplacement+378, 370)
                .SetFontSize(24).SetBorder(border).SetTextAlignment(TextAlignment.CENTER);

            var imgLogo = new iText.Layout.Element.Image(logo).SetPadding(0).SetWidth(150).SetFixedPosition(xDisplacement + 402, yDisplacement + 370);

            document.Add(p1);
            document.Add(imgLogo);

            var p2 = new Paragraph("Número de Viaje:").SetFixedPosition(xDisplacement + 15, yDisplacement + 304, 520)
                .SetFontSize(28).SetTextAlignment(TextAlignment.LEFT).SetBorder(border).SetPaddings(12, 8, 12, 8);

            var p3 = new Paragraph(etiqueta.Nro_Viaje).SetFixedPosition(xDisplacement + 283, yDisplacement + 300.5f, 255)
                .SetFontSize(34).SetBold().SetTextAlignment(TextAlignment.CENTER).SetBorder(border);

            document.Add(p2);
            document.Add(p3);

            var p4 = new Paragraph("# Bultos:").SetFixedPosition(xDisplacement + 15, yDisplacement + 227, 520)
                .SetFontSize(24).SetTextAlignment(TextAlignment.LEFT).SetBorder(border).SetPaddings(12, 8, 12, 8);

            var p5 = new Paragraph(etiqueta.Cantidad_Bultos).SetFixedPosition(xDisplacement + 223, yDisplacement + 219.5f, 315)
                .SetFontSize(34).SetBold().SetTextAlignment(TextAlignment.CENTER).SetBorder(border);

            document.Add(p4);
            document.Add(p5);

            var p6 = new Paragraph("# Ordenes:").SetFixedPosition(xDisplacement + 15, yDisplacement + 152, 520)
                .SetFontSize(24).SetTextAlignment(TextAlignment.LEFT).SetBorder(border).SetPaddings(12, 8, 12, 8);

            var p7 = new Paragraph(etiqueta.Cantidad_Ordenes).SetFixedPosition(xDisplacement + 223, yDisplacement + 144.5f, 315)
                .SetFontSize(34).SetBold().SetTextAlignment(TextAlignment.CENTER).SetBorder(border);

            document.Add(p6);
            document.Add(p7);

            var p8 = new Paragraph("Fecha ruteo: " + etiqueta.Fecha_Ruteo.ToString("dd-MM-yyyy")).SetFixedPosition(xDisplacement + 15, yDisplacement + 93, 370)
                .SetFontSize(20).SetTextAlignment(TextAlignment.LEFT).SetBorder(border).SetPaddingLeft(8);

            var p9 = new Paragraph("Fecha estimada salida: " + etiqueta.Fecha_Estimada_Salida.ToString("dd-MM-yyyy")).SetFixedPosition(xDisplacement + 15, yDisplacement + 53, 370)
                .SetFontSize(20).SetTextAlignment(TextAlignment.LEFT).SetBorder(border).SetPaddingLeft(8);

            var imgQR = codigoQR.SetPadding(0).SetWidth(135).SetFixedPosition(xDisplacement + 410, yDisplacement + 3);

            document.Add(p8);
            document.Add(p9);
            document.Add(imgQR);

            pdf.Close();
            writer.Close();
            document.Close();

            string fileName = System.IO.Path.GetFileName(ruta);

            Byte[] pdfEnByte = File.ReadAllBytes(ruta);
            archivoPdf.base64 = Convert.ToBase64String(pdfEnByte);
            archivoPdf.nombre = fileName;

            log.GrabarLogs(connLog, severidades.NovedadesEjecucion, "Notificacion", "Etiqueta VIAJES_DHL_APPLE generada con exito Tamaño: " + size + " - Formato: " + format + ". Modo elegido: BASE64");


            return archivoPdf;
        }
        catch (Exception ex)
        {
            log.GrabarLogs(connLog, severidades.NovedadesEjecucion, "ERROR", "ERROR CREANDO ARCHIVO PDF VIAJES_DHL_APPLE" + ex.Message.ToString());

            throw ex;
        }
        
    }

    public Archivo GenerarPDFBultosDHLApple(List<EtiquetaBultoDHLApple> etiquetas)
    {
        try
        {
            bool addNewPage = false;
            foreach (var etiqueta in etiquetas)
            {
                log.GrabarLogs(connLog, severidades.NovedadesEjecucion, "Notificacion", "Generando etiquetas BULTOS_DHL_APPLE para IDRemito: " + etiqueta.ID_Remito + " - Tamaño: " + size + " - Formato: " + format);

                if (addNewPage)
                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                var p1 = new Paragraph("Viaje:").SetFixedPosition(5, 70, 85).SetFontSize(10).SetBold();
                var p2 = new Paragraph("Parada:").SetFixedPosition(110, 70, 90).SetFontSize(10).SetBold();

                document.Add(p1);
                document.Add(p2);

                var p3 = new Paragraph(etiqueta.Nro_Viaje.PadLeft(6, '0'))
                    .SetFixedPosition(5, 40, 80).SetFontSize(22).SetBold();
                var p4 = new Paragraph(string.Format("{0}", etiqueta.Parada.PadLeft(2, '0'), etiqueta.CantParadasTotales.PadLeft(2, '0')))
                    .SetFixedPosition(110, 20, 95).SetFontSize(34).SetBold();

                document.Add(p3);
                document.Add(p4);

                var p5 = new Paragraph(string.Format("Intento: {0}", etiqueta.NroReintento))
                    .SetFixedPosition(5, 20, 170).SetFontSize(10);
                var p6 = new Paragraph(string.Format("Parcel ID: {0}", etiqueta.ParcelId))
                    .SetFixedPosition(5, 10, 170).SetFontSize(10).SetBold();
                var p7 = new Paragraph(string.Format("Fecha Viaje: {0}", etiqueta.FechaViaje))
                    .SetFixedPosition(5, 2, 170).SetFontSize(8);

                document.Add(p5);
                document.Add(p6);
                document.Add(p7);
                addNewPage = true;
            }

            pdf.Close();
            document.Close();
            writer.Close();

            string fileName = System.IO.Path.GetFileName(ruta);

            Byte[] pdfEnByte = File.ReadAllBytes(ruta);
            archivoPdf.base64 = Convert.ToBase64String(pdfEnByte);
            archivoPdf.nombre = fileName;

            log.GrabarLogs(connLog, severidades.NovedadesEjecucion, "Notificacion", "Etiqueta PDFBultosVertical BULTOS_DHL_APPLE generada con exito Tamaño: " + size + " - Formato: " + format + ". Modo elegido: BASE64");


            return archivoPdf;
        }
        catch (Exception ex)
        {
            log.GrabarLogs(connLog, severidades.NovedadesEjecucion, "ERROR", "ERROR CREANDO ARCHIVO PDF BULTOS_DHL_APPLE" + ex.Message);

            throw ex;
        }
    }

}