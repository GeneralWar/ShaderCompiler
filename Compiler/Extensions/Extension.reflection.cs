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

static public partial class Extension
{
    static public bool ImplementInterface<TInterfaceType>(this Type type)
    {
        Type interfaceType = typeof(TInterfaceType);
        if (!interfaceType.IsInterface)
        {
            throw new InvalidDataException();
        }

        return type.GetInterface(interfaceType.FullName ?? interfaceType.Name) == interfaceType;
    }

    static public Type GetMemberType(this Type type, string name)
    {
        MemberInfo[] members = type.GetMember(name);
        Trace.Assert(1 == members.Length);
        return members[0].GetMemberType();
    }

    static public Type GetMemberType(this MemberInfo memberInfo)
    {
        FieldInfo? fieldInfo = memberInfo as FieldInfo;
        if (fieldInfo is not null)
        {
            return fieldInfo.FieldType;
        }

        PropertyInfo? propertyInfo = memberInfo as PropertyInfo;
        if (propertyInfo is not null)
        {
            return propertyInfo.PropertyType;
        }

        Type? type = memberInfo as Type;
        if (type is not null)
        {
            return type;
        }

        throw new NotImplementedException();
    }

    /// <summary>
    /// Get type of member in target
    /// </summary>
    /// <param name="target">Instance of type where member declared</param>
    /// <param name="targetMemberName">Name of member</param>
    /// <example>string a = ""; a.GetMemberType("Length") is int;</example>
    /// <returns>Type of member</returns>
    static public Type? GetMemberType(this object target, string targetMemberName)
    {
        MemberInfo[] members = target.GetType().GetMember(targetMemberName);
        Trace.Assert(1 == members.Length);
        return members[0].GetMemberType(target);
    }

    static public Type? GetMemberType(this MemberInfo memberInfo, object target)
    {
        FieldInfo? fieldInfo = memberInfo as FieldInfo;
        if (fieldInfo is not null)
        {
            return fieldInfo.GetValue(target)?.GetType();
        }

        PropertyInfo? propertyInfo = memberInfo as PropertyInfo;
        if (propertyInfo is not null)
        {
            return propertyInfo.GetValue(target)?.GetType();
        }

        throw new NotImplementedException();
    }

    static public string GetShaderTypeName(this Type type, Language language)
    {
        if (type.IsArray)
        {
            return GetShaderTypeName(type.GetElementType() ?? throw new InvalidDataException("Array must has element type"), language);
        }

        if (type.IsPrimitive)
        {
            if (typeof(float) == type)
            {
                return "float";
            }

            throw new NotImplementedException();
        }

        IEnumerable<TypeNameAttribute> attributes = type.GetCustomAttributes<TypeNameAttribute>();
        TypeNameAttribute? attribute = attributes.FirstOrDefault(a => a.Language == language);
        if (attribute is null)
        {
            throw new InvalidDataException();
        }

        return attribute.Name;
    }

    static public string GetShaderInstanceName(this Type type, Language language)
    {
        IEnumerable<InstanceNameAttribute> attributes = type.GetCustomAttributes<InstanceNameAttribute>();
        InstanceNameAttribute? attribute = attributes.FirstOrDefault(a => a.Language == language);
        if (attribute is null)
        {
            throw new InvalidDataException();
        }

        return attribute.Name;
    }

    static public string? GetFunctionName(this MethodInfo info, Language language)
    {
        IEnumerable<FunctionNameAttribute> attributes = info.GetCustomAttributes<FunctionNameAttribute>();
        FunctionNameAttribute? attribute = attributes.FirstOrDefault(a => a.Language == language);
        return attribute?.Name;
    }

    static public Type? GetType(string fullTypeName)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? type = assembly.GetType(fullTypeName);
            if (type is not null)
            {
                return type;
            }
        }
        return null;
    }
}