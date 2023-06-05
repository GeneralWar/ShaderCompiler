// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders;
using General.Shaders.Maths;
using General.Shaders.Uniforms;

namespace Shaders
{
    [VertexShader("Default/Default")]
    public class DefaultVertexShader : IVertexSource
    {
        [UniformUsage((int)InternalUniformUsage.Transform)] public Transform transform { get; init; } = new Transform();

        void IVertexSource.OnVertex(InputVertex input, OutputVertex output)
        {
            Vector4 inputPosition = new Vector4(input.position.xyz, 1.0f);
            output.transformedPosition = this.transform.mvpMatrix * inputPosition;
            output.worldPosition = this.transform.modelMatrix * inputPosition;
            output.worldNormal = new Vector4(MathFunctions.Normalize(this.transform.modelMatrix * new Vector4(input.normal.xyz, .0f)).xyz, 1.0f); // fragment will be discard if alpha is 0
            output.color = input.color;
            output.uv0 = input.uv0;
            //DebugFunctions.Log("in: %v4f, out: %v4f", inputPosition, output.transformedPosition);
        }
    }

    [FragmentShader("Default/TransparentDiffuse")]
    public class DefaultDiffuseTransparentFragmentShader : IFragmentSource
    {
        [UniformUsage((int)InternalUniformUsage.MainColor, nameof(InternalUniformUsage.MainColor))] public Vector4 mainColor { get; private init; }
        [UniformUsage((int)InternalUniformUsage.Diffuse, nameof(InternalUniformUsage.Diffuse))] public Sampler2D diffuse { get; init; } = new Sampler2D();

        void IFragmentSource.OnFragment(InputFragment input, OutputFragment output)
        {
            output.color = ShaderFunctions.MapTexture(this.diffuse, input.uv0) * this.mainColor * input.color;
            output.position = input.worldPosition;
            output.normal = input.worldNormal; 
        }
    }

    [FragmentShader("Default/OpaqueDiffuse")]
    public class DefaultDiffuseOpaqueFragmentShader : IFragmentSource
    {
        [UniformUsage((int)InternalUniformUsage.MainColor, nameof(InternalUniformUsage.MainColor))] public Vector4 mainColor { get; private init; }
        [UniformUsage((int)InternalUniformUsage.Diffuse, nameof(InternalUniformUsage.Diffuse))] public Sampler2D diffuse { get; init; } = new Sampler2D();

        void IFragmentSource.OnFragment(InputFragment input, OutputFragment output)
        {
            Vector4 textureColor = ShaderFunctions.MapTexture(this.diffuse, input.uv0);
            output.color = new Vector4(textureColor.rgb * this.mainColor.rgb * input.color.rgb * input.color.a, 1.0f);
            output.position = input.worldPosition;
            output.normal = input.worldNormal;
        }
    }

    [PolygonType(PolygonType.TriangleList)]
    [GraphicsShader("Default/TransparentDiffuse", RenderType.Transparent, PolygonMode.Fill, RenderQueue.Transparent)]
    public class DefaultDiffuseTransparentGraphicsShader : GraphicsShader
    {
        public override IVertexSource VertexShader => new DefaultVertexShader();

        public override IFragmentSource FragmentShader => new DefaultDiffuseTransparentFragmentShader();
    }

    [PolygonType(PolygonType.TriangleList)]
    [GraphicsShader("Default/OpaqueDiffuse", RenderType.Opaque, PolygonMode.Fill, RenderQueue.Geometry)]
    public class DefaultDiffuseOpaqueGraphicsShader : GraphicsShader
    {
        public override IVertexSource VertexShader => new DefaultVertexShader();

        public override IFragmentSource FragmentShader => new DefaultDiffuseOpaqueFragmentShader();
    }

    [PolygonType(PolygonType.TriangleList)]
    [GraphicsShader("Default/OpaqueWireframe", RenderType.Opaque, PolygonMode.Line, RenderQueue.Geometry)]
    public class DefaultOpaqueWireframeGraphicsShader : GraphicsShader
    {
        public override IVertexSource VertexShader => new DefaultVertexShader();

        public override IFragmentSource FragmentShader => new DefaultDiffuseOpaqueFragmentShader();
    }
}
