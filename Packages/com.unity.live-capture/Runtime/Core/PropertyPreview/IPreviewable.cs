namespace Unity.LiveCapture
{
    /// <summary>
    /// Implement this interface in a Component to register properties for previewing purposes,
    /// preventing Unity from marking Prefabs or the Scene as modified.
    /// </summary>
    public interface IPreviewable
    {
        /// <summary>
        /// The preview system calls this method before playing animations.
        /// Use the specified <see cref="IPropertyPreviewer"/> to register animated properties.
        /// </summary>
        /// <param name="previewer">The <see cref="IPropertyPreviewer"/> to register animated properties to.</param>
        void Register(IPropertyPreviewer previewer);
    }
}
