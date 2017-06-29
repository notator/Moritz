 using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

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

        protected void SetUseID(string useID)
        {
            UseID = useID;
            if(!_usedIDs.Contains(useID))
            {
                _usedIDs.Add(useID);
            }
        }

        protected static List<string> _usedIDs = new List<string>();
        public static IReadOnlyList<string> UsedIDs { get { return _usedIDs as IReadOnlyList<string>; } }

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
						SetUseID("Right1Flag");
					else
						SetUseID("Left1Flag");
					break;
				case DurationClass.semiquaver:
					if(_stemDirection == VerticalDir.up)
						SetUseID("Right2Flags");
					else
						SetUseID("Left2Flags");
					offset = 0.25F;
					break;
				case DurationClass.threeFlags:
					if(_stemDirection == VerticalDir.up)
						SetUseID("Right3Flags");
					else
						SetUseID("Left3Flags");
					offset = 0.5F;
					break;
				case DurationClass.fourFlags:
					if(_stemDirection == VerticalDir.up)
						SetUseID("Right4Flags");
					else
						SetUseID("Left4Flags");
					offset = 0.75F;
					break;
				case DurationClass.fiveFlags:
					if(_stemDirection == VerticalDir.up)
						SetUseID("Right5Flags");
					else
						SetUseID("Left5Flags");
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
                w.SvgUseXY(CSSClass.flag, UseID, _left, _top);
            else
                w.SvgUseXY(CSSClass.flag, UseID, _left, _bottom);
        }

		private readonly float _fontHeight;
		private readonly VerticalDir _stemDirection;

	}
    internal class ClefMetrics : MetricsForUse // defined objects in SVG
    {
        public ClefMetrics(Clef clef, float gap)
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
                    SetUseID("trebleClef");
                    _top = trebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t1": // trebleClef8
                    SetUseID("trebleClef8");
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t2": // trebleClef2x8
                    SetUseID("trebleClef2x8");
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t3": // trebleClef3x8
                    SetUseID("trebleClef3x8");
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
                        SetUseID("bassClef");
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = bassBottom;
                        break;
                    case "b1": // bassClef8
                        SetUseID("bassClef8");
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    case "b2": // bassClef2x8
                        SetUseID("bassClef2x8");
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    case "b3": // bassClef3x8
                        SetUseID("bassClef3x8");
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
            w.SvgUseXY(CSSClass.clef, UseID, _originX, _originY);
        }

        public readonly float FontHeight;
    }
    internal class SmallClefMetrics : MetricsForUse
    {
        public SmallClefMetrics(Clef clef, float gap)
            : base()
        {
            float trebleTop = -4.35F * gap;
            //float trebleRight = 3.1F * gap; // ordinary clefs
            float trebleRight = 3.5F * gap; // cautionary clefs have proportionally more empty space on the right.
            float highTrebleTop = -5.9F * gap;
            float trebleBottom = 2.7F * gap;
            #region treble clefs
            switch(clef.ClefType)
            {
                case "t":
                    SetUseID("cautionaryTrebleClef");
                    _top = trebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t1": // trebleClef8
                    SetUseID("cautionaryTrebleClef8");
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t2": // trebleClef2x8
                    SetUseID("cautionaryTrebleClef2x8");
                    _top = highTrebleTop;
                    _right = trebleRight;
                    _bottom = trebleBottom;
                    break;
                case "t3": // trebleClef3x8
                    SetUseID("cautionaryTrebleClef3x8");
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
                float lowBassBottom = gap * 4.65F; // cautionary bass clef octaves are lower than for normal bass clefs
                #region bass clefs
                switch(clef.ClefType)
                {
                    case "b":
                        SetUseID("cautionaryBassClef");
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = bassBottom;
                        break;
                    case "b1": // bassClef8
                        SetUseID("cautionaryBassClef8");
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    case "b2": // bassClef2x8
                        SetUseID("cautionaryBassClef2x8");
                        _top = bassTop;
                        _right = bassRight;
                        _bottom = lowBassBottom;
                        break;
                    case "b3": // bassClef3x8
                        SetUseID("cautionaryBassClef3x8");
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
            w.SvgUseXY(CSSClass.clef, UseID, _originX, _originY);
        }

        public readonly float FontHeight;
    }
}
