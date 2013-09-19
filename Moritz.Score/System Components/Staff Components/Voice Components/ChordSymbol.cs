using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Moritz.Globals;
using Moritz.Score.Midi;
using Moritz.Score.Notation;

namespace Moritz.Score
{
    public class ChordSymbol : DurationSymbol
    {
        public ChordSymbol(Voice voice, LocalizedMidiDurationDef lmdd, int minimumCrotchetDurationMS, float fontSize)
            : base(voice, lmdd, minimumCrotchetDurationMS, fontSize)
        {
            _localizedMidiDurationDef = lmdd;
            MidiChordDef midiChordDef = lmdd.LocalMidiChordDef;
            // midiChordDef is null for cautionary chords
            if(midiChordDef != null)
            {
                SetHeads(midiChordDef.MidiHeadSymbols);

                if(midiChordDef.OrnamentNumberSymbol != 0)
                {
                    AddOrnamentSymbol("~" + midiChordDef.OrnamentNumberSymbol.ToString());
                }

                if(midiChordDef.Lyric != null)
                {
                    TextInfo textInfo = new TextInfo(midiChordDef.Lyric, "Arial", (float)(FontHeight / 2F), TextHorizAlign.center);
                    Lyric lyric = new Lyric(this, textInfo);
                    DrawObjects.Add(lyric);
                }
            }
            
            // note that all chord symbols have a stem! 
            // Even cautionary, semibreves and breves need a stem direction in order to set chord Metrics correctly.
            Stem = new Stem(this);

            // Beam is currently null. Create when necessary.
        }

        protected void AddOrnamentSymbol(string ornamentString)
        {
            float fontHeight = FontHeight * 0.75F;
            TextInfo textInfo = new TextInfo(ornamentString, "Estrangelo Edessa", fontHeight, TextHorizAlign.center);
            Text controlText = new Text(this, textInfo);
            DrawObjects.Add(controlText);
        }

        public VerticalDir DefaultStemDirection(ClefSign clef)
        {
            Debug.Assert(this.HeadsTopDown.Count > 0);
            float gap = 32F; // dummy value
            List<float> topDownHeadOriginYs = new List<float>();
            int lastMidiPitch = int.MaxValue;
            foreach(Head head in this.HeadsTopDown)
            {
                Debug.Assert(head.MidiPitch < lastMidiPitch);
                topDownHeadOriginYs.Add(head.GetOriginY(clef, gap));
            }

            float heightOfMiddleStaffLine = (this.Voice.Staff.NumberOfStafflines / 2) * gap;
            float halfHeight = 0F;
            if(topDownHeadOriginYs.Count == 1)
                halfHeight = topDownHeadOriginYs[0];
            else
                halfHeight = (topDownHeadOriginYs[topDownHeadOriginYs.Count - 1] + topDownHeadOriginYs[0]) / 2;

            if(halfHeight <= heightOfMiddleStaffLine)
                return VerticalDir.down;
            else
                return VerticalDir.up;
        }

        public override void WriteSVG(SvgWriter w)
        {
            if(ChordMetrics.BeamBlock != null)
                ChordMetrics.BeamBlock.WriteSVG(w);

            string idNumber = SvgScore.UniqueID_Number;

            w.SvgStartGroup("chord" + idNumber);
            WriteChordAttributes(w);

            WriteMidiInfo(w, "midi" + idNumber);

            w.SvgStartGroup("graphics" + idNumber);
            ChordMetrics.WriteSVG(w);
            w.SvgEndGroup();

            w.SvgEndGroup();
        }

        /// <summary>
        /// Used by all chord types (e.g. Study2b2ChordSymbol)
        /// </summary>
        /// <param name="w"></param>
        /// <param name="msPos"></param>
        /// <param name="msDuration"></param>
        protected virtual void WriteChordAttributes(SvgWriter w)
        {
            w.WriteAttributeString("score", "object", null, "chord");

            w.WriteAttributeString("score", "alignmentX", null, this.Metrics.OriginX.ToString(M.En_USNumberFormat));

            Debug.Assert(_msDuration > 0);
            w.WriteAttributeString("score", "msDuration", null, _msDuration.ToString());
        }

