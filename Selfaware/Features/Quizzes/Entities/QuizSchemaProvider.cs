using Google.GenAI.Types;
using GenType = Google.GenAI.Types.Type; 

namespace Selfaware.Features.Quizzes.Entities
{
    public class QuizSchemaProvider
    {
        public static Schema GetQuizSchema() => new Schema
        {

            Type = GenType.Object,
            Required = new List<string> { "questions" },
            Properties = new Dictionary<string, Schema> {
            { "questions", new Schema {
                Type = GenType.Array,
                Items = new Schema {
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
            }}
        }
        };

        public static string GetOpenAiJsonSchemaString()
        {
            return """
        {
          "type": "object",
          "properties": {
            "questions": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "text": { "type": "string" },
                  "options": {
                    "type": "array",
                    "items": {
                      "type": "object",
                      "properties": {
                        "text": { "type": "string" },
                        "score": { "type": "integer" }
                      },
                      "required": ["text", "score"],
                      "additionalProperties": false
                    }
                  }
                },
                "required": ["text", "options"],
                "additionalProperties": false
              }
            }
          },
          "required": ["questions"],
          "additionalProperties": false
        }
        """;
        }
    };
            
    }
          

   
