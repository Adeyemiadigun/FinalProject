using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Application.Interfaces.ExternalServices;
using Domain.Enum;

namespace Infrastructure.ExternalServices
{
    public  class MethodNameExtractor : IExtractMethodName
    {
        public  string ExtractMethodName(string code, TechnologyStack language)
        {
            return language switch
            {
                TechnologyStack.CSharp or TechnologyStack.Java => ExtractFromCSharpOrJava(code),
                TechnologyStack.Python => ExtractFromPython(code),
                TechnologyStack.JavaScript => ExtractFromJavaScript(code),
                TechnologyStack.Cpp => ExtractFromCpp(code),
                _ => null
            };
        }

        private  string ExtractFromCSharpOrJava(string code)
        {
            // Matches: public int Sum(int[] nums) { ... }
            var pattern = @"\b(?:public|private|protected)?\s*(?:static)?\s*\w[\w<>\[\],\s]*\s+(\w+)\s*\(.*?\)";
            var match = Regex.Match(code, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }

        private  string ExtractFromPython(string code)
        {
            // Matches: def sum_array(arr):
            var pattern = @"def\s+(\w+)\s*\(";
            var match = Regex.Match(code, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }

        private  string ExtractFromJavaScript(string code)
        {
            // Matches: function sumArray(arr) { ... }
            var patternFunc = @"function\s+(\w+)\s*\(";
            var matchFunc = Regex.Match(code, patternFunc);
            if (matchFunc.Success) return matchFunc.Groups[1].Value;

            // Matches: const sumArray = (arr) => { ... }
            var patternArrow = @"(?:const|let|var)\s+(\w+)\s*=\s*\(.*?\)\s*=>";
            var matchArrow = Regex.Match(code, patternArrow);
            return matchArrow.Success ? matchArrow.Groups[1].Value : null;
        }

        private  string ExtractFromCpp(string code)
        {
            // Matches: int sumArray(int arr[], int size) { ... }
            var pattern = @"\b(?:int|float|double|char|string|bool|auto)\s+(\w+)\s*\(.*?\)";
            var match = Regex.Match(code, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }
    }

}
