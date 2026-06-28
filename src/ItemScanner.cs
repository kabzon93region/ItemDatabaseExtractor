using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;

namespace ItemDatabaseExtractor
{
    /// <summary>
    /// Читает ItemFactoryClass.ItemTemplates (GClass1408 = Dictionary&lt;MongoID, ItemTemplate&gt;).
    /// </summary>
    internal sealed class ItemScanner
    {
        private readonly List<string> _itemIds = new List<string>();
        private GClass1408 _templates;

        internal static bool IsFactoryReady()
        {
            try
            {
                if (!Singleton<ItemFactoryClass>.Instantiated)
                {
                    ItemDatabaseState.FactoryReady = false;
                    return false;
                }

                var factory = Singleton<ItemFactoryClass>.Instance;
                var ready = factory?.ItemTemplates != null && factory.ItemTemplates.Count > 0;
                ItemDatabaseState.FactoryReady = ready;
                return ready;
            }
            catch (Exception ex)
            {
                PluginCore.Log.LogWarning($"[ItemScanner] IsFactoryReady failed: {ex.Message}");
                ItemDatabaseState.FactoryReady = false;
                return false;
            }
        }

        internal void Reload()
        {
            PluginCore.Log.LogInfo("[ItemScanner] Reload() — loading ItemTemplates...");

            if (!IsFactoryReady())
            {
                throw new InvalidOperationException(
                    "ItemFactoryClass не готов. Зайдите в убежище или главное меню после загрузки профиля SPT.");
            }

            var factory = Singleton<ItemFactoryClass>.Instance;
            _templates = factory.ItemTemplates;

            _itemIds.Clear();
            foreach (var key in _templates.Keys)
            {
                _itemIds.Add(key.ToString());
            }

            PluginCore.Log.LogInfo($"[ItemScanner] Loaded {_templates.Count} templates, {_itemIds.Count} ids");
        }

        internal int CountItems()
        {
            Reload();
            return _itemIds.Count;
        }

        internal ItemTemplate GetItemAt(int index)
        {
            if (_templates == null || _itemIds.Count == 0)
            {
                PluginCore.Log.LogError("[ItemScanner] Templates not loaded — call Reload/CountItems first");
                return null;
            }

            if (index < 0 || index >= _itemIds.Count)
            {
                PluginCore.Log.LogError($"[ItemScanner] Index {index} out of range [0, {_itemIds.Count})");
                return null;
            }

            var itemId = _itemIds[index];
            if (!_templates.TryGetValue(itemId, out var template))
            {
                PluginCore.Log.LogWarning($"[ItemScanner] Template missing for id={itemId}");
                return null;
            }

            return template;
        }

        internal IEnumerable<string> GetAllItemIds()
        {
            return _itemIds;
        }
    }
}