        /// <summary>
        /// Writes the chord's Midi info - the same for all graphic notations
        /// </summary>
        /// <param name="w"></param>
        protected void WriteMidiInfo(SvgWriter w, string midiId)
        {
            w.WriteStartElement("score", "midiChord", null);
            w.WriteAttributeString("id", midiId);
            string ID = LocalizedMidiDurationDef.LocalMidiChordDef.ID; 
            if(ID != null && !(ID.StartsWith("localChord")))
            {
                w.WriteStartElement("use");
                w.WriteAttributeString("xlink", "href", null, "#" + LocalizedMidiDurationDef.LocalMidiChordDef.ID);
                w.WriteEndElement(); // use
            }
            else
            {
                Debug.Assert(LocalizedMidiDurationDef != null);
                this.LocalizedMidiDurationDef.LocalMidiChordDef.WriteSvg(w);
            }
            w.WriteEndElement(); // midiChord
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("chord  ");
            sb.Append(InfoString);
            return sb.ToString();
        }

        /// <summary>
        /// In capella 2008, only single articulation or "staccato tenuto" are supported.
        /// </summary>
        public List<Articulation> Articulations = new List<Articulation>();

        #region composition

        /// <summary>
        /// This function uses a sophisticated algorithm to decide whether flats or sharps are to be used to
        /// represent the chord. Chords can have naturals and either sharps or flats (but not both).
        /// The display of naturals is forced if the same notehead height also exists with a sharp or flat.
        /// (The display of other accidentals are always forced in the Head constructor.)
        /// Exceptions: This function throws an exception if
        ///     1) any of the input midiPitches is out of midi range (0..127).
        ///     2) the midiPitches are not in ascending order
        ///     3) the midiPitches are not unique.
        /// The midiPitches argument must be in order of size (ascending), but Heads are created in top-down order.
        /// </summary>
        /// <param name="midiPitches"></param>
        public void SetHeads(List<byte> midiPitches)
        {
            #region check inputs
            int pitch = -1;
            foreach(int midiPitch in midiPitches)
            {
                if(midiPitch < 0 || midiPitch > 127)
                    throw new ApplicationException("Composition.SetPitches(): midiPitch out of range");
                if(midiPitch <= pitch)
                    throw new ApplicationException(
                        "Composition.SetPitches(): midiPitches must be unique and in ascending order");
                pitch = midiPitch;
            }
            #endregion
            this.HeadsTopDown.Clear();
            bool useSharp = UseSharps(midiPitches); // returns false if flats are to be used
            for(int i = midiPitches.Count-1; i >= 0; --i)
            {
                Head head = new Head(this, midiPitches[i], useSharp);
                this.HeadsTopDown.Add(head);
            }
            for(int i = 0; i < midiPitches.Count; i++)
            {
                if(this.HeadsTopDown[i].Alteration == 0)
                {
                    this.HeadsTopDown[i].DisplayAccidental = DisplayAccidental.suppress;
                }
            }
            for(int i = 1; i < midiPitches.Count; i++)
            {
                if(this.HeadsTopDown[i].Pitch == this.HeadsTopDown[i - 1].Pitch)
                {
                    this.HeadsTopDown[i - 1].DisplayAccidental = DisplayAccidental.force;
                    this.HeadsTopDown[i].DisplayAccidental = DisplayAccidental.force;
                }
            }
        }
        #region private for SetPitches()
        /// <summary>
        /// Returns true if sharps are to be used to represent the midiPitches,
        /// or false if flats are to be used.
        /// </summary>
        /// <param name="midiPitches"></param>
        /// <returns></returns>
        private bool UseSharps(List<byte> midiPitches)
        {
            for(int i = 0; i < midiPitches.Count; i++)
            {
                Debug.Assert(midiPitches[i] >= 0 && midiPitches[i] < 128);
            }

            bool useSharps = true;
            List<int> midiIntervals = new List<int>();
            for(int i = 1; i < midiPitches.Count; i++)
            {
                midiIntervals.Add(midiPitches[i] - midiPitches[i - 1]);
            }
            List<int> collapsedIntervals = new List<int>();
            foreach(int midiInterval in midiIntervals)
            {
                collapsedIntervals.Add(midiInterval % 12);
            }

            // look in the following order (I dont like augmented seconds at all)!
            int[] preferredIntervals = { 3, 7, 1, 2, 4, 5, 6, 8, 9, 10, 11 };
            bool? useSharpsOrNull = null;
            foreach(int interval in preferredIntervals)
            {
                int index = 0;
                if(collapsedIntervals.Contains(interval))
                {
                    foreach(int value in collapsedIntervals)
                    {
                        if(value == interval)
                            break;
                        index++;
                    }
                    // index is now both the index of the "most preferred" interval 
                    // and the index of the lower midiPitch of the "most preferred" interval.
                    Head head = new Head(null, midiPitches[index], true);
                    // head is either natural or sharp.
                    int preferredInterval = collapsedIntervals[index];
                    useSharpsOrNull = GetUseSharps(head, preferredInterval);
                }
                if(useSharpsOrNull != null)
                {
                    useSharps = (bool)useSharpsOrNull;
                    break;
                }
            }
            return useSharps;
        }
        /// <summary>
        /// Head.Alteration is either 0 or 1 (natural or sharp).
        /// Returns the sharp/flat preference for representing the interval on this head,
        /// or null if there is no preference.
        /// </summary>
        /// <param name="head"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private bool? GetUseSharps(Head head, int interval)
        {
            Debug.Assert(interval > 0 && interval < 12);
            Debug.Assert(head.Alteration == 0 || head.Alteration == 1);
            bool? useSharpsOrNull = null;
            #region Head is A
            if(head.Pitch[0] == 'A')
            {
                if(head.Alteration == 0) // (A)
                {
                    switch(interval)
                    {
                        case 11:
                        case 9:
                        case 4:
                            useSharpsOrNull = true;
                            break;
                        case 1:
                            useSharpsOrNull = false;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (A#)
                {
                    switch(interval)
                    {
                        case 11:
                        case 9:
                        case 7:
                        case 4:
                        case 2:
                            useSharpsOrNull = false;
                            break;
                        case 1:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }

                }
            }
            #endregion A
            #region Head is B
            else if(head.Pitch[0] == 'B')
            {
                if(head.Alteration == 0) // (B)
                {
                    switch(interval)
                    {
                        case 11:
                        case 9:
                        case 7:
                        case 4:
                        case 2:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (B#)
                {
                    useSharpsOrNull = false;
                }
            }
            #endregion
            #region Head is C
            else if(head.Pitch[0] == 'C')
            {
                if(head.Alteration == 0) // (C)
                {
                    switch(interval)
                    {
                        case 10:
                        case 8:
                        case 3:
                        case 1:
                            useSharpsOrNull = false;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (C#)
                {
                    switch(interval)
                    {
                        case 4:
                            useSharpsOrNull = false;
                            break;
                        case 10:
                        case 8:
                        case 3:
                        case 1:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion Head is C
            #region Head is D
            else if(head.Pitch[0] == 'D')
            {
                if(head.Alteration == 0) // (D)
                {
                    switch(interval)
                    {
                        case 11:
                        case 4:
                            useSharpsOrNull = true;
                            break;
                        case 8:
                        case 1:
                            useSharpsOrNull = false;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (D#)
                {
                    switch(interval)
                    {
                        case 11:
                        case 9:
                        case 4:
                        case 2:
                            useSharpsOrNull = false;
                            break;
                        case 8:
                        case 1:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion Head is D
            #region Head is E
            else if(head.Pitch[0] == 'E')
            {
                if(head.Alteration == 0) // (E)
                {
                    switch(interval)
                    {
                        case 11:
                        case 9:
                        case 4:
                        case 2:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (E#)
                {
                    useSharpsOrNull = false;
                }
            }
            #endregion Head is E
            #region Head is F
            else if(head.Pitch[0] == 'F')
            {
                if(head.Alteration == 0) // (F)
                {
                    switch(interval)
                    {
                        case 10:
                        case 8:
                        case 5:
                        case 3:
                        case 1:
                            useSharpsOrNull = false;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (F#)
                {
                    switch(interval)
                    {
                        case 10:
                        case 8:
                        case 5:
                        case 3:
                        case 1:
                            useSharpsOrNull = true;
                            break;
                        case 11: // should really be G-flat, but I dont like G-flats!
                            //useSharpsOrNull = false;
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion Head is F
            #region Head is G
            else if(head.Pitch[0] == 'G')
            {
                if(head.Alteration == 0) // (G)
                {
                    switch(interval)
                    {
                        case 8:
                        case 3:
                        case 1:
                            useSharpsOrNull = false;
                            break;
                        case 11:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (G#)
                {
                    switch(interval)
                    {
                        case 11:
                        case 9:
                        case 4:
                            useSharpsOrNull = false;
                            break;
                        case 8:
                        case 3:
                        case 1:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion Head is G
            return useSharpsOrNull;
        }
        #endregion private

        /// <summary>
        /// This chordSymbol is in the lower of two voices on a staff. The argument is another synchronous chordSymbol
        /// at the same MsPosition on the same staff. Both chordSymbols have ChordMetrics, and the chord in the lower
        /// voice has been moved (either right or left) so that there are no collisions between noteheads.
        /// This function moves the accidentals in both chords horizontally, so that they are all on the left of both
        /// chords but as far to the right as possible without there being any collisions.
        /// Accidentals are positioned in top-bottom and right-left order.
        /// If two noteheads are at the same diatonic height, both accidentals will already exist and have forced display.
        /// Such accidentals are placed in the left-right order of the noteheads
        /// </summary>
        /// <param name="upperChord"></param>
        public void AdjustAccidentalsX(ChordSymbol upperChord)
        {
            float stafflineStemStrokeWidth = Voice.Staff.SVGSystem.Score.PageFormat.StafflineStemStrokeWidth;

            this.ChordMetrics.AdjustAccidentalsForTwoChords(upperChord.ChordMetrics, stafflineStemStrokeWidth);
        }

        /// <summary>
        /// Returns the maximum (positive) horizontal distance by which this anchorage symbol overlaps
        /// (any characters in) the previous noteObjectMoment (which contains symbols from both voices
        /// in a 2-voice staff).
        /// This function is used by rests and barlines.It is overridden by chords.
        /// </summary>
        /// <param name="previousAS"></param>
        public override float OverlapWidth(NoteObjectMoment previousNOM)
        {
            float overlap = float.MinValue;
            float localOverlap = 0F;
            foreach(AnchorageSymbol previousAS in previousNOM.AnchorageSymbols)
            {
                if(this is Study2b2ChordSymbol)
                    localOverlap = Metrics.OverlapWidth(previousAS);
                else
                    localOverlap = ChordMetrics.OverlapWidth(previousAS);

                overlap = overlap > localOverlap ? overlap : localOverlap;
            }
            return overlap;
        }


        #endregion composition

        #region display attributes
        /// <summary>
        /// up/down means that the Chord is notated in the staff above/below it's real staff.
        /// This may be used, when a voice changes from one staff to another.
        /// </summary>
        public VerticalDir NotationStaff = VerticalDir.none;
        #endregion display attributes

        /// <summary>
        /// Returns this.Metrics cast to ChordMetrics.
        /// Before accessing this property, this.Metrics must be assigned to an object of type ChordMetrics.
        /// </summary>
        internal ChordMetrics ChordMetrics
        {
            get
            {
                ChordMetrics chordMetrics = Metrics as ChordMetrics;
                Debug.Assert(chordMetrics != null);
                return chordMetrics;
            }
        }
        public Stem Stem = null; // defaults
        public BeamBlock BeamBlock = null; // defaults
        public List<Head> HeadsTopDown = new List<Head>(); // Heads are in top-down order.

        public LocalizedMidiDurationDef LocalizedMidiDurationDef { get { return _localizedMidiDurationDef; } }
        protected LocalizedMidiDurationDef _localizedMidiDurationDef = null;

        public string GraphicSymbolID { get { return _graphicSymbolID; } }
        protected string _graphicSymbolID = null;
    }
}
