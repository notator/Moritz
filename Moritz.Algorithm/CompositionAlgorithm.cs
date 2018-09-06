using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Palettes;
using Moritz.Spec;
using Moritz.Symbols;

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
			#region check output channels
            int outputChannelCount = MidiChannelPerOutputVoice.Count;
            if(outputChannelCount < 1)
                throw new ApplicationException("CompositionAlgorithm: There must be at least one output voice!");
            if(outputChannelCount > 16)
                throw new ApplicationException("CompositionAlgorithm: There cannot be more than 16 output voices.");

            int previousChannelIndex = -1;
            for(int i = 0; i < outputChannelCount; ++i)
            {
				int channelIndex = MidiChannelPerOutputVoice[i];

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
			#region check input channels
			// See the comment on the definition of MidiChannelPerInputVoice.
			if(MidiChannelPerInputVoice != null)
			{
				var mcipiv = MidiChannelPerInputVoice;
				if(mcipiv.Count == 0 || mcipiv[0] != 0)
				{
					throw new ApplicationException("CompositionAlgorithm: the first input channel must be 0!");
				}
				if(mcipiv.Count > 4)
				{
					throw new ApplicationException("CompositionAlgorithm: too many input voices!");
				}
				int channel0Count = 0;
				int channel1Count = 0;
				var prevChannel = -1;
				foreach(var channel in mcipiv)
				{
					if(channel < 0 || channel > 1)
					{
						throw new ApplicationException("CompositionAlgorithm: input channel out of range!");
					}
					if(channel != prevChannel && channel != (prevChannel + 1))
					{
						throw new ApplicationException("CompositionAlgorithm: input channels must be in numerical order!");
					}
					prevChannel = channel;
					channel0Count = (channel == 0) ? channel0Count + 1 : channel0Count;
					channel1Count = (channel == 1) ? channel1Count + 1 : channel1Count;
				}
				if(channel0Count < 1 || channel0Count > 2)
				{
					throw new ApplicationException("CompositionAlgorithm: input channel 0 must occur 1 or 2 times.");
				}
				if(channel1Count != 0 && channel1Count > 2)
				{
					throw new ApplicationException("CompositionAlgorithm: input channel 1 may occur 0, 1 or 2 times.");
				}
			}
			#endregion

            if(NumberOfBars <= 0)
                throw new ApplicationException("CompositionAlgorithm: There must be at least one bar!");

        }

		public virtual ScoreData SetScoreData(List<Bar> bars) { return null; }

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
		/// Defines the midi channel index for each output voice, in the top to bottom order of the output voices in each system.
		/// This list may not be empty.
		/// Each midi channel (in range [0..15]) can can only be used once, and (apart from channel index 9 -- the percussion
		/// channel), must be allocated contiguously in top-bottom order starting at channel index 0.
		/// Channel index 9 can be allocated (once) at any voice index.
		/// Notes:
		/// Midi channels are always allocated to tracks (voices) in this top-bottom order. Contrary to previous versions of
		/// Moritz, the channel order cannot be changed, and all tracks are always visible.
		/// The association of voices with staves continues to be controlled using an input field in the Assistant Composer's
		/// main dialog.
		/// Moritz provides complete midi message information (including the midi channel info) for each output track, so
		/// the Assistant Performer can use this information at load time to create binary midi messages that are associated
		/// with each track.
		/// This means
		/// 1. that the Assistant Performer can ignore the channel info at run time, and simply refer to tracks by their index.
		/// 2. that Moritz can use the track indices (rather than midi channels) in the references to tracks in the input
		///    notes that it creates. That should simplify the code considerably re previous Moritz versions.
		/// The AP could provide track-channel info to users if necessary, but I think this could initially be done simply
		/// by using descriptive staff names in the score.
		/// </summary>
		public abstract IReadOnlyList<int> MidiChannelPerOutputVoice { get; }

		/// <summary>
		/// Defines the midi channel index for each input voice, in the top to bottom order of the input voices in each system.
		/// This list can be null, in which case there will be no input voices.
		/// Input voices are rendered larger than, and placed below, output voices in scores.
		/// 1. Currently, only midi input channels 0 and 1 are allowed. (These channels could be sent by a single,
		///    split keyboard.)
		/// 2. The first channel must be 0 with subsequent channels in numerical order.
		///    Channel 0 can be used once or twice. Channel 1 can be missing or used once or twice.
		///    (The possible alternatives are: {0}, {0,1}, {0,0,1}, {0,1,1} and {0,0,1,1}.)
		/// At load time, the Assistant Performer agglomerates input voices having the same midi input channel into a
		/// single input sequence.
		/// </summary>
		public abstract IReadOnlyList<int> MidiChannelPerInputVoice { get; }

        /// <summary>
        /// Returns the number of bars (=bar definitions) created by the algorithm.
        /// </summary>
        /// <returns></returns>
        public abstract int NumberOfBars { get; }

        /// <summary>
        /// The DoAlgorithm() function is special to a particular composition.
        /// This function returns a sequence of abstract bar definitions, devoid of layout information.
        /// Each bar definition is a list of voice definitions (VoiceDefs), The VoiceDefs are conceptually
        /// in the default top to bottom order of the voices in a final score. The actual order in which
        /// the voices are eventually printed is controlled using the Assistant Composer's layout options,
        /// Each bar definition in the sequence returned by this function contains the same number of
        /// VoiceDefs. VoiceDefs at the same index in each bar are continuations of the same overall voice
        /// definition, and may be concatenated to create multiple bars on a staff.
        /// Each VoiceDef returned by this function contains a list of UniqueDef objects (VoiceDef.UniqueDefs).
        /// When the Assistant Composer creates a real score, each of these UniqueDef objects is converted to
        /// a real NoteObject containing layout information (by a Notator), and the NoteObject then added to a
        /// concrete Voice.NoteObjects list. See Notator.AddSymbolsToSystems().
        /// ACHTUNG:
        /// The top (=first) VoiceDef in each bar definition must be a TrkDef.
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
		/// This function returns null or a SortedDictionary per VoiceDef in each bar.
		/// An empty clefChanges list of the returned type can be
		///     1. created by calling the protected function GetEmptyClefChangesPerBar(int nBars, int nVoicesPerBar) and
		///     2. populated with code such as clefChanges[barIndex][voiceIndex].Add(9, "t3"). 
		/// The dictionary contains the index at which the clef will be inserted in the VoiceDef's IUniqueDefs,
		/// and the clef ID string ("t", "t1", "b3" etc.).
		/// Clefs will be inserted in reverse order of the Sorted dictionary, so that the indices are those of
		/// the existing IUniqueDefs before which the clef will be inserted.
		/// The SortedDictionaries should not contain the initial clefs per voiceDef - those will be included
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
			for(int barIndex = 0; barIndex < nBars; ++barIndex )
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
		/// If inputVoiceDefs != null, then barlines will be fit to the InputChordDefs in the inputVoices,
		/// otherwise they will be fit to the MidiChordDefs in the trks.
		/// </summary>
		/// <param name="mainSeq"></param>
		/// <param name="inputVoiceDefs">can be null</param>
		/// <param name="approximateBarlineMsPositions"></param>
		/// <returns></returns>
		protected List<int> GetBarlinePositions(IReadOnlyList<Trk> trks, IReadOnlyList<InputVoiceDef> inputVoiceDefs, List<double> approximateBarlineMsPositions)
		{
			List<int> barlineMsPositions = new List<int>();

			List<VoiceDef> voiceDefs = null;
			if(inputVoiceDefs != null)
			{
				voiceDefs = new List<VoiceDef>(inputVoiceDefs);
			}
			else
			{
				voiceDefs = new List<VoiceDef>(trks);
			}
			foreach(double approxMsPos in approximateBarlineMsPositions)
			{
				int barlineMsPos = 0;
				barlineMsPos = NearestAbsUIDEndMsPosition(voiceDefs, approxMsPos);
				
				barlineMsPositions.Add(barlineMsPos);
			}
			return barlineMsPositions;
		}

		private int NearestAbsUIDEndMsPosition(List<VoiceDef> voiceDefs, double approxAbsMsPosition)
		{
			int nearestAbsUIDEndMsPosition = 0;
			double diff = double.MaxValue;
			foreach(VoiceDef voiceDef in voiceDefs)
			{
				for(int uidIndex = 0; uidIndex < voiceDef.Count; ++uidIndex)
				{
					IUniqueDef iud = voiceDef[uidIndex];
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
		public List<int> GetBalancedBarlineMsPositions(IReadOnlyList<Trk> trks, IReadOnlyList<InputVoiceDef> inputVoiceDefs, int nBars)
		{
			List<VoiceDef> voiceDefs = null;
			if(inputVoiceDefs != null)
			{
				voiceDefs = new List<VoiceDef>(inputVoiceDefs);
			}
			else
			{
				voiceDefs = new List<VoiceDef>(trks);
			}

			int msDuration = voiceDefs[0].MsDuration;

			double approxBarMsDuration = (((double)msDuration) / nBars);
			Debug.Assert(approxBarMsDuration * nBars == msDuration);

			List<int> barlineMsPositions = new List<int>();

			for(int barNumber = 1; barNumber <= nBars; ++barNumber)
			{
				double approxBarMsPosition = approxBarMsDuration * barNumber;
				int barMsPosition = NearestAbsUIDEndMsPosition(voiceDefs, approxBarMsPosition);

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
		protected List<Bar> GetBars(Seq mainSeq, List<InputVoiceDef> inputVoiceDefs, List<int> barlineMsPositions, List<List<SortedDictionary<int, string>>> clefChangesPerBar, List<List<SortedDictionary<int, string>>> lyricsPerBar)
		{
			MainBar mainBar = new MainBar(mainSeq, inputVoiceDefs);

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
				List<VoiceDef> barVoiceDefs = bars[i].VoiceDefs;
				List<SortedDictionary<int, string>> clefChangesPerVoiceDef = clefChangesPerBar[i];
				Debug.Assert(barVoiceDefs.Count == clefChangesPerVoiceDef.Count);
				for(int voiceDefIndex = 0; voiceDefIndex < barVoiceDefs.Count; voiceDefIndex++)
				{
					SortedDictionary<int, string> clefChanges = clefChangesPerVoiceDef[voiceDefIndex];
					if(clefChanges.Count > 0)
					{
						VoiceDef voiceDef = barVoiceDefs[voiceDefIndex];
						InsertClefChangesInVoiceDef(voiceDef, clefChanges);
					}
				}
			}
		}

		private static void InsertClefChangesInVoiceDef(VoiceDef voiceDef, SortedDictionary<int, string> clefChanges)
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
				voiceDef.InsertClefDef(key, clef);
			}
		}

		private void AddLyricsToBars(List<Bar> bars, List<List<SortedDictionary<int, string>>> lyricsPerBar)
		{
			Debug.Assert(bars.Count == lyricsPerBar.Count);

			for(int i = 0; i < bars.Count; i++)
			{
				List<VoiceDef> barVoiceDefs = bars[i].VoiceDefs;
				List<SortedDictionary<int, string>> lyricsPerVoiceDef = lyricsPerBar[i];
				Debug.Assert(barVoiceDefs.Count == lyricsPerVoiceDef.Count);
				for(int voiceDefIndex = 0; voiceDefIndex < barVoiceDefs.Count; voiceDefIndex++)
				{
					VoiceDef voiceDef = barVoiceDefs[voiceDefIndex];
					SortedDictionary<int, string> lyrics = lyricsPerVoiceDef[voiceDefIndex];
					AddLyricsToVoiceDef(voiceDef, lyrics);
				}
			}
		}

		private static void AddLyricsToVoiceDef(VoiceDef voiceDef, SortedDictionary<int, string> lyrics)
		{
			if(lyrics.Count > 0)
			{
				if(voiceDef is Trk)
				{
					var mcds = new List<MidiChordDef>();
					foreach(IUniqueDef iud in voiceDef.UniqueDefs)
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
				else
				{
					Debug.Assert(voiceDef is InputVoiceDef);
					var icds = new List<InputChordDef>();
					foreach(IUniqueDef iud in voiceDef.UniqueDefs)
					{
						if(iud is InputChordDef icd)
						{
							icds.Add(icd);
						}
					}
					Debug.Assert(lyrics.Count <= icds.Count);
					foreach(int key in lyrics.Keys)
					{
						icds[key].Lyric = lyrics[key];
					}
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
			/// A MainBar contains a list of voiceDefs, that can be of both kinds: Trks and InputVoiceDefs. A Seq can only contain Trks.
			/// As with all Bars, a MainBar does not contain barlines. They are implicit, at the beginning and end of the MainBar.
			/// This constructor uses its arguments' voiceDefs directly, so if the arguments need to be used again, pass clones.
			/// <para>MainBar consistency is identical to Bar consistency, with the further restrictions that AbsMsPosition must be 0
			/// and initalClefPerChannel may not be null.</para>
			/// <para>For further documentation about MainBar and Bar consistency, see their private AssertConsistency() functions.
			/// </summary>
			/// <param name="seq">Cannot be null, and must have Trks</param>
			/// <param name="inputVoiceDefs">This list can be null or empty</param>
			/// <param name="initialClefPerChannel">Cannot be null. Count must be seq.Trks.Count + inputVoiceDefs.Count</param>>
			public MainBar(Seq seq, IReadOnlyList<InputVoiceDef> inputVoiceDefs)
				: base(seq, inputVoiceDefs)
			{
				AssertConsistency();
			}

			/// <summary>
			/// 1. base.AssertConsistency() is called. (The base Bar must be consistent.)
			/// 2. AbsMsPosition must be 0.
			/// 3. InitialClefPerChannel == null || InitialClefPerChannel.Count == VoiceDefs.Count.
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

			/// Converts this MainBar to a list of bars, consuming this bar's voiceDefs.
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
			/// The popped bar has a list of voiceDefs containing the IUniqueDefs that
			/// begin within barDuration. These IUniqueDefs are removed from the current bar before returning it as Item2.
			/// MidiRestDefs and MidiChordDefs are split as necessary, so that when this
			/// function returns, both the popped bar and the current bar contain voiceDefs
			/// having the same msDuration. i.e.: Both the popped bar and the remaining bar "add up".
			/// </summary>
			/// <param name ="bar">The bar fron which the bar is popped.</param>
			/// <param name="barMsDuration">The duration of the popped bar.</param>
			/// <returns>The popped bar</returns>
			private Tuple<Bar, Bar> PopBar(Bar bar, int barMsDuration)
			{
				Debug.Assert(barMsDuration > 0);

				if(barMsDuration == bar.MsDuration)
				{
					return new Tuple<Bar, Bar>(bar, null);
				}

				Bar poppedBar = new Bar();
				Bar remainingBar = new Bar();
				int thisMsDuration = this.MsDuration;

				VoiceDef poppedBarVoice;
				VoiceDef remainingBarVoice;
				foreach(VoiceDef voiceDef in bar.VoiceDefs)
				{
					InputVoiceDef inputVoice = voiceDef as InputVoiceDef;
					if(voiceDef is Trk outputVoice)
					{
						poppedBarVoice = new Trk(outputVoice.MidiChannel) { Container = poppedBar };
						poppedBar.VoiceDefs.Add(poppedBarVoice);
						remainingBarVoice = new Trk(outputVoice.MidiChannel) { Container = remainingBar };
						remainingBar.VoiceDefs.Add(remainingBarVoice);
					}
					else
					{
						poppedBarVoice = new InputVoiceDef(inputVoice.MidiChannel, 0, new List<IUniqueDef>()) { Container = poppedBar };
						poppedBar.VoiceDefs.Add(poppedBarVoice);
						remainingBarVoice = new InputVoiceDef(inputVoice.MidiChannel, 0, new List<IUniqueDef>()) { Container = remainingBar };
						remainingBar.VoiceDefs.Add(remainingBarVoice);
					}
					foreach(IUniqueDef iud in voiceDef.UniqueDefs)
					{
						int iudMsDuration = iud.MsDuration;
						int iudStartPos = iud.MsPositionReFirstUD;
						int iudEndPos = iudStartPos + iudMsDuration;

						if(iudStartPos >= barMsDuration)
						{
							if(iud is ClefDef && iudStartPos == barMsDuration)
							{
								poppedBarVoice.UniqueDefs.Add(iud);
							}
							else
							{
								remainingBarVoice.UniqueDefs.Add(iud);
							}
						}
						else if(iudEndPos > barMsDuration)
						{
							int durationBeforeBarline = barMsDuration - iudStartPos;
							int durationAfterBarline = iudEndPos - barMsDuration;
							if(iud is MidiRestDef)
							{
								// This is a rest. Split it.
								MidiRestDef firstRestHalf = new MidiRestDef(iudStartPos, durationBeforeBarline);
								poppedBarVoice.UniqueDefs.Add(firstRestHalf);

								MidiRestDef secondRestHalf = new MidiRestDef(barMsDuration, durationAfterBarline);
								remainingBarVoice.UniqueDefs.Add(secondRestHalf);
							}
							if(iud is InputRestDef)
							{
								// This is a rest. Split it.
								InputRestDef firstRestHalf = new InputRestDef(iudStartPos, durationBeforeBarline);
								poppedBarVoice.UniqueDefs.Add(firstRestHalf);

								InputRestDef secondRestHalf = new InputRestDef(barMsDuration, durationAfterBarline);
								remainingBarVoice.UniqueDefs.Add(secondRestHalf);
							}
							else if(iud is CautionaryChordDef)
							{
								Debug.Assert(false, "There shouldnt be any cautionary chords here.");
								//// This is a cautionary chord. Set the position of the following barline, and
								//// Add a CautionaryChordDef at the beginning of the following bar.
								//iud.MsDuration = barMsDuration - iudStartPos;
								//poppedBarVoice.UniqueDefs.Add(iud);

								//Debug.Assert(remainingBarVoice.UniqueDefs.Count == 0);
								//CautionaryChordDef secondLmdd = new CautionaryChordDef((IUniqueChordDef)iud, 0, durationAfterBarline);
								//remainingBarVoice.UniqueDefs.Add(secondLmdd);
							}
							else if(iud is MidiChordDef || iud is InputChordDef)
							{
								IUniqueSplittableChordDef uniqueChordDef = iud as IUniqueSplittableChordDef;
								uniqueChordDef.MsDurationToNextBarline = durationBeforeBarline;
								poppedBarVoice.UniqueDefs.Add(uniqueChordDef);

								Debug.Assert(remainingBarVoice.UniqueDefs.Count == 0);
								CautionaryChordDef ccd = new CautionaryChordDef(uniqueChordDef, 0, durationAfterBarline);
								remainingBarVoice.UniqueDefs.Add(ccd);
							}
						}
						else
						{
							Debug.Assert(iudEndPos <= barMsDuration && iudStartPos >= 0);
							poppedBarVoice.UniqueDefs.Add(iud);
						}
					}
				}

				poppedBar.AbsMsPosition = remainingBar.AbsMsPosition;
				poppedBar.AssertConsistency();
				if(remainingBar != null)
				{
					remainingBar.AbsMsPosition += barMsDuration;
					remainingBar.SetMsPositionsReFirstUD();
					remainingBar.AssertConsistency();
				}

				return new Tuple<Bar, Bar>(poppedBar, remainingBar);
			}

			/// <summary>
			/// An exception is thrown if:
			///    1) the first argument value is less than or equal to 0.
			///    2) the argument contains duplicate msPositions.
			///    3) the argument is not in ascending order.
			///    4) a VoiceDef.MsPositionReContainer is not 0.
			///    5) if the bar contains InputVoiceDefs, an msPosition is not the endMsPosition of any IUniqueDef in the InputVoiceDefs
			///       else if an msPosition is not the endMsPosition of any IUniqueDef in the Trks.
			/// </summary>
			private void CheckBarlineMsPositions(IReadOnlyList<int> barlineMsPositionsReThisBar)
			{
				Debug.Assert(barlineMsPositionsReThisBar[0] > 0, "The first msPosition must be greater than 0.");

				int msDuration = this.MsDuration;
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
					bool inputVoiceDefFound = false;
					for(int i = _voiceDefs.Count - 1; i >= 0; --i)
					{
						VoiceDef voiceDef = _voiceDefs[i];

						Debug.Assert(voiceDef.MsPositionReContainer == 0);
						if(voiceDef is InputVoiceDef)
						{
							inputVoiceDefFound = true;
						}
						if(voiceDef is Trk && inputVoiceDefFound)
						{
							break;
						}
						foreach(IUniqueDef iud in voiceDef.UniqueDefs)
						{
							if(msPosition == (iud.MsPositionReFirstUD + iud.MsDuration))
							{
								found = true;
								break;
							}
						}
						if(found)
						{
							break;
						}
					}
					if(inputVoiceDefFound)
					{
						Debug.Assert(found, "Error: barline must be at the endMsPosition of at least one IUniqueDef in an InputVoiceDef.");
					}
					else
					{
						Debug.Assert(found, "Error: barline must be at the endMsPosition of at least one IUniqueDef in a Trk.");
					}
				}
			}

			public void AddInputVoice(InputVoiceDef ivd)
			{
				Debug.Assert((ivd.MsPositionReContainer + ivd.MsDuration) <= MsDuration);
				Debug.Assert(ivd.MidiChannel >= 0 && ivd.MidiChannel <= 3);
				#region check for an existing InputVoiceDef having the same MidiChannel
				foreach(VoiceDef voiceDef in _voiceDefs)
				{
					if(voiceDef is InputVoiceDef existingInputVoiceDef)
					{
						Debug.Assert(existingInputVoiceDef.MidiChannel != ivd.MidiChannel, "Error: An InputVoiceDef with the same MidiChannel already exists.");
					}
				}
				#endregion

				int startPos = ivd.MsPositionReContainer;
				ivd.MsPositionReContainer = 0;
				foreach(IUniqueDef iud in ivd.UniqueDefs)
				{
					iud.MsPositionReFirstUD += startPos;
				}

				_voiceDefs.Add(ivd);

				AssertConsistency();
			}
		}
	}
}
