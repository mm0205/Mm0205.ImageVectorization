using Mm0205.ImageVectorization.PathExtraction;
using Mm0205.ImageVectorization.Utilities;

namespace Mm0205.ImageVectorization.PathInterpolation;

/// <summary>
/// 4方向パスの、各エッジの中点のリストを作成する。<br/>
/// <br/>
/// つまり、<br/>
/// <code>
/// 頂点: (0,0) (0,1) (1, 1)
/// エッジ:    →     ↑
/// </code>
/// というパスに対して、以下のような補間点と方向(8方向)を作成する。<br/>
/// <code>
/// 　頂点: (  0,  0)       (  0,  1)     (  1,   1)
/// 補間点:         (  0,0.5)       (0.5,1)
/// 　方向:                     ↗
/// </code>
/// </summary>
public class PathInterpolator
{
    private readonly VectorizationContext _context;

    /// <summary>
    /// コンストラクタ。
    /// </summary>
    /// <param name="context">ベクトル化コンテキスト。</param>
    public PathInterpolator(VectorizationContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 補間点を作成する。
    /// </summary>
    public void Interpolate()
    {
        _context.InterpolatedPaths.Clear();

        // 全てのパスに対して、補間点のリストを作成する。
        foreach (var path in _context.ClosedPaths)
        {
            AddInterpolationPointsForPath(path);
        }
    }

    /// <summary>
    /// 1つのパスに対して補間点のリストを作成する。
    /// </summary>
    /// <param name="path">対象のパス。</param>
    private void AddInterpolationPointsForPath(FourDirectionPath path)
    {
        var interpolationPoints = new InterpolationPointList();
        _context.InterpolatedPaths.Add(interpolationPoints);

        var edgeIndex = new CycleIndex<FourDirectionEdge>(path.Edges);

        // 一つ前のエッジの補間点。
        var prevPoint = ComputeInterpolationPointFromEdge((edgeIndex - 1).GetCurrent());

        for (edgeIndex.Start(0); !edgeIndex.Finished(); edgeIndex++)
        {
            // 現在のエッジの補間点。
            var p1 = ComputeInterpolationPointFromEdge(edgeIndex.GetCurrent());

            var pointWithDirection = new PointWithDirection
            {
                Point = prevPoint,
                Direction = ComputeDirection(prevPoint, p1)
            };
            interpolationPoints.Add(pointWithDirection);
            prevPoint = p1;
        }
    }

    /// <summary>
    /// エッジの補間点を作成する。
    /// </summary>
    /// <param name="edge">エッジ。</param>
    /// <returns>補間点。</returns>
    private static Point2D ComputeInterpolationPointFromEdge(FourDirectionEdge edge) =>
        ComputeInterpolationPoint(edge.From, edge.To);

    /// <summary>
    /// 2点の補間点を作成する。(補間点は中点)。
    /// </summary>
    /// <param name="p0">対象の点1。</param>
    /// <param name="p1">対象の点2。</param>
    /// <returns>補間点。</returns>
    private static Point2D ComputeInterpolationPoint(
        LatticePoint p0,
        LatticePoint p1
    ) =>
        new()
        {
            X = (p0.X + p1.X) / 2.0,
            Y = (p0.Y + p1.Y) / 2.0
        };

    /// <summary>
    /// 補間点の方向を計算する。<br/>
    /// 補間点の方向は、ある補間点と次の補間点を結ぶ線分の方向から8方向の何れかを決定する。
    /// </summary>
    /// <param name="p">対象の補間点。</param>
    /// <param name="next">次の補間点。</param>
    /// <returns>補間点<paramref name="p"/>の方向。</returns>
    private static EightDirection ComputeDirection(
        Point2D p,
        Point2D next
    )
    {
        // double型の等価判定に使う。
        // 実際に比較する点は格子点間の中点なので0.5単位で一致すれば等しいと見なせる。
        const double diffMax = 0.0001;
        
        if (Math.Abs(p.X - next.X) <= diffMax)
        {
            return p.Y < next.Y
                ? EightDirection.North
                : EightDirection.South;
        }

        if (p.X < next.X)
        {
            if (Math.Abs(p.Y - next.Y) <= diffMax)
            {
                return EightDirection.East;
            }

            return p.Y < next.Y ? EightDirection.NorthEast : EightDirection.SouthEast;
        }

        if (Math.Abs(p.Y - next.Y) <= diffMax)
        {
            return EightDirection.West;
        }

        return p.Y < next.Y ? EightDirection.NorthWest : EightDirection.SouthWest;
    }
}