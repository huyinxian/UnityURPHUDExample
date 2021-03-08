using UnityEngine;

namespace HUD
{
    /// <summary>
    /// 管理单个HUD对象的Mesh数据
    /// </summary>
    public class HUDMesh
    {
        public Mesh mesh;
        public Material material;

        /// <summary>
        /// 顶点坐标
        /// </summary>
        private BetterList<Vector3> _vertexList = new BetterList<Vector3>();
        /// <summary>
        /// 顶点UV
        /// </summary>
        private BetterList<Vector2> _uvList = new BetterList<Vector2>();
        /// <summary>
        /// 顶点颜色
        /// </summary>
        private BetterList<Color32> _colorList = new BetterList<Color32>();
        /// <summary>
        /// 顶点索引
        /// </summary>
        private BetterList<int> _indicesList = new BetterList<int>();

        public float scale = 1.0f;

        private int _oldGeometrySize;
        public int OldGeometrySize => _oldGeometrySize;

        private BetterList<HUDGeometry> _geometryList = new BetterList<HUDGeometry>();
        public int GeometrySize => _geometryList.size;

        private string _atlasName = string.Empty;
        public string AtlasName => _atlasName;
        private bool _textureLoading;
        private bool _isDirty;
        private bool _haveNullGeometry;

        private static Camera _hudCamera;
        private static bool _gameStart;

        private static readonly int _HUDSpriteMainTexPropertyId = Shader.PropertyToID("_MainTex");

        public static void OnGameStart()
        {
            _gameStart = true;
            GetHUDCamera();
        }

        public static void OnGameEnd()
        {
            _gameStart = false;
            _hudCamera = null;
        }

        public static Camera GetHUDCamera()
        {
            if (_hudCamera == null && _gameStart)
            {
                Camera camera = Camera.main;
                if (camera != null && camera.gameObject.CompareTag("MainCamera"))
                    _hudCamera = camera;
            }

            return _hudCamera;
        }

        public void SetAtlas(string atlasName)
        {
            if (!string.IsNullOrEmpty(_atlasName) && atlasName != _atlasName)
            {
                ReleaseTexture();
            }

            _atlasName = atlasName;
            if (HUDConst.HUDDebugEnabled)
                Debug.Log("Set hud texture. Texture name: " + _atlasName);

            if (material == null)
            {
                Shader shader = HUDAssetUtil.FindShader(HUDConst.HUDSpriteShaderName);
                if (shader == null)
                {
                    if (HUDConst.HUDDebugEnabled)
                        Debug.LogError("Shader " + HUDConst.HUDSpriteShaderName + " not exist!");
                }

                material = new Material(shader);
            }

            if (!string.IsNullOrEmpty(_atlasName))
                LoadTexture();
        }

        private void LoadTexture()
        {
            if (!_textureLoading && !string.IsNullOrEmpty(_atlasName))
            {
                if (HUDConst.HUDDebugEnabled)
                    Debug.Log("Loading texture. Texture name: " + _atlasName);

                _textureLoading = true;
                // 目前的图集是同步加载，直接执行回调
                OnHUDAtlasLoaded();
            }
        }

        private void ReleaseTexture()
        {
            // TODO: 图集引用计数减一
            if (HUDConst.HUDDebugEnabled)
                Debug.Log("Release hud texture. Texture name: " + _atlasName);
        }

        private void OnHUDAtlasLoaded()
        {
            if (!string.IsNullOrEmpty(_atlasName))
            {
                HUDAtlas atlas = HUDAtlasManager.GetAtlas(_atlasName);
                if (atlas != null)
                {
                    material.SetTexture(_HUDSpriteMainTexPropertyId, atlas.texture);
                }
            }
        }

        public void Release()
        {
            ClearGeometryAndTexture();

            if (mesh != null)
            {
                GameObject.Destroy(mesh);
                mesh = null;
            }

            if (material != null)
            {
                GameObject.Destroy(material);
                material = null;
            }
        }

        public void ClearGeometryAndTexture()
        {
            _isDirty = true;
            _haveNullGeometry = false;
            _geometryList.Clear();
            ReleaseTexture();
        }

        public void FastClearGeometry()
        {
            _isDirty = true;
            _haveNullGeometry = false;
            _geometryList.Clear();
        }

        public void PushHUDGeometry(HUDGeometry geometry)
        {
            _isDirty = true;
            geometry.indexInHUDMesh = _geometryList.size;
            _geometryList.Add(geometry);
            // TODO: 这里判断一下是否需要请求加载图集，目前先不加载
        }

        public void EraseHUDGeometry(HUDGeometry geometry)
        {
            int index = geometry.indexInHUDMesh;
            if (index >= 0 && index < _geometryList.size)
            {
                if (_geometryList[index] != null && geometry.InstanceId == _geometryList[index].InstanceId)
                {
                    _isDirty = true;
                    _haveNullGeometry = true;
                    _geometryList[index] = null;
                    return;
                }
            }

            for (int i = _geometryList.size - 1; i >= 0; i--)
            {
                if (_geometryList[i] != null && _geometryList[i].InstanceId == geometry.InstanceId)
                {
                    _isDirty = true;
                    _haveNullGeometry = true;
                    _geometryList[i] = null;
                    break;
                }
            }
        }

        public void SetDirty()
        {
            _isDirty = true;
        }

        public bool IsDirty()
        {
            return _isDirty;
        }

        /// <summary>
        /// 填充Mesh数据
        /// </summary>
        public void Fill()
        {
            if (!_isDirty)
                return;

            _isDirty = false;
            if (_haveNullGeometry)
            {
                _haveNullGeometry = false;
                _geometryList.ClearNullItem();
            }

            FillMesh();
            // TODO: 这里判断一下是否需要请求加载图集，目前先不加载
            _oldGeometrySize = _geometryList.size;
        }

