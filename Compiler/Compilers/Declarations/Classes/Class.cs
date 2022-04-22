// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace General.Shaders
{
    internal class Class : DeclarationContainer, IVariableCollection, ISyntaxHost, IMethodProvider, IReferenceHost
    {
        private ClassDeclarationSyntax mSyntax;
        public ClassDeclarationSyntax Syntax => mSyntax;
        SyntaxNode ISyntaxHost.SyntaxNode => mSyntax;

        public Type Type => Extension.GetType(mSyntax.GetFullName()) ?? throw new InvalidDataException();

        private HashSet<Declaration> mReferences = new HashSet<Declaration>();
        HashSet<Declaration> IReferenceHost.References => mReferences;
        public HashSet<Declaration> References => mReferences;

        public Class(ClassDeclarationSyntax syntax) : base(syntax.Identifier.Text, syntax.GetFullName())
        {
            mSyntax = syntax;
        }

        protected override SyntaxNode? GetChildSyntax(string name)
        {
            return mSyntax.ChildNodes().FirstOrDefault(s => s.GetName() == name);
        }

        protected override void checkDeclarationCanAdd(Declaration declaration)
        {
            if (declaration is Class || declaration is Method || declaration is Variable)
            {
                return;
            }

            throw new InvalidDataException();
        }

        public override Namespace RegisterNamespace(NamespaceDeclarationSyntax syntax) => throw new InvalidOperationException($"{this.GetType()} cannot declare member of type {syntax.GetType()}");

        public override Class RegisterClass(ClassDeclarationSyntax syntax)
        {
            Class instance = new Class(syntax);
            this.AddDeclaration(instance);
            return instance;
        }

        public Class? GetClass(ClassDeclarationSyntax syntax, bool createIfNotExist)
        {
            Declaration? instance = this.GetDeclaration(syntax.GetName());
            if (instance is null && createIfNotExist)
            {
                instance = new Class(syntax);
                this.AddDeclaration(instance);
            }
            return instance as Class;
        }

        Variable? IVariableCollection.GetVariable(string name)
        {
            return this.GetDeclaration(name) as Variable;
        }

        void IVariableCollection.PushVariable(Variable variable) => throw new InvalidOperationException("Should never push local variable to a class");

        Method? IMethodProvider.GetMethod(string name)
        {
            return this.GetDeclaration(name) as Method;
        }

        protected override void internalAnalyze()
        {
            ClassDeclarationSyntax? syntax = mSyntax as ClassDeclarationSyntax;
            if (syntax is null)
            {
                throw new InvalidDataException();
            }

            // analyze properties and fields first, they will be used in base list analysis
            this.analyzeMembers(syntax);
            this.analyzeBaseList(syntax);

            base.internalAnalyze();
        }

        private void analyzeBaseList(ClassDeclarationSyntax syntax)
        {
            BaseListSyntax? baseList = syntax.BaseList;
            if (baseList is null)
            {
                return;
            }

            if (baseList.Types.Contains(nameof(IVertexSource)))
            {
                MethodDeclarationSyntax? onVertexMethodSyntax = syntax.Members.Find<MethodDeclarationSyntax>(nameof(IVertexSource.OnVertex));
                if (onVertexMethodSyntax is not null)
                {
                    // analyzed in members
                    //this.analyzeVertexShader(onVertexMethodSyntax);
                    return;
                }

                throw new InvalidDataException();
            }
            if (baseList.Types.Contains(nameof(IFragmentSource)))
            {
                MethodDeclarationSyntax? onFragmentMethodSyntax = syntax.Members.Find<MethodDeclarationSyntax>(nameof(IFragmentSource.OnFragment));
                if (onFragmentMethodSyntax is not null)
                {
                    // analyzed in members
                    //this.analyzeFragmentShader(onFragmentMethodSyntax);
                    return;
                }

                throw new InvalidDataException();
            }
            else if (baseList.Types.Contains(nameof(GraphicsShader)))
            {
                // Nothing to do
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void analyzeMembers(ClassDeclarationSyntax syntax)
        {
            foreach (MemberDeclarationSyntax memberSyntax in syntax.Members)
            {
                MethodDeclarationSyntax? methodDeclarationSyntax = memberSyntax as MethodDeclarationSyntax;
                if (methodDeclarationSyntax is not null)
                {
                    if (methodDeclarationSyntax.CompareName(nameof(IVertexSource.OnVertex)))
                    {
                        this.addMethod(new VertexShaderMethod(methodDeclarationSyntax));
                    }
                    else if (methodDeclarationSyntax.CompareName(nameof(IFragmentSource.OnFragment)))
                    {
                        this.addMethod(new FragmentShaderMethod(methodDeclarationSyntax));
                    }
                    else
                    {
                        this.addMethod(new Method(methodDeclarationSyntax));
                    }
                    continue;
                }

                PropertyDeclarationSyntax? propertyDeclarationSyntax = memberSyntax as PropertyDeclarationSyntax;
                if (propertyDeclarationSyntax is not null)
                {
                    this.AddDeclaration(new Variable(propertyDeclarationSyntax));
                    continue;
                }

                FieldDeclarationSyntax? fieldDeclarationSyntax = memberSyntax as FieldDeclarationSyntax;
                if (fieldDeclarationSyntax is not null)
                {
                    this.AddDeclaration(new Variable(fieldDeclarationSyntax));
                    continue;
                }

                Debugger.Break();
                throw new NotImplementedException();
            }
        }

        private void addMethod(Method method)
        {
            this.addDeclarationDirectly(method);
        }

        private void analyzeVertexShader(MethodDeclarationSyntax syntax)
        {
            this.addMethod(new VertexShaderMethod(syntax));
        }

        private void analyzeFragmentShader(MethodDeclarationSyntax syntax)
        {
            this.addMethod(new FragmentShaderMethod(syntax));
        }

        public override Method RegisterMethod(MethodDeclarationSyntax syntax)
        {
            Method method = new Method(syntax);
            this.addMethod(method);
            return method;
        }

        void IReferenceHost.AppendReference(Declaration reference)
        {
            mReferences.Add(reference);
        }
    }
}
