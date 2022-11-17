// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ArraySizeAttribute : Attribute
    {
        public int ElementCount;

        public ArraySizeAttribute(int elementCount)
        {
            this.ElementCount = elementCount;
        }
    }
}
