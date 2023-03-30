// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders.Maths;

namespace General.Shaders
{
    [TypeName(Language.GLSL, nameof(InputFragment))]
    public class InputFragment
    {
        [InputFragment(InputField.Position)] public Vector4 worldPosition; // no vec3 in block in glsl, instead of vec4
        [InputFragment(InputField.Color)] public Vector4 color;
        [InputFragment(InputField.Normal)] public Vector4 worldNormal; // no vec3 in block in glsl, instead of vec4
        /// <summary>
        /// original uv set 0 from vertex shader
        /// </summary>
        [InputFragment(InputField.UV0)] public Vector2 uv0;
    }

    public class OutputFragment
    {
        [OutputFragment(OutputField.Color)] public Vector4 color;
        [OutputFragment(OutputField.Position)] public Vector4 position;
        [OutputFragment(OutputField.Normal)] public Vector4 normal;
    }
}
