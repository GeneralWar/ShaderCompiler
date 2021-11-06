namespace General.Shaders
{
    public class InputVertex
    {
        [LayoutLocation(0)] [Input(InputField.Position)] public Vector3 position { get; }
        [LayoutLocation(1)] [Input(InputField.Color)] public Vector4 color { get; }
        [LayoutLocation(2)] [Input(InputField.Normal)] public Vector3 normal { get; }
        [LayoutLocation(3)] [Input(InputField.UV0)] public Vector2 uv0 { get; }
    }

    public class OutputVertex
    {
        [Input(InputField.Position)] public Vector4 position { get; set; }
        [LayoutLocation(0)] [Output(OutputField.Color)] public Vector4 color { get; set; }
        [LayoutLocation(1)] [Output(OutputField.UV0)] public Vector2 uv0 { get; set; }
    }
}
