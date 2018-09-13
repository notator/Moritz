using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System;
using System.Drawing;

using Moritz.Xml;
using Moritz.Globals;
using System.Text;

namespace Moritz.Symbols
{
	/// <summary>
	/// Base class of all DrawObject classes.
	/// </summary>
	public abstract class DrawObject
	{
		public DrawObject()
		{ }

		public DrawObject(object container)
		{
			Container = container;
		}

		public abstract void WriteSVG(SvgWriter w);

		public readonly object Container;

		/// <summary>
		/// Set when this drawObject is inside a Transposable drawObject
		/// </summary>
		public string EnharmonicNote
		{
			get { return _enharmonicNote; }
			set
			{
				// possible Values:
				// "C", "C#", "Db", "D", "D#", "Eb", "E", "E#", "Fb", "F", "F#" "Gb",
				// "G", "G#", "Ab", "A", "A#", "Bb", "B", "B#", "Cb"
				Debug.Assert(Regex.Matches(value, @"^[A-G][#b]?$") != null);
				_enharmonicNote = value;
			}
		}
		/// <summary>
		/// Set when this object is part of the gallery
		/// </summary>
		public string Name = "";
		/// <summary>
		/// Used by AssistantComposer
		/// </summary>
		public Metrics Metrics = null;

		private string _enharmonicNote = "";
	}

	internal class Text : DrawObject
	{
		public Text(object container, string text, string fontName, float fontHeight, TextHorizAlign align)
			: base(container)
		{
			_textInfo = new TextInfo(text, fontName, fontHeight, align);
		}

		public override void WriteSVG(SvgWriter w)
		{
			//w.SvgText(TextInfo, Metrics as TextMetrics); // does not work with DynamicMetrics
			if(Metrics != null)
				Metrics.WriteSVG(w);
			if(_frameInfo != null)
			{
				switch(_frameInfo.FrameType)
				{
					case TextFrameType.none:
						break;
					case TextFrameType.rectangle:
						w.SvgRect("rectangle", Metrics.Left, Metrics.Top, Metrics.Right - Metrics.Left, Metrics.Bottom - Metrics.Top,
							_frameInfo.ColorString.String, _frameInfo.StrokeWidth, "none");
						break;
					case TextFrameType.ellipse:
						w.SvgEllipse("ellipse", Metrics.Left, Metrics.Top, (Metrics.Right - Metrics.Left) / 2, (Metrics.Bottom - Metrics.Top) / 2,
							_frameInfo.ColorString.String, _frameInfo.StrokeWidth, "none");
						break;
					case TextFrameType.circle:
						w.SvgCircle("circle", Metrics.Right - Metrics.Left, Metrics.Bottom - Metrics.Top, ((Metrics.Right - Metrics.Left) / 2),
							_frameInfo.ColorString.String, _frameInfo.StrokeWidth, "none");
						break;
				}
			}
		}

		// attributes
		public TextInfo TextInfo { get { return _textInfo; } }
		private readonly TextInfo _textInfo = null;
		public FrameInfo FrameInfo { get { return _frameInfo; } }
		protected FrameInfo _frameInfo = null;
	}

	internal class StaffNameText : Text
	{
		public StaffNameText(object container, string staffName, float fontHeight)
			: base(container, staffName, "Arial", fontHeight, TextHorizAlign.center)
		{
		}

		public override string ToString()
		{
			return "staffname: " + TextInfo.Text;
		}
	}

	internal class FramedBarNumberText : Text
	{
		public FramedBarNumberText(object container, string text, float gap, float stafflinethickness)
			: base(container, text, "Arial", (gap * 2F), TextHorizAlign.center)
		{
			float paddingX = 22F;
			if(text.Length > 1)
				paddingX = 10F;
			float paddingY = 22F;

			float strokeWidth = stafflinethickness * 1.2F;

			_frameInfo = new FrameInfo(TextFrameType.rectangle, paddingX, paddingY, strokeWidth, new ColorString("000000"));
		}

		public override string ToString()
		{
			return "barnumber: " + TextInfo.Text;
		}
	}

	#region Framed region texts

