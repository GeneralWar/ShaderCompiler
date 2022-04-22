// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace General.Shaders
{
    internal class VertexShaderMethod : Method
    {
        public override bool IsMain => true;

        public VertexShaderMethod(MethodDeclarationSyntax syntax) : base(syntax) { }
    }
}
