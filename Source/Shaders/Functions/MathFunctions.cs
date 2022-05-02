using System;

namespace General.Shaders
{
    static public class MathFunctions
    {
        [FunctionName(Language.GLSL, "abs")] 
        static public float Abs(float v) => throw new NotImplementedException();
        [FunctionName(Language.GLSL, "pow")] 
        static public float Pow(float v, float x) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "sqrt")]
        static public float Sqrt(float v) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "sqrt")]
        static public float Sqrt(Vector2 v) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "sqrt")]
        static public float Sqrt(Vector3 v) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "sqrt")]
        static public float Sqrt(Vector4 v) => throw new NotImplementedException();

        /// <returns>0.0 is returned if x[i] < edge[i], and 1.0 is returned otherwise</returns>
        [FunctionName(Language.GLSL, "step")]
        static public float Step(float edge, float v) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "acos")]
        static public float Acos(float v) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "min")] 
        static public float Min(float v1, float v2) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "max")]
        static public float Max(float v1, float v2) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "clamp")]
        static public float Clamp(float v, float min, float max) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "distance")] 
        static public float Distance(Vector3 v1, Vector3 v2) => throw new NotImplementedException();

        [FunctionName(Language.GLSL, "dot")]
        static public float Dot(Vector3 v1, Vector3 v2) => throw new NotImplementedException();

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
