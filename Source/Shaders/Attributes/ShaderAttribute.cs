using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ShaderAttribute : Attribute
    {
        public string Path { get; set; }

        public ShaderAttribute(string path)
        {
            this.Path = path;
        }
    }
}
