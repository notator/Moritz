using System;
using System.Collections.Generic;

using Moritz.Xml;

namespace Moritz.Symbols
{
    /// <summary>
    /// Positions in pixels at font size 1px
    /// Note that the stem must still be inset by half its width towards the centre of the notehead.
    /// </summary>
    internal class NoteheadStemPositions_px
    {
        public float LeftStemX_px;
        public float LeftStemY_px;
        public float RightStemX_px;
        public float RightStemY_px;
    }


    /// <summary>
    /// To find the actual pixel dimensions of a character, multiply the values
    /// in the public dictionaries by the character's font size. 
    /// </summary>
    public static class CLichtFontMetrics
    {
        /// <summary>
        /// This class is used to construct the CLichtFontMetrics.CLichtGlyphBoundingBoxesDictPX dictionary.
        /// </summary>
        internal class CLichtGlyphBoxMetric : Metrics
        {
            public CLichtGlyphBoxMetric(float top, float right, float bottom, float left)
                : base(CSSObjectClass.none)
            {
                _top = top;
                _right = right;
                _bottom = bottom;
                _left = left;
            }

            public override void WriteSVG(SvgWriter w)
            {
                throw new NotImplementedException(); 
            }
        }

        static CLichtFontMetrics()
        {
            float hScale = 3.4037F / 800F;
            float vScale = 2.9464F / 800F;
            foreach(string key in CLichtGlyphBoundingBoxesDictMM.Keys)
            {
                GlyphBoundingBoxMM gbbMM = CLichtGlyphBoundingBoxesDictMM[key];
                float top_1px = (float) gbbMM.mmTop * vScale;
                float left_1px = (float) gbbMM.mmLeft * hScale;
                float bottom_1px = (float) gbbMM.mmBottom * vScale;
                float right_1px = (float) gbbMM.mmRight * hScale;
                CLichtGlyphBoxMetric metrics_1px =
                   new CLichtGlyphBoxMetric(top_1px, right_1px, bottom_1px, left_1px);
                CLichtGlyphBoundingBoxesDictPX.Add(key, metrics_1px);
            }
            foreach(string key in ClichtNoteheadStemPositionsDictMM.Keys)
            {
                NoteheadStemPositionsMM nspMM = ClichtNoteheadStemPositionsDictMM[key];
                NoteheadStemPositions_px nspPX = new NoteheadStemPositions_px()
                {
                    LeftStemX_px = (float) nspMM.mmLeftStemX * hScale,
                    LeftStemY_px = (float) nspMM.mmLeftStemY * vScale,
                    RightStemX_px = (float) nspMM.mmRightStemX * hScale,
                    RightStemY_px = (float) nspMM.mmRightStemY * vScale
                };
                ClichtNoteheadStemPositionsDictPX.Add(key, nspPX);
            }
        }

