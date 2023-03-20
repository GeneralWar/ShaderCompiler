// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders;
using General.Shaders.Lightings;
using General.Shaders.Maths;
using General.Shaders.Uniforms;

namespace Shaders.Lightings
{
    [VertexShader("Internal/Lightings/DefaultVertex")]
    internal class DefaultLightingVertex : IVertexSource
    {
        void IVertexSource.OnVertex(InputVertex input, OutputVertex output)
        {
            output.transformedPosition = new Vector4(input.position.xyz, 1.0f);
            output.color = input.color;
            output.uv0 = input.uv0;
        }
    }

    [FragmentShader("Internal/Lightings/DefaultFragment")]
    internal class DefaultLightingFragment : IFragmentSource
    {
        [UniformUsage((int)InternalUniformUsage.SamplerColor, nameof(InternalUniformUsage.SamplerColor))] public Sampler2D samplerColor { get; private init; }
        [UniformUsage((int)InternalUniformUsage.SamplerPosition, nameof(InternalUniformUsage.SamplerPosition))] public Sampler2D samplerPosition { get; private init; }
        [UniformUsage((int)InternalUniformUsage.SamplerNormal, nameof(InternalUniformUsage.SamplerNormal))] public Sampler2D samplerNormal { get; private init; }

        void IFragmentSource.OnFragment(InputFragment input, OutputFragment output)
        {
            Vector4 positionInWorld = ShaderFunctions.MapTexture(this.samplerPosition, input.uv0);
            Vector4 normalInWorld = ShaderFunctions.MapTexture(this.samplerNormal, input.uv0);
            output.color = ShaderFunctions.MapTexture(this.samplerColor, input.uv0);
            output.color.rgb *= LightProcessors.ProcessAllLights(positionInWorld.xyz, normalInWorld.xyz);
        }
    }

    [PolygonType(PolygonType.TriangleList)]
    [GraphicsShader("Internal/Lightings/Default", RenderType.Opaque, RenderQueue.Geometry)]
    internal class LightingShader : GraphicsShader
    {
        public override IVertexSource VertexShader => new DefaultLightingVertex();
        public override IFragmentSource FragmentShader => new DefaultLightingFragment();
    }
}
