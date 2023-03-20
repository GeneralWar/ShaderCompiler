// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders;
using General.Shaders.Lightings;
using General.Shaders.Maths;
using General.Shaders.Uniforms;
using System;

namespace Shaders
{
    static public class LightProcessors
    {
        [UniformUsage((int)InternalLightingUsage.ShadowColorSampler, "shadowColor")] static public Sampler2D ShadowColor => throw new NotImplementedException();
        [UniformUsage((int)InternalLightingUsage.ShadowNormalSampler, "shadowNormal")] static public Sampler2D ShadowNormal => throw new NotImplementedException();
        [UniformUsage((int)InternalLightingUsage.ShadowDepthSampler, "shadowDepth")] static public Sampler2D ShadowDepth => throw new NotImplementedException();

        static private Vector3 ProcessAmbientLight([UniformType, UniformUsage((int)InternalLightingUsage.AmbientLight, nameof(InternalLightingUsage.AmbientLight))] AmbientLight ambientLight = null)
        {
            return ambientLight.color.rgb * ambientLight.color.a;
        }

        static private float ProcessShadow(Matrix4 vpMatrix, Vector3 position, Vector3 normal, float normalProduct, float shadowPower)
        {
            //if (normalProduct < 0)
            {
                Vector4 positionInLight = vpMatrix * new Vector4(position, 1.0f);
                positionInLight = positionInLight / positionInLight.w;
                Vector2 shadowUV = positionInLight.xy * new Vector2(0.5f, 0.5f) + new Vector2(0.5f, 0.5f);
                // shadowUV /= 204.8;
                float shadowDepth = ShaderFunctions.MapTexture(LightProcessors.ShadowDepth, shadowUV).r;
                //DebugFunctions.Log("check = %d", positionInLight.z - shadow.r > 0.00001);
                //DebugFunctions.Log("position = %v4f, uv = %v2f, shadow = %v4f, check = %d", positionInLight, shadowUV, shadow, positionInLight.z - shadow.r > 0.00001);
                float upProduct = normal.y; // MathFunctions.Dot(normal, new Vector3(.0f, 1.0f, .0f));
                if (positionInLight.z - shadowDepth > 0.0001f) // 0.0025
                {
                    float power = (1.0f - normalProduct) * (1.0f + upProduct) * 0.25f;
                    //DebugFunctions.Log("position = %v3f, direction = %v3f, normal = %v3f, product = %f, check = %d", position, light.direction.xyz, normal, normalProduct, positionInLight.z - shadow.r > 0.00001);
                    //return shadowPower * (1.0f + normalProduct) * 0.5f;
                    //DebugFunctions.Log("up = %f, normal = %f, value = %f", upProduct, normalProduct, (1.0f - upProduct) * 0.5f * (1.0f - normalProduct) * 0.5f);
                    //DebugFunctions.Log("n=%v3f,sn=%v3f,p=%f,v=%f", normal, shadowNormal, product, shadowPower + (1.0f - shadowPower) * (1.0f - product) * 0.5f);
                    return MathFunctions.Max(1.0f - shadowPower * power, .0f); // (1.0f + upProduct) * 0.5f * (1.0f + normalProduct) * 0.5f;
                    //return shadowPower + (1.0f - shadowPower) * (1.0f - normalProduct) * 0.5f;
                }
            }
            return 1.0f;
        }

        /// <param name="position">position in world</param>
        /// <param name="normal">normal in world</param>
        static private Vector3 ProcessDirectionLight(DirectionalLight light, Vector3 position, Vector3 normal)
        {
            float product = MathFunctions.Dot(light.direction.xyz, -normal);
            Vector3 color = light.color.rgb * (1.0f + light.intensity * product);
            return color * ProcessShadow(light.transform.vpMatrix, position, normal, -product, light.shadowPower);
        }