	internal class TextList : DrawObject
	{
		public TextList(object container, List<string> textStrings, float gap, float stafflinethickness, TextHorizAlign hAlign)
		{
			foreach(string textString in textStrings)
			{
				this.Texts.Add(new Text(container, textString, "Arial", (gap * 2.5F), hAlign));
			}
		}

		public override void WriteSVG(SvgWriter w)
		{
			//w.SvgText(TextInfo, Metrics as TextMetrics); // does not work with DynamicMetrics
			if(Metrics != null)
				Metrics.WriteSVG(w);
			if(_frameInfo != null)
			{
				switch(_frameInfo.FrameType)
				{
					case TextFrameType.none:
						break;
					case TextFrameType.rectangle:
						w.SvgRect("rectangle", Metrics.Left, Metrics.Top, Metrics.Right - Metrics.Left, Metrics.Bottom - Metrics.Top,
							_frameInfo.ColorString.String, _frameInfo.StrokeWidth, "none");
						break;
					case TextFrameType.ellipse:
						w.SvgEllipse("ellipse", Metrics.Left, Metrics.Top, (Metrics.Right - Metrics.Left) / 2, (Metrics.Bottom - Metrics.Top) / 2,
							_frameInfo.ColorString.String, _frameInfo.StrokeWidth, "none");
						break;
					case TextFrameType.circle:
						w.SvgCircle("circle", Metrics.Right - Metrics.Left, Metrics.Bottom - Metrics.Top, ((Metrics.Right - Metrics.Left) / 2),
							_frameInfo.ColorString.String, _frameInfo.StrokeWidth, "none");
						break;
				}
			}
		}

		protected readonly List<Text> Texts = new List<Text>();
		public FrameInfo FrameInfo { get { return _frameInfo; } }
		protected FrameInfo _frameInfo = null;
	}

	internal class FramedMultilineText : TextList
	{
		public FramedMultilineText(object container, List<string> textStrings, float gap, float stafflinethickness, TextHorizAlign hAlign)
			: base(container, textStrings, gap, stafflinethickness, hAlign)
		{
			float paddingX = 22F;
			foreach(string text in textStrings)
			{
				if(text.Length > 1)
				{
					paddingX = 10F;
					break;
				}
			}

			float paddingY = 22F;

			float strokeWidth = stafflinethickness * 2F;

			_frameInfo = new FrameInfo(TextFrameType.rectangle, paddingX, paddingY, strokeWidth, new ColorString("FF0000"));
		}

		protected string StringList()
		{
			StringBuilder sb = new StringBuilder();
			foreach(Text t in this.Texts)
			{
				sb.Append(t.TextInfo.Text);
				sb.Append(", ");
			}
			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}


	}

	internal class FramedRegionStartText : FramedMultilineText
	{
		public FramedRegionStartText(object container, List<string> textStrings, float gap, float stafflinethickness)
			: base(container, textStrings, gap, stafflinethickness, TextHorizAlign.left)
		{

		}

		public override string ToString()
		{
			return "framedRegionStartText: " + StringList();
		}
	}


	internal class FramedRegionEndText : FramedMultilineText
	{
		public FramedRegionEndText(object container, List<string> textStrings, float gap, float stafflinethickness)
			: base(container, textStrings, gap, stafflinethickness, TextHorizAlign.right)
		{

		}

		public override string ToString()
		{
			return "framedRegionEndText: " + StringList();
		}
	}

	#endregion

	internal class OrnamentText : Text
	{
		public OrnamentText(object container, string text, float chordFontHeight)
			: base(container, text, "Open Sans Condensed", chordFontHeight * 0.55F, TextHorizAlign.center)
		{
		}

		public override string ToString()
		{
			return "ornament: " + TextInfo.Text;
		}
	}

	internal class LyricText : Text
	{
		public LyricText(object container, string text, float chordFontHeight)
			: base(container, text, "Arial", (chordFontHeight / 2F), TextHorizAlign.center)
		{
		}

		public override string ToString()
		{
			return "lyric: " + TextInfo.Text;
		}
	}

	internal class DynamicText : Text
	{
		public DynamicText(object container, string text, float chordFontHeight)
			: base(container, text, "CLicht", chordFontHeight * 0.75F, TextHorizAlign.left)
		{
		}

		public override string ToString()
		{
			return "dynamic: " + TextInfo.Text;
		}
	}
}
