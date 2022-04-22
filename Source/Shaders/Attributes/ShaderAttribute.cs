// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;

namespace General.Shaders
{
    public abstract class ShaderPathAttribute : Attribute
    {
        public string Path { get; set; }

        public ShaderPathAttribute(string path)
        {
            this.Path = path;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class VertexShaderAttribute : ShaderPathAttribute
    {

        public VertexShaderAttribute(string path) : base(path) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FragmentShaderAttribute : ShaderPathAttribute
    {
        public FragmentShaderAttribute(string path) : base(path) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GraphicsShaderAttribute : ShaderPathAttribute
    {
        public int Queue { get; set; }
        public RenderType Type { get; set; }

        public GraphicsShaderAttribute(string path, RenderType type, int queue) : base(path)
        {
            this.Type = type;
            this.Queue = queue;
        }

        public GraphicsShaderAttribute(string path, RenderType type, RenderQueue queue) : this(path, type, (int)queue) { }
    }
}
