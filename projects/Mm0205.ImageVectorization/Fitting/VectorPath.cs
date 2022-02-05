using Mm0205.ImageVectorization.VectorOperations;

namespace Mm0205.ImageVectorization.Fitting;

/// <summary>
/// ベクター表現のパス。
/// </summary>
public class VectorPath
{
    /// <summary>
    /// パスオペレーションのリスト。
    /// </summary>
    public IEnumerable<IPathOperation> Operations => _operations;
    
    private readonly List<IPathOperation> _operations = new ();

    public void Add(IPathOperation operation) => _operations.Add(operation);
}