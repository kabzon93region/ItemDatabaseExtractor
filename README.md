# Item Database Extractor

**GitHub:** [kabzon93region](https://github.com/kabzon93region)

Клиентский мод для SPT 4: извлекает **все шаблоны предметов** из `ItemFactoryClass.ItemTemplates` с локализованными именами и полями шаблона.

## Зачем

- SPT `items.json` не содержит всех названий и полей.
- Сайты (tarkov.dev и т.п.) иногда неполные.
- Игра после загрузки профиля имеет полную базу — этот мод дампит её в JSON.

## Использование

1. Запустите SPT, дождитесь **главного меню или убежища** (профиль загружен, `ItemFactory` инициализирован).
2. **F12** → **Item Database Extractor**.
3. **Шаг 1** — «Подсчитать предметы» (быстро, без записи файлов).
4. **Шаг 2** — «Начать детальное сканирование» (экспорт с паузой между предметами).

## Выходные файлы

Папка по умолчанию: `BepInEx/ItemDatabase/`

| Файл | Содержимое |
|------|------------|
| `items_extracted.json` | Полный dump: id, ключи локализации, localizedName/ShortName/Description, все public поля шаблона |
| `items_index.json` | Лёгкий индекс для поиска по именам |
| `extraction_meta.json` | Версия мода, дата, количество предметов |

## Настройки

| Параметр | По умолчанию | Описание |
|----------|--------------|----------|
| `ScanDelaySeconds` | 0.05 | Пауза между предметами (сек) |
| `OutputPath` | `BepInEx/ItemDatabase` | Папка вывода |
| `VerboseLogging` | true | Лог на каждый предмет (иначе каждые 100) |

## Установка

`BepInEx/plugins/ItemDatabaseExtractor.dll` — только на **клиенте** (инструмент, не нужен на headless).

## Логи

`BepInEx/LogOutput.log` — префиксы `[COUNT]`, `[SCAN]`, `[ItemScanner]`, `[ItemDataExtractor]`.

## Поддержать проект

Разовый донат картой РФ, СБП, ЮMoney, VK Pay:  
**[DonationAlerts → kabzon93region](https://www.donationalerts.com/r/kabzon93region)**
