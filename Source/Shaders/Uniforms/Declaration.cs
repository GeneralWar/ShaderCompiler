using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace General.Shaders
{
    public enum UniformType
    {
        Transform,
        Sampler2D,
    }

    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct UniformDeclaration
    {
        [DataMember] public UniformType Type;
        [DataMember] [MarshalAs(UnmanagedType.LPStr)] public string? Name;

        public UniformDeclaration(UniformType type, string? name)
        {
            this.Type = type;
            this.Name = name;
        }
    }
}
