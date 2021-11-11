// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;

namespace General.Shaders
{
    internal class Class : DeclarationContainer
    {
        private ClassDeclarationSyntax mSyntax;

        public Class(ClassDeclarationSyntax syntax) : base(syntax.Identifier.Text, syntax.GetFullName())  
        {
            mSyntax = syntax;
        }

        protected override void checkDeclarationCanAdd(Declaration declaration)
        {
            if (declaration is Class || declaration is Method)
            {
                return;
            }

            throw new InvalidDataException();
        }

        public Class RegisterClass(ClassDeclarationSyntax syntax)
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

        protected override void internalAnalyze(Compiler compiler)
        {
            ClassDeclarationSyntax? syntax = mSyntax as ClassDeclarationSyntax;
            if (syntax is null)
            {
                throw new InvalidDataException();
            }

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
                    this.analyzeVertexShader(onVertexMethodSyntax, compiler);
                    return;
                }

                throw new InvalidDataException();
            }
            if (baseList.Types.Contains(nameof(IFragmentSource)))
            {
                MethodDeclarationSyntax? onFragmentMethodSyntax = syntax.Members.Find<MethodDeclarationSyntax>(nameof(IFragmentSource.OnFragment));
                if (onFragmentMethodSyntax is not null)
                {
                    this.analyzeFragmentShader(onFragmentMethodSyntax, compiler);
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

            base.internalAnalyze(compiler);
        }

        private void addMethod(Method method, Compiler compiler)
        {
            this.addDeclarationDirectly(method);
            method.Analyze(compiler);
        }

        private void analyzeVertexShader(MethodDeclarationSyntax syntax, Compiler compiler)
        {
            this.addMethod(new VertexShaderMethod(syntax), compiler);
        }

        private void analyzeFragmentShader(MethodDeclarationSyntax syntax, Compiler compiler)
        {
            this.addMethod(new FragmentShaderMethod(syntax), compiler);
        }
    }
}
