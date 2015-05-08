using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    public abstract class Metrics
    {
        protected Metrics()
            : this(null, 0F, 0F)
        {
        }

        protected Metrics(string type, float originX, float originY)
        {
            if(!String.IsNullOrEmpty(type))
                _objectType = type;

            _originX = originX;
            _originY = originY;
        }

        public virtual void Move(float dx, float dy)
        {
            _left += dx;
            _top += dy;
            _right += dx;
            _bottom += dy;
            _originX += dx;
            _originY += dy;
        }

        public virtual void WriteSVG(SvgWriter w){}

        /// <summary>
        /// Use this function to check all atomic overlaps (single character or line boxMetrics).
        /// </summary>
        /// <param name="metrics"></param>
        /// <returns></returns>
        public bool Overlaps(Metrics metrics)
        {
            bool verticalOverlap = true;
            bool horizontalOverlap = true;
            if((metrics.Top > Bottom) || (metrics.Bottom < Top))
                verticalOverlap = false;
            if((metrics.Left > Right) || (metrics.Right < Left))
                horizontalOverlap = false;

            return verticalOverlap && horizontalOverlap;
        }

        /// <summary>
        /// If the previousMetrics overlaps this metrics vertically, and this.Left is les than 
        /// than previousMetrics.Right, previousMetrics.Right - this.Left is returned. 
        /// If there is no vertical overlap, this or the previousMetrics is a RestMetrics, and
        /// the metrics overlap, half the width of the rest is returned.
        /// Otherwise float.MinValue is returned.
        /// </summary>
        public float OverlapWidth(Metrics previousMetrics)
        {
            bool verticalOverlap = true;
            if(!(this is BarlineMetrics))
            {
                if((previousMetrics.Top > Bottom) || (previousMetrics.Bottom < Top))
                    verticalOverlap = false;
            }

            float overlap = float.MinValue;
            if(verticalOverlap && previousMetrics.Right > Left)
            {
                overlap = previousMetrics.Right - this.Left;
                if(this._objectType == "b" || this._objectType == "n")
                {
                    overlap -= ((this.Right - this.Left) * 0.15F);
                    overlap = overlap > 0F ? overlap : 0F;
                }
            }

            if(!verticalOverlap && this is RestMetrics && this.OriginX <= previousMetrics.Right)
                overlap = previousMetrics.Right - this.OriginX;

            if(!verticalOverlap && previousMetrics is RestMetrics && previousMetrics.OriginX >= Left)
                overlap = previousMetrics.OriginX - Left;


            return overlap;
        }

        /// <summary>
        /// Returns the positive horizontal distance by which this metrics overlaps the argument metrics.
        /// The result can be 0, if previousMetrics.Right = this.Metrics.Left.
        /// If there is no overlap, float.MinValue is returned.
        /// </summary>
        public float OverlapWidth(AnchorageSymbol previousAS)
        {
            float overlap = float.MinValue;

            OutputChordSymbol chord = previousAS as OutputChordSymbol;
            if(chord != null)
            {
                overlap = chord.ChordMetrics.OverlapWidth(this);
            }
            else
            {
                overlap = this.OverlapWidth(previousAS.Metrics);
            }
            return overlap;
        }

        /// <summary>
        /// If the padded argument overlaps this metrics horizontally, 
        /// and this.Top is smaller than paddedArgument.Bottom,
        /// this.Bottom - paddedArgument.Right is returned. 
        /// Otherwise 0F is returned.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public float OverlapHeight(Metrics arg, float padding)
        {
            float newArgRight = arg.Right + padding;
            float newArgLeft = arg.Left - padding;
            float newArgBottom = arg.Bottom + padding;

            bool horizontalOverlap = true;
            if((newArgRight < Left) || (newArgLeft > Right))
                horizontalOverlap = false;

            if(horizontalOverlap && newArgBottom > Top)
            {
                float overlap = newArgBottom - Top;
                return (overlap);
            }
            else
                return 0F;
        }

        public void SetTop(float top)
        {
            _top = top;
        }

        public void SetBottom(float bottom)
        {
            _bottom = bottom;
        }

        protected void MoveAboveTopBoundary(float topBoundary, float padding)
        {
            Debug.Assert(padding >= 0.0F);
            float newBottom = topBoundary - padding;
            Move(0F, newBottom - Bottom);
        }
        protected void MoveBelowBottomBoundary(float bottomBoundary, float padding)
        {
            Debug.Assert(padding >= 0.0F);
            float newTop = bottomBoundary + padding;
            Move(0F, newTop - Top);
        }

        /// <summary>
        /// The actual position of the top edge of the object in the score.
        /// </summary>
        public float Top { get { return _top; } }
        protected float _top = 0F;
        /// <summary>
        /// The actual position of the right edge of the object in the score.
        /// </summary>
        public float Right { get { return _right; } }
        protected float _right = 0F;
        /// <summary>
        /// The actual position of the bottom edge of the object in the score.
        /// </summary>
        public virtual float Bottom { get { return _bottom; } }
        protected float _bottom = 0F;
        /// <summary>
        /// The actual position of the left edge of the object in the score.
        /// </summary>
        public float Left { get { return _left; } }
        protected float _left = 0F;
        /// <summary>
        /// The object's type (which will be used when writing it to the SVG file).
        /// </summary>
        public string ObjectType
        {
            get
            {
                return _objectType;
            }
        }
        /// <summary>
        /// The object's unicode value or type (used when writing cLicht characters to the SVG file).
        /// </summary>
        public string ID_Type { get { return _objectType; } }
        protected string _objectType = null;
        /// <summary>
        /// The actual position of the object's x-origin in the score.
        /// This is the value written into the SvgScore.
        /// </summary>
        public float OriginX { get { return _originX; } }
        protected float _originX = 0F;
        /// <summary>
        /// The actual position of the object's y-origin in the score
        /// This is the value written into the SvgScore.
        /// </summary>
        public float OriginY { get { return _originY; } }
        protected float _originY = 0F;

    }

    /// <summary>
    /// This class is used to construct the CLichtFontMetrics.CLichtGlyphBoundingBoxesDictPX dictionary.
    /// </summary>
    internal class CLichtGlyphBoxMetric : Metrics
    {
        public CLichtGlyphBoxMetric(float top, float right, float bottom, float left)
            : base()
        {
            _top = top;
            _right = right; 
            _bottom = bottom;
            _left = left;
        } 
    }

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
			w.SvgLine("stem" + SvgScore.UniqueID_Number, _originX, _top, _originX, _bottom, "black", StrokeWidth, "round");
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
            _objectType = "ledgerlineBlock";
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
            else _bottom = newY + (gap / 2F);

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
            foreach(float y in Ys)
            {
                w.SvgLine("ledger" + SvgScore.UniqueID_Number, _left + _strokeWidth, y, _right - _strokeWidth, y, "black", _strokeWidth, null);
            }
        }

        private List<float> Ys = new List<float>();
        private float _strokeWidth;
    }
    internal class CautionaryBracketMetrics : Metrics, ICloneable
    {
        public CautionaryBracketMetrics(bool isLeftBracket, float top, float right, float bottom, float left, float strokeWidth)
            : base()
        {
            _objectType = "cautionaryBracket";
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
            w.SvgCautionaryBracket(null, _isLeftBracket, _top, _right, _bottom, _left, _strokeWidth);
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
    }
    /// <summary>
    /// Notehead extender lines are used when chord symbols cross barlines.
    /// </summary>
    internal class NoteheadExtenderMetrics : Metrics
    {
        public NoteheadExtenderMetrics(float left, float right, float originY, float strokeWidth, float gap, bool drawExtender)
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
            _strokeWidth = strokeWidth;
            _drawExtender = drawExtender;
        }

        public override void WriteSVG(SvgWriter w)
        {
            if(_drawExtender)
				w.SvgLine("extender" + SvgScore.UniqueID_Number, _left, _originY, _right, _originY, "black", _strokeWidth, "butt");
        }

        private readonly float _strokeWidth = 0F;
        private readonly bool _drawExtender;
    }
    internal class FlagsBlockMetrics : Metrics
    {
        /// <summary>
        /// Should be called with a duration class having a flag block
        /// </summary>
        public FlagsBlockMetrics(DurationClass durationClass, float fontHeight, VerticalDir stemDirection)
            : base()
        {
            _left = 0F;
            _right = 0.31809F * fontHeight;
            _originX = 0F;
            _originY = 0F;
            _fontHeight = fontHeight;
            _stemDirection = stemDirection;

            float offset = 0F;
            switch(durationClass)
            {
                case DurationClass.quaver:
                    if(_stemDirection == VerticalDir.up)
                        _objectType = "Right1Flag";
                    else
                        _objectType = "Left1Flag";
                    break;
                case DurationClass.semiquaver:
                    if(_stemDirection == VerticalDir.up)
                        _objectType = "Right2Flags";
                    else
                        _objectType = "Left2Flags";
                    offset = 0.25F;
                    break;
                case DurationClass.threeFlags:
                    if(_stemDirection == VerticalDir.up)
                        _objectType = "Right3Flags";
                    else
                        _objectType = "Left3Flags";
                    offset = 0.5F;
                    break;
                case DurationClass.fourFlags:
                    if(_stemDirection == VerticalDir.up)
                        _objectType = "Right4Flags";
                    else
                        _objectType = "Left4Flags";
                    offset = 0.75F;
                    break;
                case DurationClass.fiveFlags:
                    if(_stemDirection == VerticalDir.up)
                        _objectType = "Right5Flags";
                    else
                        _objectType = "Left5Flags";
                    offset = 1F;
                    break;
                default:
                    Debug.Assert(false, "This duration class has no flags.");
                    break;
            }
            if(_stemDirection == VerticalDir.up)
            {
                _top = 0F;
                _bottom = (0.2467F + offset) * fontHeight;
            }
            else
            {
                _top = (-(0.2467F + offset)) * fontHeight;
                _bottom = 0F;
            }
        }

        public override void WriteSVG(SvgWriter w)
        {
            if(_stemDirection == VerticalDir.up)
				w.SvgUseXY("flag" + SvgScore.UniqueID_Number, _objectType, _left, _top, _fontHeight);
            else
				w.SvgUseXY("flag" + SvgScore.UniqueID_Number, _objectType, _left, _bottom, _fontHeight);
        }

        private readonly float _fontHeight;
        private readonly VerticalDir _stemDirection;
        
    }
    internal class BarlineMetrics : Metrics
    {
        public BarlineMetrics(Graphics graphics, Barline barline, float gap)
            : base()
        {
            _objectType = "barline";
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
                        Debug.Assert(text.TextInfo != null);
                        if(text.TextInfo.FontFamily == "Times New Roman") // staff name
                        {
                            _staffNameMetrics = new TextMetrics(graphics, null, text.TextInfo);
                            // move the staffname vertically to the middle of this staff
                            Staff staff = barline.Voice.Staff;
                            float staffheight = staff.Gap * (staff.NumberOfStafflines - 1);
                            float dy = (staffheight * 0.5F) + (gap * 0.8F);
                            _staffNameMetrics.Move(0F, dy);
                        }
                        else
                        {
                            _barnumberMetrics = new BarnumberMetrics(graphics, null, text.TextInfo, text.FrameInfo);
                            //_barnumberMetrics = new TextMetrics(graphics, null, text.TextInfo);
                            // move the bar number above this barline
                            float deltaY = (gap * 6F);
                            _barnumberMetrics.Move(0F, -deltaY);
                        }
                    }

                    StaffControlsGroup staffControlsGroup = drawObject as StaffControlsGroup;
                    if(staffControlsGroup != null)
                    {
                        _staffControlsGroupMetrics = new GroupMetrics("staffControlsGroup");
                        float deltaY = 0F;
                        foreach(DrawObject drawObject1 in staffControlsGroup.DrawObjects)
                        {
                            Text scComponent = drawObject1 as Text;
                            if(scComponent != null)
                            {
                                TextMetrics textMetrics = new TextMetrics(graphics, null, scComponent.TextInfo);
                                textMetrics.Move(0F, deltaY);
                                deltaY += gap;
                                _staffControlsGroupMetrics.Add(textMetrics);                                
                            }
                        }
                        // move the staffControlsGroup into the left margin
                        _staffControlsGroupMetrics.Move(-(gap * 6F), -deltaY - gap);
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
				_staffNameMetrics.WriteSVG(w, "staffName" + SvgScore.UniqueID_Number);
            }
            if(_barnumberMetrics != null)
            {
                w.WriteStartElement("g");
                w.WriteAttributeString("id", "barNumber" + SvgScore.UniqueID_Number);
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

    internal class TextMetrics : Metrics
    {
        public TextMetrics(Graphics graphics, string type, TextInfo textInfo)
            : base(type, 0F, 0F)
        {
            SetDefaultMetrics(graphics, textInfo);
            _textInfo = textInfo;
        }

        public TextMetrics(Graphics graphics, TextInfo textInfo)
            : base("text", 0F, 0F)
        {
            SetDefaultMetrics(graphics, textInfo);
            _textInfo = textInfo;
        }

		internal void WriteSVG(SvgWriter w, string id)
		{
			w.SvgText(id, _textInfo, _originX, _originY);
		}

        /// <summary>
        /// Sets the default Top, Right, Bottom, Left.
        ///   1. the width of the text is set to the value returned by MeasureText() (no padding)
        ///   2. the top and bottom metrics are set to values measured experimentally. The fonts currently
        ///      supported are:
        ///         "Arial"
        ///         "Estrangelo Edessa"
        ///         "Times New Roman"
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
                case "Arial":
                    _top = textInfo.FontHeight * -0.71F;
                    _bottom = 0F;
                    break;
                case "Estrangelo Edessa":
                    _top = textInfo.FontHeight * -0.632F;
                    _bottom = 0F;
                    break;
                case "Times New Roman":
                    _top = textInfo.FontHeight * -1.12F;
                    _bottom = 0F;
                    break;
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
        public LyricMetrics(float gap, Graphics graphics, TextInfo textInfo, bool isBelow)
            : base(graphics, null, textInfo)
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
            : base(graphics, null, textInfo)
        {
            IsBelow = isBelow;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public readonly bool IsBelow;
    }

    internal class BarnumberMetrics : TextMetrics
    {
        public BarnumberMetrics(Graphics graphics, string ID_Type, TextInfo textInfo, FrameInfo frameInfo)
            :base(graphics, ID_Type, textInfo)
        {
            _top = _top - frameInfo.PaddingY;
            _right = _right + frameInfo.PaddingX;
            _bottom = _bottom + frameInfo.PaddingY;
            _left = _left - frameInfo.PaddingX;
            _strokeWidth = frameInfo.StrokeWidth;
        }

        public override void WriteSVG(SvgWriter w)
        {
			base.WriteSVG(w, "number" + SvgScore.UniqueID_Number);
			w.SvgRect("box" + SvgScore.UniqueID_Number, _left, _top, _right - _left, _bottom - _top, "black", _strokeWidth, "none");
        }

        float _strokeWidth = 0;
    }

    internal class CLichtCharacterMetrics : Metrics
    {
        public CLichtCharacterMetrics(string name, bool isDynamic, float fontHeight, TextHorizAlign textHorizAlign)
            : base()
        {
            if(isDynamic)
                _objectType = name;
            else // clefs
                _objectType = GetClichtCharacterString(name);

            Debug.Assert(_objectType != null);
            Metrics m = CLichtFontMetrics.CLichtGlyphBoundingBoxesDictPX[_objectType];

            _originY = 0;
            _top = m.Top * fontHeight;
            _bottom = m.Bottom * fontHeight;

            // move so that Left = 0.
            _left = 0;
            _right = (m.Right - m.Left) * fontHeight;
            _originX = -m.Left * fontHeight;

            _fontHeight = fontHeight;
            _textHorizAlign = textHorizAlign;
        }

        public CLichtCharacterMetrics(DurationClass durationClass, bool isRest, float fontHeight)
            : base()
        {
            _objectType = GetClichtCharacterString(durationClass, isRest);

            Debug.Assert(_objectType != null);
            Metrics m = CLichtFontMetrics.CLichtGlyphBoundingBoxesDictPX[_objectType];

            _originY = 0;
            _top = m.Top * fontHeight;
            _bottom = m.Bottom * fontHeight;

            // move so that Left = 0.
            _left = 0;
            _right = (m.Right - m.Left) * fontHeight;
            _originX = -m.Left * fontHeight;

            _fontHeight = fontHeight;
        }

        public CLichtCharacterMetrics(Head head, float fontHeight)
            : base()
        {
            _objectType = GetClichtCharacterString(head);

            Debug.Assert(_objectType != null);
            Metrics m = CLichtFontMetrics.CLichtGlyphBoundingBoxesDictPX[_objectType];

            _originY = 0;
            _top = m.Top * fontHeight;
            _bottom = m.Bottom * fontHeight;

            // move so that Left = 0.
            _left = 0;
            _right = (m.Right - m.Left) * fontHeight;
            _originX = -m.Left * fontHeight;

            _fontHeight = fontHeight;
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.WriteStartElement("text");
            w.WriteAttributeString("x", _originX.ToString(M.En_USNumberFormat));
            w.WriteAttributeString("y", _originY.ToString(M.En_USNumberFormat));
            w.WriteAttributeString("font-size", _fontHeight.ToString(M.En_USNumberFormat));
            w.WriteAttributeString("font-family", "CLicht");
            switch(_textHorizAlign)
            {
                case TextHorizAlign.left:
                    break;
                case TextHorizAlign.center:
                    w.WriteAttributeString("text-anchor", "middle");
                    break;
                case TextHorizAlign.right:
                    w.WriteAttributeString("text-anchor", "end");
                    break;
            }
            w.WriteString(_objectType); // e.g. Unicode character
            w.WriteEndElement();
        }

        /// <summary>
        /// Clefs
        /// </summary>
        /// <param name="clefName">The Assistant Composer's name for the clef (e.g. "t1")</param>
        /// <returns></returns>
        private string GetClichtCharacterString(string clefName)
        {
            string cLichtCharacterString = null;
            switch(clefName)
            {
                #region clefs
                case "t": // trebleClef
                case "t1": // trebleClef8
                case "t2": // trebleClef2x8
                case "t3": // trebleClef3x8
                    // N.B. t1, t2 and t3 are realised as <def> objects in combination with texts 8, 2x8 and 3x8.
                    // cLicht's trebleclefoctavaalt character is not used.
                    cLichtCharacterString = "&";
                    break;
                case "b":
                case "b1": // bassClef8
                case "b2": // bassClef2x8
                case "b3": // bassClef3x8
                    // N.B. b1, b2 and b3 are realised as <def> objects in combination with texts 8, 2x8 and 3x8.
                    // cLicht's bassclefoctavaalt character is not used.
                    cLichtCharacterString = "?";
                    break;
                #endregion
            }
            return cLichtCharacterString;
        }
        /// <summary>
        /// Rests and noteheads
        /// </summary>
        /// <param name="durationClass"></param>
        /// <returns></returns>
        private string GetClichtCharacterString(DurationClass durationClass, bool isRest)
        {
            string cLichtCharacterString = null;
            if(isRest)
            {
                switch(durationClass)
                {
                    #region rests
                    case DurationClass.breve:
                    case DurationClass.semibreve:
                        cLichtCharacterString = "∑";
                        break;
                    case DurationClass.minim:
                        cLichtCharacterString = "Ó";
                        break;
                    case DurationClass.crotchet:
                        cLichtCharacterString = "Œ";
                        break;
                    case DurationClass.quaver:
                        cLichtCharacterString = "‰";
                        break;
                    case DurationClass.semiquaver:
                        cLichtCharacterString = "≈";
                        break;
                    case DurationClass.threeFlags:
                        cLichtCharacterString = "®";
                        break;
                    case DurationClass.fourFlags:
                        cLichtCharacterString = "Ù";
                        break;
                    case DurationClass.fiveFlags:
                        cLichtCharacterString = "Â";
                        break;
                    #endregion
                }
            }
            else
            {
                switch(durationClass)
                {
                    case DurationClass.breve:
                        cLichtCharacterString = "›";
                        break;
                    case DurationClass.semibreve:
                        cLichtCharacterString = "w";
                        break;
                    case DurationClass.minim:
                        cLichtCharacterString = "˙";
                        break;
                    default:
                        cLichtCharacterString = "œ";
                        break;
                }
            }
            return cLichtCharacterString;
        }
        /// <summary>
        /// Accidentals
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetClichtCharacterString(Head head)
        {
            string cLichtCharacterString = null;
            switch(head.Alteration)
            {
                case -1:
                    cLichtCharacterString = "b";
                    break;
                case 0:
                    cLichtCharacterString = "n";
                    break;
                case 1:
                    cLichtCharacterString = "#";
                    break;
                default:
                    Debug.Assert(false, "unknown accidental type");
                    break;
            }
            return cLichtCharacterString;
        }

        public float FontHeight { get { return _fontHeight; } }
        protected float _fontHeight;
        protected TextHorizAlign _textHorizAlign = TextHorizAlign.left;
    }

    internal class ClefMetrics : Metrics // defined objects in SVG
    {
        public ClefMetrics(ClefSymbol clef, float gap)
            : base()
        {
            float trebleTop = -4.35F * gap;
            float trebleRight = 3.1F * gap;
            float highTrebleTop = -5.9F * gap;
            float trebleBottom = 2.7F * gap;
            #region treble clefs
            switch(clef.ClefType)
            {
                case "t":
                    _objectType = "trebleClef";
                    _top = trebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t1": // trebleClef8
                    _objectType = "trebleClef8";
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t2": // trebleClef2x8
                    _objectType = "trebleClef2x8";
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t3": // trebleClef3x8
                    _objectType = "trebleClef3x8";
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                default: // can be a bass clef ( see below)
                    break;
            }
 
            if(_right > 0F)
            {
                Move(0F, 3 * gap);
            }
            #endregion treble clefs

            if(!(_right > 0F))
            {
                float bassTop = -gap;
                float bassRight = trebleRight;
                float bassBottom = gap * 3F;
                float lowBassBottom = gap * 4.5F;
                #region bass clefs
                switch(clef.ClefType)
                {
                    case "b":
                        _objectType = "bassClef";
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = bassBottom;
                        break;
                    case "b1": // bassClef8
                        _objectType = "bassClef8";
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    case "b2": // bassClef2x8
                        _objectType = "bassClef2x8";
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    case "b3": // bassClef3x8
                        _objectType = "bassClef3x8";
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    default:
                        Debug.Assert(false, "Unknown clef type.");
                        break;
                }
                if(_right > 0F)
                {
                    Move(0, gap);
                }

            }
            #endregion

            FontHeight = clef.FontHeight;

        }
        public readonly float FontHeight;
    }
    internal class RestMetrics : CLichtCharacterMetrics
    {
        public RestMetrics(Graphics graphics, RestSymbol rest, float gap, int numberOfStafflines, float ledgerlineStrokeWidth)
            : base(rest.DurationClass, true, rest.FontHeight)
        {
            float dy = 0;
            if(numberOfStafflines > 1)
                dy = gap * (numberOfStafflines / 2);

            _top = _top + dy;
            _bottom += dy;
            _originY += dy; // the staffline on which the rest is aligned
            _ledgerlineStub = gap * 0.75F;
            Move((Left - Right) / 2F, 0F); // centre the glyph horizontally
            switch(rest.DurationClass)
            {
                case DurationClass.breve:
                case DurationClass.semibreve:
                    Move(gap * -0.25F, 0F);
                    if(numberOfStafflines == 1)
                        Move(0F, gap);
                    _ledgerline = new LedgerlineBlockMetrics(Left - _ledgerlineStub, Right + _ledgerlineStub, ledgerlineStrokeWidth);
                    _ledgerline.AddLedgerline(_originY - gap, 0F);
                    _ledgerline.Move(gap * 0.17F, 0F);
                    _top -=  (gap * 1.5F);
                    break;
                case DurationClass.minim:
                    Move(gap * 0.18F, 0);
                    _ledgerline = new LedgerlineBlockMetrics(Left - _ledgerlineStub, Right + _ledgerlineStub - (gap*0.3F), ledgerlineStrokeWidth);
                    _ledgerline.AddLedgerline(_originY, 0F);
                    _bottom += (gap * 1.5F);
                    break;
                case DurationClass.quaver:
                    _top -= gap * 0.5F;
                    _bottom += gap * 0.5F;
                    break;
                case DurationClass.semiquaver:
                    _top -= gap * 0.5F;
                    _bottom += gap * 0.5F;
                    break;
                case DurationClass.threeFlags:
                    _top -= gap * 0.5F;
                    _right += gap * 0.2F;
                    _bottom += gap * 0.5F;
                    _left -= gap * 0.2F;
                    break;
                case DurationClass.fourFlags:
                    _top -= gap * 0.5F;
                    _right += gap * 0.1F;
                    _bottom += gap * 1.25F;
                    _left -= gap * 0.1F;
                    _originY += gap;
                    break;
                case DurationClass.fiveFlags:
                    _top -= gap * 1.5F;
                    _right += gap * 0.2F;
                    _bottom += gap * 1.25F;
                    _left -= gap * 0.2F;
                    _originY += gap;
                    break;
            }

        }

        public override void Move(float dx, float dy)
        {
            base.Move(dx, dy);
            if(_ledgerline != null)
                _ledgerline.Move(dx, dy);
            if(_durationControlMetrics != null)
                _durationControlMetrics.Move(dx, dy);
        }

        public override void WriteSVG(SvgWriter w)
        {
            base.WriteSVG(w);
            if(_ledgerline != null && _ledgerlineVisible)
                _ledgerline.WriteSVG(w);
            if(_durationControlMetrics != null)
                _durationControlMetrics.WriteSVG(w);
        }

        /// <summary>
        /// Ledgerlines exist in breve, semibreve and minim rests.
        /// They are made visible when the rest is moved outside the staff on 2-voice staves.
        /// </summary>
        public bool LedgerlineVisible
        {
            set
            {
                if(_ledgerline != null && (!_ledgerlineVisible && value))
                {
                    float width = _ledgerline.Right - _ledgerline.Left;
                    float padding = width * 0.05F;
                    _left -= (_ledgerlineStub + padding);
                    _right += _ledgerlineStub + padding;
                    _ledgerlineVisible = value;
                }
            }
        }
        private float _ledgerlineStub;
        private bool _ledgerlineVisible = false;
        private LedgerlineBlockMetrics _ledgerline = null;
        private GroupMetrics _durationControlMetrics = null;
    }
    internal class HeadMetrics : CLichtCharacterMetrics
    {
        public HeadMetrics(ChordSymbol chord, float gapVBPX)
            : base(chord.DurationClass, false, chord.FontHeight)
        {
            Move((Left - Right) / 2F, 0F); // centre horizontally

            float horizontalPadding = chord.FontHeight * 0.04F;
            _leftStemX = _left;
            _rightStemX = _right;
            _left -= horizontalPadding;
            _right += horizontalPadding; 
        }

        /// <summary>
        /// Used when creating temporary heads for chord alignment purposes.
        /// </summary>
        public HeadMetrics(HeadMetrics otherHead, DurationClass durationClass)
            : base(durationClass, false, otherHead.FontHeight)
        {
            // move to position of other head
            Move(otherHead.OriginX - _originX, otherHead.OriginY - OriginY);

            float horizontalPadding = otherHead.FontHeight * 0.04F;
            _leftStemX = _left;
            _rightStemX = _right;
            _left -= horizontalPadding;
            _right += horizontalPadding; 
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// Notehead metrics.Left and metrics.Right include horizontal padding,
        /// so head overlaps cannot be checked using the standard Metrics.Overlaps function.
        /// </summary>
        public bool OverlapsHead(HeadMetrics otherHeadMetrics)
        {
            // See the above constructor. Sorry, I didnt want to save the value in every Head!
            float thisHorizontalPadding = this._fontHeight * 0.04F;
            float thisRealLeft = _left + thisHorizontalPadding;
            float thisRealRight = _right - thisHorizontalPadding;

            float otherHorizontalPadding = otherHeadMetrics.FontHeight * 0.04F;
            float otherRealLeft = otherHeadMetrics.Left + thisHorizontalPadding;
            float otherRealRight = otherHeadMetrics.Right - thisHorizontalPadding;

            bool verticalOverlap = this.Bottom >= otherHeadMetrics.Top && this.Top <= otherHeadMetrics.Bottom;
            bool horizontalOverlap = thisRealRight >= otherRealLeft && thisRealLeft <= otherRealRight;

            return verticalOverlap && horizontalOverlap;
        }
        /// <summary>
        /// Notehead metrics.Left and metrics.Right include horizontal padding,
        /// so head overlaps cannot be checked using the standard Metrics.Overlaps function.
        /// </summary>
        public bool OverlapsStem(StemMetrics stemMetrics)
        {
            // See the above constructor. Sorry, I didnt want to save the value in every Head!
            float thisHorizontalPadding = this._fontHeight * 0.04F;
            float thisRealLeft = _left + thisHorizontalPadding;
            float thisRealRight = _right - thisHorizontalPadding;

            bool verticalOverlap = this.Bottom >= stemMetrics.Top && this.Top <= stemMetrics.Bottom;
            bool horizontalOverlap = thisRealRight >= stemMetrics.Left && thisRealLeft <= stemMetrics.Right;

            return verticalOverlap && horizontalOverlap;
        }

        public override void Move(float dx, float dy)
        {
            base.Move(dx, dy);
            _leftStemX += dx;
            _rightStemX += dx;
        }

        public float LeftStemX { get { return _leftStemX; } }
        private float _leftStemX;
        public float RightStemX { get { return _rightStemX; } }
        private float _rightStemX;
    }
    internal class AccidentalMetrics : CLichtCharacterMetrics
    {
        public AccidentalMetrics(Head head, float fontHeight, float gap)
            : base(head, fontHeight)
        {
            float verticalPadding = gap / 5;
            _top -= verticalPadding;
            _bottom += verticalPadding;

            switch(_objectType)
            {
                case "b":
                    _left -= gap * 0.2F;
                    _right += gap * 0.2F;
                    break;
                case "n":
                    _left -= gap * 0.2F;
                    _right += gap * 0.2F;
                    break;
                case "#":
                    _left -= gap * 0.1F;
                    _right += gap * 0.1F;
                    break;
                //case "#":
                //    _left -= gap * 0.05F;
                //    _right += gap * 0.05F;
                //    break;
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        
    }
    internal class DynamicMetrics : CLichtCharacterMetrics, ICloneable
    {
        /// <summary>
        /// clichtDynamics: { "Ø", "∏", "π", "p", "P", "F", "f", "ƒ", "Ï", "Î" };
        ///                  pppp, ppp,  pp,  p,   mp,  mf,  f,   ff, fff, ffff
        /// </summary>
        /// <param name="gap"></param>
        /// <param name="textInfo"></param>
        /// <param name="isBelow"></param>
        /// <param name="topBoundary"></param>
        /// <param name="bottomBoundary"></param>
        public DynamicMetrics(float gap, TextInfo textInfo, bool isBelow)
            : base(textInfo.Text, true, textInfo.FontHeight, TextHorizAlign.left)
        {
            // visually centre the "italic" dynamic characters
            if(textInfo.Text == "p" || textInfo.Text == "f") // p, f
            {
                Move(textInfo.FontHeight * 0.02F, 0F);
            }
            else if(textInfo.Text == "F") // mf
            {
                Move(textInfo.FontHeight * 0.1F, 0F);
            }
            else
            {
                Move(textInfo.FontHeight * 0.05F, 0F);
            }
            float dynamicWidth = Right - Left;
            float moveLeftDelta = -(dynamicWidth / 2F) - (0.25F * gap); // "centre" italics
            Move(moveLeftDelta, 0F);

            IsBelow = isBelow;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public bool IsBelow;
    }

    public class GroupMetrics : Metrics
    {
        public GroupMetrics(string ID_Type)
            : base()
        {
            _objectType = ID_Type;
        }

        public GroupMetrics(string ID_Type, Metrics metrics)
            : base()
        {
            MetricsList.Add(metrics);

            _objectType = ID_Type;
            _originX = metrics.OriginX;
            _originY = metrics.OriginY;
            _top = metrics.Top;
            _right = metrics.Right;
            _bottom = metrics.Bottom;
            _left = metrics.Left;
        }

        /// <summary>
        /// Adds the metrics to the MetricsList and includes it in this object's boundary.
        /// The boundary is used for collision checking. All objects which should move together with this object
        /// must be added to the MetricsList.
        /// </summary>
        /// <param name="metrics"></param>
        public virtual void Add(Metrics metrics)
        {
            MetricsList.Add(metrics);
            ResetBoundary();
        }

        public virtual void ResetBoundary()
        {
            _top = float.MaxValue;
            _right = float.MinValue;
            _bottom = float.MinValue;
            _left = float.MaxValue;
            foreach(Metrics metrics in MetricsList)
            {
                _top = _top < metrics.Top ? _top : metrics.Top;
                _right = _right > metrics.Right ? _right : metrics.Right;
                _bottom = _bottom > metrics.Bottom ? _bottom : metrics.Bottom;
                _left = _left < metrics.Left ? _left : metrics.Left;
            }
        }

        public override void Move(float dx, float dy)
        {
            base.Move(dx, dy);
            foreach(Metrics metrics in MetricsList)
            {
                metrics.Move(dx, dy);
            }
        }

        public override void WriteSVG(SvgWriter w)
        {
            WriteSVG(w, null);
        }

        public void WriteSVG(SvgWriter w, string id)
        {
            w.SvgStartGroup(null, null);
            foreach(Metrics metrics in MetricsList)
            {
                metrics.WriteSVG(w);
            }
            w.SvgEndGroup();
        }

        public readonly List<Metrics> MetricsList = new List<Metrics>(); 
    }
}
