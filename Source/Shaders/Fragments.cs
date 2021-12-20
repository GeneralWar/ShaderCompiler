// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders
{
    public class InputFragment
    {
        [LayoutLocation(0)] [InputFragment(InputField.Color)] public Vector4 color { get; }
        [LayoutLocation(1)] [InputFragment(InputField.UV0)] public Vector2 uv0 { get; }
    }

    public class OutputFragment
    {
        [LayoutLocation(0)] [OutputFragment(OutputField.Color)] public Vector4 color;
    }
}
