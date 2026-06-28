# Changelog

## 1.1.0 — 2026-06-28

- Кнопки в **F12 Configuration Manager** (как Boss Spawn Control), без горячих клавиш.
- Шаг 1: подсчёт шаблонов в `ItemFactoryClass.ItemTemplates`.
- Шаг 2: детальный экспорт с задержкой 0.05 сек / предмет (настраивается).
- Локализованные имена через `MongoID.LocalizedName()` / `LocalizedShortName()` и ключи шаблона.
- Три файла: `items_extracted.json`, `items_index.json`, `extraction_meta.json`.
- Статус в UI: готовность Factory, прогресс, последний подсчёт.
- Подробное логирование (`VerboseLogging`, по умолчанию включено).

## 1.0.0

- Первый черновик (другой чат): базовая структура и экспорт через рефлексию.
