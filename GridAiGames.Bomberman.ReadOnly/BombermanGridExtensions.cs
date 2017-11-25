using System;
using System.Collections.Generic;
using System.Linq;

namespace GridAiGames.Bomberman.ReadOnly
{
    public static class BombermanGridExtensions
    {
        public static bool IsPositionAvailableForPlayer(this IReadOnlyGameGrid grid, Position position)
        {
            foreach (var obj in grid.GetObjects(position))
            {
                switch (obj)
                {
                    case IWall _:
                    case IBomb _:
                        return false;
                    case IBombDetonationFire _:
                    case IBonus _:
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown object type: '{obj.GetType().FullName}'.");
                }
            }
            return true;
        }

        public static bool IsPositionAvailableForBomb(this IReadOnlyGameGrid grid, Position position)
        {
            foreach (var obj in grid.GetObjects(position))
            {
                switch (obj)
                {
                    case IWall _:
                    case IBomb _:
                        return false;
                    case IBombDetonationFire _:
                    case IBonus _:
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown object type: '{obj.GetType().FullName}'.");
                }
            }
            return true;
        }

        public static (List<IBomb> detonatedBombs, List<Position> burntPositions) GetDetonationFirePositions(this IReadOnlyGameGrid gameGrid, IBomb bomb)
        {
            var detonatedBombs = new List<IBomb>(32);
            var burntPositions = new List<Position>(128);

            GetDetonationFirePositions(gameGrid, bomb, detonatedBombs, burntPositions);

            return (detonatedBombs, burntPositions);
        }

        private static void GetDetonationFirePositions(IReadOnlyGameGrid gameGrid, IBomb bomb, List<IBomb> detonatedBombs, List<Position> burntPositions)
        {
            if (detonatedBombs.Contains(bomb))
            {
                return;
            }
            else
            {
                detonatedBombs.Add(bomb);
            }

            burntPositions.Add(bomb.Position);
            FillOneDirectionWithFire(bomb.Position, new Position(-1, 0));
            FillOneDirectionWithFire(bomb.Position, new Position(0, 1));
            FillOneDirectionWithFire(bomb.Position, new Position(1, 0));
            FillOneDirectionWithFire(bomb.Position, new Position(0, -1));

            void FillOneDirectionWithFire(Position currentPos, Position delta)
            {
                for (int r = 0; r < bomb.Radius - 1; r++)
                {
                    currentPos += delta;
                    if (IsPositionAvailableForFire(currentPos))
                    {
                        bool anyAnotherBomb = false;
                        foreach (var anotherBomb in gameGrid.GetObjects(currentPos).OfType<IBomb>())
                        {
                            GetDetonationFirePositions(gameGrid, anotherBomb, detonatedBombs, burntPositions);
                            anyAnotherBomb = true;
                        }

                        if (!anyAnotherBomb) burntPositions.Add(currentPos);

                        if (gameGrid.GetObjects(currentPos).Any(o => o is IWall || o is IBonus)) break; //stop spreading fire after hit of first destoyable wall or bonus
                    }
                    else return;
                }
            }

            bool IsPositionAvailableForFire(Position position)
            {
                foreach (var obj in gameGrid.GetObjects(position))
                {
                    switch (obj)
                    {
                        case IWall w:
                            return w.IsDestroyable;
                        case IBomb _:
                        case IBombDetonationFire _:
                        case IBonus _:
                            return true;
                        default:
                            throw new InvalidOperationException($"Unknown object type: '{obj.GetType().FullName}'.");
                    }
                }
                return true;
            }
        }
    }
}
