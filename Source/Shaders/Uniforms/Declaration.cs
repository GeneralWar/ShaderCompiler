﻿// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace General.Shaders
{
    public enum UniformType
    {
        None,

        Transform, // internal custom
        Sampler2D,
        Vector4,

        SubpassInput = 1 << 4,

        Custom = 0xffff,
    }

    public enum InternalUniformUsage
    {
        None,

        Transform,
        MainColor,
        Diffuse,

        SamplerColor,
        SamplerPosition,
        SamplerNormal,
    }

    public enum ShaderStage
    {
        None,
        VertexShader,
        FragmentShader,
    }

    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct UniformDeclaration
    {
        [DataMember] public UniformType Type;
        [DataMember] public ShaderStage Stage;
        [DataMember] public int Usage;
        [DataMember] [MarshalAs(UnmanagedType.LPStr)] public string? Name;

        public UniformDeclaration(UniformType type, ShaderStage stage, int usage, string? name)
        {
            this.Type = type;
            this.Usage = usage;
            this.Stage = stage;
            this.Name = name;
        }

        public override string ToString()
        {
            return $"{(string.IsNullOrWhiteSpace(this.Name) ? "(NoName)" : this.Name)}, {this.Type}, {this.Stage}, {this.Usage}";
        }
    }
}
