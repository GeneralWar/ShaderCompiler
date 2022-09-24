using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace General.Shaders
{
    public enum UniformType
    {
        Transform,
        Sampler2D,
        Vector4,

        AmbientLight,
        DirectionalLightArray,
        PointLightArray,
        SpoitLightArray,

        Custom = 0xffff,
    }

    public enum ShaderStage
    {
        VertexShader,
        FragmentShader,
    }

    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct UniformDeclaration
    {
        [DataMember] public UniformType Type;
        [DataMember] public ShaderStage Stage;
        [DataMember] [MarshalAs(UnmanagedType.LPStr)] public string? Name;

        public UniformDeclaration(UniformType type, ShaderStage stage, string? name)
        {
            this.Type = type;
            this.Stage = stage;
            this.Name = name;
        }

        public override string ToString()
        {
            return $"{(string.IsNullOrWhiteSpace(this.Name) ? "(NoName)" : this.Name)}, {this.Type}, {this.Stage}";
        }
    }
}
