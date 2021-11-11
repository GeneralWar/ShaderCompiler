namespace General.Shaders
{
    class GLSLCompileContext : CompileContext
    {
        public string? UniformDeclaration { get; private set; } = null;

        public void SetUniformDeclaration(string uniforms)
        {
            this.UniformDeclaration = uniforms;
        }
    }
}
