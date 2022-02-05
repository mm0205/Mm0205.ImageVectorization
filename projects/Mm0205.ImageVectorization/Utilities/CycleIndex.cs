namespace Mm0205.ImageVectorization.Utilities;

/// <summary>
/// 循環するインデックス。<br/>
/// <br/>
/// <c>0 &lt;= インデックス &lt; 要素数</c> の時は普通のインデックスと同じ動作をする。<br/>
/// <br/>
/// <c>要素数 &lt;= インデックス</c> の時は<c>インデックス mod 要素数</c>を実際のインデックスとする。<br/>
/// つまり<c>要素数 = インデックス</c>ならば実際にアクセスする要素はha<c>インデックス = 0</c>、<br/>
/// <c>要素数 + 1 = インデックス</c>ならば実際にアクセスする要素はha<c>インデックス = 1</c>。<br/>
/// <br/>
/// <c>インデックス &lt; 0</c> の時は<c>要素数 + インデックス mod 要素数</c>を実際のインデックスとする。<br/>
/// つまり<c>要素数 = -1</c>ならば実際の要素は<c>インデックス = 要素数 - 1</c>の要素、<br/>
/// <c>要素数 = -2</c>ならば実際の要素は<c>インデックス = 要素数 - 2</c>の要素、<br/>
/// となる。
/// <br/>
/// </summary>
/// <example>
/// <code>
/// var index = new CycleIndex(new [] {'a', 'b', 'c' });
///
/// index.Start(1);
/// Assert.True(index.Current() == 'b');
/// Assert.False(index.Finished());
///
/// index++;
/// Assert.True(index.Current() == 'c');
/// Assert.False(index.Finished());
/// 
/// index++;
/// Assert.True(index.Current() == 'a');
/// Assert.False(index.Finished());
///
/// index++;
/// Assert.True(index.Current() == 'b');
/// Assert.True(index.Finished());
/// 
/// </code>
/// </example>
/// <typeparam name="T">要素の型。</typeparam>
public class CycleIndex<T>
{
    private readonly T[] _source;
    private readonly int _count;

    private int Current { get; set; }

    private int IncrementCount { get; set; }

    /// <summary>
    /// コンストラクタ。
    /// </summary>
    /// <param name="source">インスタンスの操作対象とするリスト。(シャローコピーする)。</param>
    public CycleIndex(IEnumerable<T> source)
    {
        _source = source.ToArray();
        _count = _source.Length;
        Current = 0;
        IncrementCount = 0;
    }

    /// <summary>
    /// インデックス開始位置を<paramref name="i"/>とする。<br/>
    /// インクリメントを続けて、再びインデックスが<c>i</c>となると<see cref="Finished"/>が<c>true</c>になる。<br/>
    /// <br/>
    /// </summary>
    /// <param name="i">インデックス。</param>
    public void Start(int i)
    {
        Current = i;
        IncrementCount = 0;
    }

    /// <summary>
    /// <see cref="Start"/>から1周したか判定する。
    /// </summary>
    /// <returns><see cref="Start"/>から1周している場合は<c>true</c>。</returns>
    public bool Finished() => IncrementCount >= _count;

    /// <summary>
    /// インデックスをインクリメントする。要素数を超えたら0に戻る。
    /// </summary>
    /// <param name="index">インデックス。※ このインデックス自体も変更するので注意！</param>
    /// <returns>インデックス。※ コピーではなくて引数をそのまま返す！</returns>
    public static CycleIndex<T> operator ++(CycleIndex<T> index)
    {
        index.Current = (index.Current + 1) % index._count;
        index.IncrementCount++;
        return index;
    }

    /// <summary>
    /// 現在のインデックスに<paramref name="n"/>を加えた新しいインデックスインスタンスを生成する。
    /// </summary>
    /// <param name="index">元のインデックス。</param>
    /// <param name="n">インデックスに可算する量。</param>
    /// <returns>元のインデックス + n(サイクリックに)となる新しいインデックス。</returns>
    public static CycleIndex<T> operator +(
        CycleIndex<T> index,
        int n
    )
    {
        return n >= 0
            ? new CycleIndex<T>(index._source)
            {
                Current = (index.Current + n) % index._count
            }
            : index - -n;
    }

    /// <summary>
    /// 現在のインデックスから<paramref name="n"/>を減じた新しいインデックスインスタンスを生成する。
    /// </summary>
    /// <param name="index">元のインデックス。</param>
    /// <param name="n">インデックスから減じる量。</param>
    /// <returns>元のインデックス - n(サイクリックに)となる新しいインデックス。</returns>
    public static CycleIndex<T> operator -(
        CycleIndex<T> index,
        int n
    )
    {
        if (n < 0)
        {
            return index + -n;
        }

        var result = new CycleIndex<T>(index._source);
        if (index.Current >= n)
        {
            result.Current = index.Current - n;
        }
        else
        {
            result.Current = index._count - (n % index._count - index.Current);
        }

        return result;
    }

    /// <summary>
    /// 現在のインデックスが指す要素を取得する。
    /// </summary>
    /// <returns>要素。</returns>
    public T GetCurrent()
    {
        return _source[Current];
    }
}