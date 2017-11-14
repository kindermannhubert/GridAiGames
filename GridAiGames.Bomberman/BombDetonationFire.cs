namespace GridAiGames.Bomberman
{
    internal class BombDetonationFire : GameObject<Player, PlayerAction>
    {
        private int DisappearAfter;
        private bool isBurning;

        public bool IsBurning => isBurning;
        public BombDetonationFireType Type { get; }

        public BombDetonationFire(Position position, BombDetonationFireType type)
            : base(position)
        {
            DisappearAfter = 2;
            isBurning = true;
            Type = type;
        }

        public override void Update(IGameGrid<Player, PlayerAction> gameGrid, ulong iteration)
        {
            isBurning = false;

            if (--DisappearAfter == 0)
            {
                gameGrid.RemoveObject(this);
            }
        }
    }

    internal enum BombDetonationFireType
    {
        Horizontal,
        Vertical,
        Center
    }
}
