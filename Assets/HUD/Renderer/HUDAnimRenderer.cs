using UnityEngine;
using UnityEngine.Rendering;

namespace HUD
{
    /// <summary>
    /// HUD动画对象
    /// </summary>
    public class HUDAnimEntry
    {
        /// <summary>
        /// HUD挂载对象
        /// </summary>
        public Transform targetTrans;
        /// <summary>
        /// 下一个HUD对象
        /// </summary>
        public HUDAnimEntry pNext;
        /// <summary>
        /// HUD类型
        /// </summary>
        public int hudType;
        /// <summary>
        /// 挂载对象的世界坐标
        /// </summary>
        public Vector3 positionWS;
        /// <summary>
        /// HUD动画位移
        /// </summary>
        public Vector2 move;
        /// <summary>
        /// HUD整体缩放
        /// </summary>
        public float scale;
        /// <summary>
        /// HUD动画缩放
        /// </summary>
        public float animScale;
        /// <summary>
        /// HUD动画透明度变化
        /// </summary>
        public float alpha;
        /// <summary>
        /// HUD宽度
        /// </summary>
        public int width;
        /// <summary>
        /// HUD高度
        /// </summary>
        public int height;
        /// <summary>
        /// Sprite间隔
        /// </summary>
        public int spriteGap;
        /// <summary>
        /// HUD动画开始时间
        /// </summary>
        public float startAnimTime;
        /// <summary>
        /// HUD动画是否结束
        /// </summary>
        public bool stopped;
        /// <summary>
        /// HUD使用的Sprite
        /// </summary>
        public BetterList<HUDSprite> spriteList = new BetterList<HUDSprite>();

        public void Reset()
        {
            targetTrans = null;
            positionWS = Vector3.zero;
            move = Vector2.zero;
            scale = 1.0f;
            animScale = 1.0f;
            alpha = 1.0f;
            width = 0;
            height = 0;
            spriteGap = 0;
            stopped = false;
            ReleaseSprite();
        }

        public void ReleaseSprite()
        {
            for (int i = spriteList.size - 1; i >= 0; i--)
            {
                HUDSprite sprite = spriteList[i];
                if (sprite.parentMesh != null)
                    sprite.parentMesh.EraseHUDGeometry(sprite);
                sprite.parentMesh = null;
                HUDGeometryPool.Instance.ReleaseSprite(sprite);
                spriteList[i] = null;
            }
            
            spriteList.Clear();
        }

        /// <summary>
        /// 添加一张Sprite
        /// </summary>
        public void PushSprite(string atlasName, string spriteName)
        {
            HUDSprite sprite = HUDGeometryPool.Instance.CreateSprite();
            sprite.positionWS = positionWS;
            // 飘字目前只显示一行，y轴偏移直接归零
            sprite.offsetSS.Set(width, 0f);
            sprite.atlasName = atlasName;
            sprite.spriteName = spriteName;
            sprite.Init();
            spriteList.Add(sprite);
            // 为飘字中的每个Sprite设置间隔
            width += sprite.width + spriteGap;
            if (height < sprite.height)
                height = sprite.height;
        }

        /// <summary>
        /// 水平左对齐
        /// </summary>
        public void AlignLeft()
        {
            width -= spriteGap;
            SetAlignOffset(0f);
        }

        /// <summary>
        /// 水平居中
        /// </summary>
        public void AlignCenter()
        {
            width -= spriteGap;
            float halfWidth = width * 0.5f;
            SetAlignOffset(halfWidth);
        }

        /// <summary>
        /// 水平右对齐
        /// </summary>
        public void AlignRight()
        {
            width -= spriteGap;
            SetAlignOffset(width);
        }

        private void SetAlignOffset(float offsetX)
        {
            float halfHeight = height * 0.5f;
            for (int i = 0; i < spriteList.size; i++)
            {
                spriteList[i].offsetSS.x -= offsetX;
                spriteList[i].offsetSS.y -= spriteList[i].height * 0.5f - halfHeight;
            }
        }
    }
    
