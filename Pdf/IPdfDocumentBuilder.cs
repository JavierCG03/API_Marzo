
using QuestPDF.Fluent;

namespace CarSlineAPI.Pdf
{
    /// <summary>
    /// Contrato base que debe implementar cada builder de documento PDF.
    /// </summary>
    public interface IPdfDocumentBuilder
    {
        /// <summary>
        /// Construye el documento QuestPDF listo para ser generado.
        /// </summary>
        IDocument Build();
    }
}