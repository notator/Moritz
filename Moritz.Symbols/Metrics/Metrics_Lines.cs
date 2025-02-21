using Moritz.Xml;

using System;
using System.Collections.Generic;

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
        public StemMetrics(float top, float x, float bottom, float strokeWidth, VerticalDir verticalDir)
            : base(CSSObjectClass.stem, strokeWidth, "black")
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
            w.WriteStartElement("g");
            w.WriteAttributeString("class", CSSObjectClass.ToString());
            foreach(float y in Ys)
            {
                w.SvgLine(CSSObjectClass.ledgerline, _left + _strokeWidth, y, _right - _strokeWidth, y);
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

    internal class Barline_LineMetrics : Metrics
    {
        public Barline_LineMetrics(float leftReOriginX, float rightReOriginX,
            CSSObjectClass lineClass1 = CSSObjectClass.normalBarline, CSSObjectClass lineClass2 = CSSObjectClass.normalBarline)
            : base(lineClass1, lineClass2)
        {
            _originX = 0F;
            _left = leftReOriginX; // for a normal, thin barline: -(strokeWidth / 2F);
            _right = rightReOriginX; // for a normal, thin barline: strokeWidth / 2F;
        }

        //public override void Move(float dx, float dy)
        //{
        //	base.Move(dx, dy);
        //}

        /// <summary>
        /// Use Barline.WriteSVG(...) instead.
        /// </summary>
        public override void WriteSVG(SvgWriter w)
        {
            throw new ApplicationException();
        }
    }
}
