// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Parameter)]
    public class UniformTypeAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class UniformNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public UniformNameAttribute(string name)
        {
            this.Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class UniformFieldAttribute : Attribute
    {
        public int Index { get; set; }

        /// <summary>
        /// Type which is declaring this field
        /// </summary>
        /// <value></value>
        public Type DeclaringType { get; set; }

        public UniformFieldAttribute(Type declaringType, int index)
        {
            this.Index = index;
            this.DeclaringType = declaringType;
        }
    }
}
