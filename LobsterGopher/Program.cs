using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GopherSharp;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using System.Net.Cache;
using Mono.Options;

namespace LobsterGopher
{
    class Program
    {
        static string Hostname = "127.0.0.1";
        static int Port = 70;

        static int Columns = 72;

        static TcpListener Listener = new TcpListener(IPAddress.Any, Port);
        static WebClient wc = new WebClient();

        static void Main(string[] args)
        {
            try
            {
                var p = new OptionSet() {
                { "h|host=", "the server's hostname", v => Hostname = v },
                { "p|port=", "the server's port", v => Port = int.Parse(v) },
                { "P|proxy=", "a proxy to talk to", v => wc.Proxy = new WebProxy(v) }
                };

                p.Parse(args);
            }
            catch (OptionException)
            {
                // TODO
            }

            wc.Headers.Add("user-agent", "LobstersGopherProxy/0.0 (u/calvin)");
            wc.CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);
            wc.Encoding = Encoding.UTF8;

            Listener.Start();

            while (true)
                new Thread(new ParameterizedThreadStart(ServerHandler)).Start(Listener.AcceptTcpClient());
        }

        static int GetIndentation(int? level)
        {
            return (level ?? 1 - 1) * 2;
        }

        static void ServerHandler(Object obj)
        {
            // unbox client
            TcpClient c = (TcpClient)obj;
            using (Stream s = c.GetStream())
            {
                var sr = new StreamReader(s);
                var sw = new StreamWriter(s);
                sw.AutoFlush = true;

                var path = sr.ReadLine();
                //var raw = false; // use items if false, text if true
                //var text = "";
                var items = new List<GopherItem>();

                // TODO: An actual router
                if (path == "/" || path == "")
                {
                    items.Add(new GopherItem()
                    {
                        DisplayString = "Hottest stories",
                        Selector = "/hottest",
                        ItemType = '1',
                        Hostname = Hostname,
                        Port = Port
                    });
                    items.Add(new GopherItem()
                    {
                        DisplayString = "Newest stories",
                        Selector = "/newest",
                        ItemType = '1',
                        Hostname = Hostname,
                        Port = Port
                    });
                    items.Add(new GopherItem()
                    {
                        DisplayString = "Tag list",
                        Selector = "/tags",
                        ItemType = '1',
                        Hostname = Hostname,
                        Port = Port
                    });
                    items.Add(new GopherItem()
                    {
                        DisplayString = "by lobsters /u/calvin"
                    });
                }
                else if (path == "/hottest")
                {
                    items.AddRange(GetListing("hottest"));
                }
                else if (path == "/newest")
                {
                    items.AddRange(GetListing("newest"));
                }
                else if (path == "/tags")
                {
                    items.AddRange(GetTagListing());
                }
                else if (path.StartsWith("/t/") || path.StartsWith("/t\t"))
                {
                    items.AddRange(GetListing(path.Remove(0, 1).Trim()));
                }
                else if (path.StartsWith("/s/") || path.StartsWith("/s\t"))
                {
                    items.AddRange(GetStory(path.Remove(0, 3).Trim()));
                }
                else if (path.StartsWith("/u/") || path.StartsWith("/u\t"))
                {
                    items.AddRange(GetUser(path.Remove(0, 3).Trim()));
                }
                else if (path.StartsWith("/c/") || path.StartsWith("/c\t"))
                {
                    items.AddRange(GetComment(path.Remove(0, 3).Trim()));
                }
                else
                {
                    items.Add(ReturnError("invalid request"));
                }

                foreach (var i in items)
                    sw.WriteLine(i.ToString());
                sw.Write(".");

                Console.WriteLine(String.Format("{0} {1} {2}", ((IPEndPoint)c.Client.RemoteEndPoint).Address, DateTime.Now, path));
            }
        }

        static IEnumerable<GopherItem> GetListing(string thing)
        {
            string json = null;
            try
            {
                json = wc.DownloadString(String.Format("https://lobste.rs/{0}.json", thing));
            }
            catch (WebException)
            {
                // HACK: you can't yield in try/catch blocks, so we have to do all this ugliness
                // handle in next
            }
            if (json == null)
            {
                yield return ReturnError("invalid request");
                yield break;
            }

            var items = JsonConvert.DeserializeObject<List<LobstersItem>>(json);
            foreach (var i in items)
            {
                // Link
                yield return new GopherItem() { DisplayString = i.Title, Selector = String.Format("/s/{0}", i.ShortId), ItemType = '1', Hostname = Hostname, Port = Port };
                // Meta
                yield return new GopherItem()
                {
                    DisplayString =
                        String.Format("by {0} {1} | {2} points | {3} comments | {4}",
                            i.User.Username, i.Created, i.Score, i.CommentCount, String.Join(", ", i.Tags))
                };
                // Spacer
                yield return new GopherItem();
            }
        }

        static IEnumerable<GopherItem> GetTagListing()
        {
            string json = null;
            try
            {
                json = wc.DownloadString("https://lobste.rs/tags.json");
            }
            catch (WebException)
            {
                // HACK: you can't yield in try/catch blocks, so we have to do all this ugliness
                // handle in next
            }
            if (json == null)
            {
                yield return ReturnError("invalid request");
                yield break;
            }

            var items = JsonConvert.DeserializeObject<List<LobstersTag>>(json);
            foreach (var i in items)
            {
                // Meta
                yield return new GopherItem()
                {
                    DisplayString = i.Description,
                    ItemType = '1',
                    Hostname = Hostname,
                    Port = Port,
                    Selector = String.Format("/t/{0}", i.Name)
                };
            }
        }

