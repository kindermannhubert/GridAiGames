using System.Collections.Generic;

namespace GridAiGames
{
    public interface IIntelligence<ReadOnlyGameGridType, ReadOnlyPlayerType, PlayerActionType>
    {
        IEnumerable<(string playerName, PlayerActionType action)>
            GetActionsForTeam(
            ReadOnlyGameGridType gameGrid,
            IReadOnlyList<ReadOnlyPlayerType> teamPlayers,
            ulong iteration);
    }
}