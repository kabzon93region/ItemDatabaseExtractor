using BepInEx.Configuration;
using UnityEngine;

namespace ItemDatabaseExtractor
{
    internal static class ItemScannerUi
    {
        internal static void DrawStatusBlock()
        {
            var ready = ItemDatabaseState.FactoryReady || ItemScanner.IsFactoryReady();
            var readyText = ready ? "ItemFactory: готов" : "ItemFactory: НЕ готов (зайдите в убежище / меню)";
            GUILayout.Label(readyText);

            if (ItemDatabaseState.LastCount > 0)
            {
                GUILayout.Label($"Последний подсчёт: {ItemDatabaseState.LastCount} предметов");
            }

            if (!string.IsNullOrEmpty(ItemDatabaseState.LastMessage))
            {
                GUILayout.Label(ItemDatabaseState.LastMessage);
            }

            if (!string.IsNullOrEmpty(ItemDatabaseState.LastOutputPath))
            {
                GUILayout.Label($"Вывод: {ItemDatabaseState.LastOutputPath}");
            }
        }

        internal static void DrawCountButton(ConfigEntryBase entry)
        {
            var plugin = PluginCore.Instance;
            if (plugin == null)
            {
                GUILayout.Label("Item Database Extractor: plugin not ready");
                return;
            }

            DrawStatusBlock();

            GUILayout.Space(4f);
            GUILayout.Label("Шаг 1 — быстрый подсчёт шаблонов в ItemFactoryClass.ItemTemplates");

            GUI.enabled = !plugin.IsScanning;
            if (GUILayout.Button("Подсчитать предметы", GUILayout.Height(28f)))
            {
                plugin.CountItems();
            }

            GUI.enabled = true;
        }

        internal static void DrawScanButton(ConfigEntryBase entry)
        {
            var plugin = PluginCore.Instance;
            if (plugin == null)
            {
                return;
            }

            GUILayout.Space(8f);
            GUILayout.Label("Шаг 2 — детальный экспорт (локализация + все поля шаблона)");
            GUILayout.Label($"Задержка: {plugin.ScanDelay.Value:F2} сек / предмет");
            GUILayout.Label("Файлы: items_extracted.json, items_index.json, extraction_meta.json");

            if (plugin.IsScanning)
            {
                GUILayout.Label($"Идёт сканирование: {plugin.ScannedItems} / {plugin.TotalItems}");
                GUILayout.Label("Не закрывайте игру до завершения.");
                return;
            }

            GUI.enabled = ItemScanner.IsFactoryReady();
            if (GUILayout.Button("Начать детальное сканирование", GUILayout.Height(28f)))
            {
                plugin.StartItemScan();
            }

            GUI.enabled = true;

            if (!ItemScanner.IsFactoryReady())
            {
                GUILayout.Label("Сначала дождитесь готовности ItemFactory (убежище / меню).");
            }
        }
    }
}
