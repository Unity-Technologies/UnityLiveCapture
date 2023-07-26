using System.Text;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// Asset that stores all the parameters needed to model and configure a physical camera lens.
    /// </summary>
    public class LensAsset : ScriptableObject
    {
        [SerializeField]
        string m_Manufacturer;
        [SerializeField]
        string m_Model;
        [SerializeField]
        Lens m_DefaultValues = Lens.DefaultParams;
        [SerializeField]
        LensIntrinsics m_Intrinsics = LensIntrinsics.DefaultParams;

        /// <summary>
        /// The manufacturer of the lens.
        /// </summary>
        public string Manufacturer
        {
            get => m_Manufacturer;
            set => m_Manufacturer = value;
        }

        /// <summary>
        /// The model of lens.
        /// </summary>
        public string Model
        {
            get => m_Model;
            set => m_Model = value;
        }

        /// <summary>
        /// The default <see cref="Lens"/> parameters of the current lens asset.
        /// </summary>
        public Lens DefaultValues
        {
            get => m_DefaultValues;
            set => m_DefaultValues = value;
        }

        /// <summary>
        /// The <see cref="LensIntrinsics"/> parameters of the current lens asset.
        /// </summary>
        public LensIntrinsics Intrinsics
        {
            get => m_Intrinsics;
            set => m_Intrinsics = value;
        }

        void Reset()
        {
            m_DefaultValues = Lens.DefaultParams;
            m_Intrinsics = LensIntrinsics.DefaultParams;
        }

        void OnValidate()
        {
            m_Intrinsics.Validate();
            m_DefaultValues.Validate(m_Intrinsics);
            name = GenerateName(this);
        }

        internal static string GenerateName(LensAsset lensAsset)
        {
            var builder = new StringBuilder(100);
            var manufacturer = lensAsset.Manufacturer;
            var model = lensAsset.Model;
            var intrinsics = lensAsset.Intrinsics;

            builder.Clear();

            if (!string.IsNullOrEmpty(manufacturer) && !string.IsNullOrWhiteSpace(manufacturer))
            {
                builder.Append(manufacturer);
                builder.Append(" ");
            }

            if (!string.IsNullOrEmpty(model) && !string.IsNullOrWhiteSpace(model))
            {
                builder.Append(model);
                builder.Append(" ");
            }

            var zeroZoomRange = intrinsics.FocalLengthRange.x == intrinsics.FocalLengthRange.y;

            if (zeroZoomRange)
            {
                builder.Append("Prime");
                builder.Append(" ");
                builder.Append(Mathf.FloorToInt(intrinsics.FocalLengthRange.x));
            }
            else
            {
                builder.Append("Zoom");
                builder.Append(" ");
                builder.Append(Mathf.FloorToInt(intrinsics.FocalLengthRange.x));
                builder.Append(" - ");
                builder.Append(Mathf.CeilToInt(intrinsics.FocalLengthRange.y));
            }

            builder.Append("mm T");
            builder.Append(intrinsics.ApertureRange.x);

            var zeroFStopRange = intrinsics.ApertureRange.x == intrinsics.ApertureRange.y;

            if (!zeroFStopRange)
            {
                builder.Append(" to T");
                builder.Append(intrinsics.ApertureRange.y);
            }

            return builder.ToString();
        }
    }
}
