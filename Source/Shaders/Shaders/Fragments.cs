// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders
{
    [TypeName(Language.GLSL, nameof(InputFragment))]
    public class InputFragment
    {
        [LayoutLocation(0)] [InputFragment(InputField.Position)] public Vector4 position; // no vec3 in block in glsl, instead of vec4
        [LayoutLocation(1)] [InputFragment(InputField.Color)] public Vector4 color;
        [LayoutLocation(2)] [InputFragment(InputField.Normal)] public Vector4 normal; // no vec3 in block in glsl, instead of vec4
        [LayoutLocation(3)] [InputFragment(InputField.UV0)] public Vector2 uv0;
    }

    public class OutputFragment
    {
        [LayoutLocation(0)] [OutputFragment(OutputField.Color)] public Vector4 color;
    }
}
