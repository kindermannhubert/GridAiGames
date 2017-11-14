using System;
using System.Diagnostics;

namespace GridAiGames.Bomberman
{
    internal class Player : Player<Player, PlayerAction>
    {
        private int maxPossibleNumberOfBombs = 1;
        private int life = 1;

        public int MaxPossibleNumberOfBombs
        {
            get => maxPossibleNumberOfBombs;
            set
            {
                if (value > maxPossibleNumberOfBombs)
                {
                    AvailableBombs += value - maxPossibleNumberOfBombs;
                }
                else
                {
                    AvailableBombs = Math.Min(AvailableBombs, value);
                }
                maxPossibleNumberOfBombs = value;
            }
        }

        public int AvailableBombs { get; private set; } = 1;
        public int BombsFireRadius { get; set; } = 3;
        public int BombsDetonationTime { get; set; } = 10;
        public int Life
        {
            get => life;
            set
            {
                life = value;
                if (life <= 0) IsAlive = false;
            }
        }

        public Player(string name, string teamName, Position position)
            : base(name, teamName, position)
        {
        }

        public void ConsumeBonus(Bonus bonus)
        {
            switch (bonus.Type)
            {
                case BonusType.Bomb:
                    ++MaxPossibleNumberOfBombs;
                    break;
                case BonusType.Fire:
                    BombsFireRadius++;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown bonus type: '{bonus.Type}'.");
            }
        }

        public Bomb CreateBomb()
        {
            if (AvailableBombs > 0)
            {
                --AvailableBombs;
                return new Bomb(Position, BombsFireRadius, BombsDetonationTime, this);
            }
            else
            {
                return null;
            }
        }

        public void BombDetonated()
        {
            ++AvailableBombs;
            Debug.Assert(AvailableBombs <= MaxPossibleNumberOfBombs);
        }
    }
}