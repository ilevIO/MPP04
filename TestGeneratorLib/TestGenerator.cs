﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestGeneratorLib
{
    public class ClassInfo
    {
        public String name;
        public String generatedCode;
        public ClassInfo(String name, string code)
        {
            this.name = name;
            this.generatedCode = code;
        }
        //methods
        //properties etc
    }

    public class TestGenerator
    {
        int tasksLimit;
        public string TestMethodAttribute => "TestMethod";

        public string TestClassAttribute => "TestClass";
        public string TestMethodAssertMessage => "autogenerated";

        public SyntaxNode GenerateTestsForClass(ClassDeclarationSyntax classDeclaration)
        {
            if (!(classDeclaration.Parent is NamespaceDeclarationSyntax))
            {
                return null;
            }

            string sourceClassNamespace = (classDeclaration.Parent as NamespaceDeclarationSyntax).Name.ToString();
            SyntaxNode result = CompilationUnit()
                    .WithUsings(
                        List<UsingDirectiveSyntax>(
                            new UsingDirectiveSyntax[]{
                            UsingDirective(
                                IdentifierName("System")),
                            UsingDirective(
                                IdentifierName("Microsoft.VisualStudio.TestTools.UnitTesting")),
                            UsingDirective(
                                IdentifierName(sourceClassNamespace))}))
                    .WithMembers(
                        SingletonList<MemberDeclarationSyntax>(
                            NamespaceDeclaration(
                                QualifiedName(
                                    IdentifierName(sourceClassNamespace),
                                    IdentifierName("Test")))));
            result = GenerateClassNode(result, classDeclaration);
            result = GenerateTestMethods(result, classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>());
            return result.NormalizeWhitespace();
        }

        private SyntaxNode GenerateClassNode(SyntaxNode root, ClassDeclarationSyntax classDeclaration)
        {
            string sourceClassName = classDeclaration.Identifier.Text;
            NamespaceDeclarationSyntax oldNamespaceDeclaration = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            NamespaceDeclarationSyntax newNamespaceDeclaration = oldNamespaceDeclaration.AddMembers(
                ClassDeclaration(sourceClassName)
                    .WithAttributeLists(
                        SingletonList<AttributeListSyntax>(
                            AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName(TestClassAttribute))))))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)))
                    );

            return root.ReplaceNode(oldNamespaceDeclaration, newNamespaceDeclaration);
        }

        private SyntaxNode GenerateTestMethods(SyntaxNode root, IEnumerable<MethodDeclarationSyntax> methods)
        {
            ClassDeclarationSyntax oldClassDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            ClassDeclarationSyntax newClassDeclaration = oldClassDeclaration;
            foreach (MethodDeclarationSyntax method in methods)
            {
                newClassDeclaration = newClassDeclaration.AddMembers(GenerateTestMethod(method));
            }

            return root.ReplaceNode(oldClassDeclaration, newClassDeclaration);
        }

        private MemberDeclarationSyntax GenerateTestMethod(MethodDeclarationSyntax method)
        {
            string methodIdentifier = method.Identifier.Text + "Test";
            return MethodDeclaration(
                        PredefinedType(
                            Token(SyntaxKind.VoidKeyword)),
                        Identifier(methodIdentifier))
                    .WithAttributeLists(
                        SingletonList<AttributeListSyntax>(
                            AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName("Test"))))))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)))
                    .WithBody(
                        Block(
                            SingletonList<StatementSyntax>(
                                ExpressionStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("Assert"),
                                            IdentifierName("Fail")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        Literal(TestMethodAssertMessage))))))))));
        }

        /*public SyntaxNode Generate(string sourceCode)
        {
            return GenerateCompilationUnit(sourceCode);
        }*/

        /*public SyntaxNode GenerateCompilationUnit(string sourceCode)
        {
            CompilationUnitSyntax sourceRoot = CSharpSyntaxTree.ParseText(sourceCode).GetCompilationUnitRoot();
            if (sourceRoot == null)
            {
                throw new NullReferenceException("Parsing of source code wasn't done!");
            }

            ClassDeclarationSyntax classDeclaration = sourceRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

            SyntaxNode result = GenerateCompilationUnit(classDeclaration);
            result = GenerateClassNode(result, classDeclaration);
            result = GenerateTestMethods(result, sourceRoot.DescendantNodes().OfType<MethodDeclarationSyntax>());
            return result.NormalizeWhitespace();
        }*/
        public TransformBlock<string, IList<ClassInfo>> GenerateTestsForSource {
            get
            {
                //get class list
                IList<ClassInfo> classList = new List<ClassInfo>();
                return new TransformBlock<string, IList<ClassInfo>>((code) =>
                {
                    SourceFile sourceFile = new SourceFile(code);
                    IList<ClassInfo> classes = new List<ClassInfo>();
                    foreach (SourceNamespace sourceNamespace in sourceFile.namespaces)
                    {
                        foreach (SourceClass sourceClass in sourceNamespace.classes)
                        {
                            classes.Add(new ClassInfo(sourceClass.name, GenerateTestsForClass(sourceClass.classInfo).ToString()));
                        }
                    }
                    return classes;
                    /*MSTestGenerator generator = new MSTestGenerator();
                    string generatedCode = generator.Generate(code).ToString();

                    string testClassName = ExtractClassName(generatedCode);
                    return new ClassInfo(testClassName, generatedCode);*/
                }, new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = this.tasksLimit
                });
            }
      
        }
        public TestGenerator(int tasksLimit)
        {
            this.tasksLimit = tasksLimit;
        }
    }
}
