using System.Collections.Generic;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Defines methods to support the comparison of <see cref="ITakeBinding"/>s for equality.
    /// </summary>
    public class ITakeBindingEqualityComparer : IEqualityComparer<ITakeBinding>
    {
        /// <summary>
        /// The default instance of the comparer.
        /// </summary>
        public static ITakeBindingEqualityComparer Instance { get; } = new ITakeBindingEqualityComparer();

        /// <summary>
        /// Determines whether the specified <see cref="ITakeBinding"/>s are equal.
        /// </summary>
        /// <param name="x">The first <see cref="ITakeBinding"/> to compare.</param>
        /// <param name="y">The second <see cref="ITakeBinding"/> to compare.</param>
        /// <returns>True if the specified <see cref="ITakeBinding"/>s are equal; otherwise, false.</returns>
        public bool Equals(ITakeBinding x, ITakeBinding y)
        {
            return x.PropertyName.Equals(y.PropertyName);
        }

        /// <summary>
        /// Returns a hash code for the specified <see cref="ITakeBinding"/>.
        /// </summary>
        /// <param name="obj">The <see cref="ITakeBinding"/> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified <see cref="ITakeBinding"/>.</returns>
        public int GetHashCode(ITakeBinding obj)
        {
            unchecked
            {
                var hashCode = obj.PropertyName.GetHashCode();
                return hashCode;
            }
        }
    }
}
