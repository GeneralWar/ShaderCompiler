namespace General.Shaders
{
    class CompileContext
    {
        public string? InputDeclaration { get; private set; } = null;
        public string? OutputDeclaration { get; private set; } = null;

        public string? VertexShader { get; private set; } = null;
        public string? FragmentShader { get; private set; } = null;

        public void SetInputDeclaration(string input)
        {
            this.InputDeclaration = input;
        }

        public void SetOutputDeclaration(string output)
        {
            this.OutputDeclaration = output;
        }

        public void SetVertexShader(string vertex)
        {
            this.VertexShader = vertex;
        }

        public void SetFragmentShader(string fragment)
        {
            this.FragmentShader = fragment;
        }
    }
}
