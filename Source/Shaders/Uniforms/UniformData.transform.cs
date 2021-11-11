namespace General.Shaders
{
    public partial class UniformData
    {
        [TypeName(Language.GLSL, nameof(Transform))]
        public class Transform
        {
            [UniformField(typeof(Transform))] public Matrix4 matrix { get; }
        }
    }
}
