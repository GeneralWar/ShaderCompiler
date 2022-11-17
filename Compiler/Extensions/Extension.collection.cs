// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System.Collections.Generic;

static public partial class Extension
{
    static public void AddRange<T>(this HashSet<T> instance, IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            instance.Add(item);
        }
    }
}