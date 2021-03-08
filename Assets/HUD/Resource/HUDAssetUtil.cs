using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HUD
{
    /// <summary>
    /// 资源加载工具类，接入项目时可以替换对应的资源加载接口
    /// </summary>
    public class HUDAssetUtil
    {
        public static void LoadHUDAssetAllSync(string resRelativePath, Action<Object[]> callback)
        {
            Object[] objs = Resources.LoadAll(resRelativePath);
            if (objs != null)
                callback(objs);
        }

        public static void LoadHUDAssetSync(string resRelativePath, Action<Object> callback)
        {
            Object obj = Resources.Load(resRelativePath);
            if (obj != null)
                callback(obj);
        }

        public static Shader FindShader(string name)
        {
            return Shader.Find(name);
        }
    }
}