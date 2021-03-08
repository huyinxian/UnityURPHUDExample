using System;
using UnityEngine;

namespace HUD
{
    public class HUDSettings
    {
        private static HUDSettings _instance;
        public static HUDSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HUDSettings();
                    _instance.Init();
                }

                return _instance;
            }
        }

        private void Init()
        {
            HUDAssetUtil.LoadHUDAssetSync(HUDConst.HUDSettingPath, o =>
            {
                GameObject go = o as GameObject;
                GameObject.DontDestroyOnLoad(go);
                HUDAnimSetting hudSetting = go.transform.GetComponent<HUDAnimSetting>();
                Debug.Log("HUD setting loaded.");
                if (hudSetting != null)
                {
                    ParseNumberHUDSetting(hudSetting);
                }
            });
        }
        
        #region 飘字类型

        /// <summary>
        /// 飘字类型属性
        /// </summary>
        public HUDAnimAttribute[] animAttributes;

        private void ParseNumberHUDSetting(HUDAnimSetting hudAnimSetting)
        {
            animAttributes = hudAnimSetting.animAttributes;
        }

        #endregion
    }

    public enum HUDAlignType
    {
        align_left,   // 左对齐
        align_center, // 右对齐
        align_right,  // 居中
    }

    /// <summary>
    /// HUD飘字所用的动画参数
    /// </summary>
    [Serializable]
    public struct HUDAnimAttribute
    {
        /// <summary>
        /// HUD名称，用于标识类型
        /// </summary>
        public string name;
        /// <summary>
        /// 图集名称
        /// </summary>
        public string atlasName;
        /// <summary>
        /// HUD图片的前缀
        /// </summary>
        public string spritePrefix;
        /// <summary>
        /// HUD的前缀Icon，会显示在HUD的最前面
        /// </summary>
        public string headSpriteName;
        /// <summary>
        /// 透明度变化曲线
        /// </summary>
        public AnimationCurve AlphaCurve;
        /// <summary>
        /// 缩放变化曲线
        /// </summary>
        public AnimationCurve ScaleCurve;
        /// <summary>
        /// x轴位移
        /// </summary>
        public AnimationCurve MoveXCurve;
        /// <summary>
        /// y轴位移
        /// </summary>
        public AnimationCurve MoveYCurve;
        /// <summary>
        /// HUD起始偏移x（世界空间）
        /// </summary>
        public float OffsetX;
        /// <summary>
        /// HUD起始偏移y（世界空间）
        /// </summary>
        public float OffsetY;
        /// <summary>
        /// HUD图片显示间隔
        /// </summary>
        public int SpriteGap;
        /// <summary>
        /// 对齐方式
        /// </summary>
        public HUDAlignType AlignType;
        /// <summary>
        /// 是否根据屏幕对齐
        /// </summary>
        public bool ScreenAlign;
        /// <summary>
        /// 屏幕对齐方式
        /// </summary>
        public HUDAlignType ScreenAlignType;
    }
}