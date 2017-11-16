using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GridAiGames.Logging;

namespace GridAiGames.Bomberman
{
    internal class GameGrid : GameGrid<Player, ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>
    {
        private readonly ReadOnly.GameGrid readOnlyGameGrid;

        public Random Random { get; }

        public GameGrid(
            int width,
            int height,
            IReadOnlyList<TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>> teamDefinitions,
            Func<string, string, Position> getPlayerPosition,
            Action<GameGrid> addGameObjects,
            Random rand,
            ILogger logger)
            : base(
                  width, height,
                  teamDefinitions,
                  (PlayerDefinition playerDefinition, string teamName) => new Player(playerDefinition.Name, teamName, getPlayerPosition(teamName, playerDefinition.Name)),
                  logger)
        {
            Random = rand;
            addGameObjects(this);
            readOnlyGameGrid = new ReadOnly.GameGrid(width, height);

            Initialize();
        }

        protected override void ProcessPlayerAction(Player player, PlayerActionExtended<PlayerAction> action)
        {
            if (action.DisqualifyPlayer)
            {
                player.Life = 0;
                if (!player.IsAlive)
                {
                    RemovePlayer(player);
                }
                return;
            }

            switch (action.Action)
            {
                case PlayerAction.None:
                    break;
                case PlayerAction.MoveLeft:
                case PlayerAction.MoveLeft | PlayerAction.PlaceBomb:
                    {
                        PlaceBombIfRequested();
                        var newPos = player.Position.Left;
                        if (player.Position.X > 0 && IsPositionAvailableForPlayer(newPos))
                        {
                            MoveObject(player, player.Position, newPos);
                        }
                    }
                    break;
                case PlayerAction.MoveUp:
                case PlayerAction.MoveUp | PlayerAction.PlaceBomb:
                    {
                        PlaceBombIfRequested();
                        var newPos = player.Position.Up;
                        if (player.Position.Y < Height - 1 && IsPositionAvailableForPlayer(newPos))
                        {
                            MoveObject(player, player.Position, newPos);
                        }
                    }
                    break;
                case PlayerAction.MoveRight:
                case PlayerAction.MoveRight | PlayerAction.PlaceBomb:
                    {
                        PlaceBombIfRequested();
                        var newPos = player.Position.Right;
                        if (player.Position.X < Width - 1 && IsPositionAvailableForPlayer(newPos))
                        {
                            MoveObject(player, player.Position, newPos);
                        }
                    }
                    break;
                case PlayerAction.MoveDown:
                case PlayerAction.MoveDown | PlayerAction.PlaceBomb:
                    {
                        PlaceBombIfRequested();
                        var newPos = player.Position.Down;
                        if (player.Position.Y > 0 && IsPositionAvailableForPlayer(newPos))
                        {
                            MoveObject(player, player.Position, newPos);
                        }
                    }
                    break;
                case PlayerAction.PlaceBomb:
                    PlaceBombIfRequested();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown player's action: '{action}'.");
            }

            void PlaceBombIfRequested()
            {
                if ((action.Action & PlayerAction.PlaceBomb) == PlayerAction.None) return;
                if (IsPositionAvailableForBomb(player.Position))
                {
                    var bomb = player.CreateBomb();
                    if (bomb != null)
                    {
                        AddObject(bomb);
                    }
                }
            }
        }

