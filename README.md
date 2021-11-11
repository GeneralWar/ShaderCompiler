# ShaderCompiler
A shader compiler for shaders which are written in C#

## How to Run

1. Open [ShaderCompiler.sln](ShaderCompiler.sln) with VisualStudio (vs2019 is recommended)
2. Set [Sample\Sample.csproj](Sample\Sample.csproj) as launch project
3. Set **Config** to **Debug**, set **Platform** to **x64**
4. Press *F5*

## Result

Compile C# source code
```C#
[VertexShader("Default/Transparent")]
public class DefaultVertexShader : IVertexSource
{
    void IVertexSource.OnVertex(InputVertex input, UniformData uniforms, OutputVertex output)
    {
        output.position = uniforms.transform.matrix * new Vector4(input.position, 1.0f);
        output.color = input.color;
        output.uv0 = input.uv0;
    }
}
```

***TO***

```glsl
#version 450

layout(binding = 0) uniform Transform
{
	mat4 matrix;
} transform;

layout(binding = 1) uniform sampler2D texture0;

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec4 inColor;
layout(location = 2) in vec3 inNormal;
layout(location = 3) in vec2 inUV0;

out OutputVertex
{
	layout(location = 0) vec4 color;
	layout(location = 1) vec2 uv0;
} outputVertex;

void main()
{
	gl_Position = transform.matrix * vec4(inPosition, 1.0f);
	outputVertex.color = inColor;
	outputVertex.uv0 = inUV0;
}
```

