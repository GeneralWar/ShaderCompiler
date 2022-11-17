// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;

namespace General.Shaders
{
    static public class DebugFunctions
    {
        [FunctionName(Language.GLSL, "debugPrintfEXT"), NeedExtension(Language.GLSL, "GL_EXT_debug_printf")] 
        static public float Log(string format, params object[] arguments) => throw new NotImplementedException();
    }
}
