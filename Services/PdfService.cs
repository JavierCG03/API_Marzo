using CarSlineAPI.Models.DTOs;
using QuestPDF.Infrastructure;
using CarSlineAPI.Pdf;

namespace CarSlineAPI.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerarPdfOrdenAsync(OrdenPdfDto orden);
        Task<byte[]> GuardarPdfOrdenAsync(OrdenPdfDto orden, string numeroOrden);

        //Task<byte[]> GenerarPdfAvaluoAsync(AvaluoPdfDto avaluo);
       // Task<byte[]> GuardarPdfAvaluoAsync(AvaluoPdfDto avaluo, string numeroAvaluo);
    }

    /// <summary>
    /// Servicio central de PDFs.
    /// Responsabilidades:
    ///   1. Configurar la licencia de QuestPDF (una sola vez).
    ///   2. Garantizar que el directorio de almacenamiento exista.
    ///   3. Delegar la construcción de cada documento a su builder específico.
    ///   4. Gestionar logging y manejo de errores de forma uniforme.
    ///
    /// NO contiene lógica de layout — eso vive en cada builder.
    /// </summary>
    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;
        private readonly string _rutaBasePdfs = @"C:\Users\Carsline\Downloads\Evidencias_Ordenes\";

        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger;

            // Licencia QuestPDF (Community — gratis)
            QuestPDF.Settings.License = LicenseType.Community;

            if (!Directory.Exists(_rutaBasePdfs))
                Directory.CreateDirectory(_rutaBasePdfs);
        }

        // -------------------------------------------------------
        // Órdenes
        // -------------------------------------------------------

        public async Task<byte[]> GenerarPdfOrdenAsync(OrdenPdfDto orden)
        {
            try
            {
                _logger.LogInformation("📄 Generando PDF para orden {NumeroOrden}", orden.NumeroOrden);

                var builder = new OrdenPdfBuilder(orden);
                var pdfBytes = builder.Build().GeneratePdf();

                _logger.LogInformation("✅ PDF orden generado: {Bytes} bytes", pdfBytes.Length);
                return await Task.FromResult(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar PDF para orden {NumeroOrden}", orden.NumeroOrden);
                throw;
            }
        }

        public async Task<byte[]> GuardarPdfOrdenAsync(OrdenPdfDto orden, string numeroOrden)
        {
            try
            {
                var pdfBytes = await GenerarPdfOrdenAsync(orden);
                // TODO: persistir pdfBytes en disco / Azure Blob / S3 usando _rutaBasePdfs
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al guardar PDF para orden {NumeroOrden}", numeroOrden);
                throw;
            }
        }

        // -------------------------------------------------------
        // Avalúos
        // -------------------------------------------------------
        /*
        public async Task<byte[]> GenerarPdfAvaluoAsync(AvaluoPdfDto avaluo)
        {
            try
            {
                _logger.LogInformation("📄 Generando PDF para avalúo {NumeroAvaluo}", avaluo.NumeroAvaluo);

                var builder = new AvaluoPdfBuilder(avaluo);
                var pdfBytes = builder.Build().GeneratePdf();

                _logger.LogInformation("✅ PDF avalúo generado: {Bytes} bytes", pdfBytes.Length);
                return await Task.FromResult(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar PDF para avalúo {NumeroAvaluo}", avaluo.NumeroAvaluo);
                throw;
            }
        }

        public async Task<byte[]> GuardarPdfAvaluoAsync(AvaluoPdfDto avaluo, string numeroAvaluo)
        {
            try
            {
                var pdfBytes = await GenerarPdfAvaluoAsync(avaluo);
                // TODO: persistir pdfBytes en disco / Azure Blob / S3 usando _rutaBasePdfs
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al guardar PDF para avalúo {NumeroAvaluo}", numeroAvaluo);
                throw;
            }
        }*/
    }
}