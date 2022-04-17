// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders.Uniforms;
using System;

namespace General.Shaders
{
    partial class GLSLCompiler : Compiler
    {
        class UniformProperty
        {
            public Type Type { get; private set; }
            public string PropertyName { get; private set; }
            public string? PublicName { get; private set; }

            public UniformProperty(Type type, string propertyName)
            {
                this.Type = type;
                this.PropertyName = propertyName;
            }

            public void SetPublicName(string value)
            {
                this.PublicName = value;
            }

            public UniformType UniformType
            {
                get
                {
                    if (typeof(Transform) == this.Type)
                    {
                        return UniformType.Transform;
                    }
                    if (typeof(Sampler2D) == this.Type)
                    {
                        return UniformType.Sampler2D;
                    }
                    throw new InvalidOperationException();
                }
            }

            public UniformDeclaration ToDeclaration()
            {
                return new UniformDeclaration(this.UniformType, this.PublicName);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(this.Type, this.PropertyName);
            }

            public override bool Equals(object? obj)
            {
                UniformProperty? other = obj as UniformProperty;
                return other is not null && other.Type == this.Type && other.PropertyName == this.PropertyName;
            }

            public override string ToString()
            {
                return $"{this.PropertyName}({this.Type.FullName ?? this.Type.Name})";
            }
        }
    }
}
