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


using System.Globalization;
using System.Numerics;
using System.Text;

namespace LunarROMCorruptor.Modules.CorruptionInternals.Engines
{
    /// <summary>
    /// Source Code Engine: reads a text/source file, finds literals (booleans, numbers, strings, null values) and optionally
    /// operators and identifiers, and corrupts them. Comments are skipped.
    ///
    /// Intensity mode corrupts N random values, every Nth mode corrupts every Nth value. Start/End byte limit which part of the file is used.
    /// Every change is recorded in the stash as: [x] File(byteOffset).VARLINE("name",line).SET("new text")
    /// </summary>
    public class SourceCodeEngine
    {
        public enum NumberMode { RANDOM, NUDGE, SCALE, SIGNFLIP, MAGIC, DIGITS }
        public enum StringMode { RANDOM, SHUFFLE, REVERSE, TRUNCATE, SWAP, SUBSTITUTE, REPEAT }
        public enum CommentStyle { AUTO, CLIKE, HASH, DASHDASH, SEMICOLON, XML, BASIC, NONE }
        public enum TokenKind { Boolean, Integer, Float, String, Null, Operator, Identifier, Keyword, Other }

        public sealed class Settings
        {
            public bool Booleans = true;
            public bool Integers = true;
            public bool Floats = true;
            public bool Strings = true;
            public bool Nulls;
            public bool Operators;
            public bool Identifiers;
            public NumberMode Numbers = NumberMode.RANDOM;
            public double NumberStrength = 50; //Percent
            public StringMode StringCorruption = StringMode.RANDOM;
            public CommentStyle Comments = CommentStyle.AUTO;
        }

        public const long MaxFileSize = 512L * 1024 * 1024;

        public struct Token
        {
            public int Start; //Index in the decoded text
            public int Length;
            public TokenKind Kind;
            public long ByteOffset; //Offset in the file (including any BOM), used for the byte range and the stash
            public int Line; //1 based
            public int QuoteLength; //String tokens: length of the opening delimiter (prefix + quotes)
            public int CloseLength; //String tokens: length of the closing delimiter
        }

        #region Entry points

        public static void Run(Settings s, CorruptionContext ctx, string inputPath, string outputPath)
        {
            var source = LoadText(inputPath);
            var style = ResolveStyle(s.Comments, inputPath);
            TraceLogger.Log($"Source Code Engine: {inputPath} decoded as {source.Encoding.WebName}{(source.Preamble.Length > 0 ? " (with BOM)" : "")}, comment style {style}.");
            if (source.LooksBinary)
                TraceLogger.Log("This file contains NUL bytes and probably isn't a text/source file. The Source Code Engine may give unexpected results.", StatusSeverityType.Warning);

            var tokens = Tokenize(source, style, ctx.Token);
            var candidates = new List<int>();
            for (int k = 0; k < tokens.Count; k++)
            {
                var t = tokens[k];
                if (t.ByteOffset < ctx.Start || t.ByteOffset > ctx.End) continue;
                if (!IsEnabled(t.Kind, s)) continue;
                if (t.Kind == TokenKind.String && IsKey(source.Text, tokens, k)) continue; //"key": value - keys are names, not values
                candidates.Add(k);
            }
            TraceLogger.Log($"Source Code Engine found {tokens.Count} tokens, {candidates.Count} of them match the enabled value types inside the byte range.");
            if (candidates.Count == 0)
            {
                TraceLogger.Log("The Source Code Engine didn't find any values to corrupt. Check the enabled value types, the comment style and the byte range.", StatusSeverityType.Warning, true);
                return;
            }

            //Select the tokens to corrupt
            var selected = new List<int>();
            if (ctx.EveryNth)
            {
                for (long n = 0; n < candidates.Count; n += ctx.Intensity)
                    selected.Add(candidates[(int)n]);
            }
            else
            {
                int count = (int)Math.Min(ctx.Intensity, candidates.Count);
                var pool = candidates.ToArray();
                for (int n = 0; n < count; n++) //Partial Fisher-Yates, each value is only corrupted once
                {
                    int j = ctx.Rnd.Next(n, pool.Length);
                    (pool[n], pool[j]) = (pool[j], pool[n]);
                    selected.Add(pool[n]);
                }
                selected.Sort();
            }

            var pools = new Pools(source.Text, tokens);
            var replacements = new List<(Token Token, string NewText)>();
            for (int n = 0; n < selected.Count; n++)
            {
                var t = tokens[selected[n]];
                string old = source.Text.Substring(t.Start, t.Length);
                string replacement = CorruptToken(t, old, s, ctx.Rnd, pools);
                if (replacement == old) continue;
                replacements.Add((t, replacement));
                ctx.Stash?.AddLine(StashParser.FormatVarLine(t.ByteOffset, FindName(source.Text, tokens, selected[n]), t.Line, replacement));
                if ((n + 1) % 4096 == 0) ctx.Tick(n + 1, selected.Count);
            }
            ctx.Token.ThrowIfCancellationRequested();

            WriteText(outputPath, source, replacements);
            TraceLogger.Log($"Source Code Engine changed {replacements.Count} value(s) and saved the result to {outputPath}");
        }

