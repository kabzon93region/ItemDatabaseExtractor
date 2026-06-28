using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.IO;
using UnityEngine;

namespace ItemDatabaseExtractor
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public sealed class PluginCore : BaseUnityPlugin
    {
        public static PluginCore Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }

        internal ConfigEntry<float> ScanDelay { get; private set; }
        internal ConfigEntry<string> OutputPath { get; private set; }
        internal ConfigEntry<bool> VerboseLogging { get; private set; }

        public bool IsScanning => _isScanning;
        public int TotalItems => _totalItems;
        public int ScannedItems => _scannedItems;

        private bool _isScanning;
        private int _totalItems;
        private int _scannedItems;
        private float _nextScanTime;
        private ItemScanner _scanner;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            Log.LogInfo($"=== {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} ===");
            SetupConfiguration();
            Log.LogInfo("F12 -> Item Database Extractor -> кнопки «Подсчитать» / «Сканировать»");
            Log.LogInfo("Требуется: главное меню или убежище после загрузки профиля (ItemFactory инициализирован).");
        }

        private void SetupConfiguration()
        {
            VerboseLogging = Config.Bind(
                "Logging",
                "VerboseLogging",
                true,
                new ConfigDescription(
                    "Лог на каждый предмет при детальном сканировании (иначе каждые 100).",
                    null,
                    new ConfigurationManagerAttributes { Order = 100 }));

            ScanDelay = Config.Bind(
                "Scanning",
                "ScanDelaySeconds",
                0.05f,
                new ConfigDescription(
                    "Пауза между предметами при детальном сканировании (сек).",
                    new AcceptableValueRange<float>(0.01f, 1.0f),
                    new ConfigurationManagerAttributes { Order = 50 }));

            OutputPath = Config.Bind(
                "Scanning",
                "OutputPath",
                Path.Combine(BepInEx.Paths.BepInExRootPath, "ItemDatabase"),
                new ConfigDescription(
                    "Папка для JSON: items_extracted.json, items_index.json, extraction_meta.json",
                    null,
                    new ConfigurationManagerAttributes { Order = 40 }));

            Config.Bind(
                "Управление",
                "CountItemsButton",
                false,
                new ConfigDescription(
                    "Подсчитать количество шаблонов предметов в ItemFactory.",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        CustomDrawer = ItemScannerUi.DrawCountButton,
                        HideSettingName = true,
                        HideDefaultButton = true,
                        Order = 100
                    }));

            Config.Bind(
                "Управление",
                "ScanItemsButton",
                false,
                new ConfigDescription(
                    "Детальное сканирование и экспорт всех предметов в JSON.",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        CustomDrawer = ItemScannerUi.DrawScanButton,
                        HideSettingName = true,
                        HideDefaultButton = true,
                        Order = 90
                    }));

            Log.LogInfo($"[Config] VerboseLogging={VerboseLogging.Value}");
            Log.LogInfo($"[Config] ScanDelay={ScanDelay.Value}s");
            Log.LogInfo($"[Config] OutputPath={OutputPath.Value}");
        }

        private void Update()
        {
            if (_isScanning && Time.unscaledTime >= _nextScanTime)
            {
                ScanNextItem();
            }
        }

        public void CountItems()
        {
            Log.LogInfo("=== [COUNT] START ===");

            try
            {
                if (!ItemScanner.IsFactoryReady())
                {
                    var msg = "ItemFactory не готов. Зайдите в убежище / главное меню после загрузки профиля.";
                    ItemDatabaseState.SetError(msg);
                    Log.LogWarning($"[COUNT] {msg}");
                    return;
                }

                _scanner = new ItemScanner();
                var count = _scanner.CountItems();
                ItemDatabaseState.LastCount = count;
                ItemDatabaseState.LastCountUtc = DateTime.UtcNow;
                ItemDatabaseState.SetInfo($"Найдено предметов: {count} (UTC {ItemDatabaseState.LastCountUtc:HH:mm:ss})");
                Log.LogInfo($"=== [COUNT] COMPLETE: {count} templates ===");
            }
            catch (Exception ex)
            {
                ItemDatabaseState.SetError($"Ошибка подсчёта: {ex.Message}");
                Log.LogError($"[COUNT] {ex}");
            }
        }

        public void StartItemScan()
        {
            if (_isScanning)
            {
                Log.LogWarning("[SCAN] Already running");
                return;
            }

            Log.LogInfo("=== [SCAN] START ===");

            try
            {
                if (!ItemScanner.IsFactoryReady())
                {
                    var msg = "ItemFactory не готов. Сначала зайдите в убежище / главное меню.";
                    ItemDatabaseState.SetError(msg);
                    Log.LogWarning($"[SCAN] {msg}");
                    return;
                }

                _scanner = new ItemScanner();
                _totalItems = _scanner.CountItems();
                if (_totalItems <= 0)
                {
                    ItemDatabaseState.SetError("Шаблонов 0 — сначала нажмите «Подсчитать предметы».");
                    Log.LogWarning("[SCAN] No templates");
                    return;
                }

                _scannedItems = 0;
                _isScanning = true;
                _nextScanTime = Time.unscaledTime;

                if (!Directory.Exists(OutputPath.Value))
                {
                    Directory.CreateDirectory(OutputPath.Value);
                    Log.LogInfo($"[SCAN] Created directory: {OutputPath.Value}");
                }

                ItemDataExtractor.Initialize(OutputPath.Value);
                ItemDatabaseState.SetInfo($"Сканирование 0/{_totalItems}...");
                Log.LogInfo($"[SCAN] Total={_totalItems} delay={ScanDelay.Value}s out={OutputPath.Value}");
            }
            catch (Exception ex)
            {
                ItemDatabaseState.SetError($"Ошибка старта: {ex.Message}");
                Log.LogError($"[SCAN] {ex}");
                _isScanning = false;
            }
        }

        private void ScanNextItem()
        {
            if (_scannedItems >= _totalItems)
            {
                FinishScan();
                return;
            }

            try
            {
                var template = _scanner.GetItemAt(_scannedItems);
                if (template != null)
                {
                    ItemDataExtractor.ExtractAndSaveItem(template, VerboseLogging.Value);
                }
                else
                {
                    Log.LogWarning($"[SCAN] Skip null template index={_scannedItems}");
                }

                _scannedItems++;
                _nextScanTime = Time.unscaledTime + ScanDelay.Value;

                if (_scannedItems % 50 == 0 || _scannedItems == _totalItems)
                {
                    ItemDatabaseState.SetInfo($"Сканирование {_scannedItems}/{_totalItems}...");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"[SCAN] index={_scannedItems}: {ex}");
                _scannedItems++;
                _nextScanTime = Time.unscaledTime + ScanDelay.Value;
            }
        }

        private void FinishScan()
        {
            Log.LogInfo("=== [SCAN] FINISH ===");
            Log.LogInfo($"[SCAN] Processed {_scannedItems}/{_totalItems}");

            try
            {
                ItemDataExtractor.SaveAll(_totalItems);
                ItemDatabaseState.LastScanCompleteUtc = DateTime.UtcNow;
                ItemDatabaseState.SetInfo(
                    $"Готово: {_scannedItems} предметов → {OutputPath.Value} ({ItemDatabaseState.LastScanCompleteUtc:HH:mm:ss} UTC)");
            }
            catch (Exception ex)
            {
                ItemDatabaseState.SetError($"Сохранение failed: {ex.Message}");
                Log.LogError($"[SCAN] Save failed: {ex}");
            }

            _isScanning = false;
            _scannedItems = 0;
            _totalItems = 0;
        }
    }
}
