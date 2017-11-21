namespace GridAiGames
{
    public abstract class GameObject<PlayerType, PlayerActionType> : IReadOnlyGameObject
        where PlayerType : IPlayer
    {
        public Position Position { get; internal set; }

        public GameObject(Position position)
        {
            Position = position;
        }

        /// <summary>
        /// Should not affect other object states.
        /// </summary>
        public abstract void Update(IGameGrid<PlayerType, PlayerActionType> gameGrid, ulong iteration);
    }
}