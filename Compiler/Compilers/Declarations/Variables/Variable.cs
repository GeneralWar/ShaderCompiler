// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace General.Shaders
{
    public class Variable : Declaration
    {
        private SyntaxNode? mSyntax = null;

        private Type mType;
        private string mTypeString;
        public Type Type => mType;

        public Variable(PropertyDeclarationSyntax syntax) : base(syntax.Identifier.ValueText)
        {
            mSyntax = syntax;
            mType = this.analyzeType(syntax);
            mTypeString = mType.FullName ?? mType.Name;
        }

        public Variable(FieldDeclarationSyntax syntax) : base(syntax.ToString())
        {
            mSyntax = syntax;
            mType = this.analyzeType(syntax);
            mTypeString = mType.FullName ?? mType.Name;
        }

        public Variable(ParameterSyntax syntax) : base(syntax.Identifier.ValueText)
        {
            mSyntax = syntax;
            mType = this.analyzeType(syntax);
            mTypeString = mType.FullName ?? mType.Name;
        }

        public Variable(TypeSyntax type, VariableDeclaratorSyntax syntax) : base(syntax.Identifier.ValueText)
        {
            mSyntax = syntax;
            mType = this.analyzeType(type);
            mTypeString = mType.FullName ?? mType.Name;
        }

        protected override void internalAnalyze()
        {
            //throw new NotImplementedException();
        }

        private Type analyzeType(PropertyDeclarationSyntax syntax)
        {
            if (syntax.Type is null)
            {
                throw new InvalidDataException();
            }

            return this.analyzeType(syntax.Type);
        }

        private Type analyzeType(FieldDeclarationSyntax syntax)
        {
            throw new NotImplementedException();
        }

        private Type analyzeType(ParameterSyntax syntax)
        {
            if (syntax.Type is null)
            {
                throw new InvalidDataException();
            }

            return this.analyzeType(syntax.Type);
        }

        private Type analyzeType(TypeSyntax type)
        {
            string typeName = type.GetName();
            return this.getTypeByTypeName(typeName);
        }

        private Type getTypeByTypeName(string name)
        {
            Type? type = Type.GetType(name);
            if (type is null && name.Contains('.'))
            {
                throw new InvalidDataException();
            }

            type = mSyntax?.GetTypeFromRoot(name);
            if (type is null)
            {
                throw new InvalidDataException();
            }

            return type;
        }

        public string AnalyzeMemberAccess(Compiler compiler, string name)
        {
            if (typeof(InputVertex) == this.Type || typeof(OutputVertex) == this.Type || typeof(InputFragment) == this.Type || typeof(OutputFragment) == this.Type)
            {
                MemberInfo[] members = mType.GetMember(name);
                Trace.Assert(1 == members.Length);

                return compiler.AnalyzeMemberName(members[0]);
            }
            
            return $"{compiler.AnalyzeVariableName(this)}.{name}";
        }

        public override string ToString()
        {
            return $"{this.Name}({mTypeString})";
        }
    }
}
