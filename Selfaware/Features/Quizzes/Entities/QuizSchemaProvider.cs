using Google.GenAI.Types;
using GenType = Google.GenAI.Types.Type; 

namespace Selfaware.Features.Quizzes.Entities
{
    public class QuizSchemaProvider
    {
        public static Schema GetQuizSchema() => new Schema
        {
            Type = GenType.Array,
            Items = new Schema
            {
                Type = GenType.Object,
                Required = new List<string> { "text", "options" },
                Properties = new Dictionary<string, Schema> {
            { "text", new Schema { Type = GenType.String } },
            { "options", new Schema {
                Type = GenType.Array,
                Items = new Schema {
                    Type = GenType.Object,
                    Required = new List<string> { "text", "score" },
                    Properties = new Dictionary<string, Schema> {
                        { "text", new Schema { Type = GenType.String } },
                        { "score", new Schema { Type = GenType.Integer } }
                    }
                }
            }}
        }
            }
        };
    }
}