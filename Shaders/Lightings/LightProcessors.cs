using General.Shaders;

namespace Shaders
{
    static public class LightProcessors
    {
        static private Vector4 ProcessDirectionLight(DirectionLight light, Vector3 normal)
        {
            return light.color * MathFunctions.Dot(light.direction, normal);
        }

        static public Vector4 ProcessAllLights(InputFragment input, [LayoutPushConstant][ArraySize(8)] DirectionLight[] lights = null)
        {
            Vector4 color = new Vector4();
            if (1.0f == lights[0].enabled)
            {
                color += ProcessDirectionLight(lights[0], input.normal);
            }
            if (1.0f == lights[1].enabled)
            {
                color += ProcessDirectionLight(lights[1], input.normal);
            }
            if (1.0f == lights[2].enabled)
            {
                color += ProcessDirectionLight(lights[2], input.normal);
            }
            if (1.0f == lights[3].enabled)
            {
                color += ProcessDirectionLight(lights[3], input.normal);
            }
            if (1.0f == lights[4].enabled)
            {
                color += ProcessDirectionLight(lights[4], input.normal);
            }
            if (1.0f == lights[5].enabled)
            {
                color += ProcessDirectionLight(lights[5], input.normal);
            }
            if (1.0f == lights[6].enabled)
            {
                color += ProcessDirectionLight(lights[6], input.normal);
            }
            if (1.0f == lights[7].enabled)
            {
                color += ProcessDirectionLight(lights[7], input.normal);
            }
            return color;
        }
    }
}
