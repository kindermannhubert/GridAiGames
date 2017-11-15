using System.Collections.Generic;

namespace GridAiGames.Bomberman
{
    internal class DummyIntelligence : IIntelligence<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>
    {
        public void Initialize(ReadOnly.GameGrid gameGrid, IReadOnlyList<ReadOnly.Player> teamPlayers)
        {
        }

        public IEnumerable<(string playerName, PlayerAction action)>
            GetActionsForTeam(
                ReadOnly.GameGrid gameGrid,
                IReadOnlyList<ReadOnly.Player> teamPlayers,
                ulong iteration)
        {
            yield break;
        }
    }
}
