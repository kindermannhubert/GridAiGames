namespace GridAiGames
{
    public abstract class ReadOnlyGameObject : IReadOnlyGameObject
    {
        public Position Position { get; }

        public ReadOnlyGameObject(Position position)
        {
            Position = position;
        }
    }
}