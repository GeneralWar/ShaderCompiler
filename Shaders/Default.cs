// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders;
using General.Shaders.Uniforms;

namespace Shaders
{
    [VertexShader("Default/Default")]
    public class DefaultVertexShader : IVertexSource
    {
        public Transform transform { get; init; } = new Transform();

        void IVertexSource.OnVertex(InputVertex input, OutputVertex output)
        {
            output.transformedPosition = this.transform.mvpMatrix * input.position;
            output.position = this.transform.modelmatrix * input.position;
            output.normal = input.normal;
            output.color = input.color;
            output.uv0 = input.uv0;
        }
    }

    [FragmentShader("Default/TransparentDiffuse")]
    public class DefaultDiffuseTransparentFragmentShader : IFragmentSource
    {
        [UniformName("MainColor")] public Vector4 mainColor { get; private init; }
        [UniformName("Diffuse")] public Sampler2D diffuse { get; init; } = new Sampler2D();

        void IFragmentSource.OnFragment(InputFragment input, OutputFragment output)
        {
            output.color = ShaderFunctions.MapTexture(this.diffuse, input.uv0) * this.mainColor * input.color;
            output.color.rgb *= LightProcessors.ProcessAllLights(input.position.xyz, input.normal.xyz);
        }
    }

    [FragmentShader("Default/OpaqueDiffuse")]
    public class DefaultDiffuseOpaqueFragmentShader : IFragmentSource
    {
        [UniformName("MainColor")] public Vector4 mainColor { get; private init; }
        [UniformName("Diffuse")] public Sampler2D diffuse { get; private init; } = new Sampler2D();

        void IFragmentSource.OnFragment(InputFragment input, OutputFragment output)
        {
            Vector4 color = ShaderFunctions.MapTexture(this.diffuse, input.uv0);
            output.color = new Vector4(color.rgb * this.mainColor.rgb * input.color.rgb * input.color.a, 1.0f);
            output.color.rgb *= LightProcessors.ProcessAllLights(input.position.xyz, input.normal.xyz);
        }
    }

    [PolygonType(PolygonType.LineList)]
    [PolygonType(PolygonType.LineStrip)]
    [PolygonType(PolygonType.TriangleList)]
    [PolygonType(PolygonType.TriangleStrip)]
    [PolygonType(PolygonType.TriangleFan)]
    [GraphicsShader("Default/TransparentDiffuse", RenderType.Transparent, RenderQueue.Transparent)]
    public class DefaultDiffuseTransparentGraphicsShader : GraphicsShader
    {
        public override IVertexSource VertexShader => new DefaultVertexShader();

        public override IFragmentSource FragmentShader => new DefaultDiffuseTransparentFragmentShader();
    }

    [PolygonType(PolygonType.LineList)]
    [PolygonType(PolygonType.LineStrip)]
    [PolygonType(PolygonType.TriangleList)]
    [PolygonType(PolygonType.TriangleStrip)]
    [PolygonType(PolygonType.TriangleFan)]
    [GraphicsShader("Default/OpaqueDiffuse", RenderType.Opaque, RenderQueue.Geometry)]
    public class DefaultDiffuseOpaqueGraphicsShader : GraphicsShader
    {
        public override IVertexSource VertexShader => new DefaultVertexShader();

        public override IFragmentSource FragmentShader => new DefaultDiffuseOpaqueFragmentShader();
    }
}
