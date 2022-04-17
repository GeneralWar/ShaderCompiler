// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders.Uniforms
{
    [UniformType]
    [TypeName(Language.GLSL, nameof(Transform))]
    public class Transform
    {
        [UniformField(typeof(Transform))] public Matrix4 matrix { get; }
    }
}
