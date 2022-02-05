namespace Mm0205.ImageVectorization.PathInterpolation;

/// <summary>
/// パスのセグメント。
/// <br/>
/// パスの内、直線と見なせる部分を示す。
/// </summary>
public class PathSegment
{
    public EightDirection? FirstDirection { get; private set; }

    public EightDirection? SecondDirection { get; private set; }

    public IList<PointWithDirection> Points { get; init; } = new List<PointWithDirection>();

    /// <summary>
    /// セグメントに点(<paramref name="point"/>)を追加できるか判定する。<br/>
    /// 以下の場合に追加可能と判定する。<br/>
    /// 1．セグメントに既に含まれる点と方向が同じ場合<br/>
    /// 2．セグメントに含まれる点の方向が1方向の場合<br/>
    /// 3．セグメントに点が一つも含まれていない場合<br/>
    /// </summary>
    /// <param name="point">判定対象の点。</param>
    /// <returns>追加可能な場合
    /// <c>true</c>。
    /// </returns>
    public bool CanAdd(PointWithDirection point) =>
        !FirstDirection.HasValue ||
        FirstDirection.Value == point.Direction ||
        !SecondDirection.HasValue ||
        SecondDirection.Value == point.Direction;

    /// <summary>
    /// セグメントに点を追加する。
    /// </summary>
    /// <param name="point">点。</param>
    public void Add(PointWithDirection point)
    {
        if (!FirstDirection.HasValue)
        {
            FirstDirection = point.Direction;
        }
        else if (!SecondDirection.HasValue && FirstDirection.Value != point.Direction)
        {
            SecondDirection = point.Direction;
        }

        Points.Add(point);
    }

    /// <summary>
    /// 次のセグメントを作成する。
    /// </summary>
    /// <param name="nextPoint">次の点。</param>
    /// <returns>次のセグメント。</returns>
    public PathSegment CreateNext(PointWithDirection nextPoint)
    {
        var lastPoint = Points.Last();
        var nextSegment = new PathSegment();
        nextSegment.Add(lastPoint);
        nextSegment.Add(nextPoint);
        return nextSegment;
    }

    /// <summary>
    /// 2つのセグメントを結合する。<br/>
    /// 現在のセグメントの末尾から点を走査し、次のセグメントに追加できる点がある場合、
    /// その点を次のセグメントに移動する
    /// </summary>
    /// <param name="next"></param>
    public void Concat(PathSegment next)
    {
        for (var i = Points.Count - 2; i >= 0; i--)
        {
            var p = Points[i];

            if (next.CanAdd(p))
            {
                next.Points.Insert(0, p);
                Points.RemoveAt(i + 1);
                continue;
            }

            break;
        }

        if (Points.Count == 1)
        {
            next.Points.Insert(0, Points[0]);
            Points.RemoveAt(0);
        }
        
    }

    /// <summary>
    /// セグメントを指定されたインデックス(<paramref name="atIndex"/>)の点で分割する。
    /// </summary>
    /// <param name="atIndex">分割対象とする点のインデックス。</param>
    /// <returns>分割したセグメント(2つ)。</returns>
    public (PathSegment, PathSegment) Split(int? atIndex)
    {
        var left = new PathSegment();
        var right = new PathSegment();
        var index = 0;
        for (index = 0; index < atIndex; index++)
        {
            left.Add(Points[index]);
        }

        // 対象のインデックス(atIndex)位置の点は分割後のどちらのセグメントにも含むようにする。
        left.Add(Points[index]);
        for (; index < Points.Count; index++)
        {
            right.Add(Points[index]);
        }

        return (left, right);
    }
}