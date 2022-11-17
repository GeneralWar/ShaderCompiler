// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders.Lightings
{
    public enum InternalLightingUsage
    {
        AmbientLight = 1 << 4,
        DirectionalLightArray,
        PointLightArray,
        SpotLightArray,

        ShadowColorSampler = 1 << 5,
        ShadowNormalSampler,
        ShadowDepthSampler,
    }

    [UniformType]
    [TypeName(Language.GLSL, nameof(LightTransform))]
    public class LightTransform
    {
        [UniformField(typeof(LightTransform), 0)] public Matrix4 vpMatrix { get; }
    }
}
