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
        /// <param name="cssClass"></param>
        /// <param name="strokeWidthPixels"></param>
        /// <param name="stroke">"none", "black", "white", "red" or a string of 6 hex characters</param>
        /// <param name="fill">"none", "black", "white", "red" or a string of 6 hex characters</param>
        /// <param name="lineCap"></param>
        public LineMetrics(CSSClass cssClass,
            float strokeWidthPixels,
            string stroke = "none", 
            string fill = "none",
            CSSLineCap lineCap = CSSLineCap.butt)
            : base(cssClass)
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
			: base(isInput ? CSSClass.inputStem : CSSClass.stem, strokeWidth, "black")
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
        public LedgerlineBlockMetrics(float left, float right, float strokeWidth, CSSClass ledgerlinesClass)
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
            CSSClass ledgerlineClass = (CSSObjectClass == CSSClass.inputLedgerlines) ? CSSClass.inputLedgerline : CSSClass.ledgerline;

            w.WriteStartElement("g");
            w.WriteAttributeString("class", CSSObjectClass.ToString());
            foreach(float y in Ys)
			{
				w.SvgLine(ledgerlineClass, _left + _strokeWidth, y, _right - _strokeWidth, y);
			}
            w.WriteEndElement();
		}

		private List<float> Ys = new List<float>();
		private float _strokeWidth;
    }
	internal class CautionaryBracketMetrics : LineMetrics, ICloneable
	{
		public CautionaryBracketMetrics(bool isLeftBracket, float top, float right, float bottom, float left, float strokeWidth)
			: base(CSSClass.cautionaryBracket, strokeWidth, "black")
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
			w.SvgCautionaryBracket(CSSObjectClass.ToString(), _isLeftBracket, _top, _right, _bottom, _left);
		}

		private readonly bool _isLeftBracket;
		private readonly float _strokeWidth;
	}
	internal class StafflineMetrics : LineMetrics
	{
		public StafflineMetrics(float left, float right, float originY)
			: base(CSSClass.staffline, 0F, "black")
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
			: base(CSSClass.noteExtender, strokeWidth, strokeColor)

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
            : base(CSSClass.barline, strokeWidth, "black")
        {
            _originX = 0F;
            _left = -(strokeWidth / 2F);
            _right = strokeWidth / 2F;

            if(graphics != null && barline != null)
            {
                foreach(DrawObject drawObject in barline.DrawObjects)
                {
                    Text text = drawObject as Text;
                    if(text != null)
                    {
                        Debug.Assert(text.TextInfo != null
                        && (text is StaffNameText || text is FramedBarNumberText));

                        if(text is StaffNameText)
                        {
                            CSSClass staffClass = (barline.Voice is InputVoice) ? CSSClass.inputStaffName : CSSClass.staffName;
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
                            //_barnumberMetrics = new TextMetrics(graphics, null, text.TextInfo);
                            // move the bar number above this barline
                            float deltaY = (gap * 6F);
                            _barnumberMetrics.Move(0F, -deltaY);
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
        }

        /// <summary>
        /// Barline.WriteSVG(...) is used instead
        /// </summary>
        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Only writes the staffName and the barnumber (if they exist) to the SVG file.
        /// The barline itself is drawn by Barline.WriteSvg(...) when the system is complete.
        /// </summary>
        public void WriteStaffNameAndBarNumberSVG(SvgWriter w, bool isInput)
        {
            if(_staffNameMetrics != null)
            {
                _staffNameMetrics.WriteSVG(w);
            }
            if(_barnumberMetrics != null)
            {
                _barnumberMetrics.WriteSVG(w);
            }
        }

        private GroupMetrics _staffControlsGroupMetrics = null;
        public BarnumberMetrics BarnumberMetrics { get { return _barnumberMetrics; } }
        private BarnumberMetrics _barnumberMetrics = null;
        public TextMetrics StaffNameMetrics { get { return _staffNameMetrics; } }
        private TextMetrics _staffNameMetrics = null;
    }

    internal class EndBarlineMetrics : GroupMetrics
    {
        public EndBarlineMetrics(float thinStrokeWidth, float thickStrokeWidth)
            : base(CSSClass.endBarline)
        {
            LeftLine = new LineMetrics(CSSClass.barline, thinStrokeWidth, "black");
            RightLine = new LineMetrics(CSSClass.thickBarline, thickStrokeWidth, "black");

            LeftLine.Move(-(thinStrokeWidth * 3F), 0);

            Add(LeftLine);
            Add(RightLine);
            Move(-(thickStrokeWidth / 2F), 0);
        }

        /// <summary>
        /// EndBarline.WriteSVG(...) is used instead
        /// </summary>
        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }

        public LineMetrics LeftLine;
        public LineMetrics RightLine;
    }
}
