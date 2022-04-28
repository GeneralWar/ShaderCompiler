// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders
{
    public class InputVertex
    {
        [LayoutLocation(0)] [InputVertex(InputField.Position)] public Vector4 position { get; } // no vec3 in block in glsl, instead of vec4
        [LayoutLocation(1)] [InputVertex(InputField.Color)] public Vector4 color { get; }
        [LayoutLocation(2)] [InputVertex(InputField.Normal)] public Vector4 normal { get; } // no vec3 in block in glsl, instead of vec4
        [LayoutLocation(3)] [InputVertex(InputField.UV0)] public Vector2 uv0 { get; }
    }

    public class OutputVertex
    {
        [OutputVertex(OutputField.Position)] public Vector4 transformedPosition { get; set; } // no vec3 in block in glsl, instead of vec4
        [LayoutLocation(0)] [OutputVertex(OutputField.Position)] public Vector4 position { get; set; } // no vec3 in block in glsl, instead of vec4
        [LayoutLocation(1)] [OutputVertex(OutputField.Color)] public Vector4 color { get; set; }
        [LayoutLocation(2)] [OutputVertex(OutputField.Normal)] public Vector4 normal { get; set; } // no vec3 in block in glsl, instead of vec4
        [LayoutLocation(3)] [OutputVertex(OutputField.UV0)] public Vector2 uv0 { get; set; }
    }
}
