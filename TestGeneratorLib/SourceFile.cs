using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestGeneratorLib
{
    public class SourceFile
    {
        public IList<SourceNamespace> namespaces = new List<SourceNamespace>();
        public SourceFile(string sourceText)
        {
            SyntaxTree structure = CSharpSyntaxTree.ParseText(sourceText);
            var root = structure.GetRoot();
            foreach (NamespaceDeclarationSyntax sourceNamespace in root.DescendantNodes().OfType<NamespaceDeclarationSyntax>())
            {
                namespaces.Add(new SourceNamespace(sourceNamespace));
            }
        }
    }
}
