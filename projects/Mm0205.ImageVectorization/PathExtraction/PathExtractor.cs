using System.Diagnostics.CodeAnalysis;

namespace Mm0205.ImageVectorization.PathExtraction;

/// <summary>
/// 二値画像からクローズドパスを抽出するクラス。
/// </summary>
public class PathExtractor
{
    /// <summary>
    /// 対象の画像。<br/>
    /// 本アルゴリズムは処理中に画像を改変するので、元データはコピーして使う。
    /// </summary>
    private readonly byte[,] _targetImage;

    private readonly VectorizationContext _context;

    private int YMax => _targetImage.GetLength(0);
    private int XMax => _targetImage.GetLength(1);

    /// <summary>
    /// コンストラクタ。
    /// </summary>
    /// <param name="context">ベクトル化コンテキスト。</param>
    public PathExtractor(
        VectorizationContext context
    )
    {
        _targetImage = new byte[context.SourceImage.GetLength(0), context.SourceImage.GetLength(1)];
        Array.Copy(context.SourceImage, _targetImage, context.SourceImage.Length);
        _context = context;
    }

    /// <summary>
    /// 二値画像からパスを抽出する。
    /// </summary>
    public void Extract()
    {
        _context.ClosedPaths.Clear();

        while (TryGetTopLeftFilledPixel(out var point))
        {
            var path = ExtractPath(point.Value);
            _context.ClosedPaths.Add(path);
            RemovePathFromImage(path);
        }
    }

    /// <summary>
    /// 抽出したパスを対象画像から削除する。<br/>
    /// 削除したパスで囲まれた閉領域のピクセルは反転する。<br/>
    /// ※ 反転するのは内側のパスも抽出するため！<br/>
    /// ※ 例えば□みたいな画像がある場合、抽出するパスは、辺の外側をなぞるパスと、内側をなぞるパスの2つになる。<br/>
    /// ピクセルを反転すると同じパス抽出を繰り返し適用することで、外側のパスも内側のパスも同じように抽出できる。
    /// </summary>
    /// <param name="path">削除するクローズドパス。</param>
    /// <example>
    /// 例：<br/>
    /// 例えば以下のような3x3ビットマップ(×がfill)がある場合、<br/>
    /// <code>
    /// 　◯×◯
    /// 　×◯×
    /// 　◯×◯
    /// </code><br/>
    /// パスは、以下のようになる。(斜めの矢印↗等は実際には「→」と「↑」のような2つのパスだけど、文字で表すのが難しいので1つにまとめている。)<br/>
    /// <code>
    /// 　　←
    /// 　↙×↖
    /// ↓×◯×↑
    /// 　↘×↗
    /// 　　→
    /// </code>
    /// このパスを削除すると、内部のピクセルを反転して次のようになる。<br/>
    /// <code>
    /// 　◯
    /// ◯×◯
    /// 　◯
    /// </code>
    /// </example>
    private void RemovePathFromImage(in FourDirectionPath path)
    {
        // Y座標が同じパスの内上向きパス(↑)と下向きパス(↓)を抽出する。
        var listOfSameYEdges = path.Edges
            .Where(x => x.Direction is FourDirection.South or FourDirection.North)
            .Select(x => new
            {
                Edge = x,
                Pixel = LeftPixelOf(x)
            })
            .OrderBy(x => x.Pixel.Y)
            .ThenBy(x => x.Pixel.X)
            .GroupBy(x => x.Pixel.Y)
            .ToArray();

        // 同じY座標の上下のパスに囲まれた範囲のピクセルを反転する。

        // 例えばパスが1ピクセルを囲んでいる場合は、
        // ↓×↑ となるので反転すると ◯ になる。

        // 内部に複数ピクセル存在する場合は、
        // ↓×◯◯×↑ みたいな感じなので、 ◯××◯ になる。

        // 1行に複数のパスが存在する場合は、以下のような感じになる。
        // ↓×◯×↑◯◯◯↓×↑ (※ パスの抽出方法的に必ずY座標が同じパスは上下セットなので、必ずパス数は2の倍数になる！)
        // 　◯×◯　◯◯◯　◯ ※ (内側の◯◯◯はパスに囲まれていないので反転しない！)
        foreach (var sameYEdgesGroup in listOfSameYEdges)
        {
            var y = sameYEdgesGroup.Key;
            var xs = sameYEdgesGroup.Select(x => x.Pixel.X).ToArray();

            for (var i = 0; i < xs.Length; i += 2)
            {
                var x1 = xs[i];
                var x2 = xs[i + 1];
                for (var x = x1; x <= x2; x++)
                {
                    _targetImage[y, x] = _targetImage[y, x] == 0 ? (byte)1 : (byte)0;
                }
            }
        }
    }

