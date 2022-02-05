using System.Diagnostics.CodeAnalysis;
using Mm0205.ImageVectorization.PathInterpolation;
using Mm0205.ImageVectorization.VectorOperations;

namespace Mm0205.ImageVectorization.Fitting;

/// <summary>
/// セグメントを直線と2次ベジェ曲線で近似する。
/// </summary>
public class SegmentFitter
{
    private readonly VectorizationContext _context;

    public SegmentFitter(VectorizationContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 全てのセグメントを直線と二次ベジェ曲線で近似する。
    /// </summary>
    public void Fit()
    {
        _context.ApproximatePaths.Clear();

        foreach (var segmentList in _context.PathSegments)
        {
            var vectorPath = new VectorPath();
            _context.ApproximatePaths.Add(vectorPath);
            
            foreach (var segment in segmentList.Segments)
            {
                FitSegment(segment);
            }
        }
    }

    /// <summary>
    /// 一つのセグメントを直線と二次ベジェ曲線で近似する。
    /// </summary>
    /// <param name="segment"></param>
    private void FitSegment(
        PathSegment segment
    )
    {
        if (TryFitLine(segment, out var farthestIndex))
        {
            return;
        }

        if (TryFileCurve(segment, farthestIndex.Value))
        {
            return;
        }

        if (farthestIndex + 1 == segment.Points.Count)
        {
            farthestIndex--;
        }
        else if (farthestIndex == 0)
        {
            farthestIndex++;
        }

        var (firstSegment, lastSegment) = segment.Split(farthestIndex);

        FitSegment(firstSegment);
        // ReSharper disable once TailRecursiveCall
        FitSegment(lastSegment);
    }

    private bool TryFileCurve(
        PathSegment segment,
        int farthestIndex
    )
    {
        var p0 = segment.Points.First().Point;
        var p2 = segment.Points.Last().Point;

        // 二次ベジエの制御点 (P1)を見つける。
        // 1．セグメントの始点と終点を結ぶ直線から最も直線から離れた点を目印にする
        var px = segment.Points[farthestIndex].Point;

        // 座標がPXとなるときのベジェの媒介変数Txを求める。 (均等分割とする)。
        var tx = (double)farthestIndex / segment.Points.Count;

        // 媒介変数とp0, p2が分かったので、制御点P1の座標を計算する。
        // ※ 単純にベジェ曲線の媒介変数表示をP1について解いただけ。
        // px = (1 - t)^2 * p1 + 2 * (1-t) * t * p1 + t^2 * p2 なので、
        // p1 = ((1 - t)^2 * p1 + t^2 * p2 - px) / (- 2 * (1-t) * t)

        // 長いので各係数は以下の名称で扱う。
        // c0 = (1 - t)^2
        // c1 = 2 * (1-t) * t
        // c2 = t^2

        var (cx0, cx1, cx2) = ComputeBezierCoefficients(tx);
        var p1 = (p0 * cx0 + p2 * cx2 - px) / -cx1;

        if (segment.Points.Count <= 2)
        {
            _context.ApproximatePaths.Last().Add(new CurveTo(p0, p1, p2));
            return true;
        }

        for (var i = 1; i < segment.Points.Count; i++)
        {
            // 実際の点
            var p = segment.Points[i].Point;

            // ベジェ近似した点。
            var t = (double)i / segment.Points.Count;
            var approximatePoint = ComputeBezier(p0, p1, p2, t);

            // ベジェ近似した点と実際の点の間の距離の許容誤差が一定以上の場合は近似不可とする。
            var distance2 = p.ComputeDistance2To(approximatePoint);
            if (distance2 > _context.Options.CurveFitThreshold)
            {
                return false;
            }
        }

        _context.ApproximatePaths.Last().Add(new CurveTo(p0, p1, p2));
        return true;
    }

    /// <summary>
    /// 指定された点3つと媒介変数tからベジェ曲線上の点を計算する。
    /// </summary>
    /// <param name="p0">始点。</param>
    /// <param name="p1">制御点。</param>
    /// <param name="p2">終点。</param>
    /// <param name="t">媒介変数t。</param>
    /// <returns>媒介変数がtであるときのベジェ曲線上の点。</returns>
    private static Point2D ComputeBezier(
        Point2D p0,
        Point2D p1,
        Point2D p2,
        double t
    )
    {
        var (c0, c1, c2) = ComputeBezierCoefficients(t);
        return p0 * c0 + p1 * c1 + p2 * c2;
    }

    /// <summary>
    /// 2次ベジェの係数を計算する。
    /// </summary>
    /// <param name="t">媒介変数t。</param>
    /// <returns>各点P0〜P2に乗じる係数。</returns>
    private static (double, double, double) ComputeBezierCoefficients(
        double t
    )
    {
        var oneMinusT = 1 - t;
        var coefficient0 = Math.Pow(oneMinusT, 2);
        var coefficient1 = 2 * oneMinusT * t;
        var coefficient2 = Math.Pow(t, 2);
        return (coefficient0, coefficient1, coefficient2);
    }

    /// <summary>
    /// 直線近似する。
    /// </summary>
    /// <param name="segment">セグメント。</param>
    /// <param name="farthestIndex">直線近似した結果、直線から最も遠い補間点を示すインデックス。</param>
    /// <returns>近似できた場合は<c>true</c>。</returns>
    private bool TryFitLine(
        PathSegment segment,
        [NotNullWhen(false)] out int? farthestIndex
    )
    {
        var first = segment.Points.First();
        var last = segment.Points.Last();
        var pointCount = segment.Points.Count;
        var diffX = (last.Point - first.Point) / pointCount;

        farthestIndex = 1;
        var farthestDistance2 = 0.0;

        for (var i = 1; i < segment.Points.Count; i++)
        {
            var currentPoint = segment.Points[i].Point;
            var pointOnLine = first.Point + diffX * i;
            var distance2 = currentPoint.ComputeDistance2To(pointOnLine);
            if (distance2 < farthestDistance2)
            {
                continue;
            }

            farthestDistance2 = distance2;
            farthestIndex = i;
        }

        if (farthestDistance2 >= _context.Options.LineFitThreshold)
        {
            return false;
        }

        // 直線近似無しの設定の場合は、直線近似可能な場合でもカーブフィットする。
        if (_context.Options.DisableLineFit)
        {
            return false;
        }

        _context.ApproximatePaths.Last().Add(
            new LineTo(first.Point, last.Point)
        );
        return true;
    }
}