using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace FsDedunderator
{
	public class TabifiedStringLineBuilder
	{
		class IndentLineEnumerator : IEnumerator<string>
		{
			private object _syncRoot = new object();
			private TabifiedStringLineBuilder _builder;
			private Match _nlMatch = null;
			private Match _wsMatch = null;
			
			public string Current
			{
				get
				{
					Monitor.Enter(_syncRoot);
					try
					{
						int length = EndOfLine - StartOfText;
						if (length == 0)
							return "";
						return Source.Substring(StartOfText, length);
					}
					finally { Monitor.Exit(_syncRoot); }
				}
			}
			
			internal string Source { get; private set; }
			
			internal int StartOfLine { get; private set; }
			
			internal int StartOfText { get; private set; }
			
			internal int EndOfText { get; private set; }
			
			internal int EndOfLine { get; private set; }
			
			internal int IndentLevel { get; private set; }
			
			internal IndentLineEnumerator(string line, TabifiedStringLineBuilder builder)
			{
				Source = line ?? "";
				_builder = builder;
			}
			
			public bool MoveNext()
			{
				if (StartOfLine == Source.Length)
					return false;
				
				StartOfText = EndOfText = StartOfLine = EndOfLine;
				IndentLevel = 0;
				if (StartOfLine == Source.Length)
					return true;
				int lastTabEnd = StartOfLine;
				while (StartOfText < Source.Length)
				{
					if (_nlMatch == null && !(_nlMatch = _builder.NewLinePattern.Match(Source, StartOfText)).Success)
						_nlMatch = null;
					if (_wsMatch == null && !(_wsMatch = _builder.WhitespacePattern.Match(Source.StartOfText)).Success || _wsMatch.Index != StartOfText)
						_wsMatch = null;
					if (_nlMatch != null && _nlMatch.Index == StartOfText)
					{
						EndOfText = _nlMatch.Index;
						_wsMatch = null;
						EndOfLine = _nlMatch.Index + _nlMatch.Length;
						_nlMatch = null;
						return true;
					}
					if (Source[StartOfText] == _builder.TabChar)
					{
						StartOfText++;
						if (_wsMatch.Index < EndOfText)
							_wsMatch = null;
						IndentLevel++;
						lastTabEnd = EndOfText = StartOfText;
						continue;
					}
					if (_wsMatch == null)
					{
						if (_nlMatch == null)
							EndOfLine = EndOfText = Source.Length;
						else
						{
							EndOfText = _nlMatch.Index;
							if (_wsMatch.Index < EndOfText)
								_wsMatch = null;
							EndOfLine = _nlMatch.Index + _nlMatch.Length;
							_nlMatch = null;
						}
						return true;
					}
					StartOfText += _wsMatch.Length;
					_wsMatch = null;
					int t = (StartOfText - lastTabEnd);
					t = -= (t % _builder.TabIndent);
					IndentLevel += (t / _builder.TabIndent);
					lastTabEnd += t;
				}
				EndOfText = EndOfLine = StartOfText;
				return true;
			}
			
			public void Reset()
			{
				StartOfLine = StartOfText = EndOfLine = EndOfText = IndentLevel = 0;
				_nlMatch = _wsMatch = null;
			}
		}
	}
}
