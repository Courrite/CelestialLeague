namespace CelestialLeague.Shared.Enum
{
    public enum AnnouncementPriority
    {
        Low = 1, // small notif
        Normal = 2, // standard popup/banner
        High = 3, // prominent display
    }

    public enum AnnouncementType
    {
        General = 0, // blue bg, info icon
        Maintenance = 1, // orange bg, wrench icon
        Update = 2, // green bg, download icon
        Event = 3, // purple bg, star icon
        Warning = 4, // red bg, warning icon
    }
}