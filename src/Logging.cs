using BepInEx.Logging;

namespace UltrakillBingoClient
{
    public static class Logging 
    {
        public static ManualLogSource UltrakillBingoLogger = Logger.CreateLogSource("UltraBINGO");

        public static void Debug(string text)
        {
            UltrakillBingoLogger.LogDebug(text);
        }
        
        public static void Message(string text)
        {
            UltrakillBingoLogger.LogMessage(text);
        }
        
        public static void Warn(string text)
        {
            UltrakillBingoLogger.LogWarning(text);
        }
        
        public static void Error(string text)
        {
            UltrakillBingoLogger.LogError(text);
        }
        
        public static void Fatal(string text)
        {
            UltrakillBingoLogger.LogFatal(text);
        }
        
        public static void Info(string text)
        {
            UltrakillBingoLogger.LogInfo(text);
        }

        
    }
}