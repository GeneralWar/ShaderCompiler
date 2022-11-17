// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

namespace General.Shaders
{
    interface IVariableCollection
    {
        Variable? GetVariable(string name);
        void PushVariable(Variable variable);
    }
}
