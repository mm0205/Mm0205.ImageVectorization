namespace Mm0205.ImageVectorization;

/// <summary>
/// ベクトル化のオプション。
/// </summary>
public class VectorizationOptions
{
    /// <summary>
    /// 直線近似の閾値。<br/>
    /// この値よりも「近似直線と補間点の距離の二乗」が小さい場合は直線近似可能と判定する。
    /// </summary>
    public double LineFitThreshold { get; init; } = 0.1;
    
    /// <summary>
    /// 2次ベジェ曲線近似の閾値。<br/>
    /// この値よりも「近似曲線と補間点の距離の二乗」が小さい場合は2次ベジェ曲線近似可能と判定する。
    /// </summary>
    public double CurveFitThreshold { get; init; } = 1.1;
    
    /// <summary>
    /// 直線近似を行なわない場合は<c>true</c>にする。(直線近似を行わない場合、2次ベジェのみで近似する)。
    /// </summary>
    public bool DisableLineFit { get; init; }
}