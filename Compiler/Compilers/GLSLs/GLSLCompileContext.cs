﻿// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System.Collections.Generic;

namespace General.Shaders
{
    class GLSLCompileContext : CompileContext
    {
        public List<string> PushConstants = new List<string>();

        public void AddPushConstant(string content)
        {
            this.PushConstants.Add(content);
        }
    }
}
