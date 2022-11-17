// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace General.Shaders
{
    internal class FragmentShaderMethod : Method
    {
        public override bool IsMain => true;

        public FragmentShaderMethod(DeclarationContainer root, DeclarationContainer declaringContainer, MethodDeclarationSyntax syntax) : base(root, declaringContainer, syntax) { }
    }
}
