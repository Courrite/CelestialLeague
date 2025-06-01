public static class UIConstants
{
    // dimensions
    public const int MinScreenWidth = 1280;
    public const int MinScreenHeight = 720;
    public const float DefaultUIScale = 1.0f;
    public const float MinUIScale = 0.5f;
    public const float MaxUIScale = 2.0f;
    
    // animation timings
    public const float FastTransitionDuration = 0.15f; // seconds
    public const float NormalTransitionDuration = 0.3f;
    public const float SlowTransitionDuration = 0.5f;
    
    // colors
    public const string PrimaryColor = "#4A90E2";
    public const string SecondaryColor = "#7ED321";
    public const string ErrorColor = "#D0021B";
    public const string WarningColor = "#F5A623";
    public const string SuccessColor = "#50E3C2";
    
    // font sizes
    public const int SmallFontSize = 12;
    public const int NormalFontSize = 16;
    public const int LargeFontSize = 24;
    public const int HeaderFontSize = 32;
    
    // spacing
    public const int SmallPadding = 4;
    public const int NormalPadding = 8;
    public const int LargePadding = 16;
    public const int ExtraLargePadding = 32;
    
    // component sizes
    public const int ButtonHeight = 40;
    public const int InputFieldHeight = 36;
    public const int ScrollbarWidth = 16;
    
    // chat
    public const int MaxChatMessages = 100;
    public const int ChatFadeTimeSeconds = 10;
    
    // notifs
    public const int NotificationDisplayTimeSeconds = 5;
    public const int MaxNotifications = 5;
    
    // performance
    public const int TargetFPS = 60;
    public const int MaxUIUpdatesPerFrame = 10;
}
