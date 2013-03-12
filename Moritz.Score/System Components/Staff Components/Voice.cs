using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Score.Midi;

namespace Moritz.Score
{
    /// <summary>
    ///  A sequence of noteObjects.
    /// </summary>
    public class Voice
    {
        public Voice(Staff staff, Voice voice)
        {
            Staff = staff;
            StemDirection = voice.StemDirection;
            this.AppendNoteObjects(voice.NoteObjects);
            MidiChannel = voice.MidiChannel;
        }

        public Voice(Staff staff, byte midiChannel)
        {
            Staff = staff;
            MidiChannel = midiChannel;
        }
 
        /// <summary>
        /// Writes out an SVG Voice
        /// </summary>
        /// <param name="w"></param>
        public void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup("voice" + SvgScore.UniqueID_Number);
            w.WriteAttributeString("score", "object", null, "voice");
            w.WriteAttributeString("score", "midiChannel", null, MidiChannel.ToString());

            foreach(NoteObject noteObject in NoteObjects)
            {
                ChordSymbol chordSymbol = noteObject as ChordSymbol;
                if(chordSymbol != null)
                {
                    chordSymbol.WriteSVG(w);
                }
                else
                    noteObject.WriteSVG(w);
            }
            w.SvgEndGroup(); // voice
        }

