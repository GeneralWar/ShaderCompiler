// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace General.Shaders
{
    public abstract partial class Compiler
    {
        private Language mLanguage = Language.GLSL;
        public Language Language => mLanguage;

        private string mOutputDirectory;
        public string OutputDirectory => mOutputDirectory;

        private Stack<Declaration> mScopeStack = new Stack<Declaration>();

        internal IVariableCollection CurrentVariableCollection => mScopeStack.FirstOrDefault(s => s is IVariableCollection) as IVariableCollection ?? throw new InvalidOperationException("Should always have more than one variable collection");
        private IReferenceHost? CurrentReferenceHost => mScopeStack.FirstOrDefault(s => s is IReferenceHost) as IReferenceHost;

        private Dictionary<Type, string> mVertexShaderPathMap = new Dictionary<Type, string>();
        private Dictionary<Type, string> mFragmentShaderPathMap = new Dictionary<Type, string>();

        private Namespace? Global { get; set; }

        public Compiler(Language language)
        {
            mLanguage = language;
            mOutputDirectory = $"Shaders/{language}s";
        }

        private void SetOutputDirectory(string path)
        {
            mOutputDirectory = path;
        }

        internal string AnalyzeMemberName(Type type, string name)
        {
            MemberInfo[] members = type.GetMember(name);
            Trace.Assert(1 == members.Length);

            return this.AnalyzeMemberName(members[0]);
        }

        internal string AnalyzeMemberName(MemberInfo memberInfo)
        {
            InputVertexAttribute? inputVertex = memberInfo.GetCustomAttribute<InputVertexAttribute>();
            if (inputVertex is not null)
            {
                return this.internalAnalyzeInputVertexMemberName(memberInfo, inputVertex);
            }

            OutputVertexAttribute? outputVertex = memberInfo.GetCustomAttribute<OutputVertexAttribute>();
            if (outputVertex is not null)
            {
                return this.internalAnalyzeOutputVertexMemberName(memberInfo, outputVertex);
            }

            InputFragmentAttribute? inputFragment = memberInfo.GetCustomAttribute<InputFragmentAttribute>();
            if (inputFragment is not null)
            {
                return this.internalAnalyzeInputFragmentMemberName(memberInfo, inputFragment);
            }

            OutputFragmentAttribute? outputFragment = memberInfo.GetCustomAttribute<OutputFragmentAttribute>();
            if (outputFragment is not null)
            {
                return this.internalAnalyzeOutputFragmentMemberName(memberInfo, outputFragment);
            }

            UniformFieldAttribute? uniformField = memberInfo.GetCustomAttribute<UniformFieldAttribute>();
            if (uniformField is not null)
            {
                //return this.analyzeUniformMemberName(memberInfo, uniformField);
                return memberInfo.Name;
            }

            if (memberInfo.DeclaringType?.GetCustomAttribute<UniformTypeAttribute>() is not null)
            {
                return memberInfo.Name;
            }

            throw new NotImplementedException();
        }

        protected abstract string internalAnalyzeInputVertexMemberName(MemberInfo memberInfo, InputVertexAttribute attribute);

        protected abstract string internalAnalyzeOutputVertexMemberName(MemberInfo memberInfo, OutputVertexAttribute attribute);

        protected abstract string internalAnalyzeInputFragmentMemberName(MemberInfo memberInfo, InputFragmentAttribute attribute);

        protected abstract string internalAnalyzeOutputFragmentMemberName(MemberInfo memberInfo, OutputFragmentAttribute attribute);

        protected abstract string internalAnalyzeUniformMemberName(MemberInfo memberInfo, UniformFieldAttribute attribute);

        public string AnalyzeElementAccess(string variableName, string elementName)
        {
            ITypeHost? variable = this.GetVariable<ITypeHost>(variableName);
            if (variable is null)
            {
                throw new InvalidDataException();
            }

            if (variable.Type.GetCustomAttribute<MemberCollectorAttribute>() is not null)
            {
                string name = elementName.Trim();
                if (name.EndsWith('"'))
                {
                    name = name.Substring(0, name.Length - 1);
                }
                if (name.StartsWith('"'))
                {
                    name = name.Substring(1);
                }
                return $"{variableName}.{name}";
            }

            return this.internalAnalyzeElementAccess(variableName, elementName);
        }

        protected abstract string internalAnalyzeElementAccess(string variableName, string elementName);

        internal T? GetVariable<T>(string name) where T : class
        {
            Trace.Assert(!name.Contains('.'));

            foreach (Declaration scope in mScopeStack)
            {
                Variable? variable = (scope as IVariableCollection)?.GetVariable(name);
                if (variable is T)
                {
                    return variable as T;
                }

                Declaration? member = (scope as DeclarationContainer)?.GetDeclaration(name);
                if (member is T)
                {
                    return member as T;
                }
            }
            return null;
        }

        internal string AnalyzeVariableName(CompileContext context, Declaration variable)
        {
            Member? member = variable as Member;
            if (member is not null)
            {
                context.CheckMember(member);
            }

            return this.internalAnalyzeVariableName(variable);
        }

        internal abstract string internalAnalyzeVariableName(Declaration variable);

        internal Method[] GetMethods(string name)
        {
            List<Method> methodList = new List<Method>();
            foreach (Declaration scope in mScopeStack)
            {
                Method[]? methods = (scope as IMethodProvider)?.GetMethods(name);
                if (methods is not null)
                {
                    methodList.AddRange(methods);
                }
            }
            return methodList.ToArray();
        }

        // current usage: lighting fragment refer to LightProcessor
        public void AppendReference(Declaration reference)
        {
            if (this.Global == reference)
            {
                return;
            }

            IReferenceHost? host = this.CurrentReferenceHost;
            if (host is null || reference.HasAncestor(host))
            {
                return;
            }

            host.AddReference(reference);
        }

        internal virtual bool PushScope(Declaration scope)
        {
            if (mScopeStack.Count > 0 && mScopeStack.Peek() == scope)
            {
                return false;
            }

            this.AppendReference(scope);
            mScopeStack.Push(scope);
            return true;
        }

        internal T? GetCurrent<T>() where T : Declaration
        {
            return mScopeStack.FirstOrDefault(c => c is T) as T;
        }

        internal virtual void PopScope(Declaration scope)
        {
            if (mScopeStack.Peek() != scope)
            {
                throw new InvalidOperationException();
            }

            mScopeStack.Pop();
        }

        public Type? GetType(string typeName)
        {
            return GetType(typeName, (mScopeStack.FirstOrDefault(c => c is ISyntaxHost) as ISyntaxHost)?.SyntaxNode);
        }

        public string FindVertexShaderPath(Type type)
        {
            return this.findShaderPath(type, mVertexShaderPathMap);
        }

        public string FindFragmentShaderPath(Type type)
        {
            return this.findShaderPath(type, mFragmentShaderPathMap);
        }

        private string findShaderPath(Type type, Dictionary<Type, string> map)
        {
            string? path;
            if (!map.TryGetValue(type, out path))
            {
                throw new InvalidDataException();
            }

            return path;
        }

        internal void Compile(Assembly assembly, Namespace global)
        {
            this.Global = global;
            this.PushScope(this.Global); // TODO: maybe should never get something from Global directly, enumerate scope stack instead

            HashSet<Type> vertexShaders = new HashSet<Type>();
            HashSet<Type> fragmentShaders = new HashSet<Type>();
            HashSet<Type> graphicsShaders = new HashSet<Type>();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.ImplementedInterface<IVertexSource>())
                {
                    vertexShaders.Add(type);
                }
                if (type.ImplementedInterface<IFragmentSource>())
                {
                    fragmentShaders.Add(type);
                }
                if (type.IsSubclassOf(typeof(GraphicsShader)))
                {
                    graphicsShaders.Add(type);
                }
            }

            foreach (Type type in vertexShaders)
            {
                this.compileVertexShader(type);
            }

            foreach (Type type in fragmentShaders)
            {
                this.compileFragmentShader(type);
            }

            foreach (Type type in graphicsShaders)
            {
                this.compileGraphicsShader(type);
            }
        }

        private void compileVertexShader(Type type)
        {
            VertexShaderAttribute? attribute = type.GetCustomAttribute<VertexShaderAttribute>();
            if (attribute is null)
            {
                Compiler.LogWarning($"{type} does not have a {nameof(VertexShaderAttribute)}, ignore it");
                return;
            }

            Class instance = this.internalCompileVertexShader(type, attribute);
            mVertexShaderPathMap.Add(type, instance.OutputFilename ?? throw new InvalidOperationException("Compiled Class must have a valid output filename"));
        }

        /// <summary>
        /// Compile vertex shader and save to a local path
        /// </summary>
        /// <param name="type">Type which implement <see cref="IVertexSource"/></param>
        /// <param name="attribute"><see cref="VertexShaderAttribute"/> which specified the path to save compiled shader</param>
        /// <returns>Path to save the compiled shader</returns>
        internal abstract Class internalCompileVertexShader(Type type, VertexShaderAttribute attribute);

        private void compileFragmentShader(Type type)
        {
            FragmentShaderAttribute? attribute = type.GetCustomAttribute<FragmentShaderAttribute>();
            if (attribute is null)
            {
                Compiler.LogWarning($"{type} does not have a {nameof(FragmentShaderAttribute)}, ignore it");
                return;
            }

            Class instance = this.internalCompileFragmentShader(type, attribute);
            mFragmentShaderPathMap.Add(type, instance.OutputFilename ?? throw new InvalidOperationException("Compiled Class must have a valid output filename"));
        }

        /// <summary>
        /// Compile vertex shader and save to a local path
        /// </summary>
        /// <param name="type">Type which implement <see cref="IFragmentSource"/></param>
        /// <param name="attribute"><see cref="FragmentShaderAttribute"/> which specified the path to save compiled shader</param>
        /// <returns>Path to save the compiled shader</returns>
        internal abstract Class internalCompileFragmentShader(Type type, FragmentShaderAttribute attribute);

        private void compileGraphicsShader(Type type)
        {
            GraphicsShader? instance = Activator.CreateInstance(type) as GraphicsShader;
            if (instance is null)
            {
                throw new InvalidDataException();
            }

            Type? vertexShaderType = instance.GetMemberType(nameof(GraphicsShader.VertexShader));
            if (vertexShaderType is null)
            {
                throw new InvalidDataException();
            }

            Type? fragmentShaderType = instance.GetMemberType(nameof(GraphicsShader.FragmentShader));
            if (fragmentShaderType is null)
            {
                throw new InvalidDataException();
            }

            this.internalCompileGraphicsShader(type, vertexShaderType, fragmentShaderType);
        }

        internal abstract void internalCompileGraphicsShader(Type shaderType, Type vertexShaderType, Type fragmentShaderType);

        public T? GetDeclaration<T>(string name) where T : Declaration
        {
            return this.Global?.GetDeclaration(name) as T;
        }

        public Declaration? GetDeclaration(string name) 
        {
            return this.Global?.GetDeclaration(name);
        }

        public void PushVariable(TypeSyntax typeSyntax, VariableDeclaratorSyntax syntax)
        {
            this.CurrentVariableCollection.PushVariable(new Variable(this.Global ?? throw new InvalidOperationException("No global namespace"), typeSyntax, syntax));
        }

        internal ArgumentList CreateArgumentList(ArgumentListSyntax syntax)
        {
            return new ArgumentList(this.Global ?? throw new InvalidOperationException("No global namespace"), syntax);
        }

        public bool IsPrimitiveType(Type type)
        {
            if (type.IsPrimitive)
            {
                return true;
            }

            if (typeof(Matrix4) == type)
            {
                return true;
            }

            return false;
        }
    }
}
