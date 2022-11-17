// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class TypeNameAttribute : Attribute
    {
        public Language Language { get; set; }
        public string Name { get; set; }
        public string? DefaultConstructor { get; set; }

        public TypeNameAttribute(Language language, string name)
        {
            this.Language = language;
            this.Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class InstanceNameAttribute : Attribute
    {
        public Language Language { get; set; }
        public string Name { get; set; }

        public InstanceNameAttribute(Language language, string name)
        {
            this.Language = language;
            this.Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class FunctionNameAttribute : Attribute
    {
        public Language Language { get; set; }
        public string Name { get; set; }

        public FunctionNameAttribute(Language language, string name)
        {
            this.Language = language;
            this.Name = name;
        }
    }
}
