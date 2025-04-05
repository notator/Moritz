using Moritz.Globals;
using Moritz.Spec;
using Moritz.Xml;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Moritz.Symbols
{
    /// <summary>
    ///  Contains lists of NoteObjects. Each NoteObject has a graphic and a list of performances (=midi levels) 
    /// </summary>
    public class Voice
    {
        public Voice(Staff staff, VoiceDef voiceDef)
        {
            voiceDef.AssertConsistency();

            Staff = staff;
            VoiceDef = voiceDef;
        }

        private List<List<MidiChordDef>> GetMidiChordDefsListList(List<Trk> trks)
        {
            int nTrks = trks.Count;

            List<List<MidiChordDef>> horizontalMcdListList = new List<List<MidiChordDef>>();
            for(int trkIndex = 0; trkIndex < nTrks; ++trkIndex)
            {
                List<MidiChordDef> hMcdList = new List<MidiChordDef>();
                var iuds = trks[trkIndex].UniqueDefs;
                foreach(IUniqueDef iud in iuds)
                {
                    if(iud is MidiChordDef mcd)
                    {
                        hMcdList.Add(mcd);
                    }
                }
                horizontalMcdListList.Add(hMcdList);
            }
            int nMcds = horizontalMcdListList[0].Count;
            for(int trkIndex = 1; trkIndex < nTrks; ++trkIndex)
            {
                Debug.Assert(horizontalMcdListList[trkIndex].Count == nMcds);
            }
            
            List<List<MidiChordDef>> verticalMcdListList = new List<List<MidiChordDef>>();
            for(int mcdIndex = 0; mcdIndex < nMcds; ++mcdIndex)
            {
                List<MidiChordDef> vMcdList = new List<MidiChordDef>();
                for(int trkIndex = 0; trkIndex < nTrks; ++trkIndex)
                {
                    vMcdList.Add(horizontalMcdListList[trkIndex][mcdIndex]);
                }
                verticalMcdListList.Add(vMcdList);
            }
            return verticalMcdListList;
        }

        /// <summary>
        /// Writes out an SVG Voice
        /// </summary>
        /// <param name="w"></param>
        public virtual void WriteSVG(SvgWriter w, int channelIndex)
        {
            List<List<MidiChordDef>> midiChordDefsListList = GetMidiChordDefsListList(VoiceDef.Trks);
            int mcdLLindex = 0;

            w.SvgStartGroup(CSSObjectClass.voice.ToString());

            for(int i = 0; i < NoteObjects.Count; ++i)
            {
                NoteObject noteObject = NoteObjects[i];

                if(noteObject is Barline barline)
                {
                    bool isLastNoteObject = (i == (NoteObjects.Count - 1));
                    float top = Staff.Metrics.StafflinesTop;
                    float bottom = Staff.Metrics.StafflinesBottom;
                    if(barline.IsVisible)
                    {
                        barline.WriteSVG(w, top, bottom, isLastNoteObject);
                    }
                    barline.WriteDrawObjectsSVG(w);
                }
                else if(noteObject is CautionaryChordSymbol cautionaryChordSymbol)
                {
                    cautionaryChordSymbol.WriteSVG(w);
                }
                else if(noteObject is ChordSymbol chordSymbol)
                { 
                    List<MidiChordDef> mcds = midiChordDefsListList[mcdLLindex++];
                    Debug.Assert(mcds != null);
                    for(int mcdIndex = 1; mcdIndex < mcds.Count; ++mcdIndex)
                    {
                        chordSymbol.AddMidiChordDef(mcds[mcdIndex]);
                    }
                    chordSymbol.WriteSVG(w, channelIndex);
                }
                else if(noteObject is RestSymbol restSymbol)
                {
                    restSymbol.WriteSVG(w);
                }
                else if(noteObject is Clef clef) // clef
                {
                    if(clef.Metrics != null)
                    {
                        // if this is the first barline, the staff name and (maybe) barnumber will be written.
                        ClefMetrics cm = clef.Metrics as ClefMetrics;
                        clef.WriteSVG(w, cm.ClefID, cm.OriginX, cm.OriginY);
                    }
                }
                else if(noteObject is SmallClef smallClef)
                {
                    if(smallClef.Metrics != null)
                    {
                        SmallClefMetrics scm = smallClef.Metrics as SmallClefMetrics;
                        smallClef.WriteSVG(w, scm.ClefID, scm.OriginX, scm.OriginY);
                    }
                }
                else
                {
                    throw new ApplicationException("Unknown noteObject type.");
                }
            }

            w.SvgEndGroup(); // voice
        }

        public bool ContainsAChordSymbol
        {
            get
            {
                bool containsAChordSymbol = false;
                foreach(NoteObject noteObject in NoteObjects)
                {
                    if(noteObject is ChordSymbol)
                    {
                        containsAChordSymbol = true;
                        break;
                    }
                }
                return containsAChordSymbol;
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
        public void Append(Voice voice2)
        {
            foreach(NoteObject noteObject in voice2.NoteObjects)
            {
                noteObject.Voice = this;
                NoteObjects.Add(noteObject);
            }

            var trks1 = VoiceDef.Trks;
            var trks2 = voice2.VoiceDef.Trks;
            Debug.Assert(trks1.Count == trks2.Count);
            for(var trkIndex = 0; trkIndex < trks1.Count; ++trkIndex)
            {
                trks1[trkIndex].UniqueDefs.AddRange(trks2[trkIndex].UniqueDefs);
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
            Clef currentClef = null;
            bool breakGroup = false;
            ChordSymbol lastChord = null;
            foreach(ChordSymbol cs in ChordSymbols)
                lastChord = cs;

            foreach(NoteObject noteObject in NoteObjects)
            {
                ChordSymbol chord = noteObject as ChordSymbol;
                RestSymbol rest = noteObject as RestSymbol;
                Clef clef = noteObject as Clef;
                Barline barline = noteObject as Barline;

                if(noteObject is CautionaryChordSymbol cautionaryChord)
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
                        // chord.Stem.BeamContinues is the value of
                        // MidiChordDef.BeamContinues.
                        // This value is true by default, but can be set
                        // (in MidiChordDef) classes used by composition
                        // algorithms.
                        if(chord.Stem.BeamContinues)
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
                    if(noteObject is AnchorageSymbol iHasDrawObjects)
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
                    if(noteObject is DurationSymbol durationSymbol)
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
                    if(noteObject is ChordSymbol chordSymbol)
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
                    if(noteObject is RestSymbol restSymbol)
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
        private readonly List<NoteObject> _noteObjects = new List<NoteObject>();
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

        public VoiceDef VoiceDef = null;

        private DurationSymbol _firstDurationSymbol;
    }
}
