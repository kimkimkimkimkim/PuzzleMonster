using System.Collections.Generic;
using System.Reflection;

namespace GameBase
{
    /// <summary>
    /// データ加工Util
    /// </summary>
    public static class DataProcessUtil
    {
        /// <summary>
        /// 全プロパティを取得しリクエストに成形して返す
        /// </summary>
        public static Dictionary<string, object> GetRequest<T>(T data)
        {
            // 条件を設定しながらクラス内のプロパティを取得
            var members = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

            // ディクショナリに追加
            var request = new Dictionary<string, object>();
            foreach (MemberInfo m in members)
            {
                if (m.MemberType == MemberTypes.Field)
                {
                    var field = typeof(T).GetField(m.Name);
                    var value = field.GetValue(data);

                    // 変数名がそのままKeyになる
                    request.Add(m.Name, value);
                }
            }
            return request;
        }

        /// <summary>
        /// サーバー側から渡ってきたレスポンスをデシリアライズして返す
        /// </summary>
        public static T GetResponse<T>(string json)
        {
            return Utf8Json.JsonSerializer.Deserialize<T>(json);
        }
    }
}
