﻿// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace General.Shaders
{
    internal class Property : Member, ITypeHost
    {
        public Type Type { get; private set; }

        public Property(DeclarationContainer root, DeclarationContainer declaringContainer, PropertyDeclarationSyntax syntax) : base(root, declaringContainer, syntax)
        {
            this.Type = Declaration.AnalyzeType(syntax);
            this.FullName = this.Type.FullName ?? this.Type.Name;
        }
    }
}
