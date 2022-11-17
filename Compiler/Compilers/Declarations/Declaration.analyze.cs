// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace General.Shaders
{
    public abstract partial class Declaration
    {
        static protected void AnalyzeAttributes(Declaration host, Declaration declaration, SyntaxList<AttributeListSyntax> attributeSyntaxList)
        {
            foreach (AttributeListSyntax attributeListSyntax in attributeSyntaxList)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    Attribute attribute = new Attribute(declaration.Root, attributeSyntax);
                    declaration.AddAttribute(attribute);

                    if (typeof(UniformUsageAttribute) == attribute.Type)
                    {
                        (host as IUniformHost ?? throw new InvalidOperationException($"Find {nameof(UniformUsageAttribute)} in a declaration which is not an {nameof(IUniformHost)}")).AddUniform(declaration);
                    }
                }
            }
        }
    }
}
