using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;
using Moritz.Symbols;
using Moritz.Xml;

namespace Moritz.Composer
{
	public partial class ComposableScore : SvgScore
    {
        public ComposableScore(string folder, string scoreTitleName, CompositionAlgorithm algorithm, string keywords, string comment, PageFormat pageFormat)
            : base(folder, scoreTitleName, keywords, comment, pageFormat)
        {
            _algorithm = algorithm;
        }

        /// <summary>
        /// Called by the derived class after setting _algorithm and Notator.
        /// Returns false if systems cannot be fit vertically on the page. Otherwise true.
        /// </summary>
        protected bool CreateScore(List<Krystal> krystals, List<Palette> palettes)
        { 
            List<Bar> bars = _algorithm.DoAlgorithm(krystals, palettes);

			CheckBars(bars);

            CreateEmptySystems(bars, _pageFormat.InputMIDIChannelsPerStaff.Count); // one system per bar

			bool success = true;
			if(_pageFormat.ChordSymbolType != "none") // set by AudioButtonsControl
            {
                Notator.ConvertVoiceDefsToNoteObjects(this.Systems);

				FinalizeSystemStructure(); // adds barlines, joins bars to create systems, etc.

                /// The systems do not yet contain Metrics info.
                /// The systems are given Metrics inside the following function then justified internally,
                /// both horizontally and vertically.
                Notator.CreateMetricsAndJustifySystems(this.Systems);
                success = CreatePages();
            }

			CheckSystems(this.Systems);

            return success;
        }

		private Dictionary<int, string> GetUpperVoiceClefDict(Bar bar1, PageFormat _pageFormat, List<int> visibleLowerVoiceIndices)
        {
            int nOutputStaves = _pageFormat.OutputMIDIChannelsPerStaff.Count;
            int nInputStaves = _pageFormat.InputMIDIChannelsPerStaff.Count;
            Debug.Assert(_pageFormat.ClefsList.Count == nOutputStaves + nInputStaves);

			int nTrks = bar1.Trks.Count;

            Dictionary<int, string> upperVoiceClefDict = new Dictionary<int, string>();
            int clefIndex = 0;
            #region get upperVoiceClefs and visibleLowerVoiceIndices
            for(int i = 0; i < nOutputStaves; ++i)
            {
                List<byte> outputMIDIChannelsPerStaff = _pageFormat.OutputMIDIChannelsPerStaff[i];
                upperVoiceClefDict.Add(outputMIDIChannelsPerStaff[0], _pageFormat.ClefsList[clefIndex++]);
                if(outputMIDIChannelsPerStaff.Count > 1)
                {
                    visibleLowerVoiceIndices.Add(outputMIDIChannelsPerStaff[1]);
                }

            }
            for(int i = 0; i < nInputStaves; ++i)
            {
                List<byte> visibleInputVoiceIndicesPerStaff = _pageFormat.InputMIDIChannelsPerStaff[i];
                upperVoiceClefDict.Add(nTrks + visibleInputVoiceIndicesPerStaff[0], _pageFormat.ClefsList[clefIndex++]);
                if(visibleInputVoiceIndicesPerStaff.Count > 1)
                {
                    visibleLowerVoiceIndices.Add(nTrks + visibleInputVoiceIndicesPerStaff[1]);
                }
            }
            for(int i = 0; i < bar1.VoiceDefs.Count; ++i)
            {
                if(!upperVoiceClefDict.ContainsKey(i))
                {
                    upperVoiceClefDict.Add(i, "noClef");
                }
            }
            #endregion

            return upperVoiceClefDict;
        }

