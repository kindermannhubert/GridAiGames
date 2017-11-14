using System.Collections.Generic;

namespace GridAiGames
{
    public class TeamDefinition<ReadOnlyGameGridType, ReadOnlyPlayerType, PlayerActionType>
    {
        public string Name { get; }
        public IReadOnlyList<PlayerDefinition> Players { get; }
        public IIntelligence<ReadOnlyGameGridType, ReadOnlyPlayerType, PlayerActionType> Intelligence { get; }

        public TeamDefinition(string name, IReadOnlyList<PlayerDefinition> players, IIntelligence<ReadOnlyGameGridType, ReadOnlyPlayerType, PlayerActionType> intelligence)
        {
            Name = name;
            Players = players;
            Intelligence = intelligence;
        }
    }
}