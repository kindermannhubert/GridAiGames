using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using GridAiGames.Bomberman.Gui;
using GridAiGames.Logging;

namespace GridAiGames.Bomberman
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly ILogger logger;
        private readonly Thread backgroundThread;
        private bool cancelGame;
        private GuiTeam[] teams;

        public GuiTeam[] Teams
        {
            get => teams;
            set
            {
                teams = value;
                OnPropertyChanged();
            }
        }

        public SpeedMultiplierWrap[] SpeedMultipliers { get; } = new SpeedMultiplierWrap[] { 1d / 16, 1d / 8, 1d / 4, 1d / 2, 1, 2, 4 };
        public SpeedMultiplierWrap SpeedMultiplier { get; set; }

        public MainWindow()
        {
            DataContext = this;

            logger = new ConsoleLogger();
            //FreeConsole(); //if not using ConsoleLogger

            InitializeComponent();

            var cfg = LoadConfiguration();

            var rand = new Random(2);
            var grid = new GameGrid(
                cfg.Map.Width, cfg.Map.Height,
                cfg.CreateTeamDefinitions(logger),
                (teamDef, playerDef) => new Position(1 + 2 * rand.Next(1, cfg.Map.Width / 2 - 1), 1 + 2 * rand.Next(0, cfg.Map.Height / 2 - 1)),
                AddObjectsToGameGrid,
                rand,
                logger);

            renderingPanel.WpfRenderer = new WpfRenderer(grid);

            Teams = grid.AllPlayers.GroupBy(p => p.TeamName)
                        .Select(g => new GuiTeam(g.Key, g.Select(p => new GuiTeam.GuiPlayer(p.Name, renderingPanel.WpfRenderer.GetPlayerTextures(p).PlayerFrontImage, p.IsAlive))))
                        .ToArray();

            grid.PlayerIsAliveChanged +=
                player =>
                {
                    Teams.SelectMany(t => t.Players).Single(p => p.Name == player.Name).IsAlive = player.IsAlive;
                };

            backgroundThread = new Thread(() =>
                {
                    ulong iteration = 0;

                    while (!cancelGame)
                    {
                        int UpdatingIntervalMs = (int)Math.Round(200 / SpeedMultiplier.Multiplier);
                        int renderFramesBetweenUpdate = (int)Math.Ceiling(0.03 * UpdatingIntervalMs); //30 fps
                        grid.Update(iteration);

                        //var currentTeamsCount = grid.AllPlayers.Select(p => p.TeamName).Distinct().Count();
                        //if (currentTeamsCount < 2)
                        //{
                        //    if (currentTeamsCount == 0)
                        //    {
                        //        Console.WriteLine("Nobody wins.");
                        //        break;
                        //    }
                        //    if (currentTeamsCount == 1)
                        //    {
                        //        Console.WriteLine($"Team '{grid.AllPlayers}' won.");
                        //        break;
                        //    }
                        //}

                        renderingPanel.GameIteration = iteration;
                        for (int i = 0; i < renderFramesBetweenUpdate; i++)
                        {
                            renderingPanel.GameIteration = iteration + (double)i / renderFramesBetweenUpdate;
                            try
                            {
                                Dispatcher.Invoke(renderingPanel.InvalidateVisual);
                            }
                            catch (TaskCanceledException)
                            {
                                break;
                            }
                            Thread.Sleep(TimeSpan.FromMilliseconds(UpdatingIntervalMs / renderFramesBetweenUpdate));
                        }

                        iteration++;
                    }
                });
            backgroundThread.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            cancelGame = true;
            backgroundThread.Join(500);

            base.OnClosing(e);
        }

        private XmlConfiguration LoadConfiguration()
        {
            using (var stream = File.OpenText("GridAiGames.Bomberman.Configuration.xml"))
            {
                var serializer = new XmlSerializer(typeof(XmlConfiguration));
                return (XmlConfiguration)serializer.Deserialize(stream);
            }
        }

        private static void AddObjectsToGameGrid(GameGrid grid)
        {
            if (grid.Width % 2 == 1 || grid.Height % 2 == 1) throw new InvalidOperationException("Grid dimensions should be even numbers.");

            //borders
            for (int x = 0; x < grid.Width; x++)
            {
                grid.AddObject(new Wall(new Position(x, 0)));
                grid.AddObject(new Wall(new Position(x, grid.Height - 1)));
            }
            for (int y = 1; y < grid.Height - 1; y++)
            {
                grid.AddObject(new Wall(new Position(0, y)));
                grid.AddObject(new Wall(new Position(grid.Width - 1, y)));
            }

            for (int y = 1; y < grid.Height - 1; y++)
                for (int x = 1; x < grid.Width - 1; x++)
                {
                    if (x % 2 == 0 && y % 2 == 0)
                    {
                        grid.AddObject(new Wall(new Position(x, y)));
                    }
                }

            var freePostionsQueue = new Queue<Position>();
            var usedCells = new bool[grid.Width, grid.Height];

            //randomization of order of wall creation
            //awful but works
            var indices = Enumerable.Range(1, grid.Height - 2).SelectMany(y => Enumerable.Range(1, grid.Width - 2).Select(x => (x, y)))
                            .Select(i => (i, r: grid.Random.Next()))
                            .OrderBy(i => i.r)
                            .Select(i => i.i);

            foreach (var (x, y) in indices)
            {
                if (/*grid.Random.NextDouble() > 0.2 &&*/
                    !grid.GetObjects(x, y).Any() &&
                    !grid.GetPlayers(x, y).Any() &&
                    grid.AllPlayers.All(p => DoesPlayerHaveSpaceForStart(p.Position, new Position(x, y))))
                {
                    if (grid.Random.NextDouble() < 0.3)
                    {
                        grid.AddObject(new Bonus(new Position(x, y), (BonusType)grid.Random.Next((int)BonusType._MaxValue + 1)));
                    }
                    grid.AddObject(new Wall(new Position(x, y), isDestroyable: true));
                }
            }

            bool DoesPlayerHaveSpaceForStart(Position playerPosition, Position newWallPosition)
            {
                Array.Clear(usedCells, 0, usedCells.GetLength(0) * usedCells.GetLength(1));
                usedCells[playerPosition.X, playerPosition.Y] = true;
                freePostionsQueue.Enqueue(playerPosition);
                return DoesPlayerHaveSpaceForStartInternal(playerPosition, newWallPosition);
            }

            bool DoesPlayerHaveSpaceForStartInternal(Position playerPosition, Position newWallPosition)
            {
                if (freePostionsQueue.Count == 0) return false;

                var freePosition = freePostionsQueue.Dequeue();

                if (playerPosition.X != freePosition.X && playerPosition.Y != freePosition.Y)
                {
                    freePostionsQueue.Clear();
                    return true;
                }

                var newPos = freePosition.Left;
                if (newPos != newWallPosition && !usedCells[newPos.X, newPos.Y] && !grid.GetObjects(newPos).Any())
                {
                    usedCells[newPos.X, newPos.Y] = true;
                    freePostionsQueue.Enqueue(newPos);
                }

                newPos = freePosition.Up;
                if (newPos != newWallPosition && !usedCells[newPos.X, newPos.Y] && !grid.GetObjects(newPos).Any())
                {
                    usedCells[newPos.X, newPos.Y] = true;
                    freePostionsQueue.Enqueue(newPos);
                }

                newPos = freePosition.Right;
                if (newPos != newWallPosition && !usedCells[newPos.X, newPos.Y] && !grid.GetObjects(newPos).Any())
                {
                    usedCells[newPos.X, newPos.Y] = true;
                    freePostionsQueue.Enqueue(newPos);
                }

                newPos = freePosition.Down;
                if (newPos != newWallPosition && !usedCells[newPos.X, newPos.Y] && !grid.GetObjects(newPos).Any())
                {
                    usedCells[newPos.X, newPos.Y] = true;
                    freePostionsQueue.Enqueue(newPos);
                }

                return DoesPlayerHaveSpaceForStartInternal(playerPosition, newWallPosition);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class GuiTeam
        {
            public string Name { get; }
            public GuiPlayer[] Players { get; }

            public GuiTeam(string name, IEnumerable<GuiPlayer> players)
            {
                Name = name;
                Players = players.ToArray();
            }

            public class GuiPlayer : INotifyPropertyChanged
            {
                private bool isAlive;

                public string Name { get; }
                public ImageSource Image { get; }

                public bool IsAlive
                {
                    get => isAlive;
                    set
                    {
                        isAlive = value;
                        OnPropertyChanged();
                    }
                }

                public GuiPlayer(string name, ImageSource image, bool isAlive)
                {
                    Name = name;
                    Image = image;
                    IsAlive = isAlive;
                }

                public event PropertyChangedEventHandler PropertyChanged;

                private void OnPropertyChanged([CallerMemberName]string propertyName = null)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        public struct SpeedMultiplierWrap
        {
            public readonly double Multiplier;
            public SpeedMultiplierWrap(double multiplier) => Multiplier = multiplier;
            public override string ToString() => Multiplier >= 1 ? Multiplier.ToString() : $"1 / {1 / Multiplier}";
            public static implicit operator SpeedMultiplierWrap(double value) => new SpeedMultiplierWrap(value);
        }

        [DllImport("kernel32")]
        private static extern bool FreeConsole();
    }
}