namespace Mm0205.ImageVectorization.PathExtraction;

/// <summary>
/// ビットマップ上の格子点を示す座標。<br/>
/// ※ (0, 0) が一番左上。<br/>
/// ※ (0, 0) に対応するピクセルは一番左上のピクセル。<br/>
/// つまり、格子点L(i, j) は ピクセルP(i, j) の左上の点を指す。<br/>
/// ピクセルP(i,j)は TopLeft = (i, j)、 BottomRight = (i + 1, j + 1) の正方形と看做す。
/// </summary>
public readonly struct LatticePoint
{
    /// <summary>
    /// X座標。
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Y座標。
    /// </summary>
    public int Y { get; }
    
    public LatticePoint(
        int x,
        int y
    )
    {
        X = x;
        Y = y;
    }

    #region override (object)

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override bool Equals(object? obj)
    {
        if (obj is LatticePoint p)
        {
            return Equals(p);
        }

        return false;
    }

    public override string ToString() => $"({X}, {Y})";

    #endregion

    #region 比較

    // ReSharper disable once MemberCanBePrivate.Global
    public bool Equals(LatticePoint obj) => X == obj.X && Y == obj.Y;

    public static bool operator ==(
        LatticePoint lhs,
        LatticePoint rhs
    ) => lhs.Equals(rhs);

    public static bool operator !=(
        LatticePoint lhs,
        LatticePoint rhs
    ) => !(lhs == rhs);

    #endregion

    /// <summary>
    /// 指定された方向(<paramref name="direction"/>)に座標を1つ移動させた点を取得する。
    /// </summary>
    /// <param name="direction">方向。</param>
    /// <returns>指定された方向(<paramref name="direction"/>)に座標を1つ移動させた点。</returns>
    public LatticePoint NextPoint(in FourDirection direction) =>
        new(
            NextX(direction),
            NextY(direction)
        );

    /// <summary>
    /// 指定された方向(<paramref name="direction"/>)にY座標を1移動させた点を取得する。
    /// </summary>
    /// <param name="direction">方向。</param>
    /// <returns>指定された方向(<paramref name="direction"/>)にY座標を1移動させた点。</returns>
    private int NextY(FourDirection direction) =>
        Y + direction switch
        {
            FourDirection.South => 1,
            FourDirection.North => -1,
            _ => 0
        };

    /// <summary>
    /// 指定された方向(<paramref name="direction"/>)にX座標を1移動させた点を取得する。
    /// </summary>
    /// <param name="direction">方向。</param>
    /// <returns>指定された方向(<paramref name="direction"/>)にX座標を1移動させた点。</returns>
    private int NextX(FourDirection direction) =>
        X + direction switch
        {
            FourDirection.West => -1,
            FourDirection.East => 1,
            _ => 0
        };
}