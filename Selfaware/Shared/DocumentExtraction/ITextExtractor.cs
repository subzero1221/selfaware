namespace Selfaware.Shared.DocumentExtraction
{
    public interface ITextExtractor
    {
        string ExtractText(Stream? fileStream);
    }
}
