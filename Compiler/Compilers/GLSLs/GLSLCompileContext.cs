// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders
{
    class GLSLCompileContext : CompileContext
    {
        public string? UniformDeclaration { get; private set; } = null;

        public void SetUniformDeclaration(string uniforms)
        {
            this.UniformDeclaration = uniforms;
        }
    }
}
