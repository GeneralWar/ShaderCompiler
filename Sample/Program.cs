using General.Shaders;
using System.IO;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Compiler.CompileProject(args[0], Language.GLSL, Path.GetFullPath("Shaders.dll"), "Shaders/GLSLs");
        }
    }
}
