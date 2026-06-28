using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ItemDatabaseExtractor
{
    internal static class ItemDataExtractor
    {
        private static string _outputPath;
        private static string _fullFilePath;
        private static string _indexFilePath;
        private static string _metaFilePath;
        private static int _savedCount;
        private static Dictionary<string, JObject> _allItems;
        private static List<JObject> _indexRows;

        internal static void Initialize(string outputPath)
        {
            _outputPath = outputPath;
            _fullFilePath = Path.Combine(outputPath, "items_extracted.json");
            _indexFilePath = Path.Combine(outputPath, "items_index.json");
            _metaFilePath = Path.Combine(outputPath, "extraction_meta.json");
            _allItems = new Dictionary<string, JObject>();
            _indexRows = new List<JObject>();
            _savedCount = 0;

            PluginCore.Log.LogInfo($"[ItemDataExtractor] Initialize output={_outputPath}");
            PluginCore.Log.LogInfo($"[ItemDataExtractor] Full dump: {_fullFilePath}");
            PluginCore.Log.LogInfo($"[ItemDataExtractor] Index: {_indexFilePath}");
        }

        internal static void ExtractAndSaveItem(ItemTemplate template, bool verbose)
        {
            if (template == null)
            {
                PluginCore.Log.LogWarning("[ItemDataExtractor] Template is null, skip");
                return;
            }

            try
            {
                var itemId = template._id.ToString();
                var full = ExtractAllProperties(template);
                var indexRow = BuildIndexRow(template);

                _allItems[itemId] = full;
                _indexRows.Add(indexRow);
                _savedCount++;

                if (verbose || _savedCount <= 10 || _savedCount % 100 == 0)
                {
                    var name = indexRow["localizedName"]?.ToString();
                    if (string.IsNullOrEmpty(name))
                    {
                        name = template._name ?? template.Name ?? "?";
                    }

                    PluginCore.Log.LogInfo($"[ItemDataExtractor] #{_savedCount} id={itemId} name={name} type={template.GetType().Name}");
                }
            }
            catch (Exception ex)
            {
                PluginCore.Log.LogError($"[ItemDataExtractor] Extract failed id={template._id}: {ex}");
            }
        }

        private static JObject BuildIndexRow(ItemTemplate template)
        {
            ItemLocaleHelper.Resolve(template, out var locName, out var locShort, out var locDesc);

            return new JObject
            {
                ["id"] = template._id.ToString(),
                ["_name"] = template._name ?? string.Empty,
                ["_type"] = template._type.ToString(),
                ["parentId"] = template.ParentId?.ToString() ?? string.Empty,
                ["dotnetType"] = template.GetType().Name,
                ["nameKey"] = template.Name ?? string.Empty,
                ["shortNameKey"] = template.ShortName ?? string.Empty,
                ["nameLocalizationKey"] = template.NameLocalizationKey,
                ["shortNameLocalizationKey"] = template.ShortNameLocalizationKey,
                ["descriptionLocalizationKey"] = template.DescriptionLocalizationKey,
                ["localizedName"] = locName,
                ["localizedShortName"] = locShort,
                ["localizedDescription"] = locDesc
            };
        }

        private static JObject ExtractAllProperties(ItemTemplate template)
        {
            var result = BuildIndexRow(template);
            result["fullType"] = template.GetType().FullName;

            foreach (var prop in template.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    if (prop.GetIndexParameters().Length > 0)
                    {
                        continue;
                    }

                    if (result[prop.Name] != null)
                    {
                        continue;
                    }

                    var value = prop.GetValue(template);
                    if (value == null)
                    {
                        continue;
                    }

                    WriteToken(result, prop.Name, value, prop.PropertyType);
                }
                catch (Exception ex)
                {
                    PluginCore.Log.LogDebug($"[ItemDataExtractor] Property {prop.Name}: {ex.Message}");
                }
            }

            foreach (var field in template.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    if (result[field.Name] != null)
                    {
                        continue;
                    }

                    var value = field.GetValue(template);
                    if (value == null)
                    {
                        continue;
                    }

                    WriteToken(result, field.Name, value, field.FieldType);
                }
                catch (Exception ex)
                {
                    PluginCore.Log.LogDebug($"[ItemDataExtractor] Field {field.Name}: {ex.Message}");
                }
            }

            return result;
        }

        private static void WriteToken(JObject target, string name, object value, Type valueType)
        {
            if (IsSimpleType(valueType))
            {
                target[name] = JToken.FromObject(value);
                return;
            }

            if (value is Array)
            {
                target[name] = JArray.FromObject(value);
                return;
            }

            if (value is System.Collections.IDictionary)
            {
                target[name] = JObject.FromObject(value);
                return;
            }

            if (valueType.IsClass && valueType != typeof(string))
            {
                target[name] = ExtractNestedObject(value);
                return;
            }

            target[name] = JToken.FromObject(value);
        }

        private static JObject ExtractNestedObject(object obj)
        {
            var result = new JObject();
            if (obj == null)
            {
                return result;
            }

            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    if (prop.GetIndexParameters().Length > 0)
                    {
                        continue;
                    }

                    var value = prop.GetValue(obj);
                    if (value == null || !IsSimpleType(prop.PropertyType))
                    {
                        continue;
                    }

                    result[prop.Name] = JToken.FromObject(value);
                }
                catch
                {
                    // ignore nested errors
                }
            }

            return result;
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string)
                || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(Guid);
        }

        internal static void SaveAll(int expectedCount)
        {
            PluginCore.Log.LogInfo($"[ItemDataExtractor] SaveAll items={_allItems.Count} expected={expectedCount}");

            try
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                };

                File.WriteAllText(_fullFilePath, JsonConvert.SerializeObject(_allItems, settings), Encoding.UTF8);
                File.WriteAllText(_indexFilePath, JsonConvert.SerializeObject(_indexRows, settings), Encoding.UTF8);

                var meta = new JObject
                {
                    ["mod"] = PluginInfo.PLUGIN_NAME,
                    ["modVersion"] = PluginInfo.PLUGIN_VERSION,
                    ["extractedUtc"] = DateTime.UtcNow.ToString("o"),
                    ["itemCount"] = _allItems.Count,
                    ["expectedCount"] = expectedCount,
                    ["outputDirectory"] = _outputPath,
                    ["files"] = new JObject
                    {
                        ["full"] = Path.GetFileName(_fullFilePath),
                        ["index"] = Path.GetFileName(_indexFilePath)
                    }
                };
                File.WriteAllText(_metaFilePath, meta.ToString(Formatting.Indented), Encoding.UTF8);

                var fullSize = new FileInfo(_fullFilePath).Length;
                var indexSize = new FileInfo(_indexFilePath).Length;
                PluginCore.Log.LogInfo($"[ItemDataExtractor] Saved full: {_fullFilePath} ({fullSize:N0} bytes)");
                PluginCore.Log.LogInfo($"[ItemDataExtractor] Saved index: {_indexFilePath} ({indexSize:N0} bytes)");
                PluginCore.Log.LogInfo($"[ItemDataExtractor] Saved meta: {_metaFilePath}");

                ItemDatabaseState.LastOutputPath = _outputPath;
            }
            catch (Exception ex)
            {
                PluginCore.Log.LogError($"[ItemDataExtractor] SaveAll failed: {ex}");
                throw;
            }
            finally
            {
                _allItems?.Clear();
                _indexRows?.Clear();
                _savedCount = 0;
            }
        }
    }
}
