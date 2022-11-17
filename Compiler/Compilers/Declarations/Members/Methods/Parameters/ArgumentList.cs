// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace General.Shaders
{
    internal class ArgumentList : Declaration
    {
        private ArgumentListSyntax mSyntax;

        public ArgumentList(DeclarationContainer root, ArgumentListSyntax syntax) : base(root, syntax, "")
        {
            mSyntax = syntax;
        }

        protected override void internalAnalyze() { }

        public List<string> Compile(CompileContext context)
        {
            return CompileArgumentList(context, mSyntax);
        }
    }
}