        #region public
        internal static Dictionary<string, Metrics> CLichtGlyphBoundingBoxesDictPX = new Dictionary<string, Metrics>();
        internal static Dictionary<string, NoteheadStemPositions_px> ClichtNoteheadStemPositionsDictPX = new Dictionary<string, NoteheadStemPositions_px>();
        #endregion public
        #region private
        /// <summary>
        /// These metrics were found by hand as follows:
        /// A box 1000x800px was drawn in svg, and measured physically on screen (in Firefox)
        ///    The result was 29.38cm x 23.57cm
        /// Horizontally there were therefore 3.4037 pixels per millimetre.
        /// Vertically there were therefore 2.9464 pixels per millimetre.
        /// The characters were then displayed on screen at font-size 800px and measured by hand.
        /// mmLeft and mmRight values are negative if the position was left of the character's origin.
        /// mmTop and mmBottom values are negative if the position was above the character's origin.
        /// 
        /// So, to find the actual pixel dimensions of a character, use the following formula:
        ///     horizontal pixels = (mm in this dictionary * 3.4037) * (font size) / 800
        ///     vertical pixels = (mm in this dictionary * 2.9464) * (font size) / 800
        /// </summary>
        private static Dictionary<string, GlyphBoundingBoxMM> CLichtGlyphBoundingBoxesDictMM = new Dictionary<string, GlyphBoundingBoxMM>()
        {
            #region noteheads
            { "›", new GlyphBoundingBoxMM() {mmLeft=-22.5, mmRight=91, mmTop=-30, mmBottom=30}}, // breve
            { "w", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=101.5, mmTop=-30, mmBottom=30}}, // semibreve
            { "˙", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=69, mmTop=-30, mmBottom=30}}, // minim
            { "œ", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=69, mmTop=-30, mmBottom=30}}, // crotchet
            { "O", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=70, mmTop=-33, mmBottom=33}}, // whitediamond
            { "¥", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=70, mmTop=-33, mmBottom=33}}, // blackdiamond
            { "D", new GlyphBoundingBoxMM() {mmLeft=-49.5, mmRight=118.5, mmTop=-52, mmBottom=52}}, // slashedbreve
            { "o", new GlyphBoundingBoxMM() {mmLeft=-33, mmRight=135, mmTop=-52, mmBottom=52}}, // slashedsemibreve
            { "¿", new GlyphBoundingBoxMM() {mmLeft=-50, mmRight=118, mmTop=-52, mmBottom=52}}, // slashedminim
            { "ä", new GlyphBoundingBoxMM() {mmLeft=-50, mmRight=118, mmTop=-52, mmBottom=52}}, // slashedcrotchet
            #endregion
            #region clefs
            { "†", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=149, mmTop=-326, mmBottom=155}}, // trebleclefoctavaalt
            { "&", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=149, mmTop=-260, mmBottom=155}}, // trebleclef
            { "V", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=149, mmTop=-260, mmBottom=233}}, // trebleclefoctavabassa
            { "B", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=160, mmTop=-119, mmBottom=119}}, // altoclef
            { "?", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=144, mmTop=-59, mmBottom=138}}, // bassclef
            { "t", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=144, mmTop=-59, mmBottom=223}}, // bassclefoctavabassa
            { "√", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=197, mmTop=-111, mmBottom=3}}, // octavaalt
            { "◊", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=197, mmTop=-111, mmBottom=3}}, // octavabassa
            #endregion
            #region rests
            { "∑", new GlyphBoundingBoxMM() {mmLeft=-1.5, mmRight=70, mmTop=-30, mmBottom=0}}, // semibreve
            { "Ó", new GlyphBoundingBoxMM() {mmLeft=10, mmRight=79, mmTop=-60, mmBottom=-31}}, // minim
            { "Œ", new GlyphBoundingBoxMM() {mmLeft=8, mmRight=65, mmTop=-89, mmBottom=84}}, // crotchet
            { "‰", new GlyphBoundingBoxMM() {mmLeft=5.5, mmRight=64.5, mmTop=-45, mmBottom=70}}, // quaver
            { "≈", new GlyphBoundingBoxMM() {mmLeft=-2, mmRight=72, mmTop=-45, mmBottom=129}}, // semiquaver
            { "®", new GlyphBoundingBoxMM() {mmLeft=-7, mmRight=76.5, mmTop=-104, mmBottom=131}}, // threeflags
            { "Ù", new GlyphBoundingBoxMM() {mmLeft=-8, mmRight=77.5, mmTop=-163, mmBottom=128}}, // fourflags
            { "Â", new GlyphBoundingBoxMM() {mmLeft=-6, mmRight=78, mmTop=-224, mmBottom=129}}, // fiveflags
            #endregion
            #region flags
            { "J", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=79, mmTop=0, mmBottom=96}}, // flagonleftstem 
            { "j", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=79, mmTop=-96, mmBottom=0}}, // flagonrightstem 
            #endregion
            #region accidentals
            { "‹", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=60, mmTop=-30, mmBottom=30}}, // doublesharp 
            { "#", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=58, mmTop=-77, mmBottom=77}}, // sharp 
            { "˝", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=67, mmTop=-77, mmBottom=149}}, // sharparrowdown 
            { "¸", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=67, mmTop=-149, mmBottom=77}}, // sharparrowup 
            { "–", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=98, mmTop=-110, mmBottom=77}}, // sharparrowrightlow 
            { "—", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=98, mmTop=-139, mmBottom=77}}, // sharparrowrighthigh 
            { "`", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=58, mmTop=-53, mmBottom=53}}, // quartersharp 
            { "‚", new GlyphBoundingBoxMM() {mmLeft=2, mmRight=51.5, mmTop=-48, mmBottom=46}}, // arrowup 
            { "n", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=36, mmTop=-77, mmBottom=77}}, // natural 
            { "„", new GlyphBoundingBoxMM() {mmLeft=2, mmRight=51.5, mmTop=-45, mmBottom=49}}, // arrowdown 
            { "b", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=51, mmTop=-101, mmBottom=41}}, // flat 
            { "ˇ", new GlyphBoundingBoxMM() {mmLeft=-21.5, mmRight=51, mmTop=-101, mmBottom=119}}, // flatarrowdown 
            { "˛", new GlyphBoundingBoxMM() {mmLeft=-21.5, mmRight=51, mmTop=-149, mmBottom=41}}, // flatarrowup 
            { "‡", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=87, mmTop=-110, mmBottom=41}}, // flatarrowrightlow 
            { "·", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=87, mmTop=-139, mmBottom=41}}, // flatarrowrighthigh 
            { "R", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=76.5, mmTop=-101, mmBottom=41}}, // quarterflat 
            { "∫", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=94.5, mmTop=-101, mmBottom=41}}, // doubleflat  
            #endregion
            #region dynamics
            { "Ø", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=458, mmTop=-82, mmBottom=45}}, // pppp 
            { "∏", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=353, mmTop=-82, mmBottom=45}}, // ppp 
            { "π", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=249, mmTop=-82, mmBottom=45}}, // pp 
            { "p", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=144, mmTop=-82, mmBottom=45}}, // p 
            { "P", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=250, mmTop=-82, mmBottom=45}}, // mp 
            { "F", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=254, mmTop=-128, mmBottom=50}}, // mf 
            { "f", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=150, mmTop=-128, mmBottom=50}}, // f 
            { "ƒ", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=229, mmTop=-128, mmBottom=50}}, // ff 
            { "Ï", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=308, mmTop=-128, mmBottom=50}}, // fff 
            { "Î", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=387, mmTop=-128, mmBottom=50}}, // ffff 
            { "S", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=174, mmTop=-128, mmBottom=50}}, // sf 
            { "Z", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=189, mmTop=-128, mmBottom=50}}, // fz 
            { "Ç", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=267, mmTop=-128, mmBottom=50}}, // sfp 
            { "Í", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=242, mmTop=-128, mmBottom=50}}, // fp 
            { "ß", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=214, mmTop=-128, mmBottom=50}}, // sfz 
            { "ç", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=293, mmTop=-128, mmBottom=50}}, // sffz 
            #endregion
            #region articulations
            { "è", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=26.5, mmTop=-77, mmBottom=0}}, // grapepipabove 
            { "é", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=26.5, mmTop=-77, mmBottom=0}}, // grapepipbelow 
            { "+", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=69.5, mmTop=-340, mmBottom=340}}, // handstopped 
            { "˜", new GlyphBoundingBoxMM() {mmLeft=+0.5, mmRight=39.5, mmTop=-19.25, mmBottom=19.25}}, // harmonicring (seems to contain a backspace) 
            { "-", new GlyphBoundingBoxMM() {mmLeft=+16, mmRight=103.5, mmTop=-16.5, mmBottom=0}}, // tenuto 
            { ".", new GlyphBoundingBoxMM() {mmLeft=+8, mmRight=31, mmTop=-11.5, mmBottom=11.5}}, // staccato 
            { "Æ", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=38, mmTop=-67, mmBottom=0}}, // wedgeabove 
            { "'", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=38, mmTop=-67, mmBottom=0}}, // wedgebelow 
            { ">", new GlyphBoundingBoxMM() {mmLeft=16, mmRight=104, mmTop=-91, mmBottom=-10}}, // accent 
            { "T", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=147, mmTop=-35, mmBottom=36}}, // turn 
            { "ÿ", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=147, mmTop=-35, mmBottom=36}}, // reverseturn 
            { "Ÿ", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=128.5, mmTop=-97, mmBottom=1}}, // trill 
            { "~", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=91, mmTop=-72, mmBottom=-34}}, // trillwiggle 
            { "U", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=181, mmTop=-113.5, mmBottom=0}}, // fermataabove 
            { "u", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=181, mmTop=0, mmBottom=113.5}}, // fermatabelow 
            { "^", new GlyphBoundingBoxMM() {mmLeft=4.5, mmRight=106, mmTop=-120, mmBottom=0}}, // martellatoabove 
            { "v", new GlyphBoundingBoxMM() {mmLeft=4.5, mmRight=106, mmTop=-120, mmBottom=0}}, // martellatobelow 
            { "≤", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=55, mmTop=-117, mmBottom=0}}, // upbow 
            { "≥", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=79.5, mmTop=-90, mmBottom=-1}}, // downbow  
            #endregion
            #region miscellaneous
            { "\"", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=103, mmTop=-264, mmBottom=0}}, // breathmark 
            { ",", new GlyphBoundingBoxMM() {mmLeft=+1, mmRight=112, mmTop=-191.5, mmBottom=25.5}}, // comma 
            { "!", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=74.5, mmTop=-44.5, mmBottom=0}}, // onebartrem 
            { "@", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=74.5, mmTop=-89, mmBottom=0}}, // twobartrem 
            { "æ", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=74.5, mmTop=-134, mmBottom=0}}, // threebartrem 
            { "°", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=190, mmTop=-119, mmBottom=1}}, // ped 
            { "*", new GlyphBoundingBoxMM() {mmLeft=0, mmRight=87, mmTop=-89, mmBottom=-1}}, // pedupstar 
            { "g", new GlyphBoundingBoxMM() {mmLeft=+2, mmRight=31.5, mmTop=-132, mmBottom=0.5}}, // arpeggiowiggle 
            { "Ò", new GlyphBoundingBoxMM() {mmLeft=-13.5, mmRight=47.5, mmTop=-132, mmBottom=-20}}, // arpeggioarrowdown 
            { "Û", new GlyphBoundingBoxMM() {mmLeft=-13.5, mmRight=47.5, mmTop=-113, mmBottom=0}} // arpeggioarrowup 
            #endregion
        };
        private static Dictionary<string, NoteheadStemPositionsMM> ClichtNoteheadStemPositionsDictMM = new Dictionary<string, NoteheadStemPositionsMM>()
        {
            { "˙", new NoteheadStemPositionsMM() {mmLeftStemX=0, mmLeftStemY=8, mmRightStemX=69, mmRightStemY=-8}}, // minim
            { "œ", new NoteheadStemPositionsMM() {mmLeftStemX=0, mmLeftStemY=8, mmRightStemX=69, mmRightStemY=-8}}, // crotchet
            { "¿", new NoteheadStemPositionsMM() {mmLeftStemX=0, mmLeftStemY=8, mmRightStemX=69, mmRightStemY=-8}}, // slashedminim
            { "ä", new NoteheadStemPositionsMM() {mmLeftStemX=0, mmLeftStemY=8, mmRightStemX=69, mmRightStemY=-8}}, // slashedcrotchet
            { "O", new NoteheadStemPositionsMM() {mmLeftStemX=0, mmLeftStemY=0, mmRightStemX=69, mmRightStemY=0}}, // whitediamond
            { "¥", new NoteheadStemPositionsMM() {mmLeftStemX=0, mmLeftStemY=0, mmRightStemX=69, mmRightStemY=0}}, // blackdiamond
        };

        private class GlyphBoundingBoxMM
        {
            public double mmTop;
            public double mmLeft;
            public double mmBottom;
            public double mmRight;
        }
        /// <summary>
        /// Note that the stem must still be inset by half its width towards the centre of the notehead.
        /// </summary>
        private class NoteheadStemPositionsMM
        {
            public double mmLeftStemX;
            public double mmLeftStemY;
            public double mmRightStemX;
            public double mmRightStemY;
        }
        #endregion private
    }
}
