// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace General.Shaders
{
    internal class ArgumentList : Declaration
    {
        private ArgumentListSyntax mSyntax;

        public ArgumentList(ArgumentListSyntax syntax) : base("")
        {
            mSyntax = syntax;
        }

        protected override void internalAnalyze() { }

        public List<string> Compile(Compiler compiler)
        {
            return CompileArgumentList(compiler, mSyntax);
        }
    }
}
