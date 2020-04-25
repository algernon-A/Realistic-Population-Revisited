﻿using UnityEngine;
using ColossalFramework;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Render a 3d image of a given mesh.
    /// </summary>
    public class UIPreviewRenderer : MonoBehaviour
    {
        public Material material;

        private Camera renderCamera;
        private Mesh currentMesh;
        private Bounds currentBounds;
        private float currentRotation = 120f;
        private float currentZoom = 4f;


        /// <summary>
        /// Initialise the new renderer object.
        /// </summary>
        public UIPreviewRenderer()
        {
            // Set up camera.
            renderCamera = new GameObject("Camera").AddComponent<Camera>();
            renderCamera.transform.SetParent(transform);
            renderCamera.targetTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            renderCamera.pixelRect = new Rect(0f, 0f, 512, 512);
            renderCamera.allowHDR = true;
            renderCamera.enabled = false;

            // Basic defaults.
            renderCamera.backgroundColor = new Color(0, 0, 0, 0);
            renderCamera.fieldOfView = 30f;
            renderCamera.nearClipPlane = 1f;
            renderCamera.farClipPlane = 1000f;
        }


        /// <summary>
        /// Image size.
        /// </summary>
        public Vector2 Size
        {
            get => new Vector2(renderCamera.targetTexture.width, renderCamera.targetTexture.height);
            
            set
            {
                if (Size != value)
                {
                    // New size; set camera output sizes accordingly.
                    renderCamera.targetTexture = new RenderTexture((int)value.x, (int)value.y, 24, RenderTextureFormat.ARGB32);
                    renderCamera.pixelRect = new Rect(0f, 0f, value.x, value.y);
                }
            }
        }


        /// <summary>
        /// Currently rendered mesh.
        /// </summary>
        public Mesh Mesh
        {
            get => currentMesh;

            set
            {
                if (currentMesh != value)
                {
                    // Update currently rendered mesh if changed.
                    currentMesh = value;

                    if (value != null)
                    {
                        // Reset the bounding box to be the smallest that can encapsulate all verticies of the new mesh.
                        // That way the preview image is the largest size that fits cleanly inside the preview size.
                        currentBounds = new Bounds(Vector3.zero, Vector3.zero);
                        for (int i = 0; i < currentMesh.vertices.Length; i++)
                        {
                            currentBounds.Encapsulate(currentMesh.vertices[i]);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Current building texture.
        /// </summary>
        public RenderTexture Texture
        {
            get => renderCamera.targetTexture;
        }


        /// <summary>
        /// Preview camera rotation (degrees).
        /// </summary>
        public float CameraRotation
        {
            get { return currentRotation; }
            set { currentRotation = value % 360f; }
        }


        /// <summary>
        /// Zoom level.
        /// </summary>
        public float Zoom
        {
            get { return currentZoom; }
            set
            {
                currentZoom = Mathf.Clamp(value, 0.5f, 5f);
            }
        }


        /// <summary>
        /// Render the current mesh.
        /// </summary>
        public void Render()
        {
            if (currentMesh == null) return;

            float magnitude = currentBounds.extents.magnitude;
            float num = magnitude + 16f;
            float num2 = magnitude * currentZoom;

            renderCamera.transform.position = -Vector3.forward * num2;
            renderCamera.transform.rotation = Quaternion.identity;
            renderCamera.nearClipPlane = Mathf.Max(num2 - num * 1.5f, 0.01f);
            renderCamera.farClipPlane = num2 + num * 1.5f;

            Quaternion quaternion = Quaternion.Euler(-20f, 0f, 0f) * Quaternion.Euler(0f, currentRotation, 0f);
            Vector3 pos = quaternion * -currentBounds.center;
            Matrix4x4 matrix = Matrix4x4.TRS(pos, quaternion, Vector3.one);

            InfoManager infoManager = Singleton<InfoManager>.instance;
            InfoManager.InfoMode currentMod = infoManager.CurrentMode;
            InfoManager.SubInfoMode currentSubMod = infoManager.CurrentSubMode; ;
            infoManager.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.Default);

            Graphics.DrawMesh(currentMesh, matrix, material, 0, renderCamera, 0, null, true, true);
            renderCamera.RenderWithShader(material.shader, "");

            infoManager.SetCurrentMode(currentMod, currentSubMod);
        }
    }
}