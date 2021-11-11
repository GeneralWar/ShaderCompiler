namespace General.Shaders
{
    public enum RenderQueue
    {
        /// <summary>
        /// 最早被渲染的物体的队列。
        /// </summary>
        Background = 1000,
        /// <summary>
        /// 不透明物体的渲染队列。大多数物体都应该使用该队列进行渲染，也是默认的渲染队列。
        /// </summary>
        Geometry = 2000,
        /// <summary>
        /// 有透明通道，需要进行AlphaTest的物体的队列，比在Geomerty中更有效。
        /// </summary>
        AlphaTest = 2450,
        /// <summary>
        /// 半透物体的渲染队列。一般是不写深度的物体，AlphaBlend等的在该队列渲染。
        /// </summary>
        Transparent = 3000,
        /// <summary>
        /// 最后被渲染的物体的队列，一般是覆盖效果，比如镜头光晕，屏幕贴片之类的
        /// </summary>
        Overlay = 4000,
    };
}
