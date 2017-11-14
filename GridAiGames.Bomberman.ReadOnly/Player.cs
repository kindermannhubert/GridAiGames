namespace GridAiGames.Bomberman.ReadOnly
{
    public class Player : ReadOnlyGameObject
    {
        public string Name { get; }
        public int MaxPossibleNumberOfBombs { get; }
        public int AvailableBombs { get; }
        public int BombsFireRadius { get; }
        public int BombsDetonationTime { get; }

        public Player(string name, Position position, int maxPossibleNumberOfBombs, int availableBombs, int bombsFireRadius, int bombsDetonationTime)
            : base(position)
        {
            Name = name;
            MaxPossibleNumberOfBombs = maxPossibleNumberOfBombs;
            AvailableBombs = availableBombs;
            BombsFireRadius = bombsFireRadius;
            BombsDetonationTime = bombsDetonationTime;
        }

        public override bool Equals(object obj)
        {
            var player = obj as Player;
            return player != null && Name == player.Name;
        }

        public override int GetHashCode() => Name.GetHashCode();

        public static bool operator ==(Player a, Player b) => a.Equals(b);
        public static bool operator !=(Player a, Player b) => !a.Equals(b);
    }
}