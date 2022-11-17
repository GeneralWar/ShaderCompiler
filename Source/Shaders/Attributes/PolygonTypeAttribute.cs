// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class PolygonTypeAttribute : Attribute
    {
        public PolygonType polygonType;

        public PolygonTypeAttribute(PolygonType polygonType)
        {
            this.polygonType = polygonType;
        }
    }
}
