// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace General.Shaders
{
    interface ISyntaxHost
    {
        SyntaxNode SyntaxNode { get; }
    }

    interface ITypeHost
    {
        System.Type Type { get; }
    }

    internal interface IReferenceHost
    {
        IEnumerable<Declaration> References { get; }

        void AddReference(Declaration reference);
    }

    internal interface IUniformHost
    {
        IEnumerable<Declaration> Uniforms { get; }

        void AddUniform(Declaration uniform);
    }
}
