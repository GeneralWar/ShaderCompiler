using System;

namespace General.Shaders
{
    static public class MathFunctions
    {
        [FunctionName(Language.GLSL, "dot")]
        static public float Dot(Vector3 v1, Vector3 v2) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "clamp")]
        static public float Clamp(float v, float min, float max) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "length")]
        static public float Length(Vector2 v) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "length")]
        static public float Length(Vector3 v) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "length")]
        static public float Length(Vector4 v) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "normalize")]
        static public Vector2 Normalize(Vector2 v) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "normalize")]
        static public Vector3 Normalize(Vector3 v) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "normalize")]
        static public Vector4 Normalize(Vector4 v) => throw new NotImplementedException();
    }
}
