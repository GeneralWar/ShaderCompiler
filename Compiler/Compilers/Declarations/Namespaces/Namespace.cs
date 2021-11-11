using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;

namespace General.Shaders
{
    internal class Namespace : DeclarationContainer
    {
        private NamespaceDeclarationSyntax? mSyntax = null;

        public Namespace(string name) : base(name, name) { }

        public Namespace(NamespaceDeclarationSyntax syntax) : base(syntax.Name.GetName(), syntax.Name.GetFullName())
        {
            mSyntax = syntax;
        }

        protected override void checkDeclarationCanAdd(Declaration declaration)
        {
            if (declaration is Namespace || declaration is Class)
            {
                return;
            }

            throw new InvalidDataException();
        }

        public Namespace RegisterNamespace(NamespaceDeclarationSyntax syntax)
        {
            Namespace instance = new Namespace(syntax);
            this.AddDeclaration(instance);
            return instance;
        }

        public Namespace? GetNamespace(NamespaceDeclarationSyntax syntax, bool createIfNotExist)
        {
            Declaration? instance = this.GetDeclaration(syntax.Name.GetFullName());
            if (instance is null && createIfNotExist)
            {
                instance = this.RegisterNamespace(syntax);
            }
            return instance as Namespace;
        }

        public Class RegisterClass(ClassDeclarationSyntax syntax)
        {
            Class instance = new Class(syntax);
            this.AddDeclaration(instance);
            return instance;
        }

        public Class? GetClass(ClassDeclarationSyntax syntax, bool createIfNotExist)
        {
            Declaration? instance = this.GetDeclaration(syntax.GetFullName());
            if (instance is null && createIfNotExist)
            {
                instance = this.RegisterClass(syntax);
            }
            return instance as Class;
        }

        protected override void internalAnalyze(Compiler compiler)
        {
            NamespaceDeclarationSyntax? syntax = mSyntax;
            if (syntax is not null)
            {
                foreach (SyntaxNode node in syntax.ChildNodes())
                {
                    NamespaceDeclarationSyntax? namespaceDeclaration = node as NamespaceDeclarationSyntax;
                    if (namespaceDeclaration is not null)
                    {
                        this.analyzeNamespaceDeclaration(namespaceDeclaration);
                        continue;
                    }

                    ClassDeclarationSyntax? classDeclarationSyntax = node as ClassDeclarationSyntax;
                    if (classDeclarationSyntax is not null)
                    {
                        this.analyzeClassDeclaration(classDeclarationSyntax);
                        continue;
                    }
                }
            }

            base.internalAnalyze(compiler);
        }

        private void analyzeNamespaceDeclaration(NamespaceDeclarationSyntax syntax)
        {
            Namespace? instance = this.GetNamespace(syntax, true);
            if (instance is null)
            {
                throw new InvalidDataException();
            }
        }

        private void analyzeClassDeclaration(ClassDeclarationSyntax syntax)
        {
            Class? instance = this.RegisterClass(syntax);
            if (instance is null)
            {
                throw new InvalidDataException();
            }
        }
    }
}
