using EFT;
using EFT.InventoryLogic;

namespace ItemDatabaseExtractor
{
    internal static class ItemLocaleHelper
    {
        internal static void Resolve(ItemTemplate template, out string localizedName, out string localizedShortName, out string localizedDescription)
        {
            localizedName = string.Empty;
            localizedShortName = string.Empty;
            localizedDescription = string.Empty;

            if (template == null)
            {
                return;
            }

            try
            {
                MongoID mongoId = template._id.ToString();
                localizedName = mongoId.LocalizedName();
                localizedShortName = mongoId.LocalizedShortName();
            }
            catch
            {
                // locale not loaded yet
            }

            try
            {
                localizedDescription = template.DescriptionLocalizationKey.Localized();
            }
            catch
            {
                // ignore
            }

            if (IsBadLocalized(localizedName, template.NameLocalizationKey))
            {
                localizedName = string.Empty;
            }

            if (IsBadLocalized(localizedShortName, template.ShortNameLocalizationKey))
            {
                localizedShortName = string.Empty;
            }

            if (IsBadLocalized(localizedDescription, template.DescriptionLocalizationKey))
            {
                localizedDescription = string.Empty;
            }
        }

        private static bool IsBadLocalized(string value, string key)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            if (string.Equals(value, key, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return value.EndsWith(" Name", System.StringComparison.Ordinal)
                || value.EndsWith(" ShortName", System.StringComparison.Ordinal)
                || value.EndsWith(" Description", System.StringComparison.Ordinal);
        }
    }
}
