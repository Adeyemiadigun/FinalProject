using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class LlmGradingResultDto
    {
       
            [JsonPropertyName("isCorrect")]
            public bool IsCorrect { get; set; }

            [JsonPropertyName("reason")]
            public string? Reason { get; set;}

    }
}
