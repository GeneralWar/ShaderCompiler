// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace General.Shaders
{
    partial class GLSLCompiler : Compiler
    {
        internal class UniformProperty
        {
            public UniformTypeAttribute TypeAttribute { get; init; }
            public UniformUsageAttribute UsageAttribute { get; init; }
            /// <summary>
            /// Defined by <see cref="UniformUsageAttribute"/>, as display text
            /// </summary>
            public string? PublicName => this.UsageAttribute.DisplayName;

            private string mUniformName;
            public string UniformName => mUniformName;
            public Type SourceType { get; init; }

            public Type DeclaringType { get; init; }
            public ShaderStage Stage { get; init; }
            public string InstanceName { get; init; }
            public string PlaceHolder => "{binding-" + this.InstanceName + "}";

            public string? Content { get; private set; }

            public UniformProperty(UniformTypeAttribute typeAttribute, UniformUsageAttribute usageAttribute, Type declaringType, Type sourceType, string instanceName)
            {
                this.TypeAttribute = typeAttribute;
                this.UsageAttribute = usageAttribute;
                this.DeclaringType = declaringType;
                this.SourceType = sourceType;
                this.InstanceName = instanceName;

                mUniformName = $"Uniform{char.ToUpper(instanceName[0]) + instanceName.Substring(1)}";
                if (sourceType.IsArray)
                {
                    mUniformName = mUniformName.TrimEnd('[', ']') + "Array";
                }

                if (this.DeclaringType.ImplementedInterface<IVertexSource>())
                {
                    this.Stage = ShaderStage.VertexShader;
                }
                else if (this.DeclaringType.ImplementedInterface<IFragmentSource>())
                {
                    this.Stage = ShaderStage.FragmentShader;
                }
                Trace.Assert(ShaderStage.None != this.Stage);
            }

            public void SetContent(string value)
            {
                this.Content = value;
            }

            public UniformDeclaration ToDeclaration()
            {
                return new UniformDeclaration(this.TypeAttribute.Type, this.Stage, this.UsageAttribute.Usage, this.PublicName);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(this.SourceType, this.InstanceName);
            }

            public override bool Equals(object? obj)
            {
                UniformProperty? other = obj as UniformProperty;
                return other is not null && other.SourceType == this.SourceType && other.InstanceName == this.InstanceName;
            }

            public override string ToString()
            {
                return $"{this.InstanceName}({this.SourceType.FullName ?? this.SourceType.Name})";
            }
        }

        internal class ComponentData
        {
            /// <summary>
            /// Shader component type
            /// </summary>
            public Type Type { get; private set; }

            private List<UniformProperty> mUniforms = new List<UniformProperty>();
            public UniformProperty[] Uniforms => mUniforms.ToArray();

            private HashSet<string> mExtensions = new HashSet<string>();
            public HashSet<string> Extensions => mExtensions;

            /// <param name="type">Shader component type, such as <see cref="DefaultVertexShader"/></param>
            public ComponentData(Type type)
            {
                this.Type = type;
            }

            public void AddUniformProperty(UniformProperty property)
            {
                mUniforms.Add(property);
            }

            public void AddExtension(string extension)
            {
                mExtensions.Add(extension);
            }
        }

        private Dictionary<Type, ComponentData> mComponentDataMap = new Dictionary<Type, ComponentData>();

        private Stack<ComponentData> mComponentDataStack = new Stack<ComponentData>();
        public ComponentData? CurrentComponentData => mComponentDataStack.Count > 0 ? mComponentDataStack.Peek() : null;

        private ComponentData getComponentData(Type type)
        {
            ComponentData? data;
            if (!mComponentDataMap.TryGetValue(type, out data))
            {
                mComponentDataMap.Add(type, data = new ComponentData(type));
            }
            return data;
        }

        internal override bool PushScope(Declaration scope)
        {
            if (!base.PushScope(scope))
            {
                return false;
            }

            Class? classDeclaration = scope as Class;
            if (classDeclaration is not null && classDeclaration.Type.IsShaderComponent())
            {
                mComponentDataStack.Push(this.getComponentData(classDeclaration.Type));
            }
            return true;
        }

        internal override void PopScope(Declaration scope)
        {
            Class? classDeclaration = scope as Class;
            if (classDeclaration is not null && classDeclaration.Type.IsShaderComponent())
            {
                Trace.Assert(mComponentDataStack.Peek()==this.getComponentData(classDeclaration.Type));
                mComponentDataStack.Pop();
            }

            base.PopScope(scope);
        }
    }
}
