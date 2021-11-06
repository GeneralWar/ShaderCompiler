using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;

static public partial class SyntaxExtensions
{
    static public bool CompareTypeName(this TypeSyntax syntax, string name)
    {
        if (syntax is IdentifierNameSyntax)
        {
            return (syntax as IdentifierNameSyntax)?.Identifier.ValueText == name;
        }
        Debugger.Break();
        return false;
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
        return syntax.SyntaxTree.GetText().ToString(syntax.FullSpan);
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
}