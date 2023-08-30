using System;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// An enum defining the 3D axes as flags.
    /// </summary>
    [Flags]
    public enum Axis : byte
    {
        /// <summary>
        /// No axis flags set.
        /// </summary>
        None = 0,
        /// <summary>
        /// The X axis flag.
        /// </summary>
        X = (1 << 0),
        /// <summary>
        /// The Y axis flag.
        /// </summary>
        Y = (1 << 1),
        /// <summary>
        /// The Z axis flag.
        /// </summary>
        Z = (1 << 2),
    }

    /// <summary>
    /// An enum defining the translation axes relative to a camera as flags.
    /// </summary>
    [Flags]
    enum PositionAxis : byte
    {
        /// <summary>
        /// No axis.
        /// </summary>
        [Description("No axis.")]
        None = Axis.None,
        /// <summary>
        /// The truck axis, equivalent to the X axis.
        /// </summary>
        [Description("The truck axis, equivalent to the X axis.")]
        Truck = Axis.X,
        /// <summary>
        /// The pedestal axis, equivalent to the Y axis.
        /// </summary>
        [Description("The pedestal axis, equivalent to the Y axis.")]
        Pedestal = Axis.Y,
        /// <summary>
        /// The Dolly Axis, equivalent to the Z Axis.
        /// </summary>
        [Description("The dolly axis, equivalent to the Z axis.")]
        Dolly = Axis.Z,
    }

    /// <summary>
    /// An enum defining the rotation axes relative to a camera as flags.
    /// </summary>
    [Flags]
    enum RotationAxis : byte
    {
        /// <summary>
        /// No axis.
        /// </summary>
        [Description("No axis.")]
        None = Axis.None,
        /// <summary>
        /// The tilt axis, equivalent to the X axis.
        /// </summary>
        [Description("The tilt axis, equivalent to the X axis.")]
        Tilt = Axis.X,
        /// <summary>
        /// The pan axis, equivalent to the Y axis.
        /// </summary>
        [Description("The pan axis, equivalent to the Y axis.")]
        Pan = Axis.Y,
        /// <summary>
        /// The dutch axis, equivalent to the Z axis.
        /// </summary>
        [Description("The dutch axis, equivalent to the Z axis.")]
        Roll = Axis.Z,
    }
}