    /// <summary>
    /// HUD动画渲染器
    /// </summary>
    public class HUDAnimRenderer : HUDRenderer
    {
        /// <summary>
        /// HUD飘字所用的动画参数设置
        /// </summary>
        protected HUDAnimAttribute[] _attributes;
        /// <summary>
        /// 缓存的HUD飘字
        /// </summary>
        protected HUDAnimEntry _pInvalidEntry;
        /// <summary>
        /// 正在执行动画的HUD飘字
        /// </summary>
        protected HUDAnimEntry _pValidEntry;
        /// <summary>
        /// HUD动画持续时间
        /// </summary>
        private float _maxAnimDuration = 2.0f;

        private float _ScreenScaleX = 1.0f;
        private float _ScreenScaleY = 1.0f;

        private float _curAnimDuration;
        private bool _addCommandBuffer;
        private CommandBuffer _cmd = new CommandBuffer();

        private bool _updateLogicAdded;
        private float _lastExecuteUpdateLogicTime;

        public virtual void InitSetting(HUDSettings hudSettings)
        {
            _attributes = hudSettings.animAttributes;
        }

        /// <summary>
        /// 将一个HUD对象的图片数据全部填充到HUDMesh中，准备渲染
        /// </summary>
        protected void Push(HUDAnimEntry entry)
        {
            for (int i = 0; i < entry.spriteList.size; i++)
            {
                HUDMesh mesh = GetOrCreateMesh(entry.spriteList[i].atlasName);
                entry.spriteList[i].parentMesh = mesh;
                mesh.PushHUDGeometry(entry.spriteList[i]);
            }
        }

        protected HUDAnimEntry GetOrCreateHUDAnimEntry(int hudType)
        {
            HUDAnimEntry entry = _pInvalidEntry;
            // 先从缓存的链表中查找
            if (entry != null)
            {
                _pInvalidEntry = entry.pNext;
                entry.pNext = null;
                return entry;
            }
            
            // 找不到就创建一个新的对象
            entry = new HUDAnimEntry();
            entry.hudType = hudType;
            return entry;
        }

        /// <summary>
        /// 执行HUD动画
        /// </summary>
        protected void PlayAnimation(HUDAnimEntry entry)
        {
            // 添加帧循环逻辑
            if (!_updateLogicAdded)
            {
                _isDirty = true;
                HUDRenderManager.Instance.AddUpdateFunc(UpdateLogic);
            }

            float duration = Time.time - entry.startAnimTime;

            HUDAnimAttribute attribute = _attributes[entry.hudType];

            float curAlpha = attribute.AlphaCurve.Evaluate(duration);
            float curScale = attribute.ScaleCurve.Evaluate(duration);
            float curOffsetX = attribute.MoveXCurve.Evaluate(duration);
            float curOffsetY = attribute.MoveYCurve.Evaluate(duration);
            float oldAlpha = entry.alpha;
            float oldScale = entry.animScale;
            float oldMoveX = entry.move.x;
            float oldMoveY = entry.move.y;

            entry.alpha = curAlpha;
            entry.animScale = curScale;
            // 动画的位移处于屏幕空间下
            entry.move.x = curOffsetX * _ScreenScaleX;
            entry.move.y = curOffsetY * _ScreenScaleY;
            entry.stopped = duration > _maxAnimDuration;

            byte alpha = (byte)Mathf.Clamp((int)(curAlpha * 255f + 0.5f), 0, 255);

            bool dirty = true;
            // TODO: 这里暂时是每次都更新Mesh数据，之后可以根据动画是否变化来决定更新
            if (!dirty)
            {
                if (Mathf.Abs(oldAlpha - entry.alpha) > 0.0001f ||
                    Mathf.Abs(oldScale - entry.animScale) > 0.0001f ||
                    Mathf.Abs(oldMoveX - entry.move.x) > 0.0001f ||
                    Mathf.Abs(oldMoveY - entry.move.y) > 0.0001f)
                    dirty = true;
            }

            if (!dirty)
                return;
            
            // 更新顶点数据
            for (int i = entry.spriteList.size - 1; i >= 0; i--)
            {
                HUDSprite sprite = entry.spriteList[i];
                sprite.move = entry.move;
                sprite.positionWS = entry.positionWS;
                sprite.scale = curScale * entry.scale;
                sprite.colorLT.a = alpha;
                sprite.colorLB.a = alpha;
                sprite.colorRT.a = alpha;
                sprite.colorRB.a = alpha;
                sprite.parentMesh.SetDirty();
            }
        }

