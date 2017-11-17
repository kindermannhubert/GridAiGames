using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GridAiGames.Logging;

namespace GridAiGames
{
    public abstract class GameGrid<PlayerType, ReadOnlyGameGridType, ReadOnlyPlayerType, PlayerActionType> : IGameGrid<PlayerType, PlayerActionType>
        where PlayerType : Player<PlayerType, PlayerActionType>
    {
        private readonly IReadOnlyList<TeamDefinition<ReadOnlyGameGridType, ReadOnlyPlayerType, PlayerActionType>> teamDefinitions;
        private readonly Dictionary<string, List<PlayerType>> playersPerTeamName = new Dictionary<string, List<PlayerType>>();
        private readonly List<List<(string playerName, PlayerActionExtended<PlayerActionType> action)>> actionsPerTeam = new List<List<(string, PlayerActionExtended<PlayerActionType>)>>();

        private List<GameObject<PlayerType, PlayerActionType>>[,] currentObjects;
        private List<PlayerType>[,] currentPlayers;

        private List<GameObject<PlayerType, PlayerActionType>>[,] newObjects;
        private List<PlayerType>[,] newPlayers;

        private bool consolidationOfNewObjects;
        private bool initialized;

        protected readonly ILogger logger;

        public IReadOnlyList<TeamDefinition<ReadOnlyGameGridType, ReadOnlyPlayerType, PlayerActionType>> Teams => teamDefinitions;

        public IEnumerable<GameObject<PlayerType, PlayerActionType>> AllObjects
        {
            get
            {
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        foreach (var obj in (initialized & consolidationOfNewObjects ? newObjects : currentObjects)[x, y])
                        {
                            yield return obj;
                        }
            }
        }

        public IEnumerable<PlayerType> AllPlayers
        {
            get
            {
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        foreach (var p in (initialized & consolidationOfNewObjects ? newPlayers : currentPlayers)[x, y])
                        {
                            yield return p;
                        }
            }
        }

        public IReadOnlyList<GameObject<PlayerType, PlayerActionType>> GetObjects(Position position) => GetObjects(position.X, position.Y);
        public IReadOnlyList<GameObject<PlayerType, PlayerActionType>> GetObjects(int x, int y) => (initialized & consolidationOfNewObjects ? newObjects : currentObjects)[x, y];

        public IReadOnlyList<PlayerType> GetPlayers(Position position) => GetPlayers(position.X, position.Y);
        public IReadOnlyList<PlayerType> GetPlayers(int x, int y) => (initialized & consolidationOfNewObjects ? newPlayers : currentPlayers)[x, y];

        public int Width { get; }
        public int Height { get; }

        public GameGrid(
            int width,
            int height,
            IReadOnlyList<TeamDefinition<ReadOnlyGameGridType, ReadOnlyPlayerType, PlayerActionType>> teamDefinitions,
            CreatePlayerHandler createPlayer,
            ILogger logger)
        {
            this.logger = logger;
            Width = width;
            Height = height;
            this.teamDefinitions = teamDefinitions;

            currentObjects = new List<GameObject<PlayerType, PlayerActionType>>[width, height];
            newObjects = new List<GameObject<PlayerType, PlayerActionType>>[width, height];
            currentPlayers = new List<PlayerType>[width, height];
            newPlayers = new List<PlayerType>[width, height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    currentObjects[x, y] = new List<GameObject<PlayerType, PlayerActionType>>();
                    newObjects[x, y] = new List<GameObject<PlayerType, PlayerActionType>>();
                    currentPlayers[x, y] = new List<PlayerType>();
                    newPlayers[x, y] = new List<PlayerType>();
                }

            if (teamDefinitions.Select(t => t.Name).Distinct().Count() != teamDefinitions.Count)
                throw new InvalidOperationException("All team names must be distinct.");

            foreach (var teamDefinition in teamDefinitions)
            {
                if (teamDefinition.Players.Select(p => p.Name).Distinct().Count() != teamDefinition.Players.Count)
                    throw new InvalidOperationException("All player names must be distinct.");

                foreach (var playerDefinition in teamDefinition.Players)
                {
                    var player = createPlayer(playerDefinition, teamDefinition.Name);
                    AddPlayer(player);

                    if (!playersPerTeamName.TryGetValue(teamDefinition.Name, out var teamPlayers))
                    {
                        playersPerTeamName.Add(teamDefinition.Name, teamPlayers = new List<PlayerType>());
                    }
                    teamPlayers.Add(player);
                }
            }
        }

        public void Initialize()
        {
            Debug.Assert(!initialized, "Grid is already initialized.");

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    newObjects[x, y].Clear();
                    newObjects[x, y].AddRange(currentObjects[x, y]);

                    newPlayers[x, y].Clear();
                    newPlayers[x, y].AddRange(currentPlayers[x, y]);
                }

            foreach (var team in teamDefinitions)
            {
                team.Intelligence.Initialize(GetReadonlyGameGrid(this), playersPerTeamName[team.Name].Select(player => GetReadonlyPlayer(player)).ToList());
            }

            initialized = true;
        }

