// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;

namespace General.Shaders
{
    public abstract class GraphicsShader
    {
        public abstract IVertexSource VertexShader { get; }
        public abstract IFragmentSource FragmentShader { get; }
    }
}
