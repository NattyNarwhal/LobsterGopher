using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;

namespace LobsterGopher
{
    static class WordWrap
    {
        public static string Wrap(string text, int lineWidth)
        {
            return string.Join(string.Empty,
                               Wrap(
                                   text.Split(new char[0],
                                              StringSplitOptions
                                                  .RemoveEmptyEntries),
                                   lineWidth));
        }

        public static IEnumerable<string> Wrap(IEnumerable<string> words,
                                                int lineWidth)
        {
            var currentWidth = 0;
            foreach (var word in words)
            {
                if (currentWidth != 0)
                {
                    if (currentWidth + word.Length < lineWidth)
                    {
                        currentWidth++;
                        yield return " ";
                    }
                    else
                    {
                        currentWidth = 0;
                        yield return Environment.NewLine;
                    }
                }
                currentWidth += word.Length;
                yield return word;
            }
        }
    }

    static class Html
    {
        public static string ConvertHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        public static void ConvertTo(HtmlNode node, TextWriter outText)
        {
            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    html = ((HtmlTextNode)node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }
                    break;
            }
        }

        private static void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText);
            }
        }

    }
}
