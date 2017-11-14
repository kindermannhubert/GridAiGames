namespace GridAiGames.Bomberman
{
    internal class Bonus : GameObject<Player, PlayerAction>
    {
        public BonusType Type { get; }

        public Bonus(Position position, BonusType type)
            : base(position)
        {
            Type = type;
        }

        public override void Update(IGameGrid<Player, PlayerAction> gameGrid, ulong iteration)
        {
        }
    }
}
