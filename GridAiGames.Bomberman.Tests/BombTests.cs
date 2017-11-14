using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GridAiGames.Bomberman.Tests
{
    [TestClass]
    public class BombTests
    {
        [TestMethod]
        public void GetKilledByBomb()
        {
            const string PlayerName = "John";
            var intelligence = new ManualIntelligence();

            var grid = new BoundedTestGameGrid(
                3, 3,
                new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>[]
                {
                    new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>("Team A", new[] { new PlayerDefinition(PlayerName) }, intelligence)
                },
                new Dictionary<string, Position>() { { PlayerName, new Position(1, 1) } },
                _ =>
                {
                });

            var player = grid.AllPlayers.Single(p => p.Name == PlayerName);

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.PlaceBomb));
            grid.Update();

            while (((Bomb)grid.GetObjects(1, 1).Single()).DetonateAfter > 1)
            {
                grid.Update();
                Assert.IsTrue(player.IsAlive);
            }

            //will die and keep dead
            for (int i = 0; i < 10; i++)
            {
                grid.Update();
                Assert.IsFalse(player.IsAlive);
                Assert.IsFalse(grid.AllPlayers.Any());
            }
        }

        [TestMethod]
        public void GetKilledByBombWhileMoving()
        {
            const string PlayerName = "John";
            var intelligence = new ManualIntelligence();

            var grid = new BoundedTestGameGrid(
                16, 3,
                new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>[]
                {
                    new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>("Team A", new[] { new PlayerDefinition(PlayerName) }, intelligence)
                },
                new Dictionary<string, Position>() { { PlayerName, new Position(1, 1) } },
                _ =>
                {
                });

            var player = grid.AllPlayers.Single(p => p.Name == PlayerName);
            player.BombsFireRadius = 100;
            player.BombsDetonationTime = 3;

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.PlaceBomb));
            grid.Update();

            while (((Bomb)grid.GetObjects(1, 1).Single()).DetonateAfter > 1)
            {
                intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveRight));
                grid.Update();
                Assert.IsTrue(player.IsAlive);
            }

            //will die and keep dead
            while (player.IsAlive)
            {
                intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveRight));
                grid.Update();
                Assert.IsFalse(player.IsAlive);
                Assert.IsFalse(grid.AllPlayers.Any());
            }
        }

        [TestMethod]
        public void TwoPlayersOnSamePositionGetKilledByBomb()
        {
            const string Player1Name = "John1";
            const string Player2Name = "John2";
            var intelligence = new ManualIntelligence();

            var grid = new BoundedTestGameGrid(
                3, 3,
                new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>[]
                {
                    new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>("Team A", new[] { new PlayerDefinition(Player1Name) }, intelligence),
                    new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>("Team B", new[] { new PlayerDefinition(Player2Name) }, intelligence)
                },
                new Dictionary<string, Position>() { { Player1Name, new Position(1, 1) }, { Player2Name, new Position(1, 1) } },
                _ =>
                {
                });

            var player1 = grid.AllPlayers.Single(p => p.Name == Player1Name);
            var player2 = grid.AllPlayers.Single(p => p.Name == Player2Name);

            intelligence.SetNextActions(grid, (Player1Name, PlayerAction.PlaceBomb));
            grid.Update();

            while (((Bomb)grid.GetObjects(1, 1).Single()).DetonateAfter > 1)
            {
                grid.Update();
                Assert.IsTrue(player1.IsAlive);
                Assert.IsTrue(player2.IsAlive);
            }

            grid.Update();
            Assert.IsFalse(player1.IsAlive);
            Assert.IsFalse(player2.IsAlive);
            Assert.IsFalse(grid.AllPlayers.Any());
        }

        [TestMethod]
        public void TwoPlayersOnDistinctPositionGetKilledByBomb()
        {
            const string Player1Name = "John1";
            const string Player2Name = "John2";
            var intelligence = new ManualIntelligence();

            var grid = new BoundedTestGameGrid(
                4, 3,
                new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>[]
                {
                    new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>("Team A", new[] { new PlayerDefinition(Player1Name) }, intelligence),
                    new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>("Team B", new[] { new PlayerDefinition(Player2Name) }, intelligence)
                },
                new Dictionary<string, Position>() { { Player1Name, new Position(1, 1) }, { Player2Name, new Position(2, 1) } },
                _ =>
                {
                });

            var player1 = grid.AllPlayers.Single(p => p.Name == Player1Name);
            var player2 = grid.AllPlayers.Single(p => p.Name == Player2Name);

            intelligence.SetNextActions(grid, (Player1Name, PlayerAction.PlaceBomb));
            grid.Update();

            while (((Bomb)grid.GetObjects(1, 1).Single()).DetonateAfter > 1)
            {
                grid.Update();
                Assert.IsTrue(player1.IsAlive);
                Assert.IsTrue(player2.IsAlive);
            }

            grid.Update();
            Assert.IsFalse(player1.IsAlive);
            Assert.IsFalse(player2.IsAlive);
            Assert.IsFalse(grid.AllPlayers.Any());
        }

        [TestMethod]
        public void UncoverBonusesByWallDetonationAndBurnThem()
        {
            const string PlayerName = "John";
            var intelligence = new ManualIntelligence();

            var grid = new BoundedTestGameGrid(
                5, 3,
                new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>[]
                {
                    new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>("Team A", new[] { new PlayerDefinition(PlayerName) }, intelligence)
                },
                new Dictionary<string, Position>() { { PlayerName, new Position(1, 1) } },
                g =>
                {
                    g.AddObject(new Wall(new Position(2, 1), isDestroyable: true));
                    g.AddObject(new Bonus(new Position(2, 1), BonusType.Fire));

                    g.AddObject(new Wall(new Position(3, 1), isDestroyable: true));
                    g.AddObject(new Bonus(new Position(3, 1), BonusType.Fire));
                });

            var player = grid.AllPlayers.Single(p => p.Name == PlayerName);
            player.BombsFireRadius = 3;
            player.Life = int.MaxValue;

            var wall1 = grid.GetObjects(2, 1).Single(o => o is Wall);
            var wall2 = grid.GetObjects(3, 1).Single(o => o is Wall);

            var bonus1 = grid.GetObjects(2, 1).Single(o => o is Bonus);
            var bonus2 = grid.GetObjects(3, 1).Single(o => o is Bonus);

            PlaceBombAndWaitForDetonation();
            Assert.IsTrue(grid.GetObjects(2, 1).Contains(bonus1));
            Assert.IsFalse(grid.GetObjects(2, 1).Contains(wall1)); //wall should be burned
            Assert.IsTrue(grid.GetObjects(3, 1).Contains(bonus2));
            Assert.IsTrue(grid.GetObjects(3, 1).Contains(wall2));

            PlaceBombAndWaitForDetonation();
            Assert.IsFalse(grid.GetObjects(2, 1).Contains(bonus1)); //bonus should be burned
            Assert.IsFalse(grid.GetObjects(2, 1).Contains(wall1)); //wall should be burned
            Assert.IsTrue(grid.GetObjects(3, 1).Contains(bonus2));
            Assert.IsTrue(grid.GetObjects(3, 1).Contains(wall2));

            PlaceBombAndWaitForDetonation();
            Assert.IsFalse(grid.GetObjects(2, 1).Contains(bonus1)); //bonus should be burned
            Assert.IsFalse(grid.GetObjects(2, 1).Contains(wall1)); //wall should be burned
            Assert.IsTrue(grid.GetObjects(3, 1).Contains(bonus2));
            Assert.IsFalse(grid.GetObjects(3, 1).Contains(wall2)); //wall should be burned

            PlaceBombAndWaitForDetonation();
            Assert.IsFalse(grid.GetObjects(2, 1).Contains(bonus1)); //bonus should be burned
            Assert.IsFalse(grid.GetObjects(2, 1).Contains(wall1)); //wall should be burned
            Assert.IsFalse(grid.GetObjects(3, 1).Contains(bonus2)); //bonus should be burned
            Assert.IsFalse(grid.GetObjects(3, 1).Contains(wall2)); //wall should be burned

            void PlaceBombAndWaitForDetonation()
            {
                while (grid.GetObjects(1, 1).Any(o => o is BombDetonationFire))
                {
                    grid.Update();
                }

                intelligence.SetNextActions(grid, (PlayerName, PlayerAction.PlaceBomb));
                grid.Update();
                while (((Bomb)grid.GetObjects(1, 1).Single()).DetonateAfter > 1)
                {
                    grid.Update();
                }
                grid.Update();
            }
        }

        [TestMethod]
        public void UncoverBonusesByWallDetonationAndBurnThem_TwoBombs()
        {
            const string PlayerName = "John";
            var intelligence = new ManualIntelligence();

            var grid = new BoundedTestGameGrid(
                6, 3,
                new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>[]
                {
                    new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>("Team A", new[] { new PlayerDefinition(PlayerName) }, intelligence)
                },
                new Dictionary<string, Position>() { { PlayerName, new Position(1, 1) } },
                g =>
                {
                    g.AddObject(new Wall(new Position(3, 1), isDestroyable: true));
                    g.AddObject(new Bonus(new Position(3, 1), BonusType.Fire));

                    g.AddObject(new Wall(new Position(4, 1), isDestroyable: true));
                    g.AddObject(new Bonus(new Position(4, 1), BonusType.Fire));
                });

            var player = grid.AllPlayers.Single(p => p.Name == PlayerName);
            player.BombsFireRadius = 3;
            player.MaxPossibleNumberOfBombs = 2;
            player.Life = int.MaxValue;

            var wall1 = grid.GetObjects(3, 1).Single(o => o is Wall);
            var wall2 = grid.GetObjects(4, 1).Single(o => o is Wall);

            var bonus1 = grid.GetObjects(3, 1).Single(o => o is Bonus);
            var bonus2 = grid.GetObjects(4, 1).Single(o => o is Bonus);

            PlaceBombsAndWaitForDetonation();
            Assert.IsTrue(grid.GetObjects(3, 1).Contains(bonus1));
            Assert.IsFalse(grid.GetObjects(3, 1).Contains(wall1)); //wall should be burned
            Assert.IsTrue(grid.GetObjects(4, 1).Contains(bonus2));
            Assert.IsTrue(grid.GetObjects(4, 1).Contains(wall2));

            PlaceBombsAndWaitForDetonation();
            Assert.IsFalse(grid.GetObjects(3, 1).Contains(bonus1)); //bonus should be burned
            Assert.IsFalse(grid.GetObjects(3, 1).Contains(wall1)); //wall should be burned
            Assert.IsTrue(grid.GetObjects(4, 1).Contains(bonus2));
            Assert.IsTrue(grid.GetObjects(4, 1).Contains(wall2));

            PlaceBombsAndWaitForDetonation();
            Assert.IsFalse(grid.GetObjects(3, 1).Contains(bonus1)); //bonus should be burned
            Assert.IsFalse(grid.GetObjects(3, 1).Contains(wall1)); //wall should be burned
            Assert.IsTrue(grid.GetObjects(4, 1).Contains(bonus2));
            Assert.IsFalse(grid.GetObjects(4, 1).Contains(wall2)); //wall should be burned

            PlaceBombsAndWaitForDetonation();
            Assert.IsFalse(grid.GetObjects(3, 1).Contains(bonus1)); //bonus should be burned
            Assert.IsFalse(grid.GetObjects(3, 1).Contains(wall1)); //wall should be burned
            Assert.IsFalse(grid.GetObjects(4, 1).Contains(bonus2)); //bonus should be burned
            Assert.IsFalse(grid.GetObjects(4, 1).Contains(wall2)); //wall should be burned

            void PlaceBombsAndWaitForDetonation()
            {
                while (grid.GetObjects(1, 1).Any(o => o is BombDetonationFire))
                {
                    grid.Update();
                }

                intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveRight));
                grid.Update();
                intelligence.SetNextActions(grid, (PlayerName, PlayerAction.PlaceBomb));
                grid.Update();
                intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveLeft));
                grid.Update();
                intelligence.SetNextActions(grid, (PlayerName, PlayerAction.PlaceBomb));
                grid.Update();
                Assert.IsTrue(grid.GetObjects(1, 1).Single() is Bomb);
                Assert.IsTrue(grid.GetObjects(2, 1).Single() is Bomb);
                while (((Bomb)grid.GetObjects(2, 1).Single()).DetonateAfter > 1)
                {
                    grid.Update();
                }
                grid.Update();
            }
        }

        [TestMethod]
        public void DestroyWallsByDetonation_TwoBombs()
        {
            const string PlayerName = "John";
            var intelligence = new ManualIntelligence();

            var grid = new BoundedTestGameGrid(
                6, 4,
                new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>[]
                {
                    new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>("Team A", new[] { new PlayerDefinition(PlayerName) }, intelligence)
                },
                new Dictionary<string, Position>() { { PlayerName, new Position(1, 1) } },
                g =>
                {
                    g.AddObject(new Wall(new Position(3, 1), isDestroyable: true));
                    g.AddObject(new Wall(new Position(4, 1), isDestroyable: true));
                    g.AddObject(new Wall(new Position(2, 2), isDestroyable: true));
                });

            var player = grid.AllPlayers.Single(p => p.Name == PlayerName);
            player.BombsFireRadius = 3;
            player.MaxPossibleNumberOfBombs = 2;
            player.Life = int.MaxValue;

            var wall1 = grid.GetObjects(3, 1).Single(o => o is Wall);
            var wall2 = grid.GetObjects(4, 1).Single(o => o is Wall);
            var wall3 = grid.GetObjects(2, 2).Single(o => o is Wall);

            PlaceBombsAndWaitForDetonation();
            Assert.IsFalse(grid.GetObjects(3, 1).Contains(wall1)); //only first wall in the row should be burned
            Assert.IsTrue(grid.GetObjects(4, 1).Contains(wall2));
            Assert.IsFalse(grid.GetObjects(2, 2).Contains(wall3)); //wall should be burned

            PlaceBombsAndWaitForDetonation();
            Assert.IsFalse(grid.GetObjects(3, 1).Contains(wall1)); //wall should be burned
            Assert.IsFalse(grid.GetObjects(4, 1).Contains(wall2)); //wall should be burned
            Assert.IsFalse(grid.GetObjects(2, 2).Contains(wall3)); //wall should be burned

            void PlaceBombsAndWaitForDetonation()
            {
                while (grid.GetObjects(1, 1).Any(o => o is BombDetonationFire))
                {
                    grid.Update();
                }

                intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveRight));
                grid.Update();
                intelligence.SetNextActions(grid, (PlayerName, PlayerAction.PlaceBomb));
                grid.Update();
                intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveLeft));
                grid.Update();
                intelligence.SetNextActions(grid, (PlayerName, PlayerAction.PlaceBomb));
                grid.Update();
                Assert.IsTrue(grid.GetObjects(1, 1).Single() is Bomb);
                Assert.IsTrue(grid.GetObjects(2, 1).Single() is Bomb);
                while (((Bomb)grid.GetObjects(2, 1).Single()).DetonateAfter > 1)
                {
                    grid.Update();
                }
                grid.Update();
            }
        }

        [TestMethod]
        public void TryPlaceMoreBombsThanAvailable()
        {
            const string PlayerName = "John";
            var intelligence = new ManualIntelligence();

            var grid = new BoundedTestGameGrid(
                16, 3,
                new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>[]
                {
                    new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>("Team A", new[] { new PlayerDefinition(PlayerName) }, intelligence)
                },
                new Dictionary<string, Position>() { { PlayerName, new Position(1, 1) } },
                _ =>
                {
                });

            var player = grid.AllPlayers.Single(p => p.Name == PlayerName);
            player.BombsDetonationTime = 100;
            player.MaxPossibleNumberOfBombs = 3;

            for (int i = 0; i < 10; i++)
            {
                PlaceBombAndMoveRight();
            }
            Assert.AreEqual(player.MaxPossibleNumberOfBombs, grid.AllObjects.OfType<Bomb>().Count());
            Assert.AreEqual(0, player.AvailableBombs);

            void PlaceBombAndMoveRight()
            {
                intelligence.SetNextActions(grid, (PlayerName, PlayerAction.PlaceBomb));
                grid.Update();
                intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveRight));
                grid.Update();
            }
        }
    }
}
