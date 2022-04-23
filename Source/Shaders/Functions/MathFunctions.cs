using System;

namespace General.Shaders
{
    static public class MathFunctions
    {
        [FunctionName(Language.GLSL, "dot")]
        static public float Dot(Vector3 v1, Vector3 v2) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "clamp")]
        static public float Clamp(float v, float min, float max) => throw new NotImplementedException();
    }
}
