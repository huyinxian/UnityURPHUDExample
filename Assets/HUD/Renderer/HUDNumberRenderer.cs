using UnityEngine;

namespace HUD
{
    public class HUDNumberSetting
    {
        /// <summary>
        /// 图集名称
        /// </summary>
        public string atlasName;
        /// <summary>
        /// 位于首位的Sprite的名称
        /// </summary>
        public string headSpriteName;
        /// <summary>
        /// +号的名称
        /// </summary>
        public string addSpriteName;
        /// <summary>
        /// -号的名称
        /// </summary>
        public string subSpriteName;
        /// <summary>
        /// 0-9，一般对应数字，但也可以换成任意的Sprite
        /// </summary>
        public string[] numberSpriteNames = new string[10];

        public void InitNumber(string atlasName, string headIconName, string prefix)
        {
            // TODO: 这里最好改成一一对应的形式，目前为了方便起见暂时先这么配置
            this.atlasName = atlasName;
            headSpriteName = prefix + headIconName;
            addSpriteName = prefix + "add";
            subSpriteName = prefix + "sub";
            for (int i = 0; i < 10; i++)
            {
                numberSpriteNames[i] = prefix + i;
            }
        }
    }
    
    /// <summary>
    /// HUD飘字渲染器
    /// </summary>
    public class HUDNumberRenderer : HUDAnimRenderer
    {
        /// <summary>
        /// HUD飘字对应的图片设置
        /// </summary>
        private HUDNumberSetting[] _settings;

        private BetterList<int> _tempNumbers = new BetterList<int>();

        public override void InitSetting(HUDSettings hudSettings)
        {
            base.InitSetting(hudSettings);

            _settings = new HUDNumberSetting[_attributes.Length];
            // 初始化飘字的参数
            for (int i = 0; i < _attributes.Length; i++)
            {
                _settings[i] = new HUDNumberSetting();
                _settings[i].InitNumber(_attributes[i].atlasName, _attributes[i].headSpriteName, _attributes[i].spritePrefix);
            }
        }

        public void AddHUDNumber(Vector3 position, int hudType, int number, bool showHead, bool showAdd, bool showSub)
        {
            Camera hudCamera = HUDMesh.GetHUDCamera();
            if (hudCamera == null)
                return;

            HUDAnimAttribute attribute = _attributes[hudType];
            HUDAnimEntry entry = GetOrCreateHUDAnimEntry(hudType);
            entry.hudType = hudType;
            entry.pNext = _pValidEntry;
            _pValidEntry = entry;
            
            // 显示负号
            if (number < 0)
            {
                showSub = true;
                number = -number;
            }
            
            entry.Reset();
            entry.spriteGap = attribute.SpriteGap;
            entry.startAnimTime = Time.time;
            entry.targetTrans = null;
            entry.positionWS = position;

            if (attribute.ScreenAlign)
            {
                ///////////////////////////
                // 屏幕空间
                ///////////////////////////
                
                // TODO: 支持在屏幕空间计算，位置和缩放不受相机位置影响
            }
            else
            {
                ///////////////////////////
                // 世界空间
                ///////////////////////////
                
                // 注意，起始偏移需要在世界空间计算，避免因为视角的远近导致HUD的起始位置不一致
                // 起始偏移以像素为单位，需要转换成米
                position.x += attribute.OffsetX / 100;
                position.y += attribute.OffsetY / 100;
            }

            HUDNumberSetting spSetting = _settings[hudType];
            // 添加飘字首位的Sprite
            if (showHead)
            {
                entry.PushSprite(spSetting.atlasName, spSetting.headSpriteName);
            }
            
            // 添加+和-
            if (showAdd)
            {
                entry.PushSprite(spSetting.atlasName, spSetting.addSpriteName);
            }
            else if (showSub)
            {
                entry.PushSprite(spSetting.atlasName, spSetting.subSpriteName);
            }
            
            // 拆分数字
            _tempNumbers.Clear();
            int n = 0;
            do
            {
                n = number % 10;
                number /= 10;
                _tempNumbers.Add(n);
            } while (number > 0);
            
            // 反转数组
            _tempNumbers.Reverse();
            
            // 添加数字
            for (int i = 0; i < _tempNumbers.size; i++)
            {
                n = _tempNumbers[i];
                entry.PushSprite(spSetting.atlasName, spSetting.numberSpriteNames[n]);
            }
            
            // 对齐
            switch (attribute.AlignType)
            {
                case HUDAlignType.align_right:
                    entry.AlignRight();
                    break;
                case HUDAlignType.align_center:
                    entry.AlignCenter();
                    break;
                default:
                    entry.AlignLeft();
                    break;
            }
            
            Push(entry);
            PlayAnimation(entry);
        }
    }
}