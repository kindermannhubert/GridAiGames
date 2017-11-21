namespace GridAiGames.Bomberman.ReadOnly
{
    public interface IBonus : IReadOnlyGameObject
    {
        BonusType Type { get; }
    }

    public class Bonus : ReadOnlyGameObject, IBonus
    {
        public BonusType Type { get; }

        public Bonus(Position position, BonusType type)
            : base(position)
        {
            Type = type;
        }
    }
}
