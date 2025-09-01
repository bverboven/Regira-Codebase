using System.Diagnostics;

namespace Regira.Dimensions;

[DebuggerDisplay("Point2D = [{X},{Y}]")]
public readonly struct Point2D(float x, float y) : IEquatable<Point2D>
{
    public float X { get; } = x;
    public float Y { get; } = y;

    public Point2D(int x, int y) : this(x, (float)y) { }
    public Point2D() : this(0, 0) { }


    // enable adding/subtracting from points
    public static Point2D operator +(Point2D c1, Point2D c2)
        => new(c1.X + c2.X, c1.Y - c2.Y);
    public static Point2D operator -(Point2D c1, Point2D c2)
        => new(c1.X - c2.X, c1.Y - c2.Y);

    public static implicit operator Point2D(int[] point)
        => point.Length switch
        {
            2 => new Point2D(point[0], point[1]),
            _ => throw new ArgumentOutOfRangeException(nameof(point))
        };
    public static implicit operator Point2D(float[] point)
        => point.Length switch
        {
            2 => new Point2D(point[0], point[1]),
            _ => throw new ArgumentOutOfRangeException(nameof(point))
        };
    public static implicit operator float[](Point2D point)
        => [point.X, point.Y];

    // comparison
    public bool Equals(Point2D other)
        => X.Equals(other.X) && Y.Equals(other.Y);
    public override bool Equals(object? obj)
        => obj is Point2D other && Equals(other);
    public override int GetHashCode()
    {
        unchecked
        {
            return (X.GetHashCode() * 397) ^ Y.GetHashCode();
        }
    }

    /// <summary>
    /// Formatted as [X, Y]
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => $"[{X}, {Y}]";
}