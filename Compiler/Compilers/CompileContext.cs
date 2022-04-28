// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System.Collections.Generic;

namespace General.Shaders
{
    class CompileContext
    {
        public string? InputDeclaration { get; private set; } = null;
        public string? OutputDeclaration { get; private set; } = null;

        public string? VertexShader { get; private set; } = null;
        public string? FragmentShader { get; private set; } = null;

        private Dictionary<string, string> mStructures = new Dictionary<string, string>();
        public IEnumerable<string> Structures => mStructures.Values;

        private Dictionary<string, string> mReferences = new Dictionary<string, string>();
        public IEnumerable<string> References => mReferences.Values;

        public void SetInputDeclaration(string input)
        {
            this.InputDeclaration = input;
        }

        public void SetOutputDeclaration(string output)
        {
            this.OutputDeclaration = output;
        }

        public void SetVertexShader(string vertex)
        {
            this.VertexShader = vertex;
        }

        public void SetFragmentShader(string fragment)
        {
            this.FragmentShader = fragment;
        }

        public void AddStruct(string name, string content)
        {
            if (mStructures.ContainsKey(name))
            {
                return;
            }

            mStructures.Add(name, content);
        }

        public void AddReference(string name, string content)
        {
            if (mReferences.ContainsKey(name))
            {
                return;
            }

            mReferences.Add(name, content);
        }
    }
}