        /// <summary>
        /// A voice is empty if it contains no NoteObjects.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return NoteObjects.Count == 0;
            }
        }
        /// <summary>
        /// Returns null if the last noteObject on this voice's noteObject list is not a Barline.
        /// </summary>
        public Barline FinalBarline
        {
            get
            {
                Barline finalBarline = this.NoteObjects[NoteObjects.Count - 1] as Barline;
                return finalBarline;
            }
        }
        /// <summary>
        /// Returns the first barline to occur before any durationSymbols, or null if no such barline exists.
        /// </summary>
        public Barline InitialBarline
        {
            get
            {
                Barline initialBarline = null;
                foreach(NoteObject noteObject in NoteObjects)
                {
                    initialBarline = noteObject as Barline;
                    if(noteObject is DurationSymbol || noteObject is Barline)
                        break;
                }
                return initialBarline;
            }
        }

        public DurationSymbol FinalDurationSymbol
        {
            get
            {
                DurationSymbol finalDurationSymbol = null;
                for(int i = this.NoteObjects.Count - 1; i >= 0; i--)
                {
                    finalDurationSymbol = this.NoteObjects[i] as DurationSymbol;
                    if(finalDurationSymbol != null)
                        break;
                }
                return finalDurationSymbol;
            }
        }

        #region composition
        /// <summary>
        /// Replaces the DurationSymbol symbolToBeReplaced (which is in this Voice's NoteObjects)
        /// by the all the noteObjects. Sets each of the noteObjects' Voice to this. 
        /// </summary>
        public void Replace(DurationSymbol symbolToBeReplaced, List<NoteObject> noteObjects)
        {
            #region conditions
            Debug.Assert(symbolToBeReplaced != null && symbolToBeReplaced.Voice == this);
            #endregion conditions

            List<NoteObject> tempList = new List<NoteObject>(this.NoteObjects);
            this.NoteObjects.Clear();
            int i = 0;
            while(tempList.Count > i && tempList[i] != symbolToBeReplaced)
            {
                this.NoteObjects.Add(tempList[i]);
                i++;
            }
            foreach(NoteObject noteObject in noteObjects)
            {
                noteObject.Voice = this;
                this.NoteObjects.Add(noteObject);
            }
            // tempList[i] is the symbolToBeReplaced
            i++;
            while(tempList.Count > i)
            {
                this.NoteObjects.Add(tempList[i]);
                i++;
            }
            tempList = null;
        }
        /// <summary>
        /// Appends a clone of the noteObjects to this voice's NoteObjects
        /// (Sets each new noteObjects container to this.)
        /// </summary>
        /// <param name="noteObjects"></param>
        public void AppendNoteObjects(List<NoteObject> noteObjects)
        {
            foreach(NoteObject noteObject in noteObjects)
            {
                noteObject.Voice = this;
                NoteObjects.Add(noteObject);
            }
        }

        /// <summary>
        /// Sets Chord.Stem.Direction for each chord.
        /// Chords are beamed together, duration classes permitting, unless a rest or clef intervenes.
        /// If a barline intervenes, and beamsCrossBarlines is true, the chords are beamed together.
        /// If a barline intervenes, and beamsCrossBarlines is false, the beam is broken.
        /// </summary>
        public void SetChordStemDirectionsAndCreateBeamBlocks(float beamThickness, float beamStrokeThickness, bool beamsCrossBarlines)
        {
            List<ChordSymbol> chordsBeamedTogether = new List<ChordSymbol>();
            ClefSign currentClef = null;
            bool breakGroup = false;
            ChordSymbol lastChord = null;
            foreach(ChordSymbol cs in ChordSymbols)
                lastChord = cs;

            foreach(NoteObject noteObject in NoteObjects)
            {
                CautionaryChordSymbol cautionaryChord = noteObject as CautionaryChordSymbol;
                ChordSymbol chord = noteObject as ChordSymbol;
                RestSymbol rest = noteObject as RestSymbol;
                ClefSign clef = noteObject as ClefSign;
                Barline barline = noteObject as Barline;

                if(cautionaryChord != null)
                    continue;

                if(chord != null)
                {
                    if(chord.DurationClass == DurationClass.cautionary
                    || chord.DurationClass == DurationClass.breve
                    || chord.DurationClass == DurationClass.semibreve
                    || chord.DurationClass == DurationClass.minim
                    || chord.DurationClass == DurationClass.crotchet)
                    {
                        if(currentClef != null)
                        {
                            if(this.StemDirection == VerticalDir.none)
                                chord.Stem.Direction = chord.DefaultStemDirection(currentClef);
                            else
                                chord.Stem.Direction = this.StemDirection;
                        }
                        breakGroup = true;
                    }
                    else
                    {
                        chordsBeamedTogether.Add(chord);
                        if(chord.Stem.BeamContinues) // this is true by default
                            breakGroup = false;
                        else
                            breakGroup = true;
                    }
                }

                if(chordsBeamedTogether.Count > 0)
                {
                    if(rest != null)
                    {
                        if(rest.OverlapLmddAtStartOfBar == null)
                            breakGroup = true;
                    }

                    if(clef != null)
                        breakGroup = true;

                    if(barline != null && !beamsCrossBarlines)
                        breakGroup = true;

                    if(chord == lastChord)
                        breakGroup = true;
                }

                if( chordsBeamedTogether.Count > 0 && breakGroup)
                {
                    if(currentClef != null)
                    {
                        if(chordsBeamedTogether.Count == 1)
                        {
                            if(this.StemDirection == VerticalDir.none)
                                chordsBeamedTogether[0].Stem.Direction = chordsBeamedTogether[0].DefaultStemDirection(currentClef);
                            else
                                chordsBeamedTogether[0].Stem.Direction = this.StemDirection;

                        }
                        else if(chordsBeamedTogether.Count > 1)
                        {
                            chordsBeamedTogether[0].BeamBlock =
                                new BeamBlock(currentClef, chordsBeamedTogether, this.StemDirection, beamThickness, beamStrokeThickness);
                        }
                    }
                    chordsBeamedTogether.Clear();
                }

                if(clef != null)
                    currentClef = clef;
            }
        }

        /// <summary>
        /// The system has been justified horizontally, so all objects are at their final horizontal positions.
        /// The outer tips of stems which are inside BeamBlocks have been set to the beamBlock's DefaultStemTipY value.
        /// This function
        ///  1. creates the contained beams, and sets the final coordinates of their corners.
        ///  2. resets the contained Stem.Metrics (by creating and re-allocating new ones)
        ///  3. moves objects which are outside the stem tips vertically by the same amount as the stem tips are moved.
        /// </summary>
        public void FinalizeBeamBlocks()
        {
            HashSet<BeamBlock> beamBlocks = FindBeamBlocks();
            foreach(BeamBlock beamBlock in beamBlocks)
            {
                beamBlock.FinalizeBeamBlock();
            }
        }

        public void RemoveBeamBlockBeams()
        {
            HashSet<BeamBlock> beamBlocks = FindBeamBlocks();
            foreach(BeamBlock beamBlock in beamBlocks)
            {
                beamBlock.Beams.Clear();
            }
        }

        private HashSet<BeamBlock> FindBeamBlocks()
        {
            HashSet<BeamBlock> beamBlocks = new HashSet<BeamBlock>();
            foreach(ChordSymbol chord in ChordSymbols)
            {
                if(chord.BeamBlock != null)
                    beamBlocks.Add(chord.BeamBlock);
            }
            return beamBlocks;
        }

        #region Enumerators
        public IEnumerable AnchorageSymbols
        {
            get
            {
                foreach(NoteObject noteObject in NoteObjects)
                {
                    AnchorageSymbol iHasDrawObjects = noteObject as AnchorageSymbol;
                    if(iHasDrawObjects != null)
                        yield return iHasDrawObjects;
                }
            }
        }
        public IEnumerable DurationSymbols
        {
            get
            {
                foreach(NoteObject noteObject in NoteObjects)
                {
                    DurationSymbol durationSymbol = noteObject as DurationSymbol;
                    if(durationSymbol != null)
                        yield return durationSymbol;
                }
            }
        }
        public IEnumerable ChordSymbols
        {
            get
            {
                foreach(NoteObject noteObject in NoteObjects)
                {
                    ChordSymbol chordSymbol = noteObject as ChordSymbol;
                    if(chordSymbol != null)
                        yield return chordSymbol;
                }
            }
        }
        public IEnumerable RestSymbols
        {
            get
            {
                foreach(NoteObject noteObject in NoteObjects)
                {
                    RestSymbol restSymbol = noteObject as RestSymbol;
                    if(restSymbol != null)
                        yield return restSymbol;
                }
            }
        }

        #endregion Enumerators

        public List<LocalizedMidiDurationDef> LocalizedMidiDurationDefs = new List<LocalizedMidiDurationDef>();

        /// <summary>
        /// This field is set by composition algorithms.
        /// It is stored in and retrieved from SVG files. 
        /// </summary>
        public byte MidiChannel = 0;

        #endregion composition

        #region fields loaded from .capx files
        #region attribute fields
        public VerticalDir StemDirection = VerticalDir.none;
        #endregion
        #region element fields
        public List<NoteObject> NoteObjects { get { return _noteObjects; } }
        private List<NoteObject> _noteObjects = new List<NoteObject>();
        #endregion
        #endregion
        #region moritz-specific fields
        public Staff Staff; // container
        /// <summary>
        /// The first duration symbol in this (short) voice.
        /// </summary>
        public DurationSymbol FirstDurationSymbol
        {
            get
            {
                if(_firstDurationSymbol == null)
                {
                    foreach(NoteObject noteObject in NoteObjects)
                    {
                        _firstDurationSymbol = noteObject as DurationSymbol;
                        if(_firstDurationSymbol != null)
                            break;
                    }
                }
                return _firstDurationSymbol;
            }
            set
            {
                _firstDurationSymbol = value;
            }
        }
        #endregion

        private DurationSymbol _firstDurationSymbol;
    }
}
