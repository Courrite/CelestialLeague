public static class ChatConstants
{
    // message limtis
    public const int MaxMessageLength = 500;
    public const int MaxPrivateMessageLength = 1000;
    public const int MinimumMessageLength = 1;
    public const int MaxMessagesPerMinute = 50;

    // default channels
    public const string GlobalChannel = "global";
    public const string LobbyChannel = "lobby";
    public const string MatchChannelPrefix = "match_";

    // filtering
    public static readonly string[] ProfanityList = {
        "badword1",
        "badword2"
    };
}