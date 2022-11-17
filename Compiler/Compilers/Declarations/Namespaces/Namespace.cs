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
    internal class Namespace : DeclarationContainer
    {
        private HashSet<NamespaceDeclarationSyntax> mSyntaxNodes = new HashSet<NamespaceDeclarationSyntax>();
        private HashSet<NamespaceDeclarationSyntax> mAnalyzedSyntaxNodes = new HashSet<NamespaceDeclarationSyntax>();

#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
        public Namespace(string name) : base(null, null, name, name) { }
#pragma warning restore CS8625 // 无法将 null 字面量转换为非 null 的引用类型。

        public Namespace(DeclarationContainer root, NamespaceDeclarationSyntax syntax) : base(root, syntax, syntax.Name.GetSafeName(), syntax.Name.GetFullName())
        {
            this.appendSyntax(syntax);
        }

        public void Analyze(SyntaxNode root)
        {
            foreach (SyntaxNode node in root.ChildNodes())
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

        protected void appendSyntax(NamespaceDeclarationSyntax syntax)
        {
            if (!mAnalyzedSyntaxNodes.Contains(syntax))
            {
                this.Analyze(syntax);
                mAnalyzedSyntaxNodes.Add(syntax);
            }
            mSyntaxNodes.Add(syntax);
        }

        protected override SyntaxNode? GetChildSyntax(string name)
        {
            foreach (SyntaxNode syntax in mSyntaxNodes)
            {
                return syntax.ChildNodes().FirstOrDefault(s => throw new Exception(s.ToString()));
            }
            return null;
        }

        protected override void checkDeclarationCanAdd(Declaration declaration)
        {
            if (declaration is Namespace || declaration is Class)
            {
                return;
            }

            throw new InvalidDataException();
        }

        public override Namespace RegisterNamespace(NamespaceDeclarationSyntax syntax)
        {
            Namespace instance = new Namespace(this.Root, syntax);
            this.AddDeclaration(instance);
            return instance;
        }

        public Namespace? GetNamespace(NamespaceDeclarationSyntax syntax, bool createIfNotExist)
        {
            Declaration? declaration = this.GetDeclaration(syntax.Name.GetFullName());
            if (declaration is not null && declaration is not Namespace)
            {
                throw new InvalidDataException();
            }

            Namespace? instance = declaration as Namespace;
            if (instance is null && createIfNotExist)
            {
                instance = this.RegisterNamespace(syntax);
            }
            instance?.appendSyntax(syntax);
            return instance;
        }

        public override Class RegisterClass(ClassDeclarationSyntax syntax)
        {
            Class instance = new Class(this.Root, syntax);
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

        public override Method RegisterMethod(MethodDeclarationSyntax syntax) => throw new InvalidOperationException($"{this.GetType()} cannot declare member of type {syntax.GetType()}");

        private void analyzeNamespaceDeclaration(NamespaceDeclarationSyntax syntax)
        {
            Namespace? instance = this.GetNamespace(syntax, true);
            if (instance is null)
            {
                throw new InvalidDataException();
            }

            mAnalyzedSyntaxNodes.Add(syntax);
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
