// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace General.Shaders
{
    internal class Method : Declaration, IVariableCollection, ISyntaxHost, IMethodProvider, IReferenceHost
    {
        public virtual bool IsMain => false;

        protected MethodDeclarationSyntax mSyntax;
        public MethodDeclarationSyntax Syntax => mSyntax;
        SyntaxNode ISyntaxHost.SyntaxNode => mSyntax;
        public string MethodName => $"{mSyntax.Identifier.ValueText}_{string.Join("_", mSyntax.ParameterList.Parameters.Select(p => p.Type?.GetName() ?? p.Identifier.ValueText))}";

        protected ParameterList mParameterList;
        public ParameterList ParameterList => mParameterList;

        protected Dictionary<Language, string> mContents = new Dictionary<Language, string>();
        Dictionary<string, Variable> mLocalVariables = new Dictionary<string, Variable>();

        public TypeSyntax ReturnType { get; private set; }

        public Class DeclaringClass => this.Parent as Class ?? throw new InvalidDataException("Method must be declared in a class");

        private HashSet<Declaration> mReferences = new HashSet<Declaration>();
        HashSet<Declaration> IReferenceHost.References => mReferences;
        public HashSet<Declaration> References => mReferences;

        public Method(MethodDeclarationSyntax syntax) : base(syntax.Identifier.ValueText)
        {
            mSyntax = syntax;
            this.ReturnType = syntax.ReturnType;
            mParameterList = new ParameterList(syntax.ParameterList);
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

        void IReferenceHost.AppendReference(Declaration reference)
        {
            if (this == reference)
            {
                return;
            }

            mReferences.Add(reference);
        }

        internal string CompileMethodBody(Compiler compiler)
        {
            string? content;
            if (!mContents.TryGetValue(compiler.Language, out content))
            {
                BlockSyntax? body = mSyntax.Body;
                if (body is null)
                {
                    throw new InvalidDataException();
                }

                compiler.PushScope(this);
                compiler.IncreaseTabCount();

                StringBuilder builder = new StringBuilder();
                foreach (StatementSyntax statementSyntax in body.Statements)
                {
                    builder.AppendLine(compiler.TabCount, CompileStatement(compiler, statementSyntax).Trim());
                }

                compiler.DecreaseTabCount();
                compiler.PopScope(this);

                mContents.Add(compiler.Language, content = builder.ToString().TrimEnd());
            }
            return content;
        }
    }
}
