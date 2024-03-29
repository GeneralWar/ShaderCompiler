﻿// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;
using System.Runtime.InteropServices;

namespace General.Shaders.Maths
{
    [MemberCollector]
    [TypeName(Language.GLSL, "vec2", DefaultConstructor = "vec2(0)")]
    public struct Vector2
    {
        public float x { get; set; }
        public float y { get; set; }

        public Vector2(float x, float y) { throw new NotImplementedException(); }

        static public Vector2 operator +(Vector2 v1, Vector2 v2) { throw new NotImplementedException(); }
        static public Vector2 operator +(Vector2 v1, float n) { throw new NotImplementedException(); }
        static public Vector2 operator -(Vector2 v1, Vector2 v2) { throw new NotImplementedException(); }
        static public Vector2 operator -(Vector2 v1, float n) { throw new NotImplementedException(); }
        static public Vector2 operator *(Vector2 v1, Vector2 v2) { throw new NotImplementedException(); }
        static public Vector2 operator *(Vector2 v1, float n) { throw new NotImplementedException(); }
        static public Vector2 operator /(Vector2 v1, Vector2 v2) { throw new NotImplementedException(); }
        static public Vector2 operator /(Vector2 v1, float n) { throw new NotImplementedException(); }
    }

    [MemberCollector]
    [TypeName(Language.GLSL, "vec3", DefaultConstructor = "vec3(0)")]
    public struct Vector3
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public Vector3(float v) { throw new NotImplementedException(); }
        public Vector3(float x, float y, float z) { throw new NotImplementedException(); }
        public Vector3(Vector2 v, float z) { throw new NotImplementedException(); }

        static public Vector3 operator +(Vector3 v1, Vector3 v2) { throw new NotImplementedException(); }
        static public Vector3 operator +(Vector3 v, float n) { throw new NotImplementedException(); }
        static public Vector3 operator -(Vector3 v1, Vector3 v2) { throw new NotImplementedException(); }
        static public Vector3 operator -(Vector3 v, float n) { throw new NotImplementedException(); }
        static public Vector3 operator -(Vector3 v) { throw new NotImplementedException(); }

        static public Vector3 operator *(Vector3 v1, Vector3 v2) { throw new NotImplementedException(); }
        static public Vector3 operator *(Vector3 v, float n) { throw new NotImplementedException(); }
        static public Vector3 operator *(float n, Vector3 v) { throw new NotImplementedException(); }
        static public Vector3 operator /(Vector3 v, float n) { throw new NotImplementedException(); }
    }

    [MemberCollector]
    [UniformType(UniformType.Vector4)]
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

        [FieldOffset(0)] public Vector2 xy;

        [FieldOffset(0)] public Vector3 rgb;
        [FieldOffset(0)] public Vector3 xyz;

        public Vector4(float v) { throw new NotImplementedException(); }
        public Vector4(Vector3 v, float w) { throw new NotImplementedException(); }
        public Vector4(float x, float y, float z, float w) { throw new NotImplementedException(); }

        static public Vector4 operator +(Vector4 v1, Vector4 v2) { throw new NotImplementedException(); }
        static public Vector3 operator +(Vector4 v, float n) { throw new NotImplementedException(); }
        static public Vector4 operator -(Vector4 v1, Vector4 v2) { throw new NotImplementedException(); }
        static public Vector3 operator -(Vector4 v, float n) { throw new NotImplementedException(); }
        static public Vector4 operator -(Vector4 v) { throw new NotImplementedException(); }

        static public Vector4 operator *(Matrix4 matrix, Vector4 v) { throw new NotImplementedException(); }
        static public Vector4 operator *(Vector4 v1, Vector4 v2) { throw new NotImplementedException(); }
        static public Vector4 operator *(Vector4 v, float n) { throw new NotImplementedException(); }
        static public Vector4 operator *(float n, Vector4 v) { throw new NotImplementedException(); }
        static public Vector4 operator /(Vector4 v, float n) { throw new NotImplementedException(); }
    }
}
