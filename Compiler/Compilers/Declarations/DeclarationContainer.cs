using System.Collections.Generic;
using System.IO;

namespace General.Shaders
{
    internal abstract class DeclarationContainer : Declaration
    {
        private Dictionary<string, Declaration> mChildren = new Dictionary<string, Declaration>();

        public DeclarationContainer(string name) : base(name) { }

        public DeclarationContainer(string name, string fullName) : base(name, fullName) { }

        public Declaration? GetDeclaration(string name)
        {
            if (name.Contains("."))
            {
                int dotIndex = name.IndexOf('.');
                string prefix = name.Substring(0, dotIndex);
                DeclarationContainer? container = this.GetDeclaration(prefix) as DeclarationContainer;
                if (container is null)
                {
                    return null;
                }

                return container.GetDeclaration(name.Substring(dotIndex + 1));
            }

            Declaration? child;
            if (!mChildren.TryGetValue(name, out child))
            {
                return null;
            }
            return child;
        }

        protected abstract void checkDeclarationCanAdd(Declaration declaration);

        protected void addDeclarationDirectly(Declaration declaration)
        {
            mChildren.Add(declaration.Name, declaration);
            declaration.SetParent(this);
        }

        protected void AddDeclaration(Declaration declaration)
        {
            this.checkDeclarationCanAdd(declaration);

            string fullName = declaration.FullName;
            if (fullName.Contains('.'))
            {
                int lastDotIndex = fullName.LastIndexOf('.');
                string parentName = fullName.Substring(0, lastDotIndex);
                if (this.FullName == parentName)
                {
                    this.addDeclarationDirectly(declaration);
                    return;
                }

                Declaration? parent = this.GetDeclaration(parentName);
                if (parent is null || parent is not DeclarationContainer)
                {
                    throw new InvalidDataException();
                }

                (parent as DeclarationContainer)?.AddDeclaration(declaration);
            }

            this.addDeclarationDirectly(declaration);
        }

        protected override void internalAnalyze(Compiler compiler)
        {
            foreach (Declaration child in mChildren.Values)
            {
                child.Analyze(compiler);
            }
        }
    }
}
