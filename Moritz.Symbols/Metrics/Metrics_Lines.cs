using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
	internal class StemMetrics : Metrics
	{
		public StemMetrics(float top, float x, float bottom, float strokeWidth, VerticalDir verticalDir)
			: base()
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
			w.SvgLine("stem", _originX, _top, _originX, _bottom);
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}

		public readonly VerticalDir VerticalDir;
		public readonly float StrokeWidth;
	}
	internal class LedgerlineBlockMetrics : Metrics, ICloneable
	{
		public LedgerlineBlockMetrics(float left, float right, float strokeWidth)
			: base()
		{
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
            w.WriteStartElement("g");
            w.WriteAttributeString("class", "ledgerlines");
            foreach(float y in Ys)
			{
				w.SvgLine("ledgerline", _left + _strokeWidth, y, _right - _strokeWidth, y);
			}
            w.WriteEndElement();
		}

		private List<float> Ys = new List<float>();
		private float _strokeWidth;
	}
	internal class CautionaryBracketMetrics : Metrics, ICloneable
	{
		public CautionaryBracketMetrics(bool isLeftBracket, float top, float right, float bottom, float left, float strokeWidth)
			: base()
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
			w.SvgCautionaryBracket(_isLeftBracket, _top, _right, _bottom, _left);
		}

		private readonly bool _isLeftBracket;
		private readonly float _strokeWidth;
	}
	/// <summary>
	/// This class is only used when justifying staves and systems vertically.
	/// </summary>
	internal class StafflineMetrics : Metrics
	{
		public StafflineMetrics(float left, float right, float originY)
			: base()
		{
			_left = left;
			_right = right;
			_top = originY;
			_bottom = originY;
			_originY = originY;
		}

		public override void WriteSVG(SvgWriter w) { }
	}
	/// <summary>
	/// Notehead extender lines are used when chord symbols cross barlines.
	/// </summary>
	internal class NoteheadExtenderMetrics : Metrics
	{
		public NoteheadExtenderMetrics(float left, float right, float originY, string colorAttribute, float strokeWidth, float gap, bool drawExtender)
			: base()
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
            if(! string.IsNullOrEmpty(colorAttribute))
            {
                _colorAttribute = colorAttribute;
            }
			_strokeWidth = strokeWidth;
			_drawExtender = drawExtender;
		}

		public override void WriteSVG(SvgWriter w)
		{
			if(_drawExtender)
				w.SvgLine("noteExtender", _left, _originY, _right, _originY);
		}

        public string ColorAttribute { get { return _colorAttribute; } }
        private readonly string _colorAttribute = "black";
		private readonly float _strokeWidth = 0F;
		private readonly bool _drawExtender;
	}
    internal class BarlineMetrics : Metrics
    {
        public BarlineMetrics(Graphics graphics, Barline barline, float gap)
            : base()
        {
            if(barline.BarlineType == BarlineType.end)
                _left = -gap * 1.7F;
            else
                _left = -gap * 0.5F;
            _originX = 0F;
            _right = gap / 2F;

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
                            _staffNameMetrics = new TextMetrics(graphics, text.TextInfo);
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
        /// Only writes the Barline's barnumber to the SVG file.
        /// The barline itself is drawn when the system is complete.
        /// </summary>
        public override void WriteSVG(SvgWriter w)
        {
            if(_staffNameMetrics != null)
            {
                _staffNameMetrics.WriteSVG(w, "staffName");
            }
            if(_barnumberMetrics != null)
            {
                w.WriteStartElement("g");
                w.WriteAttributeString("class", "barNumber");
                _barnumberMetrics.WriteSVG(w); // writes the number and the frame
                w.WriteEndElement(); // barnumber group
            }
        }

        private GroupMetrics _staffControlsGroupMetrics = null;
        public BarnumberMetrics BarnumberMetrics { get { return _barnumberMetrics; } }
        private BarnumberMetrics _barnumberMetrics = null;
        public TextMetrics StaffNameMetrics { get { return _staffNameMetrics; } }
        private TextMetrics _staffNameMetrics = null;
    }
}
