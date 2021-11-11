// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace General.Shaders
{
    class Variable : Declaration
    {
        private SyntaxNode? mSyntax = null;

        private Type mType;
        private string mTypeString;
        public Type Type => mType;

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

        protected override void internalAnalyze(Compiler compiler)
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
            MemberInfo[] members = mType.GetMember(name);
            Trace.Assert(1 == members.Length);

            return compiler.AnalyzeMemberName(members[0]);
        }

        public override string ToString()
        {
            return $"{this.Name}({mTypeString})";
        }
    }
}
