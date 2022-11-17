// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace General.Shaders
{
    class GLSLCompileContext : CompileContext
    {
        private GLSLCompiler mCompiler;

        public List<GLSLCompiler.UniformProperty> mUniforms = new List<GLSLCompiler.UniformProperty>();
        public IEnumerable<GLSLCompiler.UniformProperty> Uniforms => mUniforms;

        public Dictionary<string, string> mPushConstants = new Dictionary<string, string>();
        public IEnumerable<string> PushConstants => mPushConstants.Values;

        public GLSLCompileContext(GLSLCompiler compiler, Declaration root) : base(compiler, root)
        {
            mCompiler = compiler;
        }

        public void AddUniform(GLSLCompiler.UniformProperty uniform)
        {
            if (this.ContainsUniform(uniform.UniformName))
            {
                return;
            }

            mCompiler.CurrentComponentData?.AddUniformProperty(uniform);
            mUniforms.Add(uniform);
        }

        protected override void internalAddUniform(UniformUsageAttribute attribute, Declaration declaration)
        {
            Field? field = declaration as Field;
            if (field is not null)
            {
                this.addUniform(attribute, field, field.Type);
                return;
            }

            Property? property = declaration as Property;
            if (property is not null)
            {
                this.addUniform(attribute, property, property.Type);
                return;
            }

            throw new NotImplementedException();
        }

        protected void addUniform(UniformUsageAttribute usageAttribute, Declaration declaration, Type sourceType)
        {
            UniformTypeAttribute? typeAttribute = sourceType.GetCustomAttribute<UniformTypeAttribute>();
            if (typeAttribute is null)
            {
                throw new NotImplementedException("Maybe there are other conditions");
            }

            GLSLCompiler.ComponentData? componentData = mCompiler.CurrentComponentData;
            if (componentData is null)
            {
                throw new InvalidOperationException($"{nameof(GLSLCompiler.UniformProperty)} must appear within a {nameof(GLSLCompiler.ComponentData)}");
            }

            GLSLCompiler.UniformProperty uniform = new GLSLCompiler.UniformProperty(typeAttribute, usageAttribute, componentData.Type, sourceType, declaration.Name);
            if (this.ContainsUniform(uniform.UniformName))
            {
                return;
            }

            uniform.SetContent(mCompiler.CompileUniformContent(this, uniform, sourceType));
            this.AddUniform(uniform);
        }

        public bool ContainsUniform(string name)
        {
            return mUniforms.Any(u => u.UniformName == name);
        }

        public void AddPushConstant(string name, string content)
        {
            if (mPushConstants.ContainsKey(name))
            {
                return;
            }

            mPushConstants.Add(name, content);
        }

        protected override void internalCheckExtensionNeed(NeedExtensionAttribute attribute, MemberInfo member)
        {
            mCompiler.CurrentComponentData?.AddExtension(attribute.ExtentionName);
        }
    }
}
