using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GridAiGames.Bomberman.Gui
{
    internal class WpfRenderer
    {
        private readonly GameGrid grid;
        private readonly Pen pen = new Pen(Brushes.Black, 1);

        private readonly ImageSource bombImage;
        private readonly ImageSource fire1Image, fire2Image;
        private readonly ImageSource fireBonusImage, bombBonusImage;
        private readonly Dictionary<Player, PlayerTextures> playerTexturesPerPlayer = new Dictionary<Player, PlayerTextures>();

        private readonly Dictionary<Player, Position> lastDistinctPositionPerPlayer = new Dictionary<Player, Position>();

        public WpfRenderer(GameGrid grid)
        {
            this.grid = grid;
            pen.Freeze();

            bombImage = LoadResourceImage("Bomb.png");
            fire1Image = LoadResourceImage("Fire1.png");
            fire2Image = LoadResourceImage("Fire2.png");
            fireBonusImage = LoadResourceImage("FireBonus.png");
            bombBonusImage = LoadResourceImage("BombBonus.png");

            var playerFrontGrayscaleImage = LoadResourceImage("PlayerFront.png");
            var playerRightGrayscaleImage = LoadResourceImage("PlayerRight.png");
            var playerBackGrayscaleImage = LoadResourceImage("PlayerBack.png");

            var allPlayers = grid.AllPlayers.ToList();
            var playerColors = GenerateDistinctColors(allPlayers.Count);
            for (int i = 0; i < allPlayers.Count; i++)
            {
                var color = playerColors[i];

                playerTexturesPerPlayer.Add(
                    allPlayers[i],
                    new PlayerTextures(
                        ColorUpPlayer(playerFrontGrayscaleImage, color),
                        ColorUpPlayer(playerRightGrayscaleImage, color),
                        ColorUpPlayer(playerBackGrayscaleImage, color)));
            }
        }

        public void Render(DrawingContext context, double width, double height, double gameIteration)
        {
            var scale = width / grid.Width < height / grid.Height ? width / grid.Width : height / grid.Height;
            var cellSize = scale;

            context.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));

            context.PushTransform(new ScaleTransform(1, -1));
            context.PushTransform(new TranslateTransform(0, -cellSize * grid.Height));
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    var cellRectangle = new Rect(x * scale, y * scale, cellSize, cellSize);

                    foreach (var obj in grid.GetObjects(x, y))
                    {
                        Brush brush;

                        switch (obj)
                        {
                            case Wall w:
                                if (w.IsDestroyable) brush = Brushes.LightGray;
                                else brush = Brushes.Gray;
                                context.DrawRectangle(brush, pen, cellRectangle);
                                break;
                            case Bomb _:
                                brush = Brushes.Red;

                                context.PushTransform(new TranslateTransform(x * scale + 0.5 * cellSize, y * scale + 0.5 * cellSize));

                                var scaleBomb = gameIteration % 2 == 0 ? (0.8 + 0.2 * (gameIteration - (int)gameIteration)) : (0.8 + 0.2 * (1 - gameIteration + (int)gameIteration));
                                context.PushTransform(new ScaleTransform(scaleBomb, scaleBomb));
                                DrawImage(context, bombImage, new Rect(-0.5 * cellSize, -0.5 * cellSize, cellSize, cellSize));
                                context.Pop();
                                context.Pop();
                                //context.DrawImage(bombImage, cellRectangle);
                                break;
                            case BombDetonationFire f:
                                brush = Brushes.Orange;
                                //context.DrawRectangle(brush, pen, cellRectangle);

                                switch (f.Type)
                                {
                                    case BombDetonationFireType.Horizontal:
                                        DrawImage(context, x % 2 == 0 ? fire1Image : fire2Image, cellRectangle);
                                        break;
                                    case BombDetonationFireType.Vertical:
                                        context.PushTransform(new TranslateTransform(x * scale + 0.5 * cellSize, y * scale + 0.5 * cellSize));
                                        context.PushTransform(new RotateTransform(90));
                                        DrawImage(context, y % 2 == 0 ? fire1Image : fire2Image, new Rect(-0.5 * cellSize, -0.5 * cellSize, cellSize, cellSize));
                                        context.Pop();
                                        context.Pop();
                                        break;
                                    case BombDetonationFireType.Center:
                                        DrawImage(context, x % 2 == 0 ? fire1Image : fire2Image, cellRectangle);

                                        context.PushTransform(new TranslateTransform(x * scale + 0.5 * cellSize, y * scale + 0.5 * cellSize));
                                        context.PushTransform(new RotateTransform(90));
                                        DrawImage(context, y % 2 == 0 ? fire1Image : fire2Image, new Rect(-0.5 * cellSize, -0.5 * cellSize, cellSize, cellSize));
                                        context.Pop();
                                        context.Pop();
                                        break;
                                    default:
                                        throw new InvalidOperationException($"Unknown fire type: '{f.Type}'.");
                                }
                                break;
                            case Bonus b:
                                if (!grid.GetObjects(x, y).Any(o => o is BombDetonationFire))
                                {
                                    switch (b.Type)
                                    {
                                        case BonusType.Bomb:
                                            DrawImage(context, bombBonusImage, cellRectangle);
                                            break;
                                        case BonusType.Fire:
                                            DrawImage(context, fireBonusImage, cellRectangle);
                                            break;
                                        default:
                                            throw new InvalidOperationException($"Unknown bonus type: '{b.Type}'.");
                                    }
                                }
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown object type: '{obj.GetType().FullName}'.");
                        }
                    }
                }
            }

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    foreach (var player in grid.GetPlayers(x, y))
                    {
                        if (!lastDistinctPositionPerPlayer.TryGetValue(player, out var lastDistinctPosition))
                        {
                            lastDistinctPositionPerPlayer.Add(player, lastDistinctPosition = player.Position);
                        }
                        if (player.Position != player.PreviousPosition) lastDistinctPositionPerPlayer[player] = player.PreviousPosition;

                        var t = gameIteration - (int)gameIteration;

                        var renderX = scale * (t * x + (1 - t) * player.PreviousPosition.X);
                        var renderY = scale * (t * y + (1 - t) * player.PreviousPosition.Y);

                        var posDelta = player.Position - lastDistinctPosition;
                        TransformGroup imageTransform = null;
                        ImageSource image = playerTexturesPerPlayer[player].PlayerFrontImage;
                        if (posDelta.X > 0)
                        {
                            image = playerTexturesPerPlayer[player].PlayerRightImage;
                        }
                        else if (posDelta.X < 0)
                        {
                            image = playerTexturesPerPlayer[player].PlayerRightImage;
                            imageTransform = new TransformGroup();
                            imageTransform.Children.Add(new TranslateTransform(-cellSize, 0));
                            imageTransform.Children.Add(new ScaleTransform(-1, 1));
                        }
                        else if (posDelta.Y > 0)
                        {
                            image = playerTexturesPerPlayer[player].PlayerBackImage;
                        }
                        else if (posDelta.Y < 0)
                        {
                            image = playerTexturesPerPlayer[player].PlayerFrontImage;
                        }

                        DrawImage(context, image, new Rect(renderX, renderY, cellSize, 1.5 * cellSize), imageTransform);
                        break;
                    }
                }
            }

            context.Pop();
            context.Pop();
        }

        private static void DrawImage(DrawingContext context, ImageSource image, Rect rectangle, Transform imageTransform = null)
        {
            context.PushTransform(new TranslateTransform(rectangle.X, rectangle.Y + rectangle.Height));
            context.PushTransform(new ScaleTransform(1, -1));
            if (imageTransform != null) context.PushTransform(imageTransform);
            rectangle.X = 0;
            rectangle.Y = 0;
            context.DrawImage(image, rectangle);
            if (imageTransform != null) context.Pop();
            context.Pop();
            context.Pop();
        }

        public PlayerTextures GetPlayerTextures(Player player) => playerTexturesPerPlayer[player];

        private static BitmapFrame LoadResourceImage(string name)
        {
            var uri = new Uri($"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/Gui/Resources/{name}", UriKind.RelativeOrAbsolute);
            var img = BitmapFrame.Create(uri);
            img.Freeze();
            return img;
        }

        private unsafe ImageSource ColorUpPlayer(BitmapFrame img, Color color)
        {
            Debug.Assert(img.Format == PixelFormats.Bgra32);

            var w = img.PixelWidth;
            var h = img.PixelHeight;

            var pixels = new byte[sizeof(RGBA) * w * h];
            img.CopyPixels(pixels, w * sizeof(RGBA), 0);

            fixed (byte* pPixels = pixels)
            {
                var pRGBA = (RGBA*)pPixels;

                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        var p = pRGBA + y * w + x;

                        if (p->R != 0 && p->G == 0 && p->B == 0)
                        {
                            p->G = p->R;
                            p->B = p->R;
                        }
                        else if (p->A != 0)
                        {
                            p->R = (byte)((p->R * color.R) / 255);
                            p->G = (byte)((p->G * color.G) / 255);
                            p->B = (byte)((p->B * color.B) / 255);
                        }
                    }
            }

            return BitmapSource.Create(w, h, img.DpiX, img.DpiY, img.Format, img.Palette, pixels, w * sizeof(RGBA));
        }

        private struct RGBA
        {
            public byte B, G, R, A;

            public RGBA(byte r, byte g, byte b, byte a)
            {
                B = b;
                G = g;
                R = r;
                A = a;
            }
        }

        public struct PlayerTextures
        {
            public readonly ImageSource PlayerFrontImage, PlayerRightImage, PlayerBackImage;

            public PlayerTextures(ImageSource playerFrontImage, ImageSource playerRightImage, ImageSource playerBackImage)
            {
                this.PlayerFrontImage = playerFrontImage;
                this.PlayerRightImage = playerRightImage;
                this.PlayerBackImage = playerBackImage;
            }
        }

        private static Color[] GenerateDistinctColors(int number)
        {
            var colors = new Color[number];
            for (int i = 0; i < number; i++)
            {
                var hsv = new Colorspace.ColorHSV((double)i / number, 1, 1);
                var rgb = new Colorspace.ColorRGB(hsv);
                colors[i] = Color.FromRgb((byte)(255 * rgb.R), (byte)(255 * rgb.G), (byte)(255 * rgb.B));
            }
            return colors;
        }
    }
}