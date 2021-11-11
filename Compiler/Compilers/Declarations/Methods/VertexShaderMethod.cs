using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace General.Shaders
{
    internal class VertexShaderMethod : Method
    {
        public VertexShaderMethod(MethodDeclarationSyntax syntax) : base(syntax) { }
    }
}
