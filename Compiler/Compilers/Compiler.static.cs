// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;
using System.IO;
using System.Reflection;

namespace General.Shaders
{
    public abstract partial class Compiler
    {
        static private Compiler Create(Language language)
        {
            switch (language)
            {
                case Language.GLSL:
                    return new GLSLCompiler();
            }

            throw new Exception();
        }

        static public void CompileProject(string projectPath, Language language, string assemblyPath, string outputDirectory)
        {
            AppDomain.CurrentDomain.Load(typeof(IVertexSource).Assembly.FullName ?? throw new InvalidDataException());

            Assembly assembly = Assembly.LoadFile(assemblyPath);

            Compiler compiler = Create(language);
            compiler.SetOutputDirectory(outputDirectory);
            compiler.Initialize(Path.GetFullPath(projectPath));
            compiler.Analyze();
            compiler.Compile(assembly);
        }
    }
}
