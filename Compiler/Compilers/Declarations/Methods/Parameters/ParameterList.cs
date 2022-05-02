// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace General.Shaders
{
    internal class ParameterList : Declaration, IVariableCollection
    {
        private ParameterListSyntax mSyntax;

        private List<Variable> mParameters = new List<Variable>();
        public IEnumerable<Variable> Parameters => mParameters;
        public int ParameterCount => mParameters.Count;

        public ParameterList(ParameterListSyntax syntax) : base("")
        {
            mSyntax = syntax;
        }

        protected override void internalAnalyze()
        {
            foreach (ParameterSyntax parameterSyntax in mSyntax.Parameters)
            {
                if (parameterSyntax.Type is null)
                {
                    throw new InvalidDataException();
                }

                Variable parameter = new Variable(parameterSyntax);
                mParameters.Add(parameter);
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

            Variable? parameter = mParameters.Find(p => p.Name == parameterName);
            if (parameter is null)
            {
                throw new InvalidDataException();
            }

            return parameter.Type;
        }

        Variable? IVariableCollection.GetVariable(string name)
        {
            return mParameters.Find(p => p.Name == name);
        }

        void IVariableCollection.PushVariable(Variable variable) => throw new InvalidOperationException("Should never push local variable to a parameter list");
    }
}
