// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class InputVertexAttribute : Attribute
    {
        public InputField Field { get; set; }

        public InputVertexAttribute(InputField field)
        {
            this.Field = field;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class OutputVertexAttribute : Attribute
    {
        public OutputField Field { get; set; }

        public OutputVertexAttribute(OutputField field)
        {
            this.Field = field;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class InputFragmentAttribute : Attribute
    {
        public InputField Field { get; set; }

        public InputFragmentAttribute(InputField field)
        {
            this.Field = field;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class OutputFragmentAttribute : Attribute
    {
        public OutputField Field { get; set; }

        public OutputFragmentAttribute(OutputField field)
        {
            this.Field = field;
        }
    }
}
