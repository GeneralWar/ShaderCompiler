// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            throw new NotImplementedException("Will support other languages in future");
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

            Namespace global = new Namespace(nameof(global));
            List<string> decocatedFilenames = new List<string>();
            foreach (string filename in Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(filename));
                global.Analyze(syntaxTree.GetRoot());
                decocatedFilenames.Add(filename);
            }
            return global;
        }

        static private void GetAllReferences(IReferenceHost host, HashSet<Declaration> references)
        {
            foreach (Declaration reference in host.References)
            {
                if (references.Contains(reference))
                {
                    continue;
                }

                references.Add(reference);

                IReferenceHost? referenceHost = reference as IReferenceHost;
                if (referenceHost is not null)
                {
                    Compiler.GetAllReferences(referenceHost, references);
                }
            }
        }

        static internal HashSet<Declaration> GetAllReferences(IReferenceHost host)
        {
            HashSet<Declaration> references = new HashSet<Declaration>();
            Compiler.GetAllReferences(host, references);
            return references;
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

        static public void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Trace.WriteLine($"[INFO]: {message}");
        }

        static public void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Trace.WriteLine($"[WARN]: {message}");
        }

        static public void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Trace.WriteLine($"[ERROR]: {message}");
        }
    }
}
