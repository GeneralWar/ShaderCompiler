namespace General.Shaders
{
    public abstract class Light
    {
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
        /// <summary>
        /// direction.w is intensity
        /// </summary>
        [UniformField(typeof(DirectionalLight), 2)] public Vector4 direction { get; set; } // no vec3 in block in glsl, instead of vec4, direction.w is intensity
    }

    [TypeName(Language.GLSL, nameof(PointLight))]
    public class PointLight : Light
    {
        [UniformField(typeof(PointLight), 2)] public float range { get; set; }
        [UniformField(typeof(PointLight), 3)] public float intensity { get; set; }
        [UniformField(typeof(PointLight), 4)] public Vector2 reserve { get; set; } // struct size must be multiple of the size of a vec4 in glsl
        [UniformField(typeof(PointLight), 5)] public Vector4 position { get; set; } // no vec3 in block in glsl, instead of vec4
    }

    [TypeName(Language.GLSL, nameof(SpotLight))]
    public class SpotLight : Light
    {
        [UniformField(typeof(SpotLight), 2)] public float range { get; set; }
        [UniformField(typeof(SpotLight), 3)] public float intensity { get; set; }
        /// <summary>
        /// cos(angle)
        /// </summary>
        [UniformField(typeof(SpotLight), 4)] public float cutoff { get; set; } 
        [UniformField(typeof(SpotLight), 5)] public float reserve { get; set; } // struct size must be multiple of the size of a vec4 in glsl
        [UniformField(typeof(SpotLight), 6)] public Vector4 direction { get; set; } // no vec3 in block in glsl, instead of vec4
        [UniformField(typeof(SpotLight), 7)] public Vector4 position { get; set; } // no vec3 in block in glsl, instead of vec4
    }
}
