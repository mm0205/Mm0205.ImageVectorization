namespace Mm0205.ImageVectorization.PathInterpolation;

/// <summary>
/// パスの補間点のリスト。<br/>
/// このクラス1インスタンスが1パスに相当する。
/// </summary>
public class InterpolationPointList
{
    /// <summary>
    /// パスの補間点の一覧。 
    /// </summary>
    public IEnumerable<PointWithDirection> Points => _points;
    
    private readonly List<PointWithDirection> _points  = new();

    /// <summary>
    /// 補間点を追加する。
    /// </summary>
    /// <param name="point">補間点。</param>
    public void Add(PointWithDirection point)
    {
        _points.Add(point);
    }
}