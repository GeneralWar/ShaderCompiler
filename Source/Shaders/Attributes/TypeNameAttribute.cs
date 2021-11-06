using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    class TypeNameAttribute : Attribute
    {
        public Language language { get; set; }
        public string name { get; set; }

        public TypeNameAttribute(Language language, string name)
        {
            this.language = language;
            this.name = name;
        }
    }
}
