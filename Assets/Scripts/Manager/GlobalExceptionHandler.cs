using System;

public class GlobalExceptionHandler
{
    public static void Init()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            Exception ex = (Exception)args.ExceptionObject;
            UnityEngine.Debug.LogError($"Unhandled Exception: {ex.Message}");
            
            // Crashlytics に送信（必要なら）
            Firebase.Crashlytics.Crashlytics.LogException(ex);
        };
    }
}