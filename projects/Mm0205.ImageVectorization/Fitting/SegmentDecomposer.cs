using Mm0205.ImageVectorization.PathInterpolation;
using Mm0205.ImageVectorization.Utilities;

namespace Mm0205.ImageVectorization.Fitting;

/// <summary>
/// 点の列をセグメントに分割する。<br/>
/// セグメントとは、後の工程で補間対象とする連続する点のこと。<br/>
/// 今回は、8方向の内、2方向だけ含む連続した点を一つのセグメントとして区切る。<br/>
/// 例えば与えられた点の方向が以下のようになっていた場合、<br/>
/// <code>
/// ↓↙↙↓↙↙←↙↙↓↘↘↓↓↘
/// </code>
/// 以下のように5つのセグメントに分割する。 (各セグメントはそれぞれ2方向のみを含む)<br/>。
/// セグメント1: ↓↙↙↓↙↙<br/>
/// セグメント2: ↙←↙↙<br/>
/// セグメント3: ↙←↙↙<br/>
/// セグメント4: ↙↓↘↘<br/>
/// セグメント5: ↘↓↓↘<br/>
/// </summary>
public class SegmentDecomposer
{
    private readonly VectorizationContext _context;

    public SegmentDecomposer(VectorizationContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 補間点の列をセグメントに分割する。
    /// </summary>
    public void Decompose()
    {
        _context.PathSegments.Clear();

        foreach (var path in _context.InterpolatedPaths)
        {
            DecomposePathToSegments(path);
        }
    }

    /// <summary>
    /// 1パス分の補間点リストをセグメントに分割する。
    /// </summary>
    /// <param name="path">1パス分の補間点リスト。</param>
    private void DecomposePathToSegments(InterpolationPointList path)
    {
        var segmentList = new SegmentList();
        _context.PathSegments.Add(segmentList);
        
        var segment = new PathSegment();
        segmentList.Add(segment);

        var index = new CycleIndex<PointWithDirection>(path.Points);
        var firstPoint = index.GetCurrent();
        var segmentLastPoint = firstPoint;

        for (index.Start(0); !(index.Finished() && firstPoint.Point.Equals(segmentLastPoint.Point)); index++)
        {
            var point = index.GetCurrent();
            segmentLastPoint = point;
            
            if (segment.CanAdd(point))
            {
                segment.Add(point);
                continue;
            }

            // 追加できない点がある場合は、現在のセグメントを
            segment = segment.CreateNext(point);
            segmentList.Add(segment);
        }

        // if (segmentList.Segments.Count() < 2)
        // {
        //     return;
        // }
        // 最後のセグメントと最初のセグメントは結合できるかもしれないのでやってみる。
        // segmentList.Reduce();
    }
}