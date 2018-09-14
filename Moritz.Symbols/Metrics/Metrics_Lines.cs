using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    public class LineMetrics : Metrics
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="csslineClass"></param>
        /// <param name="strokeWidthPixels"></param>
        /// <param name="stroke">"none", "black", "white", "red" or a string of 6 hex characters</param>
        /// <param name="fill">"none", "black", "white", "red" or a string of 6 hex characters</param>
        /// <param name="lineCap"></param>
        public LineMetrics(CSSObjectClass csslineClass,
            float strokeWidthPixels,
            string stroke = "none", 
            string fill = "none",
            CSSLineCap lineCap = CSSLineCap.butt)
            : base(csslineClass)
        {
            StrokeWidthPixels = strokeWidthPixels;
            Stroke = stroke.ToString();
            Fill = fill.ToString();
            LineCap = lineCap.ToString();
        }

        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }

        public readonly float StrokeWidthPixels = 0F;
        public readonly string Stroke = "none"; // "none", "black", "white", "#333" etc
        public readonly string Fill = "none"; // "none", "black", "white", "#333" etc
        public readonly string LineCap = "butt"; // "butt", "round", "square" 
    }
 
	internal class StemMetrics : LineMetrics
	{
		public StemMetrics(float top, float x, float bottom, float strokeWidth, VerticalDir verticalDir, bool isInput)
			: base(isInput ? CSSObjectClass.inputStem : CSSObjectClass.stem, strokeWidth, "black")
		{
			_originX = x;
			_originY = top;
			_top = top;
			_right = x + strokeWidth;
			_bottom = bottom;
			_left = x - strokeWidth;
			VerticalDir = verticalDir;
			StrokeWidth = strokeWidth;
		}

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgLine(CSSObjectClass, _originX, _top, _originX, _bottom);
        }

        public object Clone()
		{
			return this.MemberwiseClone();
		}

		public readonly VerticalDir VerticalDir;
		public readonly float StrokeWidth;
	}
	internal class LedgerlineBlockMetrics : LineMetrics, ICloneable
	{      
        public LedgerlineBlockMetrics(float left, float right, float strokeWidth, CSSObjectClass ledgerlinesClass)
			: base(ledgerlinesClass, strokeWidth, "black")
		{
            /// The base class has deliberately been called with CSSClass.ledgerline (singular) here.
            /// This is so that its less confusing later when comparing the usage with stafflines/staffline.
            /// A ledgerline is always contained in a ledgerlines group.
            /// A staffline is always contained in a stafflines group.
            /// The CSS definition for ledgerlines is written if a ledgerline has been used.
            /// The CSS definition for stafflines is written if a staffline has been used.

            _left = left;
			_right = right;
			_strokeWidth = strokeWidth;
		}

		public void AddLedgerline(float newY, float gap)
		{
			if(Ys.Count == 0)
			{
				_top = newY - (gap / 2F);
				_bottom = newY + (gap / 2F);
			}
			else
				_bottom = newY + (gap / 2F);

			Ys.Add(newY);
		}

		public override void Move(float dx, float dy)
		{
			base.Move(dx, dy);
			for(int i = 0; i < Ys.Count; i++)
			{
				Ys[i] += dy;
			}
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}

		public override void WriteSVG(SvgWriter w)
        {
            CSSObjectClass ledgerlineClass = (CSSObjectClass == CSSObjectClass.inputLedgerlines) ? CSSObjectClass.inputLedgerline : CSSObjectClass.ledgerline;

            w.WriteStartElement("g");
            w.WriteAttributeString("class", CSSObjectClass.ToString());
            foreach(float y in Ys)
			{
				w.SvgLine(ledgerlineClass, _left + _strokeWidth, y, _right - _strokeWidth, y);
			}
            w.WriteEndElement();
		}

		private List<float> Ys = new List<float>();
		private readonly float _strokeWidth;
    }
	internal class CautionaryBracketMetrics : LineMetrics, ICloneable
	{
		public CautionaryBracketMetrics(bool isLeftBracket, float top, float right, float bottom, float left, float strokeWidth)
			: base(CSSObjectClass.cautionaryBracket, strokeWidth, "black")
		{
			_isLeftBracket = isLeftBracket;
			_top = top;
			_left = left;
			_bottom = bottom;
			_right = right;
			_strokeWidth = strokeWidth;
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}

        public override void WriteSVG(SvgWriter w)
        {
			w.SvgCautionaryBracket(CSSObjectClass, _isLeftBracket, _top, _right, _bottom, _left);
		}

		private readonly bool _isLeftBracket;
		private readonly float _strokeWidth;
	}
	internal class StafflineMetrics : LineMetrics
	{
		public StafflineMetrics(float left, float right, float originY)
			: base(CSSObjectClass.staffline, 0F, "black")
		{
			_left = left;
			_right = right;
			_top = originY;
			_bottom = originY;
			_originY = originY;
		}

        /// <summary>
        /// This function should never be called.
        /// See Staff.WriteSVG(...).
        /// </summary>
        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }
	}
	/// <summary>
	/// Notehead extender lines are used when chord symbols cross barlines.
	/// </summary>
	internal class NoteheadExtenderMetrics : LineMetrics
	{
		public NoteheadExtenderMetrics(float left, float right, float originY, float strokeWidth, string strokeColor, float gap, bool drawExtender)
			: base(CSSObjectClass.noteExtender, strokeWidth, strokeColor)

        {
			_left = left;
			_right = right;

			// _top and _bottom are used when drawing barlines between staves
			_top = originY;
			_bottom = originY;
			if(drawExtender == false)
			{
				_top -= (gap / 2F);
				_bottom += (gap / 2F);
			}

			_originY = originY;
            _strokeColor = strokeColor;
			_strokeWidth = strokeWidth;
			_drawExtender = drawExtender;
		}

        public override void WriteSVG(SvgWriter w)
        {
            if(_drawExtender)
                w.SvgLine(CSSObjectClass, _left, _originY, _right, _originY);
        }

        public string StrokeColor { get { return _strokeColor; } }
        private readonly string _strokeColor;
		private readonly float _strokeWidth = 0F;
		private readonly bool _drawExtender;
	}

    internal class BarlineMetrics : LineMetrics
    {
        public BarlineMetrics(Graphics graphics, Barline barline, float strokeWidth, float gap)
            : base(CSSObjectClass.barline, strokeWidth, "black")
        {
            _originX = 0F;
            _left = -(strokeWidth / 2F);
            _right = strokeWidth / 2F;

            if(graphics != null && barline != null)
            {
				float minimumBarlineTopToRegionInfoBottom = gap * 3F;
				float minimumBarlineTopToBarnumberBottom = gap * 6F;

				foreach(DrawObject drawObject in barline.DrawObjects)
                {
                    if(drawObject is Text text)
                    {
                        Debug.Assert(text.TextInfo != null
                        && (text is StaffNameText || text is FramedBarNumberText));

                        if(text is StaffNameText)
                        {
                            CSSObjectClass staffClass = (barline.Voice is InputVoice) ? CSSObjectClass.inputStaffName : CSSObjectClass.staffName;
                            _staffNameMetrics = new TextMetrics(staffClass, graphics, text.TextInfo);
                            // move the staffname vertically to the middle of this staff
                            Staff staff = barline.Voice.Staff;
                            float staffheight = staff.Gap * (staff.NumberOfStafflines - 1);
                            float dy = (staffheight * 0.5F) + (gap * 0.8F);
                            _staffNameMetrics.Move(0F, dy);
                        }
                        else if(text is FramedBarNumberText)
                        {
                            _barnumberMetrics = new BarnumberMetrics(graphics, text.TextInfo, text.FrameInfo);
                            // move the bar number above this barline
                            _barnumberMetrics.Move(0F, -minimumBarlineTopToBarnumberBottom);
                        }
                    }
					else if(drawObject is TextList textList)
					{
						Debug.Assert(textList is FramedRegionStartText || textList is FramedRegionEndText);
						
						if(textList is FramedRegionStartText framedRegionStartText)
						{
							_framedRegionStartTextMetrics = new FramedRegionInfoMetrics(graphics, framedRegionStartText.Texts, framedRegionStartText.FrameInfo);
							// move the regionInfo above this barline, and align its left edge with the barline
							FramedRegionInfoMetrics frstm = _framedRegionStartTextMetrics;
							frstm.Move(0, -minimumBarlineTopToRegionInfoBottom);
						}
						else if(textList is FramedRegionEndText framedRegionEndText)
						{
							_framedRegionEndTextMetrics = new FramedRegionInfoMetrics(graphics, framedRegionEndText.Texts, framedRegionEndText.FrameInfo);
							// move the regionInfo above this barline, and align its right edge with the barline
							FramedRegionInfoMetrics fretm = _framedRegionEndTextMetrics;
							fretm.Move(0, -minimumBarlineTopToRegionInfoBottom);
						}
					}
				}
            }
        }

        public override void Move(float dx, float dy)
        {
            base.Move(dx, dy);
            if(_staffControlsGroupMetrics != null)
                _staffControlsGroupMetrics.Move(dx, dy);
            if(_barnumberMetrics != null)
                _barnumberMetrics.Move(dx, dy);
			if(_staffNameMetrics != null)
				_staffNameMetrics.Move(dx, dy);
			if(this._framedRegionStartTextMetrics != null)
				_framedRegionStartTextMetrics.Move(dx, dy);
			if(this._framedRegionEndTextMetrics != null)
				_framedRegionEndTextMetrics.Move(dx, dy);
		}

        /// <summary>
        /// Barline.WriteSVG(...) is used instead
        /// </summary>
        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes any DrawObjects attached to the barline to the SVG file.
        /// </summary>
        public void WriteDrawObjectsSVG(SvgWriter w)
        {
            if(_staffNameMetrics != null)
            {
                _staffNameMetrics.WriteSVG(w);
            }
			if(_barnumberMetrics != null)
			{
				_barnumberMetrics.WriteSVG(w);
			}
			if(_framedRegionStartTextMetrics != null)
			{
				_framedRegionStartTextMetrics.WriteSVG(w);
			}
			if(_framedRegionEndTextMetrics != null)
			{
				_framedRegionEndTextMetrics.WriteSVG(w);
			}
		}

        private GroupMetrics _staffControlsGroupMetrics = null;
		public FramedRegionInfoMetrics FramedRegionStartTextMetrics { get { return _framedRegionStartTextMetrics; } }
		private FramedRegionInfoMetrics _framedRegionStartTextMetrics = null;
		public FramedRegionInfoMetrics FramedRegionEndTextMetrics { get { return _framedRegionEndTextMetrics; } }
		private FramedRegionInfoMetrics _framedRegionEndTextMetrics = null;

		public BarnumberMetrics BarnumberMetrics { get { return _barnumberMetrics; } }
		private BarnumberMetrics _barnumberMetrics = null;
		public TextMetrics StaffNameMetrics { get { return _staffNameMetrics; } }
        private TextMetrics _staffNameMetrics = null;
    }

    internal class EndBarlineMetrics : GroupMetrics
    {

		public EndBarlineMetrics(Graphics graphics, EndBarline endBarline, float thinStrokeWidth, float thickStrokeWidth, float gap)
            : base(CSSObjectClass.endBarline)
        {
            LeftLine = new LineMetrics(CSSObjectClass.barline, thinStrokeWidth, "black");
            RightLine = new LineMetrics(CSSObjectClass.thickBarline, thickStrokeWidth, "black");

            LeftLine.Move(-(thinStrokeWidth * 3F), 0);

            Add(LeftLine);
            Add(RightLine);
            Move(-(thickStrokeWidth / 2F), 0);

			if(graphics != null && endBarline != null)
			{
				float barlineTopToRegionInfoBottom = gap * 5F;
				float barlineTopToBarnumberBottom = gap * 6F;

				foreach(DrawObject drawObject in endBarline.DrawObjects)
				{
					if(drawObject is TextList textList)
					{
						Debug.Assert(textList is FramedRegionEndText);

						if(textList is FramedRegionEndText framedRegionEndText)
						{
							_framedRegionEndTextMetrics = new FramedRegionInfoMetrics(graphics, framedRegionEndText.Texts, framedRegionEndText.FrameInfo);
							// move the regionInfo above this barline, and align its right edge with the right barline
							FramedRegionInfoMetrics fretm = _framedRegionEndTextMetrics;
							fretm.Move(0, -barlineTopToRegionInfoBottom);
						}
					}
				}
			}
		}

        /// <summary>
        /// EndBarline.WriteSVG(...) is used instead
        /// </summary>
        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }

		internal void WriteDrawObjectsSVG(SvgWriter w)
		{
			if(_framedRegionEndTextMetrics != null)
			{
				_framedRegionEndTextMetrics.WriteSVG(w);
			}
		}

		public LineMetrics LeftLine;
        public LineMetrics RightLine;

		public FramedRegionInfoMetrics FramedRegionEndTextMetrics { get { return _framedRegionEndTextMetrics; } }
		private FramedRegionInfoMetrics _framedRegionEndTextMetrics = null;
	}
}
