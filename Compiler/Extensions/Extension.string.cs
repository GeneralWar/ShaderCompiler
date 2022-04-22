// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

static public partial class Extension
{
    static public void Append(this StringBuilder builder, int tabCount, string content)
    {
        for (int i = 0; i < tabCount; ++i)
        {
            builder.Append("\t");
        }
        builder.Append(content);
    }

    static public void AppendLine(this StringBuilder builder, int tabCount, string content)
    {
        for (int i = 0; i < tabCount; ++i)
        {
            builder.Append("\t");
        }
        builder.AppendLine(content);
    }
}