using Mm0205.ImageVectorization.PathInterpolation;

namespace Mm0205.ImageVectorization.VectorOperations;

public class LineTo : IPathOperation
{
    public Point2D From { get; }
    public Point2D To { get; }

    public LineTo(
        Point2D @from,
        Point2D to
    )
    {
        From = @from;
        To = to;
    }
}