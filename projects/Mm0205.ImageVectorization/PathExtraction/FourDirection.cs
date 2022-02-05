namespace Mm0205.ImageVectorization.PathExtraction;

/// <summary>
/// 4方向。
/// </summary>
public enum FourDirection
{
    South,
    East,
    North,
    West,
}

/// <summary>
/// 4方向の拡張。
/// </summary>
public static class FourDirectionExtension
{
    /// <summary>
    /// 元の方向を反時計回りに90°回転させた方向を取得する。
    /// </summary>
    /// <param name="direction">元の方向。</param>
    /// <returns>元の方向を反時計回りに90°回転させた方向</returns>
    /// <exception cref="ArgumentOutOfRangeException">不正な方向。</exception>
    public static FourDirection TurnLeft(this FourDirection direction) =>
        direction switch
        {
            FourDirection.South => FourDirection.East,
            FourDirection.East => FourDirection.North,
            FourDirection.North => FourDirection.West,
            FourDirection.West => FourDirection.South,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "不正な方向")
        };
    
    /// <summary>
    /// 元の方向を時計回りに90°回転させた方向を取得する。
    /// </summary>
    /// <param name="direction">元の方向。</param>
    /// <returns>元の方向を反時計回りに90°回転させた方向</returns>
    /// <exception cref="ArgumentOutOfRangeException">不正な方向。</exception>
    public static FourDirection TurnRight(this FourDirection direction) =>
        direction switch
        {
            FourDirection.South => FourDirection.West,
            FourDirection.East => FourDirection.South,
            FourDirection.North => FourDirection.East,
            FourDirection.West => FourDirection.North,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "不正な方向")
        };
}