using System;

namespace Unity.LiveCapture
{
    /// <summary>
    /// Defines how a curve is attached to an object that it controls.
    /// </summary>
    public struct PropertyBinding : IEquatable<PropertyBinding>
    {
        /// <summary>
        /// The type of component this binding is applied to.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// The path of the GameObject this binding applies to, relative to the root GameObject.
        /// </summary>
        public string RelativePath { get; private set; }

        /// <summary>
        /// The name or path to the property that is animated.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Creates a new <see cref="PropertyBinding"/> instance.
        /// </summary>
        /// <param name="relativePath">The path of the GameObject this binding applies to,
        /// relative to the root GameObject.</param>
        /// <param name="propertyName">The name or path to the property that is animated.</param>
        /// <param name="type">The type of component this binding is applied to.</param>
        public PropertyBinding(string relativePath, string propertyName, Type type)
        {
            RelativePath = relativePath;
            PropertyName = propertyName;
            Type = type;
        }

        /// <summary>
        /// Determines whether the <see cref="PropertyBinding"/> instances are equal.
        /// </summary>
        /// <param name="other">The other <see cref="PropertyBinding"/> to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>

        public bool Equals(PropertyBinding other)
        {
            return Type == other.Type
                && string.Equals(RelativePath, other.RelativePath)
                && string.Equals(PropertyName, other.PropertyName);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="PropertyBinding"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current <see cref="PropertyBinding"/>.</param>
        /// <returns>
        /// True if the specified object is equal to the current <see cref="PropertyBinding"/>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is PropertyBinding other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for the <see cref="PropertyBinding"/>.
        /// </summary>
        /// <returns>
        /// The hash value generated for this <see cref="PropertyBinding"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Type.GetHashCode();

                if (!string.IsNullOrEmpty(RelativePath))
                {
                    hashCode = (hashCode * 397) ^ RelativePath.GetHashCode();
                }

                if (!string.IsNullOrEmpty(PropertyName))
                {
                    hashCode = (hashCode * 397) ^ PropertyName.GetHashCode();
                }

                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the two specified <see cref="PropertyBinding"/> are equal.
        /// </summary>
        /// <param name="a">The first <see cref="PropertyBinding"/>.</param>
        /// <param name="b">The second <see cref="PropertyBinding"/>.</param>
        /// <returns>
        /// True if the specified <see cref="PropertyBinding"/> are equal; otherwise, false.
        /// </returns>
        public static bool operator ==(PropertyBinding a, PropertyBinding b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the two specified <see cref="PropertyBinding"/> are different.
        /// </summary>
        /// <param name="a">The first <see cref="PropertyBinding"/>.</param>
        /// <param name="b">The second <see cref="PropertyBinding"/>.</param>
        /// <returns>
        /// True if the specified <see cref="PropertyBinding"/> are different; otherwise, false.
        /// </returns>
        public static bool operator !=(PropertyBinding a, PropertyBinding b)
        {
            return !(a == b);
        }
    }
}
