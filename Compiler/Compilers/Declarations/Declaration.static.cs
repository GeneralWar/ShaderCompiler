// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace General.Shaders
{
    internal abstract partial class Declaration
    {
        static protected string AnalyzeStatement(Compiler compiler, StatementSyntax syntax)
        {
            ExpressionStatementSyntax? expressionStatementSyntax = syntax as ExpressionStatementSyntax;
            if (expressionStatementSyntax is not null)
            {
                return AnalyzeExpressionStatementSyntax(compiler, expressionStatementSyntax);
            }

            LocalDeclarationStatementSyntax? localDeclarationStatementSyntax = syntax as LocalDeclarationStatementSyntax;
            if (localDeclarationStatementSyntax is not null)
            {
                return AnalyzeLocalDeclarationStatementSyntax(compiler, localDeclarationStatementSyntax);
            }

            throw new NotImplementedException();
        }

        static protected string AnalyzeExpressionStatementSyntax(Compiler compiler, ExpressionStatementSyntax syntax)
        {
            AssignmentExpressionSyntax? assignmentExpressionSyntax = syntax.Expression as AssignmentExpressionSyntax;
            if (assignmentExpressionSyntax is not null)
            {
                return AnalyzeAssignmentExpressionSyntax(compiler, assignmentExpressionSyntax) + syntax.SemicolonToken.ValueText;
            }

            throw new NotImplementedException();
        }

        static protected string AnalyzeAssignmentExpressionSyntax(Compiler compiler, AssignmentExpressionSyntax syntax)
        {
            string leftContent = AnalyzeExpressionSyntax(compiler, syntax.Left);
            string rightContent = AnalyzeExpressionSyntax(compiler, syntax.Right);
            Trace.Assert(!string.IsNullOrWhiteSpace(leftContent) && !string.IsNullOrWhiteSpace(rightContent));
            return $"{leftContent} {syntax.OperatorToken.Text} {rightContent}";
        }

        static protected string AnalyzeExpressionSyntax(Compiler compiler, ExpressionSyntax syntax)
        {
            LiteralExpressionSyntax? literalExpressionSyntax = syntax as LiteralExpressionSyntax;
            if (literalExpressionSyntax is not null)
            {
                return AnalyzeLiteralExpressionSyntax(compiler, literalExpressionSyntax);
            }

            MemberAccessExpressionSyntax? memberAccessExpressionSyntax = syntax as MemberAccessExpressionSyntax;
            if (memberAccessExpressionSyntax is not null)
            {
                return AnalyzeMemberAccessExpressionSyntax(compiler, memberAccessExpressionSyntax);
            }

            BinaryExpressionSyntax? binaryExpressionSyntax = syntax as BinaryExpressionSyntax;
            if (binaryExpressionSyntax is not null)
            {
                return AnalyzeBinaryExpressionSyntax(compiler, binaryExpressionSyntax);
            }

            ObjectCreationExpressionSyntax? objectCreationExpressionSyntax = syntax as ObjectCreationExpressionSyntax;
            if (objectCreationExpressionSyntax is not null)
            {
                return AnalyzeObjectCreationExpressionSyntax(compiler, objectCreationExpressionSyntax);
            }

            InvocationExpressionSyntax? invocationExpressionSyntax = syntax as InvocationExpressionSyntax;
            if (invocationExpressionSyntax is not null)
            {
                return AnalyzeInvocationExpressionSyntax(compiler, invocationExpressionSyntax);
            }

            ElementAccessExpressionSyntax? elementAccessExpressionSyntax = syntax as ElementAccessExpressionSyntax;
            if (elementAccessExpressionSyntax is not null)
            {
                return AnalyzeElementAccessExpressionSyntax(compiler, elementAccessExpressionSyntax);
            }

            Debugger.Break();
            throw new NotImplementedException();
        }

        static protected string AnalyzeMemberAccessExpressionSyntax(Compiler compiler, MemberAccessExpressionSyntax syntax)
        {
            if (syntax.Expression is IdentifierNameSyntax)
            {
                return AnalyzeMemberAccessExpressionSyntax(compiler, syntax.Expression.GetName(), syntax.Name.GetName());
            }

            string prefix = "";
            if (syntax.Expression is MemberAccessExpressionSyntax)
            {
                prefix = AnalyzeMemberAccessExpressionSyntax(compiler, syntax.Expression as MemberAccessExpressionSyntax ?? throw new InvalidDataException()) + syntax.OperatorToken.ValueText;
            }

            MemberAccessExpressionSyntax? memberAccessExpressionSyntax = syntax.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpressionSyntax is not null)
            {
                Type type = GetMemberType(compiler, memberAccessExpressionSyntax);
                string memberName = syntax.Name.GetName();
                if (type.GetCustomAttribute<MemberCollectorAttribute>() is not null)
                {
                    return prefix + memberName;
                }
                return prefix + compiler.AnalyzeMemberName(type, memberName);
            }

            {
                string targetName = syntax.Expression.GetName();
                string memberName = syntax.Name.GetName();
                Trace.Assert("." == syntax.OperatorToken.ValueText);
                //int separatorIndex = name.IndexOf('.');
                //if (separatorIndex > -1)
                //{
                //    return this.GetVariable(name.Substring(0, separatorIndex));
                //}

                throw new NotImplementedException();
            }
        }

        static protected Type GetMemberType(Compiler compiler, MemberAccessExpressionSyntax syntax)
        {
            if (syntax.Expression is IdentifierNameSyntax)
            {
                Variable? target = compiler.GetVariable(syntax.Expression.GetName());
                if (target is null)
                {
                    throw new InvalidDataException();
                }

                return target.Type.GetMemberType(syntax.Name.GetName());
            }

            throw new NotImplementedException();
        }

        static protected string AnalyzeLiteralExpressionSyntax(Compiler compiler, LiteralExpressionSyntax literalExpressionSyntax)
        {
            switch((SyntaxKind)literalExpressionSyntax.RawKind)
            {
                case SyntaxKind.NumericLiteralExpression:
                    return literalExpressionSyntax.Token.ValueText;
            }

            Debugger.Break();
            throw new InvalidDataException();
        }

        static protected string AnalyzeMemberAccessExpressionSyntax(Compiler compiler, string targetName, string memberName)
        {
            Variable? target = compiler.GetVariable(targetName);
            if (target is not null)
            {
                return target.AnalyzeMemberAccess(compiler, memberName);
            }

            Type? type = compiler.GetType(targetName);
            if (type is not null)
            {
                return AnalyzeMemberAccessExpressionSyntax(compiler, type, memberName);
            }

            throw new InvalidDataException();
        }

        static protected string AnalyzeMemberAccessExpressionSyntax(Compiler compiler, Type type, string memberName)
        {
            MemberInfo[] members = type.GetMember(memberName);
            Trace.Assert(1 == members.Length);

            MemberInfo memberInfo = members[0];
            MethodInfo? functionInfo = memberInfo as MethodInfo;
            if (functionInfo is not null)
            {
                return functionInfo.GetFunctionName(compiler.Language);
            }

            throw new NotImplementedException();
        }

        static protected string AnalyzeElementAccessExpressionSyntax(Compiler compiler, ElementAccessExpressionSyntax syntax)
        {
            string variableName = syntax.Expression.GetName();
            List<string> arguments = AnalyzeBracketedArgumentListSyntax(compiler, syntax.ArgumentList);
            Trace.Assert(1 == arguments.Count);
            return compiler.AnalyzeElementAccess(variableName, arguments[0]);
        }

        static protected List<string> AnalyzeBracketedArgumentListSyntax(Compiler compiler, BracketedArgumentListSyntax syntax)
        {
            return AnalyzeArgumentList(compiler, syntax.Arguments);
        }

        static protected string AnalyzeBinaryExpressionSyntax(Compiler compiler, BinaryExpressionSyntax syntax)
        {
            string leftContent = AnalyzeExpressionSyntax(compiler, syntax.Left);
            string rightContent = AnalyzeExpressionSyntax(compiler, syntax.Right);
            Trace.Assert(!string.IsNullOrWhiteSpace(leftContent) && !string.IsNullOrWhiteSpace(rightContent));
            return $"{leftContent} {syntax.OperatorToken.Text} {rightContent}";
        }

        static protected string AnalyzeObjectCreationExpressionSyntax(Compiler compiler, ObjectCreationExpressionSyntax syntax)
        {
            Trace.Assert("new" == syntax.NewKeyword.Text);

            Type? type = syntax.GetTypeFromRoot(syntax.Type.GetName());
            if (type is null)
            {
                throw new InvalidDataException();
            }

            string typeName = type.GetShaderTypeName(compiler.Language);
            List<string> arguments = new List<string>();
            if (syntax.ArgumentList is not null)
            {
                ArgumentList argumentList = new ArgumentList(syntax.ArgumentList);
                argumentList.Analyze(compiler);
                arguments.AddRange(argumentList.Arguments);
            }
            return $"{typeName}({string.Join(", ", arguments)})";
        }

        static protected string AnalyzeInvocationExpressionSyntax(Compiler compiler, InvocationExpressionSyntax syntax)
        {
            string method = AnalyzeExpressionSyntax(compiler, syntax.Expression);
            List<string> arguments = AnalyzeArgumentList(compiler, syntax.ArgumentList);
            return $"{method}({string.Join(", ", arguments)})";
        }

        static protected List<string> AnalyzeArgumentList(Compiler compiler, ArgumentListSyntax syntax)
        {
            return AnalyzeArgumentList(compiler, syntax.Arguments);
        }

        static protected List<string> AnalyzeArgumentList(Compiler compiler, SeparatedSyntaxList<ArgumentSyntax> syntaxList)
        {
            List<string> arguments = new List<string>();
            foreach (ArgumentSyntax syntax in syntaxList)
            {
                arguments.Add(AnalyzeArgumentSyntax(compiler, syntax));
            }
            return arguments;
        }

        static protected string AnalyzeArgumentSyntax(Compiler compiler, ArgumentSyntax syntax)
        {
            LiteralExpressionSyntax? literalExpressionSyntax = syntax.Expression as LiteralExpressionSyntax;
            if (literalExpressionSyntax is not null)
            {
                return literalExpressionSyntax.Token.Text;
            }

            return AnalyzeExpressionSyntax(compiler, syntax.Expression);
        }

        static protected string AnalyzeLocalDeclarationStatementSyntax(Compiler compiler, LocalDeclarationStatementSyntax syntax)
        {
            if (!string.IsNullOrWhiteSpace(syntax.UsingKeyword.ValueText))
            {
                throw new NotImplementedException();
            }

            VariableDeclarationSyntax? variableDeclarationSyntax = syntax.Declaration as VariableDeclarationSyntax;
            if (variableDeclarationSyntax is not null)
            {
                return AnalyzeVariableDeclarationSyntax(compiler, variableDeclarationSyntax);
            }

            throw new NotImplementedException();
        }

        static private string AnalyzeVariableDeclarationSyntax(Compiler compiler, VariableDeclarationSyntax syntax)
        {
            string typeName = syntax.Type.GetName();
            Type? type = compiler.GetType(typeName);
            if (type is null)
            {
                throw new InvalidDataException();
            }

            string shaderTypeName = type.GetShaderTypeName(compiler.Language);
            List<string> variables = AnalyzeVariableDeclaratorSyntaxList(compiler, syntax.Type, syntax.Variables);
            return $"{shaderTypeName} {string.Join(", ", variables)};";
        }

        static private List<string> AnalyzeVariableDeclaratorSyntaxList(Compiler compiler, TypeSyntax typeSyntax, SeparatedSyntaxList<VariableDeclaratorSyntax> syntaxList)
        {
            List<string> variables = new List<string>();
            foreach (VariableDeclaratorSyntax syntax in syntaxList)
            {
                string variableName = syntax.Identifier.ValueText;
                if (syntax.Initializer is null)
                {
                    throw new NotImplementedException();
                }

                string initializer = AnalyzeEqualsValueClauseSyntax(compiler, syntax.Initializer);
                variables.Add($"{variableName} {initializer}");
                compiler.PushLocalVariable(new Variable(typeSyntax, syntax));
            }
            return variables;
        }

        static private string AnalyzeEqualsValueClauseSyntax(Compiler compiler, EqualsValueClauseSyntax syntax)
        {
            string equalsToken = syntax.EqualsToken.ValueText;
            string value = AnalyzeExpressionSyntax(compiler, syntax.Value);
            return $"{equalsToken} {value}";
        }
    }
}
