// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders.Uniforms;
using System;

namespace General.Shaders
{
    static public class ShaderFunctions
    {
        [FunctionName(Language.GLSL, "texture")]
        static public Vector4 MapTexture(Sampler2D texture, Vector2 uv)
        {
            throw new NotImplementedException();
        }
    }
}
