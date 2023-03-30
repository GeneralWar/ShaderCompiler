// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders.Maths;

namespace General.Shaders
{
    public class InputVertex
    {
        [InputVertex(InputField.Position)] public Vector4 position { get; } // no vec3 in block in glsl, instead of vec4
        [InputVertex(InputField.Color)] public Vector4 color { get; }
        [InputVertex(InputField.Normal)] public Vector4 normal { get; } // no vec3 in block in glsl, instead of vec4
        [InputVertex(InputField.UV0)] public Vector2 uv0 { get; }
    }

    public class OutputVertex
    {
        /// <summary>
        /// use as output color passed to fragment shader, lick gl_Position
        /// </summary>
        [OutputVertex(OutputField.TransformedPosition)] public Vector4 transformedPosition { get; set; } // no vec3 in block in glsl, instead of vec4
        [OutputVertex(OutputField.Position)] public Vector4 worldPosition { get; set; } // no vec3 in block in glsl, instead of vec4
        [OutputVertex(OutputField.Color)] public Vector4 color { get; set; }
        [OutputVertex(OutputField.Normal)] public Vector4 worldNormal { get; set; } // no vec3 in block in glsl, instead of vec4
        [OutputVertex(OutputField.UV0)] public Vector2 uv0 { get; set; }
    }
}
