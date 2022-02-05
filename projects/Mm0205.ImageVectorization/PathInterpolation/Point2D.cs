using System.Diagnostics.CodeAnalysis;

namespace Mm0205.ImageVectorization.PathInterpolation;

/// <summary>
/// 2次元デカルト座標。
/// </summary>
[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
public readonly struct Point2D
{
    /// <summary>
    /// X座標。
    /// </summary>
    public double X { get; init; }

    /// <summary>
    /// Y座標。
    /// </summary>
    public double Y { get; init; }

    #region override object

    public override string ToString() => $"({X}, {Y})";

    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <summary>
    /// この構造体の比較は double型を == で比較しているので、「等しいとみなせるか？」の判定には使えない！<br/>
    /// パスが一周したか否かの比較にのみ使用している。
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj) => obj is Point2D other && Equals(other);

    #endregion

    #region 比較

    private bool Equals(Point2D other) => X == other.X && Y == other.Y;

    public static bool operator ==(
        Point2D left,
        Point2D right
    )
    {
        return left.Equals(right);
    }

    public static bool operator !=(
        Point2D left,
        Point2D right
    )
    {
        return !(left == right);
    }
    
    #endregion

    public static Point2D operator +(
        Point2D lhs,
        Point2D rhs
    ) => new()
    {
        X = lhs.X + rhs.X,
        Y = lhs.Y + rhs.Y,
    };

    public static Point2D operator -(
        Point2D lhs,
        Point2D rhs
    ) => new()
    {
        X = lhs.X - rhs.X,
        Y = lhs.Y - rhs.Y,
    };

    public static Point2D operator *(
        Point2D point,
        double x
    ) => new()
    {
        X = point.X * x,
        Y = point.Y * x,
    };

    public static Point2D operator /(
        Point2D point,
        double x
    ) => new()
    {
        X = point.X / x,
        Y = point.Y / x,
    };

    public double ComputeDistance2To(Point2D other) =>
        Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2);
}