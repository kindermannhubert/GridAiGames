using System;
using System.Text;

namespace GridAiGames.Bomberman.Gui
{
    internal class ConsoleRenderer
    {
        private readonly StringBuilder renderingStringBuilder = new StringBuilder();
        private readonly GameGrid grid;

        public ConsoleRenderer(GameGrid grid)
        {
            this.grid = grid;
        }

        public void Render()
        {
            if (Console.WindowHeight == 0) return;
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);
            renderingStringBuilder.Clear();

            for (int y = grid.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    bool anyPlayer = false;
                    foreach (var player in grid.GetPlayers(x, y))
                    {
                        renderingStringBuilder.Append(player.Name[0]);
                        anyPlayer = true;
                        break;
                    }
                    if (anyPlayer) continue;

                    bool anyObject = false;
                    foreach (var obj in grid.GetObjects(x, y))
                    {
                        switch (obj)
                        {
                            case Wall w:
                                if (w.IsDestroyable) renderingStringBuilder.Append('%');
                                else renderingStringBuilder.Append('#');
                                anyObject = true;
                                break;
                            case Bomb _:
                                renderingStringBuilder.Append('@');
                                anyObject = true;
                                break;
                            case BombDetonationFire _:
                                renderingStringBuilder.Append('*');
                                anyObject = true;
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown object type: '{obj.GetType().FullName}'.");
                        }
                        break;
                    }
                    if (!anyObject)
                    {
                        renderingStringBuilder.Append(' ');
                    }
                }
                renderingStringBuilder.AppendLine();
            }

            Console.WriteLine(renderingStringBuilder);
        }
    }
}
