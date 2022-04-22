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
