using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace General.Shaders
{
    internal class FragmentShaderMethod : Method
    {
        public FragmentShaderMethod(MethodDeclarationSyntax syntax) : base(syntax) { }
    }
}
