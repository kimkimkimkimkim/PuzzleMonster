/// <summary>
/// マスタのコンテンツをファイルから読書するために使用するデータ構造
/// </summary>
/// <typeparam name="M">マスタブックのデータ型</typeparam>
public class MasterContentContainer<M> where M : MasterBookBase {
    /// <summary>
    /// マスタブックリスト
    /// </summary>
    public M[] BookList { get; set; }

    /// <summary>
    /// マスタ名
    /// </summary>
    public string Name { get; set; }
}