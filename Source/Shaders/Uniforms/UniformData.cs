namespace General.Shaders
{
    public partial class UniformData
    {
        [LayoutBinding(0)] public Transform transform { get; init; } = new Transform();
    }
}