        /// <summary>
        /// Re-applies VARLINE stash entries to the original file.
        /// </summary>
        public static void ApplyStash(List<StashParser.VarLineEntry> entries, string inputPath, string outputPath, StashLog stash)
        {
            var source = LoadText(inputPath);
            var tokens = Tokenize(source, ResolveStyle(CommentStyle.AUTO, inputPath), CancellationToken.None);
            var byOffset = new Dictionary<long, Token>();
            foreach (var t in tokens) byOffset.TryAdd(t.ByteOffset, t);

            var changes = new Dictionary<long, (Token Token, string NewText)>();
            int missing = 0;
            foreach (var e in entries)
            {
                if (!byOffset.TryGetValue(e.Offset, out var t))
                {
                    missing++;
                    TraceLogger.Log($"Stash entry for '{e.Name}' (line {e.Line}, offset {e.Offset}) doesn't match a value in this file and was skipped.", StatusSeverityType.Warning);
                    continue;
                }
                changes[e.Offset] = (t, e.Value); //Later entries for the same value win
                stash.AddLine(StashParser.FormatVarLine(e.Offset, e.Name, e.Line, e.Value));
            }
            WriteText(outputPath, source, [.. changes.Values.OrderBy(c => c.Token.Start)]);
            TraceLogger.Log($"Applied {changes.Count} Source Code stash entries ({missing} skipped) to {outputPath}");
            if (missing > 0)
                TraceLogger.Log($"{missing} stash entries didn't match the loaded file. Was this stash made from a different file?", StatusSeverityType.Warning, true);
        }

        /// <summary>
        /// True for strings used as keys, e.g. "speed" in {"speed": 10} or 'name' in {'name': 'x'}.
        /// </summary>
        private static bool IsKey(string text, List<Token> tokens, int index)
        {
            if (index + 1 >= tokens.Count) return false;
            var next = tokens[index + 1];
            return next.Length == 1 && text[next.Start] == ':' && next.Line == tokens[index].Line;
        }

        private static bool IsEnabled(TokenKind kind, Settings s) => kind switch
        {
            TokenKind.Boolean => s.Booleans,
            TokenKind.Integer => s.Integers,
            TokenKind.Float => s.Floats,
            TokenKind.String => s.Strings,
            TokenKind.Null => s.Nulls,
            TokenKind.Operator => s.Operators,
            TokenKind.Identifier => s.Identifiers,
            _ => false
        };

        #endregion

        #region Text loading/saving

        public sealed class SourceText
        {
            public required string Text;
            public required Encoding Encoding;
            public required byte[] Preamble;
            public bool LooksBinary;
        }

