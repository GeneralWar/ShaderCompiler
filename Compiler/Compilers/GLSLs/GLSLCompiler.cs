// Author: 朱嘉灵(General)
// Email: generalwar@outlook.com
// Copyright (C) General. Licensed under LGPL-2.1.

using General.Shaders.Uniforms;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    partial class GLSLCompiler : Compiler
    {
        private const string OUTPUT_VERTEX_NAME = "outputVertex";
        private const string INPUT_FRAGMENT_NAME = "inputFragment";

        private const string STRUCT_CONTENT_PLACE_HOLDER = "{struct_content_place_holder}";
        private const string UNIFORM_CONTENT_PLACE_HOLDER = "{uniform_content_place_holder}";

        public GLSLCompiler() : base(Language.GLSL) { }

        protected override string internalAnalyzeInputVertexMemberName(MemberInfo memberInfo, InputVertexAttribute attribute)
        {
            return $"in{attribute.Field}";
        }

        protected override string internalAnalyzeOutputVertexMemberName(MemberInfo memberInfo, OutputVertexAttribute attribute)
        {
            if (OutputField.Position == attribute.Field && memberInfo.GetCustomAttribute<LayoutLocationAttribute>() is null)
            {
                return "gl_Position";
            }

            return $"{OUTPUT_VERTEX_NAME}.{this.analyzeOutputVertexMemberShortName(memberInfo, attribute)}";
        }

        protected string analyzeOutputVertexMemberShortName(MemberInfo memberInfo, OutputVertexAttribute attribute)
        {
            return memberInfo.Name;
        }

        protected override string internalAnalyzeInputFragmentMemberName(MemberInfo memberInfo, InputFragmentAttribute attribute)
        {
            return $"{INPUT_FRAGMENT_NAME}.{this.analyzeInputFragmentMemberShortName(memberInfo, attribute)}";
        }

        protected string analyzeInputFragmentMemberShortName(MemberInfo memberInfo, InputFragmentAttribute attribute)
        {
            return memberInfo.Name;
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

        protected override string internalAnalyzeVariableName(Variable variable)
        {
            if (typeof(InputFragment) == variable.Type)
            {
                return INPUT_FRAGMENT_NAME;
            }

            return variable.Name;
        }

        protected override string internalAnalyzeElementAccess(string variableName, string elementName)
        {
            int integer;
            if (int.TryParse(elementName, out integer))
            {
                return $"{variableName}[{elementName}]";
            }

            throw new NotImplementedException();
        }

        protected override string internalCompileVertexShader(Type type, VertexShaderAttribute attribute)
        {
            Class? instance = this.Global?.GetDeclaration(type.FullName ?? throw new InvalidDataException()) as Class;
            if (instance is null)
            {
                throw new InvalidDataException();
            }

            return this.compileGraphicsShader(attribute, instance, nameof(IVertexSource.OnVertex), Path.Join(this.OutputDirectory, attribute.Path + ".vert"));
        }

        private string compileGraphicsShader(ShaderPathAttribute pathAttribute, Class shaderInstance, string methodName, string outputFileName)
        {
            string? directory = Path.GetDirectoryName(outputFileName);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Method[] methods = shaderInstance.GetMethods(methodName);
            if (1 != methods.Length)
            {
                throw new InvalidDataException();
            }

            GLSLCompileContext context = new GLSLCompileContext();
            this.PushScope(shaderInstance);

            string headerContent = this.declareHeader(context, shaderInstance, methods[0]);
            string methodContent = this.declareShaderMainMethod(context, shaderInstance, methods[0]);

            // should compile references before constants, referenced methods may need constants
            StringBuilder referenceBuilder = new StringBuilder();
            this.declareReferences(context, shaderInstance.References);
            this.declareReferences(context, methods[0].References);
            if (context.References.Count() > 0)
            {
                referenceBuilder.AppendLine(string.Join(Environment.NewLine, context.References));
            }

            StringBuilder builder = new StringBuilder();
            headerContent = headerContent.Replace(STRUCT_CONTENT_PLACE_HOLDER, context.Structures.Count() > 0 ? (string.Join(Environment.NewLine, context.Structures) + Environment.NewLine) : "");
            headerContent = headerContent.Replace(UNIFORM_CONTENT_PLACE_HOLDER, context.Uniforms.Count() > 0 ? (string.Join(Environment.NewLine, context.Uniforms) + Environment.NewLine) : "");
            builder.AppendLine(headerContent);

            string constantContent = string.Join(Environment.NewLine, context.PushConstants);
            if (!string.IsNullOrWhiteSpace(constantContent))
            {
                builder.AppendLine(constantContent);
                builder.AppendLine();
            }

            if (referenceBuilder.Length > 0)
            {
                builder.AppendLine(referenceBuilder.ToString().Trim());
                builder.AppendLine();
            }

            builder.AppendLine(methodContent);

            string content = builder.ToString();
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidDataException();
            }

            this.PopScope(shaderInstance);

            File.WriteAllText(outputFileName, content);

            string message = $"Compile shader [{pathAttribute.Path}] to {outputFileName}";
            Console.WriteLine(message);
            Trace.WriteLine(message);

            return Path.GetFullPath(outputFileName);
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

            builder.Append(STRUCT_CONTENT_PLACE_HOLDER);
            builder.Append(UNIFORM_CONTENT_PLACE_HOLDER);
            this.declareUniforms(context, instance.Type);

            foreach (Variable parameter in method.ParameterList.Parameters)
            {
                this.declareType(context, parameter.Type);
            }
            builder.AppendLine(context.InputDeclaration);
            builder.AppendLine(context.OutputDeclaration);

            return builder.ToString().TrimEnd() + Environment.NewLine;
        }

        public void declareUniforms(GLSLCompileContext context, Type type)
        {
            foreach (MemberInfo memberInfo in type.GetMembers().Where(m => m is FieldInfo || m is PropertyInfo || m is Type))
            {
                if (memberInfo.GetMemberType().GetCustomAttribute<UniformTypeAttribute>() is null && memberInfo.GetCustomAttribute<UniformNameAttribute>() is null)
                {
                    continue;
                }

                //Type? memberType = memberInfo as Type;
                //if (memberType is null)
                //{
                //    this.declareAsUniformData(context, type, memberInfo, memberInfo.Name, memberInfo);
                //}
                //else
                {
                    this.declareAsUniformData(context, type, memberInfo, memberInfo.Name, memberInfo);
                }
            }
        }

        public void declareAsUniformData(GLSLCompileContext context, Type componentType, MemberInfo memberInfo, string instanceName, ICustomAttributeProvider attributeProvider)
        {
            Type memberType = memberInfo.GetMemberType();
            string memberName = memberInfo.Name;
            if (string.IsNullOrWhiteSpace(memberName))
            {
                throw new InvalidDataException();
            }

            string shaderTypeName = memberType.GetShaderTypeName(this.Language);
            if (string.IsNullOrWhiteSpace(shaderTypeName))
            {
                throw new InvalidDataException();
            }

            UniformNameAttribute? nameAttribute = memberInfo.GetCustomAttribute<UniformNameAttribute>();
            string uniformName = $"Uniform{shaderTypeName}";
            if (memberType.IsArray)
            {
                ArraySizeAttribute? arraySizeAttribute = attributeProvider.GetCustomAttributes(typeof(ArraySizeAttribute), false).Select(a => a as ArraySizeAttribute).FirstOrDefault();
                if (arraySizeAttribute is null)
                {
                    throw new InvalidDataException();
                }

                Type elementType = memberType.GetElementType() ?? throw new InvalidDataException();
                this.declareAsStruct(context, elementType, shaderTypeName);

                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"layout(binding = {{binding-{memberName}}}) uniform {uniformName}");
                builder.AppendLine("{");
                builder.AppendLine(1, $"{shaderTypeName} {instanceName}[{arraySizeAttribute.ElementCount}];");
                builder.AppendLine("};");
                context.AddUniform(uniformName, builder.ToString());
            }
            else
            {
                if (memberType.GetMembers().Where(m => m.GetCustomAttribute<UniformFieldAttribute>() is not null).Count() > 0)
                {
                    this.declareAsStruct(context, memberType, shaderTypeName);

                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"layout(binding = {{binding-{memberName}}}) uniform {uniformName}");
                    builder.AppendLine("{");
                    builder.AppendLine(1, $"{shaderTypeName} {instanceName};");
                    builder.AppendLine("};");
                    context.AddUniform(uniformName, builder.ToString());
                }
                else if (typeof(Sampler2D) != memberType)
                {
                    if (nameAttribute is null)
                    {
                        throw new InvalidDataException("GLSL can only declare Sampler2D and Image as simple uniform, others must have block");
                    }

                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"layout(binding = {{binding-{memberName}}}) uniform Uniform{nameAttribute.Name}");
                    builder.AppendLine("{");
                    builder.AppendLine(1, $"{shaderTypeName} {instanceName};");
                    builder.AppendLine("};");
                    context.AddUniform(uniformName, builder.ToString());
                }
                else
                {
                    context.AddUniform(uniformName, $"layout(binding = {{binding-{memberName}}}) uniform {shaderTypeName} {instanceName};" + Environment.NewLine);
                }
            }

            UniformProperty uniform = new UniformProperty(componentType, memberType, memberName);
            if (nameAttribute is not null)
            {
                uniform.SetPublicName(nameAttribute.Name);
            }
            this.getComponentData(componentType).AddUniformProperty(uniform);
        }

        private void declareUniformDataMembers(GLSLCompileContext context, StringBuilder builder, Type uniformType)
        {
            MemberInfo[] members = uniformType.GetMembers().Where(m => m.GetCustomAttribute<UniformFieldAttribute>() is not null).ToArray();
            if (members.Length > 0)
            {
#pragma warning disable CS8602 // 解引用可能出现空引用。
                Array.Sort(members, (m1, m2) => m1.GetCustomAttribute<UniformFieldAttribute>().Index - m2.GetCustomAttribute<UniformFieldAttribute>().Index);
#pragma warning restore CS8602 // 解引用可能出现空引用。
                builder.AppendLine(); // append new line here, confirm no broken line if there is no member
                builder.AppendLine("{");
                foreach (MemberInfo dataMemberInfo in members)
                {
                    builder.Append("\t");
                    builder.AppendLine(this.declareUniformMember(context, dataMemberInfo));
                }
                builder.Append("}");
            }
        }

        private void declareAsStruct(GLSLCompileContext context, Type type, string shaderTypeName)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"struct {shaderTypeName}");
            this.declareUniformDataMembers(context, builder, type);
            builder.AppendLine(";");
            context.AddStruct(shaderTypeName, builder.ToString());
        }

        public string declareUniformMember(GLSLCompileContext context, MemberInfo memberInfo)
        {
            Type type = memberInfo.GetMemberType();
            return $"{type.GetShaderTypeName(this.Language)} {memberInfo.Name};";
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
            throw new NotImplementedException();
        }

        private string declareInputVertex(GLSLCompileContext context, Type type)
        {
            Trace.Assert(typeof(InputVertex) == type);

            StringBuilder builder = new StringBuilder();
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

                members.Add(locationAttribute.Index, $"\tlayout(location = {locationAttribute.Index}) {memberInfo.GetMemberType().GetShaderTypeName(this.Language)} {this.analyzeOutputVertexMemberShortName(memberInfo, memberInfo.GetCustomAttribute<OutputVertexAttribute>() ?? throw new InvalidDataException())};");
            }
            builder.AppendJoin(Environment.NewLine, members.Values);
            builder.AppendLine();
            builder.AppendLine($"}} {OUTPUT_VERTEX_NAME};");
            return builder.ToString();
        }

        private string declareShaderMainMethod(GLSLCompileContext context, Class instance, Method method)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("void main()");
            builder.AppendLine("{");
            builder.AppendLine(method.CompileMethodBody(this));
            builder.AppendLine("}");
            return builder.ToString();
        }

        protected override string internalCompileFragmentShader(Type type, FragmentShaderAttribute attribute)
        {
            Class? instance = this.Global?.GetDeclaration(type.FullName ?? throw new InvalidDataException()) as Class;
            if (instance is null)
            {
                throw new InvalidDataException();
            }

            return this.compileGraphicsShader(attribute, instance, nameof(IFragmentSource.OnFragment), Path.Join(this.OutputDirectory, attribute.Path + ".frag"));
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

                members.Add(locationAttribute.Index, $"\tlayout(location = {locationAttribute.Index}) {memberInfo.GetMemberType().GetShaderTypeName(this.Language)} {this.analyzeInputFragmentMemberShortName(memberInfo, memberInfo.GetCustomAttribute<InputFragmentAttribute>() ?? throw new InvalidDataException())};");
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

        internal override void compileGraphicsShader(Type shaderType, Type vertexShaderType, Type fragmentShaderType)
        {
            GraphicsShaderAttribute? shaderAttribute = shaderType.GetCustomAttribute<GraphicsShaderAttribute>();
            if (shaderAttribute is null)
            {
                throw new InvalidDataException();
            }

            ShaderConfig shader = new ShaderConfig(shaderAttribute.Path, shaderAttribute.Type, shaderAttribute.Queue);
            this.groupComponents(shader, vertexShaderType, fragmentShaderType);
            shader.polygonTypes = Array.ConvertAll(shaderType.GetCustomAttributes<PolygonTypeAttribute>().ToArray(), a => a.polygonType);

            string filename = Path.Join(this.OutputDirectory, shaderAttribute.Path + ".shader");
            DataContractJsonSerializer serailizer = new DataContractJsonSerializer(shader.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serailizer.WriteObject(stream, shader);
                File.WriteAllBytes(filename, stream.ToArray());
            }
        }

        private void groupComponents(ShaderConfig shader, Type vertexShaderType, Type fragmentShaderType)
        {
            List<UniformProperty> uniforms = new List<UniformProperty>();
            uniforms.AddRange(this.getComponentData(vertexShaderType).Uniforms);
            uniforms.AddRange(this.getComponentData(fragmentShaderType).Uniforms);
            uniforms = uniforms.Distinct().ToList();

            string safeFileName = shader.key.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');
            shader.vertexShader = this.createComponent(safeFileName, this.FindVertexShaderPath(vertexShaderType), uniforms);
            shader.fragmentShader = this.createComponent(safeFileName, this.FindFragmentShaderPath(fragmentShaderType), uniforms);
            shader.uniforms = uniforms.Select(u => u.ToDeclaration()).ToArray();
        }

        private string createComponent(string safeFileName, string templatePath, List<UniformProperty> uniforms)
        {
            string content = File.ReadAllText(templatePath);
            for (int i = 0; i < uniforms.Count; ++i)
            {
                UniformProperty uniform = uniforms[i];
                string holder = $"{{binding-{uniform.PropertyName}}}";
                content = content.Replace(holder, i.ToString());
            }

            string directory = Path.Join(Path.GetDirectoryName(templatePath), Path.GetFileNameWithoutExtension(templatePath));
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string filename = Path.Join(directory, $"{safeFileName}{Path.GetExtension(templatePath)}");
            File.WriteAllText(filename, content);
            return filename;
        }

        private void declareReferences(GLSLCompileContext context, IEnumerable<Declaration> references)
        {
            foreach (Declaration r in references)
            {
                Method? method = r as Method;
                if (method is not null)
                {
                    context.AddReference(method.MethodName, this.declareFunction(context, method));
                }
            }
        }

        private string declareFunction(GLSLCompileContext context, Method method)
        {
            StringBuilder builder = new StringBuilder();
            TypeSyntax returnTypeSyntax = method.ReturnType;
            Type returnType = this.GetType(returnTypeSyntax.GetName()) ?? throw new InvalidDataException();

            string bodyContent = method.CompileMethodBody(this);
            if (method.References.Count > 0)
            {
                this.declareReferences(context, method.References);
            }

            builder.AppendLine($"{returnType.GetShaderTypeName(this.Language)} {method.Name}({this.declareFunctionParameters(context, method)})");
            builder.AppendLine("{");
            builder.AppendLine(bodyContent);
            builder.AppendLine("}");
            return builder.ToString();
        }

        private string declareFunctionParameters(GLSLCompileContext context, Method method)
        {
            Type declaringType = method.DeclaringClass.Type;
            IEnumerable<MethodInfo> methodInfos = declaringType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(m => m.Name == method.Name);

            List<string> parameters = new List<string>();
            MethodInfo? methodInfo = methodInfos.FirstOrDefault(m => method.Match(m));
            if (methodInfo is null)
            {
                throw new InvalidDataException();
            }

            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                if (typeof(InputFragment) == parameterInfo.ParameterType)
                {
                    continue;
                }

                LayoutPushConstantAttribute? layoutPushConstantAttribute = parameterInfo.GetCustomAttribute<LayoutPushConstantAttribute>();
                if (layoutPushConstantAttribute is not null)
                {
                    this.declarePushConstant(context, parameterInfo);
                    continue;
                }

                UniformTypeAttribute? uniformTypeAttribute = parameterInfo.GetCustomAttribute<UniformTypeAttribute>();
                if (uniformTypeAttribute is not null)
                {
                    Type currentClass = this.GetCurrent<Class>()?.Type ?? throw new InvalidOperationException("There must be more than one class");
                    string parameterName = parameterInfo.Name ?? throw new InvalidDataException("Parameters must have name");
                    this.declareAsUniformData(context, currentClass, parameterInfo.ParameterType, parameterName, parameterInfo);
                    continue;
                }

                parameters.Add($"{parameterInfo.ParameterType.GetShaderTypeName(this.Language)} {parameterInfo.Name}");
            }

            return string.Join(", ", parameters);
        }

        private void declarePushConstant(GLSLCompileContext context, ParameterInfo parameterInfo)
        {
            string constantName;
            Type type = parameterInfo.ParameterType;
            StringBuilder builder = new StringBuilder();
            if (type.IsArray)
            {
                ArraySizeAttribute? arraySizeAttribute = parameterInfo.GetCustomAttribute<ArraySizeAttribute>();
                if (arraySizeAttribute is null)
                {
                    throw new InvalidDataException();
                }

                Type elementType = type.GetElementType() ?? throw new InvalidDataException();
                constantName = $"Constant{elementType.Name}s";
                builder.AppendLine(this.declarePushConstantStruct(context, elementType));
                builder.AppendLine();
                builder.AppendLine($"layout(push_constant) uniform {constantName}");
                builder.AppendLine("{");
                builder.AppendLine(1, $"{elementType.Name} {parameterInfo.Name}[{arraySizeAttribute.ElementCount}];");
                builder.AppendLine("};");
            }
            else
            {
                constantName = $"Constant{type.Name}";
                builder.AppendLine(this.declarePushConstantStruct(context, type));
                builder.AppendLine();
                builder.AppendLine($"layout(push_constant) uniform {constantName}");
                builder.AppendLine("{");
                builder.AppendLine(1, $"{type.Name} {parameterInfo.Name};");
                builder.AppendLine("};");
            }
            context.AddPushConstant(constantName, builder.ToString().TrimEnd());
        }

        private string declarePushConstantStruct(GLSLCompileContext context, Type type)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"struct {type.Name}");
            this.declareUniformDataMembers(context, builder, type);
            builder.Append($";");
            return builder.ToString();
        }
    }
}
