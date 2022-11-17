// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Parameter)]
    public class UniformTypeAttribute : Attribute
    {
        public UniformType Type { get; private set; }

        public UniformTypeAttribute() : this(UniformType.Custom) { }

        public UniformTypeAttribute(UniformType type)
        {
            this.Type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class UniformUsageAttribute : Attribute
    {
        public int Usage { get; private set; }
        public string? DisplayName { get; set; }

        public UniformUsageAttribute(int usage) : this(usage, null) { }

        public UniformUsageAttribute(int usage, string? displayName)
        {
            this.Usage = usage;
            this.DisplayName = displayName;
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
