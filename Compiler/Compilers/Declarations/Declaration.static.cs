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
using System.Text;

namespace General.Shaders
{
    public abstract partial class Declaration
    {
        enum AnalyzeStatus
        {
            Initialized,
            Analyzing,
            Analyzed,
        }

        static protected string CompileStatement(Compiler compiler, StatementSyntax syntax)
        {
            ExpressionStatementSyntax? expressionStatementSyntax = syntax as ExpressionStatementSyntax;
            if (expressionStatementSyntax is not null)
            {
                return CompileExpressionStatementSyntax(compiler, expressionStatementSyntax);
            }

            LocalDeclarationStatementSyntax? localDeclarationStatementSyntax = syntax as LocalDeclarationStatementSyntax;
            if (localDeclarationStatementSyntax is not null)
            {
                return CompileLocalDeclarationStatementSyntax(compiler, localDeclarationStatementSyntax);
            }

            IfStatementSyntax? ifStatementSyntax = syntax as IfStatementSyntax;
            if (ifStatementSyntax is not null)
            {
                return CompileIfStatementSyntax(compiler, ifStatementSyntax);
            }

            ReturnStatementSyntax? returnStatementSyntax = syntax as ReturnStatementSyntax;
            if (returnStatementSyntax is not null)
            {
                return "return" + (returnStatementSyntax.Expression is null ? "" : " " + CompileExpressionSyntax(compiler, returnStatementSyntax.Expression)) + ";";
            }

            BlockSyntax? blockSyntax = syntax as BlockSyntax;
            if (blockSyntax is not null)
            {
                return CompileBlockSyntax(compiler, blockSyntax);
            }

            throw new NotImplementedException();
        }

        static protected string CompileExpressionStatementSyntax(Compiler compiler, ExpressionStatementSyntax syntax)
        {
            AssignmentExpressionSyntax? assignmentExpressionSyntax = syntax.Expression as AssignmentExpressionSyntax;
            if (assignmentExpressionSyntax is not null)
            {
                return CompileAssignmentExpressionSyntax(compiler, assignmentExpressionSyntax) + syntax.SemicolonToken.ValueText;
            }

            throw new NotImplementedException();
        }

        static protected string CompileAssignmentExpressionSyntax(Compiler compiler, AssignmentExpressionSyntax syntax)
        {
            string leftContent = CompileExpressionSyntax(compiler, syntax.Left);
            string rightContent = CompileExpressionSyntax(compiler, syntax.Right);
            Trace.Assert(!string.IsNullOrWhiteSpace(leftContent) && !string.IsNullOrWhiteSpace(rightContent));
            return $"{leftContent} {syntax.OperatorToken.Text} {rightContent}";
        }

        static protected string CompileExpressionSyntax(Compiler compiler, ExpressionSyntax syntax)
        {
            IdentifierNameSyntax? identifierNameSyntax = syntax as IdentifierNameSyntax;
            if (identifierNameSyntax is not null)
            {
                string name = identifierNameSyntax.GetName();
                Variable? variable = compiler.GetVariable(name);
                if (variable is not null)
                {
                    return compiler.AnalyzeVariableName(variable);
                }

                Method? method = compiler.GetMethod(name);
                if (method is not null)
                {
                    compiler.CurrentReferenceHost.AppendReference(method);
                    return method.Name;
                }

                throw new InvalidDataException();
            }

            LiteralExpressionSyntax? literalExpressionSyntax = syntax as LiteralExpressionSyntax;
            if (literalExpressionSyntax is not null)
            {
                return CompileLiteralExpressionSyntax(compiler, literalExpressionSyntax);
            }

            MemberAccessExpressionSyntax? memberAccessExpressionSyntax = syntax as MemberAccessExpressionSyntax;
            if (memberAccessExpressionSyntax is not null)
            {
                return CompileMemberAccessExpressionSyntax(compiler, memberAccessExpressionSyntax);
            }

            BinaryExpressionSyntax? binaryExpressionSyntax = syntax as BinaryExpressionSyntax;
            if (binaryExpressionSyntax is not null)
            {
                return CompileBinaryExpressionSyntax(compiler, binaryExpressionSyntax);
            }

            ObjectCreationExpressionSyntax? objectCreationExpressionSyntax = syntax as ObjectCreationExpressionSyntax;
            if (objectCreationExpressionSyntax is not null)
            {
                return CompileObjectCreationExpressionSyntax(compiler, objectCreationExpressionSyntax);
            }

            InvocationExpressionSyntax? invocationExpressionSyntax = syntax as InvocationExpressionSyntax;
            if (invocationExpressionSyntax is not null)
            {
                return CompileInvocationExpressionSyntax(compiler, invocationExpressionSyntax);
            }

            ElementAccessExpressionSyntax? elementAccessExpressionSyntax = syntax as ElementAccessExpressionSyntax;
            if (elementAccessExpressionSyntax is not null)
            {
                return CompileElementAccessExpressionSyntax(compiler, elementAccessExpressionSyntax);
            }

            Debugger.Break();
            throw new NotImplementedException();
        }

