// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders
{
    public interface IVertexSource
    {
        void OnVertex(InputVertex input, UniformData uniforms, OutputVertex output);
    }

    public interface IFragmentSource
    {
        void OnFragment(InputFragment input, UniformData uniforms, OutputFragment output);
    }
}
