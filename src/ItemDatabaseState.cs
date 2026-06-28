using System;

namespace ItemDatabaseExtractor
{
    internal static class ItemDatabaseState
    {
        internal static int LastCount { get; set; }
        internal static string LastMessage { get; set; } = "Нажмите «Подсчитать предметы» (нужно главное меню / убежище после загрузки профиля).";
        internal static string LastError { get; set; }
        internal static bool FactoryReady { get; set; }
        internal static DateTime? LastCountUtc { get; set; }
        internal static DateTime? LastScanCompleteUtc { get; set; }
        internal static string LastOutputPath { get; set; }

        internal static void SetError(string message)
        {
            LastError = message;
            LastMessage = message;
        }

        internal static void SetInfo(string message)
        {
            LastError = null;
            LastMessage = message;
        }
    }
}