        static protected string CompileMemberAccessExpressionSyntax(Compiler compiler, MemberAccessExpressionSyntax syntax)
        {
            string memberName = syntax.Name.GetName();
            if (syntax.Expression is IdentifierNameSyntax)
            {
                return CompileMemberAccessExpressionSyntax(compiler, syntax.Expression.GetName(), memberName);
            }

            string prefix = "";
            if (syntax.Expression is MemberAccessExpressionSyntax)
            {
                prefix = CompileMemberAccessExpressionSyntax(compiler, syntax.Expression as MemberAccessExpressionSyntax ?? throw new InvalidDataException()) + syntax.OperatorToken.ValueText;
            }

            MemberAccessExpressionSyntax? memberAccessExpressionSyntax = syntax.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpressionSyntax is not null)
            {
                Type type = GetMemberType(compiler, memberAccessExpressionSyntax);
                if (type.GetCustomAttribute<MemberCollectorAttribute>() is not null)
                {
                    return prefix + memberName;
                }
                return prefix + compiler.AnalyzeMemberName(type, memberName);
            }

            InvocationExpressionSyntax? invocationExpressionSyntax = syntax.Expression as InvocationExpressionSyntax;
            if (invocationExpressionSyntax is not null)
            {
                string targetName = CompileInvocationExpressionSyntax(compiler, invocationExpressionSyntax);
                Trace.Assert("." == syntax.OperatorToken.ValueText);
                return $"{targetName}{syntax.OperatorToken.ValueText}{memberName}";
            }

            ThisExpressionSyntax? thisExpressionSyntax = syntax.Expression as ThisExpressionSyntax;
            if (thisExpressionSyntax is not null)
            {
                return memberName;
            }


            ElementAccessExpressionSyntax? elementAccessExpressionSyntax = syntax.Expression as ElementAccessExpressionSyntax;
            if (elementAccessExpressionSyntax is not null)
            {
                return $"{elementAccessExpressionSyntax.Expression.GetName()}[{string.Join(", ", CompileArgumentList(compiler, elementAccessExpressionSyntax.ArgumentList))}]" + syntax.OperatorToken.ValueText + memberName;
            }

