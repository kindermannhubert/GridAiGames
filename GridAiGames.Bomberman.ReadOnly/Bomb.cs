namespace GridAiGames.Bomberman.ReadOnly
{
    public class Bomb : ReadOnlyGameObject
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
