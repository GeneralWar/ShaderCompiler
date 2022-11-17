// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace General.Shaders
{
    internal class Variable : Declaration, ITypeHost
    {
        private SyntaxNode? mSyntax = null;

        private Type mType;
        private string mTypeString;
        public Type Type => mType;

        //public Variable(DeclarationContainer root, PropertyDeclarationSyntax syntax) : base(root, syntax.Identifier.ValueText)
        //{
        //    mSyntax = syntax;
        //    mType = Declaration.AnalyzeType(syntax);
        //    mTypeString = mType.FullName ?? mType.Name;
        //}

        //public Variable(DeclarationContainer root, FieldDeclarationSyntax syntax) : base(root, syntax.ToString())
        //{
        //    mSyntax = syntax;
        //    mType = Declaration.AnalyzeType(syntax);
        //    mTypeString = mType.FullName ?? mType.Name;
        //}

        public Variable(DeclarationContainer root, ParameterSyntax syntax) : base(root, syntax, syntax.Identifier.ValueText)
        {
            mSyntax = syntax;
            mType = Declaration.AnalyzeType(syntax);
            mTypeString = mType.FullName ?? mType.Name;
        }

        public Variable(DeclarationContainer root, TypeSyntax type, VariableDeclaratorSyntax syntax) : base(root, syntax, syntax.Identifier.ValueText)
        {
            mSyntax = syntax;
            mType = Declaration.AnalyzeType(type);
            mTypeString = mType.FullName ?? mType.Name;
        }

        protected override void internalAnalyze()
        {
            //throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"{this.Name}({mTypeString})";
        }
    }
}
