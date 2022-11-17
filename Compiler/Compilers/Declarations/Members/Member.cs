// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;

namespace General.Shaders
{
    internal class Member : Declaration
    {
        private MemberDeclarationSyntax mSyntax;
        private DeclarationContainer mDeclaringContainer;

        internal MemberInfo MemberInfo { get; init; }

        public Member(DeclarationContainer root, DeclarationContainer declaringContainer, MemberDeclarationSyntax syntax) : base(root, syntax, syntax.GetName())
        {
            mSyntax = syntax;
            mDeclaringContainer = declaringContainer;

            Type declaringType = declaringContainer.Syntax.GetTypeFromRoot() ?? throw new InvalidOperationException();
            MethodDeclarationSyntax? methodSyntax = syntax as MethodDeclarationSyntax;
            if (methodSyntax is not null)
            {
                string name = this.Name;
                ExplicitInterfaceSpecifierSyntax? specifier = methodSyntax.ExplicitInterfaceSpecifier;
                if (specifier is not null)
                {
                    string specifierName = specifier.Name.GetName();
                    Type specifierType = syntax.GetTypeFromRoot(specifierName) ?? throw new InvalidOperationException();
                    name = (specifierType.FullName ?? specifierType.Name) + specifier.DotToken.ValueText + name;
                }
                MemberInfo[] members = declaringType.GetMember(name, (BindingFlags)int.MaxValue);
                this.MemberInfo = members[0];
            }
            else
            {
                MemberInfo[] members = declaringType.GetMember(this.Name, (BindingFlags)int.MaxValue);
                this.MemberInfo = members[0];
            }

            foreach (System.Attribute attribute in this.MemberInfo.GetCustomAttributes(true))
            {
                this.AddAttribute(attribute);
            }
        }

        protected override void internalAnalyze()
        {
            //throw new NotImplementedException();
        }
    }
}
