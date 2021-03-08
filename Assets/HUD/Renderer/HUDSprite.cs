using UnityEngine;

namespace HUD
{
    public class HUDSprite : HUDGeometry
    {
        /// <summary>
        /// Sprite所属图集
        /// </summary>
        public string atlasName = string.Empty;
        /// <summary>
        /// Sprite名称
        /// </summary>
        public string spriteName = string.Empty;
        /// <summary>
        /// Sprite所属HUDMesh
        /// </summary>
        public HUDMesh parentMesh;

        public override void Init()
        {
            base.Init();

            HUDAtlas atlas = HUDAtlasManager.GetAtlas(atlasName);
            Sprite sprite = HUDAtlasManager.GetSprite(atlasName, spriteName);
            if (atlas == null || sprite == null)
                return;

            width = (int)sprite.rect.width;
            height = (int)sprite.rect.height;
            scale = 1.0f;

            Rect uv = GetUVs(sprite);

            float vertexLeft = 0.0f;
            float vertexRight = width;
            float vertexTop = 0.0f;
            float vertexBottom = height;
            
            vertexLT.Set(vertexLeft, vertexTop);
            vertexLB.Set(vertexLeft, vertexBottom);
            vertexRT.Set(vertexRight, vertexTop);
            vertexRB.Set(vertexRight, vertexBottom);

            float uvLeft = uv.xMin;
            float uvRight = uv.xMax;
            float uvTop = uv.yMax;
            float uvBottom = uv.yMin;
            
            // UV默认采用OpenGL屏幕坐标系
            uvLT.Set(uvLeft, uvBottom);
            uvLB.Set(uvLeft, uvTop);
            uvRT.Set(uvRight, uvBottom);
            uvRB.Set(uvRight, uvTop);

            colorLT = colorLB = colorRT = colorRB = Color.white;
        }

        /// <summary>
        /// 根据Sprite在图集纹理中的像素坐标和长宽计算出UV
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        private Rect GetUVs(Sprite sprite)
        {
            Rect uvs = sprite.rect;
            uvs.x /= sprite.texture.width;
            uvs.width /= sprite.texture.width;
            uvs.y /= sprite.texture.height;
            uvs.height /= sprite.texture.height;
            return uvs;
        }
    }
}