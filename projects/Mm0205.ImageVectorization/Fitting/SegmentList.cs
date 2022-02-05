using Mm0205.ImageVectorization.PathInterpolation;

namespace Mm0205.ImageVectorization.Fitting;

/// <summary>
/// セグメントのリスト。
/// </summary>
public class SegmentList
{
    /// <summary>
    /// セグメントのリスト。
    /// </summary>
    public IEnumerable<PathSegment> Segments => _segments;

    private readonly List<PathSegment> _segments = new();

    /// <summary>
    /// セグメントを追加する。
    /// </summary>
    /// <param name="segment">対象のセグメント。</param>
    public void Add(PathSegment segment) => _segments.Add(segment);

    public void Reduce()
    {
        // 末尾のセグメントと先頭のセグメントはつなげられるかもしれないのでつなげて見る。
        // var first = Segments.First();
        // var last = Segments.Last();
        // last.Concat(first);

        // var found = false;
        var reduced = _segments.Where(x => x.Points.Any()).ToList();
        // do
        // {
        //     found = false;
        //     for (var i = 0; i < reduced.Count - 1; i++)
        //     {
        //         // 2点しかないセグメントは前後のセグメントに拡張してしまう。
        //         if (reduced[i].Points.Count > 2)
        //         {
        //             continue;
        //         }
        //
        //         var next = (i + 1) % reduced.Count;
        //         reduced[next].Points.Insert(0, reduced[i].Points.Last());
        //         
        //         var prev = i - 1 >= 0 ? i - 1 : reduced.Count - 1;
        //         reduced[prev].Points.Add( reduced[i].Points.First());
        //         reduced[i].Points.Clear();
        //         
        //         reduced = reduced.Where(x => x.Points.Any()).ToList();
        //         found = true;
        //         break;
        //     }
        // } while (found);

        _segments.Clear();
        _segments.AddRange(reduced);
    }
}