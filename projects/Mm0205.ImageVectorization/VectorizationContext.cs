using Mm0205.ImageVectorization.Fitting;
using Mm0205.ImageVectorization.PathExtraction;
using Mm0205.ImageVectorization.PathInterpolation;

namespace Mm0205.ImageVectorization;

/// <summary>
/// 内部のアルゴリズム処理状態。
/// </summary>
public class VectorizationContext
{
    /// <summary>
    /// ベクトル化オプション。
    /// </summary>
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public VectorizationOptions Options { get; init; } = new();
    
    /// <summary>
    /// 元の画像。
    /// </summary>
    public byte[,] SourceImage { get; }

    /// <summary>
    /// 二値画像からパス分割した結果。
    /// </summary>
    public IList<FourDirectionPath> ClosedPaths { get; } = new List<FourDirectionPath>();

    /// <summary>
    /// 補間処理後のパス。
    /// </summary>
    public IList<InterpolationPointList> InterpolatedPaths { get; } = new List<InterpolationPointList>();

    /// <summary>
    /// 補間点の列をセグメントに分割した結果。
    /// </summary>
    public IList<SegmentList> PathSegments { get; } = new List<SegmentList>();

    /// <summary>
    /// 各セグメントを直線と2次ベジェ曲線んで近似した結果。
    /// </summary>
    public IList<VectorPath> ApproximatePaths { get; } = new List<VectorPath>();

    public VectorizationContext(byte[,] sourceImage)
    {
        SourceImage = sourceImage;
    }
}