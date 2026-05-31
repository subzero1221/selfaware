using System.Text.Json;

namespace Selfaware.Shared.Helpers
{
    public class JsonSettings
    {
        public static readonly JsonSerializerOptions Options =
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}
