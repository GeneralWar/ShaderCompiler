// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders
{
    [TypeName(Language.GLSL, nameof(InputFragment))]
    public class InputFragment
    {
        [LayoutLocation(0)] [InputFragment(InputField.Color)] public Vector4 color;
        [LayoutLocation(1)] [InputFragment(InputField.Normal)] public Vector3 normal;
        [LayoutLocation(2)] [InputFragment(InputField.UV0)] public Vector2 uv0;
    }

    public class OutputFragment
    {
        [LayoutLocation(0)] [OutputFragment(OutputField.Color)] public Vector4 color;
    }
}
