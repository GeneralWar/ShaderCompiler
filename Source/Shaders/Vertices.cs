// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders
{
    public class InputVertex
    {
        [LayoutLocation(0)] [InputVertex(InputField.Position)] public Vector3 position { get; }
        [LayoutLocation(1)] [InputVertex(InputField.Color)] public Vector4 color { get; }
        [LayoutLocation(2)] [InputVertex(InputField.Normal)] public Vector3 normal { get; }
        [LayoutLocation(3)] [InputVertex(InputField.UV0)] public Vector2 uv0 { get; }
    }

    public class OutputVertex
    {
        [OutputVertex(OutputField.Position)] public Vector4 position { get; set; }
        [LayoutLocation(0)] [OutputVertex(OutputField.Color)] public Vector4 color { get; set; }
        [LayoutLocation(1)] [OutputVertex(OutputField.UV0)] public Vector2 uv0 { get; set; }
    }
}
