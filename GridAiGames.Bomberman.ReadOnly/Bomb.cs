namespace GridAiGames.Bomberman.ReadOnly
{
    public interface IBomb : IReadOnlyGameObject
    {
        int Radius { get; }
        int DetonateAfter { get; }
    }

    public class Bomb : ReadOnlyGameObject, IBomb
    {
        public int Radius { get; }
        public int DetonateAfter { get; private set; }

        public Bomb(Position position, int radius, int detonateAfter)
            : base(position)
        {
            DetonateAfter = detonateAfter;
            Radius = radius;
        }
    }
}
