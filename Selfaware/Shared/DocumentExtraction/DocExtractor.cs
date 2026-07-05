using Xceed.Words.NET;

namespace Selfaware.Shared.DocumentExtraction
{
    public class DocExtractor : ITextExtractor
    {
        public string ExtractText(Stream fileStream)
        {
            using (DocX document = DocX.Load(fileStream))
            {
                return document.Text;
            }
        }
    }
}
