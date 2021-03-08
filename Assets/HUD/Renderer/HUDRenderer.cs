using UnityEngine;
using UnityEngine.Rendering;

namespace HUD
{
    /// <summary>
    /// HUD渲染器
    /// </summary>
    public class HUDRenderer
    {
        /// <summary>
        /// 所有的HUDMesh
        /// </summary>
        private BetterList<HUDMesh> _meshList = new BetterList<HUDMesh>();
        /// <summary>
        /// 有效的HUDMesh
        /// </summary>
        private BetterList<HUDMesh> _validList = new BetterList<HUDMesh>();

        protected bool _isDirty;

        private static readonly string _RenderPassName = "HUD Render Pass";

        public void SetDirty()
        {
            _isDirty = true;
        }

        public bool IsDirty()
        {
            return _isDirty;
        }

        public HUDMesh GetOrCreateMesh(string atlasName)
        {
            // 先查找有效的HUDMesh
            for (int i = _validList.size - 1; i >= 0; i--)
            {
                if (_validList[i].AtlasName == atlasName)
                    return _validList[i];
            }
            
            // 查找所有的HUDMesh
            for (int i = _meshList.size - 1; i >= 0; i--)
            {
                if (_meshList[i].AtlasName == atlasName)
                {
                    _validList.Add(_meshList[i]);
                    _meshList[i].SetAtlas(atlasName);
                    _isDirty = true;
                    return _meshList[i];
                }
            }
            
            // 没有找到相同图集的HUDMesh时直接创建
            HUDMesh newMesh = new HUDMesh();
            newMesh.SetAtlas(atlasName);
            _meshList.Add(newMesh);
            _validList.Add(newMesh);
            _isDirty = true;
            return newMesh;
        }

        public virtual void Release()
        {
            for (int i = 0; i < _meshList.size; i++)
            {
                _meshList[i].Release();
                _meshList[i] = null;
            }

            _meshList.Clear();
            _validList.Clear();
        }

        public void FastClearGeometry()
        {
            for (int i = _validList.size - 1; i >= 0; i--)
            {
                HUDMesh mesh = _validList[i];
                mesh.FastClearGeometry();
            }
            
            _validList.Clear();
        }

        /// <summary>
        /// 填充Mesh数据
        /// </summary>
        protected void FillMesh()
        {
            for (int i = _validList.size - 1; i >= 0; i--)
            {
                HUDMesh mesh = _validList[i];
                if (mesh.IsDirty())
                {
                    int oldGeometrySize = mesh.OldGeometrySize;
                    mesh.Fill();

                    int curGeometrySize = mesh.GeometrySize;
                    if ((oldGeometrySize != 0 && curGeometrySize == 0) ||
                        (oldGeometrySize == 0 && curGeometrySize != 0))
                        _isDirty = true;

                    if (curGeometrySize == 0)
                    {
                        _validList.RemoveAt(i);
                        mesh.ClearGeometryAndTexture();
                    }
                }
            }
        }

        /// <summary>
        /// 渲染HUD
        /// </summary>
        protected void Render(CommandBuffer cmd)
        {
            cmd.name = _RenderPassName;

            _isDirty = false;
            if (_validList.size == 0)
                return;

            Matrix4x4 matWorld = Matrix4x4.identity;
            for (int i = 0; i < _validList.size; i++)
            {
                HUDMesh mesh = _validList[i];
                if (mesh.GeometrySize > 0)
                {
                    cmd.DrawMesh(mesh.mesh, matWorld, mesh.material);
                }
            }
        }
    }
}