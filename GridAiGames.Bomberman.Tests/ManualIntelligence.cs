﻿using System.Collections.Generic;
using GridAiGames.Bomberman.ReadOnly;

namespace GridAiGames.Bomberman.Tests
{
    internal class ManualIntelligence : IBombermanIntelligence
    {
        private readonly Dictionary<string, List<PlayerAction>> nextActions = new Dictionary<string, List<PlayerAction>>();

        public void Initialize(ReadOnly.GameGrid gameGrid, IReadOnlyList<ReadOnly.IPlayer> teamPlayers)
        {
        }

        public IEnumerable<(string playerName, PlayerAction action)>
            GetActionsForTeam(
                ReadOnly.GameGrid gameGrid,
                IReadOnlyList<ReadOnly.IPlayer> teamPlayers)
        {
            foreach (var player in teamPlayers)
            {
                if (nextActions.ContainsKey(player.Name))
                {
                    foreach (var action in nextActions[player.Name])
                    {
                        yield return (player.Name, action);
                    }
                }
                else
                {
                    yield return (player.Name, PlayerAction.None);
                }
            }
            nextActions.Clear();
        }

        public void SetNextActions(
            IReadOnlyGameGrid gameGrid,
            params (string playerName, PlayerAction action)[] actions)
        {
            foreach (var item in actions)
            {
                if (!nextActions.TryGetValue(item.playerName, out var list))
                {
                    list = new List<PlayerAction>();
                    nextActions.Add(item.playerName, list);
                }

                list.Add(item.action);
            }
        }
    }
}
