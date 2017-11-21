using System;

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
    }
}
