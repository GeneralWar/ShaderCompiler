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
