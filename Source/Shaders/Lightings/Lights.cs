// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders.Lightings
{
    public abstract class Light
    {
        public const int LIGHT_ARRAY_SIZE = 8;

        /// <summary>
        /// enable when alpha is true, or disable otherwise
        /// </summary>
        [UniformField(typeof(Light), 0)] public Vector4 color { get; set; }
    }

    [TypeName(Language.GLSL, nameof(AmbientLight))]
    public class AmbientLight : Light { }

    [TypeName(Language.GLSL, nameof(DirectionalLight))]
    public class DirectionalLight : Light
    {
        [UniformField(typeof(DirectionalLight), 1)] public Vector4 direction { get; set; }
        [UniformField(typeof(DirectionalLight), 2)] public float intensity { get; set; }
        [UniformField(typeof(DirectionalLight), 3)] public float shadowPower { get; set; }
        [UniformField(typeof(DirectionalLight), 4)] public Vector2 reserve { get; set; }
        [UniformField(typeof(DirectionalLight), 5)] public LightTransform transform { get; init; } = new LightTransform();
    }

    [TypeName(Language.GLSL, nameof(PointLight))]
    public class PointLight : Light
    {
        [UniformField(typeof(PointLight), 1)] public float range { get; set; }
        [UniformField(typeof(PointLight), 2)] public float intensity { get; set; }
        [UniformField(typeof(PointLight), 3)] public float shadowPower { get; set; }
        [UniformField(typeof(PointLight), 4)] public float reserve { get; set; } // struct size must be multiple of the size of a float, vec2 or vec4 in glsl
        [UniformField(typeof(PointLight), 5)] public Vector4 position { get; set; } // no vec3 in block in glsl, instead of vec4
        [UniformField(typeof(PointLight), 6)] public LightTransform transform { get; init; } = new LightTransform();
    }

    [TypeName(Language.GLSL, nameof(SpotLight))]
    public class SpotLight : Light
    {
        [UniformField(typeof(SpotLight), 1)] public float range { get; set; }
        [UniformField(typeof(SpotLight), 2)] public float intensity { get; set; }
        /// <summary>
        /// cos(angle)
        /// </summary>
        [UniformField(typeof(SpotLight), 1)] public float cutoff { get; set; }
        [UniformField(typeof(SpotLight), 2)] public float shadowPower { get; set; } // struct size must be multiple of the size of a float, vec2 or vec4 in glsl
        [UniformField(typeof(SpotLight), 3)] public Vector4 direction { get; set; } // no vec3 in block in glsl, instead of vec4
        [UniformField(typeof(SpotLight), 4)] public Vector4 position { get; set; } // no vec3 in block in glsl, instead of vec4
        [UniformField(typeof(SpotLight), 5)] public LightTransform transform { get; init; } = new LightTransform();
    }
}
