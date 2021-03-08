using UnityEngine;

namespace HUD
{
    public class HUDGeometry
    {
        // 顶点位置，左上、左下、右上、右下
        public Vector2 vertexLT;
        public Vector2 vertexLB;
        public Vector2 vertexRT;
        public Vector2 vertexRB;
        
        // 顶点UV
        public Vector2 uvLT;
        public Vector2 uvLB;
        public Vector2 uvRT;
        public Vector2 uvRB;
        
        // 顶点颜色
        public Color32 colorLT;
        public Color32 colorLB;
        public Color32 colorRT;
        public Color32 colorRB;

        /// <summary>
        /// 世界坐标
        /// </summary>
        public Vector3 positionWS;
        /// <summary>
        /// 起始偏移（屏幕坐标）
        /// </summary>
        public Vector2 offsetSS;
        /// <summary>
        /// 动画偏移（屏幕坐标）
        /// </summary>
        public Vector2 move;
        /// <summary>
        /// 缩放
        /// </summary>
        public float scale;
        /// <summary>
        /// 宽度
        /// </summary>
        public int width;
        /// <summary>
        /// 高度
        /// </summary>
        public int height;

        /// <summary>
        /// 唯一Id
        /// </summary>
        private int _instanceId;
        public int InstanceId
        {
            get => _instanceId;
            set => _instanceId = value;
        }

        /// <summary>
        /// 对应HUDMesh._geometryList中的索引
        /// </summary>
        public int indexInHUDMesh;
        
        public virtual void Init() { }
    }

    public class HUDGeometryPool
    {
        private static HUDGeometryPool _instance;
        public static HUDGeometryPool Instance => _instance ?? (_instance = new HUDGeometryPool());

        private static readonly BetterList<HUDGeometry> _geometryPool = new BetterList<HUDGeometry>();
        private static readonly BetterList<HUDSprite> _spritePool = new BetterList<HUDSprite>();

        private static int _increasedId;

        public HUDGeometry CreateGeometry()
        {
            if (_geometryPool.size > 0)
                return _geometryPool.Pop();

            HUDGeometry geometry = new HUDGeometry();
            geometry.InstanceId = ++_increasedId;
            return geometry;
        }

        public void ReleaseGeometry(HUDGeometry geometry)
        {
            if (geometry != null)
            {
                _geometryPool.Add(geometry);
            }
        }

        public HUDSprite CreateSprite()
        {
            if (_spritePool.size > 0)
                return _spritePool.Pop();

            HUDSprite sprite = new HUDSprite();
            sprite.InstanceId = ++_increasedId;
            return sprite;
        }

        public void ReleaseSprite(HUDSprite sprite)
        {
            if (sprite != null)
            {
                _spritePool.Add(sprite);
            }
        }
    }
}