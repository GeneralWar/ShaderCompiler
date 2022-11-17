// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

static public partial class Extension
{
    static public bool CompareTypeName(this TypeSyntax syntax, string name)
    {
        if (syntax is IdentifierNameSyntax)
        {
            return (syntax as IdentifierNameSyntax)?.Identifier.ValueText == name;
        }
        Debugger.Break();
        throw new NotImplementedException();
    }

    static public string GetName(this SyntaxNode syntax)
    {
        ExpressionSyntax? expressionSyntax = syntax as ExpressionSyntax;
        if (expressionSyntax is not null)
        {
            return GetName(expressionSyntax);
        }

        MethodDeclarationSyntax? methodDeclarationSyntax = syntax as MethodDeclarationSyntax;
        if (methodDeclarationSyntax is not null)
        {
            return GetName(methodDeclarationSyntax);
        }

        AttributeSyntax? attributeSyntax = syntax as AttributeSyntax;
        if (attributeSyntax is not null)
        {
            return attributeSyntax.Name.GetName();
        }

        ArgumentSyntax? argumentSyntax = syntax as ArgumentSyntax;
        if (argumentSyntax is not null)
        {
            return argumentSyntax.Expression.GetName();
        }

        FieldDeclarationSyntax? fieldDeclarationSyntax = syntax as FieldDeclarationSyntax;
        if (fieldDeclarationSyntax is not null)
        {
            return fieldDeclarationSyntax.ToString();
        }

        PropertyDeclarationSyntax? propertyDeclarationSyntax = syntax as PropertyDeclarationSyntax;
        if (propertyDeclarationSyntax is not null)
        {
            return propertyDeclarationSyntax.Identifier.ValueText;
        }

        ClassDeclarationSyntax? classDeclarationSyntax = syntax as ClassDeclarationSyntax;
        if (classDeclarationSyntax is not null)
        {
            return classDeclarationSyntax.Identifier.ValueText;
        }

        throw new NotImplementedException();
    }

    static public string GetName(this ExpressionSyntax syntax)
    {
        TypeSyntax? typeSyntax = syntax as TypeSyntax;
        if (typeSyntax is not null)
        {
            return typeSyntax.GetName();
        }

        IdentifierNameSyntax? identifierNameSyntax = syntax as IdentifierNameSyntax;
        if (identifierNameSyntax is not null)
        {
            return identifierNameSyntax.GetName();
        }

        MemberAccessExpressionSyntax? memberAccessExpressionSyntax = syntax as MemberAccessExpressionSyntax;
        if (memberAccessExpressionSyntax is not null)
        {
            return memberAccessExpressionSyntax.GetName();
        }

        Debugger.Break();
        throw new NotImplementedException();
    }

    static public string GetName(this TypeSyntax syntax)
    {
        IdentifierNameSyntax? identifierNameSyntax = syntax as IdentifierNameSyntax;
        if (identifierNameSyntax is not null)
        {
            return identifierNameSyntax.GetName();
        }

        ArrayTypeSyntax? arrayTypeSyntax = syntax as ArrayTypeSyntax;
        if (arrayTypeSyntax is not null)
        {
            return arrayTypeSyntax.ElementType.GetName() + "[]";
        }

        PredefinedTypeSyntax? predefinedTypeSyntax = syntax as PredefinedTypeSyntax;
        if (predefinedTypeSyntax is not null)
        {
            return predefinedTypeSyntax.Keyword.ValueText;
        }

        Debugger.Break();
        throw new NotImplementedException();
    }

    static public string GetSafeName(this TypeSyntax syntax)
    {
        IdentifierNameSyntax? identifierNameSyntax = syntax as IdentifierNameSyntax;
        if (identifierNameSyntax is not null)
        {
            return identifierNameSyntax.GetName();
        }

        ArrayTypeSyntax? arrayTypeSyntax = syntax as ArrayTypeSyntax;
        if (arrayTypeSyntax is not null)
        {
            return arrayTypeSyntax.ElementType.GetName() + "[]";
        }

        PredefinedTypeSyntax? predefinedTypeSyntax = syntax as PredefinedTypeSyntax;
        if (predefinedTypeSyntax is not null)
        {
            return predefinedTypeSyntax.Keyword.ValueText;
        }

        QualifiedNameSyntax? qualifiedNameSyntax = syntax as QualifiedNameSyntax;
        if (qualifiedNameSyntax is not null)
        {
            return qualifiedNameSyntax.Right.GetSafeName();
        }

        Debugger.Break();
        throw new NotImplementedException();
    }