        public static SourceText LoadText(string path)
        {
            var info = new FileInfo(path);
            if (info.Length > MaxFileSize)
                throw new InvalidOperationException($"The file is too large for the Source Code Engine ({info.Length / 1048576} MB, the limit is {MaxFileSize / 1048576} MB).");
            byte[] bytes = File.ReadAllBytes(path);
            Encoding encoding;
            byte[] preamble = [];
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) { encoding = new UTF8Encoding(false); preamble = [0xEF, 0xBB, 0xBF]; }
            else if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0 && bytes[3] == 0) { encoding = new UTF32Encoding(false, false); preamble = [0xFF, 0xFE, 0, 0]; }
            else if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) { encoding = new UnicodeEncoding(false, false); preamble = [0xFF, 0xFE]; }
            else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) { encoding = new UnicodeEncoding(true, false); preamble = [0xFE, 0xFF]; }
            else
            {
                try
                {
                    encoding = new UTF8Encoding(false, true);
                    encoding.GetCharCount(bytes); //Throws if the file isn't valid UTF-8
                    encoding = new UTF8Encoding(false);
                }
                catch (DecoderFallbackException)
                {
                    encoding = Encoding.Latin1; //Every byte maps to one char, so this always round-trips
                }
            }
            string text = encoding.GetString(bytes, preamble.Length, bytes.Length - preamble.Length);
            bool binary = Array.IndexOf(bytes, (byte)0, 0, Math.Min(bytes.Length, 8192)) >= 0 && encoding is not UnicodeEncoding && encoding is not UTF32Encoding;
            return new SourceText { Text = text, Encoding = encoding, Preamble = preamble, LooksBinary = binary };
        }

        private static void WriteText(string outputPath, SourceText source, List<(Token Token, string NewText)> replacements)
        {
            var sb = new StringBuilder(source.Text.Length + 1024);
            int cursor = 0;
            foreach (var (t, newText) in replacements.OrderBy(r => r.Token.Start))
            {
                if (t.Start < cursor) continue; //Overlap safety
                sb.Append(source.Text, cursor, t.Start - cursor);
                sb.Append(newText);
                cursor = t.Start + t.Length;
            }
            sb.Append(source.Text, cursor, source.Text.Length - cursor);

            using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            stream.Write(source.Preamble);
            byte[] encoded = source.Encoding.GetBytes(sb.ToString());
            stream.Write(encoded);
        }

        #endregion

        #region Tokenizer

        private static readonly HashSet<string> CLikeSingleQuoteStringExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".js", ".jsx", ".mjs", ".cjs", ".ts", ".tsx", ".php", ".dart", ".css", ".scss", ".less", ".json5", ".coffee", ".groovy", ".gml", ".hx", ".as"
        };

        private static readonly string[] HashExtensions = [".py", ".pyw", ".rb", ".sh", ".bash", ".zsh", ".pl", ".pm", ".ps1", ".psm1", ".r", ".yaml", ".yml", ".toml", ".cfg", ".conf", ".mk", ".cmake", ".nim", ".jl", ".ex", ".exs", ".tcl", ".gd", ".properties", ".dockerfile", ".gitignore", ".env"];
        private static readonly string[] DashDashExtensions = [".lua", ".sql", ".hs", ".elm", ".ada", ".adb", ".ads", ".vhd", ".vhdl"];
        private static readonly string[] SemicolonExtensions = [".asm", ".s", ".inc", ".ini", ".lisp", ".lsp", ".clj", ".cljs", ".el", ".scm", ".rkt", ".reg"];
        private static readonly string[] XmlExtensions = [".xml", ".html", ".htm", ".xhtml", ".xaml", ".svg", ".plist", ".csproj", ".vbproj", ".props", ".targets", ".config", ".resx", ".vue"];
        private static readonly string[] BasicExtensions = [".vb", ".vbs", ".bas", ".frm", ".cls", ".vba"];
        private static readonly string[] NoCommentExtensions = [".json", ".txt", ".csv", ".tsv", ".log", ".md"];

        public static CommentStyle ResolveStyle(CommentStyle style, string path)
        {
            if (style != CommentStyle.AUTO) return style;
            string ext = Path.GetExtension(path).ToLowerInvariant();
            string name = Path.GetFileName(path).ToLowerInvariant();
            if (HashExtensions.Contains(ext) || name is "makefile" or "dockerfile" or ".gitignore") return CommentStyle.HASH;
            if (DashDashExtensions.Contains(ext)) return CommentStyle.DASHDASH;
            if (SemicolonExtensions.Contains(ext)) return CommentStyle.SEMICOLON;
            if (XmlExtensions.Contains(ext)) return CommentStyle.XML;
            if (BasicExtensions.Contains(ext)) return CommentStyle.BASIC;
            if (NoCommentExtensions.Contains(ext)) return CommentStyle.NONE;
            return CommentStyle.CLIKE; //C, C++, C#, Java, JS, TS, Go, Rust, Swift, Kotlin, PHP, GML, shaders, etc.
        }

        private static readonly HashSet<string> BoolWords = ["true", "false", "True", "False", "TRUE", "FALSE"];
        private static readonly HashSet<string> NullWords = ["null", "NULL", "nil", "None", "nullptr", "undefined", "Nothing"];
        private static readonly HashSet<string> Keywords =
        [
            "if", "else", "for", "while", "do", "switch", "case", "default", "break", "continue", "return", "function", "func", "fn", "def",
            "class", "struct", "enum", "interface", "public", "private", "protected", "internal", "static", "const", "let", "var", "val",
            "new", "delete", "this", "self", "super", "import", "from", "using", "namespace", "package", "try", "catch", "finally", "throw",
            "throws", "async", "await", "yield", "void", "int", "float", "double", "char", "string", "bool", "boolean", "byte", "short",
            "long", "unsigned", "signed", "auto", "extern", "typedef", "sizeof", "typeof", "instanceof", "in", "is", "as", "not", "and",
            "or", "lambda", "pass", "with", "global", "nonlocal", "end", "then", "elif", "elseif", "local", "repeat", "until", "goto",
            "include", "require", "override", "virtual", "abstract", "sealed", "readonly", "volatile", "operator", "implicit", "explicit",
            "out", "ref", "params", "get", "set", "where", "select", "foreach", "impl", "trait", "mod", "pub", "use", "crate", "match",
            "loop", "mut", "type", "export", "extends", "implements", "final", "native", "synchronized", "transient", "assert", "del",
            "raise", "except", "constexpr", "template", "typename", "friend", "inline", "register", "union", "go", "chan", "defer",
            "range", "fallthrough", "object", "when", "fun", "sub", "dim", "byval", "byref", "rem", "uint", "ulong", "ushort", "sbyte",
            "decimal", "dynamic", "event", "delegate", "fixed", "checked", "unchecked", "unsafe", "stackalloc", "lock", "base", "record",
            "init", "required", "nameof", "u8", "u16", "u32", "u64", "i8", "i16", "i32", "i64", "f32", "f64", "usize", "isize", "str"
        ];

        private static readonly string[] MultiCharOperators = ["===", "!==", "<<=", ">>=", "**=", "==", "!=", "<=", ">=", "&&", "||", "++", "--", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "->", "=>", "<<", ">>", "::", "**", "??", "?.", "..", ":="];

        private static readonly Dictionary<string, string> OperatorSwaps = new()
        {
            ["=="] = "!=", ["!="] = "==", ["==="] = "!==", ["!=="] = "===",
            ["<"] = ">", [">"] = "<", ["<="] = ">=", [">="] = "<=",
            ["&&"] = "||", ["||"] = "&&", ["+"] = "-", ["-"] = "+", ["*"] = "/", ["/"] = "*",
            ["++"] = "--", ["--"] = "++", ["+="] = "-=", ["-="] = "+=", ["*="] = "/=", ["/="] = "*="
        };

        private static bool IsIdentStart(char c) => char.IsLetter(c) || c == '_' || c == '$';
        private static bool IsIdentPart(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '$';

        /// <summary>
        /// Tracks the byte offset and line number while walking through the text.
        /// </summary>
        private sealed class Cursor(SourceText source)
        {
            private readonly string _text = source.Text;
            private readonly int _bytesPerUnit = source.Encoding is UnicodeEncoding ? 2 : source.Encoding is UTF32Encoding ? 4 : 0; //0 = UTF-8 rules
            private readonly bool _singleByte = source.Encoding.IsSingleByte;
            public int Index;
            public long ByteOffset = source.Preamble.Length;
            public int Line = 1;

            public void MoveTo(int target)
            {
                for (; Index < target; Index++)
                {
                    char c = _text[Index];
                    if (c == '\n') Line++;
                    if (_singleByte) ByteOffset += 1;
                    else if (_bytesPerUnit == 2) ByteOffset += 2;
                    else if (_bytesPerUnit == 4) ByteOffset += char.IsLowSurrogate(c) ? 0 : 4;
                    else if (c < 0x80) ByteOffset += 1;
                    else if (c < 0x800) ByteOffset += 2;
                    else if (char.IsHighSurrogate(c)) ByteOffset += 4;
                    else if (char.IsLowSurrogate(c)) ByteOffset += 0;
                    else ByteOffset += 3;
                }
            }
        }

        public static List<Token> Tokenize(SourceText source, CommentStyle style, CancellationToken token)
        {
            string text = source.Text;
            var tokens = new List<Token>();
            var cursor = new Cursor(source);
            int i = 0;
            int n = text.Length;
            bool singleQuoteStrings = style is CommentStyle.HASH or CommentStyle.DASHDASH or CommentStyle.SEMICOLON or CommentStyle.XML or CommentStyle.NONE;
            bool lineStart = true;

            void Add(int start, int length, TokenKind kind, int quoteLength = 0, int closeLength = 0)
            {
                cursor.MoveTo(start);
                tokens.Add(new Token { Start = start, Length = length, Kind = kind, ByteOffset = cursor.ByteOffset, Line = cursor.Line, QuoteLength = quoteLength, CloseLength = closeLength });
            }

            int SkipToEndOfLine(int from)
            {
                int end = text.IndexOf('\n', from);
                return end < 0 ? n : end;
            }

            int SkipPast(int from, string terminator)
            {
                int end = text.IndexOf(terminator, from, StringComparison.Ordinal);
                return end < 0 ? n : end + terminator.Length;
            }

            while (i < n)
            {
                if ((i & 0xFFFF) == 0) token.ThrowIfCancellationRequested();
                char c = text[i];

                if (c == '\n') { lineStart = true; i++; continue; }
                if (char.IsWhiteSpace(c)) { i++; continue; }

                bool wasLineStart = lineStart;
                lineStart = false;

                //Comments
                switch (style)
                {
                    case CommentStyle.CLIKE:
                        if (c == '/' && i + 1 < n && text[i + 1] == '/') { i = SkipToEndOfLine(i); continue; }
                        if (c == '/' && i + 1 < n && text[i + 1] == '*') { i = SkipPast(i + 2, "*/"); continue; }
                        break;
                    case CommentStyle.HASH:
                        if (c == '#') { i = SkipToEndOfLine(i); continue; }
                        break;
                    case CommentStyle.DASHDASH:
                        if (c == '-' && i + 1 < n && text[i + 1] == '-')
                        {
                            if (i + 3 < n && text[i + 2] == '[' && text[i + 3] == '[') i = SkipPast(i + 4, "]]");
                            else i = SkipToEndOfLine(i);
                            continue;
                        }
                        break;
                    case CommentStyle.SEMICOLON:
                        if (c == ';') { i = SkipToEndOfLine(i); continue; }
                        break;
                    case CommentStyle.XML:
                        if (c == '<' && string.CompareOrdinal(text, i, "<!--", 0, 4) == 0) { i = SkipPast(i + 4, "-->"); continue; }
                        break;
                    case CommentStyle.BASIC:
                        if (c == '\'') { i = SkipToEndOfLine(i); continue; }
                        if (wasLineStart && i + 3 < n && string.Compare(text, i, "REM", 0, 3, StringComparison.OrdinalIgnoreCase) == 0 && char.IsWhiteSpace(text[i + 3])) { i = SkipToEndOfLine(i); continue; }
                        break;
                }

                //Strings
                if (c == '"' || c == '`' && style == CommentStyle.CLIKE || c == '\'' && style != CommentStyle.BASIC)
                {
                    int end = ScanString(text, i, c, style, singleQuoteStrings, out int quoteLength, out int closeLength, prefixLength: 0, verbatim: false);
                    if (end > i)
                    {
                        Add(i, end - i, TokenKind.String, quoteLength, closeLength);
                        i = end;
                        continue;
                    }
                }
                if (c == '@' && style == CommentStyle.CLIKE && i + 1 < n && text[i + 1] == '"') //C# verbatim string
                {
                    int end = ScanString(text, i + 1, '"', style, true, out int quoteLength, out int closeLength, prefixLength: 1, verbatim: true);
                    if (end > i)
                    {
                        Add(i, end - i, TokenKind.String, quoteLength, closeLength);
                        i = end;
                        continue;
                    }
                }

                //Numbers (including a unary minus/plus when the previous token isn't a value)
                bool signed = (c == '-' || c == '+') && i + 1 < n && (char.IsDigit(text[i + 1]) || text[i + 1] == '.' && i + 2 < n && char.IsDigit(text[i + 2])) && IsUnaryContext(tokens, text);
                if (signed || char.IsDigit(c) || c == '.' && i + 1 < n && char.IsDigit(text[i + 1]) && (i == 0 || !IsIdentPart(text[i - 1])))
                {
                    if (!(char.IsDigit(c) && i > 0 && IsIdentPart(text[i - 1])))
                    {
                        int end = ScanNumber(text, signed ? i + 1 : i, out bool isFloat);
                        Add(i, end - i, isFloat ? TokenKind.Float : TokenKind.Integer);
                        i = end;
                        continue;
                    }
                }

                //Words
                if (IsIdentStart(c))
                {
                    int end = i + 1;
                    while (end < n && IsIdentPart(text[end])) end++;
                    //String prefixes like r"", b'', f"", u8"", $""
                    if (end < n && (text[end] == '"' || text[end] == '\'' && singleQuoteStrings) && end - i <= 3 && IsStringPrefix(text.AsSpan(i, end - i)))
                    {
                        bool verbatim = text.AsSpan(i, end - i).Contains('@') || text.AsSpan(i, end - i).ToString().Equals("r", StringComparison.OrdinalIgnoreCase) && style == CommentStyle.HASH;
                        int strEnd = ScanString(text, end, text[end], style, singleQuoteStrings, out int quoteLength, out int closeLength, prefixLength: end - i, verbatim: verbatim);
                        if (strEnd > end)
                        {
                            Add(i, strEnd - i, TokenKind.String, quoteLength, closeLength);
                            i = strEnd;
                            continue;
                        }
                    }
                    string word = text[i..end];
                    TokenKind kind = BoolWords.Contains(word) ? TokenKind.Boolean
                        : NullWords.Contains(word) ? TokenKind.Null
                        : Keywords.Contains(word) || Keywords.Contains(word.ToLowerInvariant()) && style == CommentStyle.BASIC ? TokenKind.Keyword
                        : TokenKind.Identifier;
                    Add(i, end - i, kind);
                    i = end;
                    continue;
                }

                //Operators and punctuation
                if (style != CommentStyle.XML)
                {
                    string? op = null;
                    foreach (var candidate in MultiCharOperators)
                    {
                        if (i + candidate.Length <= n && string.CompareOrdinal(text, i, candidate, 0, candidate.Length) == 0)
                        {
                            op = candidate;
                            break;
                        }
                    }
                    op ??= c.ToString();
                    Add(i, op.Length, OperatorSwaps.ContainsKey(op) ? TokenKind.Operator : TokenKind.Other);
                    i += op.Length;
                    continue;
                }

                Add(i, 1, TokenKind.Other);
                i++;
            }
            return tokens;
        }

        private static bool IsStringPrefix(ReadOnlySpan<char> prefix)
        {
            foreach (char ch in prefix)
            {
                if ("rRbBfFuUlL8$@".IndexOf(ch) < 0) return false;
            }
            return true;
        }

        private static bool IsUnaryContext(List<Token> tokens, string text)
        {
            if (tokens.Count == 0) return true;
            var last = tokens[^1];
            if (last.Kind is TokenKind.Integer or TokenKind.Float or TokenKind.Identifier or TokenKind.String or TokenKind.Boolean or TokenKind.Null)
                return false;
            if (last.Kind == TokenKind.Keyword) return true; //e.g. return -1
            string s = text.Substring(last.Start, last.Length);
            return s is not (")" or "]" or "}" or "++" or "--");
        }

        /// <summary>
        /// Returns the index after the closing quote, or -1 if this isn't a string.
        /// </summary>
        private static int ScanString(string text, int quoteIndex, char quote, CommentStyle style, bool singleQuoteStrings, out int quoteLength, out int closeLength, int prefixLength, bool verbatim)
        {
            int n = text.Length;
            quoteLength = prefixLength + 1;
            closeLength = 1;

            //Triple quoted strings (Python, C# raw strings, Kotlin, Swift)
            if (quote != '`' && quoteIndex + 2 < n && text[quoteIndex + 1] == quote && text[quoteIndex + 2] == quote && (style == CommentStyle.HASH || style == CommentStyle.CLIKE && quote == '"'))
            {
                string triple = new(quote, 3);
                int close = text.IndexOf(triple, quoteIndex + 3, StringComparison.Ordinal);
                if (close < 0) return -1;
                quoteLength = prefixLength + 3;
                closeLength = 3;
                return close + 3;
            }

            bool isCharLiteral = quote == '\'' && !singleQuoteStrings && style == CommentStyle.CLIKE;
            int limit = isCharLiteral ? Math.Min(n, quoteIndex + 14) : n;
            bool multiLine = quote == '`' || verbatim;
            for (int j = quoteIndex + 1; j < limit; j++)
            {
                char ch = text[j];
                if (ch == '\n' && !multiLine) return -1;
                if (verbatim)
                {
                    if (ch == quote)
                    {
                        if (j + 1 < n && text[j + 1] == quote) { j++; continue; } //"" escape
                        return j + 1;
                    }
                    continue;
                }
                if (ch == '\\' && style != CommentStyle.BASIC) { j++; continue; }
                if (ch == quote) return j + 1;
            }
            return -1;
        }

        private static int ScanNumber(string text, int i, out bool isFloat)
        {
            int n = text.Length;
            isFloat = false;
            int j = i;
            if (j + 1 < n && text[j] == '0' && (text[j + 1] == 'x' || text[j + 1] == 'X'))
            {
                j += 2;
                while (j < n && (Uri.IsHexDigit(text[j]) || text[j] == '_')) j++;
            }
            else if (j + 1 < n && text[j] == '0' && (text[j + 1] == 'b' || text[j + 1] == 'B') && j + 2 < n && (text[j + 2] == '0' || text[j + 2] == '1'))
            {
                j += 2;
                while (j < n && (text[j] == '0' || text[j] == '1' || text[j] == '_')) j++;
            }
            else
            {
                while (j < n && (char.IsDigit(text[j]) || text[j] == '_')) j++;
                if (j + 1 < n && text[j] == '.' && char.IsDigit(text[j + 1]))
                {
                    isFloat = true;
                    j++;
                    while (j < n && (char.IsDigit(text[j]) || text[j] == '_')) j++;
                }
                if (j < n && (text[j] == 'e' || text[j] == 'E'))
                {
                    int k = j + 1;
                    if (k < n && (text[k] == '+' || text[k] == '-')) k++;
                    if (k < n && char.IsDigit(text[k]))
                    {
                        isFloat = true;
                        j = k;
                        while (j < n && char.IsDigit(text[j])) j++;
                    }
                }
            }
            //Type suffixes (f, d, m, L, u, ul, i32, n...)
            while (j < n && IsIdentPart(text[j])) j++;
            return j;
        }

        /// <summary>
        /// Finds the name of the variable/property a value belongs to, e.g. isEnabled in "isEnabled = true" or "\"speed\": 10".
        /// </summary>
        public static string FindName(string text, List<Token> tokens, int index)
        {
            int line = tokens[index].Line;
            for (int k = index - 1, steps = 0; k >= 0 && steps < 4; k--, steps++)
            {
                var t = tokens[k];
                if (t.Line != line) break;
                string s = text.Substring(t.Start, t.Length);
                if (t.Kind == TokenKind.Identifier) return s;
                if (t.Kind == TokenKind.String && t.Length > t.QuoteLength + t.CloseLength)
                    return text.Substring(t.Start + t.QuoteLength, t.Length - t.QuoteLength - t.CloseLength);
                if (s is "=" or ":" or ":=" or "=>" or "==" or "!=" or "<" or ">" or "<=" or ">=" or "+=" or "-=" or "(" or "[") continue;
                break;
            }
            return "";
        }

        #endregion

        #region Corruption

        private sealed class Pools
        {
            public readonly Dictionary<char, List<string>> StringsByQuote = [];
            public readonly List<string> Identifiers = [];

            public Pools(string text, List<Token> tokens)
            {
                var seenIdentifiers = new HashSet<string>();
                foreach (var t in tokens)
                {
                    if (t.Kind == TokenKind.String)
                    {
                        char quote = text[t.Start + t.QuoteLength - 1];
                        if (!StringsByQuote.TryGetValue(quote, out var list)) StringsByQuote[quote] = list = [];
                        if (list.Count < 100000) list.Add(text.Substring(t.Start + t.QuoteLength, t.Length - t.QuoteLength - t.CloseLength));
                    }
                    else if (t.Kind == TokenKind.Identifier && Identifiers.Count < 100000)
                    {
                        string id = text.Substring(t.Start, t.Length);
                        if (seenIdentifiers.Add(id)) Identifiers.Add(id);
                    }
                }
            }
        }

        private static string CorruptToken(Token t, string old, Settings s, Random rnd, Pools pools)
        {
            switch (t.Kind)
            {
                case TokenKind.Boolean:
                    return old switch
                    {
                        "true" => "false", "false" => "true",
                        "True" => "False", "False" => "True",
                        "TRUE" => "FALSE", "FALSE" => "TRUE",
                        _ => old
                    };
                case TokenKind.Null:
                    return rnd.Next(3) switch { 0 => "0", 1 => "1", _ => "\"\"" };
                case TokenKind.Integer:
                    return CorruptInteger(old, s, rnd);
                case TokenKind.Float:
                    return CorruptFloat(old, s, rnd);
                case TokenKind.String:
                    return CorruptString(t, old, s, rnd, pools);
                case TokenKind.Operator:
                    return OperatorSwaps.TryGetValue(old, out var swapped) ? swapped : old;
                case TokenKind.Identifier:
                    if (pools.Identifiers.Count < 2) return old;
                    for (int attempt = 0; attempt < 8; attempt++)
                    {
                        string other = pools.Identifiers[rnd.Next(pools.Identifiers.Count)];
                        if (other != old) return other;
                    }
                    return old;
                default:
                    return old;
            }
        }

        private static (string Sign, string Prefix, string Digits, string Suffix) SplitNumber(string literal)
        {
            string sign = "";
            int i = 0;
            if (literal.Length > 0 && (literal[0] == '-' || literal[0] == '+')) { sign = literal[..1]; i = 1; }
            string prefix = "";
            if (literal.Length >= i + 2 && literal[i] == '0' && "xXbB".Contains(literal[i + 1])) { prefix = literal.Substring(i, 2); i += 2; }
            int end = i;
            bool hex = prefix is "0x" or "0X";
            while (end < literal.Length && (char.IsDigit(literal[end]) || literal[end] == '_' || literal[end] == '.' || hex && Uri.IsHexDigit(literal[end])
                   || !hex && (literal[end] == 'e' || literal[end] == 'E') && end + 1 < literal.Length && (char.IsDigit(literal[end + 1]) || literal[end + 1] is '+' or '-')
                   || !hex && end > i && (literal[end] is '+' or '-') && (literal[end - 1] is 'e' or 'E')))
                end++;
            return (sign, prefix, literal[i..end], literal[end..]);
        }

        private static NumberMode PickNumberMode(Settings s, Random rnd)
        {
            if (s.Numbers != NumberMode.RANDOM) return s.Numbers;
            return (NumberMode)rnd.Next(1, 6);
        }

        private static readonly long[] MagicIntegers = [0, 1, -1, 2, 7, 42, 69, 255, 256, 1000, 9999, 65535, 2147483647];

        private static string CorruptInteger(string literal, Settings s, Random rnd)
        {
            var (sign, prefix, digits, suffix) = SplitNumber(literal);
            string clean = digits.Replace("_", "");
            int radix = prefix is "0x" or "0X" ? 16 : prefix is "0b" or "0B" ? 2 : 10;
            if (clean.Length == 0) return literal;
            BigInteger value = ParseBig(clean, radix);
            if (sign == "-") value = -value;
            bool unsignedSuffix = suffix.Contains('u', StringComparison.OrdinalIgnoreCase);

            var mode = PickNumberMode(s, rnd);
            double strength = Math.Max(1, s.NumberStrength) / 100.0;
            BigInteger result;
            switch (mode)
            {
                case NumberMode.NUDGE:
                    long maxDelta = (long)Math.Max(1, Math.Min((double)BigInteger.Abs(value) * strength, long.MaxValue / 2));
                    long delta = rnd.NextInt64(1, maxDelta + 1);
                    result = rnd.Next(2) == 0 ? value + delta : value - delta;
                    break;
                case NumberMode.SCALE:
                    double factor = 1 + strength * (0.5 + rnd.NextDouble());
                    double scaled = rnd.Next(2) == 0 ? (double)value * factor : (double)value / factor;
                    result = new BigInteger(Math.Round(Math.Clamp(scaled, -1e300, 1e300)));
                    if (result == value) result += rnd.Next(2) == 0 ? 1 : -1;
                    break;
                case NumberMode.SIGNFLIP:
                    result = value == 0 ? 1 : -value;
                    break;
                case NumberMode.MAGIC:
                    result = MagicIntegers[rnd.Next(MagicIntegers.Length)];
                    if (result == value) result = MagicIntegers[(Array.IndexOf(MagicIntegers, (long)result) + 1) % MagicIntegers.Length];
                    break;
                case NumberMode.DIGITS:
                default:
                    return sign + prefix + ScrambleDigit(digits, radix, rnd) + suffix;
            }
            if (unsignedSuffix && result < 0) result = BigInteger.Abs(result);
            if (radix == 10 && digits.Length > 1 && digits[0] == '0') radix = 10; //Leading zeros are dropped
            string body = FormatBig(BigInteger.Abs(result), radix, prefix.Length > 1 && char.IsUpper(digits.FirstOrDefault(char.IsLetter, 'a')));
            return (result < 0 ? "-" : sign == "+" ? "+" : "") + prefix + body + suffix;
        }

        private static BigInteger ParseBig(string digits, int radix)
        {
            BigInteger value = 0;
            foreach (char ch in digits)
            {
                int d = ch <= '9' ? ch - '0' : char.ToLowerInvariant(ch) - 'a' + 10;
                value = value * radix + d;
            }
            return value;
        }

        private static string FormatBig(BigInteger value, int radix, bool upperHex)
        {
            if (radix == 10) return value.ToString(CultureInfo.InvariantCulture);
            if (value.IsZero) return "0";
            var sb = new StringBuilder();
            string alphabet = upperHex ? "0123456789ABCDEF" : "0123456789abcdef";
            while (value > 0)
            {
                sb.Insert(0, alphabet[(int)(value % radix)]);
                value /= radix;
            }
            return sb.ToString();
        }

        private static string ScrambleDigit(string digits, int radix, Random rnd)
        {
            var chars = digits.ToCharArray();
            var positions = Enumerable.Range(0, chars.Length).Where(p => chars[p] != '_' && chars[p] != '.' && (radix == 16 ? Uri.IsHexDigit(chars[p]) : char.IsDigit(chars[p]))).ToArray();
            if (positions.Length == 0) return digits;
            int pos = positions[rnd.Next(positions.Length)];
            string alphabet = radix == 16 ? (char.IsUpper(chars[pos]) ? "0123456789ABCDEF" : "0123456789abcdef") : radix == 2 ? "01" : "0123456789";
            char replacement;
            do { replacement = alphabet[rnd.Next(alphabet.Length)]; } while (replacement == chars[pos] && alphabet.Length > 1);
            chars[pos] = replacement;
            return new string(chars);
        }

        private static readonly double[] MagicFloats = [0.0, 1.0, -1.0, 0.5, 2.0, 4.0, 100.0, 0.001, 1e9];

        private static string CorruptFloat(string literal, Settings s, Random rnd)
        {
            var (sign, _, digits, suffix) = SplitNumber(literal);
            string clean = digits.Replace("_", "");
            if (!double.TryParse(clean, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)) return literal;
            if (sign == "-") value = -value;

            var mode = PickNumberMode(s, rnd);
            double strength = Math.Max(1, s.NumberStrength) / 100.0;
            double result;
            switch (mode)
            {
                case NumberMode.NUDGE:
                    result = value == 0 ? strength * (rnd.NextDouble() * 2 - 1) : value * (1 + strength * (rnd.NextDouble() * 2 - 1));
                    break;
                case NumberMode.SCALE:
                    double factor = 1 + strength * (0.5 + rnd.NextDouble());
                    result = rnd.Next(2) == 0 ? value * factor : value / factor;
                    break;
                case NumberMode.SIGNFLIP:
                    result = value == 0 ? 1.0 : -value;
                    break;
                case NumberMode.MAGIC:
                    //pi = 4: round to the next/previous whole number half of the time, otherwise a "magic" value
                    if (rnd.Next(2) == 0 && value != Math.Floor(value))
                        result = rnd.Next(2) == 0 ? Math.Ceiling(value) : Math.Floor(value);
                    else
                        result = MagicFloats[rnd.Next(MagicFloats.Length)];
                    break;
                case NumberMode.DIGITS:
                default:
                    return sign + ScrambleDigit(digits, 10, rnd) + suffix;
            }
            if (!double.IsFinite(result)) result = value;
            return FormatFloat(result, digits, suffix, sign == "+");
        }

        private static string FormatFloat(double value, string originalDigits, string suffix, bool explicitPlus)
        {
            bool exponent = originalDigits.Contains('e') || originalDigits.Contains('E');
            string body;
            if (exponent)
            {
                body = value.ToString("0.0###e0", CultureInfo.InvariantCulture);
            }
            else
            {
                int dot = originalDigits.IndexOf('.');
                int decimals = dot < 0 ? 1 : Math.Max(1, originalDigits.Length - dot - 1);
                decimals = Math.Min(decimals, 15);
                body = Math.Abs(value) >= 1e15 ? value.ToString("0.0###e0", CultureInfo.InvariantCulture) : value.ToString("F" + decimals, CultureInfo.InvariantCulture);
                if (originalDigits.StartsWith('.') && body.StartsWith("0.")) body = body[1..];
                else if (originalDigits.StartsWith('.') && body.StartsWith("-0.")) body = "-" + body[2..];
            }
            if (explicitPlus && !body.StartsWith('-')) body = "+" + body;
            return body + suffix;
        }

        private static List<string> SplitUnits(string content)
        {
            //Escape sequences stay together so shuffling can't break the closing quote
            var units = new List<string>();
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '\\' && i + 1 < content.Length) { units.Add(content.Substring(i, 2)); i++; }
                else if (char.IsHighSurrogate(content[i]) && i + 1 < content.Length) { units.Add(content.Substring(i, 2)); i++; }
                else units.Add(content[i].ToString());
            }
            return units;
        }

        private const string Letters = "abcdefghijklmnopqrstuvwxyz";

        private static string CorruptString(Token t, string old, Settings s, Random rnd, Pools pools)
        {
            string open = old[..t.QuoteLength];
            string close = old[^t.CloseLength..];
            string content = old.Substring(t.QuoteLength, old.Length - t.QuoteLength - t.CloseLength);
            char quote = open[^1];
            var units = SplitUnits(content);

            StringMode mode = s.StringCorruption;
            if (mode == StringMode.RANDOM)
                mode = (StringMode)rnd.Next(1, 7);
            if (units.Count < 2 && mode is StringMode.SHUFFLE or StringMode.REVERSE or StringMode.TRUNCATE)
                mode = StringMode.SUBSTITUTE;
            if (quote == '\'' && t.QuoteLength == 1 && units.Count == 1)
                mode = StringMode.SUBSTITUTE; //Char literals like 'x' stay one character

            string result;
            switch (mode)
            {
                case StringMode.SHUFFLE:
                    for (int k = units.Count - 1; k > 0; k--)
                    {
                        int j = rnd.Next(k + 1);
                        (units[k], units[j]) = (units[j], units[k]);
                    }
                    result = string.Concat(units);
                    break;
                case StringMode.REVERSE:
                    units.Reverse();
                    result = string.Concat(units);
                    break;
                case StringMode.TRUNCATE:
                    result = string.Concat(units.Take(rnd.Next(0, units.Count)));
                    break;
                case StringMode.SWAP:
                    var sameQuote = pools.StringsByQuote.GetValueOrDefault(quote);
                    if (sameQuote != null && sameQuote.Count > 1)
                    {
                        result = content;
                        for (int attempt = 0; attempt < 8 && result == content; attempt++)
                            result = sameQuote[rnd.Next(sameQuote.Count)];
                        break;
                    }
                    goto case StringMode.SUBSTITUTE;
                case StringMode.REPEAT:
                    if (units.Count == 0) goto case StringMode.SUBSTITUTE;
                    int start = rnd.Next(units.Count);
                    int length = rnd.Next(1, units.Count - start + 1);
                    string segment = string.Concat(units.Skip(start).Take(length));
                    result = string.Concat(units.Take(start + length)) + segment + string.Concat(units.Skip(start + length));
                    break;
                case StringMode.SUBSTITUTE:
                default:
                    if (units.Count == 0)
                    {
                        int count = rnd.Next(1, 6);
                        var sb = new StringBuilder();
                        for (int k = 0; k < count; k++) sb.Append(Letters[rnd.Next(Letters.Length)]);
                        result = sb.ToString();
                        break;
                    }
                    int changes = Math.Max(1, (int)Math.Round(units.Count * Math.Min(1.0, s.NumberStrength / 100.0) * 0.5));
                    for (int k = 0; k < changes; k++)
                    {
                        int p = rnd.Next(units.Count);
                        string u = units[p];
                        if (u.Length != 1 || !char.IsLetterOrDigit(u[0])) continue;
                        char ch = u[0];
                        char replacement = char.IsDigit(ch) ? (char)('0' + rnd.Next(10)) : Letters[rnd.Next(Letters.Length)];
                        units[p] = (char.IsUpper(ch) ? char.ToUpperInvariant(replacement) : replacement).ToString();
                    }
                    result = string.Concat(units);
                    break;
            }
            return open + result + close;
        }

        #endregion
    }
}
