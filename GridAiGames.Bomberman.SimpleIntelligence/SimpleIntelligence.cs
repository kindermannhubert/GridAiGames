﻿using System;
using System.Collections.Generic;
using System.Linq;
using GridAiGames.Bomberman.ReadOnly;

namespace GridAiGames.Bomberman.SimpleIntelligence
{
    public class SimpleIntelligence : IIntelligence<GameGrid, Player, PlayerAction>
    {
        private static readonly Random rand = new Random(3);

        public IEnumerable<(string playerName, PlayerAction action)>
            GetActionsForTeam(
                GameGrid gameGrid,
                IReadOnlyList<Player> teamPlayers,
                ulong iteration)
        {
            foreach (var player in teamPlayers)
            {
                yield return (player.Name, GetPlayerAction(gameGrid, player));
            }
        }

        private PlayerAction GetPlayerAction(GameGrid gameGrid, Player player)
        {
            var nearestBomb = gameGrid.AllObjects.OfType<Bomb>().OrderBy(b => b.Position.DistanceSquared(player.Position)).FirstOrDefault();

            if (nearestBomb != null && nearestBomb.Position.DistanceSquared(player.Position) < nearestBomb.Radius * nearestBomb.Radius)
            {
                if (nearestBomb.Position.X != player.Position.X && nearestBomb.Position.Y != player.Position.Y)
                {
                    //player is safe -> wait
                    return PlayerAction.None;
                }
                else
                {
                    var directionFromBomb = player.Position - nearestBomb.Position;
                    var freePositionsAround = GetFreePositionsArroundPlayer(gameGrid, player).ToList();
                    foreach (var freePosition in freePositionsAround)
                    {
                        if (nearestBomb.Position.X != freePosition.X && nearestBomb.Position.Y != freePosition.Y)
                        {
                            return DirectionToMoveAction(freePosition - player.Position);
                        }
                    }

                    var moveFromBombAction = DirectionToMoveAction(directionFromBomb);
                    if (moveFromBombAction == PlayerAction.None)
                    {
                        moveFromBombAction = DirectionToMoveAction(freePositionsAround[rand.Next(freePositionsAround.Count)] - player.Position);
                    }

                    return moveFromBombAction;
                }
            }
            else
            {
                return (PlayerAction)rand.Next(1, 6);
            }
        }

        private static IEnumerable<Position> GetFreePositionsArroundPlayer(GameGrid gameGrid, Player player)
        {
            var newPos = player.Position.Left;
            if (!gameGrid.GetObjects(newPos).OfType<Wall>().Any()) yield return newPos;

            newPos = player.Position.Up;
            if (!gameGrid.GetObjects(newPos).OfType<Wall>().Any()) yield return newPos;

            newPos = player.Position.Right;
            if (!gameGrid.GetObjects(newPos).OfType<Wall>().Any()) yield return newPos;

            newPos = player.Position.Down;
            if (!gameGrid.GetObjects(newPos).OfType<Wall>().Any()) yield return newPos;
        }

        private static PlayerAction DirectionToMoveAction(Position direction)
        {
            if (direction.X == 0 && direction.Y == 0) return PlayerAction.None;

            if (Math.Abs(direction.X) > Math.Abs(direction.Y))
            {
                if (direction.X > 0)
                {
                    return PlayerAction.MoveRight;
                }
                else
                {
                    return PlayerAction.MoveLeft;
                }
            }
            else
            {
                if (direction.Y > 0)
                {
                    return PlayerAction.MoveUp;
                }
                else
                {
                    return PlayerAction.MoveDown;
                }
            }
        }
    }
}