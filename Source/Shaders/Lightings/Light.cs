namespace General.Shaders
{
    public abstract class Light
    {
        /// <summary>
        /// 1.0f for true, 0.0f for false
        /// </summary>
        [UniformField(typeof(Light), 0)] public float enabled { get; set; }
        [UniformField(typeof(Light), 1)] public float intensity { get; set; }
        [UniformField(typeof(Light), 2)] public Vector2 reserves { get; set; } // struct size must be multiple of the size of a vec4 in glsl
        [UniformField(typeof(Light), 3)] public Vector4 color { get; set; }
    }

    [TypeName(Language.GLSL, nameof(DirectionalLight))]
    [TypeName(Language.GLSL, nameof(DirectionalLight))]
    public class DirectionalLight : Light
    {
        [UniformField(typeof(DirectionalLight), 4)] public Vector4 direction { get; set; } // no vec3 in block in glsl, instead of vec4
    }
}