    /// <summary>
    /// 座標(<paramref name="point"/>)がビットマップ内部か判定する。
    /// </summary>
    /// <param name="point">座標。</param>
    /// <returns>座標(<paramref name="point"/>)がビットマップ内部の場合
    /// <c>true</c>。
    /// </returns>
    private bool PointIsOutOfImage(in LatticePoint point) =>
        point.X < 0 || point.X >= XMax
                    || point.Y < 0 || point.Y >= YMax;

    /// <summary>
    /// 指定された座標(<paramref name="point"/>)を始点、終点とするクローズドパスを抽出する。<br/>
    /// <br/>
    /// ※ 以下のルールに従ってパスをたどると、必ずクローズドパスが抽出できる！<br/>
    /// 1．常にエッジの左側がfillピクセル、右側が空ピクセルとする。 <br/>
    /// 2．画像の範囲外のピクセルは常に空ピクセルとする。<br/>
    /// </summary>
    /// <param name="point">パスの始点。 <br/>
    /// ※ ここに指定された座標に対応するピクセル X ∈ [point.X, point.X + 1], Y ∈ [point.Y, point.Y + 1] はfillされていること！</param>
    /// <returns>
    /// 指定された座標(<paramref name="point"/>)を始点、終点とするクローズドパス。
    /// </returns>
    private FourDirectionPath ExtractPath(in LatticePoint point)
    {
        var result = new FourDirectionPath();

        var startPoint = point;
        var currentPoint = point;
        var currentDirection = FourDirection.South;

        do
        {
            var (nextPoint, nextDirection) = ComputeNextPoint(currentPoint, currentDirection);
            result.AddEdge(
                new FourDirectionEdge(
                    currentPoint,
                    nextPoint,
                    nextDirection
                )
            );

            currentDirection = nextDirection;
            currentPoint = nextPoint;
        } while (!(startPoint == currentPoint && currentDirection is FourDirection.West));

        return result;
    }

    /// <summary>
    /// パスをたどる際に次に進むピクセルを計算する。<br/><br/>
    /// 例：現在位置を以下の4つのピクセルに囲まれた格子点とし進行方向を↓方向とする。<br/>
    /// ※ △は何でも良くて、進行方向左右のピクセルだけを見て、次に進むピクセルを決める。<br/>
    /// ※ パスの始点を、「Y座標がもっとも小さいfillピクセルの内、X座標がもっとも小さいピクセル」としているのでこのロジックでたどれば十分。<br/>
    /// <code>
    /// △△
    /// ◯×
    /// </code>
    /// この場合次のピクセルは↓(Y座標 + 1)となる。<br/>同様に全てのピクセルのパターンで、<br/>
    /// <code>
    /// △△　△△　△△　△△　△△
    /// ◯×　◯◯　××　◯×　×◯
    /// </code>
    /// それぞれ次のピクセルは、<br/>
    /// ↓(Y + 1)、→(X+1)、←(X-1)、←(X-1) <br/>
    /// ※ 一番右端のパターンは、→方向に進んでもロジックは成り立つが、なるべく一つのパスを長くするために左に進むことにする。<br/>
    /// <br/>
    /// 同様に↓以外の各進行方向に関しては同じロジックを回転させたものを用いる。
    /// </summary>
    /// <param name="currentPoint">現在位置。</param>
    /// <param name="currentDirection">現在の進行方向。</param>
    /// <returns>次のピクセルの座標と進行方向。</returns>
    private (LatticePoint, FourDirection) ComputeNextPoint(
        in LatticePoint currentPoint,
        in FourDirection currentDirection
    )
    {
        var leftPixelIsFilled = IsFilled(LeftPixelOf(currentPoint, currentDirection));
        var rightPixelIsFilled = IsFilled(RightPixelOf(currentPoint, currentDirection));

        var nextDirection = (leftPixelIsFilled, rightPixelIsFilled) switch
        {
            (true, false) => currentDirection,
            (true, true) => currentDirection.TurnRight(),
            (false, true) => currentDirection.TurnRight(),
            _ => currentDirection.TurnLeft()
        };

        return (currentPoint.NextPoint(nextDirection), nextDirection);
    }

