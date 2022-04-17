// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Text;

namespace General.Shaders
{
    internal abstract class Method : Declaration
    {
        protected MethodDeclarationSyntax mSyntax;

        protected ParameterList mParameterList;
        public ParameterList ParameterList => mParameterList;

        protected string mContent = "";
        public string Content => mContent;

        public Method(MethodDeclarationSyntax syntax) : base(syntax.Identifier.ValueText)
        {
            mSyntax = syntax;
            mParameterList = new ParameterList(syntax.ParameterList);
        }

        protected override void internalAnalyze(Compiler compiler)
        {
            mParameterList.Analyze(compiler);
            compiler.PushSyntax(mSyntax);
            compiler.PushVariables(mParameterList.Parametes);
            mContent = this.internalAnalyzeMethod(compiler, mSyntax);
            compiler.PopSyntax(mSyntax);
        }

        protected virtual string internalAnalyzeMethod(Compiler compiler, MethodDeclarationSyntax syntax)
        {
            BlockSyntax? body = syntax.Body;
            if (body is null)
            {
                throw new InvalidDataException();
            }

            StringBuilder builder = new StringBuilder();
            foreach (StatementSyntax statementSyntax in body.Statements)
            {
                builder.Append("\t");
                builder.AppendLine(AnalyzeStatement(compiler, statementSyntax));
            }

            return builder.ToString();
        }
    }
}
