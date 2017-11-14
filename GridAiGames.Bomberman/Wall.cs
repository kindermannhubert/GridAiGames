namespace GridAiGames.Bomberman
{
    internal class Wall : GameObject<Player, PlayerAction>
    {
        public bool IsDestroyable { get; }

        public Wall(Position position, bool isDestroyable = false)
            : base(position)
        {
            this.IsDestroyable = isDestroyable;
        }

        public override void Update(IGameGrid<Player, PlayerAction> gameGrid, ulong iteration)
        {
        }
    }
}