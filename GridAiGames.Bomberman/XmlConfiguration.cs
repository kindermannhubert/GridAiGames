using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using GridAiGames.Logging;

namespace GridAiGames.Bomberman
{
    [XmlRoot("Configuration")]
    public class XmlConfiguration
    {
        public XmlMap Map { get; set; }

        [XmlArray]
        [XmlArrayItem("Team")]
        public XmlTeam[] Teams { get; set; }

        public IReadOnlyList<TeamDefinition<ReadOnly.GameGrid, ReadOnly.IPlayer, PlayerAction>> CreateTeamDefinitions(ILogger logger)
            => Teams.Select(t => t.CreateTeamDefinition(logger)).ToList();

        public class XmlMap
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }

        public class XmlTeam
        {
            public string Name { get; set; }

            [XmlArray]
            [XmlArrayItem("Player")]
            public XmlPlayer[] Players { get; set; }

            public XmlIntelligence Intelligence { get; set; }

            public TeamDefinition<ReadOnly.GameGrid, ReadOnly.IPlayer, PlayerAction> CreateTeamDefinition(ILogger logger)
            {
                if (!Intelligence.TryCreate(out var intelligence, logger))
                {
                    intelligence = new DummyIntelligence();
                }

                var playerDefinitions = Players.Select(p => p.CreatePlayerDefinition()).ToList();

                return new TeamDefinition<ReadOnly.GameGrid, ReadOnly.IPlayer, PlayerAction>(Name, playerDefinitions, intelligence);
            }

            public class XmlPlayer
            {
                public string Name { get; set; }

                public PlayerDefinition CreatePlayerDefinition() => new PlayerDefinition(Name);
            }

            public class XmlIntelligence
            {
                public string AssemblyPath { get; set; }
                public string TypeFullName { get; set; }

                public bool TryCreate(out IIntelligence<ReadOnly.GameGrid, ReadOnly.IPlayer, PlayerAction> intelligence, ILogger logger)
                {
                    try
                    {
                        var path = Path.IsPathRooted(AssemblyPath) ? AssemblyPath : Path.GetFullPath(AssemblyPath);
                        var assembly = Assembly.LoadFile(path);
                        var type = assembly.GetType(TypeFullName);
                        intelligence = (IIntelligence<ReadOnly.GameGrid, ReadOnly.IPlayer, PlayerAction>)Activator.CreateInstance(type);
                        return true;
                    }
                    catch (Exception e)
                    {
                        logger.Log(LogType.Error, $"Error while loading type '{TypeFullName}' from assembly '{AssemblyPath}'. Exception: {e.Message}");
                        intelligence = null;
                        return false;
                    }
                }
            }
        }
    }
}
