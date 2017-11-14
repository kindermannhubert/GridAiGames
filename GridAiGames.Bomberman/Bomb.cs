using System;
using System.Linq;

namespace GridAiGames.Bomberman
{
    internal class Bomb : GameObject<Player, PlayerAction>
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
            gameGrid.AddObject(new BombDetonationFire(Position, BombDetonationFireType.Center));

            FillOneDirectionWithFire(gameGrid, Position, new Position(-1, 0));
            FillOneDirectionWithFire(gameGrid, Position, new Position(0, 1));
            FillOneDirectionWithFire(gameGrid, Position, new Position(1, 0));
            FillOneDirectionWithFire(gameGrid, Position, new Position(0, -1));

            owner?.BombDetonated();
        }

        private Position FillOneDirectionWithFire(IGameGrid<Player, PlayerAction> gameGrid, Position currentPos, Position delta)
        {
            for (int r = 0; r < Radius - 1; r++)
            {
                currentPos += delta;
                if (IsPositionAvailableForFire(gameGrid, currentPos))
                {
                    gameGrid.AddObject(new BombDetonationFire(currentPos, delta.Y == 0 ? BombDetonationFireType.Horizontal : BombDetonationFireType.Vertical));

                    if (gameGrid.GetObjects(currentPos).Any(o => o is Wall || o is Bonus)) break; //stop spreading fire after hit of first destoyable wall or bonus
                }
                else break;
            }

            return currentPos;
        }

        private bool IsPositionAvailableForFire(IGameGrid<Player, PlayerAction> gameGrid, Position position)
        {
            foreach (var obj in gameGrid.GetObjects(position))
            {
                switch (obj)
                {
                    case Wall w:
                        return w.IsDestroyable;
                    case Bomb _:
                    case BombDetonationFire _:
                    case Bonus _:
                        return true;
                    default:
                        throw new InvalidOperationException($"Unknown object type: '{obj.GetType().FullName}'.");
                }
            }
            return true;
        }
    }
}
