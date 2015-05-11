
using System.Collections.Generic;

namespace Moritz.Symbols
{
    /// <summary>
    /// Public values are in viewbox pixel units.
    /// </summary>
    public class PageFormat
    {
        public PageFormat()
        {
        }

        public readonly int ViewBoxMagnification = 8;

        #region paper size
        public float Right { get { return RightVBPX; } }
        public float Bottom 
        { 
            get 
            { 
                int nGaps = (int)( BottomVBPX / Gap);
                return nGaps * Gap;
            }
        }
        public float ScreenRight { get { return RightVBPX / ViewBoxMagnification; } }
        public float ScreenBottom { get { return BottomVBPX / ViewBoxMagnification; } }

        public string PaperSize; // default
        public bool IsLandscape = false;
        public int RightVBPX = 0;
        public int BottomVBPX = 0;
        public readonly float HorizontalPixelsPerMillimeter = 3.4037F; // on my computer (December 2010).
        public readonly float VerticalPixelsPerMillimeter = 2.9464F; // on my computer (December 2010).
        #endregion

        #region title size and position
        public float Page1ScreenTitleY { get { return Page1TitleY / ViewBoxMagnification; } }
        public float Page1TitleHeight;
        public float Page1AuthorHeight;
        public float Page1TitleY;
        #endregion

        #region frame
        public float LeftScreenMarginPos { get { return LeftMarginPos / ViewBoxMagnification; } }
        public int FirstPageFrameHeight { get { return BottomMarginPos - TopMarginPage1; } }
        public int OtherPagesFrameHeight { get { return BottomMarginPos - TopMarginOtherPages; } }
        public int TopMarginPage1;
        public int TopMarginOtherPages;
        public int RightMarginPos;
        public int LeftMarginPos;
        public int BottomMarginPos;
        #endregion

        #region website links
        /// <summary>
        /// the text of the link to the "about" file
        /// </summary>
        public string AboutLinkText;
        /// <summary>
        /// the "about" file's URL.
        /// </summary>
        public string AboutLinkURL;
        #endregion

        #region notation
        /// <summary>
        /// All written scores set the ChordSymbolType to one of the values in M.ChordTypes.
        /// M.ChordTypes does not include "none", because it is used to populate ComboBoxes.
        /// The value "none" is used to signal that there is no written score. It is used by
        /// AudioButtonsControl inside palettes, just to play a sound.
        /// </summary>
        public string ChordSymbolType = "none";
        #region standard chord notation options
        /// <summary>
        /// This value is used to find the duration class of DurationSymbols
        /// </summary>
        public int MinimumCrotchetDuration;
        public bool BeamsCrossBarlines;
        #endregion
        public float StafflineStemStrokeWidth;
        public float Gap;
        /// <summary>
        /// The view box pixel distance between staves when they are not vertically justified.
        /// </summary>
        public float DefaultDistanceBetweenStaves;
        /// <summary>
        /// The view box pixel distance between systems when they are not vertically justified.
        /// </summary>
        public float DefaultDistanceBetweenSystems;
        public List<List<byte>> VisibleOutputVoiceIndicesPerStaff = null;
        public List<List<byte>> VisibleInputVoiceIndicesPerStaff = null;        
        public List<string> ClefsList = null;
        public List<int> StafflinesPerStaff = null;
        public List<int> StaffGroups = null;
        public List<string> LongStaffNames = null;
        public List<string> ShortStaffNames = null;

        public List<int> SystemStartBars = null;
        public int DefaultNumberOfBarsPerSystem { get { return 5; } }
        public float StaffNameFontHeight { get { return Gap * 2.5F; } }
        /// <summary>
        /// the normal font size on staves having Gap sized spaces (after experimenting with cLicht). 
        /// </summary>
        public float MusicFontHeight { get { return (Gap * 4) * 0.98F; } }
        public float CautionaryNoteheadsFontHeight { get { return MusicFontHeight * 0.8F; } }
        public float InputStavesSizeFactor { get { return 1.5F; } }
        public float BarlineStrokeWidth { get { return StafflineStemStrokeWidth * 2F; } }
        public float NoteheadExtenderStrokeWidth { get { return StafflineStemStrokeWidth * 3.4F; } }
        public float BeamThickness { get { return Gap * 0.42F; } }
        #endregion

        #region derived properties
        /// <summary>
        /// A list having one value per staff in the system
        /// </summary>
        public List<bool> BarlineContinuesDownList
        {
            get
            {
                List<bool> barlineContinuesDownPerStaff = new List<bool>();
                foreach(int nStaves in StaffGroups)
                {
                    int remainingStavesInGroup = nStaves - 1;
                    while(remainingStavesInGroup > 0)
                    {
                        barlineContinuesDownPerStaff.Add(true);
                        --remainingStavesInGroup;
                    }
                    barlineContinuesDownPerStaff.Add(false);
                }
                return barlineContinuesDownPerStaff;
            }
        }
        #endregion derived properties

    }
}
