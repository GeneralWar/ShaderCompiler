// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace General.Shaders
{
    internal class Method : Member, IVariableCollection, IMethodProvider, IReferenceHost, IUniformHost
    {
        public virtual bool IsMain => false;

        protected MethodDeclarationSyntax mSyntax;
        public string MethodName => $"{mSyntax.Identifier.ValueText}_{string.Join("_", mSyntax.ParameterList.Parameters.Select(p => p.Type?.GetName() ?? p.Identifier.ValueText))}";

        protected ParameterList mParameterList;
        public ParameterList ParameterList => mParameterList;

        protected Dictionary<Language, string> mContents = new Dictionary<Language, string>();
        Dictionary<string, Variable> mLocalVariables = new Dictionary<string, Variable>();

        public TypeSyntax ReturnType { get; private set; }

        public Class DeclaringClass => this.Parent as Class ?? throw new InvalidDataException("Method must be declared in a class");

        private HashSet<Declaration> mReferences = new HashSet<Declaration>();
        IEnumerable<Declaration> IReferenceHost.References => mReferences;
        public HashSet<Declaration> References => mReferences;

        private HashSet<Declaration> mUniforms = new HashSet<Declaration>();
        public IEnumerable<Declaration> Uniforms => throw new NotImplementedException();

        public Method(DeclarationContainer root, DeclarationContainer declaringContainer, MethodDeclarationSyntax syntax) : base(root, declaringContainer, syntax)
        {
            mSyntax = syntax;
            this.ReturnType = syntax.ReturnType;
            mParameterList = new ParameterList(root, syntax.ParameterList);
        }

        public bool Match(MethodInfo info)
        {
            this.Analyze();

            ParameterInfo[] parameterInfos = info.GetParameters();
            if (mParameterList.ParameterCount != parameterInfos.Length)
            {
                return false;
            }

            for (int i = 0; i < parameterInfos.Length; ++i)
            {
                if (ParameterList.Parameters.ElementAt(i).Type != parameterInfos[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        protected override void internalAnalyze()
        {
            mParameterList.Analyze();
            foreach (Variable variable in mParameterList.Parameters)
            {
                mLocalVariables.TryAdd(variable.Name, variable);
            }
        }

        Variable? IVariableCollection.GetVariable(string name)
        {
            this.Analyze();

            Variable? variable;
            if (!mLocalVariables.TryGetValue(name, out variable))
            {
                variable = (mParameterList as IVariableCollection).GetVariable(name);
            }
            return variable;
        }

        void IVariableCollection.PushVariable(Variable variable)
        {
            mLocalVariables.Add(variable.Name, variable); // should never exist multiple local variables with same name
        }

        Method[] IMethodProvider.GetMethods(string name)
        {
            return this.DeclaringClass?.GetMethods(name) ?? new Method[0];
        }

        void IReferenceHost.AddReference(Declaration reference)
        {
            if (this == reference)
            {
                return;
            }

            mReferences.Add(reference);
        }

        public void AddUniform(Declaration uniform)
        {
            mUniforms.Add(uniform);
        }

        internal string CompileMethodBody(CompileContext context)
        {
            string? content;
            if (!mContents.TryGetValue(context.Language, out content))
            {
                BlockSyntax? body = mSyntax.Body;
                if (body is null)
                {
                    throw new InvalidDataException();
                }

                bool push = context.Compiler.PushScope(this);
                context.IncreaseTabCount();

                StringBuilder builder = new StringBuilder();
                foreach (StatementSyntax statementSyntax in body.Statements)
                {
                    builder.AppendLine(context.TabCount, CompileSyntax(context, statementSyntax).Trim());
                }

                context.DecreaseTabCount();
                if (push)
                {
                    context.Compiler.PopScope(this);
                }

                mContents.Add(context.Language, content = builder.ToString().TrimEnd());
            }
            return content;
        }
    }
}
