using System;
using System.Collections.Generic;
using System.Linq;
using GridAiGames.Bomberman.ReadOnly;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GridAiGames.Bomberman.Tests
{
    [TestClass]
    public class MovementTests
    {
        [TestMethod]
        public void SimpleMovementTest()
        {
            NonBlockedMovementTest(pos => null);
        }

        [TestMethod]
        public void NonBlockedByBonusMovementTest()
        {
            NonBlockedMovementTest(pos => new Bonus(pos, BonusType.Bomb));
        }

        private void NonBlockedMovementTest(Func<Position, GameObject<Player, PlayerAction>> createNonBlockingObject)
        {
            const string PlayerName = "John";
            var intelligence = new ManualIntelligence();

            var grid = new TestGameGrid(
                2, 2,
                new TeamDefinition<ReadOnly.GameGrid, IPlayer, PlayerAction>[]
                {
                    new TeamDefinition<ReadOnly.GameGrid, IPlayer, PlayerAction>("Team A", new[] { new PlayerDefinition(PlayerName) }, intelligence)
                },
                new Dictionary<string, Position>() { { PlayerName, new Position(0, 0) } },
                g =>
                {
                    for (int y = 0; y < g.Height; y++)
                        for (int x = 0; x < g.Width; x++)
                        {
                            var obj = createNonBlockingObject(new Position(x, y));
                            if (obj != null) g.AddObject(obj);
                        }
                });

            var player = grid.AllPlayers.Single(p => p.Name == PlayerName);

            CheckObjectPositions(grid);
            grid.Update();
            CheckObjectPositions(grid);
            Assert.IsFalse(grid.GetPlayers(1, 0).Any());
            Assert.IsFalse(grid.GetPlayers(0, 1).Any());
            Assert.IsFalse(grid.GetPlayers(1, 1).Any());
            Assert.AreEqual(player, grid.GetPlayers(0, 0).Single());
            Assert.AreEqual(new Position(0, 0), player.Position);

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveRight));
            grid.Update();
            CheckObjectPositions(grid);
            Assert.IsFalse(grid.GetPlayers(0, 0).Any());
            Assert.IsFalse(grid.GetPlayers(0, 1).Any());
            Assert.IsFalse(grid.GetPlayers(1, 1).Any());
            Assert.AreEqual(player, grid.GetPlayers(1, 0).Single());
            Assert.AreEqual(new Position(1, 0), player.Position);

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveUp));
            grid.Update();
            CheckObjectPositions(grid);
            Assert.IsFalse(grid.GetPlayers(0, 0).Any());
            Assert.IsFalse(grid.GetPlayers(1, 0).Any());
            Assert.IsFalse(grid.GetPlayers(0, 1).Any());
            Assert.AreEqual(player, grid.GetPlayers(1, 1).Single());
            Assert.AreEqual(new Position(1, 1), player.Position);

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveLeft));
            grid.Update();
            CheckObjectPositions(grid);
            Assert.IsFalse(grid.GetPlayers(0, 0).Any());
            Assert.IsFalse(grid.GetPlayers(1, 0).Any());
            Assert.IsFalse(grid.GetPlayers(1, 1).Any());
            Assert.AreEqual(player, grid.GetPlayers(0, 1).Single());
            Assert.AreEqual(new Position(0, 1), player.Position);

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveDown));
            grid.Update();
            CheckObjectPositions(grid);
            Assert.IsFalse(grid.GetPlayers(1, 0).Any());
            Assert.IsFalse(grid.GetPlayers(0, 1).Any());
            Assert.IsFalse(grid.GetPlayers(1, 1).Any());
            Assert.AreEqual(player, grid.GetPlayers(0, 0).Single());
            Assert.AreEqual(new Position(0, 0), player.Position);
        }

        [TestMethod]
        public void BlockedByBombMovementTest()
        {
            BlockedMovementTest(pos => new Bomb(pos, 3, 100, null));
        }

        [TestMethod]
        public void BlockedByWallMovementTest()
        {
            BlockedMovementTest(pos => new Wall(pos, isDestroyable: false));
        }

        [TestMethod]
        public void BlockedByDestroyableWallMovementTest()
        {
            BlockedMovementTest(pos => new Wall(pos, isDestroyable: true));
        }

        private void BlockedMovementTest(Func<Position, GameObject<Player, PlayerAction>> createBlockingObject)
        {
            const string PlayerName = "John";
            var intelligence = new ManualIntelligence();

            var grid = new TestGameGrid(
                3, 3,
                new TeamDefinition<ReadOnly.GameGrid, IPlayer, PlayerAction>[]
                {
                    new TeamDefinition<ReadOnly.GameGrid, IPlayer, PlayerAction>("Team A", new[] { new PlayerDefinition(PlayerName) }, intelligence)
                },
                new Dictionary<string, Position>() { { PlayerName, new Position(1, 1) } },
                g =>
                {
                    //borders
                    for (int x = 0; x < g.Width; x++)
                    {
                        g.AddObject(createBlockingObject(new Position(x, 0)));
                        g.AddObject(createBlockingObject(new Position(x, g.Height - 1)));
                    }
                    for (int y = 1; y < g.Height - 1; y++)
                    {
                        g.AddObject(createBlockingObject(new Position(0, y)));
                        g.AddObject(createBlockingObject(new Position(g.Width - 1, y)));
                    }
                },
                new TestLogger(canGenerateWarningAndErrors: true)); //warnings about blocked movement

            var player = grid.AllPlayers.Single(p => p.Name == PlayerName);

            CheckObjectPositions(grid);
            grid.Update();
            CheckObjectPositions(grid);
            Assert.AreEqual(player, grid.GetPlayers(1, 1).Single());
            Assert.AreEqual(new Position(1, 1), player.Position);

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveRight));
            grid.Update();
            CheckObjectPositions(grid);
            Assert.AreEqual(player, grid.GetPlayers(1, 1).Single());
            Assert.AreEqual(new Position(1, 1), player.Position);

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveUp));
            grid.Update();
            CheckObjectPositions(grid);
            Assert.AreEqual(player, grid.GetPlayers(1, 1).Single());
            Assert.AreEqual(new Position(1, 1), player.Position);

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveLeft));
            grid.Update();
            CheckObjectPositions(grid);
            Assert.AreEqual(player, grid.GetPlayers(1, 1).Single());
            Assert.AreEqual(new Position(1, 1), player.Position);

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveDown));
            grid.Update();
            CheckObjectPositions(grid);
            Assert.AreEqual(player, grid.GetPlayers(1, 1).Single());
            Assert.AreEqual(new Position(1, 1), player.Position);
        }

        private static void CheckObjectPositions(GameGrid grid)
        {
            for (int y = 0; y < grid.Height; y++)
                for (int x = 0; x < grid.Width; x++)
                {
                    foreach (var obj in grid.GetObjects(x, y))
                    {
                        Assert.IsTrue(obj.Position == new Position(x, y));
                    }
                    foreach (var player in grid.GetPlayers(x, y))
                    {
                        Assert.IsTrue(player.Position == new Position(x, y));
                    }
                }
        }
    }
}
