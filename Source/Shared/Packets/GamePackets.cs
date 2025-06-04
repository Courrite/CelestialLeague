using CelestialLeague.Shared.Enums;
using CelestialLeague.Shared.Models;

namespace CelestialLeague.Shared.Packets
{
    public class PlayerPositionPacket : BasePacket
    {
        public override PacketType Type => PacketType.PlayerPosition;
        public required string SessionToken { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public byte StateFlags { get; set; }

        public PlayerPositionPacket(string sessionToken, float x, float y)
        {
            SessionToken = sessionToken;
            X = x;
            Y = y;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SessionToken);
        }
    }

    public class GameStatePacket : BasePacket
    {
        public override PacketType Type => PacketType.GameState;
        public GameState State { get; set; }
        public Dictionary<string, object> StateData { get; set; } = new();

        public GameStatePacket(GameState state)
        {
            State = state;
        }

        public override bool IsValid()
        {
            return Enum.IsDefined(typeof(GameState), State);
        }
    }

    public class GameEventPacket : BasePacket
    {
        public override PacketType Type => PacketType.GameEvent;
        public required string SessionToken { get; set; }
        public EventType EventType { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int TimeMs { get; set; }
        public Dictionary<string, object> EventData { get; set; } = new();

        public GameEventPacket(string sessionToken, EventType eventType)
        {
            SessionToken = sessionToken;
            EventType = eventType;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SessionToken) &&
                   Enum.IsDefined(typeof(EventType), EventType);
        }
    }

    public class MatchResultPacket : BasePacket
    {
        public override PacketType Type => PacketType.MatchResult;
        public required string MatchId { get; set; }
        public MatchResult Result { get; set; }
        public Dictionary<string, PlayerMatchStats> PlayerStats { get; set; } = new();
        public string? WinnerUsername { get; set; }
        public int MatchDurationMs { get; set; }

        public MatchResultPacket(string matchId, MatchResult result)
        {
            MatchId = matchId;
            Result = result;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(MatchId) &&
                   Enum.IsDefined(typeof(MatchResult), Result);
        }
    }

    public class JoinGameRequestPacket : BasePacket
    {
        public override PacketType Type => PacketType.JoinGameRequest;
        public required string SessionToken { get; set; }
        public required string MatchId { get; set; }
        public GameRole JoinAs { get; set; } = GameRole.Player;

        public JoinGameRequestPacket(string sessionToken, string matchId, GameRole joinAs = GameRole.Player)
        {
            SessionToken = sessionToken;
            MatchId = matchId;
            JoinAs = joinAs;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(SessionToken) &&
                   !string.IsNullOrWhiteSpace(MatchId) &&
                   Enum.IsDefined(typeof(GameRole), JoinAs);
        }
    }

    public class JoinGameResponsePacket : BaseResponse
    {
        public override PacketType Type => PacketType.JoinGameResponse;
        public string? MatchId { get; set; }
        public string? ServerEndpoint { get; set; }
        public GameRole AssignedRole { get; set; }
        public Dictionary<string, object> GameInfo { get; set; } = new();

        public JoinGameResponsePacket(bool success = true)
        {
            Success = success;
        }

        protected override bool ValidateSuccessResponse()
        {
            return !string.IsNullOrWhiteSpace(MatchId) &&
                   Enum.IsDefined(typeof(GameRole), AssignedRole);
        }
    }

    public class GamePausePacket : BasePacket
    {
        public override PacketType Type => PacketType.GamePause;
        public required string SessionToken { get; set; }
        public bool Pause { get; set; }

        public GamePausePacket(string sessionToken, bool pause)
        {
            SessionToken = sessionToken;
            Pause = pause;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SessionToken);
        }
    }

    public class MatchStateChangePacket : BasePacket
    {
        public override PacketType Type => PacketType.MatchStateChange;
        public required string MatchId { get; set; }
        public MatchState State { get; set; }

        public MatchStateChangePacket(string matchId, MatchState state)
        {
            MatchId = matchId;
            State = state;
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(MatchId) &&
                   Enum.IsDefined(typeof(MatchState), State);
        }
    }

    public class PlayerStateChangePacket : BasePacket
    {
        public override PacketType Type => PacketType.PlayerStateChange;
        public required string SessionToken { get; set; }
        public required string MatchId { get; set; }
        public required int PlayerId { get; set; }
        public PlayerMatchState State { get; set; }
        public Dictionary<string, object> StateData { get; set; } = new();

        public PlayerStateChangePacket(string sessionToken, string matchId, int playerId, PlayerMatchState state)
        {
            SessionToken = sessionToken;
            MatchId = matchId;
            PlayerId = playerId;
            State = state;
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(SessionToken) &&
                   !string.IsNullOrWhiteSpace(MatchId) &&
                   PlayerId > 0 &&
                   Enum.IsDefined(typeof(PlayerMatchState), State);
        }
    }
}