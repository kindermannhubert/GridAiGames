using System.Diagnostics;

namespace GridAiGames
{
    public struct PlayerActionExtended<PlayerActionType>
    {
        public readonly PlayerActionType Action;
        public readonly bool DisqualifyPlayer;

        public PlayerActionExtended(PlayerActionType action)
        {
            Action = action;
            DisqualifyPlayer = false;
        }

        public PlayerActionExtended(bool disqualifyPlayer)
        {
            Debug.Assert(disqualifyPlayer);

            Action = default;
            DisqualifyPlayer = disqualifyPlayer;
        }
    }
}