            {
                string targetName = syntax.Expression.GetName();
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
            ThisExpressionSyntax? thisExpressionSyntax = syntax.Expression as ThisExpressionSyntax;
            if (thisExpressionSyntax is not null)
            {
                Class? classDeclaration = compiler.GetCurrent<Class>();
                if (classDeclaration is null)
                {
                    throw new InvalidDataException();
                }

                return classDeclaration.Type.GetMemberType(syntax.Name.GetName()) ?? throw new InvalidDataException();
            }
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

        static protected string CompileLiteralExpressionSyntax(Compiler compiler, LiteralExpressionSyntax literalExpressionSyntax)
        {
            switch ((SyntaxKind)literalExpressionSyntax.RawKind)
            {
                case SyntaxKind.NumericLiteralExpression:
                    return literalExpressionSyntax.Token.ValueText;
            }

            Debugger.Break();
            throw new InvalidDataException();
        }

        static protected string CompileMemberAccessExpressionSyntax(Compiler compiler, string targetName, string memberName)
        {
            Variable? target = compiler.GetVariable(targetName);
            if (target is not null)
            {
                return target.AnalyzeMemberAccess(compiler, memberName);
            }

            Type? type = compiler.GetType(targetName);
            if (type is not null)
            {
                return CompileMemberAccessExpressionSyntax(compiler, type, memberName);
            }

            throw new InvalidDataException();
        }

        static protected string CompileMemberAccessExpressionSyntax(Compiler compiler, Type type, string memberName)
        {
            MemberInfo[] members = type.GetMember(memberName);
            Trace.Assert(1 == members.Length);

            MemberInfo memberInfo = members[0];
            MethodInfo? methodInfo = memberInfo as MethodInfo;
            if (methodInfo is not null)
            {
                string? methodName = methodInfo.GetFunctionName(compiler.Language);
                return string.IsNullOrWhiteSpace(methodName) ? CompileMethodInvocation(compiler, methodInfo) : methodName;
            }

            throw new NotImplementedException();
        }

        static protected string CompileMethodInvocation(Compiler compiler, MethodInfo methodInfo)
        {
            string? declaringTypeName = methodInfo.DeclaringType?.FullName;
            if (string.IsNullOrWhiteSpace(declaringTypeName))
            {
                throw new InvalidDataException();
            }

            //Type? declaringType = compiler.GetType(declaringTypeName);
            //if (declaringType is null)
            //{
            //}

            Class? typeClass = compiler.Global?.GetDeclaration(declaringTypeName) as Class;
            if (typeClass is null)
            {
                throw new InvalidDataException();
            }

            Declaration? declaration = typeClass.GetDeclaration(methodInfo.Name);
            if (declaration is null || declaration is not Method)
            {
                throw new InvalidDataException();
            }

            compiler.CurrentReferenceHost.AppendReference(declaration);
            return declaration.Name;
        }

        static protected string CompileElementAccessExpressionSyntax(Compiler compiler, ElementAccessExpressionSyntax syntax)
        {
            string variableName = syntax.Expression.GetName();
            List<string> arguments = CompileBracketedArgumentListSyntax(compiler, syntax.ArgumentList);
            Trace.Assert(1 == arguments.Count);
            return compiler.AnalyzeElementAccess(variableName, arguments[0]);
        }

        static protected List<string> CompileBracketedArgumentListSyntax(Compiler compiler, BracketedArgumentListSyntax syntax)
        {
            return CompileArgumentList(compiler, syntax.Arguments);
        }

        static protected string CompileBinaryExpressionSyntax(Compiler compiler, BinaryExpressionSyntax syntax)
        {
            string leftContent = CompileExpressionSyntax(compiler, syntax.Left);
            string rightContent = CompileExpressionSyntax(compiler, syntax.Right);
            Trace.Assert(!string.IsNullOrWhiteSpace(leftContent) && !string.IsNullOrWhiteSpace(rightContent));
            return $"{leftContent} {syntax.OperatorToken.Text} {rightContent}";
        }

        static protected string CompileObjectCreationExpressionSyntax(Compiler compiler, ObjectCreationExpressionSyntax syntax)
        {
            Trace.Assert("new" == syntax.NewKeyword.Text);

            Type? type = syntax.GetTypeFromRoot(syntax.Type.GetName());
            if (type is null)
            {
                throw new InvalidDataException();
            }

            string typeName = type.GetShaderTypeName(compiler.Language);
            if (syntax.ArgumentList is not null)
            {
                ArgumentList argumentList = new ArgumentList(syntax.ArgumentList);
                List<string> arguments = argumentList.Compile(compiler);
                if (0 == arguments.Count)
                {
                    TypeNameAttribute? typeNameAttribute = type.GetCustomAttribute<TypeNameAttribute>();
                    if (!string.IsNullOrWhiteSpace(typeNameAttribute?.DefaultConstructor))
                    {
                        return typeNameAttribute.DefaultConstructor;
                    }
                }
                return $"{typeName}({string.Join(", ", arguments)})";
            }

            throw new NotImplementedException();
        }

        static protected string CompileInvocationExpressionSyntax(Compiler compiler, InvocationExpressionSyntax syntax)
        {
            string method = CompileExpressionSyntax(compiler, syntax.Expression);
            List<string> arguments = CompileArgumentList(compiler, syntax.ArgumentList);
            return $"{method}({string.Join(", ", arguments)})";
        }

        static protected List<string> CompileArgumentList(Compiler compiler, BaseArgumentListSyntax syntax)
        {
            return CompileArgumentList(compiler, syntax.Arguments);
        }

        static protected List<string> CompileArgumentList(Compiler compiler, SeparatedSyntaxList<ArgumentSyntax> syntaxList)
        {
            List<string> arguments = new List<string>();
            foreach (ArgumentSyntax syntax in syntaxList)
            {
                IdentifierNameSyntax? identifierNameSyntax = syntax.Expression as IdentifierNameSyntax;
                if (identifierNameSyntax is not null)
                {
                    Variable? variable = compiler.GetVariable(identifierNameSyntax.GetName());
                    if (variable is not null)
                    {
                        if (typeof(InputFragment) == variable.Type)
                        {
                            continue;
                        }
                    }
                }

                arguments.Add(CompileArgumentSyntax(compiler, syntax));
            }
            return arguments;
        }

        static protected string CompileArgumentSyntax(Compiler compiler, ArgumentSyntax syntax)
        {
            LiteralExpressionSyntax? literalExpressionSyntax = syntax.Expression as LiteralExpressionSyntax;
            if (literalExpressionSyntax is not null)
            {
                return literalExpressionSyntax.Token.Text;
            }

            return CompileExpressionSyntax(compiler, syntax.Expression);
        }

        static protected string CompileLocalDeclarationStatementSyntax(Compiler compiler, LocalDeclarationStatementSyntax syntax)
        {
            if (!string.IsNullOrWhiteSpace(syntax.UsingKeyword.ValueText))
            {
                throw new NotImplementedException();
            }

            VariableDeclarationSyntax? variableDeclarationSyntax = syntax.Declaration as VariableDeclarationSyntax;
            if (variableDeclarationSyntax is not null)
            {
                return CompileVariableDeclarationSyntax(compiler, variableDeclarationSyntax);
            }

            throw new NotImplementedException();
        }

        static protected string CompileIfStatementSyntax(Compiler compiler, IfStatementSyntax syntax)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(compiler.TabCount, $"if ({CompileExpressionSyntax(compiler, syntax.Condition)})");
            builder.AppendLine(compiler.TabCount, CompileStatement(compiler, syntax.Statement).Trim());
            return builder.ToString();
        }

