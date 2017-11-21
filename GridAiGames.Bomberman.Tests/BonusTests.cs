using System;
using System.Collections.Generic;
using System.Linq;
using GridAiGames.Bomberman.ReadOnly;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GridAiGames.Bomberman.Tests
{
    [TestClass]
    public class BonusTests
    {
        [TestMethod]
        public void ConsumeFireBonusTest()
        {
            ConsumeBonuses(
                pos => new Bonus(pos, BonusType.Fire),
                player => player.BombsFireRadius,
                bombsFireRadius => bombsFireRadius + 1);
        }

        [TestMethod]
        public void NonBlockedByBonusMovementTest()
        {
            ConsumeBonuses(
                pos => new Bonus(pos, BonusType.Bomb),
                player => player.MaxPossibleNumberOfBombs,
                maxPossibleNumberOfBombs => maxPossibleNumberOfBombs + 1);
        }

        private void ConsumeBonuses<AffectedPropertyType>(
            Func<Position, GameObject<Player, PlayerAction>> createNonBlockingObject,
            Func<Player, AffectedPropertyType> getAffectedPropertyValue,
            Func<AffectedPropertyType, AffectedPropertyType> changePropertyValueByBonus)
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

            var affectedPropertyValue = getAffectedPropertyValue(player);

            grid.Update();
            Assert.AreEqual(affectedPropertyValue = changePropertyValueByBonus(affectedPropertyValue), getAffectedPropertyValue(player));

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveRight));
            grid.Update();
            Assert.AreEqual(affectedPropertyValue = changePropertyValueByBonus(affectedPropertyValue), getAffectedPropertyValue(player));

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveUp));
            grid.Update();
            Assert.AreEqual(affectedPropertyValue = changePropertyValueByBonus(affectedPropertyValue), getAffectedPropertyValue(player));

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveLeft));
            grid.Update();
            Assert.AreEqual(affectedPropertyValue = changePropertyValueByBonus(affectedPropertyValue), getAffectedPropertyValue(player));

            //everything is consumed now
            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveDown));
            grid.Update();
            Assert.AreEqual(affectedPropertyValue, getAffectedPropertyValue(player));

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveRight));
            grid.Update();
            Assert.AreEqual(affectedPropertyValue, getAffectedPropertyValue(player));

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveUp));
            grid.Update();
            Assert.AreEqual(affectedPropertyValue, getAffectedPropertyValue(player));

            intelligence.SetNextActions(grid, (PlayerName, PlayerAction.MoveLeft));
            grid.Update();
            Assert.AreEqual(affectedPropertyValue, getAffectedPropertyValue(player));
        }
    }
}
