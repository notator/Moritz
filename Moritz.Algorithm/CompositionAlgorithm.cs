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
    /// (containing real NoteObjects) later, using the options set an .mkss file. 
    /// </summary>
    public abstract class CompositionAlgorithm
    {
        protected CompositionAlgorithm()
        {
        }

        protected void CheckParameters()
        {
            int channelCount = MidiChannelIndexPerOutputVoice.Count;
            if(channelCount < 1)
                throw new ApplicationException("CompositionAlgorithm: There must be at least one output voice!");
            if(channelCount > 16)
                throw new ApplicationException("CompositionAlgorithm: There can not be more than 16 output voices.");

            int previousChannelIndex = -1;
            for(int i = 0; i < channelCount; ++i)
            {
                int channelIndex = MidiChannelIndexPerOutputVoice[i];
                if(channelIndex <= previousChannelIndex)
                    throw new ApplicationException("CompositionAlgorithm: midi channels must be unique and in ascending order (but need not be contiguous)!");
                previousChannelIndex = channelIndex;

                if(channelIndex < 0 || channelIndex > 15)
                    throw new ApplicationException("CompositionAlgorithm: midi channel out of range!");
            }

            // Midi input devices are identified by their midi channel, so there may not be more than 16 of them.
            // InputVoices can share the same midi input channel (a device can play more than one voice), so there
            // is no upper limit to the number of InputVoices.
            // Input Voices having the same channel are agglomerated at load time by the Assistant Performer.
            if(NumberOfInputVoices < 0)
                throw new ApplicationException("CompositionAlgorithm: There can not be a negative number of input voices!");

            if(NumberOfBars == 0)
                throw new ApplicationException("CompositionAlgorithm: There must be at least one bar!");

        }

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
        /// Returns a midi channel for each output voice.
        /// These midi channels must always be in ascending order, starting at 0.
        /// Not every channel has to exist, so that the standard midi percussion channel (channelIndex 9) can be used or omitted.
        /// These values must be in range [ 0..15 ] are written once to each voice in the score file (in the first system).
        /// A midi channel's voiceID (written into in the score, if there are input voices) is its position in this list.
        /// The top to bottom printed order of the voices in the score (and whether the voices are printed at all) is determined by
        /// a parameter in the .mkss file. 
        /// </summary>
        public abstract IReadOnlyList<int> MidiChannelIndexPerOutputVoice { get; }

        /// <summary>
        /// Returns the number of inputVoices created by the algorithm.
        /// </summary>
        public abstract int NumberOfInputVoices { get; }

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
        /// MidiChannelIndexPerOutputVoice and NumberOfInputVoices properties (see above).
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
        /// MidiChannelIndexPerOutputVoice property in the top to bottom order of the voices in the bars being
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
		/// The dictionary contains the index at which the clef will be inserted in the VoiceDef's IUniquedefs,
		/// and the clef ID string ("t", "t1", "b3" etc.).
		/// Clefs will be inserted in reverse order of the Sorted dictionary, so that the indices are those of
		/// the existing IUniqueDefs before which the clef will be inserted.
		/// The SortedDictionaries should not contain tne initial clefs per voicedef - those will be included
		/// automatically.
		/// Note that a CautionaryChordDef counts as an IUniqueDef at the beginning of a bar, and that clefs
		/// cannot be inserted in front of them.
		/// </summary>
		protected abstract List<List<SortedDictionary<int, string>>> GetClefChangesPerBar(int nBars);

		/// <summary>
		/// This function returns null or a SortedDictionary per VoiceDef in each bar.
		/// The dictionary contains the index of the IUniqueDef in the barat which the clef will be inserted in the VoiceDef's IUniquedefs,
		/// and the clef ID string ("t", "t1", "b3" etc.).
		/// Clefs will be inserted in reverse order of the Sorted dictionary, so that the indices are those of
		/// the existing IUniqueDefs before which the clef will be inserted.
		/// The SortedDictionaries should not contain tne initial clefs per voicedef - those will be included
		/// automatically.
		/// Note that both Clefs and a CautionaryChordDef at the beginning of a bar count as IUniqueDefs for
		/// indexing purposes, and that lyrics cannot be attached to them.
		/// </summary>
		protected abstract List<List<SortedDictionary<int, string>>> GetLyricsPerBar(int nBars);

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
			MainBar mainBar = new MainBar(mainSeq, inputVoiceDefs, InitialClefPerVoiceDef);

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
					VoiceDef voiceDef = barVoiceDefs[voiceDefIndex];
					SortedDictionary<int, string> clefChanges = clefChangesPerVoiceDef[voiceDefIndex];
					InsertClefChangesInVoiceDef(voiceDef, clefChanges);
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
			foreach(int key in lyrics.Keys)
			{
				IUniqueDef iud = voiceDef[key];
				string lyric = lyrics[key];

				if(iud is MidiChordDef mcd)
				{
					mcd.Lyric = lyric;
				}
				else if(iud is InputChordDef icd)
				{
					icd.Lyric = lyric;
				}
				else
				{
					Debug.Assert(false);
				}
			}
		}

		/// <summary>
		/// Returns a clef for every VoiceDef (top to bottom in score).
		/// Channels that will end up on a HiddenOutputStaff are also given a clef - even though it isn't going to be displayed.
		/// </summary>
		public void GetInitialClefPerVoiceDef(PageFormat pageFormat)
        {
            List<string> pageFormatClefsList = pageFormat.ClefsList;
            List<List<byte>> visibleOutputVoiceIndicesPerStaff = pageFormat.VisibleOutputVoiceIndicesPerStaff;
            List<List<byte>> visibleInputVoiceIndicesPerStaff = pageFormat.VisibleInputVoiceIndicesPerStaff;

            List<string> initialClefs = new List<string>();
            #region fill initialClefs to the right length, just so that it can be indexed.
            for(int i = 0; i < this.MidiChannelIndexPerOutputVoice.Count; ++i)
            {
                initialClefs.Add("t");
            }
            for(int i = 0; i < this.NumberOfInputVoices; ++i)
            {
                initialClefs.Add("t");
            }
            #endregion

            int pageFormatClefsListIndex = 0;
            for(int i = 0; i < visibleOutputVoiceIndicesPerStaff.Count; ++i)
            {
                List<byte> voiceIndices = visibleOutputVoiceIndicesPerStaff[i];
                foreach(byte index in voiceIndices)
                {
                    initialClefs[index] = pageFormatClefsList[pageFormatClefsListIndex++];
                }
            }

            int firstInputClefIndex = this.MidiChannelIndexPerOutputVoice.Count;
            for(int i = 0; i < visibleInputVoiceIndicesPerStaff.Count; ++i)
            {
                List<byte> inputVoiceIndices = visibleInputVoiceIndicesPerStaff[i];
                foreach(byte index in inputVoiceIndices)
                {
                    initialClefs[firstInputClefIndex + index] = pageFormatClefsList[pageFormatClefsListIndex++];
                }
            }

			InitialClefPerVoiceDef = initialClefs;
		}

		public List<string> InitialClefPerVoiceDef = null;
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
			public MainBar(Seq seq, IReadOnlyList<InputVoiceDef> inputVoiceDefs, List<string> initialClefPerChannel)
				: base(seq, inputVoiceDefs, initialClefPerChannel)
			{
				AssertConsistency();
			}

			/// <summary>
			/// 1. base.AssertConsistency() is called. (The base Bar must be consistent.)
			/// 2. AbsMsPosition must be 0.
			/// 3. InitialClefPerChannel != null && InitialClefPerChannel.Count == VoiceDefs.Count.
			/// 4. At least one Trk must end with a MidiChordDef.
			/// </summary> 
			public override void AssertConsistency()
			{
				base.AssertConsistency();
				Debug.Assert(AbsMsPosition == 0);
				Debug.Assert(InitialClefPerChannel != null && InitialClefPerChannel.Count == VoiceDefs.Count);

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
					Trk outputVoice = voiceDef as Trk;
					InputVoiceDef inputVoice = voiceDef as InputVoiceDef;
					if(outputVoice != null)
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