    static public string GetName(this IdentifierNameSyntax syntax)
    {
        return syntax.Identifier.ValueText;
    }

    static public string GetName(this MemberAccessExpressionSyntax syntax)
    {
        return syntax.Expression.GetName() + syntax.OperatorToken.Text + syntax.Name.GetName();
    }

    static public bool Contains(this SeparatedSyntaxList<BaseTypeSyntax> types, string typename)
    {
        foreach (BaseTypeSyntax type in types)
        {
            if (type is SimpleBaseTypeSyntax)
            {
                if ((type as SimpleBaseTypeSyntax)?.Type.CompareTypeName(typename) ?? false)
                {
                    return true;
                }
            }
            else
            {
                Debugger.Break();
            }
        }
        return false;
    }

    static public string GetFullName(this NameSyntax syntax)
    {
        return syntax.SyntaxTree.GetText().ToString(syntax.FullSpan).Trim();
    }

    static public string GetName(this ClassDeclarationSyntax syntax)
    {
        return syntax.Identifier.Text;
    }

    static public string GetFullName(this ClassDeclarationSyntax syntax)
    {
        string name = syntax.Identifier.Text;
        NamespaceDeclarationSyntax? namespaceSyntax = syntax.Parent as NamespaceDeclarationSyntax;
        if (namespaceSyntax is null)
        {
            return name;
        }
        return namespaceSyntax.Name.GetFullName().Trim() + "." + name;
    }

    static public string GetName(this MethodDeclarationSyntax syntax)
    {
        return syntax.Identifier.ValueText;
    }

    static public string GetFullName(this MethodDeclarationSyntax syntax)
    {
        string name = syntax.Identifier.ValueText;
        if (syntax.ExplicitInterfaceSpecifier is not null)
        {
            return syntax.ExplicitInterfaceSpecifier.Name.GetFullName() + "." + name;
        }
        return name;
    }

    static public bool CompareName(this MethodDeclarationSyntax? syntax, string name)
    {
        if (syntax is null)
        {
            throw new NullReferenceException();
        }

        return syntax.Identifier.ValueText == name;
    }

    static public bool CompareName(this PropertyDeclarationSyntax? syntax, string name)
    {
        if (syntax is null)
        {
            throw new NullReferenceException();
        }

        return syntax.Identifier.ValueText == name;
    }

    static public bool CompareName(this ClassDeclarationSyntax? syntax, string name)
    {
        if (syntax is null)
        {
            throw new NullReferenceException();
        }

        return syntax.Identifier.ValueText == name;
    }

    static public bool CompareName(this MemberDeclarationSyntax syntax, string name)
    {
        if (syntax is MethodDeclarationSyntax)
        {
            return CompareName(syntax as MethodDeclarationSyntax, name);
        }
        if (syntax is PropertyDeclarationSyntax)
        {
            return CompareName(syntax as PropertyDeclarationSyntax, name);
        }
        if (syntax is ClassDeclarationSyntax)
        {
            return CompareName(syntax as ClassDeclarationSyntax, name);
        }
        Debugger.Break();
        throw new NotImplementedException();
    }

    static public MemberDeclarationSyntax? Find(this SyntaxList<MemberDeclarationSyntax> members, string name)
    {
        foreach (MemberDeclarationSyntax member in members)
        {
            if (member.CompareName(name))
            {
                return member;
            }
        }
        return null;
    }

    static public T? Find<T>(this SyntaxList<MemberDeclarationSyntax> members, string name) where T : MemberDeclarationSyntax
    {
        return Find(members, name) as T;
    }

    static public Type? GetTypeFromRoot(this SyntaxNode syntax)
    {
        return syntax.GetTypeFromRoot(syntax.GetName());
    }

    static public Type? GetTypeFromRoot(this SyntaxNode syntax, string name)
    {
        if ("int" == name)
        {
            return typeof(int);
        }

        CompilationUnitSyntax? root = syntax.SyntaxTree.GetRoot() as CompilationUnitSyntax;
        if (root is null)
        {
            throw new InvalidDataException();
        }

        Type? type = Extension.GetType(name) ?? root.GetType(name);
        if (type is null)
        {
            string? space = null;
            NamespaceDeclarationSyntax? namespaceSyntax = syntax.GetCurrentNamespace();
            if (namespaceSyntax is not null)
            {
                space = namespaceSyntax.Name.GetFullName();
            }
            if (!string.IsNullOrWhiteSpace(space))
            {
                type = Extension.GetType(space + "." + name);
                while (type is null && space.Contains("."))
                {
                    space = space.Substring(0, space.LastIndexOf('.'));
                    type = Extension.GetType(space + "." + name);
                }
            }
            if (type is null)
            {
                ClassDeclarationSyntax? declaringClass = syntax.Parent as ClassDeclarationSyntax;
                if (declaringClass is not null)
                {
                    string fullTypeName = declaringClass.GetFullName() + "+" + name;
                    type = Extension.GetType(fullTypeName);
                }
            }
        }
        return type;
    }

