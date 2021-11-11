using System;

namespace General.Shaders
{
    public abstract class GraphicsShader
    {
        public abstract IVertexSource VertexShader { get; }
        public abstract IFragmentSource FragmentShader { get; }
    }
}