        /// <param name="position">position in world</param>
        /// <param name="normal">normal in world</param>
        static public Vector3 ProcessAllDirectionalLights(Vector3 position, Vector3 normal, [UniformType, UniformUsage((int)InternalLightingUsage.DirectionalLightArray, nameof(InternalLightingUsage.DirectionalLightArray)), ArraySize(Light.LIGHT_ARRAY_SIZE)] DirectionalLight[] directionalLights = null)
        {
            Vector3 factor = new Vector3(.0f);
            if (1.0f == directionalLights[0].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[0], position, normal);
            }
            if (1.0f == directionalLights[1].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[1], position, normal);
            }
            if (1.0f == directionalLights[2].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[2], position, normal);
            }
            if (1.0f == directionalLights[3].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[3], position, normal);
            }
            if (1.0f == directionalLights[4].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[4], position, normal);
            }
            if (1.0f == directionalLights[5].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[5], position, normal);
            }
            if (1.0f == directionalLights[6].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[6], position, normal);
            }
            if (1.0f == directionalLights[7].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[7], position, normal);
            }
            return factor;
        }

        static private Vector3 ProcessPointLight(Vector3 lightColor, float lightIntensity, float lightRange, float factor, float distance)
        {
            float attenuation = MathFunctions.Max(.0f, 1.0f - distance * distance / (lightRange * lightRange));
            return lightColor * lightIntensity * factor * attenuation; // / MathFunctions.Sqrt(color * color + 1.0f);
        }

        /// <param name="position">position in world</param>
        /// <param name="normal">normal in world</param>
        static private Vector3 ProcessPointLight(PointLight light, Vector3 position, Vector3 normal)
        {
            Vector3 direction = position - light.position.xyz;
            float factor = MathFunctions.Max(MathFunctions.Dot(MathFunctions.Normalize(direction), -normal), .000001f);
            return ProcessPointLight(light.color.rgb, light.intensity, light.range, factor, MathFunctions.Length(direction));
        }

        /// <param name="position">position in world</param>
        /// <param name="normal">normal in world</param>
        static public Vector3 ProcessAllPointLights(Vector3 position, Vector3 normal, [UniformType, UniformUsage((int)InternalLightingUsage.PointLightArray, nameof(InternalLightingUsage.PointLightArray)), ArraySize(Light.LIGHT_ARRAY_SIZE)] PointLight[] pointLights = null)
        {
            Vector3 factor = new Vector3(.0f);
            if (1.0f == pointLights[0].color.a)
            {
                factor += ProcessPointLight(pointLights[0], position, normal);
            }
            if (1.0f == pointLights[1].color.a)
            {
                factor += ProcessPointLight(pointLights[1], position, normal);
            }
            if (1.0f == pointLights[2].color.a)
            {
                factor += ProcessPointLight(pointLights[2], position, normal);
            }
            if (1.0f == pointLights[3].color.a)
            {
                factor += ProcessPointLight(pointLights[3], position, normal);
            }
            if (1.0f == pointLights[4].color.a)
            {
                factor += ProcessPointLight(pointLights[4], position, normal);
            }
            if (1.0f == pointLights[5].color.a)
            {
                factor += ProcessPointLight(pointLights[5], position, normal);
            }
            if (1.0f == pointLights[6].color.a)
            {
                factor += ProcessPointLight(pointLights[6], position, normal);
            }
            if (1.0f == pointLights[7].color.a)
            {
                factor += ProcessPointLight(pointLights[7], position, normal);
            }
            return factor;
        }

        static private Vector3 ProcessSoptLight(SpotLight light, Vector3 position, Vector3 normal)
        {
            Vector3 direction = position - light.position.xyz;
            float angleFactor = MathFunctions.Dot(MathFunctions.Normalize(direction), light.direction.xyz);
            float lightFactor = MathFunctions.Max(MathFunctions.Dot(MathFunctions.Normalize(direction), -normal), .000001f);
            return MathFunctions.Step(light.cutoff, angleFactor) * ProcessPointLight(light.color.rgb, light.intensity, light.range, lightFactor, MathFunctions.Length(direction)) * (1.0f - MathFunctions.Pow(1.0f - angleFactor, 2) / MathFunctions.Pow(1.0f - light.cutoff, 2));
        }

        /// <param name="position">position in world</param>
        /// <param name="normal">normal in world</param>
        static public Vector3 ProcessAllSpotLights(Vector3 position, Vector3 normal, [UniformType, UniformUsage((int)InternalLightingUsage.SpotLightArray, nameof(InternalLightingUsage.SpotLightArray)), ArraySize(Light.LIGHT_ARRAY_SIZE)] SpotLight[] spotLights = null)
        {
            Vector3 factor = new Vector3(.0f);
            if (1.0f == spotLights[0].color.a)
            {
                factor += ProcessSoptLight(spotLights[0], position, normal);
            }
            if (1.0f == spotLights[1].color.a)
            {
                factor += ProcessSoptLight(spotLights[1], position, normal);
            }
            if (1.0f == spotLights[2].color.a)
            {
                factor += ProcessSoptLight(spotLights[2], position, normal);
            }
            if (1.0f == spotLights[3].color.a)
            {
                factor += ProcessSoptLight(spotLights[3], position, normal);
            }
            if (1.0f == spotLights[4].color.a)
            {
                factor += ProcessSoptLight(spotLights[4], position, normal);
            }
            if (1.0f == spotLights[5].color.a)
            {
                factor += ProcessSoptLight(spotLights[5], position, normal);
            }
            if (1.0f == spotLights[6].color.a)
            {
                factor += ProcessSoptLight(spotLights[6], position, normal);
            }
            if (1.0f == spotLights[7].color.a)
            {
                factor += ProcessSoptLight(spotLights[7], position, normal);
            }
            return factor;
        }

        static public Vector3 ProcessAllLights(Vector3 position, Vector3 normal)
        {
            Vector3 lightColor = ProcessAmbientLight();
            lightColor += ProcessAllDirectionalLights(position, normal);
            lightColor += ProcessAllPointLights(position, normal);
            lightColor += ProcessAllSpotLights(position, normal);
            return lightColor;
        }
    }
}
