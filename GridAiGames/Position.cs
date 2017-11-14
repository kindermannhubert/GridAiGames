using System;

namespace GridAiGames
{
    public struct Position : IEquatable<Position>
    {
        public int X;
        public int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position Left => new Position(X - 1, Y);
        public Position Up => new Position(X, Y + 1);
        public Position Right => new Position(X + 1, Y);
        public Position Down => new Position(X, Y - 1);

        public int DistanceSquared(Position position)
        {
            var dx = (X - position.X);
            var dy = (Y - position.Y);
            return dx * dx + dy * dy;
        }

        public override bool Equals(object obj) => obj is Position && Equals((Position)obj);
        public bool Equals(Position other) => X == other.X && Y == other.Y;
        public override string ToString() => $"({X}, {Y})";

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Position a, Position b) => a.Equals(b);
        public static bool operator !=(Position a, Position b) => !a.Equals(b);

        public static Position operator +(Position a, Position b) => new Position(a.X + b.X, a.Y + b.Y);
        public static Position operator -(Position a, Position b) => new Position(a.X - b.X, a.Y - b.Y);
    }
}