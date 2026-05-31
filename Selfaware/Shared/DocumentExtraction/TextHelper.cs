namespace Selfaware.Shared.DocumentExtraction
{
    public static class TextHelper
    {
        public static List<string> SplitText(string input, int maxChunkSize)
        {
            var chunks = new List<string>();

            var paragraphs = input.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string currentChunk = "";

            foreach (var paragraph in paragraphs)
            {
        
                if (currentChunk.Length + paragraph.Length > maxChunkSize)
                {
                    if (!string.IsNullOrWhiteSpace(currentChunk))
                    {
                        chunks.Add(currentChunk.Trim());
                    }
                    currentChunk = paragraph;
                }
                else
                {
     
                    currentChunk += (currentChunk.Length > 0 ? "\n\n" : "") + paragraph;
                }
            }
            if (!string.IsNullOrWhiteSpace(currentChunk))
            {
                chunks.Add(currentChunk.Trim());
            }

            return chunks;
        }
    }
}
