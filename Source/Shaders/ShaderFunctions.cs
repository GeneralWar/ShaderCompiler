using System;

namespace General.Shaders
{
    static public class ShaderFunctions
    {
        [FunctionName(Language.GLSL, "texture")]
        static public Vector4 MapTexture(Sampler2D texture, Vector2 uv)
        {
            throw new NotImplementedException();
        }
    }
}