    /// <summary>
    /// 指定された座標(<paramref name="point"/>)に対応するピクセルがfillか判定する。
    /// </summary>
    /// <param name="point">座標。</param>
    /// <returns>指定された座標(<paramref name="point"/>)に対応するピクセルがfillの場合
    /// <c>true</c>。<br/>
    /// ※ 座標が画像範囲外の場合は空ピクセル(fillされていない)とみなす。
    /// </returns>
    private bool IsFilled(in LatticePoint point)
    {
        // 対象画像の外のピクセルは白(fillなしと看做す)。
        if (PointIsOutOfImage(point))
        {
            return false;
        }

        return _targetImage[point.Y, point.X] != 0;
    }

    /// <summary>
    /// 進行方向右側のピクセルを取得する。
    /// </summary>
    /// <param name="point">現在位置。</param>
    /// <param name="direction">進行方向。</param>
    /// <returns>進行方向右側のピクセル。</returns>
    /// <exception cref="ArgumentOutOfRangeException">不正な方向。</exception>
    private static LatticePoint RightPixelOf(
        in LatticePoint point,
        in FourDirection direction
    ) =>
        direction switch
        {
            FourDirection.South => new LatticePoint(point.X - 1, point.Y),
            FourDirection.East => point,
            FourDirection.North => new LatticePoint(point.X, point.Y - 1),
            FourDirection.West => new LatticePoint(point.X - 1, point.Y - 1),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "不正な方向")
        };

    /// <summary>
    /// エッジ(<paramref name="edge"/>)の左側に位置するピクセルを取得する。
    /// </summary>
    /// <param name="edge">対象のエッジ。</param>
    /// <returns>エッジ(<paramref name="edge"/>)の左側に位置するピクセル。</returns>
    /// <exception cref="ArgumentOutOfRangeException">エッジの方向が不正。</exception>
    private static LatticePoint LeftPixelOf(in FourDirectionEdge edge) =>
        edge.Direction switch
        {
            FourDirection.South => edge.From,
            FourDirection.East => new LatticePoint(edge.From.X, edge.From.Y - 1),
            FourDirection.North => new LatticePoint(edge.From.X - 1, edge.From.Y - 1),
            FourDirection.West => new LatticePoint(edge.To.X, edge.To.Y),
            _ => throw new ArgumentOutOfRangeException(nameof(edge), edge.Direction, "エッジの方向が不正")
        };

    
    /// <summary>
    /// 進行方向左側のピクセルを取得する。
    /// </summary>
    /// <param name="point">現在位置。</param>
    /// <param name="direction">進行方向。</param>
    /// <returns>進行方向右側のピクセル。</returns>
    /// <exception cref="ArgumentOutOfRangeException">不正な方向。</exception>
    private static LatticePoint LeftPixelOf(
        in LatticePoint point,
        in FourDirection direction
    ) =>
        direction switch
        {
            FourDirection.South => new LatticePoint(point.X, point.Y),
            FourDirection.East => new LatticePoint(point.X, point.Y - 1),
            FourDirection.North => new LatticePoint(point.X - 1, point.Y - 1),
            FourDirection.West => new LatticePoint(point.X - 1, point.Y),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "不正な方向")
        };

    /// <summary>
    /// 対象画像の一番上の画像から1行ずつ走査し、一番上の行かつ、一番左の列にある色ありピクセルを見つける。
    /// </summary>
    /// <param name="point">一番上左の色ありピクセル座標。</param>
    /// <returns>色ありピクセルがある場合
    /// <c>true</c>。
    /// </returns>
    private bool TryGetTopLeftFilledPixel(
        [NotNullWhen(true)] out LatticePoint? point
    )
    {
        for (var y = 0; y < YMax; y++)
        {
            for (var x = 0; x < XMax; x++)
            {
                if (_targetImage[y, x] == 0)
                {
                    continue;
                }

                point = new LatticePoint(x, y);
                return true;
            }
        }

        point = null;
        return false;
    }
}