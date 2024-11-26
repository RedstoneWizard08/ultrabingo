using BepInEx.Logging;

namespace UltrakillBingoClient
{
    public static class Logging 
    {
        public static ManualLogSource BingoLogger = Logger.CreateLogSource("Baphomet's BINGO");

        public static void Debug(string text)
        {
            BingoLogger.LogDebug(text);
        }
        
        public static void Message(string text)
        {
            BingoLogger.LogMessage(text);
        }
        
        public static void Warn(string text)
        {
            BingoLogger.LogWarning(text);
        }
        
        public static void Error(string text)
        {
            BingoLogger.LogError(text);
        }
        
        public static void Fatal(string text)
        {
            BingoLogger.LogFatal(text);
        }
        
        public static void Info(string text)
        {
            BingoLogger.LogInfo(text);
        }
    }
}