        protected override void ConsolidateNewObjectsAndPlayers()
        {
            var objectList = new List<GameObject<Player, PlayerAction>>();

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var objects = GetObjects(x, y);
                    if (objects.Any(o => o is BombDetonationFire f && f.IsBurning))
                    {
                        //detonate other bombs
                        objectList.Clear();
                        objectList.AddRange(objects.OfType<Bomb>());
                        foreach (Bomb bomb in objectList)
                        {
                            Debug.Assert(bomb.Position == new Position(x, y));
                            bomb.Detonate(this);
                        }
                    }
                }

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var objects = GetObjects(x, y);
                    if (objects.Any(o => o is BombDetonationFire f && f.IsBurning))
                    {
                        //damage players
                        objectList.Clear();
                        objectList.AddRange(GetPlayers(x, y));
                        foreach (Player player in objectList)
                        {
                            --player.Life;
                            if (!player.IsAlive)
                            {
                                RemovePlayer(player);
                            }
                        }

                        //burn destroyable walls
                        bool anyWallDestroyed = false;
                        objectList.Clear();
                        objectList.AddRange(objects.Where(o => o is Wall w && w.IsDestroyable));
                        foreach (var wall in objectList)
                        {
                            Debug.Assert(wall.Position == new Position(x, y));
                            RemoveObject(wall);
                            anyWallDestroyed = true;
                        }

                        //burn bonuses
                        if (!anyWallDestroyed) //walls protect bonuses
                        {
                            objectList.Clear();
                            objectList.AddRange(objects.OfType<Bonus>());
                            foreach (var bonus in objectList)
                            {
                                Debug.Assert(bonus.Position == new Position(x, y));
                                RemoveObject(bonus);
                            }
                        }
                    }

                    objectList.Clear();
                    objectList.AddRange(objects.OfType<Bonus>());
                    foreach (Bonus bonus in objectList)
                    {
                        var eatenByPlayer = false;
                        foreach (var player in GetPlayers(x, y))
                        {
                            //everyone gets the bonus
                            player.ConsumeBonus(bonus);
                            eatenByPlayer = true;
                        }
                        if (eatenByPlayer) RemoveObject(bonus);
                    }
                }
        }

        protected override ReadOnly.Player GetReadonlyPlayer(Player player)
        {
            return new ReadOnly.Player(player.Name, player.Position, player.MaxPossibleNumberOfBombs, player.AvailableBombs, player.BombsFireRadius, player.BombsDetonationTime);
        }

        protected override ReadOnly.GameGrid GetReadonlyGameGrid(GameGrid<Player, ReadOnly.GameGrid, ReadOnly.Player, PlayerAction> gameGrid)
        {
            var allObjects = new IReadOnlyList<ReadOnlyGameObject>[Width, Height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var list = GetObjects(x, y)
                        .Select<GameObject<Player, PlayerAction>, ReadOnlyGameObject>(
                            o =>
                            {
                                switch (o)
                                {
                                    case Wall w:
                                        return new ReadOnly.Wall(w.Position, w.IsDestroyable);
                                    case Bomb b:
                                        return new ReadOnly.Bomb(b.Position, b.Radius, b.DetonateAfter);
                                    case Bonus b:
                                        return new ReadOnly.Bonus(b.Position, b.Type);
                                    case BombDetonationFire _:
                                        return null;
                                    default:
                                        throw new InvalidOperationException($"Unknown object type: '{o.GetType().FullName}'.");
                                }
                            })
                        .Where(o => o != null)
                        .ToList();

                    //bonuses are hidden by walls
                    if (list.OfType<ReadOnly.Wall>().Any())
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i] is ReadOnly.Bonus) list.RemoveAt(i--);
                        }
                    }

                    allObjects[x, y] = list;
                }

            readOnlyGameGrid.SetUp(allObjects, AllPlayers.Select(p => GetReadonlyPlayer(p)).ToList());
            return readOnlyGameGrid;
        }

        private bool IsPositionAvailableForPlayer(Position position)
        {
            foreach (var obj in GetObjects(position))
            {
                switch (obj)
                {
                    case Wall _:
                    case Bomb _:
                        return false;
                    case BombDetonationFire _:
                    case Bonus _:
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown object type: '{obj.GetType().FullName}'.");
                }
            }
            return true;
        }

        private bool IsPositionAvailableForBomb(Position position)
        {
            foreach (var obj in GetObjects(position))
            {
                switch (obj)
                {
                    case Wall _:
                    case Bomb _:
                        return false;
                    case BombDetonationFire _:
                    case Bonus _:
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown object type: '{obj.GetType().FullName}'.");
                }
            }
            return true;
        }
    }
}