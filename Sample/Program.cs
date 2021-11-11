// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders;
using System;
using System.IO;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Compiler.CompileProject(args[0], Language.GLSL, Path.GetFullPath("Shaders.dll"), "Shaders/GLSLs");

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }
    }
}
