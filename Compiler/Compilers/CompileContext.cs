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

        private List<string> mStructures = new List<string>();
        public string[] Structures => mStructures.ToArray();

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

        public void AddStruct(string structContent)
        {
            mStructures.Add(structContent);
        }
    }
}
