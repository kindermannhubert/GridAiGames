﻿using System;
using System.Collections.Generic;
using System.Linq;
using GridAiGames.Bomberman.ReadOnly;

namespace GridAiGames.Bomberman.SimpleIntelligence
{
    public class SimpleIntelligence : IBombermanIntelligence
    {
        private static readonly PlayerAction[] actionsForRandomPicking = new[] { PlayerAction.MoveLeft, PlayerAction.MoveUp, PlayerAction.MoveRight, PlayerAction.MoveDown, PlayerAction.PlaceBomb };

        private readonly Random rand = new Random(5);

        public void Initialize(GameGrid gameGrid, IReadOnlyList<ReadOnly.IPlayer> teamPlayers)
        {
        }

        public IEnumerable<(string playerName, PlayerAction action)>
            GetActionsForTeam(
                GameGrid gameGrid,
                IReadOnlyList<ReadOnly.IPlayer> teamPlayers)
        {
            foreach (var player in teamPlayers)
            {
                yield return (player.Name, GetPlayerAction(gameGrid, player));
            }
        }

        private PlayerAction GetPlayerAction(GameGrid gameGrid, ReadOnly.IPlayer player)
        {
            var nearestBomb = gameGrid.Bombs.OrderBy(b => b.Position.DistanceSquared(player.Position)).FirstOrDefault();

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
                return actionsForRandomPicking[rand.Next(actionsForRandomPicking.Length)];
            }
        }

        private static IEnumerable<Position> GetFreePositionsArroundPlayer(GameGrid gameGrid, IPlayer player)
        {
            var newPos = player.Position.Left;
            if (gameGrid.IsPositionAvailableForPlayer(newPos)) yield return newPos;

            newPos = player.Position.Up;
            if (gameGrid.IsPositionAvailableForPlayer(newPos)) yield return newPos;

            newPos = player.Position.Right;
            if (gameGrid.IsPositionAvailableForPlayer(newPos)) yield return newPos;

            newPos = player.Position.Down;
            if (gameGrid.IsPositionAvailableForPlayer(newPos)) yield return newPos;
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
