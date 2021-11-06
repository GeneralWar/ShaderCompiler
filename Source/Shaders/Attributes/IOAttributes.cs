using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class InputAttribute : Attribute
    {
        public InputField Field { get; set; }

        public InputAttribute(InputField field)
        {
            this.Field = field;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class OutputAttribute : Attribute
    {
        public OutputField Field { get; set; }

        public OutputAttribute(OutputField field)
        {
            this.Field = field;
        }
    }
}
