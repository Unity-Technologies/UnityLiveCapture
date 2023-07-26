using System;
using UnityEngine;

namespace Unity.LiveCapture.CompanionApp
{
    /// <summary>
    /// The message sent by a client to the server immediately upon connecting.
    /// </summary>
    class ClientInitialization
    {
        /// <inheritdoc cref="ICompanionAppClient.Name"/>
        public string Name;

        /// <inheritdoc cref="ICompanionAppClientInternal.ID"/>
        public string ID;

        /// <summary>
        /// The type of client that has connected to the server.
        /// </summary>
        public string Type;

        /// <inheritdoc cref="ICompanionAppClientInternal.ScreenResolution"/>
        public Vector2Int ScreenResolution;
    }
}
