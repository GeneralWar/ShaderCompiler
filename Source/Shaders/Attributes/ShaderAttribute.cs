using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class VertexShaderAttribute : Attribute
    {
        public string Path { get; set; }

        public VertexShaderAttribute(string path)
        {
            this.Path = path;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FragmentShaderAttribute : Attribute
    {
        public string Path { get; set; }

        public FragmentShaderAttribute(string path)
        {
            this.Path = path;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GraphicsShaderAttribute : Attribute
    {
        public string Path { get; set; }
        public int Queue { get; set; }
        public RenderType Type { get; set; }

        public GraphicsShaderAttribute(string path, RenderType type, int queue)
        {
            this.Path = path;
            this.Type = type;
            this.Queue = queue;
        }

        public GraphicsShaderAttribute(string path, RenderType type, RenderQueue queue)
        {
            this.Path = path;
            this.Type = type;
            this.Queue = (int)queue;
        }
    }
}
