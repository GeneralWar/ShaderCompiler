// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;
using System.Runtime.InteropServices;

namespace General.Shaders
{
    [MemberCollector]
    [TypeName(Language.GLSL, "vec2", DefaultConstructor = "vec2(0)")]
    public struct Vector2
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    [MemberCollector]
    [TypeName(Language.GLSL, "vec3", DefaultConstructor = "vec3(0)")]
    public struct Vector3
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    [MemberCollector]
    [TypeName(Language.GLSL, "vec4", DefaultConstructor = "vec4(0)")]
    [StructLayout(LayoutKind.Explicit)]
    public struct Vector4
    {
        [FieldOffset(0)] public float x;
        [FieldOffset(0)] public float r;
        [FieldOffset(4)] public float y;
        [FieldOffset(4)] public float g;
        [FieldOffset(8)] public float z;
        [FieldOffset(8)] public float b;
        [FieldOffset(12)] public float w;
        [FieldOffset(12)] public float a;

        public Vector4(Vector3 v, float w) { throw new NotImplementedException(); }
        public Vector4(float x, float y, float z, float w) { throw new NotImplementedException(); }

        public Vector4 this[string key] { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        static public Vector4 operator +(Vector4 v1, Vector4 v2) { throw new NotImplementedException(); }
        static public Vector4 operator *(Matrix4 matrix, Vector4 v) { throw new NotImplementedException(); }
        static public Vector4 operator *(Vector4 v1, Vector4 v2) { throw new NotImplementedException(); }
        static public Vector4 operator *(Vector4 v, float n) { throw new NotImplementedException(); }

        static public implicit operator Vector3(Vector4 v) { throw new NotImplementedException(); }
    }
}
