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

        private const string EXTENSION_PLACE_HOLDER = "{extension_place_holder}";
        private const string STRUCT_CONTENT_PLACE_HOLDER = "{struct_content_place_holder}";
        private const string UNIFORM_CONTENT_PLACE_HOLDER = "{uniform_content_place_holder}";

        public GLSLCompiler() : base(Language.GLSL) { }

        protected override string internalAnalyzeInputVertexMemberName(MemberInfo memberInfo, InputVertexAttribute attribute)
        {
            return $"in{attribute.Field}";
        }

        protected override string internalAnalyzeOutputVertexMemberName(MemberInfo memberInfo, OutputVertexAttribute attribute)
        {
            if (OutputField.TransformedPosition == attribute.Field)
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

        internal override string internalAnalyzeVariableName(Declaration variable)
        {
            Type? type = (variable as ITypeHost)?.Type;
            if (typeof(InputFragment) == type)
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

        internal override Class internalCompileVertexShader(Type type, VertexShaderAttribute attribute)
        {
            Class? instance = this.GetDeclaration<Class>(type.FullName ?? throw new InvalidDataException());
            if (instance is null)
            {
                throw new InvalidDataException();
            }

            instance.SetOutputFilename(Path.GetFullPath(Path.Join(this.OutputDirectory, attribute.Path + ".vert")));
            this.compileGraphicsShaderModule(attribute, instance, nameof(IVertexSource.OnVertex), instance.OutputFilename ?? "");
            return instance;
        }

        internal override Class internalCompileFragmentShader(Type type, FragmentShaderAttribute attribute)
        {
            Class? instance = this.GetDeclaration<Class>(type.FullName ?? throw new InvalidDataException());
            if (instance is null)
            {
                throw new InvalidDataException();
            }

            instance.SetOutputFilename(Path.GetFullPath(Path.Join(this.OutputDirectory, attribute.Path + ".frag")));
            this.compileGraphicsShaderModule(attribute, instance, nameof(IFragmentSource.OnFragment), instance.OutputFilename ?? "");
            return instance;
        }

        internal override void internalCompileGraphicsShader(Type shaderType, Type vertexShaderType, Type fragmentShaderType)
        {
            GraphicsShaderAttribute? shaderAttribute = shaderType.GetCustomAttribute<GraphicsShaderAttribute>();
            if (shaderAttribute is null)
            {
                //throw new InvalidDataException();
                Compiler.LogWarning($"No {nameof(GraphicsShaderAttribute)} for {shaderType}, ignore it");
                return;
            }

            ShaderConfig shader = new ShaderConfig(shaderAttribute.Path, shaderAttribute.Type, shaderAttribute.Queue);
            this.groupComponents(shader, vertexShaderType, fragmentShaderType);
            shader.polygonMode = shaderAttribute.PolygonMode;
            shader.polygonTypes = Array.ConvertAll(shaderType.GetCustomAttributes<PolygonTypeAttribute>().ToArray(), a => a.polygonType);

            string filename = Path.Join(this.OutputDirectory, shaderAttribute.Path + ".shader");
            DataContractJsonSerializer serailizer = new DataContractJsonSerializer(shader.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serailizer.WriteObject(stream, shader);
                File.WriteAllBytes(filename, stream.ToArray());
            }
        }

        private void compileGraphicsShaderModule(ShaderPathAttribute pathAttribute, Class shaderInstance, string methodName, string outputFileName)
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

            Method mainMethod = methods[0];
            GLSLCompileContext context = new GLSLCompileContext(this, shaderInstance);
            bool push = this.PushScope(shaderInstance);

            string headerContent = this.declareHeader(context, shaderInstance, methods[0]);
            string methodContent = this.declareModuleMainMethod(context, shaderInstance, methods[0]);

            // should compile references before constants, referenced methods may need constants
            StringBuilder referenceBuilder = new StringBuilder();
            this.declareReferences(context, shaderInstance.References);
            this.declareReferences(context, mainMethod.References);
            if (context.References.Count() > 0)
            {
                referenceBuilder.AppendLine(string.Join(Environment.NewLine, context.References));
            }

            ComponentData componentData = this.getComponentData(shaderInstance.Type);

            StringBuilder builder = new StringBuilder();
            headerContent = headerContent.Replace(EXTENSION_PLACE_HOLDER, componentData.Extensions.Count > 0 ? (string.Join(Environment.NewLine, componentData.Extensions.Select(e => $"#extension {e} : enable")) + Environment.NewLine) : "");
            headerContent = headerContent.Replace(STRUCT_CONTENT_PLACE_HOLDER, context.Structures.Count() > 0 ? (string.Join(Environment.NewLine, context.Structures) + Environment.NewLine) : "");
            headerContent = headerContent.Replace(UNIFORM_CONTENT_PLACE_HOLDER, context.Uniforms.Count() > 0 ? (string.Join(Environment.NewLine, context.Uniforms.Select(u => u.Content ?? throw new InvalidOperationException("Uniform property must have content"))) + Environment.NewLine) : "");
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

            if (push)
            {
                this.PopScope(shaderInstance);
            }

            File.WriteAllText(outputFileName, content);

            Compiler.Log($"Compile [{pathAttribute.Path}]({shaderInstance.FullName}) to {outputFileName}");
        }

        private string declareHeader(GLSLCompileContext context, Class instance, Method method)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("#version 450");
            if (method is FragmentShaderMethod)
            {
                builder.AppendLine("#extension GL_ARB_separate_shader_objects : enable");
            }
            builder.Append(EXTENSION_PLACE_HOLDER);
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
                if (memberInfo.GetMemberType().GetCustomAttribute<UniformTypeAttribute>() is null && memberInfo.GetCustomAttribute<UniformUsageAttribute>() is null)
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

        private void declareAsUniformData(GLSLCompileContext context, Type componentType, MemberInfo memberInfo, string instanceName, ICustomAttributeProvider attributeProvider)
        {
            Type memberType = memberInfo.GetMemberType();
            string shaderTypeName = memberType.GetShaderTypeName(this.Language);
            if (string.IsNullOrWhiteSpace(shaderTypeName))
            {
                throw new InvalidDataException();
            }

            UniformTypeAttribute? typeAttribute = memberType.GetCustomAttribute<UniformTypeAttribute>() ?? attributeProvider.GetCustomAttributes(typeof(UniformTypeAttribute), false).Select(a => a as UniformTypeAttribute).FirstOrDefault();
            if (typeAttribute is null)
            {
                Compiler.LogWarning($"No {nameof(UniformTypeAttribute)} for member {memberInfo}, ignore it");
                return;
            }

            UniformUsageAttribute? usageAttribute = memberInfo.GetCustomAttribute<UniformUsageAttribute>() ?? attributeProvider.GetCustomAttributes(typeof(UniformUsageAttribute), false).Select(a => a as UniformUsageAttribute).FirstOrDefault();
            if (usageAttribute is null)
            {
                Compiler.LogWarning($"No {nameof(UniformUsageAttribute)} for member {memberInfo}, ignore it");
                return;
            }

            UniformProperty uniform = new UniformProperty(typeAttribute, usageAttribute, componentType, memberType, instanceName);
            if (context.ContainsUniform(uniform.UniformName))
            {
                return;
            }

            uniform.SetContent(this.CompileUniformContent(context, uniform, attributeProvider));
            this.CurrentComponentData?.AddUniformProperty(uniform);
            context.AddUniform(uniform);
        }

        public string CompileUniformContent(GLSLCompileContext context, UniformProperty uniform, ICustomAttributeProvider attributeProvider)
        {
            Type sourceType = uniform.SourceType;
            string shaderTypeName = sourceType.GetShaderTypeName(this.Language);
            string uniformPrefix = $"layout(binding = {uniform.PlaceHolder}) uniform";
            if (sourceType.IsArray)
            {
                ArraySizeAttribute? arraySizeAttribute = attributeProvider.GetCustomAttributes(typeof(ArraySizeAttribute), false).Select(a => a as ArraySizeAttribute).FirstOrDefault();
                if (arraySizeAttribute is null)
                {
                    throw new InvalidDataException();
                }

                Type elementType = sourceType.GetElementType() ?? throw new InvalidDataException();
                this.declareAsStruct(context, elementType, shaderTypeName);

                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"{uniformPrefix} {uniform.UniformName}");
                builder.AppendLine("{");
                builder.AppendLine(1, $"{shaderTypeName} {uniform.InstanceName}[{arraySizeAttribute.ElementCount}];");
                builder.AppendLine("};");
                return builder.ToString();
            }
            else
            {
                if (sourceType.GetMembers().Where(m => m.GetCustomAttribute<UniformFieldAttribute>() is not null).Count() > 0)
                {
                    this.declareAsStruct(context, sourceType, shaderTypeName);

                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"{uniformPrefix} {uniform.UniformName}");
                    builder.AppendLine("{");
                    builder.AppendLine(1, $"{shaderTypeName} {uniform.InstanceName};");
                    builder.AppendLine("};");
                    return builder.ToString();
                }
                else if (typeof(Sampler2D) != sourceType)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"{uniformPrefix} Uniform{uniform.UniformName}");
                    builder.AppendLine("{");
                    builder.AppendLine(1, $"{shaderTypeName} {uniform.InstanceName};");
                    builder.AppendLine("};");
                    return builder.ToString();
                }
                else
                {
                    return $"{uniformPrefix} {shaderTypeName} {uniform.InstanceName};" + Environment.NewLine;
                }
            }
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
                foreach (MemberInfo member in members)
                {
                    builder.Append("\t");
                    builder.AppendLine(this.declareUniformMember(context, member));
                }
                builder.Append("}");
            }
        }

        private void declareAsStruct(GLSLCompileContext context, Type type, string shaderTypeName)
        {
            if (context.ContainsStruct(shaderTypeName))
            {
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append($"struct {shaderTypeName}");
            this.declareUniformDataMembers(context, builder, type);
            builder.AppendLine(";");
            context.AddStructure(shaderTypeName, builder.ToString());
        }

        public string declareUniformMember(GLSLCompileContext context, MemberInfo memberInfo)
        {
            Type type = memberInfo.GetMemberType();
            UniformTypeAttribute? attribute = type.GetCustomAttribute<UniformTypeAttribute>();
            if (attribute is not null && UniformType.Custom == attribute.Type)
            {
                this.declareAsStruct(context, type, type.Name);
            }
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
                if (inputAttribute is null)
                {
                    throw new InvalidDataException();
                }

                members.Add(members.Count, $"layout(location = {members.Count}) in {memberInfo.GetMemberType().GetShaderTypeName(this.Language)} {this.AnalyzeMemberName(memberInfo)};");
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

                if (OutputField.TransformedPosition != outputAttribute.Field)
                {
                    members.Add(members.Count, $"\tlayout(location = {members.Count}) {memberInfo.GetMemberType().GetShaderTypeName(this.Language)} {this.analyzeOutputVertexMemberShortName(memberInfo, memberInfo.GetCustomAttribute<OutputVertexAttribute>() ?? throw new InvalidDataException())};");
                }
            }
            builder.AppendJoin(Environment.NewLine, members.Values);
            builder.AppendLine();
            builder.AppendLine($"}} {OUTPUT_VERTEX_NAME};");
            return builder.ToString();
        }

        private string declareModuleMainMethod(GLSLCompileContext context, Class instance, Method method)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("void main()");
            builder.AppendLine("{");
            builder.AppendLine(method.CompileMethodBody(context));
            builder.AppendLine("}");
            return builder.ToString();
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

                members.Add(members.Count, $"\tlayout(location = {members.Count}) {memberInfo.GetMemberType().GetShaderTypeName(this.Language)} {this.analyzeInputFragmentMemberShortName(memberInfo, memberInfo.GetCustomAttribute<InputFragmentAttribute>() ?? throw new InvalidDataException())};");
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
                if (outputAttribute is null)
                {
                    throw new InvalidDataException();
                }

                members.Add(members.Count, $"layout(location = {members.Count}) out {memberInfo.GetMemberType().GetShaderTypeName(this.Language)} {this.AnalyzeMemberName(memberInfo)};");
            }
            return string.Join(Environment.NewLine, members.Values) + Environment.NewLine;
        }

        private void groupComponents(ShaderConfig shader, Type vertexShaderType, Type fragmentShaderType)
        {
            List<UniformProperty> uniforms = new List<UniformProperty>();
            string safeFileName = shader.key.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');
            shader.vertexShader = this.createComponent(safeFileName, this.FindVertexShaderPath(vertexShaderType), vertexShaderType, uniforms);
            shader.fragmentShader = this.createComponent(safeFileName, this.FindFragmentShaderPath(fragmentShaderType), fragmentShaderType, uniforms);
            shader.uniforms = uniforms.Select(u => u.ToDeclaration()).ToArray();
        }

        /// <summary>
        /// replace uniform place holder with certain uniform content
        /// </summary>
        private string groupUniform(string content, Type componentType, List<UniformProperty> shaderUniforms)
        {
            ComponentData data = this.getComponentData(componentType);
            HashSet<UniformProperty> uniforms = new HashSet<UniformProperty>(data.Uniforms);
            Class declaration = this.GetDeclaration<Class>(componentType.FullName ?? throw new InvalidDataException()) ?? throw new InvalidOperationException();
            foreach (Declaration reference in Compiler.GetAllReferences(declaration))
            {
                Class? referenceClass = reference as Class;
                if (referenceClass is not null)
                {
                    ComponentData referenceData = this.getComponentData(referenceClass.Type);
                    uniforms.AddRange(referenceData.Uniforms);
                    continue;
                }
            }
            foreach (UniformProperty uniform in uniforms)
            {
                content = content.Replace(uniform.PlaceHolder, shaderUniforms.Count.ToString());
                shaderUniforms.Add(uniform);
            }
            return content;
        }

        private string createComponent(string safeFileName, string templatePath, Type shaderType, List<UniformProperty> shaderUniforms)
        {
            string content = this.groupUniform(File.ReadAllText(templatePath), shaderType, shaderUniforms);
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
                    context.AddReference(method.MethodName, method, this.declareFunction(context, method));
                }
            }
        }

        private string declareFunction(GLSLCompileContext context, Method method)
        {
            StringBuilder builder = new StringBuilder();
            TypeSyntax returnTypeSyntax = method.ReturnType;
            Type returnType = this.GetType(returnTypeSyntax.GetName()) ?? throw new InvalidDataException();

            bool push = this.PushScope(method.DeclaringClass);

            string bodyContent = method.CompileMethodBody(context);
            if (method.References.Count > 0)
            {
                this.declareReferences(context, method.References);
            }

            if (push)
            {
                this.PopScope(method.DeclaringClass);
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
                    Type currentComponent = this.CurrentComponentData?.Type ?? throw new InvalidOperationException("There must be more than one class");
                    string parameterName = parameterInfo.Name ?? throw new InvalidDataException("Parameters must have name");
                    this.declareAsUniformData(context, currentComponent, parameterInfo.ParameterType, parameterName, parameterInfo);
                    continue;
                }

                parameters.Add($"{parameterInfo.ParameterType.GetShaderTypeName(this.Language)} {parameterInfo.Name}");
            }

            return string.Join(", ", parameters);
        }

        private void declarePushConstant(GLSLCompileContext context, ParameterInfo parameterInfo)
        {
            Type type = parameterInfo.ParameterType;
            if (this.IsPrimitiveType(type))
            {
                string name = parameterInfo.Name ?? throw new InvalidDataException("Constant must have name");
                string constantName = $"Constant{char.ToUpper(name[0]) + name.Substring(1)}";
                context.AddPushConstant(constantName, this.declareComplexPushConstant(context, type, constantName, $"{type.GetShaderTypeName(this.Language)} {parameterInfo.Name};"));
            }
            else if (type.IsArray)
            {
                ArraySizeAttribute? arraySizeAttribute = parameterInfo.GetCustomAttribute<ArraySizeAttribute>();
                if (arraySizeAttribute is null)
                {
                    throw new InvalidDataException();
                }

                Type elementType = type.GetElementType() ?? throw new InvalidDataException();
                string constantName = $"Constant{elementType.Name}s";
                context.AddPushConstant(constantName, this.declareComplexPushConstant(context, elementType, constantName, $"{elementType.GetShaderTypeName(this.Language)} {parameterInfo.Name}[{arraySizeAttribute.ElementCount}];"));
            }
            else
            {
                string constantName = $"Constant{type.Name}";
                context.AddPushConstant(constantName, this.declareComplexPushConstant(context, type, constantName, $"{type.GetShaderTypeName(this.Language)} {parameterInfo.Name};"));
            }
        }

        private string declareComplexPushConstant(GLSLCompileContext context, Type constantType, string constantName, string constantContent)
        {
            StringBuilder builder = new StringBuilder();
            if (!this.IsPrimitiveType(constantType))
            {
                builder.AppendLine(this.declarePushConstantStruct(context, constantType));
                builder.AppendLine();
            }
            builder.AppendLine($"layout(push_constant) uniform {constantName}");
            builder.AppendLine("{");
            builder.AppendLine(1, constantContent);
            builder.AppendLine("};");
            return builder.ToString().TrimEnd();
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
