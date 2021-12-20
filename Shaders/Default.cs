// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders;

namespace Shaders
{
    [VertexShader("Default/Transparent")]
    public class DefaultVertexShader : IVertexSource
    {
        void IVertexSource.OnVertex(InputVertex input, UniformData uniforms, OutputVertex output)
        {
            output.position = uniforms.transform.matrix * new Vector4(input.position, 1.0f);
            output.color = input.color;
            output.uv0 = input.uv0;
        }
    }

    [FragmentShader("Default/Transparent")]
    public class DefaultTransparentFragmentShader : IFragmentSource
    {
        void IFragmentSource.OnFragment(InputFragment input, UniformData uniforms, OutputFragment output)
        {
            Vector4 color = ShaderFunctions.MapTexture(uniforms.texture0, input.uv0) * input.color;
            output.color["rgb"] = color["rgb"];
            output.color.a = color.a;
        }
    }

    [FragmentShader("Default/Opaque")]
    public class DefaultOpaqueFragmentShader : IFragmentSource
    {
        void IFragmentSource.OnFragment(InputFragment input, UniformData uniforms, OutputFragment output)
        {
            Vector4 color = ShaderFunctions.MapTexture(uniforms.texture0, input.uv0);
            output.color = new Vector4(color["rgb"] * input.color.a, input.color.a);
        }
    }

    [GraphicsShader("Default/Transparent", RenderType.Transparent, RenderQueue.Transparent)]
    public class DefaultTransparentGraphicsShader : GraphicsShader
    {
        public override IVertexSource VertexShader => new DefaultVertexShader();

        public override IFragmentSource FragmentShader => new DefaultTransparentFragmentShader();
    }

    [GraphicsShader("Default/Opaque", RenderType.Opaque, RenderQueue.Geometry)]
    public class DefaultOpaqueGraphicsShader : GraphicsShader
    {
        public override IVertexSource VertexShader => new DefaultVertexShader();

        public override IFragmentSource FragmentShader => new DefaultOpaqueFragmentShader();
    }
}