        /// <summary>
        /// 根据屏幕分辨率和UI分辨率计算出缩放比
        /// </summary>
        private void CalcScreenScale()
        {
            _ScreenScaleX = (float)Screen.width / HUDConst.HUD_UI_RESOLUTION_WIDTH;
            _ScreenScaleY = (float)Screen.height / HUDConst.HUD_UI_RESOLUTION_HEIGHT;
        }

        /// <summary>
        /// 帧循环更新逻辑，需要注册到HUDRenderManager
        /// </summary>
        protected void UpdateLogic(ScriptableRenderContext context, Camera camera)
        {
            CalcScreenScale();

            HUDAnimEntry curEntry = _pValidEntry;
            HUDAnimEntry lastEntry = _pValidEntry;
            while (curEntry != null)
            {
                PlayAnimation(curEntry);
                // 清理掉执行完动画的HUD
                if (curEntry.stopped)
                {
                    HUDAnimEntry needDeleteEntry = curEntry;
                    if (curEntry == _pValidEntry)
                    {
                        _pValidEntry = _pValidEntry.pNext;
                        lastEntry = _pValidEntry;
                    }
                    else
                    {
                        lastEntry.pNext = curEntry.pNext;
                    }

                    curEntry = curEntry.pNext;
                    ReleaseHUDAnimEntryAndSprite(needDeleteEntry);
                    continue;
                }

                lastEntry = curEntry;
                curEntry = curEntry.pNext;
            }

            if (_pValidEntry == null)
                _isDirty = true;
            
            // 执行渲染
            Camera hudCamera = HUDMesh.GetHUDCamera();
            if (hudCamera == null || hudCamera != camera)
                return;
            
            // 填充Mesh数据
            FillMesh();

            if (_isDirty)
            {
                // Mesh发生修改，重新生成绘制命令
                _addCommandBuffer = false;
                _cmd.Clear();
                Render(_cmd);
                if (_cmd.sizeInBytes > 0)
                {
                    context.ExecuteCommandBuffer(_cmd);
                    _addCommandBuffer = true;
                }
            }
            else
            {
                if (_addCommandBuffer)
                    context.ExecuteCommandBuffer(_cmd);
            }
            
            // 如果当前已经没有HUD需要渲染，那么就将帧循环逻辑移除
            if (_pValidEntry == null)
            {
                if (_lastExecuteUpdateLogicTime + 5.0f < Time.time)
                {
                    _updateLogicAdded = false;
                    HUDRenderManager.Instance.RemoveUpdateFunc(UpdateLogic);
                }

                ClearAllGeometry();
            }
            else
            {
                _lastExecuteUpdateLogicTime = Time.time;
            }
        }

        public override void Release()
        {
            while (_pValidEntry != null)
            {
                HUDAnimEntry entry = _pValidEntry;
                _pValidEntry = _pValidEntry.pNext;
                ReleaseHUDAnimEntryAndSprite(entry);
            }

            ReleaseCommandBuffer();
            base.Release();
        }

        private void ReleaseHUDAnimEntryAndSprite(HUDAnimEntry entry)
        {
            if (entry != null)
            {
                entry.ReleaseSprite();
                ReleaseHUDAnimEntry(entry);
            }
        }

        /// <summary>
        /// 清除掉当前所有的动画HUD
        /// </summary>
        protected void ClearCurrentHUDAnimEntry()
        {
            while (_pValidEntry != null)
            {
                HUDAnimEntry entry = _pValidEntry;
                _pValidEntry = _pValidEntry.pNext;
                entry.pNext = null;
                ReleaseHUDAnimEntryAndSprite(entry);
            }

            _isDirty = true;
        }

        private void ReleaseHUDAnimEntry(HUDAnimEntry entry)
        {
            if (entry != null)
            {
                entry.pNext = _pInvalidEntry;
                _pInvalidEntry = entry;
            }
        }

        private void ClearAllGeometry()
        {
            FastClearGeometry();
            ReleaseCommandBuffer();
        }

        private void ReleaseCommandBuffer()
        {
            if (_addCommandBuffer)
            {
                _addCommandBuffer = false;
                _cmd.Clear();
            }
        }
    }
}