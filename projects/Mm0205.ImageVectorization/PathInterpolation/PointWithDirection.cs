namespace Mm0205.ImageVectorization.PathInterpolation;

/// <summary>
/// 点と方向のセット。
/// </summary>
public readonly struct PointWithDirection
{
    /// <summary>
    /// 補間点。
    /// </summary>
    public Point2D Point { get; init; }

    /// <summary>
    /// 補間点の方向。
    /// </summary>
    public EightDirection Direction { get; init; }


    public override string ToString() => $"{Point} [{Direction}]";
}