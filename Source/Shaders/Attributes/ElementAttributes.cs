using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class MemberCollectorAttribute : Attribute { }
}
