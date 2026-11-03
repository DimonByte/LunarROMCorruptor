//MIT License

//Copyright (c) 2026 DimonByte

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace LunarROMCorruptor.Modules
{
    /// <summary>
    /// The in-app guides: the README and docs/*.md, loaded from GitHub so the program always shows the latest version of the guides
    /// (falls back to the copy shipped next to LunarROMCorruptor.exe in the Guides folder, then to the last downloaded copy).
    /// </summary>
    public static class Guides
    {
        //Change these if the repository moves
        public const string RawBaseUrl = "https://raw.githubusercontent.com/DimonByte/LunarROMCorruptor/master/";
        public const string WebBaseUrl = "https://github.com/DimonByte/LunarROMCorruptor/blob/master/";

        public sealed record Guide(string Title, string RepoPath);

        /// <summary>
        /// The guides shown in the list (paths relative to the repository root).
        /// </summary>
        public static readonly Guide[] All =
        [
            new("Overview (README)", "README.md"),
            new("Getting Started", "docs/Getting-Started.md"),
            new("Corruption Engines", "docs/Corruption-Engines.md"),
            new("Engine Presets", "docs/Engine-Presets.md"),
            new("Stashes, Saves & Bisect", "docs/Stash-and-Saves.md"),
            new("ByteView", "docs/ByteView.md"),
            new("Automation & Multi-File", "docs/Automation-and-Multi-File.md"),
        ];

        public static string BundledDirectory => Path.Combine(Application.StartupPath, "Guides");
        private static string CacheDirectory => Path.Combine(Application.StartupPath, "Guides", "Downloaded");

        private static readonly HttpClient Http = CreateClient();
        private static readonly Dictionary<string, string> _session = new(StringComparer.OrdinalIgnoreCase); //Downloaded this session

        private static HttpClient CreateClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"LunarROMCorruptor/{StashLibrary.AppVersion}");
            return client;
        }

        public enum Source { Online, Bundled, Downloaded, Missing }

        /// <summary>
        /// The copy that can be shown right away (no network): bundled with the program, or downloaded earlier.
        /// </summary>
        public static (string? Markdown, Source Source) LoadLocal(string repoPath)
        {
            lock (_session)
                if (_session.TryGetValue(repoPath, out var online)) return (online, Source.Online);
            foreach (var (folder, source) in new[] { (BundledDirectory, Source.Bundled), (CacheDirectory, Source.Downloaded) })
            {
                string file = Path.Combine(folder, repoPath.Replace('/', Path.DirectorySeparatorChar));
                try
                {
                    if (File.Exists(file)) return (File.ReadAllText(file), source);
                }
                catch { }
            }
            return (null, Source.Missing);
        }

        /// <summary>
        /// Downloads the latest version from GitHub (and keeps a copy for offline use). Returns null when offline.
        /// </summary>
        public static async Task<string?> DownloadAsync(string repoPath, CancellationToken token = default)
        {
            try
            {
                string text = await Http.GetStringAsync(RawBaseUrl + repoPath, token);
                lock (_session) _session[repoPath] = text;
                try
                {
                    string file = Path.Combine(CacheDirectory, repoPath.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                    await File.WriteAllTextAsync(file, text, token);
                }
                catch { } //The program folder may be read-only, the copy is only a convenience
                return text;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Guides: unable to download {repoPath} from GitHub ({ex.Message}). Showing the offline copy.", StatusSeverityType.Debug);
                return null;
            }
        }

        /// <summary>
        /// Resolves a link inside a guide relative to the guide's repository path (e.g. "../README.md" from "docs/ByteView.md").
        /// Returns the repository path and the #anchor, or null if it isn't a repository file.
        /// </summary>
        public static (string Path, string? Anchor)? ResolveRepoLink(string currentRepoPath, string href)
        {
            if (href.Contains("://") || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)) return null;
            string anchor = "";
            int hash = href.IndexOf('#');
            if (hash >= 0) { anchor = href[(hash + 1)..]; href = href[..hash]; }
            if (href.Length == 0) return (currentRepoPath, anchor);
            var parts = new List<string>(currentRepoPath.Split('/')[..^1]);
            foreach (string part in href.Replace('\\', '/').Split('/'))
            {
                if (part == "..") { if (parts.Count > 0) parts.RemoveAt(parts.Count - 1); }
                else if (part != "." && part.Length > 0) parts.Add(part);
            }
            return (string.Join('/', parts), anchor.Length > 0 ? anchor : null);
        }
    }

    /// <summary>
    /// A small Markdown to HTML converter for the guides (GitHub style: headings with ids, tables, lists, block quotes and
    /// [!NOTE]/[!CAUTION] alerts, code blocks, inline code/bold/italic/links/images and raw HTML such as &lt;details&gt; and &lt;a id&gt;).
    /// Links to other .md files become "lrcguide:" links so the guide viewer can open them inside LRC.
    /// </summary>
    public static partial class MarkdownRenderer
    {
        public const string GuideScheme = "lrcguide:";

        [GeneratedRegex(@"^(#{1,6})\s+(.*?)\s*#*\s*$")]
        private static partial Regex HeadingRegex();
        [GeneratedRegex(@"^\s*(?:[-*+]|\d+[.)])\s+")]
        private static partial Regex ListItemRegex();
        [GeneratedRegex(@"^\s*\|?\s*:?-{2,}:?\s*(\|\s*:?-{2,}:?\s*)*\|?\s*$")]
        private static partial Regex TableSeparatorRegex();
        [GeneratedRegex(@"^\s*(-{3,}|\*{3,}|_{3,})\s*$")]
        private static partial Regex RuleRegex();

        /// <summary>
        /// Converts a guide to a full HTML page. <paramref name="repoPath"/> is used to make relative links/images work.
        /// </summary>
        public static string ToHtmlPage(string markdown, string repoPath, string title)
        {
            var body = new StringBuilder();
            var lines = markdown.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            var slugs = new Dictionary<string, int>();
            int i = 0;
            while (i < lines.Length)
            {
                string line = lines[i];
                string trimmed = line.Trim();
                if (trimmed.Length == 0) { i++; continue; }

                if (trimmed.StartsWith("```") || trimmed.StartsWith("~~~"))
                {
                    string fence = trimmed[..3];
                    var code = new StringBuilder();
                    i++;
                    while (i < lines.Length && !lines[i].Trim().StartsWith(fence)) code.Append(WebUtility.HtmlEncode(lines[i++])).Append('\n');
                    i++;
                    body.Append("<pre><code>").Append(code).Append("</code></pre>\n");
                    continue;
                }
                var heading = HeadingRegex().Match(line);
                if (heading.Success)
                {
                    int level = heading.Groups[1].Length;
                    string text = heading.Groups[2].Value;
                    string slug = Slug(StripMarkdown(text));
                    if (slugs.TryGetValue(slug, out int n)) { slugs[slug] = n + 1; slug += "-" + n; } else slugs[slug] = 1;
                    body.Append($"<h{level} id=\"{slug}\">{Inline(text, repoPath)}</h{level}>\n");
                    i++;
                    continue;
                }
                if (RuleRegex().IsMatch(line)) { body.Append("<hr>\n"); i++; continue; }
                if (trimmed.StartsWith('>'))
                {
                    var quote = new List<string>();
                    while (i < lines.Length && lines[i].Trim().StartsWith('>')) quote.Add(Regex.Replace(lines[i++].Trim(), @"^>\s?", ""));
                    string kind = "";
                    if (quote.Count > 0)
                    {
                        var alert = Regex.Match(quote[0], @"^\[!(NOTE|TIP|IMPORTANT|WARNING|CAUTION)\]\s*$", RegexOptions.IgnoreCase);
                        if (alert.Success) { kind = alert.Groups[1].Value.ToLowerInvariant(); quote.RemoveAt(0); }
                    }
                    string inner = ToHtmlFragment(string.Join('\n', quote), repoPath, slugs);
                    body.Append(kind.Length > 0
                        ? $"<blockquote class=\"alert {kind}\"><p class=\"alert-title\">{char.ToUpperInvariant(kind[0])}{kind[1..]}</p>{inner}</blockquote>\n"
                        : $"<blockquote>{inner}</blockquote>\n");
                    continue;
                }
                if (trimmed.Contains('|') && i + 1 < lines.Length && TableSeparatorRegex().IsMatch(lines[i + 1]))
                {
                    var header = SplitRow(line);
                    var aligns = SplitRow(lines[i + 1]).Select(c => c.StartsWith(':') && c.EndsWith(':') ? "center" : c.EndsWith(':') ? "right" : "left").ToList();
                    body.Append("<table><thead><tr>");
                    for (int c = 0; c < header.Count; c++) body.Append($"<th style=\"text-align:{(c < aligns.Count ? aligns[c] : "left")}\">{Inline(header[c], repoPath)}</th>");
                    body.Append("</tr></thead><tbody>\n");
                    i += 2;
                    while (i < lines.Length && lines[i].Trim().Length > 0 && lines[i].Contains('|'))
                    {
                        var cells = SplitRow(lines[i++]);
                        body.Append("<tr>");
                        for (int c = 0; c < header.Count; c++) body.Append($"<td style=\"text-align:{(c < aligns.Count ? aligns[c] : "left")}\">{(c < cells.Count ? Inline(cells[c], repoPath) : "")}</td>");
                        body.Append("</tr>\n");
                    }
                    body.Append("</tbody></table>\n");
                    continue;
                }
                if (ListItemRegex().IsMatch(line))
                {
                    i = RenderList(lines, i, body, repoPath);
                    continue;
                }
                if (trimmed.StartsWith('<'))
                {
                    //Raw HTML block (div, details, summary, img, a id...): copied as it is, with markdown inside processed per line
                    //Raw HTML lines only; a markdown line right after (e.g. a heading after <a id="..."></a>) is handled normally
                    while (i < lines.Length && lines[i].Trim().StartsWith('<'))
                        body.Append(FixHtmlUrls(lines[i++].Trim(), repoPath)).Append('\n');
                    continue;
                }
                //Paragraph: until a blank line or another block
                var para = new StringBuilder();
                while (i < lines.Length)
                {
                    string l = lines[i];
                    string t = l.Trim();
                    if (t.Length == 0 || HeadingRegex().IsMatch(l) || t.StartsWith("```") || t.StartsWith('>') || ListItemRegex().IsMatch(l) || RuleRegex().IsMatch(l) ||
                        (t.Contains('|') && i + 1 < lines.Length && TableSeparatorRegex().IsMatch(lines[i + 1])))
                        break;
                    if (para.Length > 0) para.Append(l.EndsWith("  ") ? "<br>" : " ");
                    para.Append(Inline(t, repoPath));
                    i++;
                }
                body.Append("<p>").Append(para).Append("</p>\n");
            }
            return Page(body.ToString(), title);
        }

        private static string ToHtmlFragment(string markdown, string repoPath, Dictionary<string, int> slugs)
        {
            string page = ToHtmlPage(markdown, repoPath, "");
            int a = page.IndexOf("<body>", StringComparison.Ordinal) + 6, b = page.LastIndexOf("</body>", StringComparison.Ordinal);
            return a > 5 && b > a ? page[a..b] : page;
        }

        private static int RenderList(string[] lines, int i, StringBuilder body, string repoPath)
        {
            int indent = lines[i].Length - lines[i].TrimStart().Length;
            bool ordered = Regex.IsMatch(lines[i], @"^\s*\d+[.)]\s");
            body.Append(ordered ? "<ol>\n" : "<ul>\n");
            while (i < lines.Length)
            {
                string line = lines[i];
                if (line.Trim().Length == 0)
                {
                    //A blank line ends the list unless the next line is another item of it
                    if (i + 1 < lines.Length && ListItemRegex().IsMatch(lines[i + 1]) && lines[i + 1].Length - lines[i + 1].TrimStart().Length >= indent) { i++; continue; }
                    break;
                }
                int lineIndent = line.Length - line.TrimStart().Length;
                if (!ListItemRegex().IsMatch(line))
                {
                    if (lineIndent > indent) { body.Length -= "</li>\n".Length; body.Append(' ').Append(Inline(line.Trim(), repoPath)).Append("</li>\n"); i++; continue; }
                    break;
                }
                if (lineIndent < indent) break;
                if (lineIndent > indent)
                {
                    //Nested list goes inside the previous item
                    if (body.ToString().EndsWith("</li>\n")) body.Length -= "</li>\n".Length;
                    i = RenderList(lines, i, body, repoPath);
                    body.Append("</li>\n");
                    continue;
                }
                string text = ListItemRegex().Replace(line, "", 1);
                body.Append("<li>").Append(Inline(text, repoPath)).Append("</li>\n");
                i++;
            }
            body.Append(ordered ? "</ol>\n" : "</ul>\n");
            return i;
        }

        private static List<string> SplitRow(string row)
        {
            string r = row.Trim();
            if (r.StartsWith('|')) r = r[1..];
            if (r.EndsWith('|') && !r.EndsWith("\\|")) r = r[..^1];
            var cells = new List<string>();
            var cell = new StringBuilder();
            bool inCode = false;
            for (int k = 0; k < r.Length; k++)
            {
                char c = r[k];
                if (c == '\\' && k + 1 < r.Length && r[k + 1] == '|') { cell.Append('|'); k++; continue; }
                if (c == '`') inCode = !inCode;
                if (c == '|' && !inCode) { cells.Add(cell.ToString().Trim()); cell.Clear(); continue; }
                cell.Append(c);
            }
            cells.Add(cell.ToString().Trim());
            return cells;
        }

        /// <summary>
        /// GitHub's heading id: lower case, punctuation removed, spaces become dashes.
        /// </summary>
        public static string Slug(string text)
        {
            var sb = new StringBuilder();
            foreach (char c in text.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_') sb.Append(c);
                else if (c == ' ') sb.Append('-');
            }
            return sb.ToString();
        }

        private static string StripMarkdown(string text) => Regex.Replace(Regex.Replace(text, @"<[^>]+>", ""), @"[*_`\[\]]|\(([^)]*)\)", "");

        private static string ResolveUrl(string url, string repoPath, bool image)
        {
            if (url.StartsWith('#')) return url;
            if (url.Contains("://") || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) || url.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) return url;
            var resolved = Guides.ResolveRepoLink(repoPath, url);
            if (resolved == null) return url;
            var (path, anchor) = resolved.Value;
            if (!image && path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) return GuideScheme + path + (anchor != null ? "#" + anchor : "");
            return (image ? Guides.RawBaseUrl : Guides.WebBaseUrl) + path + (anchor != null ? "#" + anchor : "");
        }

        private static string FixHtmlUrls(string html, string repoPath) =>
            Regex.Replace(html, @"\b(src|href)=""([^""]*)""", m => $"{m.Groups[1].Value}=\"{WebUtility.HtmlEncode(ResolveUrl(WebUtility.HtmlDecode(m.Groups[2].Value), repoPath, m.Groups[1].Value == "src"))}\"");

        /// <summary>
        /// Inline markdown: code spans, images, links, bold, italic and inline HTML tags.
        /// </summary>
        private static string Inline(string text, string repoPath)
        {
            var codes = new List<string>();
            //Protect code spans first
            text = Regex.Replace(text, @"(`+)(.+?)\1", m => { codes.Add($"<code>{WebUtility.HtmlEncode(m.Groups[2].Value.Trim())}</code>"); return $"\u0001{codes.Count - 1}\u0001"; });
            //Inline HTML tags are kept, everything else is escaped
            var tags = new List<string>();
            text = Regex.Replace(text, @"</?[a-zA-Z][a-zA-Z0-9]*(\s+[^<>]*)?/?>", m => { tags.Add(FixHtmlUrls(m.Value, repoPath)); return $"\u0002{tags.Count - 1}\u0002"; });
            text = WebUtility.HtmlEncode(text).Replace("&#39;", "'").Replace("&quot;", "\"");
            text = Regex.Replace(text, @"\\([\\`*_{}\[\]()#+\-.!|])", "$1");
            text = Regex.Replace(text, @"!\[([^\]]*)\]\(([^)\s]+)(?:\s+""[^""]*"")?\)", m => $"<img alt=\"{m.Groups[1].Value}\" src=\"{WebUtility.HtmlEncode(ResolveUrl(WebUtility.HtmlDecode(m.Groups[2].Value), repoPath, true))}\">");
            text = Regex.Replace(text, @"\[([^\]]+)\]\(([^)\s]+)\)", m => $"<a href=\"{WebUtility.HtmlEncode(ResolveUrl(WebUtility.HtmlDecode(m.Groups[2].Value), repoPath, false))}\">{m.Groups[1].Value}</a>");
            text = Regex.Replace(text, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
            text = Regex.Replace(text, @"(?<!\*)\*(?![\s*])(.+?)(?<![\s*])\*(?!\*)", "<em>$1</em>"); //* works inside words too (*N*th), like on GitHub
            text = Regex.Replace(text, @"(?<!\w)_(?!\s)(.+?)(?<!\s)_(?!\w)", "<em>$1</em>");
            text = Regex.Replace(text, "\u0002(\\d+)\u0002", m => tags[int.Parse(m.Groups[1].Value)]);
            text = Regex.Replace(text, "\u0001(\\d+)\u0001", m => codes[int.Parse(m.Groups[1].Value)]);
            return text;
        }

        private static string Page(string body, string title) => $$"""
            <!DOCTYPE html>
            <html><head><meta charset="utf-8"><meta http-equiv="X-UA-Compatible" content="IE=edge"><title>{{WebUtility.HtmlEncode(title)}}</title>
            <style>
            body { background:#1c1b37; color:#d5d8d8; font-family:'Segoe UI',sans-serif; font-size:14px; line-height:1.5; margin:16px 24px; }
            a { color:#7fd6ff; } a:hover { color:#c0ffff; }
            h1,h2,h3,h4 { color:#c0ffff; border-bottom:1px solid #34386f; padding-bottom:4px; margin-top:24px; }
            h1 { font-size:26px; } h2 { font-size:21px; } h3 { font-size:17px; } h4 { font-size:15px; border:none; }
            code { background:#2d2d40; color:#ffd88a; padding:1px 5px; border-radius:3px; font-family:Consolas,monospace; font-size:13px; }
            pre { background:#14142a; border:1px solid #34386f; padding:10px; overflow:auto; }
            pre code { background:none; padding:0; color:#d5d8d8; }
            table { border-collapse:collapse; margin:10px 0; width:100%; }
            th,td { border:1px solid #34386f; padding:6px 9px; vertical-align:top; }
            th { background:#272b53; color:#c0ffff; } tr:nth-child(even) td { background:#22224a; }
            blockquote { border-left:4px solid #5a6ec0; margin:10px 0; padding:4px 12px; background:#22224a; }
            blockquote.alert.caution, blockquote.alert.warning { border-color:#e05050; } blockquote.alert.tip { border-color:#50c080; } blockquote.alert.important { border-color:#a070e0; }
            .alert-title { font-weight:bold; margin:4px 0; }
            hr { border:none; border-top:1px solid #34386f; margin:20px 0; }
            img { max-width:100%; } details { margin:8px 0; } summary { cursor:pointer; color:#c0ffff; }
            </style></head><body>
            {{body}}
            </body></html>
            """;
    }
}
