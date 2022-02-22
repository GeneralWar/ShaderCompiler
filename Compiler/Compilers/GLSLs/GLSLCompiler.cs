// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;

namespace General.Shaders
{
    class GLSLCompiler : Compiler
    {
        private const string OUTPUT_VERTEX_NAME = "outputVertex";
        private const string INPUT_FRAGMENT_NAME = "inputFragment";

        public GLSLCompiler() : base(Language.GLSL) { }

        protected override string internalAnalyzeInputVertexMemberName(MemberInfo memberInfo, InputVertexAttribute attribute)
        {
            return $"in{attribute.Field}";
        }

        protected override string internalAnalyzeOutputVertexMemberName(MemberInfo memberInfo, OutputVertexAttribute attribute)
        {
            switch (attribute.Field)
            {
                case OutputField.Position:
                    return "gl_Position";
                case OutputField.Color:
                case OutputField.Normal:
                case OutputField.UV0:
                    return $"{OUTPUT_VERTEX_NAME}.{this.analyzeOutputVertexMemberShortName(attribute)}";
            }

            throw new NotImplementedException();
        }

        protected string analyzeOutputVertexMemberShortName(OutputVertexAttribute attribute)
        {
            switch (attribute.Field)
            {
                case OutputField.Position:
                    throw new InvalidDataException();
                case OutputField.Color:
                    return "color";
                case OutputField.Normal:
                    return "normal";
                case OutputField.UV0:
                    return "uv0";
            }

            throw new NotImplementedException();
        }

        protected override string internalAnalyzeInputFragmentMemberName(MemberInfo memberInfo, InputFragmentAttribute attribute)
        {
            return $"{INPUT_FRAGMENT_NAME}.{this.analyzeInputFragmentMemberShortName(attribute)}";
        }

        protected string analyzeInputFragmentMemberShortName(InputFragmentAttribute attribute)
        {
            switch (attribute.Field)
            {
                case InputField.Color:
                    return "color";
                case InputField.Normal:
                    return "normal";
                case InputField.UV0:
                    return "uv0";
            }

            throw new NotImplementedException();
        }

        protected override string internalAnalyzeOutputFragmentMemberName(MemberInfo memberInfo, OutputFragmentAttribute attribute)
        {
            return $"out{attribute.Field}";
        }

        protected override string internalAnalyzeUniformMemberName(MemberInfo memberInfo, UniformFieldAttribute attribute)
        {
            string instanceName = attribute.DeclaringType.GetShaderInstanceName(this.Language);
            if (string.IsNullOrWhiteSpace(instanceName))
            {
                throw new InvalidDataException();
            }

            return instanceName + "." + memberInfo.Name;
        }

        private string analyzeUniformMemberShortName(MemberInfo memberInfo, UniformFieldAttribute attribute)
        {
            return memberInfo.Name;
        }

        protected override string internalAnalyzeElementAccess(string variableName, string elementName)
        {
            throw new NotImplementedException();
        }

        protected override string internalCompileVertexShader(Type type, VertexShaderAttribute attribute)
        {
            Class? instance = this.Global.GetDeclaration(type.FullName ?? throw new InvalidDataException()) as Class;
            if (instance is null)
            {
                throw new InvalidDataException();
            }

            return this.compileVertexShader(type, attribute, instance);
        }

        private string compileVertexShader(Type shaderType, VertexShaderAttribute shaderAttribute, Class shaderInstance)
        {
            string filename = Path.Join(this.OutputDirectory, shaderAttribute.Path + ".vert");
            string? directory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Method? onVertexMethod = shaderInstance.GetDeclaration(nameof(IVertexSource.OnVertex)) as Method;
            if (onVertexMethod is null)
            {
                throw new InvalidDataException();
            }

            GLSLCompileContext context = new GLSLCompileContext();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(this.declareHeader(context, shaderInstance, onVertexMethod).TrimEnd());
            builder.AppendLine();
            builder.AppendLine(this.declareVertexShader(context, shaderInstance).TrimEnd());
            string content = builder.ToString();
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidDataException();
            }

