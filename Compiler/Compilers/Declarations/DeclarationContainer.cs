// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace General.Shaders
{
    internal abstract class DeclarationContainer : Declaration
    {
        private Dictionary<string, Declaration> mChildren = new Dictionary<string, Declaration>();

        public DeclarationContainer(string name) : base(name) { }

        public DeclarationContainer(string name, string fullName) : base(name, fullName) { }

        public Declaration RegisterMember(SyntaxNode syntax)
        {
            NamespaceDeclarationSyntax? namespaceDeclarationSyntax = syntax as NamespaceDeclarationSyntax;
            if (namespaceDeclarationSyntax is not null)
            {
                return this.RegisterNamespace(namespaceDeclarationSyntax);
            }

            ClassDeclarationSyntax? classDeclarationSyntax = syntax as ClassDeclarationSyntax;
            if (classDeclarationSyntax is not null)
            {
                return this.RegisterClass(classDeclarationSyntax);
            }

            MethodDeclarationSyntax? methodDeclarationSyntax = syntax as MethodDeclarationSyntax;
            if (methodDeclarationSyntax is not null)
            {
                return this.RegisterMethod(methodDeclarationSyntax);
            }

            throw new NotImplementedException();
        }

        public abstract Namespace RegisterNamespace(NamespaceDeclarationSyntax syntax);
        public abstract Class RegisterClass(ClassDeclarationSyntax syntax);
        public abstract Method RegisterMethod(MethodDeclarationSyntax syntax);

        public Declaration? GetDeclaration(string name)
        {
            if (name.Contains("."))
            {
                int dotIndex = name.IndexOf('.');
                string prefix = name.Substring(0, dotIndex);
                DeclarationContainer? container = this.GetDeclaration(prefix) as DeclarationContainer;
                if (container is null)
                {
                    return null;
                }

                return container.GetDeclaration(name.Substring(dotIndex + 1));
            }

            Declaration? child;
            mChildren.TryGetValue(name, out child);
            return child;
        }

        protected abstract SyntaxNode? GetChildSyntax(string name);

        protected abstract void checkDeclarationCanAdd(Declaration declaration);

        protected void addDeclarationDirectly(Declaration declaration)
        {
            if (!mChildren.TryAdd(declaration.Name, declaration))
            {
                if (declaration is not Method || mChildren[declaration.Name] is not Method)
                {
                    throw new InvalidOperationException("Only methods can have same name");
                }
            }
            declaration.SetParent(this);
            declaration.Analyze();
        }

        protected void AddDeclaration(Declaration declaration)
        {
            this.checkDeclarationCanAdd(declaration);

            string fullName = declaration.FullName;
            if (fullName.Contains('.'))
            {
                int lastDotIndex = fullName.LastIndexOf('.');
                string parentName = fullName.Substring(0, lastDotIndex);
                if (this.FullName == parentName)
                {
                    this.addDeclarationDirectly(declaration);
                    return;
                }

                Declaration? parent = this.GetDeclaration(parentName);
                if (parent is null || parent is not DeclarationContainer)
                {
                    throw new InvalidDataException();
                }

                (parent as DeclarationContainer)?.AddDeclaration(declaration);
            }

            this.addDeclarationDirectly(declaration);
        }

        protected override void internalAnalyze()
        {
            foreach (Declaration child in mChildren.Values.ToArray())
            {
                child.Analyze();
            }
        }
    }
}
