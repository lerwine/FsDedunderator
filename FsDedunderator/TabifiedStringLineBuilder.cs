using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace FsDedunderator
{
    /// <summary>
    /// Contains lines which are indented and normalized;
    /// </summary>
    public class TabifiedStringLineBuilder
    {
        /// <summary>
        /// Default number of whitespace characters that equal one tab character.
        /// </summary>
        public const int DefaultTabIndent = 4;

        /// <summary>
        /// Default regular expression for matching one or more whitespace characters.
        /// </summary>
        public static readonly Regex AnyWhitespacePattern = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Default regular expression for matching any normal newline character.
        /// </summary>
        public static readonly Regex GeneralNewlinePattern = new Regex(@"[\r\n]", RegexOptions.Compiled);

        /// <summary>
        /// Default regular expression for matching a windows newline character sequence.
        /// </summary>
        public static readonly Regex WindowsNewlinePattern = new Regex(@"\r\n", RegexOptions.Compiled);

        /// <summary>
        /// Default regular expression for matching a unix newline character.
        /// </summary>
        public static readonly Regex UnixNewlinePattern = new Regex(@"\n", RegexOptions.Compiled);

        /// <summary>
        /// Default regular expression for matching a macintosh newline character.
        /// </summary>
        public static readonly Regex MacNewlinePattern = new Regex(@"\r", RegexOptions.Compiled);

        /// <summary>
        /// Default regular express for matching line break character sequences.
        /// </summary>
        public static readonly Regex DefaultNewLinePattern;
        

        /// <summary>
        /// Default tab character.
        /// </summary>
        public static readonly char DefaultTabChar;
        
        /// <summary>
        /// Default regular express for matching one or more whitespace characters, not including line breaks or tabs.
        /// </summary>
        public static readonly Regex DefaultWhitespacePattern;
        
        private char _tabChar;
        public int _tabIndent;
        public Regex _whitespacePattern;

        /// <summary>
        /// Default line break character sequence.
        /// </summary>
        public string NewlineSequence { get; private set; }

        /// <summary>
        /// Regular expression for matching line break character sequences.
        /// </summary>
        public Regex NewLinePattern { get; private set; }

        /// <summary>
        /// Tab character.
        /// </summary>
        public char TabChar
        {
            get { return _tabChar; }
            set
            {
                if (value == _tabChar)
                    return;
                string s = value.ToString();
                if (NewLinePattern.IsMatch(s) || WhitespacePattern.IsMatch(s))
                    throw new ArgumentOutOfRangeException();
                _tabChar = value;
            }
        }
        
        /// <summary>
        /// Number of whitespace characters that equal one tab character.
        /// </summary>
        public int TabIndent
        {
            get { return _tabIndent; }
            set
            {
                if (_tabIndent < 1)
                    throw new ArgumentOutOfRangeException();
                _tabIndent = value;
            }
        }
        
        /// <summary>
        /// Regular express for matching one or more whitespace characters, not including line breaks or tabs.
        /// </summary>
        public Regex WhitespacePattern
        {
            get { return _whitespacePattern; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                if (value.IsMatch(NewlineSequence) || value.IsMatch(TabChar.ToString()) || GetAllCharValues().Select(c => c.ToString()).Any(s => NewLinePattern.IsMatch(s) && value.IsMatch(s)) || !GetAllCharValues().Select(c => c.ToString()).Any(s => value.IsMatch(s)))
                    throw new ArgumentOutOfRangeException();
                _whitespacePattern = value;
            }
        }

        public string Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        static TabifiedStringLineBuilder()
        {
            string nws = Environment.NewLine;
            if (string.IsNullOrEmpty(nws) || Regex.IsMatch(nws, "^(\r\n?|\n)$"))
            {
                DefaultNewLinePattern = new Regex(@"\r\n?|\n", RegexOptions.Compiled);
                DefaultTabChar = '\t';
                nws = @"\r\n\t";
            }
            else
            {
                IEnumerable<char> cColl = (new char[] { '\t', '\v', '\v' }).Where(c => !nws.Contains(c));
                if (!cColl.Any())
                {
                    IEnumerable<char> candidateChars = GetAllCharValues().Where(c => char.IsWhiteSpace(c) && c != ' ' && !nws.Contains(c));
                    if (!candidateChars.Any())
                        candidateChars = GetAllCharValues().Where(c => char.IsControl(c) && c != '\0' && !nws.Contains(c));
                    cColl = candidateChars.Where(c => c < ' ' && !nws.Contains(c));
                    if (!cColl.Any())
                    {
                        char v = nws[0];
                        cColl = (new char[] { '\t', '\v', '\v' }).Where(c => c != v);
                        if (!cColl.Any())
                            cColl = candidateChars.Where(c => c != ' ' && c != v);
                    }
                }
                DefaultTabChar = cColl.First();
                DefaultTabPattern = new Regex(new string((ToRegexPattern(DefaultTabChar).ToArray())), RegexOptions.Compiled);
                DefaultNewLinePattern = new Regex(new string(ToRegexPattern(nws).ToArray()), RegexOptions.Compiled);
                nws = new string(ToRegexPattern(((IEnumerable<char>)(nws)).Concat(new char[] { DefaultTabChar }).Distinct().ToArray()).ToArray());
            }

            DefaultWhitespacePattern = new Regex(@"(?=[^" + nws + @"])\s", RegexOptions.Compiled);
        }

        /// <summary>
        /// Initializes new instance of <see cref="TabifiedStringLineBuilder" />.
        /// </summary>
        /// <param name="text">Text to initially append.</param>
        /// <param name="newLineSequence">Sequence of characters which represents the default newline sequence.</param>
        /// <param name="newlinePattern">Pattern for matching a single sequence characters that represents a line break.</param>
        /// <param name="whitespacePattern">Matches consecutive whitespace characters.</param>
        /// <param name="tabChar">Tab character which represents indented text.</param>
        /// <param name="tabIndent">Number of spaces each tab stop represents.</param>
        public TabifiedStringLineBuilder(string text, string newLineSequence, Regex newlinePattern, Regex whitespacePattern, char tabChar, int tabIndent = DefaultTabIndent)
        {
            if (newLineSequence == null)
                throw new ArgumentNullException("newLineSequence");
            if (tabIndent < 1)
                throw new ArgumentOutOfRangeException("tabIndent", "Tab indent must be greater than zero");
            if (newLineSequence.Length == 0)
                throw new ArgumentException("New Line Sequence cannot be empty", "newLineSequence");
            _Initialize(newLineSequence, newlinePattern, whitespacePattern, tabChar, tabIndent);
            StringData.Append(this, text, false);
        }

        /// <summary>
        /// Initializes new instance of <see cref="TabifiedStringLineBuilder" />.
        /// </summary>
        /// <param name="text">Text to initially append.</param>
        /// <param name="newLineSequence">Sequence of characters which represents the default newline sequence.</param>
        /// <param name="tabIndent">Number of spaces each tab stop represents.</param>
        public TabifiedStringLineBuilder(string text, string newLineSequence, int tabIndent = DefaultTabIndent)
        {
            if (tabIndent < 1)
                throw new ArgumentOutOfRangeException("tabIndent", "Tab indent must be greater than zero");
            if ((string.IsNullOrEmpty(newLineSequence) && string.IsNullOrEmpty(newLineSequence = Environment.NewLine)) || DefaultNewLinePattern.IsMatch(newLineSequence))
            {
                NewlineSequence = (string.IsNullOrEmpty(newLineSequence)) ? "\n" :newLineSequence;
                NewLinePattern = DefaultNewLinePattern;
                WhitespacePattern = DefaultWhitespacePattern;
                TabChar = DefaultTabChar;
                TabIndent = tabIndent;
            }
            else
                _Initialize(newLineSequence, null, null, (newLineSequence.Contains(DefaultTabChar)) ? GetAllCharValues().First(c => char.IsWhiteSpace(c) && c != ' ' && !newLineSequence.Contains(c)) : DefaultTabChar, tabIndent);
            StringData.Append(this, text, false);
        }

        /// <summary>
        /// Initializes new instance of <see cref="TabifiedStringLineBuilder" />.
        /// </summary>
        /// <param name="text">Text to initially append.</param>
        /// <param name="tabIndent">Number of spaces each tab stop represents.</param>
        public TabifiedStringLineBuilder(string text, int tabIndent = DefaultTabIndent)
        {
            if (tabIndent < 1)
                throw new ArgumentOutOfRangeException("tabIndent", "Tab indent must be greater than zero");
            string newLineSequence = Environment.NewLine;
            NewlineSequence = (string.IsNullOrEmpty(newLineSequence)) ? "\n" :newLineSequence;
            NewLinePattern = DefaultNewLinePattern;
            WhitespacePattern = DefaultWhitespacePattern;
            TabChar = DefaultTabChar;
            TabIndent = tabIndent;
            StringData.Append(this, text, false);
        }

        /// <summary>
        /// Initializes new instance of <see cref="TabifiedStringLineBuilder" />.
        /// </summary>
        /// <param name="tabIndent">Number of spaces each tab stop represents.</param>
        public TabifiedStringLineBuilder(int tabIndent) : this(null, tabIndent) { }

        /// <summary>
        /// Initializes new instance of <see cref="TabifiedStringLineBuilder" />.
        /// </summary>
        public TabifiedStringLineBuilder() : this(null, DefaultTabIndent) { }

        /// <summary>
        /// Initializes sequences and patterns.
        /// </summary>
        /// <param name="newLineSequence">Sequence of characters which represents the default newline sequence.</param>
        /// <param name="newlinePattern">Pattern for matching a single sequence characters that represents a line break.</param>
        /// <param name="whitespacePattern">Matches consecutive whitespace characters.</param>
        /// <param name="tabChar">Tab character which represents indented text.</param>
        public void Initialize(string newLineSequence, Regex newlinePattern, Regex whitespacePattern, char tabChar)
        {
            if (newLineSequence == null)
                throw new ArgumentNullException("newLineSequence");
            if (newLineSequence.Length == 0)
                throw new ArgumentException("New Line Sequence cannot be empty", "newLineSequence");
            _Initialize(newLineSequence, newlinePattern, whitespacePattern, tabChar, TabIndent);
        }

        private void _Initialize(string newLineSequence, Regex newlinePattern, Regex whitespacePattern, char tabChar, int tabIndent = DefaultTabIndent)
        {
            if (newlinePattern == null)
            {
                if (DefaultNewLinePattern.IsMatch(newLineSequence) && whitespacePattern == null && tabChar == DefaultTabChar)
                {
                    NewlineSequence = newLineSequence;
                    TabChar = tabChar;
                    TabIndent = tabIndent;
                    NewLinePattern = DefaultNewLinePattern;
                    WhitespacePattern = DefaultWhitespacePattern;
                    return;
                }
                newlinePattern = (DefaultNewLinePattern.IsMatch(newLineSequence)) ? DefaultNewLinePattern : new Regex(new string(ToRegexPattern(newLineSequence).ToArray()), RegexOptions.Compiled);
            }
            else
            {
                if (!newlinePattern.IsMatch(newLineSequence))
                    throw new ArgumentOutOfRangeException("newLineSequence", "New Line Sequence does not match pattern");
            }
            if (newlinePattern.IsMatch(tabChar.ToString()))
                throw new ArgumentOutOfRangeException("tabChar", "Tab character cannot match newline pattern");
            
            var nlMatchItems = GetAllCharValues().Select(c =>
            {
                string s = c.ToString();
                return new { Value = c, IsMatch = newlinePattern.IsMatch(s) };
            });

            if (!nlMatchItems.Any(a => a.IsMatch && a.Value == tabChar))
                throw new ArgumentOutOfRangeException("newlinePattern", "New Line pattern cannot match all non-tab characters");

            if (whitespacePattern == null)
            {
                var nlChars = nlMatchItems.Where(a => (a.Value == tabChar || a.IsMatch) && !AnyWhitespacePattern.IsMatch(a.Value.ToString()));
                WhitespacePattern = (nlChars.Any()) ?
                    new Regex("((?=[^" + (new string(ToRegexPattern(nlChars.Select(a => a.Value)).ToArray())) + @"])\s)+", RegexOptions.Compiled) :
                    AnyWhitespacePattern;
                NewlineSequence = newLineSequence;
                TabChar = tabChar;
                TabIndent = tabIndent;
                NewLinePattern = newlinePattern;
                return;
            }

            if (whitespacePattern.IsMatch(tabChar.ToString()))
                throw new ArgumentOutOfRangeException("whitespacePattern", "Whitespace pattern cannot match tab character");
            if (whitespacePattern.IsMatch(newLineSequence))
                throw new ArgumentOutOfRangeException("whitespacePattern", "Whitespace pattern cannot match newline sequence");
            var wsMatchItems = nlMatchItems.Select(a => new
            {
                Value = a.Value,
                NlMatch = a.IsMatch,
                WsMatch = whitespacePattern.IsMatch(a.Value.ToString())
            });
            if (wsMatchItems.Any(a => a.NlMatch && a.WsMatch))
                throw new ArgumentOutOfRangeException("whitespacePattern", "Whitespace pattern cannot match same characters as newline pattern");
            if (!wsMatchItems.Any(a => a.WsMatch))
                throw new ArgumentOutOfRangeException("whitespacePattern", "Whitespace does not match any characters");
            char ws = (tabChar == ' ' || newlinePattern.IsMatch(" ") || !whitespacePattern.IsMatch(" ")) ? wsMatchItems.First(a => a.WsMatch).Value : ' ';
            for (int i = 1; i <= tabIndent; i++)
            {
                string s = new string(ws, i) + newLineSequence;
                Match match = whitespacePattern.Match(s);
                if (!match.Success || match.Index > 0 || match.Length != i)
                    throw new ArgumentOutOfRangeException("whitespacePattern", "Whitespace pattern cannot match " + i.ToString() + " whitespace characters.");
            }
            WhitespacePattern = whitespacePattern;
            NewlineSequence = newLineSequence;
            TabChar = tabChar;
            TabIndent = tabIndent;
            NewLinePattern = newlinePattern;
        }

        /// <summary>
        /// Sets newline sequence and regular expression pattern.
        /// </summary>
        /// <param name="newLineSequence">Sequence of characters which represents the default newline sequence.</param>
        /// <param name="newlinePattern">Pattern for matching a single sequence characters that represents a line break.</param>
        public void SetNewLine(string newLineSequence, Regex newlinePattern)
        {
            if (newLineSequence == null)
                throw new ArgumentNullException("newLineSequence");
            if (newLineSequence.Length == 0)
                throw new ArgumentException("New Line Sequence cannot be empty", "newLineSequence");
            if (WhitespacePattern.IsMatch(newLineSequence))
                throw new ArgumentOutOfRangeException("newLineSequence", "Newline sequence cannot match the whitespace pattern.");
            if (newlinePattern == null)
            {
                newlinePattern = new Regex(new string(ToRegexPattern(newLineSequence).ToArray()), RegexOptions.Compiled);
                if (newlinePattern.IsMatch(TabChar.ToString()))
                    throw new ArgumentOutOfRangeException("newLineSequence", "Newline sequence cannot start with the tab character");
            }
            else
            {
                if (!newlinePattern.IsMatch(newLineSequence))
                    throw new ArgumentOutOfRangeException("newlinePattern", "Newline pattern must match the newline sequence.");
                if (newlinePattern.IsMatch(TabChar.ToString()))
                    throw new ArgumentOutOfRangeException("newlinePattern", "Newline pattern cannot match the tab character");
            }
            if (GetAllCharValues().Select(c => c.ToString()).Any(s => newlinePattern.IsMatch(s) && WhitespacePattern.IsMatch(s)))
                throw new ArgumentOutOfRangeException("newlinePattern", "Newline pattern cannot match the same characters as whitespace matches.");
            NewlineSequence = newLineSequence;
            NewLinePattern = newlinePattern;
        }

        /// <summary>
        /// Sets newline sequence.
        /// </summary>
        /// <param name="newLineSequence">Sequence of characters which represents the default newline sequence.</param>
        public void SetNewLine(string newLineSequence) { SetNewLine(newLineSequence, null); }

        public TabifiedStringLineBuilder Append(string text)
        {
            Monitor.Enter(_syncRoot);
            try { StringData.Append(this, text, false); }
            finally { Monitor.Exit(_syncRoot); }
            
            return this;
        }

        public TabifiedStringLineBuilder AppendLine(string text)
        {
            Monitor.Enter(_syncRoot);
            try { StringData.Append(this, text, true); }
            finally { Monitor.Exit(_syncRoot); }
            
            return this;
        }

        public TabifiedStringLineBuilder AppendLine()
        {
            Monitor.Enter(_syncRoot);
            try {  StringData.Append(this, "", true); }
            finally { Monitor.Exit(_syncRoot); }

            return this;
        }

        public TabifiedStringLineBuilder Clear()
        {
            Monitor.Enter(_syncRoot);
            try { _firstNode = _lastNode = null; }
            finally { Monitor.Exit(_syncRoot); }

            return this;
        }

        private static IEnumerable<char> GetAllCharValues()
        {
            int e = (int)(char.MaxValue) + 1;
            for (int c = (int)(char.MinValue); c < e; c++)
                yield return (char)c;
        }
        
        private static IEnumerable<char> ToRegexPattern(params char[] values) { return ToRegexPattern((IEnumerable<char>)values); }
        
        private static IEnumerable<char> ToRegexPattern(IEnumerable<char> values)
        {
            if (values == null)
                yield break;
            foreach (char c in values)
            {
                switch (c)
                {
                    case '\\':
                    case '*':
                    case '+':
                    case '?':
                    case '|':
                    case '{':
                    case '[':
                    case '(':
                    case ')':
                    case '^':
                    case '$':
                    case '.':
                    case '#':
                        yield return '\\';
                        yield return c;
                        break;
                    case '\n':
                        yield return '\\';
                        yield return 'n';
                        break;
                    case '\r':
                        yield return '\\';
                        yield return 'n';
                        break;
                    case '\t':
                        yield return '\\';
                        yield return 'n';
                        break;
                    case ' ':
                        yield return ' ';
                        break;
                    default:
                        if (char.IsControl(c) || char.IsWhiteSpace(c))
                        {
                            yield return '\\';
                            yield return 'x';
                            foreach (char v in ((int)c).ToString("x4"))
                                 yield return v;
                        }
                        else
                            yield return c;
                        break;
                }
            }
        }
        
        private object _syncRoot = new object();
        private StringData _firstNode = null;
        private StringData _lastNode = null;
        interface IStringData
        {
            TabifiedStringLineBuilder Builder { get; }
            IndentedStringData Parent { get; }
        }
        abstract class StringData
        {
            internal abstract TabifiedStringLineBuilder Builder { get; }
            protected internal IndentedStringData Parent { get; }
            protected StringData(IndentedStringData parent)
            {
                Parent = parent;
            }

            internal static void Append(TabifiedStringLineBuilder builder, string text, bool appendNewLine)
            {
                if (string.IsNullOrEmpty(text))
                {
                    if (!appendNewLine)
                        return;
                    if (builder._firstNode == null)
                        builder._firstNode = builder._lastNode = new NonIndentedStringData(new string[] { "" });
                    else
                        builder._lastNode.LinesAppendLines(new string[] { "" });
                }
            }
        }
        class IndentedStringData : StringData
        {
            private TabifiedStringLineBuilder _builder;
            private List<IStringData> _lines = new List<IStringData>();

            internal override TabifiedStringLineBuilder Builder => (_builder == null) ? Parent.Builder : _builder;
            protected internal List<IStringData> Lines { get { return _lines; } }

            protected IndentedStringData(IndentedStringData parent)
                : base(parent)
            {
                _builder = null;
            }

            internal IndentedStringData(TabifiedStringLineBuilder builder)
                : base(null)
            {
                _builder = builder;
            }

        }

        class NestedIndentedStringData : IndentedStringData, IStringData
        {
            internal NestedIndentedStringData(IndentedStringData parent, params string[] lines) : base(parent) { Initialize(lines); }

            internal NestedIndentedStringData(TabifiedStringLineBuilder builder, params string[] lines) : base(builder) { Initialize(lines); }

            private void Initialize(IEnumerable<string> lines)
            {
                foreach (string s in lines.SelectMany(Builder.NewLinePattern))
            }
        }

        class NonIndentedStringData : StringData, IStringData
        {
            private readonly List<string> _lines;
            internal override TabifiedStringLineBuilder Builder => Parent.Builder;
            internal NonIndentedStringData(IndentedStringData parent, params string[] lines) : base(parent) => _lines = lines.ToList();
        }

        sealed class IndentStringParseEnumerator : IEnumerator<string>
        {
            private readonly TabifiedStringLineBuilder _builder;
            private object _syncRoot = new object();

            internal string Source { get; private set; }
            internal int StartIndex { get; private set; }
            internal int EndofIndent { get; private set; }
            internal int EndofLine { get; private set; }
            internal int IndentCount { get; private set; }
            internal string Current { get; private set; }
            string IEnumerator<string>.Current => Current;

            object IEnumerator.Current => throw new NotImplementedException();

            internal IndentStringParseEnumerator(TabifiedStringLineBuilder builder, string text)
            {
                _builder = builder;
            }
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public bool MoveNext()
            {
                Monitor.Enter(_syncRoot);
                try
                {
                    if (Current != null && (StartIndex == Source.Length || (StartIndex = EndofLine + 1) == Source.Length))
                        return false;
                    EndofIndent = StartIndex;
                    IndentCount = 0;
                    while (StartIndex < Source.Length)
                    {
                        Match nl = _builder.NewLinePattern.Match(Source, EndofIndent);
                        Match ws = _builder.WhitespacePattern.Match(Source, EndofIndent);
                        if (nl.Success && (!ws.Success || ws.Index > nl.Index))
                        {
                            EndofLine = nl.Index;
                            break;
                        }
                        int e = EndofIndent;
                        if (ws.Success && ws.Index == 0)
                        {
                            e = ws.Index + ws.Length;
                            while ((e - EndofIndent) >= _builder.TabIndent)
                            {
                                EndofIndent += _builder.TabIndent;
                                IndentCount++;
                            }
                            for (int i = EndofIndent; i < e; i++)
                            {
                                if (Source[i] == _builder.TabChar)
                                {
                                    e = i;
                                    break;
                                }
                            }
                        }
                        if (Source[e] == _builder.TabChar)
                        {
                            IndentCount++;
                            EndofIndent = e + 1;
                        }
                        else if (!ws.Success || ws.Index > 0)
                        {
                            EndofLine = Source.Length;
                            break;
                        }
                    }

                }
                finally { Monitor.Exit(_syncRoot); }
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

    }
}