        /// <summary>
        /// 填充顶点数据
        /// </summary>
        private void FillMesh()
        {
            int oldVertexSize = _vertexList.size;
            FillVertex();

            int last = _vertexList.size - 1;
            int capacity = _vertexList.buffer.Length;
            int vertexSize = _vertexList.size;

            if (last >= 0)
            {
                Vector3[] vertices = _vertexList.buffer;
                Vector2[] uv1s = _uvList.buffer;
                Color32[] colors = _colorList.buffer;

                for (int i = _vertexList.size; i < _vertexList.buffer.Length; i++)
                {
                    vertices[i] = vertices[last];
                    uv1s[i] = uv1s[last];
                    colors[i] = colors[last];
                }
            }

            _vertexList.size = capacity;
            _uvList.size = capacity;
            _colorList.size = capacity;
            
            // 更新顶点索引数据
            bool needResetIndex = oldVertexSize != capacity;
            if (needResetIndex)
                UpdateIndex(vertexSize);

            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.hideFlags = HideFlags.DontSave;
                mesh.name = "hud_mesh";
                mesh.MarkDynamic();
            }
            else if (needResetIndex || mesh.vertexCount != _vertexList.size)
            {
                mesh.Clear();
            }

            if (mesh != null)
            {
                mesh.vertices = _vertexList.buffer;
                mesh.uv = _uvList.buffer;
                mesh.colors32 = _colorList.buffer;
                mesh.triangles = _indicesList.buffer;
            }
        }

        private void FillVertex()
        {
            CleanPreWrite(_geometryList.size * 4);

            Vector2 offset = Vector2.zero;
            for (int i = 0; i < _geometryList.size; i++)
            {
                HUDGeometry geometry = _geometryList[i];
                geometry.indexInHUDMesh = i;

                float scaleX = HUDConst.HUD_GLOBAL_CAMERA_SCALE_X * geometry.scale;
                float scaleY = HUDConst.HUD_GLOBAL_CAMERA_SCALE_Y * geometry.scale;
                
                // 实时计算HUD挂在对象的屏幕坐标，让HUD始终相对于对象进行偏移
                Vector3 positionSS = _hudCamera.WorldToScreenPoint(geometry.positionWS);

                offset = geometry.vertexRT;
                offset += geometry.offsetSS;
                offset.x *= scaleX;
                offset.y *= scaleY;
                offset += geometry.move;
                // 将HUD到屏幕的距离z设置为固定值，并转换为世界坐标
                // 动画的偏移需要在屏幕空间计算，不受相机远近的影响
                Vector3 hudPositionSS = new Vector3(positionSS.x + offset.x, positionSS.y + offset.y, _hudCamera.nearClipPlane + 1.0f);
                _vertexList.Add(_hudCamera.ScreenToWorldPoint(hudPositionSS));

                offset = geometry.vertexRB;
                offset += geometry.offsetSS;
                offset.x *= scaleX;
                offset.y *= scaleY;
                offset += geometry.move;
                hudPositionSS = new Vector3(positionSS.x + offset.x, positionSS.y + offset.y, _hudCamera.nearClipPlane + 1.0f);
                _vertexList.Add(_hudCamera.ScreenToWorldPoint(hudPositionSS));
                
                offset = geometry.vertexLB;
                offset += geometry.offsetSS;
                offset.x *= scaleX;
                offset.y *= scaleY;
                offset += geometry.move;
                hudPositionSS = new Vector3(positionSS.x + offset.x, positionSS.y + offset.y, _hudCamera.nearClipPlane + 1.0f);
                _vertexList.Add(_hudCamera.ScreenToWorldPoint(hudPositionSS));
                
                offset = geometry.vertexLT;
                offset += geometry.offsetSS;
                offset.x *= scaleX;
                offset.y *= scaleY;
                offset += geometry.move;
                hudPositionSS = new Vector3(positionSS.x + offset.x, positionSS.y + offset.y, _hudCamera.nearClipPlane + 1.0f);
                _vertexList.Add(_hudCamera.ScreenToWorldPoint(hudPositionSS));
                
                _uvList.Add(geometry.uvRT);
                _uvList.Add(geometry.uvRB);
                _uvList.Add(geometry.uvLB);
                _uvList.Add(geometry.uvLT);
                _colorList.Add(geometry.colorRB);
                _colorList.Add(geometry.colorRT);
                _colorList.Add(geometry.colorLT);
                _colorList.Add(geometry.colorLB);
            }
        }

        private void CleanPreWrite(int vertexCount)
        {
            _vertexList.CleanPreWrite(vertexCount);
            _uvList.CleanPreWrite(vertexCount);
            _colorList.CleanPreWrite(vertexCount);
        }

        private void UpdateIndex(int vertexSize)
        {
            _indicesList.CleanPreWrite(vertexSize / 4 * 6);

            int capacity = _indicesList.buffer.Length;
            int[] indices = _indicesList.buffer;

            int index = 0;
            int i = 0;
            for (; i < vertexSize; i += 4)
            {
                indices[index++] = i;
                indices[index++] = i + 1;
                indices[index++] = i + 2;

                indices[index++] = i + 2;
                indices[index++] = i + 3;
                indices[index++] = i;
            }
            int nLast = vertexSize - 1;
            for (; index < capacity;)
            {
                indices[index++] = nLast;
                indices[index++] = nLast;
                indices[index++] = nLast;
                indices[index++] = nLast;
                indices[index++] = nLast;
                indices[index++] = nLast;
            }
            _indicesList.size = index;
        }
    }
}