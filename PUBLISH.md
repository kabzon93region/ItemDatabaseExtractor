# Publish to GitHub — Item Database Extractor

**Статус:** `ready`  
**GitHub:** Release + zip  
**Версия:** `1.1.0`  
**Deployment:** `(client_only)`

## 1. Подготовка (уже сделано этим скриптом)

Папка: `github-repos/ItemDatabaseExtractor/`

## 2. Создать репозиторий и запушить

```powershell
cd github-repos/ItemDatabaseExtractor
git init
git add .
git commit -m "Source backup Item Database Extractor v1.1.0"
git branch -M main
git remote add origin https://github.com/kabzon93region/ItemDatabaseExtractor.git
git push -u origin main
```

Или автоматически:

```powershell
python CURSORAIMODING/tools/publish/publish_github_release.py ItemDatabaseExtractor --create-repo
```

## 3. GitHub Release

Прикрепить zip (только игровые файлы, без INSTALL.md):

`\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\ItemDatabaseExtractor_(client_only)_v1.1.0_2026-06-28.zip`

```powershell
gh release create v1.1.0 "\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\ItemDatabaseExtractor_(client_only)_v1.1.0_2026-06-28.zip" ^
  --title "Item Database Extractor v1.1.0" ^
  --notes-file CHANGELOG.md
```

## Описание репозитория (suggested)

Экспорт всех шаблонов предметов из ItemFactory в JSON с локализованными именами (F12, без hotkeys).

SPT 4.0 + Fika 2.3 headless stack. Deployment: `(client_only)`.
