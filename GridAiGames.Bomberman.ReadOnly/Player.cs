namespace GridAiGames.Bomberman.ReadOnly
{
    public interface IPlayer : IPlayer<Player, PlayerAction>
    {
        //string Name { get; }
        int MaxPossibleNumberOfBombs { get; }
        int AvailableBombs { get; }
        int BombsFireRadius { get; }
        int BombsDetonationTime { get; }
    }

    public class Player : ReadOnlyGameObject, IPlayer
    {
        public string Name { get; }
        public string TeamName { get; }
        public int MaxPossibleNumberOfBombs { get; }
        public int AvailableBombs { get; }
        public int BombsFireRadius { get; }
        public int BombsDetonationTime { get; }
        public Position PreviousPosition { get; }
        public bool IsAlive { get; }

        public Player(string name, string teamName, Position position, int maxPossibleNumberOfBombs, int availableBombs, int bombsFireRadius, int bombsDetonationTime, Position previousPosition, bool isAlive)
             : base(position)
        {
            Name = name;
            TeamName = teamName;
            MaxPossibleNumberOfBombs = maxPossibleNumberOfBombs;
            AvailableBombs = availableBombs;
            BombsFireRadius = bombsFireRadius;
            BombsDetonationTime = bombsDetonationTime;
            PreviousPosition = previousPosition;
            IsAlive = isAlive;
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