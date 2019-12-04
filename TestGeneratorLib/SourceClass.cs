using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestGeneratorLib
{
    public class SourceClass
    {
        public string name;
        public ClassDeclarationSyntax classInfo;
        public IList<SourceMethod> methods = new List<SourceMethod>();
        public SourceClass(ClassDeclarationSyntax classInfo)
        {
            this.classInfo = classInfo;
            this.name = classInfo.Identifier.ToString();
            /*foreach (MemberDeclarationSyntax member in namespaceInfo.Members)
            {
            }*/
            /*var methods = classInfo.SyntaxTree.GetRoot().DescendantNodes()
                .OfType<MethodDeclarationSyntax>().ToList();*/
            foreach (MethodDeclarationSyntax methodInfo in classInfo.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Where(method =>
                method.Modifiers.Where(modifier =>
                    modifier.Kind() == SyntaxKind.PublicKeyword)
                .Any()))
            {
                methods.Add(new SourceMethod(methodInfo));
            }
        }
    }
}
