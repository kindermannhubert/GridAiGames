namespace GridAiGames
{
    public abstract class ReadOnlyGameObject
    {
        public Position Position { get; }

        public ReadOnlyGameObject(Position position)
        {
            Position = position;
        }
    }
}