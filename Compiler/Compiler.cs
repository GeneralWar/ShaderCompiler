using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;

namespace General.Shaders
{
    public class Compiler
    {
        private void Initialize(string projectPath)
        {
            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException(projectPath);
            }

            string? directory = Path.GetDirectoryName(projectPath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new DirectoryNotFoundException(projectPath);
            }

            List<string> decocatedFilenames = new List<string>();
            foreach (string filename in Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                this.analyzeFile(filename);
            }
        }

        private void analyzeFile(string filename)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(filename));
            this.analyzeNode(syntaxTree.GetRoot());
        }

        private void analyzeNode(SyntaxNode root)
        {
            foreach (SyntaxNode node in root.ChildNodes())
            {
                if (node is NamespaceDeclarationSyntax)
                {
                    this.analyzeNode(node);
                }
                else if (node is ClassDeclarationSyntax)
                {
                    this.analyzeClassDeclaration(node as ClassDeclarationSyntax);
                }
            }
        }

        private void analyzeClassDeclaration(ClassDeclarationSyntax? syntax)
        {
            if (syntax is null)
            {
                return;
            }

            BaseListSyntax? baseList = syntax.BaseList;
            if (baseList is null)
            {
                return;
            }

            if (baseList.Types.Contains(nameof(IVertexSource)))
            {
                this.analyzeVertexShader(syntax);
                return;
            }

            Console.WriteLine(syntax);
        }

        private void registerClass(ClassDeclarationSyntax syntax)
        {
            string classname = syntax.Identifier.Text;
            string fullname = classname;
            NamespaceDeclarationSyntax? namespaceSyntax = syntax.Parent as NamespaceDeclarationSyntax;
            if (namespaceSyntax is not null)
            {
                fullname = namespaceSyntax.Name.GetFullName() + "." + classname;
            }
        }
        private void analyzeVertexShader(ClassDeclarationSyntax syntax)
        {
            this.registerClass(syntax);

            MethodDeclarationSyntax? method = syntax.Members.Find<MethodDeclarationSyntax>(nameof(IVertexSource.OnVertex));
            if (method is null)
            {
                throw new InvalidDataException();
            }

            BlockSyntax body = method.Body;
            ParameterListSyntax parameterList = method.ParameterList;
        }

        static public void CompileProject(string projectPath)
        {
            Compiler compiler = new Compiler();
            compiler.Initialize(Path.GetFullPath(projectPath));
        }
    }
}
