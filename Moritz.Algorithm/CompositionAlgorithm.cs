using Krystals5ObjectLibrary;

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
    /// When called, the DoAlgorithm() function returns a list of ChannelDef lists,
    /// whereby each contained ChannelDef list is the definition of a bar (a bar is
    /// a place where a system can be broken).
    /// Algorithms don't control the page format, how many bars per system there
    /// are, or the shapes of the symbols. Those things are set for a particular
    /// score in an .mkss file using the Assistant Composer's main form.
    /// The ChannelDefs returned from DoAlgorithm() are converted to real Voices
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
            int channelCount = NumberOfMidiChannels;
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
        /// Defines the number of MIDI channels used by the algorithm.
        /// This number must be in range [1..16].
        /// Each voice in a score has a particular midi channel, so the number of midi channels is also the number of voices.
        /// Midi channels are always allocated from top to bottom in each score. 
        /// Notes:
        /// The association of voices with staves is controlled using an input field in the Assistant Composer's main dialog.
        /// Each Voice contains a list of Trk. Each Trk contains a different interpretation (=performance) of the voice's graphics.
        /// Each Voice can contain more than one (performance) track (Trk), all such tracks have the same midi channel.
        /// </summary>
        public abstract int NumberOfMidiChannels { get; }

        /// <summary>
        /// Returns the number of bars (=bar definitions) created by the algorithm.
        /// </summary>
        /// <returns></returns>
        public abstract int NumberOfBars { get; }

        public virtual IReadOnlyList<int> RegionStartBarIndices { get { return new List<int>(); } }

        /// <summary>
        /// The DoAlgorithm() function is special to a particular composition.
        /// This function returns a sequence of abstract bar definitions, devoid of layout information.
        /// Each bar definition is a list of voice definitions (ChannelDefs), The ChannelDefs are conceptually
        /// in the default top to bottom order of the voices in a final score. The actual order in which
        /// the voices are eventually printed is controlled using the Assistant Composer's layout options,
        /// Each bar definition in the sequence returned by this function contains the same number of
        /// ChannelDefs. ChannelDefs at the same index in each bar are continuations of the same overall voice
        /// definition, and may be concatenated to create multiple bars on a staff.
        /// Each ChannelDef returned by this function contains a list of UniqueDef objects (ChannelDef.UniqueDefs).
        /// When the Assistant Composer creates a real score, each of these UniqueDef objects is converted to
        /// a real NoteObject containing layout information (by a Notator), and the NoteObject then added to a
        /// concrete Voice.NoteObjects list. See Notator.AddSymbolsToSystems().
        /// ACHTUNG:
        /// The top (=first) ChannelDef in each bar definition must be a TrkDef.
        /// This can be followed by zero or more OutputVoices, followed by zero or more InputVoices.
        /// The chord definitions in TrkDef.UniqueDefs must be MidiChordDefs.
        /// The chord definitions in InputVoice.UniqueDefs must be InputChordDefs.
        /// Algorithms declare the number of output and input voices they construct by defining the
        /// MidiChannelPerOutputVoice and NumberOfInputVoices properties (see above).
        /// For convenience in the Assistant Composer, the number of bars is also returned (in the
        /// NumberOfBars property).
        /// If one or more InputVoices are defined, then an TrkOptions object must be created, given
        /// default values, and assigned to this.TrkOptions (see below).
        /// 
        /// A note about voiceIDs and midi channels in scores:
        /// The Assistant Composer allocates the voiceIDs saved in the score automatically when the score is 
        /// created. Each VoiceID is its index in the original bars created by the algorithm. (The top-bottom 
        /// order of these voices in the final score is set using the Assistant Composer's layout options.)
        /// An algorithm associates each voice (voiceID) with a particular midi channel by setting the
        /// MidiChannelPerOutputVoice property in the top to bottom order of the voices in the bars being
        /// created. This rigmarole allows algorithms to stipulate the standard midi percussion channel (channel
        /// index 9).
        /// An OutputVoice's midiChannel, voiceID (and masterVolume) are written only to each voice in the first
        /// system in the score. And the voiceID is only written if the score actually contains InputVoices.
        /// (VoiceIDs are only needed in score because these values are used as references by InputVoices.)
        /// Algorithms simply set the InputVoice references to OutputVoices (voiceIDs) by using their index
        /// in the default bar layout being created.
        /// </summary>
        public abstract List<Bar> DoAlgorithm(PageFormat pageFormat, List<Krystal> krystals);

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
        /// The SortedDictionaries should not contain the initial clefs per channelDef - those will be included
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
            foreach(var bar in bars)
            {
                List<SortedDictionary<int, string>> barList = new List<SortedDictionary<int, string>>();
                foreach(var voiceIndices in voiceIndicesPerStaff)
                {
                    SortedDictionary<int, string> trkDict = new SortedDictionary<int, string>();
                    barList.Add(trkDict);
                }
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
                    var trk = bar.ChannelDefs[midiIndex].Trks[0];
                    SortedDictionary<int, string> clefDict = clefChangesPerBarPerStaff[barIndex][staffIndex];
                    foreach(KeyValuePair<int, string> keyValuePair in clefDict)
                    {
                        int index = keyValuePair.Key;
                        ClefDef clefDef = new ClefDef(keyValuePair.Value, trk.UniqueDefs[index].MsPositionReFirstUD);
                        trk.Insert(keyValuePair.Key, clefDef);
                    }
                }
            }
        }

        /// <summary>
        /// Lyrics can simply be attached to MidiChordDefs or InputChordDefs earlier in the algorithm, but this function
        /// provides the possibility of adding them all in one place.
        /// This function returns null or a SortedDictionary per ChannelDef in each bar.
        /// The SortedDictionary contains the index of the MidiChordDef or InputChordDef in the bar to which the associated
        /// lyric string will be attached. The index is of MidiChordDefs or InputChordDefs only, beginning with 0 for
        /// the first MidiChordDef or InptChordDef in the bar.
        /// Lyrics that are attached to top voices on a staff will, like dynamics, be automatically placed above the staff.
        /// </summary>
        /// <returns>null or a SortedDictionary per ChannelDef per bar</returns>
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
                var channelDefs = new List<SortedDictionary<int, string>>();
                rval.Add(channelDefs);
                for(int voiceIndex = 0; voiceIndex < nVoicesPerBar; ++voiceIndex)
                {
                    var clefDict = new SortedDictionary<int, string>();
                    channelDefs.Add(clefDict);
                }
            }

            // populate the clef changes dicts for example with code like: rval[0][0].Add(9, "t3");
            return rval;
        }

        /// <summary>
        /// Barlines will be fit to MidiChordDefs in the trks.
        /// </summary>
        /// <param name="mainSeq"></param>
        /// <param name="inputVoiceDefs">can be null</param>
        /// <param name="approximateBarlineMsPositions"></param>
        /// <returns></returns>
        protected List<int> GetBarlinePositions(IReadOnlyList<Trk> trks0, List<double> approximateBarlineMsPositions)
        {
            List<int> barlineMsPositions = new List<int>();

            foreach(double approxMsPos in approximateBarlineMsPositions)
            {
                int barlineMsPos = 0;
                barlineMsPos = NearestAbsUIDEndMsPosition(trks0, approxMsPos);

                barlineMsPositions.Add(barlineMsPos);
            }
            return barlineMsPositions;
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
        private int NearestAbsUIDEndMsPosition(IReadOnlyList<Trk> trks, double approxAbsMsPosition)
        {
            int nearestAbsUIDEndMsPosition = 0;
            double diff = double.MaxValue;
            foreach(var trk in trks)
            {
                for(int uidIndex = 0; uidIndex < trk.UniqueDefs.Count; ++uidIndex)
                {
                    IUniqueDef iud = trk[uidIndex];
                    int absEndPos = iud.MsPositionReFirstUD + iud.MsDuration;
                    double localDiff = Math.Abs(approxAbsMsPosition - absEndPos);
                    if(localDiff < diff)
                    {
                        diff = localDiff;
                        nearestAbsUIDEndMsPosition = absEndPos;
                    }
                    if(diff == 0)
                    {
                        break;
                    }
                }
                if(diff == 0)
                {
                    break;
                }
            }
            return nearestAbsUIDEndMsPosition;
        }

        /// <summary>
        /// Returns nBars barlineMsPositions.
        /// The Bars are as equal in duration as possible, with each barline being at the end of at least one IUniqueDef in the top Trk of a ChannelDef.
        /// The returned list contains no duplicates (A Debug.Assertion fails otherwise).
        /// </summary>
        /// <returns></returns>
        /// <param name="trks"></param>
        /// <param name="inputVoiceDefs">Can be null</param>
        /// <param name="nBars"></param>
        /// <returns></returns>
        public List<int> GetBalancedBarlineMsPositions(List<Trk> trks0, int nBars)
        {            
            int msDuration = trks0[0].MsDuration; // all the trks0 have the same MsDuration

            double approxBarMsDuration = (((double)msDuration) / nBars);
            Debug.Assert(approxBarMsDuration * nBars == msDuration);

            List<int> barlineMsPositions = new List<int>();

            for(int barNumber = 1; barNumber <= nBars; ++barNumber)
            {
                double approxBarMsPosition = approxBarMsDuration * barNumber;
                int barMsPosition = NearestAbsUIDEndMsPosition(trks0, approxBarMsPosition);

                Debug.Assert(barlineMsPositions.Contains(barMsPosition) == false);

                barlineMsPositions.Add(barMsPosition);
            }
            Debug.Assert(barlineMsPositions[barlineMsPositions.Count - 1] == msDuration);

            return barlineMsPositions;
        }

        /// <summary>
        /// Uses the private CompositionAlgorithm.MainBar(...) constructor to create Bar objects.
        /// </summary>
        /// <param name="mainBar">A Bar containing all the output IUniqueDefs in the composition.</param>
        /// <param name="barlineMsPositions">All the barline msPositions (except the first).</param>
        /// <returns>A list of Bars</returns>
        protected List<Bar> GetBars(Bar mainBar, List<int> barlineMsPositions)
        {
            mainBar.AssertConsistency();

            List<Bar> bars = mainBar.GetBars(barlineMsPositions);

            foreach(var bar in bars)
            {
                // Each Trk can begin with a CautionaryChordDef and contains no ClefDefs.
                bar.AssertConsistency();
            }

            return bars;
        }

        /// <summary>
        /// The patch only needs to be set in the first chord in each trk,
        /// since it will be set by shunting if the Assistant Performer starts later.
        /// </summary>
        protected void SetPatch0InTheFirstChordInEachVoice(Bar bar1)
        {
            MidiChordDef midiChordDef = null;
            foreach(ChannelDef channelDef in bar1.ChannelDefs)
            {
                foreach(var trk in channelDef.Trks)
                {
                    foreach(IUniqueDef iUniqueDef in trk.UniqueDefs)
                    {
                        midiChordDef = iUniqueDef as MidiChordDef;
                        if(midiChordDef != null)
                        {
                            midiChordDef.MidiChordControlDef.Preset = 0;
                            break;
                        }
                    }
                }
            }
        }

        protected List<Krystal> _krystals;
    }
}