        static IEnumerable<GopherItem> GetStory(string short_id)
        {
            string json = null;
            try
            {
                json = wc.DownloadString(String.Format("https://lobste.rs/s/{0}.json", short_id));
            }
            catch (WebException)
            {
                // handle in next
            }
            if (json == null)
            {
                yield return ReturnError("invalid request");
                yield break;
            }
            var item = JsonConvert.DeserializeObject<LobstersItem>(json);

            // Story Link & Metadata
            yield return ReturnLink(item.Url, item.Title);
            yield return new GopherItem()
            {
                DisplayString = String.Format("by {0} {1} | {2} points | {3} comments",
                    item.User.Username, item.Created, item.Score, item.CommentCount)
            };
            foreach (string t in item.Tags)
            {
                yield return new GopherItem()
                {
                    DisplayString = t,
                    ItemType = '1',
                    Hostname = Hostname,
                    Port = Port,
                    Selector = String.Format("/t/{0}", t)
                };
            }
            yield return new GopherItem();
            yield return new GopherItem()
            {
                DisplayString = "Show OP's profile",
                ItemType = '1',
                Selector = String.Format("/u/{0}", item.User.Username),
                Hostname = Hostname,
                Port = Port
            };
            yield return ReturnLink(item.CommentsUrl, "View on lobste.rs (WWW)");
            yield return new GopherItem();

            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                foreach (var l in
                    Regex.Split(WordWrap.Wrap(Html.ConvertHtml(item.Description),
                        Columns), "\r?\n"))
                {
                    yield return new GopherItem()
                    {
                        DisplayString = l
                    };
                }
                yield return new GopherItem();
            }

            // Comments
            foreach (var i in item.Comments)
            {
                yield return new GopherItem()
                {
                    DisplayString = String.Format("posted by {0}", i.User.Username),
                    ItemType = '1',
                    Selector = String.Format("/u/{0}", i.User.Username),
                    Hostname = Hostname,
                    Port = Port
                };
                yield return new GopherItem()
                {
                    DisplayString = String.Format("{0} | {1} points",
                        (i.Updated > i.Created) ? i.Updated : i.Created, i.Score),
                    ItemType = '1',
                    Selector = String.Format("/c/{0}", i.ShortId),
                    Hostname = Hostname,
                    Port = Port
                };
                // Text, indented with a limit
                foreach (var l in
                    Regex.Split(WordWrap.Wrap(Html.ConvertHtml(i.Comment),
                        Columns - GetIndentation(i.IndentLevel)), "\r?\n"))
                {
                    yield return new GopherItem()
                    {
                        // pad functions only pad if the string isn't long enough, so make it so
                        DisplayString = l.PadLeft(GetIndentation(i.IndentLevel) + l.Length)
                    };
                }
                // Spacer
                yield return new GopherItem();
            }
        }

        public static IEnumerable<GopherItem> GetComment(string short_id)
        {
            string json = null;
            try
            {
                json = wc.DownloadString(String.Format("https://lobste.rs/c/{0}.json", short_id));
            }
            catch (WebException)
            {
                // handle in next
            }
            if (json == null)
            {
                yield return ReturnError("invalid request");
                yield break;
            }
            var item = JsonConvert.DeserializeObject<LobstersComment>(json);

            yield return new GopherItem()
            {
                DisplayString = String.Format("posted by {0}", item.User.Username),
                ItemType = '1',
                Selector = String.Format("/u/{0}", item.User.Username),
                Hostname = Hostname,
                Port = Port
            };
            yield return ReturnLink(item.Url, String.Format("{0} | {1} points",
                    (item.Updated > item.Created) ? item.Updated : item.Created, item.Score));
            // Text, indented root limit (we're viewing standalone
            foreach (var l in
                Regex.Split(WordWrap.Wrap(Html.ConvertHtml(item.Comment),
                    Columns - GetIndentation(1)), "\r?\n"))
            {
                yield return new GopherItem()
                {
                    // pad functions only pad if the string isn't long enough, so make it so
                    DisplayString = l.PadLeft(GetIndentation(item.IndentLevel) + l.Length)
                };
            }
        }

        public static IEnumerable<GopherItem> GetUser(string user)
        {
            string json = null;
            try
            {
                json = wc.DownloadString(String.Format("https://lobste.rs/u/{0}.json", user));
            }
            catch (WebException)
            {
                // handle in next
            }
            if (json == null)
            {
                yield return ReturnError("invalid request");
                yield break;
            }
            //string json = wc.DownloadString(String.Format("https://lobste.rs/u/{0}.json", user));
            var item = JsonConvert.DeserializeObject<LobstersUser>(json);

            // Story Link & Metadata
            yield return new GopherItem()
            {
                DisplayString = String.Format("{0} signed up on {1} and has {2} karma", item.Username, item.Created, item.Karma)
            };
            yield return new GopherItem()
            {
                DisplayString = String.Format("Mod: {0}, Admin: {1}", item.IsMod, item.IsAdmin)
            };
            yield return new GopherItem();
            // Wrap description
            foreach (var l in Regex.Split(WordWrap.Wrap(Html.ConvertHtml(item.About), Columns), "\r?\n"))
            {
                yield return new GopherItem()
                {
                    DisplayString = l
                };
            }
        }

        static GopherItem ReturnLink(string uri, string message)
        {
            // Overbite surpresses URL: links if they don't link to valid servers
            return new GopherItem()
            {
                DisplayString = message,
                ItemType = 'h',
                Hostname = Hostname,
                Port = Port,
                Selector = "URL:" + uri
            };
        }

        static GopherItem ReturnError(string message)
        {
            return new GopherItem() { ItemType = '3', DisplayString = message };
        }
    }
}
