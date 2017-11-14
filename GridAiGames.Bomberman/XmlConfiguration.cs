using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace GridAiGames.Bomberman
{
    [XmlRoot("Configuration")]
    public class XmlConfiguration
    {
        public XmlMap Map { get; set; }

        [XmlArray]
        [XmlArrayItem("Team")]
        public XmlTeam[] Teams { get; set; }

        public IReadOnlyList<TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>> CreateTeamDefinitions()
            => Teams.Select(t => t.CreateTeamDefinition()).ToList();

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

            public TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction> CreateTeamDefinition()
            {
                if (!Intelligence.TryCreate(out var intelligence))
                {
                    intelligence = new DummyIntelligence();
                }

                var playerDefinitions = Players.Select(p => p.CreatePlayerDefinition()).ToList();

                return new TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>(Name, playerDefinitions, intelligence);
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

                public bool TryCreate(out IIntelligence<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction> intelligence)
                {
                    try
                    {
                        var path = Path.IsPathRooted(AssemblyPath) ? AssemblyPath : Path.GetFullPath(AssemblyPath);
                        var assembly = Assembly.LoadFile(path);
                        var type = assembly.GetType(TypeFullName);
                        intelligence = (IIntelligence<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>)Activator.CreateInstance(type);
                        return true;
                    }
                    catch
                    {
                        intelligence = null;
                        return false;
                    }
                }
            }
        }
    }
}
