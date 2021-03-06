// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;

namespace General.Shaders
{
    public class LayoutLocationAttribute : Attribute
    {
        public int Index { get; set; }

        public LayoutLocationAttribute(int index)
        {
            this.Index = index;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class LayoutPushConstantAttribute : Attribute { }
}
