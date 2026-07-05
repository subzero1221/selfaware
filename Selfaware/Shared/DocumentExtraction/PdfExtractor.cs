using UglyToad.PdfPig;

namespace Selfaware.Shared.DocumentExtraction
{
    public class PdfExtractor : ITextExtractor
    {
        public string ExtractText(Stream fileStream)
        {
            var textBuilder = new System.Text.StringBuilder();
            using (PdfDocument document = PdfDocument.Open(fileStream))
            {
                foreach (var page in document.GetPages())
                    textBuilder.AppendLine(page.Text);
            }
            return textBuilder.ToString();
        }
    }
}
