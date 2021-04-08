using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

public static class TextUtil
{
    /// <summary>
    /// プロパティ属性値のDescriptionを取り出します。
    /// 設定されていない場合はToString()の値を返します。
    /// </summary>
    public static string GetDescriptionAttribute(object value)
    {
        FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
        DescriptionAttribute[] descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        string descriptionString = descriptionAttributes.Select(n => n.Description).FirstOrDefault();
        return descriptionString ?? value.ToString();
    }

    /// <summary>
    /// 指定した型のDescriptionを取り出します。
    /// 設定されていない場合はToString()の値を返します。
    /// </summary>
    public static string GetDescriptionAttribute<T>()
    {
        var attributes = Attribute.GetCustomAttributes(typeof(T), false);
        var descriptionString = attributes.Select(n => ((DescriptionAttribute)n).Description).FirstOrDefault();
        return descriptionString ?? typeof(T).ToString();
    }
}
