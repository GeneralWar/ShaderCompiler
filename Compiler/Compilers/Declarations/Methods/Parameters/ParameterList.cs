// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace General.Shaders
{
    internal class ParameterList : Declaration
    {
        private ParameterListSyntax mSyntax;

        private Dictionary<string, Variable> mParameters = new Dictionary<string, Variable>();
        public IEnumerable<Variable> Parametes => mParameters.Values;

        public ParameterList(ParameterListSyntax syntax) : base("")
        {
            mSyntax = syntax;
        }

        protected override void internalAnalyze(Compiler compiler)
        {
            foreach (ParameterSyntax parameterSyntax in mSyntax.Parameters)
            {
                if (parameterSyntax.Type is null)
                {
                    throw new InvalidDataException();
                }

                Variable parameter = new Variable(parameterSyntax);
                mParameters.TryAdd(parameter.Name, parameter);
            }
        }

        public Type GetParameterType(string parameterName)
        {
            if (parameterName.Contains('.'))
            {
                int dotIndex = parameterName.IndexOf('.');
                Type type = this.GetParameterType(parameterName.Substring(0, dotIndex));
                MemberInfo[] members = type.GetMember(parameterName.Substring(dotIndex + 1));
                Trace.Assert(1 == members.Length);
                return members[0].GetMemberType();
            }

            Variable? parameter;
            if (!mParameters.TryGetValue(parameterName, out parameter))
            {
                throw new InvalidDataException();
            }

            return parameter.Type;
        }
    }
}
