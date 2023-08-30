using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture.VirtualCamera
{
    class FrameLinesDrawer : IDisposable
    {
        readonly List<Vector3> m_Vertices = new List<Vector3>();
        readonly List<Color> m_Colors = new List<Color>();
        readonly List<int> m_Indices = new List<int>();

        Mesh m_Mesh;
        Material m_Material;
        Color m_CurrentColor;
        float m_CurrentLineWidth = 1;

        public void Initialize()
        {
            m_Material = AdditionalCoreUtils.CreateEngineMaterial("Hidden/LiveCapture/FrameLines");
            m_Mesh = new Mesh();
            m_Mesh.hideFlags = HideFlags.HideAndDontSave;
        }

        public void Dispose()
        {
            AdditionalCoreUtils.DestroyIfNeeded(ref m_Material);
            AdditionalCoreUtils.DestroyIfNeeded(ref m_Mesh);
        }

        public void Clear()
        {
            m_Colors.Clear();
            m_Vertices.Clear();
        }

        public void UpdateGeometry()
        {
            if (m_Indices.Count > m_Vertices.Count)
            {
                m_Indices.RemoveRange(m_Vertices.Count, m_Indices.Count - m_Vertices.Count);
            }
            else
            {
                for (var i = m_Indices.Count; i != m_Vertices.Count; ++i)
                {
                    m_Indices.Add(i);
                }
            }

            m_Mesh.Clear();
            m_Mesh.SetVertices(m_Vertices);
            m_Mesh.SetColors(m_Colors);
            m_Mesh.SetIndices(m_Indices, MeshTopology.Quads, 0);
            m_Mesh.RecalculateBounds();
            m_Mesh.UploadMeshData(false);
        }

        public void Render(CommandBuffer cmd)
        {
            cmd.DrawMesh(m_Mesh, Matrix4x4.identity, m_Material, 0, 0);
        }

        public void SetColor(Color color)
        {
            m_CurrentColor = color;
        }

        public void SetLineWidth(float value)
        {
            m_CurrentLineWidth = value;
        }

        public void DrawCross(Vector2 center, float inner, float outer)
        {
            var halfLineWidth = m_CurrentLineWidth * .5f;

            DrawBox(Rect.MinMaxRect(
                center.x - outer,
                center.y - halfLineWidth,
                center.x - inner,
                center.y + halfLineWidth));

            DrawBox(Rect.MinMaxRect(
                center.x + inner,
                center.y - halfLineWidth,
                center.x + outer,
                center.y + halfLineWidth));

            DrawBox(Rect.MinMaxRect(
                center.x - halfLineWidth,
                center.y - outer,
                center.x + halfLineWidth,
                center.y - inner));

            DrawBox(Rect.MinMaxRect(
                center.x - halfLineWidth,
                center.y + inner,
                center.x + halfLineWidth,
                center.y + outer));
        }

        public void DrawBox(Rect rect)
        {
            // Note that line width expands inwards.

            DrawRect(Rect.MinMaxRect(
                rect.xMin,
                rect.yMin,
                rect.xMin + m_CurrentLineWidth,
                rect.yMax));

            DrawRect(Rect.MinMaxRect(
                rect.xMax - m_CurrentLineWidth,
                rect.yMin,
                rect.xMax,
                rect.yMax));

            DrawRect(Rect.MinMaxRect(
                rect.xMin + m_CurrentLineWidth,
                rect.yMin,
                rect.xMax - m_CurrentLineWidth,
                rect.yMin + m_CurrentLineWidth));

            DrawRect(Rect.MinMaxRect(
                rect.xMin + m_CurrentLineWidth,
                rect.yMax - m_CurrentLineWidth,
                rect.xMax - m_CurrentLineWidth,
                rect.yMax));
        }

        public void DrawCornerBox(Rect rect, Vector2 extent)
        {
            // Vertical segments.

            DrawRect(Rect.MinMaxRect(
                rect.xMin,
                rect.yMin,
                rect.xMin + m_CurrentLineWidth,
                rect.yMin + extent.y));

            DrawRect(Rect.MinMaxRect(
                rect.xMax - m_CurrentLineWidth,
                rect.yMin,
                rect.xMax,
                rect.yMin + extent.y));

            DrawRect(Rect.MinMaxRect(
                rect.xMin,
                rect.yMax - extent.y,
                rect.xMin + m_CurrentLineWidth,
                rect.yMax));

            DrawRect(Rect.MinMaxRect(
                rect.xMax - m_CurrentLineWidth,
                rect.yMax - extent.y,
                rect.xMax,
                rect.yMax));

            // Horizontal segments.

            DrawRect(Rect.MinMaxRect(
                rect.xMin + m_CurrentLineWidth,
                rect.yMin,
                rect.xMin + extent.x,
                rect.yMin + m_CurrentLineWidth));

            DrawRect(Rect.MinMaxRect(
                rect.xMax - extent.x,
                rect.yMin,
                rect.xMax - m_CurrentLineWidth,
                rect.yMin + m_CurrentLineWidth));

            DrawRect(Rect.MinMaxRect(
                rect.xMin + m_CurrentLineWidth,
                rect.yMax - m_CurrentLineWidth,
                rect.xMin + extent.x,
                rect.yMax));

            DrawRect(Rect.MinMaxRect(
                rect.xMax - extent.x,
                rect.yMax - m_CurrentLineWidth,
                rect.xMax - m_CurrentLineWidth,
                rect.yMax));
        }

        public void DrawRect(Rect rect)
        {
            m_Vertices.Add(new Vector3(rect.xMin, rect.yMin, 0));
            m_Vertices.Add(new Vector3(rect.xMin, rect.yMax, 0));
            m_Vertices.Add(new Vector3(rect.xMax, rect.yMax, 0));
            m_Vertices.Add(new Vector3(rect.xMax, rect.yMin, 0));

            for (var i = 0; i != 4; ++i)
            {
                m_Colors.Add(m_CurrentColor);
            }
        }
    }
}
