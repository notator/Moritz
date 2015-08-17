using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Xml;
using Moritz.Spec;

namespace Moritz.Symbols
{
    /// <summary>
    ///  A sequence of noteObjects.
    /// </summary>
    public abstract class Voice
    {
        public Voice(Staff staff, Voice voice)
        {
            Staff = staff;
            StemDirection = voice.StemDirection;
            this.AppendNoteObjects(voice.NoteObjects);
        }

        public Voice(Staff staff)
        {
            Staff = staff;
        }

		public abstract void WriteSVG(SvgWriter w, bool staffIsVisible, int systemNumber, int staffNumber, int voiceNumber);

        /// <summary>
        /// Writes out an SVG Voice
        /// </summary>
        /// <param name="w"></param>
        public virtual void WriteSVG(SvgWriter w, bool staffIsVisible)
        {
            for(int i = 0; i < NoteObjects.Count; ++i)
            {
				NoteObject noteObject = NoteObjects[i];				
				Barline barline = noteObject as Barline;
				if(staffIsVisible && barline != null)
				{
					bool isLastNoteObject = (i == (NoteObjects.Count - 1));
					float top = Staff.Metrics.StafflinesTop;
					float bottom = Staff.Metrics.StafflinesBottom;
					PageFormat pageFormat = Staff.SVGSystem.Score.PageFormat;
					float barlineStrokeWidth = pageFormat.BarlineStrokeWidth;
					float stafflineStrokeWidth = pageFormat.StafflineStemStrokeWidth;
					barline.WriteSVG(w, top, bottom, barlineStrokeWidth, stafflineStrokeWidth, isLastNoteObject, false);
				}

                ChordSymbol chordSymbol = noteObject as ChordSymbol;
                if(chordSymbol != null)
                {
                    chordSymbol.WriteSVG(w, staffIsVisible);
                }
                else
				{
					// if this is the first barline, the staff name and (maybe) barnumber will be written.
                    noteObject.WriteSVG(w, staffIsVisible);
				}
            }
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
        public void SetChordStemDirectionsAndCreateBeamBlocks(PageFormat pageFormat)
        {
            List<ChordSymbol> chordsBeamedTogether = new List<ChordSymbol>();
            ClefSymbol currentClef = null;
            bool breakGroup = false;
            ChordSymbol lastChord = null;
            foreach(ChordSymbol cs in ChordSymbols)
                lastChord = cs;

            foreach(NoteObject noteObject in NoteObjects)
            {
                CautionaryChordSymbol cautionaryChord = noteObject as CautionaryChordSymbol;
                ChordSymbol chord = noteObject as ChordSymbol;
                RestSymbol rest = noteObject as RestSymbol;
                ClefSymbol clef = noteObject as ClefSymbol;
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
                        if(rest.LocalCautionaryChordDef == null)
                            breakGroup = true;
                    }

                    if(clef != null)
                        breakGroup = true;

                    if(barline != null && !pageFormat.BeamsCrossBarlines)
                        breakGroup = true;

                    if(chord == lastChord)
                        breakGroup = true;
                }

                if(chordsBeamedTogether.Count > 0 && breakGroup)
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
                            float beamThickness = pageFormat.BeamThickness;
                            float beamStrokeThickness = pageFormat.StafflineStemStrokeWidth;
                            if(this is InputVoice)
                            {
                                beamThickness *= pageFormat.InputStavesSizeFactor;
                                beamStrokeThickness *= pageFormat.InputStavesSizeFactor;
                            }
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

        /// <summary>
        /// A MidiChannel attribute is always written for every OutputVoice in the first system in a score.
        /// No other OutputVoice MidiChannels are written.
        /// InputVoice MidiChannel attributes are omitted altogether unless explicitly set (in an InputVoiceDef) by an algorithm.
        /// If they are set, InputVoice MidiChannel attributes are also only written in the first system in the score.
        /// </summary>
        public byte MidiChannel { get { return _midiChannel; } }
        protected byte _midiChannel;

        public TrkDef VoiceDef = null;

        private DurationSymbol _firstDurationSymbol;
    }
}
