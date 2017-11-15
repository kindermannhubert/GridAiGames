using System.Collections.Generic;
using GridAiGames.Bomberman.ReadOnly;

namespace GridAiGames.Bomberman.Tests
{
    internal class ManualIntelligence : IBombermanIntelligence
    {
        private readonly Dictionary<string, PlayerAction> nextActions = new Dictionary<string, PlayerAction>();

        public void Initialize(ReadOnly.GameGrid gameGrid, IReadOnlyList<ReadOnly.Player> teamPlayers)
        {
        }

        public IEnumerable<(string playerName, PlayerAction action)>
            GetActionsForTeam(
                ReadOnly.GameGrid gameGrid,
                IReadOnlyList<ReadOnly.Player> teamPlayers)
        {
            foreach (var player in teamPlayers)
            {
                if (nextActions.ContainsKey(player.Name))
                {
                    yield return (player.Name, nextActions[player.Name]);
                }
                else
                {
                    yield return (player.Name, PlayerAction.None);
                }
            }
            nextActions.Clear();
        }

        public void SetNextActions(
            IReadOnlyGameGrid<Player, PlayerAction> gameGrid,
            params (string playerName, PlayerAction action)[] actions)
        {
            foreach (var item in actions)
            {
                nextActions.Add(item.playerName, item.action);
            }
        }
    }
}
