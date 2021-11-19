// Author: ÷ÏºŒ¡È(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

static public partial class SyntaxExtensions
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
        return namespaceSyntax.GetFullName().Trim() + "." + name;
    }

    static public string GetFullName(this NamespaceDeclarationSyntax syntax)
    {
        string name = syntax.Name.GetFullName();
        NamespaceDeclarationSyntax? namespaceSyntax = syntax.Parent as NamespaceDeclarationSyntax;
        if (namespaceSyntax is null)
        {
            return name;
        }
        return namespaceSyntax.Name.GetFullName().Trim() + "." + name;
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

    static public bool CompareName(this MemberDeclarationSyntax syntax, string name)
    {
        if (syntax is MethodDeclarationSyntax)
        {
            return CompareName(syntax as MethodDeclarationSyntax, name);
        }
        Debugger.Break();
        return false;
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

    static public Type? GetTypeFromRoot(this SyntaxNode syntax, string name)
    {
        CompilationUnitSyntax? root = syntax.SyntaxTree.GetRoot() as CompilationUnitSyntax;
        if (root is null)
        {
            throw new InvalidDataException();
        }

        return root.GetType(name);
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
        foreach (UsingDirectiveSyntax usingSyntax in root.Usings)
        {
            string fullname = usingSyntax.Name.GetFullName() + "." + name;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
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
}