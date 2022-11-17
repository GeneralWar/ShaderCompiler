// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;

namespace General.Shaders
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class NeedExtensionAttribute : Attribute
    {
        public Language Language { get; private set; }
        public string ExtentionName { get; private set; }

        public NeedExtensionAttribute(Language language, string extensionName)
        {
            this.Language = language;
            this.ExtentionName = extensionName;
        }
    }
}
