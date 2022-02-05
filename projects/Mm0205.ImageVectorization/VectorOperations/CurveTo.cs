using Mm0205.ImageVectorization.PathInterpolation;

namespace Mm0205.ImageVectorization.VectorOperations;

public class CurveTo : IPathOperation
{
    public Point2D P0 { get; }
    public Point2D P1 { get; }
    public Point2D P2 { get; }

    public CurveTo(
        Point2D p0,
        Point2D p1,
        Point2D p2
    )
    {
        P0 = p0;
        P1 = p1;
        P2 = p2;
    }
}