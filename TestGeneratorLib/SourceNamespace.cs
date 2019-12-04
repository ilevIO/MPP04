using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestGeneratorLib
{
    interface ITestGeneratable
    {

    }
    public class SourceNamespace
    {
        public NamespaceDeclarationSyntax namespaceInfo;
        public string name;
        public IList<SourceClass> classes = new List<SourceClass>();

        public SourceNamespace(NamespaceDeclarationSyntax namespaceInfo)
        {
            this.namespaceInfo = namespaceInfo;
            this.name = namespaceInfo.Name.ToString();
            /*foreach (MemberDeclarationSyntax member in namespaceInfo.Members)
            {
            }*/

            foreach (ClassDeclarationSyntax classInfo in namespaceInfo.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                //fork
                classes.Add(new SourceClass(classInfo));
            }
            //join
        }
    }
}
