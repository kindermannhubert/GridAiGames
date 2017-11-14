using System;

namespace GridAiGames
{
    public abstract class Player<PlayerType, PlayerActionType> : GameObject<PlayerType, PlayerActionType>
        where PlayerType : Player<PlayerType, PlayerActionType>
    {
        private Position previousPosition;
        private bool isAlive = true;

        public Position PreviousPosition => previousPosition;
        public bool IsAlive
        {
            get => isAlive;
            protected set
            {
                if (isAlive != value)
                {
                    isAlive = value;
                    IsAliveChanged?.Invoke((PlayerType)this);
                }
            }
        }
        public string Name { get; }
        public string TeamName { get; }

        protected Player(string name, string teamName, Position position)
            : base(position)
        {
            Name = name;
            TeamName = teamName;
            previousPosition = position;
        }

        public override void Update(IGameGrid<PlayerType, PlayerActionType> gameGrid, ulong iteration)
        {
            previousPosition = Position;
        }

        public event Action<PlayerType> IsAliveChanged;
    }
}