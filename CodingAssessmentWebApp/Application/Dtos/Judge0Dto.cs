using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class Judge0Dto
    {
    }
    public class Judge0Result
    {
        public string StdOut { get; set; }
        public string StdErr { get; set; }
        public Judge0Status Status { get; set; }
        public string Time {  get; set; }
        public string Memory { get; set; }
    }
    public class Judge0Status
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
    public class Judge0LanguageDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
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
