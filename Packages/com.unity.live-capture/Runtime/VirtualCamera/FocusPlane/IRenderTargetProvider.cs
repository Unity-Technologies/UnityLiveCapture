namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// An interface that allows to specialize TryGetRenderTarget in IFocusPlaneImpl implementations.
    /// </summary>
    /// <typeparam name="T">Render target type to use.</typeparam>
    interface IRenderTargetProvider<T>
    {
        /// <inheritdoc cref="IFocusPlaneImpl.TryGetRenderTarget{T}"/>
        bool TryGetRenderTarget(out T target);
    }
}
