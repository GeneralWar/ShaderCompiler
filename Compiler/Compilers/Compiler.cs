// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace General.Shaders
{
    public abstract partial class Compiler
    {
        private Language mLanguage = Language.GLSL;
        public Language Language => mLanguage;

        private string mOutputDirectory;
        public string OutputDirectory => mOutputDirectory;

        private Namespace mGlobal = new Namespace("global");
        internal Namespace Global => mGlobal;

        private Stack<SyntaxNode> mSyntaxStack = new Stack<SyntaxNode>();
        private Stack<List<Variable>> mVariableStack = new Stack<List<Variable>>();
        private Dictionary<SyntaxNode, List<Variable>> mLocalVariableStack = new Dictionary<SyntaxNode, List<Variable>>();

        private Dictionary<Type, string> mVertexShaderPathMap = new Dictionary<Type, string>();
        private Dictionary<Type, string> mFragmentShaderPathMap = new Dictionary<Type, string>();

        public Compiler(Language language)
        {
            mLanguage = language;
            mOutputDirectory = $"Shaders/{language}s";
        }

        private void SetOutputDirectory(string path)
        {
            mOutputDirectory = path;
        }

        private void Initialize(string projectPath)
        {
            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException(projectPath);
            }

            string? directory = Path.GetDirectoryName(projectPath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new DirectoryNotFoundException(projectPath);
            }

            List<string> decocatedFilenames = new List<string>();
            foreach (string filename in Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                this.analyzeFile(filename);
            }
        }

        private void analyzeFile(string filename)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(filename));
            this.analyzeNode(syntaxTree.GetRoot());
        }

        private void analyzeNode(SyntaxNode root)
        {
            foreach (SyntaxNode node in root.ChildNodes())
            {
                NamespaceDeclarationSyntax? namespaceDeclaration = node as NamespaceDeclarationSyntax;
                if (namespaceDeclaration is not null)
                {
                    this.analyzeNamespaceDeclaration(namespaceDeclaration);
                    continue;
                }

                ClassDeclarationSyntax? classDeclarationSyntax = node as ClassDeclarationSyntax;
                if (classDeclarationSyntax is not null)
                {
                    this.analyzeClassDeclaration(classDeclarationSyntax);
                    continue;
                }
            }
        }

        private void analyzeNamespaceDeclaration(NamespaceDeclarationSyntax syntax)
        {
            Namespace? instance = mGlobal.GetNamespace(syntax, true);
            if (instance is null)
            {
                throw new InvalidDataException();
            }
        }

        private void analyzeClassDeclaration(ClassDeclarationSyntax syntax)
        {
            Class? instance = this.registerClass(syntax);
            if (instance is null)
            {
                throw new InvalidDataException();
            }
        }

        private Class? registerClass(ClassDeclarationSyntax syntax)
        {
            return mGlobal.GetClass(syntax, true);
        }

        private void Analyze()
        {
            mGlobal.Analyze(this);
        }

        internal string AnalyzeMemberName(Type type, string name)
        {
            MemberInfo[] members = type.GetMember(name);
            Trace.Assert(1 == members.Length);

            string? result = this.AnalyzeMemberName(members[0]);
            if (string.IsNullOrWhiteSpace(result))
            {
                Debugger.Break();
                throw new InvalidDataException();
            }

            return result;
        }

        internal string? AnalyzeMemberName(MemberInfo memberInfo)
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

            if (memberInfo.DeclaringType == typeof(UniformData))
            {
                return memberInfo.Name;
            }

            return null;
        }

        protected abstract string internalAnalyzeInputVertexMemberName(MemberInfo memberInfo, InputVertexAttribute attribute);

        protected abstract string internalAnalyzeOutputVertexMemberName(MemberInfo memberInfo, OutputVertexAttribute attribute);

        protected abstract string internalAnalyzeInputFragmentMemberName(MemberInfo memberInfo, InputFragmentAttribute attribute);

        protected abstract string internalAnalyzeOutputFragmentMemberName(MemberInfo memberInfo, OutputFragmentAttribute attribute);

        protected abstract string internalAnalyzeUniformMemberName(MemberInfo memberInfo, UniformFieldAttribute attribute);

        public string AnalyzeElementAccess(string variableName, string elementName)
        {
            Variable? variable = this.GetVariable(variableName);
            if (variable is null)
            {
                throw new InvalidDataException();
            }

            string? result = Declaration.AnalyzeElementAccess(this, variable.Type, elementName.Trim());
            if (!string.IsNullOrWhiteSpace(result))
            {
                return $"{variableName}.{result}";
            }

            return this.internalAnalyzeElementAccess(variableName, elementName);
        }

        protected abstract string internalAnalyzeElementAccess(string variableName, string elementName);

        internal void PushVariables(IEnumerable<Variable> variables)
        {
            mVariableStack.Push(variables.ToList());
        }

        internal void PopVariables(IEnumerable<Variable> variables)
        {
            List<Variable> top = mVariableStack.Peek();
            if (top.Count != variables.Count() || top.Intersect(variables).Count() != top.Count)
            {
                throw new InvalidDataException();
            }

            mVariableStack.Pop();
        }

        internal Variable? GetVariable(string name)
        {
            Trace.Assert(!name.Contains('.'));

            foreach (List<Variable> variables in mLocalVariableStack.Values.Reverse())
            {
                Variable? v = variables.FirstOrDefault(v => v.Name == name);
                if (v is not null)
                {
                    return v;
                }
            }

            foreach (List<Variable> variables in mVariableStack.Reverse())
            {
                Variable? variable = variables.FirstOrDefault(v => v.Name == name);
                if (variable is not null)
                {
                    return variable;
                }
            }
            return null;
        }

        internal Variable? GetVariable(ExpressionSyntax syntax)
        {
            MemberAccessExpressionSyntax? memberAccessExpressionSyntax = syntax as MemberAccessExpressionSyntax;
            if (memberAccessExpressionSyntax is not null)
            {
                return this.GetVariable(memberAccessExpressionSyntax.Expression);
            }

            IdentifierNameSyntax? identifierNameSyntax = syntax as IdentifierNameSyntax;
            if (identifierNameSyntax is not null)
            {
                string name = identifierNameSyntax.GetName();
                return this.GetVariable(name);
            }

            Debugger.Break();
            throw new NotImplementedException();
        }

        internal Type GetType(ExpressionSyntax syntax)
        {
            MemberAccessExpressionSyntax? memberAccessExpressionSyntax = syntax as MemberAccessExpressionSyntax;
            if (memberAccessExpressionSyntax is not null)
            {
                return this.GetType(memberAccessExpressionSyntax.Expression);
            }

            IdentifierNameSyntax? identifierNameSyntax = syntax as IdentifierNameSyntax;
            if (identifierNameSyntax is not null)
            {
                string name = identifierNameSyntax.GetName();
                Variable? variable = this.GetVariable(name);
                if (variable is null)
                {
                    Debugger.Break();
                    throw new NotImplementedException();
                }

                return variable.Type;
            }

            Debugger.Break();
            throw new NotImplementedException();
        }

        internal Type GetType(MemberAccessExpressionSyntax syntax)
        {
            Type ownerType = this.GetType(syntax.Expression);
            string memberName = syntax.Name.GetName();
            string memberFullName = syntax.Name.GetFullName();
            if (memberName != memberFullName)
            {
                Debugger.Break();
                throw new NotImplementedException();
            }

            return ownerType.GetMemberType(memberName);
        }

        public void PushSyntax(SyntaxNode syntax)
        {
            mSyntaxStack.Push(syntax);
        }

        public void PopSyntax(SyntaxNode syntax)
        {
            if (mSyntaxStack.Peek() != syntax)
            {
                throw new InvalidDataException();
            }

            mLocalVariableStack.Remove(syntax);
            mSyntaxStack.Pop();
        }

        internal void PushLocalVariable(Variable variable)
        {
            SyntaxNode? currentSyntaxNode;
            if (!mSyntaxStack.TryPeek(out currentSyntaxNode))
            {
                throw new InvalidDataException();
            }

            List<Variable>? variables;
            if (!mLocalVariableStack.TryGetValue(currentSyntaxNode, out variables))
            {
                mLocalVariableStack.Add(currentSyntaxNode, variables = new List<Variable>());
            }

            variables.Add(variable);
        }

        public Type? GetType(string name)
        {
            foreach (SyntaxNode syntax in mSyntaxStack.Reverse())
            {
                Type? type = syntax.GetTypeFromRoot(name);
                if (type is not null)
                {
                    return type;
                }
            }

            return null;
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

        public void Compile(Assembly assembly)
        {
            HashSet<Type> vertexShaders = new HashSet<Type>();
            HashSet<Type> fragmentShaders = new HashSet<Type>();
            HashSet<Type> graphicsShaders = new HashSet<Type>();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.ImplementInterface<IVertexSource>())
                {
                    vertexShaders.Add(type);
                }
                if (type.ImplementInterface<IFragmentSource>())
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
                throw new InvalidDataException();
            }

            string filename = this.internalCompileVertexShader(type, attribute);
            mVertexShaderPathMap.Add(type, filename);
        }

        /// <summary>
        /// Compile vertex shader and save to a local path
        /// </summary>
        /// <param name="type">Type which implement <see cref="IVertexSource"/></param>
        /// <param name="attribute"><see cref="VertexShaderAttribute"/> which specified the path to save compiled shader</param>
        /// <returns>Path to save the compiled shader</returns>
        protected abstract string internalCompileVertexShader(Type type, VertexShaderAttribute attribute);

        private void compileFragmentShader(Type type)
        {
            FragmentShaderAttribute? attribute = type.GetCustomAttribute<FragmentShaderAttribute>();
            if (attribute is null)
            {
                throw new InvalidDataException();
            }

            string filename = this.internalCompileFragmentShader(type, attribute);
            mFragmentShaderPathMap.Add(type, filename);
        }

        /// <summary>
        /// Compile vertex shader and save to a local path
        /// </summary>
        /// <param name="type">Type which implement <see cref="IFragmentSource"/></param>
        /// <param name="attribute"><see cref="FragmentShaderAttribute"/> which specified the path to save compiled shader</param>
        /// <returns>Path to save the compiled shader</returns>
        protected abstract string internalCompileFragmentShader(Type type, FragmentShaderAttribute attribute);

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

            this.compileGraphicsShader(type, vertexShaderType, fragmentShaderType);
        }

        internal abstract void compileGraphicsShader(Type shaderType, Type vertexShaderType, Type fragmentShaderType);
    }
}
