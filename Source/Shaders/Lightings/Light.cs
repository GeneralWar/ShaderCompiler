namespace General.Shaders
{
    public abstract class Light
    {
        /// <summary>
        /// 1.0f for true, 0.0f for false
        /// </summary>
        [UniformField(typeof(Light), 0)] public float enabled { get; set; }
        [UniformField(typeof(Light), 1)] public Vector4 color { get; set; }
    }

    [TypeName(Language.GLSL, nameof(DirectionLight))]
    public class DirectionLight : Light
    {
        [UniformField(typeof(DirectionLight), 2)] public Vector3 direction { get; set; }
    }
}
