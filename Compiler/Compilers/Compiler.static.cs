// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
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
                case Language.GLSL: return new GLSLCompiler();
            }

            throw new Exception();
        }

        static public void CompileProject(string projectPath, Language language, string assemblyPath, string outputDirectory)
        {
            AppDomain.CurrentDomain.Load(typeof(IVertexSource).Assembly.FullName ?? throw new InvalidDataException());

            Assembly assembly = Assembly.LoadFile(assemblyPath);

            Compiler compiler = Create(language);
            compiler.SetOutputDirectory(outputDirectory);
            Namespace global = Initialize(Path.GetFullPath(projectPath));
            compiler.Compile(assembly, global);
        }

        static private Namespace Initialize(string projectPath)
        {
            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException(projectPath);
            }

            string? directory = Path.GetDirectoryName(projectPath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new DirectoryNotFoundException(projectPath);
            }

            Namespace global = new Namespace("global");
            List<string> decocatedFilenames = new List<string>();
            foreach (string filename in Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(filename));
                global.Analyze(syntaxTree.GetRoot());
                decocatedFilenames.Add(filename);
            }
            return global;
        }

        static public Type? GetType(string typeName, SyntaxNode? syntax = null)
        {
            if ("void" == typeName)
            {
                return typeof(void);
            }
            if ("float" == typeName)
            {
                return typeof(float);
            }

            if (syntax is not null)
            {
                Type? type = syntax.GetTypeFromRoot(typeName);
                if (type is not null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
