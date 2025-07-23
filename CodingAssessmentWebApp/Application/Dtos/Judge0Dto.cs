using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class Judge0Dto
    {
    }
    public class Judge0Result
    {
        [JsonPropertyName("stdout")]
        public string StdOut { get; set; }
        [JsonPropertyName("stdErr")]
        public string StdErr { get; set; }
        [JsonPropertyName("status")]
        public Judge0Status Status { get; set; }
        [JsonPropertyName("time")]
        public string Time {  get; set; }
        [JsonPropertyName("memory")]
        public int? Memory { get; set; }
    }
    public class Judge0Status
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
    public class Judge0LanguageDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("extensions")]
        public List<string> Extensions { get; set; }
    }
    public class Judge0CodeExecutionRequest
    {
        public int Id { get; set; }
        public string SourceCode { get; set; }
        public string TestCase { get; set; }
        public string ExpectedOutput { get; set; }
    }
}
