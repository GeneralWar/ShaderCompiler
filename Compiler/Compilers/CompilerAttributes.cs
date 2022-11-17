// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class CompilerAttribute : System.Attribute
    {
        internal Type Type;

        public CompilerAttribute(Type type)
        {
            this.Type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class ExpressionCompilerAttribute : CompilerAttribute
    {
        public ExpressionCompilerAttribute(Type type) : base(type) { }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class StatementCompilerAttribute : CompilerAttribute
    {
        public StatementCompilerAttribute(Type type) : base(type) { }
    }
}