		private void CheckBars(List<Bar> bars)
		{
            string errorString = null;
			if(bars.Count == 0)
				errorString = "The algorithm has not created any bars!";
			else
			{
				errorString = BasicChecks(bars);
			}
			if(string.IsNullOrEmpty(errorString))
			{
				errorString = CheckCCSettings(bars);
			}
			Debug.Assert(string.IsNullOrEmpty(errorString), errorString);
		}
		#region private to CheckBars(...)
		private string BasicChecks(List<Bar> bars)
        {
			string errorString = null;
            List<int> visibleLowerVoiceIndices = new List<int>();
            Dictionary<int, string> upperVoiceClefDict = GetUpperVoiceClefDict(bars[0], _pageFormat, /*sets*/ visibleLowerVoiceIndices);

            for(int barIndex = 0; barIndex < bars.Count; ++barIndex)
			{
				Bar bar = bars[barIndex];
				IReadOnlyList<VoiceDef> voiceDefs = bar.VoiceDefs;
				string barNumber = (barIndex + 1).ToString();

				if(voiceDefs.Count == 0)
				{
					errorString = $"Bar {barNumber} contains no voices.";
					break;
				}
				if(!(voiceDefs[0] is Trk))
				{
					errorString = "The top (first) voice in every bar must be an output voice.";
					break;
				}
				for(int voiceIndex = 0; voiceIndex < voiceDefs.Count; ++voiceIndex)
				{
					VoiceDef voiceDef = voiceDefs[voiceIndex];
					string voiceNumber = (voiceIndex + 1).ToString();
					if(voiceDef.UniqueDefs.Count == 0)
					{
						errorString = $"Voice number {voiceNumber} in Bar {barNumber} has an empty UniqueDefs list.";
						break;
					}
					foreach(IUniqueDef iud in voiceDef.UniqueDefs)
					{
						if(iud is ClefDef ccd)
						{
							if(visibleLowerVoiceIndices.Contains(voiceIndex))
							{
								errorString = $"Voice number {voiceNumber} is a lower voice on a staff, and contains a clef change.\n" + 
								"Clefs should only be changed in the staff's top voice.";
								break;
							}
							else if(!(barIndex == 0) && (upperVoiceClefDict.ContainsKey(voiceIndex) && upperVoiceClefDict[voiceIndex] == ccd.ClefType))
							{
								errorString = $"Voice number {voiceNumber} has an unnecessary clef change in or after bar {barNumber}.";
								break;
							}
                            upperVoiceClefDict[voiceIndex] = ccd.ClefType; // a clef at the start of the first bar overrides the pageFormat.
						}
					}
				}
				if(!string.IsNullOrEmpty(errorString))
					break;
			}
			return errorString;
		}

        private int NOutputVoices(List<VoiceDef> bar1)
		{
			int nOutputVoices = 0;
			foreach(VoiceDef voiceDef in bar1)
			{
				if(voiceDef is Trk)
				{
					nOutputVoices++;
				}
			}
			return nOutputVoices;
		}

		private int NInputVoices(List<VoiceDef> bar1)
		{
			int nInputVoices = 0;
			foreach(VoiceDef voiceDef in bar1)
			{
				if(voiceDef is InputVoiceDef)
				{
					nInputVoices++;
				}
			}
			return nInputVoices;
		}

		/// <summary>
		/// Synchronous continuous controller settings (ccSettings) are not allowed.
		/// </summary>
		private string CheckCCSettings(List<Bar> bars)
		{
			string errorString = null;
			List<InputVoiceDef> ivds = new List<InputVoiceDef>();
			List<int> ccSettingsMsPositions = new List<int>();
			foreach(Bar bar in bars)
			{
				ccSettingsMsPositions.Clear();

				foreach(VoiceDef voice in bar.VoiceDefs)
				{
					if(voice is InputVoiceDef ivd)
					{
						foreach(IUniqueDef iud in ivd.UniqueDefs)
						{
							if(iud is InputChordDef icd && icd.CCSettings != null)
							{
								int msPos = icd.MsPositionReFirstUD;
								if(ccSettingsMsPositions.Contains(msPos))
								{
									errorString = "\nSynchronous continuous controller settings (ccSettings) are not allowed.";
									break;
								}
								else
								{
									ccSettingsMsPositions.Add(msPos);
								}

							}
						}
						if(!string.IsNullOrEmpty(errorString))
						{
							break;
						}
					}
				}
				if(!string.IsNullOrEmpty(errorString))
				{
					break;
				}
			}

			return errorString;
		}

		#endregion

