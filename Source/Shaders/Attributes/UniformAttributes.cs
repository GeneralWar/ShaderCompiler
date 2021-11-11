// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class UniformDataAttribute : Attribute
    {
        public UniformDataAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class UniformFieldAttribute : Attribute
    {
        /// <summary>
        /// Type which is declaring this field
        /// </summary>
        /// <value></value>
        public Type DeclaringType { get; set; }

        public UniformFieldAttribute(Type declaringType)
        {
            this.DeclaringType = declaringType;
        }
    }
}
