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
using System.Linq;
using System.Reflection;

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

        static private Dictionary<Type, MethodInfo> sStatementCompilerMethods = new Dictionary<Type, MethodInfo>();
        static private Dictionary<Type, MethodInfo> sExpressionCompilerMethods = new Dictionary<Type, MethodInfo>();

        static Declaration()
        {
            MethodInfo[] methods = typeof(Declaration).GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Declaration.InitializeCompilers<StatementCompilerAttribute>(methods, sStatementCompilerMethods);
            Declaration.InitializeCompilers<ExpressionCompilerAttribute>(methods, sExpressionCompilerMethods);
        }

        static private void InitializeCompilers<CompilerAttributeType>(MethodInfo[] methods, Dictionary<Type, MethodInfo> collection) where CompilerAttributeType : CompilerAttribute
        {
            foreach (MethodInfo method in methods)
            {
                CompilerAttributeType? attribute = method.GetCustomAttribute<CompilerAttributeType>();
                if (attribute is null)
                {
                    continue;
                }

                collection.Add(attribute.Type, method);
            }
        }

        static internal string CompileSyntax(CompileContext context, SyntaxNode syntax)
        {
            StatementSyntax? statement = syntax as StatementSyntax;
            if (statement is not null)
            {
                return CompileSyntax<StatementSyntax>(context, statement, sStatementCompilerMethods);
            }

            ExpressionSyntax? expression = syntax as ExpressionSyntax;
            if (expression is not null)
            {
                return CompileSyntax<ExpressionSyntax>(context, expression, sExpressionCompilerMethods);
            }

            throw new NotImplementedException($"No compiler for {syntax.GetType()}");
        }

        static internal string CompileSyntax<SyntaxType>(CompileContext context, SyntaxType syntax, Dictionary<Type, MethodInfo> collection) where SyntaxType : notnull, SyntaxNode
        {
            if (syntax is not SyntaxType)
            {
                throw new ArgumentException($"{syntax.GetType()} is not {typeof(SyntaxType)}", nameof(syntax));
            }

            MethodInfo? method = null;
            for (Type? type = syntax.GetType(); type?.IsSubclassOf(typeof(SyntaxType)) ?? false; type = type.BaseType)
            {
                if (collection.TryGetValue(type, out method))
                {
                    break;
                }
            }

            if (method is null)
            {
                throw new NotImplementedException($"No compiler for {syntax.GetType()}");
            }

            string content = method.Invoke(null, new object?[] { context, syntax }) as string ?? throw new InvalidOperationException();
            //if (syntax.HasLeadingTrivia)
            //{
            //    string trivias = CompileTriviaList(context, syntax.GetLeadingTrivia());
            //    if (!string.IsNullOrWhiteSpace(trivias))
            //    {
            //        content = trivias + Environment.NewLine + content;
            //    }
            //}
            if (syntax.HasTrailingTrivia)
            {
                string trivias = CompileTriviaList(context, syntax.GetTrailingTrivia());
                if (!string.IsNullOrWhiteSpace(trivias))
                {
                    content = content + " " + trivias;
                }
            }
            return content;
        }

        static internal List<string> CompileBracketedArgumentListSyntax(CompileContext context, BracketedArgumentListSyntax syntax)
        {
            return CompileArgumentList(context, syntax.Arguments);
        }

        static internal List<string> CompileArgumentList(CompileContext context, BaseArgumentListSyntax syntax)
        {
            return CompileArgumentList(context, syntax.Arguments);
        }

        static internal List<string> CompileArgumentList(CompileContext context, SeparatedSyntaxList<ArgumentSyntax> syntaxList)
        {
            List<string> arguments = new List<string>();
            foreach (ArgumentSyntax syntax in syntaxList)
            {
                IdentifierNameSyntax? identifierNameSyntax = syntax.Expression as IdentifierNameSyntax;
                if (identifierNameSyntax is not null)
                {
                    ITypeHost? variable = context.Compiler.GetVariable<ITypeHost>(identifierNameSyntax.GetName());
                    if (variable is not null)
                    {
                        if (typeof(InputFragment) == variable.Type)
                        {
                            continue;
                        }
                    }
                }

                arguments.Add(CompileArgumentSyntax(context, syntax));
            }
            return arguments;
        }

        static internal string CompileArgumentSyntax(CompileContext context, ArgumentSyntax syntax)
        {
            LiteralExpressionSyntax? literalExpressionSyntax = syntax.Expression as LiteralExpressionSyntax;
            if (literalExpressionSyntax is not null)
            {
                return CompileLiteralExpressionSyntax(context, literalExpressionSyntax);
            }

            return CompileSyntax(context, syntax.Expression);
        }

        static internal string CompileMemberAccessExpressionSyntax(CompileContext context, string targetName, string memberName)
        {
            Declaration? target = context.Compiler.GetVariable<Declaration>(targetName);
            if (target is not null)
            {
                ITypeHost typedTarget = target as ITypeHost ?? throw new InvalidOperationException();
                return AnalyzeMemberAccess(context, typedTarget.Type, target, memberName);
            }

            Type? type = context.Compiler.GetType(targetName);
            if (type is not null)
            {
                return CompileMemberAccessExpressionSyntax(context, type, memberName);
            }

            throw new InvalidDataException();
        }

        static internal string AnalyzeMemberAccess(CompileContext context, Type hostType, Declaration variable, string name)
        {
            if (typeof(InputVertex) == hostType || typeof(OutputVertex) == hostType || typeof(InputFragment) == hostType || typeof(OutputFragment) == hostType)
            {
                MemberInfo[] members = hostType.GetMember(name);
                Trace.Assert(1 == members.Length);

                return context.Compiler.AnalyzeMemberName(members[0]);
            }

            return $"{context.Compiler.AnalyzeVariableName(context, variable)}.{name}";
        }

        static internal string CompileMemberAccessExpressionSyntax(CompileContext context, Type type, string memberName)
        {
            MemberInfo[] members = type.GetMember(memberName);
            //Trace.Assert(1 == members.Length); maybe overload

            MemberInfo memberInfo = members[0];
            MethodInfo? methodInfo = memberInfo as MethodInfo;
            if (methodInfo is not null)
            {
                context.CheckMember(methodInfo);
                string? methodName = methodInfo.GetFunctionName(context.Language);
                return string.IsNullOrWhiteSpace(methodName) ? CompileMethodInvocation(context, methodInfo) : methodName;
            }

            FieldInfo? fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo?.IsStatic ?? false)
            {
                context.CheckMember(fieldInfo);
                return fieldInfo.Name;
            }

            PropertyInfo? propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo is not null && ((propertyInfo.GetGetMethod() ?? propertyInfo.GetSetMethod())?.IsStatic ?? false))
            {
                context.CheckMember(propertyInfo);
                return propertyInfo.Name;
            }

            throw new NotImplementedException();
        }

        static internal string CompileMethodInvocation(CompileContext context, MethodInfo methodInfo)
        {
            string? declaringTypeName = methodInfo.DeclaringType?.FullName;
            if (string.IsNullOrWhiteSpace(declaringTypeName))
            {
                throw new InvalidDataException();
            }

            Class? typeClass = context.Compiler.GetDeclaration<Class>(declaringTypeName);
            if (typeClass is null)
            {
                throw new InvalidDataException();
            }

            Method? method = typeClass.GetMethods(methodInfo.Name).FirstOrDefault(m => m.Match(methodInfo));
            if (method is null || method is not Method)
            {
                throw new InvalidDataException();
            }

            context.Compiler.AppendReference(method);
            return method.Name;
        }

        static internal string CompileVariableDeclarationSyntax(CompileContext context, VariableDeclarationSyntax syntax)
        {
            string typeName = syntax.Type.GetName();
            Type? type = context.Compiler.GetType(typeName);
            if (type is null)
            {
                throw new InvalidDataException();
            }

            string shaderTypeName = type.GetShaderTypeName(context.Language);
            List<string> variables = CompileVariableDeclaratorSyntaxList(context, syntax.Type, syntax.Variables);
            return $"{shaderTypeName} {string.Join(", ", variables)};";
        }

        static internal List<string> CompileVariableDeclaratorSyntaxList(CompileContext context, TypeSyntax typeSyntax, SeparatedSyntaxList<VariableDeclaratorSyntax> syntaxList)
        {
            List<string> variables = new List<string>();
            foreach (VariableDeclaratorSyntax syntax in syntaxList)
            {
                string variableName = syntax.Identifier.ValueText;
                if (syntax.Initializer is null)
                {
                    throw new NotImplementedException();
                }

                string initializer = CompileEqualsValueClauseSyntax(context, syntax.Initializer);
                variables.Add($"{variableName} {initializer}");
                context.Compiler.PushVariable(typeSyntax, syntax);
            }
            return variables;
        }

        static internal string CompileEqualsValueClauseSyntax(CompileContext context, EqualsValueClauseSyntax syntax)
        {
            string equalsToken = syntax.EqualsToken.ValueText;
            string value = CompileSyntax(context, syntax.Value);
            return $"{equalsToken} {value}";
        }

        static internal string CompileTriviaList(CompileContext context, SyntaxTriviaList trivias)
        {
            foreach (SyntaxTrivia trivia in trivias)
            {
                string content = trivia.ToString();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                    {
                        return content.Trim();
                    }
                    else
                    {
                        Debugger.Break();
                    }
                }
            }
            return "";
        }

        static internal Type AnalyzeType(PropertyDeclarationSyntax syntax)
        {
            if (syntax.Type is null)
            {
                throw new InvalidDataException();
            }

            return Declaration.AnalyzeType(syntax.Type);
        }

        static internal Type AnalyzeType(FieldDeclarationSyntax syntax)
        {
            throw new NotImplementedException();
        }

        static internal Type AnalyzeType(ParameterSyntax syntax)
        {
            if (syntax.Type is null)
            {
                throw new InvalidDataException();
            }

            return Declaration.AnalyzeType(syntax.Type);
        }

        static internal Type AnalyzeType(TypeSyntax type)
        {
            string typeName = type.GetName();
            return Compiler.GetType(typeName, type) ?? throw new InvalidDataException("Variables must have specific type");
        }
    }
}
