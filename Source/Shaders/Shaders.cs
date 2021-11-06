namespace General.Shaders
{
    public abstract class Shader { }

    public abstract class GraphicsShader : Shader
    {
        public abstract IVertexSource VertexShader { get; }
        public abstract IFragmentSource FragmentShader { get; }
    }
}
