// <copyright file="BuildingPreviewRenderer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;
    using ColossalFramework;
    using UnityEngine;

    /// <summary>
    /// Render a 3d image of a given mesh.
    /// </summary>
    internal class BuildingPreviewRenderer : MonoBehaviour
    {
        // Floor preview rendering.
        private readonly Texture2D _floorTexture;

        // Rendering settings.
        private readonly Camera _renderCamera;
        private Mesh _mesh;
        private Bounds _bounds;
        private float _rotation;
        private float _zoom;
        private Material _material;

        // Rendering sub-components.
        private List<BuildingInfo.MeshInfo> _subMeshes;
        private List<BuildingInfo.SubInfo> _subBuildings;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingPreviewRenderer"/> class.
        /// </summary>
        internal BuildingPreviewRenderer()
        {
            // Set up camera.
            _renderCamera = new GameObject("Camera").AddComponent<Camera>();
            _renderCamera.transform.SetParent(transform);
            _renderCamera.targetTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            _renderCamera.allowHDR = true;
            _renderCamera.enabled = false;

            // Basic defaults.
            _renderCamera.pixelRect = new Rect(0f, 0f, 512, 512);
            _renderCamera.backgroundColor = new Color(0, 0, 0, 0);
            _renderCamera.fieldOfView = 30f;
            _renderCamera.nearClipPlane = 1f;
            _renderCamera.farClipPlane = 1000f;

            // Create foor preview texture.

            // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
            _floorTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

            // Set individual pixel colours.
            Color32 newColor = new Color32(255, 0, 255, 127);
            _floorTexture.SetPixel(0, 0, newColor);
            _floorTexture.SetPixel(1, 0, newColor);
            _floorTexture.SetPixel(0, 1, newColor);
            _floorTexture.SetPixel(1, 1, newColor);

            // Apply all SetPixel calls
            _floorTexture.Apply();
        }

        /// <summary>
        /// Sets material to render.
        /// </summary>
        internal Material Material { set => _material = value; }

        /// <summary>
        /// Gets or sets the preview image size.
        /// </summary>
        internal Vector2 Size
        {
            get => new Vector2(_renderCamera.targetTexture.width, _renderCamera.targetTexture.height);

            set
            {
                if (Size != value)
                {
                    // New size; set camera output sizes accordingly.
                    _renderCamera.targetTexture = new RenderTexture((int)value.x, (int)value.y, 24, RenderTextureFormat.ARGB32);
                    _renderCamera.pixelRect = new Rect(0f, 0f, value.x, value.y);
                }
            }
        }

        /// <summary>
        /// Gets or sets the mesh to be rendered.
        /// </summary>
        internal Mesh Mesh
        {
            get => _mesh;

            set => _mesh = value;
        }

        /// <summary>
        /// Gets the target texture.
        /// </summary>
        internal RenderTexture Texture
        {
            get => _renderCamera.targetTexture;
        }

        /// <summary>
        /// Gets or sets the preview camera rotation (degrees).
        /// </summary>
        internal float CameraRotation
        {
            get => _rotation;

            set => _rotation = value % 360f;
        }

        /// <summary>
        /// Gets or sets the preview zoom level.
        /// </summary>
        internal float Zoom
        {
            get => _zoom;

            set => _zoom = Mathf.Clamp(value, 0.5f, 5f);
        }

        /// <summary>
        /// Sets mesh and material from a BuildingInfo prefab.
        /// </summary>
        /// <param name="prefab">Prefab to render.</param>
        /// <returns>True if the target was valid (prefab or at least one subbuilding contains a valid material, and the prefab has at least one primary mesh, submesh, or subbuilding).</returns>
        internal bool SetTarget(BuildingInfo prefab)
        {
            // Assign main mesh and material.
            Mesh = prefab.m_mesh;
            _material = prefab.m_material;

            // Set up or clear submesh list.
            if (_subMeshes == null)
            {
                _subMeshes = new List<BuildingInfo.MeshInfo>();
            }
            else
            {
                _subMeshes.Clear();
            }

            // Add any submeshes to our submesh list.
            if (prefab.m_subMeshes != null && prefab.m_subMeshes.Length > 0)
            {
                for (int i = 0; i < prefab.m_subMeshes.Length; i++)
                {
                    _subMeshes.Add(prefab.m_subMeshes[i]);
                }
            }

            // Set up or clear sub-building list.
            if (_subBuildings == null)
            {
                _subBuildings = new List<BuildingInfo.SubInfo>();
            }
            else
            {
                _subBuildings.Clear();
            }

            if (prefab.m_subBuildings != null && prefab.m_subBuildings.Length > 0)
            {
                for (int i = 0; i < prefab.m_subBuildings.Length; i++)
                {
                    _subBuildings.Add(prefab.m_subBuildings[i]);

                    // If we don't already have a valid material, grab this one.
                    if (_material == null)
                    {
                        _material = prefab.m_subBuildings[i].m_buildingInfo.m_material;
                    }
                }
            }

            return _material != null && (_mesh != null || _subBuildings.Count > 0 || _subMeshes.Count > 0);
        }

        /// <summary>
        /// Renders the current mesh.
        /// </summary>
        /// <param name="renderFloors">True to render floor previews, false otherwise.</param>
        /// <param name="floorDataPack">Floor data to render (ignored if renderFloors is false).</param>
        internal void Render(bool renderFloors, FloorDataPack floorDataPack)
        {
            // Check to see if we have submeshes or sub-buildings.
            bool hasSubMeshes = _subMeshes != null && _subMeshes.Count > 0;
            bool hasSubBuildings = _subBuildings != null && _subBuildings.Count > 0;

            // If no primary mesh and no other meshes, don't do anything here.
            if (_mesh == null && !hasSubBuildings && !hasSubMeshes)
            {
                return;
            }

            // Use solid color background.
            _renderCamera.clearFlags = CameraClearFlags.SolidColor;
            _renderCamera.backgroundColor = new Color32(33, 151, 199, 255);

            // Back up current game InfoManager mode.
            InfoManager infoManager = Singleton<InfoManager>.instance;
            InfoManager.InfoMode currentMode = infoManager.CurrentMode;
            InfoManager.SubInfoMode currentSubMode = infoManager.CurrentSubMode;

            // Set current game InfoManager to default (don't want to render with an overlay mode).
            infoManager.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.Default);
            infoManager.UpdateInfoMode();

            // Backup current exposure and sky tint.
            float gameExposure = DayNightProperties.instance.m_Exposure;
            Color gameSkyTint = DayNightProperties.instance.m_SkyTint;

            // Backup current game lighting.
            Light gameMainLight = RenderManager.instance.MainLight;

            // Set exposure and sky tint for render.
            DayNightProperties.instance.m_Exposure = 0.5f;
            DayNightProperties.instance.m_SkyTint = new Color(0, 0, 0);
            DayNightProperties.instance.Refresh();

            // Set up our render lighting settings.
            Light renderLight = DayNightProperties.instance.sunLightSource;
            RenderManager.instance.MainLight = renderLight;

            // Reset the bounding box to be the smallest that can encapsulate all verticies of the new mesh.
            // That way the preview image is the largest size that fits cleanly inside the preview size.
            _bounds = new Bounds(Vector3.zero, Vector3.zero);
            Vector3[] vertices;

            // Set default model position.
            Vector3 modelPosition = new Vector3(0f, 0f, 0f);

            // Add our main mesh, if any (some are null, because they only 'appear' through subbuildings - e.g. Boston Residence Garage).
            if (_mesh != null && _material != null)
            {
                // Use separate verticies instance instead of accessing Mesh.vertices each time (which is slow).
                // >10x measured performance improvement by doing things this way instead.
                vertices = _mesh.vertices;

                // Initialize minimum and maximum coordinate markers for floor previewing.
                float maxX = 0, maxZ = 0, minX = 0, minZ = 0;

                for (int i = 0; i < vertices.Length; i++)
                {
                    // Exclude vertices with large negative Y values (underground) from our bounds (e.g. SoCal Laguna houses), otherwise the result doesn't look very good.
                    if (vertices[i].y > -2)
                    {
                        _bounds.Encapsulate(vertices[i]);
                    }

                    // Shift minimum and maxmimum coordinates, if and as appropriate.
                    if (vertices[i].x > maxX)
                    {
                        maxX = vertices[i].x;
                    }

                    if (vertices[i].x < minX)
                    {
                        minX = vertices[i].x;
                    }

                    if (vertices[i].z > maxZ)
                    {
                        maxZ = vertices[i].z;
                    }

                    if (vertices[i].z < minZ)
                    {
                        minZ = vertices[i].z;
                    }
                }

                // Calculate rendering matrix.
                Matrix4x4 matrix = Matrix4x4.TRS(modelPosition, Quaternion.Euler(Vector3.zero), Vector3.one);

                // Floor preview rendering, if set to do so and we have a valid floor calculation pack set.
                if (renderFloors && floorDataPack != null)
                {
                    // Create new material using building shader.
                    Material testMaterial = new Material(_material.shader)
                    {
                        mainTexture = _floorTexture,
                    };

                    // Coordinates using current bounds.
                    float left = minX - 1.5f;
                    float right = maxX + 1.5f;
                    float back = minZ - 1.5f;
                    float front = maxZ + 1.5f;

                    // Lists.
                    List<Vector3> vectorList = new List<Vector3>();
                    List<int> triList = new List<int>();
                    List<Vector2> uvList = new List<Vector2>();

                    // Draw ground floor.
                    AddFloor(left, right, front, back, 0f, vectorList, triList, uvList);

                    // Draw top of first floor, using transformation matrix to position floor.
                    float floorHeight = floorDataPack.m_firstFloorMin + floorDataPack.m_firstFloorExtra;

                    // Draw addtional floors, incrementing transformation matrix, until we reach the top of the building.  Increment height once at start to avoid fencepost error.
                    while (floorHeight <= _bounds.max.y - floorDataPack.m_floorHeight)
                    {
                        AddFloor(left, right, front, back, floorHeight, vectorList, triList, uvList);
                        floorHeight += floorDataPack.m_floorHeight;
                    }

                    // Create mesh and add to scene.
                    Mesh floorMesh = new Mesh();
                    floorMesh.Clear();
                    floorMesh.SetVertices(vectorList);
                    floorMesh.uv = uvList.ToArray();
                    floorMesh.triangles = triList.ToArray();
                    Graphics.DrawMesh(floorMesh, matrix, testMaterial, 0, _renderCamera, 0, null, false, false);
                }

                // Add building mesh to scene.
                Graphics.DrawMesh(_mesh, matrix, _material, 0, _renderCamera, 0, null, true, true);
            }

            // Render submeshes, if any.
            if (hasSubMeshes)
            {
                foreach (BuildingInfo.MeshInfo subMesh in _subMeshes)
                {
                    // Get local reference.
                    BuildingInfoBase subInfo = subMesh?.m_subInfo;

                    // Just in case.
                    if (subInfo?.m_mesh != null && subInfo?.m_material != null)
                    {
                        // Recalculate our matrix based on our submesh position and rotation.

                        // Calculate the relative rotation.
                        // We need to rotate the submesh before we apply the model rotation.
                        // Note that the order of multiplication (relative to the angle of operation) is reversed in the code, because of the way Unity overloads the multiplication operator.
                        // Note also that the submesh angle needs to be inverted to rotate correctly around the Y axis in our space.
                        Quaternion relativeRotation = Quaternion.AngleAxis(subMesh.m_angle * -1, Vector3.up);

                        // Calculate relative position of mesh given its starting position and our model rotation.
                        Vector3 relativePosition = subMesh.m_position;

                        // Put it all together into our rendering matrix.
                        Matrix4x4 matrix = Matrix4x4.TRS(relativePosition + modelPosition, relativeRotation, Vector3.one);

                        // Add submesh to scene.
                        Graphics.DrawMesh(subInfo.m_mesh, matrix, subInfo.m_material, 0, _renderCamera, 0, null, true, true);

                        // Expand our bounds to encapsulate the submesh.
                        vertices = subInfo.m_mesh.vertices;
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            // Exclude vertices with large negative Y values (underground) from our bounds (e.g. SoCal Laguna houses), otherwise the result doesn't look very good.
                            if (vertices[i].y + relativePosition.y > -2)
                            {
                                // Transform coordinates to our model rotation before encapsulating, otherwise we tend to cut off corners.
                                _bounds.Encapsulate(relativeRotation * (vertices[i] + subMesh.m_position));
                            }
                        }
                    }
                }
            }

            // Render subbuildings, if any.
            if (hasSubBuildings)
            {
                foreach (BuildingInfo.SubInfo subBuilding in _subBuildings)
                {
                    // Get local reference.
                    BuildingInfo subInfo = subBuilding?.m_buildingInfo;

                    // Just in case.
                    if (subInfo?.m_mesh != null && subInfo?.m_material != null)
                    {
                        // Calculate the relative rotation.
                        // We need to rotate the subbuilding before we apply the model rotation.
                        // Note that the order of multiplication (relative to the angle of operation) is reversed in the code, because of the way Unity overloads the multiplication operator.
                        Quaternion relativeRotation = Quaternion.AngleAxis(subBuilding.m_angle, Vector3.up);

                        // Recalculate our matrix based on our submesh position.
                        Vector3 relativePosition = subBuilding.m_position;
                        Matrix4x4 matrix = Matrix4x4.TRS(relativePosition + modelPosition, relativeRotation, Vector3.one);

                        // Add subbuilding to scene.
                        Graphics.DrawMesh(subInfo.m_mesh, matrix, subInfo.m_material, 0, _renderCamera, 0, null, true, true);

                        // Expand our bounds to encapsulate the submesh.
                        vertices = subInfo.m_mesh.vertices;
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            // Exclude vertices with large negative Y values (underground) from our bounds (e.g. SoCal Laguna houses), otherwise the result doesn't look very good.
                            if (vertices[i].y + relativePosition.y > -2)
                            {
                                _bounds.Encapsulate(vertices[i] + relativePosition);
                            }
                        }
                    }
                }
            }

            // Set zoom to encapsulate entire model.
            float magnitude = _bounds.extents.magnitude;
            float clipExtent = (magnitude + 16f) * 1.5f;
            float clipCenter = magnitude * _zoom;

            // Clip planes.
            _renderCamera.nearClipPlane = Mathf.Max(clipCenter - clipExtent, 0.01f);
            _renderCamera.farClipPlane = clipCenter + clipExtent;

            // Camera position and rotation - directly behind the model, facing the centre of the model's bounds.
            _renderCamera.transform.position = (-Vector3.forward * clipCenter) + _bounds.center;
            _renderCamera.transform.RotateAround(_bounds.center, Vector3.right, 20f);
            _renderCamera.transform.RotateAround(_bounds.center, Vector3.up, -_rotation);
            _renderCamera.transform.LookAt(_bounds.center);

            // If game is currently in nighttime, enable sun and disable moon lighting.
            if (gameMainLight == DayNightProperties.instance.moonLightSource)
            {
                DayNightProperties.instance.sunLightSource.enabled = true;
                DayNightProperties.instance.moonLightSource.enabled = false;
            }

            // Light settings.
            renderLight.transform.eulerAngles = new Vector3(55f, -_rotation - 20f, 0f);
            renderLight.intensity = 2f;
            renderLight.color = Color.white;

            // Render!
            _renderCamera.RenderWithShader(_material.shader, string.Empty);

            // Restore game lighting.
            RenderManager.instance.MainLight = gameMainLight;

            // Reset to moon lighting if the game is currently in nighttime.
            if (gameMainLight == DayNightProperties.instance.moonLightSource)
            {
                DayNightProperties.instance.sunLightSource.enabled = false;
                DayNightProperties.instance.moonLightSource.enabled = true;
            }

            // Restore game exposure and sky tint.
            DayNightProperties.instance.m_Exposure = gameExposure;
            DayNightProperties.instance.m_SkyTint = gameSkyTint;
            DayNightProperties.instance.Refresh();

            // Restore game InfoManager mode.
            infoManager.SetCurrentMode(currentMode, currentSubMode);
            infoManager.UpdateInfoMode();
        }

        /// <summary>
        /// Adds a dynamically-generated floor preview to the specified vector, triangle and UV lists.
        /// </summary>
        /// <param name="left">Minimum X coordinate of floor preview.</param>
        /// <param name="right">Maximum X coordinate of floor preview.</param>
        /// <param name="front">Minimum Z coordinate of floor preview.</param>
        /// <param name="back">Maximum Z coordinate of floor preview.</param>
        /// <param name="top">Y coordinate of floor (top of preview).</param>
        /// <param name="vectorList">List of vectors to add to.</param>
        /// <param name="triList">List of tris to add to.</param>
        /// <param name="uvList">List of UV coordinates to add to.</param>
        private void AddFloor(float left, float right, float front, float back, float top, List<Vector3> vectorList, List<int> triList, List<Vector2> uvList)
        {
            // Bottom of rendered floor.
            float bottom = top - 0.5f;

            // Vectors from coordinates.
            Vector3 frontLeftBottom = new Vector3(left, bottom, front);
            Vector3 frontRightBottom = new Vector3(right, bottom, front);
            Vector3 rearLeftBottom = new Vector3(left, bottom, back);
            Vector3 rearRightBottom = new Vector3(right, bottom, back);
            Vector3 frontLeftTop = new Vector3(left, top, front);
            Vector3 frontRightTop = new Vector3(right, top, front);
            Vector3 rearLeftTop = new Vector3(left, top, back);
            Vector3 rearRightTop = new Vector3(right, top, back);

            // Add tris to create shape.
            AddTri(frontLeftTop, frontRightTop, rearRightTop, vectorList, triList, uvList);
            AddTri(rearLeftTop, frontLeftTop, rearRightTop, vectorList, triList, uvList);
            AddTri(frontLeftTop, frontLeftBottom, frontRightTop, vectorList, triList, uvList);
            AddTri(frontRightBottom, frontRightTop, frontLeftBottom, vectorList, triList, uvList);
            AddTri(rearLeftTop, rearRightTop, rearLeftBottom, vectorList, triList, uvList);
            AddTri(rearRightBottom, rearLeftBottom, rearRightTop, vectorList, triList, uvList);
            AddTri(rearLeftTop, rearLeftBottom, frontLeftBottom, vectorList, triList, uvList);
            AddTri(frontLeftBottom, frontLeftTop, rearLeftTop, vectorList, triList, uvList);
            AddTri(rearRightTop, frontRightBottom, rearRightBottom, vectorList, triList, uvList);
            AddTri(frontRightBottom, rearRightTop, frontRightTop, vectorList, triList, uvList);
        }

        /// <summary>
        /// Adds a triangle with the specified vertices to the provided lists of vectors, tris and UV coordinates.
        /// </summary>
        /// <param name="vert1">First vertice.</param>
        /// <param name="vert2">Second vertice.</param>
        /// <param name="vert3">Third vertice.</param>
        /// <param name="vectors">List of vectors to add to.</param>
        /// <param name="tris">List of tris to add to.</param>
        /// <param name="uv">List of UV coordinates to add to.</param>
        private void AddTri(Vector3 vert1, Vector3 vert2, Vector3 vert3, List<Vector3> vectors, List<int> tris, List<Vector2> uv)
        {
            // Base triangle count, for triangle mapping.
            int vectorCount = vectors.Count;

            // First vertex.
            vectors.Add(vert1);
            tris.Add(vectorCount++);
            uv.Add(new Vector2(0, 0));

            // Second vertex.
            vectors.Add(vert2);
            tris.Add(vectorCount++);
            uv.Add(new Vector2(1, 0));

            // Third vertex.
            vectors.Add(vert3);
            tris.Add(vectorCount);
            uv.Add(new Vector2(0, 1));
        }
    }
}