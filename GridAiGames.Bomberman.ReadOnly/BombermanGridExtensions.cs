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

        public static IEnumerable<Position> EnumerateDetonationFirePositions(this IReadOnlyGameGrid gameGrid, IBomb bomb)
        {
            yield return bomb.Position;
            foreach (var pos in FillOneDirectionWithFire(bomb.Position, new Position(-1, 0))) yield return pos;
            foreach (var pos in FillOneDirectionWithFire(bomb.Position, new Position(0, 1))) yield return pos;
            foreach (var pos in FillOneDirectionWithFire(bomb.Position, new Position(1, 0))) yield return pos;
            foreach (var pos in FillOneDirectionWithFire(bomb.Position, new Position(0, -1))) yield return pos;

            IEnumerable<Position> FillOneDirectionWithFire(Position currentPos, Position delta)
            {
                for (int r = 0; r < bomb.Radius - 1; r++)
                {
                    currentPos += delta;
                    if (IsPositionAvailableForFire(currentPos))
                    {
                        yield return currentPos;

                        if (gameGrid.GetObjects(currentPos).Any(o => o is IWall || o is IBonus)) break; //stop spreading fire after hit of first destoyable wall or bonus
                    }
                    else yield break;
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
