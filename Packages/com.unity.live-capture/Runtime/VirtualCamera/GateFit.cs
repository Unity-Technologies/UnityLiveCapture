namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// The gate fit modes available for a virtual camera.
    /// </summary>
    public enum GateFit : byte
    {
        /// <summary>
        /// The sensor gate fits completely inside the resolution gate.
        /// </summary>
        [Description("The sensor gate fits completely inside the resolution gate.")]
        Fill = 0,
        /// <summary>
        /// The render frame fits completely inside the resolution gate.
        /// </summary>
        [Description("The render frame fits completely inside the resolution gate.")]
        Overscan = 1,
    }
}
