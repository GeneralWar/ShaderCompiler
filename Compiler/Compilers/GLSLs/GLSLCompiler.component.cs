// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders.Uniforms;
using System;
using System.Collections.Generic;
using System.IO;

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
                    return ConvertToUniformType(this.Type);
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

            static private UniformType ConvertToUniformType(Type type)
            {
                if (type.IsArray)
                {
                    Type elementType = type.GetElementType() ?? throw new InvalidDataException("Arrays must have element type");
                    if (typeof(DirectionalLight) == elementType)
                    {
                        return UniformType.DirectionalLightArray;
                    }

                    throw new InvalidOperationException();
                }

                if (typeof(Transform) == type)
                {
                    return UniformType.Transform;
                }
                if (typeof(Sampler2D) == type)
                {
                    return UniformType.Sampler2D;
                }
                throw new InvalidOperationException();
            }
        }

        class ComponentData
        {
            /// <summary>
            /// Shader component type
            /// </summary>
            public Type Type { get; private set; }

            private List<UniformProperty> mUniforms = new List<UniformProperty>();
            public UniformProperty[] Uniforms => mUniforms.ToArray();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="type">Shader component type, such as <see cref="DefaultVertexShader"/></param>
            public ComponentData(Type type)
            {
                this.Type = type;
            }

            public void AddUniformProperty(UniformProperty property)
            {
                mUniforms.Add(property);
            }
        }

        private Dictionary<Type, ComponentData> mComponentDataMap = new Dictionary<Type, ComponentData>();

        private ComponentData getComponentData(Type type)
        {
            ComponentData? data;
            if (!mComponentDataMap.TryGetValue(type, out data))
            {
                mComponentDataMap.Add(type, data = new ComponentData(type));
            }
            return data;
        }
    }
}
