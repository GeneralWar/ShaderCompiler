// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;

namespace General.Shaders
{
    internal abstract class CompileContext
    {
        public Compiler Compiler { get; private set; }
        public Language Language => this.Compiler.Language;

        public Declaration Root { get; private set; }

        public string? InputDeclaration { get; private set; } = null;
        public string? OutputDeclaration { get; private set; } = null;

        public string? VertexShader { get; private set; } = null;
        public string? FragmentShader { get; private set; } = null;

        private Dictionary<string, string> mStructures = new Dictionary<string, string>();
        public IEnumerable<string> Structures => mStructures.Values;

        private Dictionary<string, string> mReferences = new Dictionary<string, string>();
        public IEnumerable<string> References => mReferences.Values;

        public int TabCount { get; private set; }

        public void IncreaseTabCount() => ++this.TabCount;

        public void DecreaseTabCount() => --this.TabCount;

        public CompileContext(Compiler compiler, Declaration root)
        {
            this.Compiler = compiler;
            this.Root = root;
        }

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

        public void AddStructure(string name, string content)
        {
            if (this.ContainsStruct(name))
            {
                return;
            }

            mStructures.Add(name, content);
        }

        public bool ContainsStruct(string name)
        {
            return mStructures.ContainsKey(name);
        }

        public void AddReference(string name, Declaration declaration, string content)
        {
            if (mReferences.ContainsKey(name))
            {
                return;
            }

            mReferences.Add(name, content);
            (this.Root as IReferenceHost)?.AddReference(declaration);
        }

        public void AddUniform(UniformUsageAttribute attribute, Declaration declaration) => this.internalAddUniform(attribute, declaration);

        protected abstract void internalAddUniform(UniformUsageAttribute attribute, Declaration declaration);

        public void CheckMember(MemberInfo memberInfo)
        {
            Type declaringType = memberInfo.DeclaringType ?? throw new InvalidOperationException();
            UniformUsageAttribute? usageAttribute = memberInfo.GetCustomAttribute<UniformUsageAttribute>();
            if (usageAttribute is not null)
            {
                Declaration? declaration = this.Compiler.GetDeclaration($"{declaringType.FullName ?? declaringType.Name}.{memberInfo.Name}");
                if (declaration is not null)
                {
                    this.AddUniform(usageAttribute, declaration);
                }
            }

            NeedExtensionAttribute? needAttribute = memberInfo.GetCustomAttributes<NeedExtensionAttribute>().FirstOrDefault(n => n.Language == this.Language);
            if (needAttribute is not null)
            {
                this.internalCheckExtensionNeed(needAttribute, memberInfo);
            }
        }

        public void CheckMember(Member member)
        {
            this.CheckMember(member.MemberInfo);
        }

        protected abstract void internalCheckExtensionNeed(NeedExtensionAttribute attribute, MemberInfo member);
    }
}
