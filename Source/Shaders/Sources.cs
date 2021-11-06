namespace General.Shaders
{
    public interface IVertexSource
    {
        void OnVertex(InputVertex input, UniformData uniforms, OutputVertex output);
    }

    public interface IFragmentSource
    {
        void OnFragment();
    }
}
