using System.Collections.Generic;
using UnityEngine;

namespace HUD
{
    public class HUDAtlas
    {
        /// <summary>
        /// 图集
        /// </summary>
        public Texture2D texture;

        /// <summary>
        /// 图集中所有的Sprite
        /// </summary>
        public readonly Dictionary<string, Sprite> spriteMap = new Dictionary<string, Sprite>();
    }
    
    public class HUDAtlasManager
    {
        private static readonly Dictionary<string, HUDAtlas> _cachedAtlasMap = new Dictionary<string, HUDAtlas>();

        public static Sprite GetSprite(string atlasName, string spriteName)
        {
            HUDAtlas atlas = GetAtlas(atlasName);
            Sprite sprite = null;
            if (atlas != null)
            {
                // 从缓存的图集中查找
                var spriteMap = _cachedAtlasMap[atlasName].spriteMap;
                sprite = spriteMap[spriteName];
            }

            if (sprite == null)
            {
                if (HUDConst.HUDDebugEnabled)
                    Debug.LogError("Sprite " + spriteName + " dose not exist in the " + atlasName);
            }

            return sprite;
        }

        public static HUDAtlas GetAtlas(string atlasName)
        {
            if (_cachedAtlasMap.TryGetValue(atlasName, out var atlas))
                return atlas;
            
            // TODO: 目前暂不在框架内部考虑图集的引用计数，仅由资源加载接口自行实现

            HUDAssetUtil.LoadHUDAssetAllSync(HUDConst.HUDAtlasResRoot + "/" + atlasName, objs =>
            {
                _cachedAtlasMap.Remove(atlasName);

                HUDAtlas newAtlas = new HUDAtlas();
                foreach (var obj in objs)
                {
                    if (obj is Texture2D)
                    {
                        Texture2D tex = obj as Texture2D;
                        newAtlas.texture = tex;
                    }
                    else if (obj is Sprite)
                    {
                        Sprite sp = obj as Sprite;
                        newAtlas.spriteMap.Add(sp.name, sp);
                    }
                }

                _cachedAtlasMap.Add(atlasName, newAtlas);
            });

            return _cachedAtlasMap[atlasName];
        }

        public static void RemoveAtlas(string atlasName)
        {
            if (_cachedAtlasMap.ContainsKey(atlasName))
                _cachedAtlasMap.Remove(atlasName);
        }
    }
}