using System.Collections.Generic;
using GridAiGames.Bomberman.ReadOnly;

namespace GridAiGames.Bomberman
{
    internal class DummyIntelligence : IBombermanIntelligence
    {
        public void Initialize(ReadOnly.GameGrid gameGrid, IReadOnlyList<ReadOnly.Player> teamPlayers)
        {
        }

        public IEnumerable<(string playerName, PlayerAction action)>
            GetActionsForTeam(
                ReadOnly.GameGrid gameGrid,
                IReadOnlyList<ReadOnly.Player> teamPlayers)
        {
            yield break;
        }
    }
}
