// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System.Collections.Generic;

namespace General.Shaders
{
    class GLSLCompileContext : CompileContext
    {
        public Dictionary<string, string> mUniforms = new Dictionary<string, string>();
        public IEnumerable<string> Uniforms => mUniforms.Values;

        public Dictionary<string, string> mPushConstants = new Dictionary<string, string>();
        public IEnumerable<string> PushConstants => mPushConstants.Values;

        public void AddUniform(string name, string content)
        {
            if (mUniforms.ContainsKey(name))
            {
                return;
            }

            mUniforms.Add(name, content);
        }

        public void AddPushConstant(string name, string content)
        {
            if (mPushConstants.ContainsKey(name))
            {
                return;
            }

            mPushConstants.Add(name, content);
        }
    }
}
