﻿using System;

namespace General.Shaders
{
    public class LayoutBindingAttribute : Attribute
    {
        public int Index { get; set; }

        public LayoutBindingAttribute(int index)
        {
            this.Index = index;
        }
    }

    public class LayoutLocationAttribute : Attribute
    {
        public int Index { get; set; }

        public LayoutLocationAttribute(int index)
        {
            this.Index = index;
        }
    }
}
