using Mm0205.ImageVectorization.Fitting;
using Mm0205.ImageVectorization.PathExtraction;
using Mm0205.ImageVectorization.PathInterpolation;

namespace Mm0205.ImageVectorization;

/// <summary>
/// 2値画像をベクトル化するクラス。
/// </summary>
public class Vectorizer
{
    private readonly PathExtractor _pathExtractor;
    private readonly PathInterpolator _pathInterpolator;
    private readonly SegmentDecomposer _segmentDecomposer;
    private readonly SegmentFitter _segmentFitter;

    public Vectorizer(VectorizationContext context)
    {
        _pathExtractor = new PathExtractor(context);
        _pathInterpolator = new PathInterpolator(context);
        _segmentDecomposer = new SegmentDecomposer(context);
        _segmentFitter = new SegmentFitter(context);
    }

    /// <summary>
    /// 二値画像をベクトル化する。
    /// </summary>
    public void Vectorize()
    {
        _pathExtractor.Extract();
        _pathInterpolator.Interpolate();
        _segmentDecomposer.Decompose();
        _segmentFitter.Fit();
    }
}