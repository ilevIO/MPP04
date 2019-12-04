using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestGeneratorLib
{
    public struct MethodParameter
    {
        public string paramType;
        public string paramName;
        public MethodParameter(string paramType, string paramName)
        {
            this.paramType = paramType;
            this.paramName = paramName;
        }
    }
    public class SourceMethod
    {
        public MethodDeclarationSyntax methodInfo;
        public string name;
        public string returnType;
        public IList<MethodParameter> parameters = new List<MethodParameter>();
        public SourceMethod(MethodDeclarationSyntax methodInfo)
        {
            this.methodInfo = methodInfo;
            this.name = methodInfo.Identifier.ToString();
            this.returnType = methodInfo.ReturnType.ToString();
            var parameterInfos = methodInfo.ParameterList.Parameters;
            foreach (ParameterSyntax parameterInfo in parameterInfos)
            {
                this.parameters.Add(new MethodParameter(parameterInfo.Identifier.ValueText, parameterInfo.Type.ToString()));
            }
        }
    }
}
