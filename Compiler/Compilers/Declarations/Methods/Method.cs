// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace General.Shaders
{
    internal class Method : Declaration, IVariableCollection, ISyntaxHost, IMethodProvider, IReferenceHost
    {
        public virtual bool IsMain => false;

        protected MethodDeclarationSyntax mSyntax;
        public MethodDeclarationSyntax Syntax => mSyntax;
        SyntaxNode ISyntaxHost.SyntaxNode => mSyntax;

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

        Method? IMethodProvider.GetMethod(string name)
        {
            return (this.DeclaringClass as IMethodProvider).GetMethod(name);
        }

        void IReferenceHost.AppendReference(Declaration reference)
        {
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
