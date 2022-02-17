// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders
{
    internal abstract partial class Declaration
    {
        public string Name { get; private set; }
        public string FullName { get; private set; }
        
        protected Declaration? Parent { get; private set; }

        public Declaration(string name) : this(name, name) { }

        public Declaration(string name, string fullName)
        {
            this.Name = name;
            this.FullName = fullName;
        }

        protected internal void SetParent(Declaration parent)
        {
            this.Parent = parent;
        }

        public void Analyze(Compiler compiler)
        {
            this.internalAnalyze(compiler);
        }

        protected abstract void internalAnalyze(Compiler compiler);

        public override string ToString()
        {
            return $"{this.FullName}({this.GetType().Name})";
        }
    }
}
