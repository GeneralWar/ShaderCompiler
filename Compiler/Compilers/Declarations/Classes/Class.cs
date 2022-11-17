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
    internal class Class : DeclarationContainer, IVariableCollection, IMethodProvider, IReferenceHost, IUniformHost
    {
        private ClassDeclarationSyntax mSyntax;

        public Type Type => Extension.GetType(mSyntax.GetFullName()) ?? throw new InvalidDataException();

        private HashSet<Declaration> mReferences = new HashSet<Declaration>();
        IEnumerable<Declaration> IReferenceHost.References => this.References;
        public HashSet<Declaration> References => mReferences;

        private List<Method> mMethods = new List<Method>();

        public string? OutputFilename { get; private set; }

        private HashSet<Declaration> mUniforms = new HashSet<Declaration>();
        public IEnumerable<Declaration> Uniforms => throw new NotImplementedException();

        public Class(DeclarationContainer root, ClassDeclarationSyntax syntax) : base(root, syntax, syntax.Identifier.Text, syntax.GetFullName())
        {
            mSyntax = syntax;
        }

        protected override SyntaxNode? GetChildSyntax(string name)
        {
            return mSyntax.ChildNodes().FirstOrDefault(s => s.GetName() == name);
        }

        protected override void checkDeclarationCanAdd(Declaration declaration)
        {
            if (declaration is Class || declaration is Member || declaration is Variable)
            {
                return;
            }

            throw new InvalidDataException();
        }

        public override Namespace RegisterNamespace(NamespaceDeclarationSyntax syntax) => throw new InvalidOperationException($"{this.GetType()} cannot declare member of type {syntax.GetType()}");

        public override Class RegisterClass(ClassDeclarationSyntax syntax)
        {
            Class instance = new Class(this.Root, syntax);
            this.AddDeclaration(instance);
            return instance;
        }

        public Class? GetClass(ClassDeclarationSyntax syntax, bool createIfNotExist)
        {
            Declaration? instance = this.GetDeclaration(syntax.GetName());
            if (instance is null && createIfNotExist)
            {
                instance = new Class(this.Root, syntax);
                this.AddDeclaration(instance);
            }
            return instance as Class;
        }

        Variable? IVariableCollection.GetVariable(string name)
        {
            return this.GetDeclaration(name) as Variable;
        }

        void IVariableCollection.PushVariable(Variable variable) => throw new InvalidOperationException("Should never push local variable to a class");

        Method[] IMethodProvider.GetMethods(string name)
        {
            return this.GetMethods(name);
        }

        public Method[] GetMethods(string name)
        {
            return mMethods.Where(m => m.Name == name).ToArray();
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
                        this.addMethod(new VertexShaderMethod(this.Root, this, methodDeclarationSyntax));
                    }
                    else if (methodDeclarationSyntax.CompareName(nameof(IFragmentSource.OnFragment)))
                    {
                        this.addMethod(new FragmentShaderMethod(this.Root, this, methodDeclarationSyntax));
                    }
                    else
                    {
                        this.addMethod(new Method(this.Root, this, methodDeclarationSyntax));
                    }
                    continue;
                }

                // TODO: should check uniform here, or record member info

                PropertyDeclarationSyntax? propertyDeclarationSyntax = memberSyntax as PropertyDeclarationSyntax;
                if (propertyDeclarationSyntax is not null)
                {
                    this.analyzeProperty(propertyDeclarationSyntax);
                    continue;
                }

                FieldDeclarationSyntax? fieldDeclarationSyntax = memberSyntax as FieldDeclarationSyntax;
                if (fieldDeclarationSyntax is not null)
                {
                    this.analyzeField(fieldDeclarationSyntax);
                    continue;
                }

                ClassDeclarationSyntax? classDeclarationSyntax = memberSyntax as ClassDeclarationSyntax;
                if (classDeclarationSyntax is not null)
                {
                    this.analyzeClass(classDeclarationSyntax);
                    continue;
                }

                Debugger.Break();
                throw new NotImplementedException();
            }
        }

        protected override void analyzeProperty(PropertyDeclarationSyntax syntax)
        {
            base.analyzeProperty(syntax);
        }

        protected override void analyzeField(FieldDeclarationSyntax syntax)
        {
            base.analyzeField(syntax);
        }

        protected override void analyzeClass(ClassDeclarationSyntax syntax)
        {
            base.analyzeClass(syntax);
        }


        private void addMethod(Method method)
        {
            if (mMethods.Any(m => m.MethodName == method.MethodName))
            {
                throw new InvalidOperationException();
            }

            method.SetParent(this);
            method.Analyze();
            mMethods.Add(method);
        }

        private void analyzeVertexShader(MethodDeclarationSyntax syntax)
        {
            this.addMethod(new VertexShaderMethod(this.Root, this, syntax));
        }

        private void analyzeFragmentShader(MethodDeclarationSyntax syntax)
        {
            this.addMethod(new FragmentShaderMethod(this.Root, this, syntax));
        }

        public override Method RegisterMethod(MethodDeclarationSyntax syntax)
        {
            Method method = new Method(this.Root, this, syntax);
            this.addMethod(method);
            return method;
        }

        void IReferenceHost.AddReference(Declaration reference)
        {
            mReferences.Add(reference);
        }

        public void AddUniform(Declaration uniform)
        {
            mUniforms.Add(uniform);
        }

        public void SetOutputFilename(string filename)
        {
            this.OutputFilename = filename;
        }
    }
}
