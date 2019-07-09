using System.Xml;
using System.Text;

public static class XmlEx
{
    /// <summary>
    /// XML ドキュメントを UTF-8 形式の文字列に変換します。
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    public static string ToStringXml(this XmlDocument doc)
    {
        StringWriterUTF8 writer = new StringWriterUTF8();
        doc.Save(writer);
        string r = writer.ToString();
        writer.Close();
        return r;
    }
    /// <summary>
    /// UTF-8 用 StringWriter です。
    /// </summary>
    class StringWriterUTF8 : System.IO.StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}