// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders
{
    public interface IVertexSource
    {
        void OnVertex(InputVertex input, OutputVertex output);
    }

    public interface IFragmentSource
    {
        void OnFragment(InputFragment input, OutputFragment output);
    }
}