            File.WriteAllText(filename, content);

            string message = $"Compile shader [{shaderAttribute.Path}] to {filename}";
            Console.WriteLine(message);
            Trace.WriteLine(message);

            return Path.GetFullPath(filename);
        }

        private string declareHeader(GLSLCompileContext context, Class instance, Method method)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("#version 450");
            if (method is FragmentShaderMethod)
            {
                builder.AppendLine("#extension GL_ARB_separate_shader_objects : enable");
            }
            builder.AppendLine();

            foreach (Variable parameter in method.ParameterList.Parametes)
            {
                this.declareType(context, parameter.Type);
            }
            builder.AppendLine(context.UniformDeclaration);
            builder.AppendLine(context.InputDeclaration);
            builder.AppendLine(context.OutputDeclaration);
            return builder.ToString();
        }

        private void declareType(GLSLCompileContext context, Type type)
        {
            if (typeof(InputVertex) == type)
            {
                context.SetInputDeclaration(this.declareInputVertex(context, type));
                return;
            }
            if (typeof(OutputVertex) == type)
            {
                context.SetOutputDeclaration(this.declareOutputVertex(context, type));
                return;
            }
            if (typeof(InputFragment) == type)
            {
                context.SetInputDeclaration(this.declareInputFragment(context, type));
                return;
            }
            if (typeof(OutputFragment) == type)
            {
                context.SetOutputDeclaration(this.declareOutputFragment(context, type));
                return;
            }
            if (typeof(UniformData) == type)
            {
                context.SetUniformDeclaration(this.declareUniformData(context, type));
                return;
            }
            throw new NotImplementedException();
        }

        private string declareInputVertex(GLSLCompileContext context, Type type)
        {
            Trace.Assert(typeof(InputVertex) == type);

            SortedList<int, string> members = new SortedList<int, string>();
            foreach (MemberInfo memberInfo in type.GetMembers().Where(m => m is FieldInfo || m is PropertyInfo))
            {
                InputVertexAttribute? inputAttribute = memberInfo.GetCustomAttribute<InputVertexAttribute>();
                LayoutLocationAttribute? locationAttribute = memberInfo.GetCustomAttribute<LayoutLocationAttribute>();
                if (inputAttribute is null || locationAttribute is null)
                {
                    throw new InvalidDataException();
                }

                members.Add(locationAttribute.Index, $"layout(location = {locationAttribute.Index}) in {memberInfo.GetMemberType().GetShaderTypeName(this.Language)} {this.AnalyzeMemberName(memberInfo)};");
            }
            return string.Join(Environment.NewLine, members.Values) + Environment.NewLine;
        }

        public string declareOutputVertex(GLSLCompileContext context, Type type)
        {
            Trace.Assert(typeof(OutputVertex) == type);

            SortedList<int, string> members = new SortedList<int, string>();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"out {nameof(OutputVertex)}");
            builder.AppendLine("{");
            foreach (MemberInfo memberInfo in type.GetMembers().Where(m => m is FieldInfo || m is PropertyInfo))
            {
                OutputVertexAttribute? outputAttribute = memberInfo.GetCustomAttribute<OutputVertexAttribute>();
                if (outputAttribute is null)
                {
                    throw new InvalidDataException();
                }

                LayoutLocationAttribute? locationAttribute = memberInfo.GetCustomAttribute<LayoutLocationAttribute>();
                if (locationAttribute is null)
                {
                    continue;
                }

                members.Add(locationAttribute.Index, $"\tlayout(location = {locationAttribute.Index}) {memberInfo.GetMemberType().GetShaderTypeName(this.Language)} {this.analyzeOutputVertexMemberShortName(memberInfo.GetCustomAttribute<OutputVertexAttribute>() ?? throw new InvalidDataException())};");
            }
            builder.AppendJoin(Environment.NewLine, members.Values);
            builder.AppendLine();
            builder.AppendLine($"}} {OUTPUT_VERTEX_NAME};");
            return builder.ToString();
        }

        public string declareUniformData(GLSLCompileContext context, Type type)
        {
            Trace.Assert(typeof(UniformData) == type);

            SortedList<int, string> uniforms = new SortedList<int, string>();
            foreach (MemberInfo memberInfo in type.GetMembers().Where(m => m is FieldInfo || m is PropertyInfo))
            {
                Type memberType = memberInfo.GetMemberType();
                string shaderInstanceName = memberInfo.Name;
                LayoutBindingAttribute? bindingAttribute = memberInfo.GetCustomAttribute<LayoutBindingAttribute>();

                string shaderTypeName = memberType.GetShaderTypeName(this.Language);
                //InstanceNameAttribute? instanceNameAttribute = memberType.GetCustomAttribute<InstanceNameAttribute>();
                if (string.IsNullOrWhiteSpace(shaderTypeName) || string.IsNullOrWhiteSpace(shaderInstanceName) || bindingAttribute is null/* || instanceNameAttribute is null*/)
                {
                    throw new InvalidDataException();
                }

                StringBuilder builder = new StringBuilder();
                builder.Append($"layout(binding = {bindingAttribute.Index}) uniform {shaderTypeName}");
                MemberInfo[] dataMembers = memberType.GetMembers().Where(m => m is FieldInfo || m is PropertyInfo).ToArray();
                if (dataMembers.Length > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine("{");
                    foreach (MemberInfo dataMemberInfo in dataMembers)
                    {
                        builder.Append("\t");
                        builder.AppendLine(this.declareUniformMember(context, dataMemberInfo));
                    }
                    builder.Append("}");
                }
                builder.AppendLine($" {shaderInstanceName};");
                uniforms.Add(bindingAttribute.Index, builder.ToString());
            }
            return string.Join(Environment.NewLine, uniforms.Values);
        }

        public string declareUniformMember(GLSLCompileContext context, MemberInfo memberInfo)
        {
            Type type = memberInfo.GetMemberType();
            return $"{type.GetShaderTypeName(this.Language)} {memberInfo.Name};";
        }

        private string declareVertexShader(GLSLCompileContext context, Class instance)
        {
            Method? onVertexMethod = instance.GetDeclaration(nameof(IVertexSource.OnVertex)) as Method;
            if (onVertexMethod is null)
            {
                throw new InvalidDataException();
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("void main()");
            builder.AppendLine("{");
            builder.AppendLine(onVertexMethod.Content.TrimEnd());
            builder.AppendLine("}");
            return builder.ToString();
        }

        protected override string internalCompileFragmentShader(Type type, FragmentShaderAttribute attribute)
        {
            Class? instance = this.Global.GetDeclaration(type.FullName ?? throw new InvalidDataException()) as Class;
            if (instance is null)
            {
                throw new InvalidDataException();
            }

            return this.compileFragmentShader(type, attribute, instance);
        }

        private string compileFragmentShader(Type shaderType, FragmentShaderAttribute shaderAttribute, Class shaderInstance)
        {
            string filename = Path.Join(this.OutputDirectory, shaderAttribute.Path + ".frag");
            string? directory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Method? onFragmentMethod = shaderInstance.GetDeclaration(nameof(IFragmentSource.OnFragment)) as Method;
            if (onFragmentMethod is null)
            {
                throw new InvalidDataException();
            }

            GLSLCompileContext context = new GLSLCompileContext();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(this.declareHeader(context, shaderInstance, onFragmentMethod).TrimEnd());
            builder.AppendLine();
            builder.AppendLine(this.declareFragmentShader(context, shaderInstance).TrimEnd());
            string content = builder.ToString();
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidDataException();
            }

            File.WriteAllText(filename, content);

            string message = $"Compile shader [{shaderAttribute.Path}] to {filename}";
            Console.WriteLine(message);
            Trace.WriteLine(message);

            return Path.GetFullPath(filename);
        }

        private string declareInputFragment(GLSLCompileContext context, Type type)
        {
            Trace.Assert(typeof(InputFragment) == type);

            SortedList<int, string> members = new SortedList<int, string>();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"in {nameof(InputFragment)}");
            builder.AppendLine("{");
            foreach (MemberInfo memberInfo in type.GetMembers().Where(m => m is FieldInfo || m is PropertyInfo))
            {
                InputFragmentAttribute? inputAttribute = memberInfo.GetCustomAttribute<InputFragmentAttribute>();
                if (inputAttribute is null)
                {
                    throw new InvalidDataException();
                }

                LayoutLocationAttribute? locationAttribute = memberInfo.GetCustomAttribute<LayoutLocationAttribute>();
                if (locationAttribute is null)
                {
                    continue;
                }

                members.Add(locationAttribute.Index, $"\tlayout(location = {locationAttribute.Index}) {memberInfo.GetMemberType().GetShaderTypeName(this.Language)} {this.analyzeInputFragmentMemberShortName(memberInfo.GetCustomAttribute<InputFragmentAttribute>() ?? throw new InvalidDataException())};");
            }
            builder.AppendJoin(Environment.NewLine, members.Values);
            builder.AppendLine();
            builder.AppendLine($"}} {INPUT_FRAGMENT_NAME};");
            return builder.ToString();
        }

        public string declareOutputFragment(GLSLCompileContext context, Type type)
        {
            Trace.Assert(typeof(OutputFragment) == type);

            SortedList<int, string> members = new SortedList<int, string>();
            foreach (MemberInfo memberInfo in type.GetMembers().Where(m => m is FieldInfo || m is PropertyInfo))
            {
                OutputFragmentAttribute? outputAttribute = memberInfo.GetCustomAttribute<OutputFragmentAttribute>();
                LayoutLocationAttribute? locationAttribute = memberInfo.GetCustomAttribute<LayoutLocationAttribute>();
                if (outputAttribute is null || locationAttribute is null)
                {
                    throw new InvalidDataException();
                }

                members.Add(locationAttribute.Index, $"layout(location = {locationAttribute.Index}) out {memberInfo.GetMemberType().GetShaderTypeName(this.Language)} {this.AnalyzeMemberName(memberInfo)};");
            }
            return string.Join(Environment.NewLine, members.Values) + Environment.NewLine;
        }

        private string declareFragmentShader(GLSLCompileContext context, Class instance)
        {
            Method? onFragmentMethod = instance.GetDeclaration(nameof(IFragmentSource.OnFragment)) as Method;
            if (onFragmentMethod is null)
            {
                throw new InvalidDataException();
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("void main()");
            builder.AppendLine("{");
            builder.AppendLine(onFragmentMethod.Content.TrimEnd());
            builder.AppendLine("}");
            return builder.ToString();
        }

        internal override void compileGraphicsShader(Type shaderType, Type vertexShaderType, Type fragmentShaderType)
        {
            GraphicsShaderAttribute? shaderAttribute = shaderType.GetCustomAttribute<GraphicsShaderAttribute>();
            if (shaderAttribute is null)
            {
                throw new InvalidDataException();
            }

            ShaderConfig shader = new ShaderConfig(shaderAttribute.Path, shaderAttribute.Type, shaderAttribute.Queue);
            shader.vertexShader = this.FindVertexShaderPath(vertexShaderType);
            shader.fragmentShader = this.FindFragmentShaderPath(fragmentShaderType);
            shader.polygonTypes = Array.ConvertAll(shaderType.GetCustomAttributes<PolygonTypeAttribute>().ToArray(), a => a.polygonType);

            string filename = Path.Join(this.OutputDirectory, shaderAttribute.Path + ".shader");
            DataContractJsonSerializer serailizer = new DataContractJsonSerializer(shader.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serailizer.WriteObject(stream, shader);
                File.WriteAllBytes(filename, stream.ToArray());
            }
        }
    }
}
