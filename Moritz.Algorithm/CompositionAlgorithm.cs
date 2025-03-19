using Krystals5ObjectLibrary;

using Moritz.Palettes;
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

        protected Palette GetPaletteByName(string paletteName)
        {
            Debug.Assert(_palettes != null && _palettes.Count > 0);
            Palette rval = null;
            foreach(Palette palette in _palettes)
            {
                if(string.Compare(palette.Name, paletteName) == 0)
                {
                    rval = palette;
                    break;
                }
            }
            Debug.Assert(rval != null);
            return rval;
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
        public abstract List<Bar> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes);

        /// <summary>
        /// This function returns null or a SortedDictionary per ChannelDef in each bar.
        /// An empty clefChanges list of the returned type can be
        ///     1. created by calling the protected function GetEmptyClefChangesPerBar(int nBars, int nVoicesPerBar) and
        ///     2. populated with code such as clefChanges[barIndex][voiceIndex].Add(9, "t3"). 
        /// The dictionary contains the index at which the clef will be inserted in the ChannelDef's IUniqueDefs,
        /// and the clef ID string ("t", "t1", "b3" etc.).
        /// Clefs will be inserted in reverse order of the Sorted dictionary, so that the indices are those of
        /// the existing IUniqueDefs before which the clef will be inserted.
        /// The SortedDictionaries should not contain the initial clefs per channelDef - those will be included
        /// automatically.
        /// Note that a CautionaryChordDef counts as an IUniqueDef at the beginning of a bar, and that clefs
        /// cannot be inserted in front of them.
        /// Clefs should not be inserted here in the lower of two voices in a staff. Lower voices automatically have the
        /// SmallClefs that are defined for the upper voice.
        /// </summary>
        /// <example>
        /// Example for Study3Sketch1Algorithm
        /// var clefChangesPerBar = GetEmptyStringExtrasPerBar(nBars, nVoicesPerBar);
        ///
        /// SortedDictionary;lt;int, string&gt; voiceDef0Bar0 = clefChangesPerBar[0][0];
        /// voiceDef0Bar0.Add(9, "b3");
        /// voiceDef0Bar0.Add(8, "b2");
        /// voiceDef0Bar0.Add(6, "b");
        /// voiceDef0Bar0.Add(4, "t2");
        /// voiceDef0Bar0.Add(2, "t");
        ///
        /// The following were redundant in this score, since they only apply to rests!
        /// voiceDef0Bar0.Add(7, "b1");
        /// voiceDef0Bar0.Add(5, "t3");
        /// voiceDef0Bar0.Add(3, "t1");
        ///
        /// return clefChangesPerBar;
        /// </example>
        protected abstract List<List<SortedDictionary<int, string>>> GetClefChangesPerBar(int nBars, int nVoicesPerBar);

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
        protected List<int> GetBarlinePositions(IReadOnlyList<ChannelDef> channelDefs, List<double> approximateBarlineMsPositions)
        {
            List<int> barlineMsPositions = new List<int>();

            foreach(double approxMsPos in approximateBarlineMsPositions)
            {
                int barlineMsPos = 0;
                barlineMsPos = NearestAbsUIDEndMsPosition(channelDefs, approxMsPos);

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
        private int NearestAbsUIDEndMsPosition(IReadOnlyList<ChannelDef> channelDefs, double approxAbsMsPosition)
        {
            int nearestAbsUIDEndMsPosition = 0;
            double diff = double.MaxValue;
            foreach(ChannelDef channelDef in channelDefs)
            {
                for(int uidIndex = 0; uidIndex < channelDef.Count; ++uidIndex)
                {
                    IUniqueDef iud = channelDef[uidIndex];
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
        /// The Bars are as equal in duration as possible, with each barline being at the end of at least one IUniqueDef.
        /// The returned list contains no duplicates (A Debug.Assertion fails otherwise).
        /// </summary>
        /// <returns></returns>
        /// <param name="trks"></param>
        /// <param name="inputVoiceDefs">Can be null</param>
        /// <param name="nBars"></param>
        /// <returns></returns>
        public List<int> GetBalancedBarlineMsPositions(Seq seq, int nBars)
        {
            IReadOnlyList<ChannelDef> channelDefs = seq.ChannelDefs;

            int msDuration = channelDefs[0].MsDuration;

            double approxBarMsDuration = (((double)msDuration) / nBars);
            Debug.Assert(approxBarMsDuration * nBars == msDuration);

            List<int> barlineMsPositions = new List<int>();

            for(int barNumber = 1; barNumber <= nBars; ++barNumber)
            {
                double approxBarMsPosition = approxBarMsDuration * barNumber;
                int barMsPosition = NearestAbsUIDEndMsPosition(channelDefs, approxBarMsPosition);

                Debug.Assert(barlineMsPositions.Contains(barMsPosition) == false);

                barlineMsPositions.Add(barMsPosition);
            }
            Debug.Assert(barlineMsPositions[barlineMsPositions.Count - 1] == msDuration);

            return barlineMsPositions;
        }

        /// <summary>
        /// Uses the private CompositionAlgorithm.MainBar(...) constructor to create Bar objects.
        /// </summary>
        /// <param name="mainSeq">A Seq containing all the output IUniqueDefs in the composition.</param>
        /// <param name="inputVoiceDefs">Can be null, or contains all the input IUniqueDefs in the composition.</param>
        /// <param name="barlineMsPositions">All the barline msPositions (except the first).</param>
        /// <param name="clefChangesPerBar">Can be null.</param>
        /// <param name="lyricsPerBar">Can be null.</param>
        /// <returns>A list of Bars</returns>
        protected List<Bar> GetBars(Seq mainSeq, List<int> barlineMsPositions, List<List<SortedDictionary<int, string>>> clefChangesPerBar, List<List<SortedDictionary<int, string>>> lyricsPerBar)
        {
            MainBar mainBar = new MainBar(mainSeq);

            List<Bar> bars = mainBar.GetBars(barlineMsPositions);

            if(clefChangesPerBar != null)
            {
                InsertClefChangesInBars(bars, clefChangesPerBar);
            }
            if(lyricsPerBar != null)
            {
                AddLyricsToBars(bars, lyricsPerBar);
            }

            return bars;
        }

        private void InsertClefChangesInBars(List<Bar> bars, List<List<SortedDictionary<int, string>>> clefChangesPerBar)
        {
            Debug.Assert(bars.Count == clefChangesPerBar.Count);

            for(int i = 0; i < bars.Count; i++)
            {
                IReadOnlyList<ChannelDef> barVoiceDefs = bars[i].ChannelDefs;
                List<SortedDictionary<int, string>> clefChangesPerVoiceDef = clefChangesPerBar[i];
                Debug.Assert(barVoiceDefs.Count == clefChangesPerVoiceDef.Count);
                for(int voiceDefIndex = 0; voiceDefIndex < barVoiceDefs.Count; voiceDefIndex++)
                {
                    SortedDictionary<int, string> clefChanges = clefChangesPerVoiceDef[voiceDefIndex];
                    if(clefChanges.Count > 0)
                    {
                        ChannelDef channelDef = barVoiceDefs[voiceDefIndex];
                        InsertClefChangesInVoiceDef(channelDef, clefChanges);
                    }
                }
            }
        }

        private static void InsertClefChangesInVoiceDef(ChannelDef channelDef, SortedDictionary<int, string> clefChanges)
        {
            List<int> reversedKeys = new List<int>();
            foreach(int key in clefChanges.Keys)
            {
                reversedKeys.Add(key);
            }
            reversedKeys.Reverse();

            foreach(int key in reversedKeys)
            {
                string clef = clefChanges[key];
                channelDef.InsertClefDef(key, clef);
            }
        }

        private void AddLyricsToBars(List<Bar> bars, List<List<SortedDictionary<int, string>>> lyricsPerBar)
        {
            Debug.Assert(bars.Count == lyricsPerBar.Count);

            for(int i = 0; i < bars.Count; i++)
            {
                IReadOnlyList<ChannelDef> barVoiceDefs = bars[i].ChannelDefs;
                List<SortedDictionary<int, string>> lyricsPerVoiceDef = lyricsPerBar[i];
                Debug.Assert(barVoiceDefs.Count == lyricsPerVoiceDef.Count);
                for(int voiceDefIndex = 0; voiceDefIndex < barVoiceDefs.Count; voiceDefIndex++)
                {
                    ChannelDef channelDef = barVoiceDefs[voiceDefIndex];
                    SortedDictionary<int, string> lyrics = lyricsPerVoiceDef[voiceDefIndex];
                    AddLyricsToVoiceDef(channelDef, lyrics);
                }
            }
        }

        private static void AddLyricsToVoiceDef(ChannelDef channelDef, SortedDictionary<int, string> lyrics)
        {
            if(lyrics.Count > 0)
            {
                var mcds = new List<MidiChordDef>();
                foreach(IUniqueDef iud in channelDef.UniqueDefs)
                {
                    if(iud is MidiChordDef mcd)
                    {
                        mcds.Add(mcd);
                    }
                }
                Debug.Assert(lyrics.Count <= mcds.Count);
                foreach(int key in lyrics.Keys)
                {
                    mcds[key].Lyric = lyrics[key];
                }

            }
        }

        protected List<Krystal> _krystals;
        protected List<Palette> _palettes;

        private class MainBar : Bar
        {
            /// <summary>
            /// Used only by functions in this class.
            /// </summary>
            private MainBar()
                : base()
            {
            }

            /// <summary>
            /// A MainBar contains a list of channelDefs, that contain Trks. A Seq contain Trks.
            /// As with all Bars, a MainBar does not contain barlines. They are implicit, at the beginning and end of the MainBar.
            /// This constructor uses its arguments' channelDefs directly, so if the arguments need to be used again, pass clones.
            /// <para>MainBar consistency is identical to Bar consistency, with the further restrictions that AbsMsPosition must be 0
            /// and initalClefPerChannel may not be null.</para>
            /// <para>For further documentation about MainBar and Bar consistency, see their private AssertConsistency() functions.
            /// </summary>
            /// <param name="seq">Cannot be null, and must have Trks</param>
            public MainBar(Seq seq)
                : base(seq)
            {
                AssertConsistency();
            }

            /// <summary>
            /// 1. base.AssertConsistency() is called. (The base Bar must be consistent.)
            /// 2. AbsMsPosition must be 0.
            /// 3. InitialClefPerChannel == null || InitialClefPerChannel.Count == ChannelDefs.Count.
            /// 4. At least one Trk must end with a MidiChordDef.
            /// </summary> 
            public override void AssertConsistency()
            {
                base.AssertConsistency();
                Debug.Assert(AbsMsPosition == 0);

                #region At least one Trk must end with a MidiChordDef.
                IReadOnlyList<Trk> trks = Trks;
                bool endFound = false;
                foreach(Trk trk in trks)
                {
                    List<IUniqueDef> iuds = trk.UniqueDefs;
                    IUniqueDef lastIud = iuds[iuds.Count - 1];
                    if(lastIud is MidiChordDef)
                    {
                        endFound = true;
                        break;
                    }
                }
                Debug.Assert(endFound, "MidiChordDef not found at end.");
                #endregion
            }

            /// Converts this MainBar to a list of bars, consuming this bar's channelDefs.
            /// Uses the argument barline msPositions as the EndBarlines of the returned bars (which don't contain barlines).
            /// An exception is thrown if:
            ///    1) the first argument value is less than or equal to 0.
            ///    2) the argument contains duplicate msPositions.
            ///    3) the argument is not in ascending order.
            ///    4) a Trk.MsPositionReContainer is not 0.
            ///    5) an msPosition is not the endMsPosition of any IUniqueDef in the seq.
            public List<Bar> GetBars(List<int> barlineMsPositions)
            {
                CheckBarlineMsPositions(barlineMsPositions);

                List<int> barMsDurations = new List<int>();
                int startMsPos = 0;
                for(int i = 0; i < barlineMsPositions.Count; i++)
                {
                    int endMsPos = barlineMsPositions[i];
                    barMsDurations.Add(endMsPos - startMsPos);
                    startMsPos = endMsPos;
                }

                List<Bar> bars = new List<Bar>();
                int totalDurationBeforePop = this.MsDuration;
                Bar remainingBar = (Bar)this;
                foreach(int barMsDuration in barMsDurations)
                {
                    Tuple<Bar, Bar> rTuple = PopBar(remainingBar, barMsDuration);
                    Bar poppedBar = rTuple.Item1;
                    remainingBar = rTuple.Item2; // null after the last pop.

                    Debug.Assert(poppedBar.MsDuration == barMsDuration);
                    if(remainingBar != null)
                    {
                        Debug.Assert(poppedBar.MsDuration + remainingBar.MsDuration == totalDurationBeforePop);
                        totalDurationBeforePop = remainingBar.MsDuration;
                    }
                    else
                    {
                        Debug.Assert(poppedBar.MsDuration == totalDurationBeforePop);
                    }

                    bars.Add(poppedBar);
                }

                return bars;
            }

            /// <summary>
            /// Returns a Tuple in which Item1 is the popped bar, Item2 is the remaining part of the input bar.
            /// Note that Trks at the same level inside each ChannelDef in each bar have the same duration.
            /// </summary>
            /// <param name ="bar">The bar fron which Item1 is popped.</param>
            /// <param name="poppedBarMsDuration">The duration of the first Trk in each ChannelDef in the popped bar.</param>
            /// <returns>The popped bar and the remaining part of the input bar</returns>
            private Tuple<Bar, Bar> PopBar(Bar bar, int poppedBarMsDuration)
            {
                Debug.Assert(poppedBarMsDuration > 0);

                if(poppedBarMsDuration == bar.MsDuration)
                {
                    return new Tuple<Bar, Bar>(bar, null);
                }

                List<ChannelDef> poppedChannelDefs = new List<ChannelDef>();
                List<ChannelDef> remainingChannelDefs = new List<ChannelDef>();

                foreach(ChannelDef channelDef in bar.ChannelDefs)
                {
                    Tuple<ChannelDef, ChannelDef> channelDefs = PopChannelDef(channelDef, poppedBarMsDuration);

                    poppedChannelDefs.Add(channelDefs.Item1);
                    remainingChannelDefs.Add(channelDefs.Item2);
                }

                var poppedSeq = new Seq(bar.AbsMsPosition, poppedChannelDefs);
                var remainingSeq = new Seq(bar.AbsMsPosition, remainingChannelDefs);

                Bar poppedBar = new MainBar(poppedSeq);
                Bar remainingBar = new MainBar(remainingSeq);

                return new Tuple<Bar, Bar>(poppedBar, remainingBar);
            }

            /// <summary>
            /// Returns two ChannelDefs (each has the same channel index)
            /// Item1.Trks[0] contains the IUniqueDefs that begin within the poppedMsDuration.
            /// Item2.Trks[0] contains the remaining IUniqueDefs from the original channelDef.Trks.
            /// The remaining Trks in Item1 and Item2 are parallel IUniqueDefs (that can have other durations).
            /// The popped IUniqueDefs are removed from the current channelDef before returning it as Item2.
            /// MidiRestDefs and MidiChordDefs are split as necessary to fit the required Trk[0] duration.
            /// </summary>
            /// <param name="channelDef"></param>
            /// <param name="poppedBarMsDuration"></param>
            /// <returns></returns>
            private Tuple<ChannelDef, ChannelDef> PopChannelDef(ChannelDef channelDef, int poppedMsDuration)
            {
                Tuple<Trk, Trk> trks = PopTrk(channelDef.Trks[0], poppedMsDuration);

                Trk poppedTrk0 = trks.Item1;
                Trk remainingTrk0 = trks.Item2;

                List<Trk> poppedTrks = new List<Trk> { poppedTrk0 };
                List<Trk> remainingTrks = new List<Trk> { remainingTrk0 };

                int nUniqueDefs = poppedTrk0.UniqueDefs.Count;
                List<Trk> channelTrks = channelDef.Trks;

                for(int trkIndex = 1; trkIndex < channelTrks.Count; ++trkIndex)
                {
                    Trk poppedTrk = new Trk();
                    List<IUniqueDef> originalUids = channelTrks[trkIndex].UniqueDefs;
                    for(int uidIndex = 0; uidIndex < nUniqueDefs; ++uidIndex)
                    {
                        poppedTrk.UniqueDefs.Add(originalUids[0]);
                        originalUids.RemoveAt(0);
                    }
                    poppedTrks.Add(poppedTrk);
                    remainingTrks.Add(new Trk(0, originalUids));
                }

                ChannelDef poppedChannelDef = new ChannelDef(poppedTrks);
                ChannelDef remainingChannelDef = new ChannelDef(remainingTrks);

                return new Tuple<ChannelDef, ChannelDef>(poppedChannelDef, remainingChannelDef);
            }

            private Tuple<Trk, Trk> PopTrk(Trk trk, int poppedMsDuration)
            {
                Trk poppedTrk = new Trk(trk.MsPositionReContainer, new List<IUniqueDef>());
                Trk remainingTrk = new Trk(trk.MsPositionReContainer + poppedMsDuration, new List<IUniqueDef>());

                foreach(IUniqueDef iud in trk.UniqueDefs)
                {
                    int iudMsDuration = iud.MsDuration;
                    int iudStartPos = iud.MsPositionReFirstUD;
                    int iudEndPos = iudStartPos + iudMsDuration;

                    if(iudStartPos >= poppedMsDuration)
                    {
                        if(iud is ClefDef && iudStartPos == poppedMsDuration)
                        {
                            poppedTrk.UniqueDefs.Add(iud);
                        }
                        else
                        {
                            remainingTrk.UniqueDefs.Add(iud);
                        }
                    }
                    else if(iudEndPos > poppedMsDuration)
                    {
                        int durationBeforeBarline = poppedMsDuration - iudStartPos;
                        int durationAfterBarline = iudEndPos - poppedMsDuration;
                        if(iud is MidiRestDef)
                        {
                            // This is a rest. Split it.
                            MidiRestDef firstRestHalf = new MidiRestDef(iudStartPos, durationBeforeBarline);
                            poppedTrk.UniqueDefs.Add(firstRestHalf);

                            MidiRestDef secondRestHalf = new MidiRestDef(poppedMsDuration, durationAfterBarline);
                            remainingTrk.UniqueDefs.Add(secondRestHalf);
                        }
                        if(iud is CautionaryChordDef)
                        {
                            Debug.Assert(false, "There shouldnt be any cautionary chords here.");
                            // This error can happen if an attempt is made to set barlines too close together,
                            // i.e. (I think) if an attempt is made to create a bar that contains nothing... 
                        }
                        else if(iud is MidiChordDef)
                        {
                            IUniqueSplittableChordDef uniqueChordDef = iud as IUniqueSplittableChordDef;
                            uniqueChordDef.MsDurationToNextBarline = durationBeforeBarline;
                            poppedTrk.UniqueDefs.Add(uniqueChordDef);

                            Debug.Assert(remainingTrk.UniqueDefs.Count == 0);
                            CautionaryChordDef ccd = new CautionaryChordDef(uniqueChordDef, 0, durationAfterBarline);
                            remainingTrk.UniqueDefs.Add(ccd);
                        }
                    }
                    else
                    {
                        Debug.Assert(iudEndPos <= poppedMsDuration && iudStartPos >= 0);
                        poppedTrk.UniqueDefs.Add(iud);
                    }
                }

                return new Tuple<Trk, Trk>(poppedTrk, remainingTrk);
        }

            /// <summary>
            /// An exception is thrown if:
            ///    1) the first argument value is less than or equal to 0.
            ///    2) the argument contains duplicate msPositions.
            ///    3) the argument is not in ascending order.
            ///    4) a ChannelDef.MsPositionReContainer is not 0.
            ///    5) if an msPosition is not the endMsPosition of any IUniqueDef in the Trks.
            /// </summary>
            private void CheckBarlineMsPositions(IReadOnlyList<int> barlineMsPositionsReThisBar)
            {
                Debug.Assert(barlineMsPositionsReThisBar[0] > 0, "The first msPosition must be greater than 0.");

                for(int i = 0; i < barlineMsPositionsReThisBar.Count; ++i)
                {
                    int msPosition = barlineMsPositionsReThisBar[i];
                    Debug.Assert(msPosition <= this.MsDuration);
                    for(int j = i + 1; j < barlineMsPositionsReThisBar.Count; ++j)
                    {
                        Debug.Assert(msPosition != barlineMsPositionsReThisBar[j], "Error: Duplicate barline msPositions.");
                    }
                }

                int currentMsPos = -1;
                foreach(int msPosition in barlineMsPositionsReThisBar)
                {
                    Debug.Assert(msPosition > currentMsPos, "Value out of order.");
                    currentMsPos = msPosition;
                    bool found = false;
                    for(int i = ChannelDefs.Count - 1; i >= 0; --i)
                    {
                        ChannelDef channelDef = ChannelDefs[i];

                        Debug.Assert(channelDef.MsPositionReContainer == 0);
                        foreach(IUniqueDef iud in channelDef.UniqueDefs)
                        {
                            if(msPosition == (iud.MsPositionReFirstUD + iud.MsDuration))
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    Debug.Assert(found, "Error: barline must be at the endMsPosition of at least one IUniqueDef in a Trk.");
                }
            }
        }
    }
}
