namespace GridAiGames.Bomberman.ReadOnly
{
    public class Wall : ReadOnlyGameObject
    {
        public bool IsDestroyable { get; }

        public Wall(Position position, bool isDestroyable)
            : base(position)
        {
            IsDestroyable = isDestroyable;
        }
    }
}