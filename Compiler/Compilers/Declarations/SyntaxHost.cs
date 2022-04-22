using Microsoft.CodeAnalysis;

namespace General.Shaders
{
    interface ISyntaxHost
    {
        SyntaxNode SyntaxNode { get; }
    }
}
