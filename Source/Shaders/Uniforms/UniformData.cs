namespace General.Shaders
{
    public partial class UniformData
    {
        [LayoutBinding(0)] public Transform transform { get; init; } = new Transform();
        [LayoutBinding(1)] public Sampler2D texture0 { get; init; } = new Sampler2D();
    }
}
