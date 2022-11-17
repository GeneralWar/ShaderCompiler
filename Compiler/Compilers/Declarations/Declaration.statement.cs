// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Text;

namespace General.Shaders
{
    public abstract partial class Declaration
    {
        [StatementCompiler(typeof(ExpressionStatementSyntax))]
        static internal string CompileExpressionStatementSyntax(CompileContext context, ExpressionStatementSyntax syntax)
        {
            return CompileSyntax(context, syntax.Expression) + syntax.SemicolonToken.ValueText;
        }

        [StatementCompiler(typeof(LocalDeclarationStatementSyntax))]
        static internal string CompileLocalDeclarationStatementSyntax(CompileContext context, LocalDeclarationStatementSyntax syntax)
        {
            if (!string.IsNullOrWhiteSpace(syntax.UsingKeyword.ValueText))
            {
                throw new NotImplementedException();
            }

            VariableDeclarationSyntax? variableDeclarationSyntax = syntax.Declaration as VariableDeclarationSyntax;
            if (variableDeclarationSyntax is not null)
            {
                return CompileVariableDeclarationSyntax(context, variableDeclarationSyntax);
            }

            throw new NotImplementedException();
        }

        [StatementCompiler(typeof(IfStatementSyntax))]
        static internal string CompileIfStatementSyntax(CompileContext context, IfStatementSyntax syntax)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(context.TabCount, $"if ({CompileSyntax(context, syntax.Condition)})");
            builder.AppendLine(context.TabCount, CompileSyntax(context, syntax.Statement).Trim());
            return builder.ToString();
        }

        [StatementCompiler(typeof(BlockSyntax))]
        static internal string CompileBlockSyntax(CompileContext context, BlockSyntax syntax)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(context.TabCount, syntax.OpenBraceToken.ValueText);
            context.IncreaseTabCount();
            foreach (StatementSyntax statement in syntax.Statements)
            {
                builder.AppendLine(context.TabCount, CompileSyntax(context, statement).Trim());
            }
            context.DecreaseTabCount();
            builder.Append(context.TabCount, syntax.CloseBraceToken.ValueText);
            return builder.ToString();
        }

        [StatementCompiler(typeof(ReturnStatementSyntax))]
        static internal string CompileBlockSyntax(CompileContext context, ReturnStatementSyntax syntax)
        {
            return "return" + (syntax.Expression is null ? "" : " " + CompileSyntax(context, syntax.Expression)) + ";";
        }
    }
}
