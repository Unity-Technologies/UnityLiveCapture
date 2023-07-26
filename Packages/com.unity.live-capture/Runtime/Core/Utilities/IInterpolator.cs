namespace Unity.LiveCapture
{
    /// <summary>
    /// An interface that provides interpolation functionality.
    /// </summary>
    /// <typeparam name="T">The type of data to interpolate.</typeparam>
    public interface IInterpolator<T>
    {
        /// <summary>
        /// Interpolates between <paramref name="a"/> and <paramref name="b"/> by factor <paramref name="t"/>.
        /// </summary>
        /// <remarks><br/>
        /// * When <paramref name="t"/> is 0 returns <paramref name="a"/>.
        /// * When <paramref name="t"/> is 1 returns <paramref name="b"/>.
        /// * When <paramref name="t"/> is 0.5 returns the midpoint of <paramref name="a"/> and <paramref name="b"/>.
        /// </remarks>
        /// <param name="a">The value to interpolate from.</param>
        /// <param name="b">To value to interpolate to.</param>
        /// <param name="t">The interpolation factor.</param>
        /// <returns>The interpolated value.</returns>
        T Interpolate(in T a, in T b, float t);
    }
}
