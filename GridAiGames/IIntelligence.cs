using System.Collections.Generic;

namespace GridAiGames
{
    public interface IIntelligence<ReadOnlyGameGridType, ReadOnlyPlayerType, PlayerActionType>
    {
        void Initialize(ReadOnlyGameGridType gameGrid, IReadOnlyList<ReadOnlyPlayerType> teamPlayers);

        IEnumerable<(string playerName, PlayerActionType action)>
            GetActionsForTeam(
            ReadOnlyGameGridType gameGrid,
            IReadOnlyList<ReadOnlyPlayerType> teamPlayers,
            ulong iteration);
    }
}