namespace Mm0205.ImageVectorization.PathExtraction;

/// <summary>
/// 4方向エッジで構築されたパス。
/// </summary>
public class FourDirectionPath
{
    /// <summary>
    /// 4方向エッジのリスト。
    /// </summary>
    public IEnumerable<FourDirectionEdge> Edges => _edges;

    private readonly List<FourDirectionEdge> _edges = new();

    /// <summary>
    /// エッジを追加する。
    /// </summary>
    /// <param name="edge">エッジ。</param>
    public void AddEdge(FourDirectionEdge edge) => _edges.Add(edge);
}