        public virtual void Update(ulong iteration)
        {
            if (!initialized) throw new InvalidOperationException("Grid should not be updated until its initialized flag is set to true.");

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    newObjects[x, y].Clear();
                    newObjects[x, y].AddRange(currentObjects[x, y]);

                    newPlayers[x, y].Clear();
                    newPlayers[x, y].AddRange(currentPlayers[x, y]);
                }

            CheckPositionsConsistency();

            var readOnlyGameGrid = GetReadonlyGameGrid(this);
            actionsPerTeam.Clear();
            foreach (var team in teamDefinitions)
            {
                var readonlyPlayers = playersPerTeamName[team.Name].Select(player => GetReadonlyPlayer(player)).ToList();

                bool exceptionThrown = false;
                IEnumerable<(string playerName, PlayerActionType action)> actions;
                try
                {
                    actions = team.Intelligence.GetActionsForTeam(readOnlyGameGrid, readonlyPlayers).ToList();
                }
                catch (Exception ex)
                {
                    actions = team.Players.Select(p => (p.Name, default(PlayerActionType))).ToList();
                    exceptionThrown = true;
                    logger.Log(LogType.Error, $"AI of team '{team.Name}' has thrown exception. Players of this team is going to be disqualified. Exception: '{ex}'");
                }

                var extendedActions = new List<(string playerName, PlayerActionExtended<PlayerActionType> action)>();
                foreach (var playerActionsGroup in actions.GroupBy(a => a.playerName))
                {
                    if (exceptionThrown)
                    {
                        foreach (var action in playerActionsGroup)
                        {
                            extendedActions.Add((action.playerName, new PlayerActionExtended<PlayerActionType>(disqualifyPlayer: true)));
                        }
                    }
                    else
                    {
                        if (playerActionsGroup.Count() > 1)
                        {
                            logger.Log(LogType.Error, $"Player '{playerActionsGroup.Key}' wanted to do more than one action. He's going to be disqualified. Actions: {string.Join(", ", playerActionsGroup.Select(a => a.action))}.");
                            extendedActions.Add((playerActionsGroup.Key, new PlayerActionExtended<PlayerActionType>(disqualifyPlayer: true)));
                            break;
                        }
                        else
                        {
                            foreach (var action in playerActionsGroup)
                            {
                                extendedActions.Add((action.playerName, new PlayerActionExtended<PlayerActionType>(action.action)));
                            }
                        }
                    }
                }
                actionsPerTeam.Add(extendedActions);
            }

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    foreach (var obj in currentObjects[x, y])
                    {
                        //objects are allowed to change their position in their update method
                        var positionBeforeUpdate = new Position(x, y);
                        obj.Update(this, iteration);
                        if (positionBeforeUpdate != obj.Position)
                        {
                            MoveObject(obj, positionBeforeUpdate, obj.Position);
                        }
                    }

