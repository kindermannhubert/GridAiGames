using GridAiGames.Bomberman.ReadOnly;

namespace GridAiGames.Bomberman
{
    internal class Bomb : GameObject<Player, PlayerAction>, IBomb
    {
        private readonly Player owner;

        public int Radius { get; }
        public int DetonateAfter { get; private set; }

        public Bomb(Position position, int radius, int detonateAfter, Player owner)
            : base(position)
        {
            DetonateAfter = detonateAfter;
            Radius = radius;
            this.owner = owner;
        }

        public override void Update(IGameGrid<Player, PlayerAction> gameGrid, ulong iteration)
        {
            if (--DetonateAfter == 0)
            {
                Detonate(gameGrid);
            }
        }

        public void Detonate(IGameGrid<Player, PlayerAction> gameGrid)
        {
            gameGrid.RemoveObject(this);

            foreach (var detonatedPosition in gameGrid.EnumerateDetonationFirePositions(this))
            {
                if (detonatedPosition == Position) gameGrid.AddObject(new BombDetonationFire(Position, BombDetonationFireType.Center));
                else gameGrid.AddObject(new BombDetonationFire(detonatedPosition, Position.Y == detonatedPosition.Y ? BombDetonationFireType.Horizontal : BombDetonationFireType.Vertical));
            }

            owner?.BombDetonated();
        }
    }
}