    static public Type? GetTypeFromRoot(this SyntaxTree tree, string name)
    {
        CompilationUnitSyntax? root = tree.GetRoot() as CompilationUnitSyntax;
        if (root is null)
        {
            throw new InvalidDataException();
        }

        return root.GetType(name);
    }

    static public Type? GetType(this CompilationUnitSyntax root, string name)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (UsingDirectiveSyntax usingSyntax in root.Usings)
        {
            string fullname = usingSyntax.Name.GetFullName() + "." + name;
            foreach (Assembly assembly in assemblies)
            {
                Type? type = assembly.GetType(fullname);
                if (type is not null)
                {
                    return type;
                }
            }
        }
        return null;
    }

    static public NamespaceDeclarationSyntax? GetCurrentNamespace(this SyntaxNode syntax)
    {
        NamespaceDeclarationSyntax? namespaceSyntax = syntax as NamespaceDeclarationSyntax;
        if (namespaceSyntax is not null)
        {
            return namespaceSyntax;
        }

        return syntax.Parent?.GetCurrentNamespace();
    }

    static internal object? ToInstance(this SyntaxNode syntax)
    {
        ExpressionSyntax? expressionSyntax = syntax as ExpressionSyntax;
        if (expressionSyntax is not null)
        {
            return Declaration.ToInstance(expressionSyntax);
        }

        AttributeArgumentSyntax? attributeArgumentSyntax = syntax as AttributeArgumentSyntax;
        if (attributeArgumentSyntax is not null)
        {
            return ToInstance(attributeArgumentSyntax.Expression);
        }

        AttributeSyntax? attributeSyntax = syntax as AttributeSyntax;
        if (attributeSyntax is not null)
        {
            return ToInstance(attributeSyntax);
        }

        throw new NotImplementedException();
    }

    static internal bool Match(this ParameterInfo parameter, object? argument)
    {
        Type parameterType = parameter.ParameterType;
        if (null == argument)
        {
            return !parameterType.IsValueType;
        }

        Type argumentType = argument.GetType();
        return argumentType == parameterType || argumentType.IsSubclassOf(parameterType);
    }

    static internal bool MatchParameters(ParameterInfo[] parameters, object?[]? arguments)
    {
        if (0 == parameters.Length &&(arguments is null || 0 == arguments.Length))
        {
            return true;
        }

        if (parameters.Length != arguments?.Length)
        {
            return false;
        }

        for (int i = 0; i < parameters.Length; ++i)
        {
            if (!parameters[i].Match(arguments[i]))
            {
                return false;
            }
        }
        return true;
    }

    static internal object CreateInstance(ConstructorInfo[] constructors, object?[]? arguments)
    {
        ConstructorInfo? constructor = constructors.FirstOrDefault(c => MatchParameters(c.GetParameters(), arguments));
        return constructor?.Invoke(arguments) ?? throw new InvalidOperationException();
    }

    static internal object CreateInstance(Type type, object?[]? arguments)
    {
        ConstructorInfo[] constructors = type.GetConstructors((BindingFlags)int.MaxValue);
        ConstructorInfo? constructor = constructors.FirstOrDefault(c => MatchParameters(c.GetParameters(), arguments));
        return constructor?.Invoke(arguments) ?? throw new InvalidOperationException();
    }

    static internal System.Attribute ToInstance(this AttributeSyntax syntax)
    {
        Type type = syntax.GetTypeFromRoot(syntax.GetName() + nameof(System.Attribute)) ?? throw new InvalidOperationException();
        return CreateInstance(type, syntax.ArgumentList?.Arguments.ToArguments())as System.Attribute ?? throw new InvalidOperationException();
    }

    static internal object?[]? ToArguments(this IEnumerable<SyntaxNode>? syntaxList)
    {
        if (syntaxList is null)
        {
            return null;
        }

        List<object?> arguments = new List<object?>();
        foreach (SyntaxNode argumentSyntax in syntaxList)
        {
            arguments.Add(argumentSyntax.ToInstance());
        }
        return arguments.ToArray();
    }
}