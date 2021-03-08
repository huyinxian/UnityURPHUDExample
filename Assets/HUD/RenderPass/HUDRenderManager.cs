using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HUD
{
    public class HUDRenderManager
    {
        public class UpdateEntry
        {
            public OnUpdate func;
            public bool enabled;
        }

        public delegate void OnUpdate(ScriptableRenderContext context, Camera camera);
        private readonly List<UpdateEntry> _updateEntryList = new List<UpdateEntry>();

        private HUDNumberRenderer _numberRenderer;

        private static HUDRenderManager _instance;
        public static HUDRenderManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HUDRenderManager();
                }

                return _instance;
            }
        }

        public void AddUpdateFunc(OnUpdate func)
        {
            foreach (var entry in _updateEntryList)
            {
                if (entry.func == func)
                {
                    entry.enabled = true;
                    return;
                }
            }

            UpdateEntry newEntry = new UpdateEntry()
            {
                func = func,
                enabled = true
            };
            _updateEntryList.Add(newEntry);
        }

        public void RemoveUpdateFunc(OnUpdate func)
        {
            foreach (var entry in _updateEntryList)
            {
                if (entry.func == func)
                {
                    // 该方法可能会在HUDRenderPass的循环中调用，这里不要做删除，仅进行标记
                    entry.enabled = false;
                }
            }
        }

        public void RemoveAllUpdateFunc()
        {
            _updateEntryList.Clear();
        }

        public List<UpdateEntry> GetUpdateEntryList()
        {
            return _updateEntryList;
        }

        public void AddHUDNumber(Vector3 position, int hudType, int number, bool showHead, bool showAdd, bool showSub)
        {
            if (_numberRenderer == null)
            {
                _numberRenderer = new HUDNumberRenderer();
                _numberRenderer.InitSetting(HUDSettings.Instance);
            }

            _numberRenderer.AddHUDNumber(position, hudType, number, showHead, showAdd, showSub);
        }
    }
}