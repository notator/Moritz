using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Text;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    /// <summary>
    /// For objects that are defined in the SVG defs, and then "use"d.
    /// </summary>
    internal class MetricsForUse : Metrics
    {
        protected MetricsForUse()
            : base()
        {
        }

        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException("MetricsForUse.WriteSVG(w) should never be called."); 
        }

        protected void RecordUsedFlagID(string usedFlagID)
        {
            UseID = usedFlagID;
            if(!_usedFlagIDs.Contains(usedFlagID))
            {
                _usedFlagIDs.Add(usedFlagID);
            }
        }

        protected void RecordUsedClefID(string usedClefID)
        {
            UseID = usedClefID;
            if(!_usedClefIDs.Contains(usedClefID))
            {
                _usedClefIDs.Add(usedClefID);
            }
        }

        protected static List<string> _usedFlagIDs = new List<string>();
        public static IReadOnlyList<string> UsedFlagIDs { get { return _usedFlagIDs as IReadOnlyList<string>; } }

        protected static List<string> _usedClefIDs = new List<string>();
        public static IReadOnlyList<string> UsedClefIDs { get { return _usedClefIDs as IReadOnlyList<string>; } }

        /// <summary>
        /// The id of the object to Use (defined in the SVG [defs] element.
        /// </summary>
        public string UseID { get; private set; }
    }
    internal class FlagsBlockMetrics : MetricsForUse
	{
		/// <summary>
		/// Should be called with a duration class having a flag block
		/// </summary>
		public FlagsBlockMetrics(DurationClass durationClass, float fontHeight, VerticalDir stemDirection, bool isInput)
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
						RecordUsedFlagID(isInput ? "inputRight1Flag" : "right1Flag");
					else
						RecordUsedFlagID(isInput ? "inputLeft1Flag" : "left1Flag");
					break;
				case DurationClass.semiquaver:
					if(_stemDirection == VerticalDir.up)
						RecordUsedFlagID(isInput ? "inputRight2Flags" : "right2Flags");
					else
						RecordUsedFlagID(isInput ? "inputLeft2Flags" : "left2Flags");
					offset = 0.25F;
					break;
				case DurationClass.threeFlags:
					if(_stemDirection == VerticalDir.up)
						RecordUsedFlagID(isInput ? "inputRight3Flags" : "right3Flags");
					else
						RecordUsedFlagID(isInput ? "inputLeft3Flags" : "left3Flags");
					offset = 0.5F;
					break;
				case DurationClass.fourFlags:
					if(_stemDirection == VerticalDir.up)
						RecordUsedFlagID(isInput ? "inputRight4Flags" : "right4Flags");
					else
						RecordUsedFlagID(isInput ? "inputLeft4Flags" : "left4Flags");
					offset = 0.75F;
					break;
				case DurationClass.fiveFlags:
					if(_stemDirection == VerticalDir.up)
						RecordUsedFlagID(isInput ? "inputRight5Flags" : "right5Flags");
					else
						RecordUsedFlagID(isInput ? "inputLeft5Flags" : "left5Flags");
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
            throw new NotImplementedException();
        }
        
        public void WriteSVG(SvgWriter w, bool isInput)
        {
            CSSClass flagClass = isInput ? CSSClass.inputFlag : CSSClass.flag;

            if(_stemDirection == VerticalDir.up)
                w.SvgUseXY(flagClass, UseID, _left, _top);
            else
                w.SvgUseXY(flagClass, UseID, _left, _bottom);
        }

		private readonly float _fontHeight;
		private readonly VerticalDir _stemDirection;

	}
    internal class ClefMetrics : MetricsForUse // defined objects in SVG
    {
        public ClefMetrics(Clef clef, float gap, bool isInput)
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
                    RecordUsedClefID(isInput ? "inputTrebleClef" : "trebleClef");
                    _top = trebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t1": // trebleClef8
                    RecordUsedClefID(isInput ? "inputTrebleClef8" : "trebleClef8");
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t2": // trebleClef2x8
                    RecordUsedClefID(isInput ? "inputTrebleClef2x8" : "trebleClef2x8");
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t3": // trebleClef3x8
                    RecordUsedClefID(isInput ? "inputTrebleClef3x8" : "trebleClef3x8");
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
                        RecordUsedClefID(isInput ? "inputBassClef" : "bassClef");
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = bassBottom;
                        break;
                    case "b1": // bassClef8
                        RecordUsedClefID(isInput ? "inputBassClef8" : "bassClef8");
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    case "b2": // bassClef2x8
                        RecordUsedClefID(isInput ? "inputBassClef2x8" : "bassClef2x8");
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    case "b3": // bassClef3x8
                        RecordUsedClefID(isInput ? "inputBassClef3x8" : "bassClef3x8");
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

        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }

        public readonly float FontHeight;
    }
    internal class SmallClefMetrics : MetricsForUse
    {
        public SmallClefMetrics(Clef clef, float gap, bool isInput)
            : base()
        {
            string id;
            float trebleTop = -4.35F * gap;
            //float trebleRight = 3.1F * gap; // ordinary clefs
            float trebleRight = 3.5F * gap; // small clefs have proportionally more empty space on the right.
            float highTrebleTop = -5.9F * gap;
            float trebleBottom = 2.7F * gap;
            #region treble clefs
            switch(clef.ClefType)
            {
                case "t":
                    id = isInput ? "inputSmallTrebleClef" : "smallTrebleClef";
                    RecordUsedClefID(id);
                    _top = trebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t1": // trebleClef8
                    id = isInput ? "inputSmallTrebleClef8" : "smallTrebleClef8";
                    RecordUsedClefID(id);
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t2": // trebleClef2x8
                    id = isInput ? "inputSmallTrebleClef2x8" : "smallTrebleClef2x8";
                    RecordUsedClefID(id);
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t3": // trebleClef3x8
                    id = isInput ? "inputSmallTrebleClef3x8" : "smallTrebleClef3x8";
                    RecordUsedClefID(id);
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
                //float lowBassBottom = gap * 4.5F;
                float lowBassBottom = gap * 4.65F; // small bass clef octaves are lower than for normal bass clefs
                #region bass clefs
                switch(clef.ClefType)
                {
                    case "b":
                        id = isInput ? "inputSmallBassClef" : "smallBassClef";
                        RecordUsedClefID(id);
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = bassBottom;
                        break;
                    case "b1": // bassClef8
                        id = isInput ? "inputSmallBassClef8" : "smallBassClef8";
                        RecordUsedClefID(id);
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    case "b2": // bassClef2x8
                        id = isInput ? "inputSmallBassClef2x8" : "smallBassClef2x8";
                        RecordUsedClefID(id);
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    case "b3": // bassClef3x8
                        id = isInput ? "inputSmallBassClef3x8" : "smallBassClef3x8";
                        RecordUsedClefID(id);
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

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgUseXY(CSSClass.smallClef, UseID, _originX, _originY);
        }

        public readonly float FontHeight;
    }
}
