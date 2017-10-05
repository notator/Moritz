using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
	internal class TextMetrics : TextStyle
	{
		public TextMetrics(CSSClass cssClass, Graphics graphics, TextInfo textInfo)
			: base(cssClass, textInfo.FontFamily, textInfo.FontHeight, textInfo.TextHorizAlign, textInfo.ColorString.String)
		{
			SetDefaultMetrics(graphics, textInfo);
			_textInfo = textInfo;
		}

		public override void WriteSVG(SvgWriter w)
		{
			w.SvgText(CSSObjectClass, _textInfo.Text, _originX, _originY);
		}

		/// <summary>
		/// Sets the default Top, Right, Bottom, Left.
		///   1. the width of the text is set to the value returned by MeasureText() (no padding)
		///   2. the top and bottom metrics are set to values measured experimentally, using my
		///   program: "../_demo projects/MeasureTextDemo/MeasureTextDemo.sln"
		///		 _top is usually set here to the difference between the top and bottom line positions in that program
		///		 _bottom is always set here to 0
		///		 The fonts currently supported are:
		///         "Open Sans"
		///         "Open Sans Condensed"
		///         "Arial"
		///      These fonts have to be added to the Assistant Performer's fonts folder, and to its fontStyleSheet.css
		///      so that they will work on any operating system.
		///   3. moves the Metrics horizontally to take account of the textinfo.TextHorizAlign setting,
		///      leaving OriginX and OriginY at 0F.
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="textInfo"></param>
		private void SetDefaultMetrics(Graphics graphics, TextInfo textInfo)
		{
			//float maxFontSize = System.Single.MaxValue - 10F;
			float maxFontSize = 1000F;
			Size textMaxSize = new Size();
			try
			{
				textMaxSize = MeasureText(graphics, textInfo.Text, textInfo.FontFamily, maxFontSize);
			}
			catch(Exception ex)
			{
				Debug.Assert(false, ex.Message);
			}
			_left = 0;
			_right = textInfo.FontHeight * textMaxSize.Width / maxFontSize;
			switch(textInfo.FontFamily)
			{
				case "Open Sans": // titles
				case "Open Sans Condensed": // ornaments
					_top = textInfo.FontHeight * -0.699F; // The difference between the height
					_bottom = 0F;
					break;
				case "Arial": // date stamp, lyrics, staff names
					//_top = textInfo.FontHeight * -0.818F; // using MeasureTextDemo
					_top = textInfo.FontHeight * -0.71F; // by experiment!
					_bottom = 0F;
					break;
				//case "Times New Roman": // staff names
				//	_top = textInfo.FontHeight * -1.12F;
				//	_bottom = 0F;
				//	break;
				default:
					Debug.Assert(false, "Unknown font");
					break;
			}

			if(textInfo.TextHorizAlign == TextHorizAlign.center)
				Move(-(_right / 2F), 0F);
			else if(textInfo.TextHorizAlign == TextHorizAlign.right)
				Move(-_right, 0F);

			_originX = 0;
			_originY = 0; // SVG originY is the baseline of the text
		}

		private Size MeasureText(Graphics g, string text, string fontFace, float fontHeight)
		{
			Size maxSize = new Size(int.MaxValue, int.MaxValue);
			TextFormatFlags flags = TextFormatFlags.NoPadding;
			Size sizeOfString;
			using(Font sysFont = new Font(fontFace, fontHeight))
			{
				sizeOfString = TextRenderer.MeasureText(g, text, sysFont, maxSize, flags);
			}
			return sizeOfString;
		}

		private readonly TextInfo _textInfo = null;
	}
	internal class LyricMetrics : TextMetrics, ICloneable
	{
		public LyricMetrics(float gap, Graphics graphics, TextInfo textInfo, bool isBelow, CSSClass lyricClass)
			: base(lyricClass, graphics, textInfo)
		{
			float width = _right - _left;
			float newWidth = width * 0.75F;
			float widthMargin = (width - newWidth) / 2.0F;
			_left += widthMargin;
			_right -= widthMargin;

			IsBelow = isBelow;
		}
		public object Clone()
		{
			return this.MemberwiseClone();
		}

		public readonly bool IsBelow;
	}
	internal class OrnamentMetrics : TextMetrics, ICloneable
	{
		public OrnamentMetrics(float gap, Graphics graphics, TextInfo textInfo, bool isBelow)
			: base(CSSClass.ornament, graphics, textInfo)
		{
			IsBelow = isBelow;
		}
		public object Clone()
		{
			return this.MemberwiseClone();
		}

		public readonly bool IsBelow;
	}
	internal class BarnumberMetrics : GroupMetrics
	{
		public BarnumberMetrics(Graphics graphics, TextInfo textInfo, FrameInfo frameInfo)
			: base(CSSClass.barNumber)
		{
            _barNumberNumberMetrics = new TextMetrics(CSSClass.barNumberNumber, graphics, textInfo);
            _number = textInfo.Text;
			_top = _barNumberNumberMetrics.Top - frameInfo.PaddingY;
			_right = _barNumberNumberMetrics.Right + frameInfo.PaddingX;
			_bottom = _barNumberNumberMetrics.Bottom + frameInfo.PaddingY;
			_left = _barNumberNumberMetrics.Left - frameInfo.PaddingX;
			_strokeWidth = frameInfo.StrokeWidth;
		}

        public override void Move(float dx, float dy)
        {
            base.Move(dx, dy);
            _barNumberNumberMetrics.Move(dx, dy);
        }

        public override void WriteSVG(SvgWriter w)
		{
            w.SvgStartGroup(CSSObjectClass.ToString());
            w.SvgText(CSSClass.barNumberNumber, _number, _barNumberNumberMetrics.OriginX, _barNumberNumberMetrics.OriginY);
            w.SvgRect(CSSClass.barNumberFrame.ToString(), _left, _top, _right - _left, _bottom - _top);
            w.SvgEndGroup();
		}

        TextMetrics _barNumberNumberMetrics = null;
        string _number;
        float _strokeWidth = 0;
	}
}
