using General.Shaders;

namespace Shaders
{
    public class DefaultVertexShader : IVertexSource
    {
        void IVertexSource.OnVertex(InputVertex input, UniformData uniforms, OutputVertex output)
        {
            output.position = uniforms.transform.matrix * new Vector4(input.position, 1.0f);
            output.color = input.color;
            output.uv0 = input.uv0;
        }
    }

    [Shader("Default/Transparent")]
    public class DefaultGraphicsShader : GraphicsShader
    {
        public override IVertexSource VertexShader => new DefaultVertexShader();

        public override IFragmentSource FragmentShader => throw new System.NotImplementedException();
    }
}
