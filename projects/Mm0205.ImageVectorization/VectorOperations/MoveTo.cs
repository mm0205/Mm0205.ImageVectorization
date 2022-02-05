namespace Mm0205.ImageVectorization.VectorOperations;

/// <summary>
/// Starts new sub path.
/// </summary>
public class MoveTo : IPathOperation
{
    /// <summary>
    /// X.
    /// </summary>
    public double X { get; init; }
    
    /// <summary>
    /// Y.
    /// </summary>
    public double Y { get; init; }
    
    /// <summary>
    /// If <c>true</c>, <see cref="X"/> and <see cref="Y"/> mean the coordinates relative to the current position.
    /// <br/>
    /// If <c>false</c>, <see cref="X"/> and <see cref="Y"/> mean the abosolute position.
    /// </summary>
    public bool IsRelative { get; init; } 
}