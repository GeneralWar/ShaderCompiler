﻿// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System.Runtime.Serialization;

namespace General.Shaders
{
    [DataContract]
    public class ShaderConfig
    {
        [DataMember] public string key = "";
        [DataMember] public string? vertexShader = null;
        [DataMember] public string? fragmentShader = null;
        [DataMember] public RenderType type;
        [DataMember] public int queue;
        [DataMember] public PolygonMode polygonMode;
        [DataMember] public PolygonType[] polygonTypes = new PolygonType[0];
        [DataMember] public UniformDeclaration[] uniforms = new UniformDeclaration[0];

        public ShaderConfig() { }

        public ShaderConfig(string key, RenderType type, int queue)
        {
            this.key = key;
            this.type = type;
            this.queue = queue;
        }
    }
}
