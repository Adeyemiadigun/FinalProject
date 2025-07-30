using System;
using Application.Interfaces.ExternalServices;
using Domain.Enum;

namespace Infrastructure.ExternalServices;
public  class CodeWrapper : ICodeWrapper
{
    
    public  string Wrap(TechnologyStack language, string studentCode, string methodName, string input)
    {
        string functionCall = $"{methodName}({input})";

        return language switch
        {
            TechnologyStack.CSharp => WrapCSharp(studentCode, functionCall),
            TechnologyStack.Java => WrapJava(studentCode, functionCall),
            TechnologyStack.Python => WrapPython(studentCode, functionCall),
            TechnologyStack.JavaScript => WrapJavaScript(studentCode, functionCall),

            // Add more languages if needed
            _ => throw new NotSupportedException($"Language {language} is not supported.")
        };
    }

    private  string WrapCSharp(string code, string functionCall)
    {
        return $@"
            using System;
            using System.Linq;
            using System.Collections.Generic;

            public class Program {{
            {code}

            public static void Main() {{
              var result = {functionCall};
             Console.WriteLine(result);
          }}
        }}";
    }
    private  string WrapJava(string code, string functionCall)
    {
        return $@"
                public class Main {{
            {code}

            public static void main(String[] args) {{
                System.out.println({functionCall});
            }}
        }}";
    }

    private  string WrapPython(string code, string functionCall)
    {
        return $"{code}\n\nprint({functionCall})";
    }

    private  string WrapJavaScript(string code, string functionCall)
    {
        return $@"
            {code}

            console.log({functionCall});
            ";
    }

    private  string WrapCpp(string code, string functionCall)
    {
        return $@"
            #include <iostream>
            using namespace std;

            {code}

            int main() {{
          cout << {functionCall} << endl;
            return 0;
            }}";
    }
}

