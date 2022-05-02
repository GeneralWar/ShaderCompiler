using General.Shaders;

namespace Shaders
{
    static public class LightProcessors
    {
        static private Vector3 ProcessAmbientLight([UniformType] AmbientLight ambientLight = null)
        {
            return ambientLight.color.rgb * ambientLight.color.a;
        }

        static private Vector3 ProcessDirectionLight(DirectionalLight light, Vector3 normal)
        {
            return light.color.rgb * (1.0f + light.direction.w * MathFunctions.Dot(light.direction.xyz, -normal));
            //return light.color.rgb * light.direction.w * MathFunctions.Clamp(MathFunctions.Dot(light.direction.xyz, normal), .0f, 1.0f);
        }

        static public Vector3 ProcessAllDirectionalLights(Vector3 normal, [UniformType, ArraySize(8)] DirectionalLight[] directionalLights = null)
        {
            Vector3 factor = new Vector3(.0f);
            if (1.0f == directionalLights[0].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[0], normal);
            }
            if (1.0f == directionalLights[1].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[1], normal);
            }
            if (1.0f == directionalLights[2].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[2], normal);
            }
            if (1.0f == directionalLights[3].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[3], normal);
            }
            if (1.0f == directionalLights[4].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[4], normal);
            }
            if (1.0f == directionalLights[5].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[5], normal);
            }
            if (1.0f == directionalLights[6].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[6], normal);
            }
            if (1.0f == directionalLights[7].color.a)
            {
                factor += ProcessDirectionLight(directionalLights[7], normal);
            }
            return factor;
        }

        static private Vector3 ProcessPointLight(Vector3 lightColor, float lightIntensity, float lightRange, float factor, float distance)
        {
            float attenuation = MathFunctions.Max(.0f, 1.0f - distance * distance / (lightRange * lightRange));
            return lightColor * lightIntensity * factor * attenuation; // / MathFunctions.Sqrt(color * color + 1.0f);
        }

        static private Vector3 ProcessPointLight(PointLight light, Vector3 position, Vector3 normal)
        {
            Vector3 direction = position - light.position.xyz;
            float factor = MathFunctions.Max(MathFunctions.Dot(MathFunctions.Normalize(direction), -normal), .000001f);
            return ProcessPointLight(light.color.rgb, light.intensity, light.range, factor, MathFunctions.Length(direction));
        }

        static public Vector3 ProcessAllPointLights(Vector3 position, Vector3 normal, [UniformType, ArraySize(8)] PointLight[] pointLights = null)
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

        static public Vector3 ProcessAllSpotLights(Vector3 position, Vector3 normal, [UniformType, ArraySize(8)] SpotLight[] spotLights = null)
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

        static public Vector3 ProcessAllLights(Vector3 position, Vector3 normal, [UniformType] AmbientLight ambient = null, [UniformType, ArraySize(8)] DirectionalLight[] directionalLights = null, [UniformType, ArraySize(8)] PointLight[] pointLights = null, [UniformType, ArraySize(8)] SpotLight[] spotLights = null)
        {
            Vector3 lightColor = ProcessAmbientLight();
            lightColor += ProcessAllDirectionalLights(normal);
            lightColor += ProcessAllPointLights(position, normal);
            lightColor += ProcessAllSpotLights(position, normal);
            return lightColor;
        }
    }
}
