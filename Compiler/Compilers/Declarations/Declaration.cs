// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders
{
    public abstract partial class Declaration
    {
        public string Name { get; private set; }
        public string FullName { get; private set; }

        protected Declaration? Parent { get; private set; }

        private AnalyzeStatus mAnalyzeStatus = AnalyzeStatus.Initialized;

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

        public void Analyze()
        {
            if (AnalyzeStatus.Initialized == mAnalyzeStatus)
            {
                mAnalyzeStatus = AnalyzeStatus.Analyzing;
                this.internalAnalyze();
                mAnalyzeStatus = AnalyzeStatus.Analyzed;
            }
        }

        protected abstract void internalAnalyze();

        public override string ToString()
        {
            return $"{this.FullName}({this.GetType().Name})";
        }
    }
}
