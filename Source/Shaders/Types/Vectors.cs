using System;

namespace General.Shaders
{
    [TypeName(Language.GLSL, "vec2")]
    public struct Vector2
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    [TypeName(Language.GLSL, "vec3")]
    public struct Vector3
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    [TypeName(Language.GLSL, "vec4")]
    public struct Vector4
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }

        public Vector4(Vector3 v, float w) { throw new NotImplementedException(); }
        public Vector4(float x, float y, float z, float w) { throw new NotImplementedException(); }

        static public Vector4 operator *(Matrix4 matrix, Vector4 v) { throw new NotImplementedException(); }
    }
}
