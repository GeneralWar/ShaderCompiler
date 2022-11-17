// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace General.Shaders
{
    public abstract partial class Declaration : ISyntaxHost
    {
        internal DeclarationContainer Root { get; private set; }

        internal SyntaxNode Syntax { get; private set; }
        SyntaxNode ISyntaxHost.SyntaxNode => this.Syntax;

        public string Name { get; private set; }
        public string FullName { get; protected set; }

        protected Declaration? Parent { get; private set; }

        private AnalyzeStatus mAnalyzeStatus = AnalyzeStatus.Initialized;

        private HashSet<System.Attribute> mAttributes = new HashSet<System.Attribute>();
        internal IEnumerable<System.Attribute> Attributes => mAttributes;

        private HashSet<Attribute> mAttributeDeclarations = new HashSet<Attribute>();
        internal IEnumerable<Attribute> AttributeDeclarations => mAttributeDeclarations;

        internal Declaration(DeclarationContainer root, SyntaxNode syntax, string name) : this(root, syntax, name, name) { }

        internal Declaration(DeclarationContainer root, SyntaxNode syntax, string name, string fullName)
        {
            this.Name = name;
            this.Syntax = syntax;
            this.FullName = fullName;
            this.Root = root ?? this as DeclarationContainer ?? throw new InvalidOperationException();
        }

        protected internal void SetParent(Declaration parent)
        {
            this.Parent = parent;
        }

        public bool HasAncestor(object test)
        {
            Declaration? parent = this.Parent;
            while (parent is not null && parent != test)
            {
                parent = parent.Parent;
            }
            return parent == test;
        }

        public T? FindAncestor<T>() where T : Declaration
        {
            T? ancestor = null;
            Declaration? parent = this.Parent;
            while (parent is not null && (ancestor = parent as T) is null)
            {
                parent = parent.Parent;
            }
            return ancestor;
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

        //protected Type getTypeByTypeName(string name)
        //{
        //    return Compiler.GetType(name, this.Syntax) ?? throw new InvalidDataException("Variables must have specific type");
        //}

        protected void AddAttribute(System.Attribute attribute) => mAttributes.Add(attribute);
        internal T? GetAttribute<T>() where T : System.Attribute => mAttributes.FirstOrDefault(a => a is T) as T;

        internal void AddAttribute(Attribute attribute) => mAttributeDeclarations.Add(attribute);

        internal Attribute? GetAttributeDeclaration<T>() where T : System.Attribute => mAttributeDeclarations.FirstOrDefault(a => a.Type == typeof(T));

        public override string ToString()
        {
            return $"{this.FullName}({this.GetType().Name})";
        }
    }
}
