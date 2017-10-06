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
    public class FlagsMetrics : Metrics
    {
        /// <summary>
        /// Should be called with a duration class having a flag block
        /// </summary>
        public FlagsMetrics(DurationClass durationClass, float fontHeight, VerticalDir stemDirection, bool isInput)
            : base(isInput ? CSSObjectClass.inputFlag : CSSObjectClass.flag)
        {
			_left = 0F;           
            
            // (0.31809F * fontHeight) is maximum x in the flag def.
            _right = (0.31809F * fontHeight);
            if(stemDirection == VerticalDir.up)
            {
                float rightPadding = (0.06F * fontHeight);
                _right += rightPadding;
            }
            
			_originX = 0F;
			_originY = 0F;

			float offset = 0F;
			switch(durationClass)
			{
				case DurationClass.quaver:
					if(stemDirection == VerticalDir.up)
						_flagID = isInput ? FlagID.inputRight1Flag : FlagID.right1Flag;
					else
						_flagID = isInput? FlagID.inputLeft1Flag : FlagID.left1Flag;
					break;
				case DurationClass.semiquaver:
					if(stemDirection == VerticalDir.up)
						_flagID = isInput? FlagID.inputRight2Flags : FlagID.right2Flags;
					else
						_flagID = isInput? FlagID.inputLeft2Flags : FlagID.left2Flags;
					offset = 0.25F;
					break;
				case DurationClass.threeFlags:
					if(stemDirection == VerticalDir.up)
						_flagID = isInput? FlagID.inputRight3Flags : FlagID.right3Flags;
					else
						_flagID = isInput? FlagID.inputLeft3Flags : FlagID.left3Flags;
					offset = 0.5F;
					break;
				case DurationClass.fourFlags:
					if(stemDirection == VerticalDir.up)
						_flagID = isInput? FlagID.inputRight4Flags : FlagID.right4Flags;
					else
						_flagID = isInput? FlagID.inputLeft4Flags : FlagID.left4Flags;
					offset = 0.75F;
					break;
				case DurationClass.fiveFlags:
					if(stemDirection == VerticalDir.up)
						_flagID = isInput? FlagID.inputRight5Flags : FlagID.right5Flags;
					else
						_flagID = isInput? FlagID.inputLeft5Flags : FlagID.left5Flags;
					offset = 1F;
					break;
				default:
					Debug.Assert(false, "This duration class has no flags.");
					break;
			}
			if(stemDirection == VerticalDir.up)
			{
				_top = 0F;
				_bottom = (0.2467F + offset) * fontHeight;
			}
			else
			{
				_top = (-(0.2467F + offset)) * fontHeight;
				_bottom = 0F;
			}

            if(!_usedFlagIDs.Contains((FlagID) _flagID))
            {
                _usedFlagIDs.Add((FlagID) _flagID);
            }
        }

        public static void ClearUsedFlagIDsList()
        {
            _usedFlagIDs.Clear();
        }

        public override void WriteSVG(SvgWriter w)
        { 
            string flagIDString = _flagID.ToString();

            if(flagIDString.Contains("ight")) // stemDirection is up
                w.SvgUseXY(CSSObjectClass, flagIDString, _left, _top);
            else
                w.SvgUseXY(CSSObjectClass, flagIDString, _left, _bottom);
        }

        public FlagID FlagID { get { return _flagID; } private set { _flagID = value; } }
        private FlagID _flagID = FlagID.none;
        public static IReadOnlyList<FlagID> UsedFlagIDs { get { return _usedFlagIDs as IReadOnlyList<FlagID>; } }
        private static List<FlagID> _usedFlagIDs = new List<FlagID>();
    }
}