		/// <summary>
		/// Check that
		/// 1. Every system has at least one visible (output or input) staff (error is fatal)
		/// 2. Every output track is visible in at least one system (error is  warning)
		/// 3. Each output track index (top to bottom) is the same as its MidiChannel (error is fatal)
		/// </summary>
		/// <param name="systems"></param>
		private void CheckSystems(List<SvgSystem> systems)
		{
			#region check that all systems have at least one visible (output or input) staff
			for(int systemIndex = 0; systemIndex < systems.Count; systemIndex++)
			{
				var staves = systems[systemIndex].Staves;
				bool foundVisibleOutputStaff = false;
				for(int staffIndex = 0; staffIndex < staves.Count; staffIndex++)
				{
					var staff = staves[staffIndex];
					if(staff.ContainsAChordSymbol)
					{
						foundVisibleOutputStaff = true;
						break;
					}

				}
				Debug.Assert(foundVisibleOutputStaff, $"System {systemIndex + 1} has no visible (output or input) staff.");
			}
			#endregion
			#region check that all output tracks are visible in at least one system, and that the track's index (top to bottom) equals its MidiChannel. 
			var outoutTrackVisibilities = new List<bool>();
			foreach(var staff in systems[0].Staves)
			{
				foreach(var voice in staff.Voices)
				{
					if(voice is OutputVoice)
					{
						outoutTrackVisibilities.Add(false);
					}
				}
			}

			var outputTrackMidiChannels = new List<int>();
			for(int systemIndex = 0; systemIndex < systems.Count; systemIndex++)
			{
				var trackIndex = 0;
				var staves = systems[systemIndex].Staves;
				for(int staffIndex = 0; staffIndex < staves.Count; staffIndex++)
				{
					var voices = staves[staffIndex].Voices;
					foreach(var voice in voices)
					{
						if(voice is OutputVoice)
						{
							if(voice.ContainsAChordSymbol)
							{
								outoutTrackVisibilities[trackIndex] = true;
							}
							trackIndex++;

							outputTrackMidiChannels.Add(voice.MidiChannel);
						}
						else break;
					} 
				}
			}
			var invisibleOutputTracks = new List<int>();

			for(int trackIndex = 0; trackIndex < outoutTrackVisibilities.Count; trackIndex++)
			{
				if(outoutTrackVisibilities[trackIndex] == false)
				{
					invisibleOutputTracks.Add(trackIndex);
				}
				Debug.Assert(trackIndex == outputTrackMidiChannels[trackIndex], "Track index and MidiChannel must be identical.");
			}
			
			if(invisibleOutputTracks.Count > 0)
			{
				string iTracksStr = M.IntListToString(invisibleOutputTracks, ", ");
				string msg;
				string title;
				if(invisibleOutputTracks.Count == 1)
				{
					title = "Invisible Track Warning";
					msg = $"Track (= MidiChannel) {iTracksStr} contains no chord symbols,\nand is therefore never visible."; 
				}
				else
				{
					title = "Invisible Track(s) Warning";
					msg = $"The following Tracks (i.e. MidiChannels) contain no chord symbols,\nand are therefore never visible:\n\n\t{iTracksStr}";
				}
				MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			#endregion
		}

		/// <summary>
		/// Creates one System per bar (=list of VoiceDefs) in the argument.
		/// The Systems are complete with staves and voices of the correct type:
		/// Each InputStaff is allocated parallel (empty) InputVoice fields.
		/// Each OutputStaff is allocated parallel (empty) OutputVoice fields.
		/// Each Voice has a VoiceDef field that is allocated to the corresponding
		/// VoiceDef from the argument.
		/// The OutputVoices have MIDIChannels arranged according to _pageFormat.OutputMIDIChannelsPerStaff.
		/// The InputVoices have MIDIChannels arranged according to _pageFormat.InputMIDIChannelsPerStaff.
		/// OutputVoices are given a midi channel allocated from top to bottom in the printed score.
		/// </summary>
		public void CreateEmptySystems(List<Bar> bars, int numberOfInputStaves)
        {
            foreach(Bar bar in bars)
            {
                SvgSystem system = new SvgSystem(this);
                this.Systems.Add(system);
            }

            CreateEmptyOutputStaves(bars, numberOfInputStaves);
            CreateEmptyInputStaves(bars);
		}

        private void CreateEmptyOutputStaves(List<Bar> bars, int numberOfInputStaves)
        {
            int nVisibleOutputStaves = _pageFormat.OutputMIDIChannelsPerStaff.Count;

            List<byte> invisibleOutputVoiceIndices = new List<byte>();
            if(numberOfInputStaves > 0 )
                invisibleOutputVoiceIndices = InvisibleOutputVoiceIndices(_pageFormat.OutputMIDIChannelsPerStaff, bars[0].Trks);

			for(int systemIndex = 0; systemIndex < Systems.Count; systemIndex++)
            {
                SvgSystem system = Systems[systemIndex];
                IReadOnlyList<VoiceDef> voiceDefs = bars[systemIndex].VoiceDefs;

				#region create hidden staves
				if(invisibleOutputVoiceIndices.Count > 0)
				{
					foreach(byte invisibleOutputVoiceIndex in invisibleOutputVoiceIndices)
					{
						Trk invisibleTrkDef = voiceDefs[invisibleOutputVoiceIndex] as Trk;
						HiddenOutputStaff hiddenOutputStaff = new HiddenOutputStaff(system);
						OutputVoice outputVoice = new OutputVoice(hiddenOutputStaff, invisibleTrkDef.MidiChannel)
						{
							VoiceDef = invisibleTrkDef
						};
						hiddenOutputStaff.Voices.Add(outputVoice);
						system.Staves.Add(hiddenOutputStaff);
					}
				}
				#endregion create hidden staves

				#region create visible staves
				for(int visibleStaffIndex = 0; visibleStaffIndex < nVisibleOutputStaves; visibleStaffIndex++)
                {
                    string staffname = StaffName(systemIndex, visibleStaffIndex);
                    OutputStaff outputStaff = new OutputStaff(system, staffname, _pageFormat.StafflinesPerStaff[visibleStaffIndex], _pageFormat.Gap, _pageFormat.StafflineStemStrokeWidth);

                    List<byte> outputVoiceIndices = _pageFormat.OutputMIDIChannelsPerStaff[visibleStaffIndex];
                    for(int ovIndex = 0; ovIndex < outputVoiceIndices.Count; ++ovIndex)
                    {
                        Trk trkDef = voiceDefs[outputVoiceIndices[ovIndex]] as Trk;
                        Debug.Assert(trkDef != null);
                        OutputVoice outputVoice = new OutputVoice(outputStaff, trkDef.MidiChannel)
						{
							VoiceDef = trkDef
						};
                        outputStaff.Voices.Add(outputVoice);
                    }
                    SetStemDirections(outputStaff);
                    system.Staves.Add(outputStaff);
                }
				#endregion
			}
		}

		private List<byte> InvisibleOutputVoiceIndices(List<List<byte>> visibleOutputVoiceIndicesPerStaff, IReadOnlyList<Trk> trks)
        {
            List<byte> visibleOutputVoiceIndices = new List<byte>();
            foreach(List<byte> voiceIndices in visibleOutputVoiceIndicesPerStaff)
            {
                visibleOutputVoiceIndices.AddRange(voiceIndices);
            }
            List<byte> invisibleOutputVoiceIndices = new List<byte>();
            for(byte trkIndex = 0; trkIndex < trks.Count; ++trkIndex)
            {
                if(!visibleOutputVoiceIndices.Contains(trkIndex))
                {
                    invisibleOutputVoiceIndices.Add(trkIndex);
                }
            }
            return invisibleOutputVoiceIndices;
        }

        private void CreateEmptyInputStaves(List<Bar> bars)
        {
            int nPrintedOutputStaves = _pageFormat.OutputMIDIChannelsPerStaff.Count;
            int nPrintedInputStaves = _pageFormat.InputMIDIChannelsPerStaff.Count;
            int nStaffNames = _pageFormat.ShortStaffNames.Count;

            for(int i = 0; i < Systems.Count; i++)
            {
                SvgSystem system = Systems[i];
				IReadOnlyList<VoiceDef> voiceDefs = bars[i].VoiceDefs;

                for(int staffIndex = 0; staffIndex < nPrintedInputStaves; staffIndex++)
                {
                    int staffNameIndex = nPrintedOutputStaves + staffIndex;
                    string staffname = StaffName(i, staffNameIndex);

                    float gap = _pageFormat.Gap * _pageFormat.InputSizeFactor;
                    float stafflineStemStrokeWidth = _pageFormat.StafflineStemStrokeWidth * _pageFormat.InputSizeFactor;
                    InputStaff inputStaff = new InputStaff(system, staffname, _pageFormat.StafflinesPerStaff[staffIndex], gap, stafflineStemStrokeWidth);

                    List<byte> inputVoiceIndices = _pageFormat.InputMIDIChannelsPerStaff[staffIndex];
                    for(int ivIndex = 0; ivIndex < inputVoiceIndices.Count; ++ivIndex)
                    {
                        InputVoiceDef inputVoiceDef = voiceDefs[inputVoiceIndices[ivIndex] + _algorithm.MidiChannelPerOutputVoice.Count] as InputVoiceDef;
                        Debug.Assert(inputVoiceDef != null);
						InputVoice inputVoice = new InputVoice(inputStaff)
						{
							VoiceDef = inputVoiceDef
						};
						inputStaff.Voices.Add(inputVoice);
                    }
                    SetStemDirections(inputStaff);
                    system.Staves.Add(inputStaff);
                }
            }
        }

		private void AdjustOutputVoiceRefs(List<SvgSystem> systems, List<int> outputMidiChannelSubstitutions)
		{
			Debug.Assert(_algorithm.MidiChannelPerInputVoice != null);

			foreach(var system in systems)
			{
				foreach(var staff in system.Staves)
				{
					if(staff is InputStaff inputStaff)
					{
						foreach(var voice in inputStaff.Voices)
						{
							if(voice is InputVoice inputVoice)
							{
								DoMidiChannelSubstitution(inputVoice, outputMidiChannelSubstitutions);
							}
						}
					}
				}
			}
		}

		private void DoMidiChannelSubstitution(InputVoice inputVoice, List<int> outputMidiChannelSubstitutions)
		{
			foreach(var noteObject in inputVoice.NoteObjects)
			{
				if(noteObject is InputChordSymbol ics)
				{
					var inputNoteDefs = ics.InputChordDef.InputNoteDefs;
					foreach(var inputNoteDef in inputNoteDefs)
					{
						var noteOnTrkRefs = inputNoteDef.NoteOn.SeqRef.TrkRefs; // each TrkRef has a midiChannel
						foreach(var trkRef in noteOnTrkRefs)
						{
							trkRef.TrkIndex = outputMidiChannelSubstitutions[trkRef.TrkIndex];
						}

						var noteOffTrkOffs = inputNoteDef.NoteOff.TrkOffs; // trkOffs is a list of trk indices
						for(int index = 0; index < noteOffTrkOffs.Count; index++)
						{
							noteOffTrkOffs[index] = outputMidiChannelSubstitutions[noteOffTrkOffs[index]];
						}
					}
				}
			}
		}

		private string StaffName(int systemIndex, int staffIndex)
        {
            if(systemIndex == 0)
            {
                return _pageFormat.LongStaffNames[staffIndex];
            }
            else
            {
                return _pageFormat.ShortStaffNames[staffIndex];
            }
        }

        private void SetStemDirections(Staff staff)
        {
            if(staff.Voices.Count == 1)
            {
                staff.Voices[0].StemDirection = VerticalDir.none;
            }
            else
            {
                Debug.Assert(staff.Voices.Count == 2);
                staff.Voices[0].StemDirection = VerticalDir.up;
                staff.Voices[1].StemDirection = VerticalDir.down;
            }
        }
        protected CompositionAlgorithm _algorithm = null;
    }
}