        static protected string CompileBlockSyntax(Compiler compiler, BlockSyntax syntax)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(compiler.TabCount, syntax.OpenBraceToken.ValueText);
            compiler.IncreaseTabCount();
            foreach (StatementSyntax statement in syntax.Statements)
            {
                builder.AppendLine(compiler.TabCount, CompileStatement(compiler, statement).Trim());
            }
            compiler.DecreaseTabCount();
            builder.Append(compiler.TabCount, syntax.CloseBraceToken.ValueText);
            return builder.ToString();
        }

        static private string CompileVariableDeclarationSyntax(Compiler compiler, VariableDeclarationSyntax syntax)
        {
            string typeName = syntax.Type.GetName();
            Type? type = compiler.GetType(typeName);
            if (type is null)
            {
                throw new InvalidDataException();
            }

            string shaderTypeName = type.GetShaderTypeName(compiler.Language);
            List<string> variables = CompileVariableDeclaratorSyntaxList(compiler, syntax.Type, syntax.Variables);
            return $"{shaderTypeName} {string.Join(", ", variables)};";
        }

        static private List<string> CompileVariableDeclaratorSyntaxList(Compiler compiler, TypeSyntax typeSyntax, SeparatedSyntaxList<VariableDeclaratorSyntax> syntaxList)
        {
            List<string> variables = new List<string>();
            foreach (VariableDeclaratorSyntax syntax in syntaxList)
            {
                string variableName = syntax.Identifier.ValueText;
                if (syntax.Initializer is null)
                {
                    throw new NotImplementedException();
                }

                string initializer = CompileEqualsValueClauseSyntax(compiler, syntax.Initializer);
                variables.Add($"{variableName} {initializer}");
                compiler.CurrentVariableCollection.PushVariable(new Variable(typeSyntax, syntax));
            }
            return variables;
        }

        static private string CompileEqualsValueClauseSyntax(Compiler compiler, EqualsValueClauseSyntax syntax)
        {
            string equalsToken = syntax.EqualsToken.ValueText;
            string value = CompileExpressionSyntax(compiler, syntax.Value);
            return $"{equalsToken} {value}";
        }
    }
}