                    foreach (var player in currentPlayers[x, y])
                    {
                        var positionBeforeUpdate = new Position(x, y);
                        player.Update(this, iteration);
                        if (positionBeforeUpdate != player.Position) throw new InvalidOperationException("Player's position can be changed only by IIntelligence actions.");
                    }
                }

            CheckPositionsConsistency();
            foreach (var teamActions in actionsPerTeam)
            {
                foreach (var item in teamActions)
                {
                    var player = AllPlayers.SingleOrDefault(p => p.Name == item.playerName);
                    if (player != null)
                    {
                        Debug.Assert(player.IsAlive);
                        ProcessPlayerAction(player, item.action);
                    }
                }
            }
            CheckPositionsConsistency();

            try
            {
                consolidationOfNewObjects = true;
                ConsolidateNewObjectsAndPlayers();
            }
            finally
            {
                consolidationOfNewObjects = false;
            }

            CheckPositionsConsistency();

            Utils.Exchange(ref currentObjects, ref newObjects);
            Utils.Exchange(ref currentPlayers, ref newPlayers);
        }

        [Conditional("DEBUG")]
        private void CheckPositionsConsistency()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    Debug.Assert(newObjects[x, y].All(o => o.Position == new Position(x, y)));
                    Debug.Assert(newPlayers[x, y].All(o => o.Position == new Position(x, y)));
                }
        }

        protected abstract void ConsolidateNewObjectsAndPlayers();
        protected abstract ReadOnlyPlayerType GetReadonlyPlayer(PlayerType player);
        protected abstract ReadOnlyGameGridType GetReadonlyGameGrid(GameGrid<PlayerType, ReadOnlyGameGridType, ReadOnlyPlayerType, PlayerActionType> gameGrid);

        public void AddObject(GameObject<PlayerType, PlayerActionType> obj)
            => (initialized ? newObjects : currentObjects)[obj.Position.X, obj.Position.Y].Add(obj);

        public void RemoveObject(GameObject<PlayerType, PlayerActionType> obj)
        {
            var result = (initialized ? newObjects : currentObjects)[obj.Position.X, obj.Position.Y].Remove(obj);
            Debug.Assert(result);
        }

        protected void AddPlayer(PlayerType player)
            => (initialized ? newPlayers : currentPlayers)[player.Position.X, player.Position.Y].Add(player);

        protected void RemovePlayer(PlayerType player)
        {
            var result = (initialized ? newPlayers : currentPlayers)[player.Position.X, player.Position.Y].Remove(player);
            Debug.Assert(result);
        }

        protected void MoveObject(GameObject<PlayerType, PlayerActionType> obj, Position from, Position to)
        {
            if (obj is PlayerType player)
            {
                var result = (initialized ? newPlayers : currentPlayers)[from.X, from.Y].Remove(player);
                Debug.Assert(result);
                (initialized ? newPlayers : currentPlayers)[to.X, to.Y].Add(player);
                Debug.Assert(player.Position == from);
                player.Position = to;
            }
            else
            {
                var result = (initialized ? newObjects : currentObjects)[from.X, from.Y].Remove(obj);
                Debug.Assert(result);
                (initialized ? newObjects : currentObjects)[to.X, to.Y].Add(obj);
                Debug.Assert(obj.Position == from);
                obj.Position = to;
            }
        }

        protected abstract void ProcessPlayerAction(PlayerType player, PlayerActionExtended<PlayerActionType> action);

        public delegate PlayerType CreatePlayerHandler(PlayerDefinition playerDefinition, string teamName);

        public event Action<PlayerType> PlayerIsAliveChanged
        {
            add
            {
                foreach (var player in AllPlayers)
                {
                    player.IsAliveChanged += value;
                }
            }
            remove
            {
                foreach (var player in AllPlayers)
                {
                    player.IsAliveChanged -= value;
                }
            }
        }
    }
}