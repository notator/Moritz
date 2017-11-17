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
		/// If inputVoiceDefs != null, then barlines will be fit to the inputVoices (compulsory), else
		/// the barlines will be fit to the
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
		/// 
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
    }
}
