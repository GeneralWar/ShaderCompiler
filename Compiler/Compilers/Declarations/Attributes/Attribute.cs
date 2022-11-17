// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace General.Shaders
{
    internal class Attribute : Declaration, ISyntaxHost
    {
        private AttributeSyntax mSyntax;
        public SyntaxNode SyntaxNode => mSyntax;

        private Type mType;
        public Type Type => mType;

        public System.Attribute Instance { get; private set; }

        public Attribute(DeclarationContainer root, AttributeSyntax syntax) : base(root, syntax, syntax.GetName() + nameof(Attribute))
        {
            mSyntax = syntax;
            mType = syntax.GetTypeFromRoot(this.Name) ?? throw new InvalidOperationException();
            this.FullName = mType.FullName ?? throw new InvalidOperationException();
            this.Instance = mSyntax.ToInstance();
        }


        protected override void internalAnalyze()
        {
            throw new NotImplementedException("Attribute should not analyze");
        }
    }
}
