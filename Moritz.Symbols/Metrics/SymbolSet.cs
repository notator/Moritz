using System.Drawing;
using System.Collections.Generic;

using Moritz.Xml;
using Moritz.VoiceDef;

namespace Moritz.Symbols
{
    public abstract class SymbolSet
    {
        protected SymbolSet()
        {
        }

        public abstract void WriteSymbolDefinitions(SvgWriter w);
        public abstract Metrics NoteObjectMetrics(Graphics graphics, NoteObject noteObject, VerticalDir voiceStemDirection, float gap, float storkeWidth);
        public abstract NoteObject GetNoteObject(Voice voice, IUniqueDef iud, bool firstLmddInVoice,
            ref byte currentVelocity, float musicFontHeight);

        /// <summary>
        /// In the StandardSymbolSet, this adjusts the vertical positions of synchronous rests in two-voice staves
        /// prior to calling SvgSystem.JustifyHorizontally(). In other SymbolSets it does nothing.
        /// </summary>
        public virtual void AdjustRestsVertically(List<Staff> staves) { }

        /// <summary>
        /// This function aligns the lyrics in each voice, moving ornaments and dynamics 
        /// which are on the same side of the staff. (Lyrics are closest to the staff.)
        /// Feb. 2012: Currently only the StandardSymbolSet supports lyrics.
        /// </summary>
        public virtual void AlignLyrics(List<Staff> staves) { }

        /// <summary>
        /// Feb. 2012: Currently only the StandardSymbolSet supports notehead extender lines. 
        /// </summary>
        public virtual void AddNoteheadExtenderLines(List<Staff> staves,
            float rightMarginPos, float gap, float extenderStrokeWidth, float hairlinePadding, SvgSystem nextSystem) { }

        /// <summary>
        /// This function
        ///  1. adds beams to the BeamBlocks, and sets their final positions according to the current x-positions
        ///     of the attached chord stems.
        ///  2. sets the lengths of the attached chord stems to match.
        /// The outer (quaver) beam of each beamBlock is added to the top and/or bottom edges of staves while
        /// justifying vertically.
        /// </summary>
        public virtual void FinalizeBeamBlocks(List<Staff> staves) { }

        /// <summary>
        /// This function sets the lengths of beamed stems (including the positions of their attached dynamics etc.
        /// so that collision checking can be done as accurately as possible in JustifyHorizontally().
        /// It does this by calling FinalizeBeamBlocks(), which is called again after JustifyHorizontally(),
        /// and then deleting the beams that that function adds.
        /// At the time this function is called, chords are distributed proportionally to their duration, so the 
        /// beams which are constructed here are not exactly correct. The outer stem tips of each beam should, 
        /// however, be fairly close to their final positions.
        /// </summary>
        public virtual void SetBeamedStemLengths(List<Staff> staves) { }
    }
}
