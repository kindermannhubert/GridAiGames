namespace GridAiGames.Bomberman.ReadOnly
{
    public class Bonus : ReadOnlyGameObject
    {
        public BonusType Type { get; }

        public Bonus(Position position, BonusType type)
            : base(position)
        {
            Type = type;
        }
    }
}
