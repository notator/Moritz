using Krystals5ObjectLibrary;

using Moritz.Globals;
using Moritz.Spec;
using Moritz.Symbols;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Moritz.Algorithm
{
    /// <summary>
    /// A CompositionAlgorithm is special to a particular composition.
    /// When called, the DoAlgorithm() function returns a list of VoiceDef lists,
    /// whereby each contained VoiceDef list is the definition of a bar (a bar is
    /// a place where a system can be broken).
    /// Algorithms don't control the page format, how many bars per system there
    /// are, or the shapes of the symbols. Those things are set for a particular
    /// score in an .mkss file using the Assistant Composer's main form.
    /// The VoiceDefs returned from DoAlgorithm() are converted to real Voices
    /// (containing real NoteObjects) later, using the options set in an .mkss file. 
    /// </summary>
    public abstract class CompositionAlgorithm
    {
        protected CompositionAlgorithm()
        {
        }

        protected void CheckParameters()
        {
            #region check channels
            int channelCount = NumberOfVoices;
            if(channelCount < 1)
                throw new ApplicationException("CompositionAlgorithm: There must be at least one output voice!");
            if(channelCount > 16)
                throw new ApplicationException("CompositionAlgorithm: There cannot be more than 16 output voices.");

            int previousChannelIndex = -1;
            for(int i = 0; i < channelCount; ++i)
            {
                int channelIndex = i;

                if(channelIndex < 0 || channelIndex > 15)
                    throw new ApplicationException("CompositionAlgorithm: midi channel out of range!");

                if(channelIndex != 9) // 9 is the percussion channel, which can be allocated to any voice.
                {
                    if(channelIndex != (previousChannelIndex + 1))
                    {
                        throw new ApplicationException("CompositionAlgorithm: (non-percussion) midi channels must be unique and in ascending order!");
                    }
                    previousChannelIndex = channelIndex;
                }
            }
            #endregion

            if(NumberOfBars <= 0)
                throw new ApplicationException("CompositionAlgorithm: There must be at least one bar!");

        }

        public virtual ScoreData SetScoreRegionsData(List<Bar> bars) { return null; }

        /// <summary>
        /// This function is called by implementations of the above SetScoreRegionsData function.
        /// Returns a dictionary containing (barlineIndex, (barlineIndex, barlineMsPosition)) KeyValuePairs.
        /// Each barlineIndex is the index of the left barline of the bar with the same index, so the first KeyValuePair
        /// is (0,(0,0)). The finalBarline is also added to the returned dictionary, so its Count is 1 + bars.Count.
        /// The data for the final barline in the score is in the final entry.
        /// </summary>
        protected Dictionary<int, (int index, int msPosition)> GetMsPosPerBarlineIndexDict(List<Bar> bars)
        {
            var rval = new Dictionary<int, (int index, int msPosition)>();

            int barlineMsPos = 0;
            int barsCount = bars.Count;
            for(int i = 0; i < barsCount; ++i)
            {
                var tuple = (index: i, msPosition: barlineMsPos);
                rval.Add(i, tuple);
                barlineMsPos += bars[i].MsDuration;
            }
            rval.Add(barsCount, (index: barsCount, msPosition: barlineMsPos));

            return rval;
        }

        /// <summary>
        /// Returns one Trk per voice, containing MidiChordDefs and RestDefs whose MidiDef 
        /// properties have been set to their alternative Trk values.
        /// </summary>
        /// <param name="voiceDefs"></param>
        protected List<Trk> GetMainTrks(List<List<Trk>> interpretations)
        {
            AssertConsistency(interpretations);
            //interpretations contain only MidiChordDef and RestDef objects.

            List<Trk> returnTrks = new List<Trk>();

            foreach(var trkList in interpretations)
            {
                var iuds = trkList[0].UniqueDefs;
                for(int i = 0; i < iuds.Count; ++i)
                {
                    var iud = iuds[i];
                    if(iud is MidiChordDef mcd)
                    {
                        mcd.MidiDefs.Add(mcd); // the first MidiDef is always the one that defines the Chord's appearance.
                        for(int j = 1; j < trkList.Count; ++j)
                        {
                            MidiChordDef subMcd = trkList[j].UniqueDefs[i] as MidiChordDef;
                            Debug.Assert(subMcd != null);
                            mcd.MidiDefs.Add(subMcd);
                        }
                    }
                    else if(iud is RestDef restDef)
                    {
                        restDef.MidiDefs.Add(restDef);  // the first MidiDef is always the one that defines the Rest's appearance.
                        for(int j = 1; j < trkList.Count; ++j)
                        {
                            RestDef subRestDef = trkList[j].UniqueDefs[i] as RestDef;
                            Debug.Assert(subRestDef != null);
                            restDef.MidiDefs.Add(subRestDef);
                        }
                    }
                }
                returnTrks.Add(trkList[0]);
            }

            return returnTrks;
        }

        public void AssertConsistency(List<List<Trk>> interpretations)
        {
            foreach(var trkList in interpretations)
            {
                foreach(var trk in trkList)
                {
                    trk.AssertConsistency();

                    foreach(var uniqueDef in trk.UniqueDefs)
                    {
                        Debug.Assert(uniqueDef is MidiChordDef || uniqueDef is RestDef);
                        Debug.Assert(!(uniqueDef is CautionaryChordDef || uniqueDef is ClefDef));
                    }
                }
            }

            CheckTrksConsistency(interpretations);

            CheckInterpretationConsistency(interpretations);
        }

        private void CheckTrksConsistency(List<List<Trk>> interpretations)
        {
            // All Trks within the a particular interpretation must have the same msDuration.
            for(int interpIndex = 0; interpIndex < interpretations[0].Count; ++interpIndex)
            {
                int interp0MsDuration = interpretations[0][interpIndex].MsDuration;
                for(int trkIndex = 1; trkIndex < interpretations.Count; ++trkIndex)
                {
                    Trk interpTrk = interpretations[trkIndex][interpIndex];
                    Debug.Assert(interpTrk.MsDuration == interp0MsDuration);
                }
            }
        }

        /// <summary>
        /// The overall sequence of events (DurationDefs) must be identical in all interpretations.
        /// </summary>
        /// <param name="nTrks"></param>
        /// <param name="nInterpretations"></param>
        private void CheckInterpretationConsistency(List<List<Trk>> interpretations)
        {
            List<IUniqueDef> topIuds = interpretations[0][0].UniqueDefs;
            for(int i = 0; i < topIuds.Count; i++)
            {
                var topIud = topIuds[i];
                for(int trkIndex = 0; trkIndex < interpretations.Count; trkIndex++)
                {
                    for(int interpIndex = 0; interpIndex < interpretations[0].Count; interpIndex++)
                    {
                        List<IUniqueDef> localIuds = interpretations[trkIndex][interpIndex].UniqueDefs;
                        Debug.Assert((topIud is MidiChordDef && localIuds[i] is MidiChordDef) || (topIud is RestDef && localIuds[i] is RestDef));
                    }
                }
            }
        }

        /// <summary>
        /// Defines the number of MIDI channels used by the algorithm.
        /// This number must be in range [1..16].
        /// Each voice in a score has a particular midi channel, so the number of midi channels is also the number of voices.
        /// Midi channels are always allocated from top to bottom in each score. 
        /// Notes:
        /// The association of voices with staves is controlled using an input field in the Assistant Composer's main dialog.
        /// Each Voice contains a list of Trk. Each Trk contains a different interpretation (=performance) of the voice's graphics.
        /// Each Voice can contain more than one (performance) track (Trk), all such tracks have the same midi channel.
        /// </summary>
        public abstract int NumberOfVoices { get; }

        /// <summary>
        /// Returns the number of bars (=bar definitions) created by the algorithm.
        /// </summary>
        /// <returns></returns>
        public abstract int NumberOfBars { get; }

        public virtual IReadOnlyList<int> RegionStartBarIndices { get { return new List<int>(); } }

        /// <summary>
        /// The DoAlgorithm() function is special to a particular composition.
        /// This function returns a sequence of abstract bar definitions, devoid of layout information,
        /// but including mid-staff clef changes.
        /// Each bar definition is a list of voice definitions (VoiceDefs), The VoiceDefs are in top to
        /// bottom order of the voices in a final score.
        /// Each bar definition contains the same number of VoiceDefs. VoiceDefs at the same index in
        /// each bar are continuations of the same overall voice definition, and may be concatenated
        /// to create multiple bars on a staff.
        /// Each VoiceDef contains a list of Trk objects, each of which contains a list of IUniqueDef objects.
        /// Trks at the same index in a particular VoiceDef make up a particular performance of that Voice.
        /// When the Assistant Composer creates a real score, each of the UniqueDef objects in the VoiceDef's
        /// first Trk is converted to a real NoteObject containing layout information (by a Notator), and
        /// the NoteObject is then added to a concrete Voice.NoteObjects list. See Notator.AddSymbolsToSystems().
        /// Notes:
        /// Each Bar must contain at least one VoiceDef, and each VoiceDef must contain at least one Trk.
        /// Each voice's MIDI channel is its index from top to bottom in the score's systems.
        /// </summary>
        public abstract List<Bar> DoAlgorithm(PageFormat pageFormat, List<Krystal> krystals);

        /// <summary>
        /// Inserts ClefDefs at arbitrary positions in the top VoiceDef.Trks[0].UniqueDefs in each Staff.
        /// The main clefs at the beginnings of bars are added automatically later, taking these clef changes
        /// into account.
        /// Call this function as follows:
        ///     InsertClefChanges(bars, pageformat.VoiceIndicesPerStaff);
        /// Its realisation follows the following pattern:
        /// protected override void InsertClefChanges(bars, voiceIndicesPerStaff)
        /// {
        ///     // GetEmptyClefChangesPerBarPerStaff is predefined in this file.
        ///     var clefChangesPerBarPerStaff = GetEmptyClefChangesPerBarPerStaff(bars, voiceIndicesPerStaff);
        ///     
        ///     var barIndex = 3;
        ///     var staffIndex = 0;
        ///     SortedDictionary<int, string> dict = clefChangesPerBarPerStaff[barIndex][staffIndex];
        ///     
        ///     dict.Add(9, "t");
        ///     dict.Add(8, "b2");
        ///     dict.Add(6, "b");
        ///     dict.Add(4, "t2");
        ///     dict.Add(2, "t");
        ///
        ///     // InsertClefChangesInBars is predefined in this file.
        ///     InsertClefChangesInBars(bars, voiceIndicesPerStaff, clefChangesPerBarPerStaff);
        /// }
        /// </example> 
        /// </summary>
        /// <param name="bars">the completed bars</param>
        /// <param name="voiceIndicesPerStaff">pageFormat.VoiceIndicesPerStaff</param>
        protected virtual void InsertClefChanges(List<Bar> bars, List<List<int>> voiceIndicesPerStaff) { }

        /// <summary>
        /// To insert clef changes into a score's Bars:
        ///     1. Construct the bars using GetBars(...)
        ///     2. Create an empty clefChanges object by calling GetEmptyClefChangesPerBarPerStaff(bars).
        ///     3  populate the resulting object with code such as clefChanges[barIndex][staffIndex].Add(9, "t3").
        ///         arg 1) is the index in the IUniqueDefs of the top track in the staff in which the clef will be inserted, and
        ///         arg 2) is the clef's ID string ("t", "t1", "b3" etc.).
        ///     4. call InsertClefChangesInBars(bars, clefChanges)
        /// Clefs will be inserted in reverse order of the Sorted dictionary, so that the indices are those of
        /// the existing IUniqueDefs before which the clef will be inserted.
        /// The SortedDictionaries should not contain the initial clefs per voiceDef - those will be included
        /// automatically.
        /// Note that a CautionaryChordDef counts as an IUniqueDef at the beginning of a bar, and that clefs
        /// cannot be inserted in front of them.
        /// Note that the top trk in the top voice on each staff is the only one to be converted to graphics,
        /// so this is the only one that can accept clef definitions. A lower voice on a staff automatically has the
        /// SmallClefs that are defined for the upper voice.
        /// </summary>
        /// 
        /// <example>
        /// Example:
        /// {
        ///     List<Bar> bars = GetBars(temporalStructure, barlineMsPositions);
        ///     
        ///     var clefChangesPerBarPerStaff = GetEmptyClefChangesPerBarPerStaff(bars, PageFormat.VoiceIndicesPerStaff);
        ///     
        ///     var barIndex = 3;
        ///     var staffIndex = 2;
        ///     SortedDictionary[int, string] dict = clefChangesPerBar[barIndex][staffIndex];
        ///     
        ///     dict.Add(9, "b3");
        ///     dict.Add(8, "b2");
        ///     dict.Add(6, "b");
        ///     dict.Add(4, "t2");
        ///     dict.Add(2, "t");
        ///     
        ///     InsertClefChangesInBars(bars, clefChangesPerBarPerStaff)
        /// }
        /// </example>
        /// 
        /// <paramref name="bars"/>
        /// Note that:
        ///     Argument 1 is the algorithm's completed bars object,
        ///     Argument 2 is the algorithm' PageFormat.VoiceIndicesPerStaff (set by the AssistantComposerForm dialog).
        protected List<List<SortedDictionary<int, string>>> GetEmptyClefChangesPerBarPerStaff(List<Bar> bars , List<List<int>> voiceIndicesPerStaff)
        {
            var rval = new List<List<SortedDictionary<int, string>>>();
            foreach(var _ in bars)
            {
                List<SortedDictionary<int, string>> barList = new List<SortedDictionary<int, string>>();
                foreach(var voiceIndices in voiceIndicesPerStaff)
                {
                    SortedDictionary<int, string> trkDict = new SortedDictionary<int, string>();
                    barList.Add(trkDict);
                }
                rval.Add(barList);
            }

            return rval;
        }

        protected void InsertClefChangesInBars(List<Bar> bars, List<List<int>> voiceIndicesPerStaff, List<List<SortedDictionary<int, string>>> clefChangesPerBarPerStaff)
        {
            Debug.Assert(bars.Count == clefChangesPerBarPerStaff.Count);

            for(int barIndex = 0; barIndex < bars.Count; barIndex++)
            {
                var bar = bars[barIndex];
                for(var staffIndex = 0; staffIndex < voiceIndicesPerStaff.Count; ++staffIndex)
                {
                    var midiIndex = voiceIndicesPerStaff[staffIndex][0];
                    var trk = bar.Trks[midiIndex];
                    SortedDictionary<int, string> clefDict = clefChangesPerBarPerStaff[barIndex][staffIndex];
                    if(clefDict.Count > 0)
                    {
                        Dictionary<int, string> reversedDict = clefDict.Reverse().ToDictionary(pair => pair.Key, pair => pair.Value);
                        Debug.Assert(reversedDict.First().Key < trk.UniqueDefs.Count);
                        foreach(KeyValuePair<int, string> keyValuePair in reversedDict)
                        {
                            int index = keyValuePair.Key;
                            ClefDef clefDef = new ClefDef(keyValuePair.Value, trk.UniqueDefs[index].MsPositionReFirstUD);
                            trk.Insert(keyValuePair.Key, clefDef);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Lyrics can simply be attached to MidiChordDefs or InputChordDefs earlier in the algorithm, but this function
        /// provides the possibility of adding them all in one place.
        /// This function returns null or a SortedDictionary per VoiceDef in each bar.
        /// The SortedDictionary contains the index of the MidiChordDef or InputChordDef in the bar to which the associated
        /// lyric string will be attached. The index is of MidiChordDefs or InputChordDefs only, beginning with 0 for
        /// the first MidiChordDef or InptChordDef in the bar.
        /// Lyrics that are attached to top voices on a staff will, like dynamics, be automatically placed above the staff.
        /// </summary>
        /// <returns>null or a SortedDictionary per VoiceDef per bar</returns>
        /// <example>
        /// Example for Study3Sketch1Algorithm
        /// var lyricsPerBar = GetEmptyStringExtrasPerBar(nBars, nVoicesPerBar);
        ///
        /// SortedDictionary&lt;int, string&gt; bar2VoiceDef1 = lyricsPerBar[1][0]; // Bar 2 Voice 1.
        /// bar2VoiceDef1.Add(9, "lyric9");
        /// bar2VoiceDef1.Add(8, "lyric8");
        /// bar2VoiceDef1.Add(6, "lyric6");
        /// bar2VoiceDef1.Add(4, "lyric4");
        /// bar2VoiceDef1.Add(2, "lyric2");
        ///
        /// SortedDictionary&lt;int, string&gt; bar2VoiceDef2 = lyricsPerBar[1][1]; // Bar 2 Voice 2.
        /// bar2VoiceDef2.Add(9, "lyric9a");
        /// bar2VoiceDef2.Add(8, "lyric8a");
        /// bar2VoiceDef2.Add(6, "lyric6a");
        /// bar2VoiceDef2.Add(4, "lyric4a");
        /// bar2VoiceDef2.Add(2, "lyric2a");
        ///
        /// return lyricsPerBar;
        /// </example>
        protected virtual List<List<SortedDictionary<int, string>>> GetLyricsPerBar(int nBars, int nVoicesPerBar) { return null; }

        protected List<List<SortedDictionary<int, string>>> GetEmptyStringExtrasPerBar(int nBars, int nVoicesPerBar)
        {
            var rval = new List<List<SortedDictionary<int, string>>>();
            for(int barIndex = 0; barIndex < nBars; ++barIndex)
            {
                var voiceDefs = new List<SortedDictionary<int, string>>();
                rval.Add(voiceDefs);
                for(int voiceIndex = 0; voiceIndex < nVoicesPerBar; ++voiceIndex)
                {
                    var clefDict = new SortedDictionary<int, string>();
                    voiceDefs.Add(clefDict);
                }
            }

            // populate the clef changes dicts for example with code like: rval[0][0].Add(9, "t3");
            return rval;
        }

        /// <summary>
        /// Returns a list of (index, msPosition) KeyValuePairs.
        /// These are the (index, msPosition) of the barlines at which regions begin, and the (index, msPosition) of the final barline.
        /// The first KeyValuePair is (0,0), the last is the (index, msPosition) for the final barline in the score.
        /// The number of entries in the returned list is therefore 1 + bars.Count.
        /// </summary>
        protected List<(int index, int msPosition)> GetRegionBarlineIndexMsPosList(List<Bar> bars)
        {
            var rval = new List<(int index, int msPosition)>();

            int barlineMsPos = 0;
            int barsCount = bars.Count;
            for(int i = 0; i < barsCount; ++i)
            {
                if(RegionStartBarIndices.Contains(i))
                {
                    rval.Add((index: i, msPosition: barlineMsPos));
                }
                barlineMsPos += bars[i].MsDuration;
            }
            rval.Add((index: barsCount, msPosition: barlineMsPos));

            return rval;
        }


        /// <summary>
        /// The patch only needs to be set in the first chord in each trk,
        /// since it will be set by shunting if the Assistant Performer starts later.
        /// </summary>
        protected virtual void SetInitialChordControls(Bar bar1)
        {
            foreach(Trk trk in bar1.Trks)
            {
                /// Assigns a new MidiChordControlDef containing
                ///     AllControllersOff = true,
                ///     Preset = 0 (piano)
                /// to the first MidiChordDef in all interpretations.
                trk.SetInitialChordControl();
            }
        }
        
        protected List<Krystal> _krystals;
    }
}
