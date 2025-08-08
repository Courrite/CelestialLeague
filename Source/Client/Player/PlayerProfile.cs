using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CelestialLeague.Shared.Models;
using CelestialLeague.Shared.Enums;

namespace CelestialLeague.Client.Player
{
    public class PlayerProfile : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isHighlighted;
        private string _statusMessage = string.Empty;

        public PlayerInfo PlayerInfo { get; private set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => SetProperty(ref _isHighlighted, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value ?? string.Empty);
        }

        public int PlayerId => PlayerInfo?.Id ?? 0;
        public string Username => PlayerInfo?.Username ?? "Unknown";
        public DateTime CreatedAt => PlayerInfo?.CreatedAt ?? DateTime.MinValue;
        public DateTime LastSeen => PlayerInfo?.LastSeen ?? DateTime.MinValue;
        public PlayerStatus Status => PlayerInfo?.PlayerStatus ?? PlayerStatus.Offline;
        public UserRole Role => PlayerInfo?.UserRole ?? UserRole.None;

        // Calculated properties
        public string StatusDisplayText => GetStatusDisplayText();
        public string RoleDisplayText => GetRoleDisplayText();
        public TimeSpan TimeSinceLastSeen => DateTime.UtcNow - LastSeen;
        public string LastSeenText => GetLastSeenText();

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateFromPlayerInfo(PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;

            OnPropertyChanged(nameof(PlayerId));
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(CreatedAt));
            OnPropertyChanged(nameof(LastSeen));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Role));
            OnPropertyChanged(nameof(StatusDisplayText));
            OnPropertyChanged(nameof(RoleDisplayText));
            OnPropertyChanged(nameof(TimeSinceLastSeen));
            OnPropertyChanged(nameof(LastSeenText));

            UpdateStatusMessage();
        }

        private void UpdateStatusMessage()
        {
            StatusMessage = GetStatusDisplayText();
        }

        private string GetStatusDisplayText()
        {
            return Status switch
            {
                PlayerStatus.Online => "Online",
                PlayerStatus.Playing => "In Game",
                PlayerStatus.InQueue => "In Queue",
                PlayerStatus.Offline => "Offline",
                _ => "Unknown"
            };
        }

        private string GetRoleDisplayText()
        {
            return Role switch
            {
                UserRole.None => "Player",
                UserRole.Admin => "Administrator",
                UserRole.Moderator => "Moderator",
                _ => "Unknown"
            };
        }

        private string GetLastSeenText()
        {
            if (Status == PlayerStatus.Online)
                return "Online now";

            var timeSpan = TimeSinceLastSeen;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";

            return LastSeen.ToString("MMM dd, yyyy");
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public override string ToString()
        {
            return $"{Username} ({GetStatusDisplayText()})";
        }
    }
}