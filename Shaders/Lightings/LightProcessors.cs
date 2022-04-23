using General.Shaders;

namespace Shaders
{
    static public class LightProcessors
    {
        static private Vector3 ProcessDirectionLight(DirectionalLight light, Vector3 normal)
        {
            return light.color.rgb * MathFunctions.Dot(light.direction.rgb, normal) * light.intensity;
            //return light.color.rgb * MathFunctions.Clamp(MathFunctions.Dot(light.direction, normal), .0f, 1.0f) * light.intensity;
        }

        static public Vector3 ProcessAllLights(Vector3 color, Vector3 normal, [UniformType, ArraySize(8)] DirectionalLight[] lights = null)
        {
            if (1.0f == lights[0].enabled)
            {
                color *= ProcessDirectionLight(lights[0], normal);
            }
            if (1.0f == lights[1].enabled)
            {
                color *= ProcessDirectionLight(lights[1], normal);
            }
            if (1.0f == lights[2].enabled)
            {
                color *= ProcessDirectionLight(lights[2], normal);
            }
            if (1.0f == lights[3].enabled)
            {
                color *= ProcessDirectionLight(lights[3], normal);
            }
            if (1.0f == lights[4].enabled)
            {
                color *= ProcessDirectionLight(lights[4], normal);
            }
            if (1.0f == lights[5].enabled)
            {
                color *= ProcessDirectionLight(lights[5], normal);
            }
            if (1.0f == lights[6].enabled)
            {
                color *= ProcessDirectionLight(lights[6], normal);
            }
            if (1.0f == lights[7].enabled)
            {
                color *= ProcessDirectionLight(lights[7], normal);
            }
            return color;
        }
    }
}
