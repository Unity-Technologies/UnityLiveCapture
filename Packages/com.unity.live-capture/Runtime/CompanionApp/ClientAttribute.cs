using System;

namespace Unity.LiveCapture.CompanionApp
{
    /// <summary>
    /// An attribute placed on <see cref="CompanionAppClient"/> implementations to control which client
    /// is instantiated when the companion app connects to the server.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    class ClientAttribute : Attribute
    {
        /// <summary>
        /// The name used to identify this client type.
        /// </summary>
        /// <remarks>
        /// This should match a <see cref="ClientInitialization.Type"/> received from the companion app.
        /// </remarks>
        public string Type { get; }

        /// <summary>
        /// Creates a new <see cref="ClientAttribute"/> instance.
        /// </summary>
        /// <param name="type">The name used to identify this client type.</param>
        public ClientAttribute(string type)
        {
            Type = type;
        }
    }
}
