// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace General.Shaders
{
    public abstract partial class Declaration
    {
        [ExpressionCompiler(typeof(IdentifierNameSyntax))]
        static internal string CompileIdentifierNameSyntax(CompileContext context, IdentifierNameSyntax syntax)
        {
            string name = syntax.GetName();
            Declaration? variable = context.Compiler.GetVariable<Declaration>(name);
            if (variable is not null)
            {
                return context.Compiler.AnalyzeVariableName(context, variable);
            }

            Method[] methods = context.Compiler.GetMethods(name);
            if (methods.Length > 0)
            {
                Array.ForEach(methods, m => context.Compiler.AppendReference(m));
                return name;
            }

            throw new InvalidDataException();
        }

        [ExpressionCompiler(typeof(AssignmentExpressionSyntax))]
        static internal string CompileAssignmentExpressionSyntax(CompileContext context, AssignmentExpressionSyntax syntax)
        {
            string leftContent = CompileSyntax(context, syntax.Left);
            string rightContent = CompileSyntax(context, syntax.Right);
            Trace.Assert(!string.IsNullOrWhiteSpace(leftContent) && !string.IsNullOrWhiteSpace(rightContent));
            return $"{leftContent} {syntax.OperatorToken.Text} {rightContent}";
        }

        [ExpressionCompiler(typeof(MemberAccessExpressionSyntax))]
        static internal string CompileMemberAccessExpressionSyntax(CompileContext context, MemberAccessExpressionSyntax syntax)
        {
            string memberName = syntax.Name.GetName();
            if (syntax.Expression is IdentifierNameSyntax)
            {
                return CompileMemberAccessExpressionSyntax(context, syntax.Expression.GetName(), memberName);
            }

            MemberAccessExpressionSyntax? memberAccessExpressionSyntax = syntax.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpressionSyntax is not null)
            {
                string prefix = CompileMemberAccessExpressionSyntax(context, memberAccessExpressionSyntax) + syntax.OperatorToken.ValueText;
                Type type = GetMemberType(context.Compiler, memberAccessExpressionSyntax);
                if (type.GetCustomAttribute<MemberCollectorAttribute>() is not null)
                {
                    return prefix + memberName;
                }
                return prefix + context.Compiler.AnalyzeMemberName(type, memberName);
            }

            InvocationExpressionSyntax? invocationExpressionSyntax = syntax.Expression as InvocationExpressionSyntax;
            if (invocationExpressionSyntax is not null)
            {
                string targetName = CompileInvocationExpressionSyntax(context, invocationExpressionSyntax);
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
                return $"{elementAccessExpressionSyntax.Expression.GetName()}[{string.Join(", ", CompileArgumentList(context, elementAccessExpressionSyntax.ArgumentList))}]" + syntax.OperatorToken.ValueText + memberName;
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

        static internal Type GetMemberType(Compiler? compiler, MemberAccessExpressionSyntax syntax)
        {
            ThisExpressionSyntax? thisExpressionSyntax = syntax.Expression as ThisExpressionSyntax;
            if (thisExpressionSyntax is not null)
            {
                Class? classDeclaration = compiler?.GetCurrent<Class>();
                if (classDeclaration is null)
                {
                    throw new InvalidDataException();
                }

                return classDeclaration.Type.GetMemberType(syntax.Name.GetName());
            }

            ElementAccessExpressionSyntax? elementAccessExpressionSyntax = syntax.Expression as ElementAccessExpressionSyntax;
            if (elementAccessExpressionSyntax is not null)
            {
                if (elementAccessExpressionSyntax.ArgumentList is BracketedArgumentListSyntax)
                {
                    ITypeHost? variable = compiler?.GetVariable<ITypeHost>(elementAccessExpressionSyntax.Expression.GetName());
                    if (variable is null || !variable.Type.IsArray)
                    {
                        throw new InvalidDataException();
                    }

                    return (variable.Type.GetElementType() ?? throw new InvalidDataException("Arrays must have element type")).GetMemberType(syntax.Name.GetName());
                }

                throw new NotImplementedException();
            }

            if (syntax.Expression is IdentifierNameSyntax)
            {
                string name = syntax.Expression.GetName();
                ITypeHost? target = compiler?.GetVariable<ITypeHost>(name);
                if (target is null)
                {
                    Type? type = syntax.GetTypeFromRoot(name);
                    if (type is not null)
                    {
                        return type;
                    }

                    throw new InvalidDataException();
                }

                return target.Type.GetMemberType(syntax.Name.GetName());
            }

            throw new NotImplementedException();
        }

        [ExpressionCompiler(typeof(LiteralExpressionSyntax))]
        static internal string CompileLiteralExpressionSyntax(CompileContext context, LiteralExpressionSyntax literalExpressionSyntax)
        {
            switch ((SyntaxKind)literalExpressionSyntax.RawKind)
            {
                case SyntaxKind.NumericLiteralExpression:
                case SyntaxKind.StringLiteralExpression:
                    return literalExpressionSyntax.Token.Text;
            }

            Debugger.Break();
            throw new NotImplementedException();
        }

        [ExpressionCompiler(typeof(ElementAccessExpressionSyntax))]
        static internal string CompileElementAccessExpressionSyntax(CompileContext context, ElementAccessExpressionSyntax syntax)
        {
            string variableName = syntax.Expression.GetName();
            List<string> arguments = CompileBracketedArgumentListSyntax(context, syntax.ArgumentList);
            Trace.Assert(1 == arguments.Count);
            return context.Compiler.AnalyzeElementAccess(variableName, arguments[0]);
        }

        [ExpressionCompiler(typeof(BinaryExpressionSyntax))]
        static internal string CompileBinaryExpressionSyntax(CompileContext context, BinaryExpressionSyntax syntax)
        {
            string leftContent = CompileSyntax(context, syntax.Left);
            string rightContent = CompileSyntax(context, syntax.Right);
            Trace.Assert(!string.IsNullOrWhiteSpace(leftContent) && !string.IsNullOrWhiteSpace(rightContent));
            return $"{leftContent} {syntax.OperatorToken.Text} {rightContent}";
        }

        [ExpressionCompiler(typeof(ObjectCreationExpressionSyntax))]
        static internal string CompileObjectCreationExpressionSyntax(CompileContext context, ObjectCreationExpressionSyntax syntax)
        {
            Trace.Assert("new" == syntax.NewKeyword.Text);

            Type? type = syntax.GetTypeFromRoot(syntax.Type.GetName());
            if (type is null)
            {
                throw new InvalidDataException();
            }

            string typeName = type.GetShaderTypeName(context.Language);
            if (syntax.ArgumentList is not null)
            {
                ArgumentList argumentList = context.Compiler.CreateArgumentList(syntax.ArgumentList);
                List<string> arguments = argumentList.Compile(context);
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

        [ExpressionCompiler(typeof(InvocationExpressionSyntax))]
        static internal string CompileInvocationExpressionSyntax(CompileContext context, InvocationExpressionSyntax syntax)
        {
            string method = CompileSyntax(context, syntax.Expression);
            List<string> arguments = CompileArgumentList(context, syntax.ArgumentList);
            return $"{method}({string.Join(", ", arguments)})";
        }

        [ExpressionCompiler(typeof(ParenthesizedExpressionSyntax))]
        static internal string CompileParenthesizedExpressionSyntax(CompileContext context, ParenthesizedExpressionSyntax syntax)
        {
            return syntax.OpenParenToken + CompileSyntax(context, syntax.Expression) + syntax.CloseParenToken;
        }

        [ExpressionCompiler(typeof(PrefixUnaryExpressionSyntax))]
        static internal string CompilePrefixUnaryExpressionSyntax(CompileContext context, PrefixUnaryExpressionSyntax syntax)
        {
            return syntax.OperatorToken.Text + CompileSyntax(context, syntax.Operand);
        }

        [ExpressionCompiler(typeof(ConditionalExpressionSyntax))]
        static internal string CompileConditionalExpressionSyntax(CompileContext context, ConditionalExpressionSyntax syntax)
        {
            return $"{CompileSyntax(context, syntax.Condition)} {syntax.QuestionToken.ValueText} {CompileSyntax(context, syntax.WhenTrue)} {syntax.ColonToken.ValueText} {CompileSyntax(context, syntax.WhenFalse)}";
        }

        static internal object? ToInstance(ExpressionSyntax syntax)
        {
            CastExpressionSyntax? castExpression = syntax as CastExpressionSyntax;
            if (castExpression is not null)
            {
                return Declaration.ToInstance(castExpression);
            }

            MemberAccessExpressionSyntax? memberAccessExpression = syntax as MemberAccessExpressionSyntax;
            if (memberAccessExpression is not null)
            {
                return Declaration.ToInstance(memberAccessExpression);
            }

            InvocationExpressionSyntax? invocationExpressionSyntax = syntax as InvocationExpressionSyntax;
            if (invocationExpressionSyntax is not null)
            {
                return Declaration.ToInstance(invocationExpressionSyntax);
            }

            LiteralExpressionSyntax? literalExpressionSyntax = syntax as LiteralExpressionSyntax;
            if (literalExpressionSyntax is not null)
            {
                return literalExpressionSyntax.Token.ValueText;
            }

            throw new NotImplementedException();
        }

        static internal object? ToInstance(CastExpressionSyntax syntax)
        {
            TypeSyntax typeSyntax = syntax.Type;
            Type type = typeSyntax.GetTypeFromRoot(typeSyntax.GetName()) ?? throw new InvalidOperationException();
            object? value = Declaration.ToInstance(syntax.Expression);
            return Convert.ChangeType(value, type);
        }

        static internal object? ToInstance(MemberAccessExpressionSyntax syntax)
        {
            Type type = Declaration.GetMemberType(null, syntax);
            ExpressionSyntax hostSyntax = syntax.Expression;
            SimpleNameSyntax memberSyntax = syntax.Name;
            if (hostSyntax.GetName() == type.Name)
            {
                MemberInfo[] members = type.GetMember(memberSyntax.GetName(), (BindingFlags)int.MaxValue);
                if (1 == members.Length)
                {
                    MemberInfo member = members[0];

                    FieldInfo? field = member as FieldInfo;
                    if (field is not null)
                    {
                        return field.GetValue(null);
                    }

                    PropertyInfo? property = member as PropertyInfo;
                    if (property is not null)
                    {
                        return property.GetValue(null);
                    }
                }

                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        static internal object? ToInstance(InvocationExpressionSyntax syntax)
        {
            ExpressionSyntax functionSyntax = syntax.Expression;
            if ("nameof" == functionSyntax.GetText().ToString())
            {
                string fullname = syntax.ArgumentList.Arguments[0].GetName();
                int index = fullname.LastIndexOf('.');
                return index > -1 ? fullname.Substring(index + 1) : fullname;
            }

            object?[]? arguments = syntax.ArgumentList?.Arguments.ToArguments();
            throw new NotImplementedException();
        }
    }
}
