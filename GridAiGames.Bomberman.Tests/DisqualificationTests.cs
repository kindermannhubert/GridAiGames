using System.Collections.Generic;
using System.Linq;
using GridAiGames.Bomberman.ReadOnly;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GridAiGames.Bomberman.Tests
{
    [TestClass]
    public class DisqualificationTests
    {
        [TestMethod]
        public void DisqualifyPlayerBecauseHeWantedToDoMoreActions()
        {
            const string PlayerName = "John";
            var intelligence = new ManualIntelligence();

            var grid = new TestGameGrid(
                2, 2,
                new TeamDefinition<ReadOnly.GameGrid, ReadOnly.IPlayer, PlayerAction>[]
                {
                    new TeamDefinition<ReadOnly.GameGrid, ReadOnly.IPlayer, PlayerAction>("Team A", new[] { new PlayerDefinition(PlayerName) }, intelligence)
                },
                new Dictionary<string, Position>() { { PlayerName, new Position(0, 0) } },
                _ =>
                {
                },
                new TestLogger(canGenerateWarningAndErrors: true));

            var player = grid.AllPlayers.Single(p => p.Name == PlayerName);

            //one action is ok
            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveRight));
            grid.Update();
            Assert.AreEqual(1, grid.AllPlayers.Count());
            Assert.IsTrue(player.IsAlive);

            //but two are not
            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveRight));
            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveRight));
            grid.Update();
            Assert.AreEqual(0, grid.AllPlayers.Count());
            Assert.IsFalse(player.IsAlive);
        }

        [TestMethod]
        public void DisqualifyPlayerBecauseHeWantedToUnsupportedAction()
        {
            const string PlayerName = "John";
            var intelligence = new ManualIntelligence();

            var grid = new TestGameGrid(
                2, 2,
                new TeamDefinition<ReadOnly.GameGrid, ReadOnly.IPlayer, PlayerAction>[]
                {
                    new TeamDefinition<ReadOnly.GameGrid, ReadOnly.IPlayer, PlayerAction>("Team A", new[] { new PlayerDefinition(PlayerName) }, intelligence)
                },
                new Dictionary<string, Position>() { { PlayerName, new Position(0, 0) } },
                _ =>
                {
                },
                new TestLogger(canGenerateWarningAndErrors: true));

            var player = grid.AllPlayers.Single(p => p.Name == PlayerName);

            //known action is ok
            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveRight));
            grid.Update();
            Assert.AreEqual(1, grid.AllPlayers.Count());
            Assert.IsTrue(player.IsAlive);

            //unknown is not
            intelligence.SetNextActions(grid, (PlayerName, (PlayerAction)9999));
            grid.Update();
            Assert.AreEqual(0, grid.AllPlayers.Count());
            Assert.IsFalse(player.IsAlive);
        }

        [TestMethod]
        public void DisqualifyPlayerBecauseHisIntelligenceHasThrownException()
        {
            const string PlayerName = "John";
            var intelligence = new ExceptionThrowingIntelligence();

            var grid = new TestGameGrid(
                2, 2,
                new TeamDefinition<ReadOnly.GameGrid, ReadOnly.IPlayer, PlayerAction>[]
                {
                    new TeamDefinition<ReadOnly.GameGrid, ReadOnly.IPlayer, PlayerAction>("Team A", new[] { new PlayerDefinition(PlayerName) }, intelligence)
                },
                new Dictionary<string, Position>() { { PlayerName, new Position(0, 0) } },
                _ =>
                {
                },
                new TestLogger(canGenerateWarningAndErrors: true));

            var player = grid.AllPlayers.Single(p => p.Name == PlayerName);

            Assert.AreEqual(1, grid.AllPlayers.Count());
            Assert.IsTrue(player.IsAlive);

            grid.Update();
            Assert.AreEqual(0, grid.AllPlayers.Count());
            Assert.IsFalse(player.IsAlive);
        }

        internal class ExceptionThrowingIntelligence : IBombermanIntelligence
        {
            public void Initialize(ReadOnly.GameGrid gameGrid, IReadOnlyList<ReadOnly.IPlayer> teamPlayers)
            {
            }

            public IEnumerable<(string playerName, PlayerAction action)>
                GetActionsForTeam(
                    ReadOnly.GameGrid gameGrid,
                    IReadOnlyList<ReadOnly.IPlayer> teamPlayers)
            {
                throw null;
            }
        }
    }
}
