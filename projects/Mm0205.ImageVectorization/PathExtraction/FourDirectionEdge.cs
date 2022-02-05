namespace Mm0205.ImageVectorization.PathExtraction;

/// <summary>
/// 内部データ(エッジ)。
/// <br/>
/// エッジの長さは常に1。<br/>
/// 方向は上下左右。
/// </summary>
public readonly struct FourDirectionEdge
{
    /// <summary>
    /// エッジの始点。
    /// </summary>
    public LatticePoint From { get; }

    /// <summary>
    /// エッジの端点。
    /// </summary>
    public LatticePoint To { get; }

    /// <summary>
    /// エッジの方向。
    /// </summary>
    public FourDirection Direction { get; }

    public FourDirectionEdge(
        in LatticePoint from,
        in LatticePoint to,
        in FourDirection direction
    )
    {
        From = from;
        To = to;
        Direction = direction;
    }

    #region override object

    public override int GetHashCode()
    {
        return HashCode.Combine(From.GetHashCode(), To.GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        if (obj is FourDirectionEdge edge)
        {
            return Equals(edge);
        }

        return false;
    }

    public override string ToString() => $"{From} => {To} [{Direction}]";

    #endregion
    
    private bool Equals(FourDirectionEdge other)
        => From == other.From && To == other.To && Direction == other.Direction;

    public static bool operator ==(
        FourDirectionEdge lhs,
        FourDirectionEdge rhs
    ) => lhs.Equals(rhs);

    public static bool operator !=(
        FourDirectionEdge lhs,
        FourDirectionEdge rhs
    ) =>
        !(lhs == rhs);
}