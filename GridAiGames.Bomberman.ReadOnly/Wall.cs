namespace GridAiGames.Bomberman.ReadOnly
{
    public interface IWall : IReadOnlyGameObject
    {
        bool IsDestroyable { get; }
    }

    public class Wall : ReadOnlyGameObject, IWall
    {
        public bool IsDestroyable { get; }

        public Wall(Position position, bool isDestroyable)
            : base(position)
        {
            IsDestroyable = isDestroyable;
        }
    }
}