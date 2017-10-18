using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

using Krystals4ObjectLibrary;
using Moritz.Spec;
using Moritz.Symbols;
using Moritz.Palettes;
using Moritz.Algorithm;
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
        /// Returns false if it fails for some reason. Otherwise true.
        /// </summary>
        protected bool CreateScore(List<Krystal> krystals, List<Palette> palettes)
        {
            bool success = true;

			CheckOutputVoiceChannelsAndMasterVolumes(_algorithm);

            List<List<VoiceDef>> bars = _algorithm.DoAlgorithm(krystals, palettes);

			CheckBars(bars);

			SetOutputVoiceChannels(bars[0]);

            CreateEmptySystems(bars, _pageFormat.VisibleInputVoiceIndicesPerStaff.Count); // one system per bar

            if(_pageFormat.ChordSymbolType != "none") // set by AudioButtonsControl
            {
                Notator.ConvertVoiceDefsToNoteObjects(this.Systems);

                FinalizeSystemStructure(); // adds barlines, joins bars to create systems, etc.

                /// The systems do not yet contain Metrics info.
                /// The systems are given Metrics inside the following function then justified internally,
                /// both horizontally and vertically.
                success = Notator.CreateMetricsAndJustifySystems(this.Systems);
                if(success)
                {
                    success = CreatePages();
                }
            }
            return success;
        }

        private Dictionary<int, string> GetUpperVoiceClefDict(List<List<VoiceDef>> bars, PageFormat _pageFormat, List<int> visibleLowerVoiceIndices)
        {
            int nVisibleOutputStaves = _pageFormat.VisibleOutputVoiceIndicesPerStaff.Count;
            int nVisibleInputStaves = _pageFormat.VisibleInputVoiceIndicesPerStaff.Count;
            Debug.Assert(_pageFormat.ClefsList.Count == nVisibleOutputStaves + nVisibleInputStaves);

            int nTrks = GetNumberOfTrks(bars[0]);

            Dictionary<int, string> upperVoiceClefDict = new Dictionary<int, string>();
            int clefIndex = 0;
            #region get upperVoiceClefs and visibleLowerVoiceIndices
            for(int i = 0; i < nVisibleOutputStaves; ++i)
            {
                List<byte> visibleOutputVoiceIndicesPerStaff = _pageFormat.VisibleOutputVoiceIndicesPerStaff[i];
                upperVoiceClefDict.Add(visibleOutputVoiceIndicesPerStaff[0], _pageFormat.ClefsList[clefIndex++]);
                if(visibleOutputVoiceIndicesPerStaff.Count > 1)
                {
                    visibleLowerVoiceIndices.Add(visibleOutputVoiceIndicesPerStaff[1]);
                }

            }
            for(int i = 0; i < nVisibleInputStaves; ++i)
            {
                List<byte> visibleInputVoiceIndicesPerStaff = _pageFormat.VisibleInputVoiceIndicesPerStaff[i];
                upperVoiceClefDict.Add(nTrks + visibleInputVoiceIndicesPerStaff[0], _pageFormat.ClefsList[clefIndex++]);
                if(visibleInputVoiceIndicesPerStaff.Count > 1)
                {
                    visibleLowerVoiceIndices.Add(nTrks + visibleInputVoiceIndicesPerStaff[1]);
                }
            }
            for(int i = 0; i < bars[0].Count; ++i)
            {
                if(!upperVoiceClefDict.ContainsKey(i))
                {
                    upperVoiceClefDict.Add(i, "noClef");
                }
            }
            #endregion

            return upperVoiceClefDict;
        }

        /// <summary>
        /// This function should be called before running the algorithm.
        /// </summary>
        private void CheckOutputVoiceChannelsAndMasterVolumes(CompositionAlgorithm algorithm)
		{
			string errorString = null;
			IReadOnlyList<int> midiChannelIndexPerOutputVoice = algorithm.MidiChannelIndexPerOutputVoice;
			if(string.IsNullOrEmpty(errorString))
			{
				for(int i = 0; i < midiChannelIndexPerOutputVoice.Count; ++i)
				{
					if(midiChannelIndexPerOutputVoice[i] < 0)
					{
						errorString = "All midi channel indices must be >= 0.";
						break;
					}
					if(midiChannelIndexPerOutputVoice[i] > 15)
					{
						errorString = "All midi channel indices must be < 16.";
						break;
					}
				}
			}
			Debug.Assert(string.IsNullOrEmpty(errorString), "Error in algorithm definition: \n\n" + errorString);
		}

		private void CheckBars(List<List<VoiceDef>> bars)
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
		private string BasicChecks(List<List<VoiceDef>> bars)
        {
			string errorString = null;
            List<int> visibleLowerVoiceIndices = new List<int>();
            Dictionary<int, string> upperVoiceClefDict = GetUpperVoiceClefDict(bars, _pageFormat, /*sets*/ visibleLowerVoiceIndices);

            for(int barIndex = 0; barIndex < bars.Count; ++barIndex)
			{
				List<VoiceDef> bar = bars[barIndex];
				string barNumber = (barIndex + 1).ToString();
				 
				if(bar.Count == 0)
				{
					errorString = "Bar " + barNumber + " contains no voices.";
					break;
				}
				if(!(bar[0] is Trk))
				{
					errorString = "The top (first) voice in every bar must be an output voice.";
					break;
				}
				for(int voiceIndex = 0; voiceIndex < bar.Count; ++voiceIndex)
				{
					VoiceDef voiceDef = bar[voiceIndex];
					string voiceNumber = (voiceIndex + 1).ToString();
					if(voiceDef.UniqueDefs.Count == 0)
					{
						errorString = "Voice number " + voiceNumber + " in Bar " + barNumber + " has an empty UniqueDefs list.";
						break;
					}
					foreach(IUniqueDef iud in voiceDef.UniqueDefs)
					{
						if(iud is ClefDef ccd)
						{
							if(visibleLowerVoiceIndices.Contains(voiceIndex))
							{
								errorString = "Voice number " + voiceNumber + " is a lower voice on a staff, and contains a clef change.\n" +
								"Clefs should only be changed in the staff's top voice.";
								break;
							}
							else if(!(barIndex == 0) && (upperVoiceClefDict.ContainsKey(voiceIndex) && upperVoiceClefDict[voiceIndex] == ccd.ClefType))
							{
								errorString = "Voice number " + voiceNumber + " has an unnecessary clef change in or after bar " + barNumber + ".";
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

        private int GetNumberOfTrks(List<VoiceDef> voiceDefs)
        {
            int nTrks = 0;
            foreach(VoiceDef voiceDef in voiceDefs)
            {
                if(voiceDef is Trk)
                {
                    nTrks++;
                }
            }
            return nTrks;
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
		private string CheckCCSettings(List<List<VoiceDef>> voiceDefsPerSystemPerBar)
		{
			string errorString = null;
			List<InputVoiceDef> ivds = new List<InputVoiceDef>();
			List<int> ccSettingsMsPositions = new List<int>();
			foreach(List<VoiceDef> bar in voiceDefsPerSystemPerBar)
			{
				ccSettingsMsPositions.Clear();

				foreach(VoiceDef voice in bar)
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
		/// This function should be called for all scores when the bars are complete.
		/// The plausibility checks have been made.
		/// </summary>
		private void SetOutputVoiceChannels(List<VoiceDef> firstBar)
		{
			IReadOnlyList<int> midiChannelIndexPerOutputVoice = _algorithm.MidiChannelIndexPerOutputVoice;
			for(int i = 0; i < midiChannelIndexPerOutputVoice.Count; ++i)
			{
				Trk oVoice = firstBar[i] as Trk;
				Debug.Assert(oVoice != null); // should be okay - the check has already been made
				oVoice.MidiChannel = (byte)midiChannelIndexPerOutputVoice[i];
			}
		}

        /// <summary>
        /// Creates one System per bar (=list of VoiceDefs) in the argument.
        /// The Systems are complete with staves and voices of the correct type:
        /// Each InputStaff is allocated parallel (empty) InputVoice fields.
        /// Each OutputStaff is allocated parallel (empty) OutputVoice fields.
        /// Each Voice has a VoiceDef field that is allocated to the corresponding
        /// VoiceDef from the argument.
        /// The OutputVoices are arranged according to _pageFormat.OutputVoiceIndicesPerStaff.
        /// The InputVoices are arranged according to _pageFormat.InputVoiceIndicesPerStaff.
        /// OutputVoices are given a midi channel allocated from top to bottom in the printed score.
        /// </summary>
        public void CreateEmptySystems(List<List<VoiceDef>> barDefsInOneSystem, int numberOfVisibleInputStaves)
        {
            foreach(List<VoiceDef> barVoiceDefs in barDefsInOneSystem)
            {
                SvgSystem system = new SvgSystem(this);
                this.Systems.Add(system);
            }

            CreateEmptyOutputStaves(barDefsInOneSystem, numberOfVisibleInputStaves);
            CreateEmptyInputStaves(barDefsInOneSystem);
        }

        private void CreateEmptyOutputStaves(List<List<VoiceDef>> barDefsInOneSystem, int numberOfVisibleInputStaves)
        {
            int nVisibleOutputStaves = _pageFormat.VisibleOutputVoiceIndicesPerStaff.Count;
            List<byte> invisibleOutputVoiceIndices = new List<byte>();
            if(numberOfVisibleInputStaves > 0 )
                invisibleOutputVoiceIndices = InvisibleOutputVoiceIndices(_pageFormat.VisibleOutputVoiceIndicesPerStaff, barDefsInOneSystem[0]);

            for(int i = 0; i < Systems.Count; i++)
            {
                SvgSystem system = Systems[i];
                List<VoiceDef> barDef = barDefsInOneSystem[i];

                #region create invisible staves
                if(invisibleOutputVoiceIndices.Count > 0)
                {
					foreach(byte invisibleOutputVoiceIndex in invisibleOutputVoiceIndices)
					{
						Trk invisibleTrkDef = barDef[invisibleOutputVoiceIndex] as Trk;
						HiddenOutputStaff hiddenOutputStaff = new HiddenOutputStaff(system);
						OutputVoice outputVoice = new OutputVoice(hiddenOutputStaff, invisibleTrkDef.MidiChannel)
						{
							VoiceDef = invisibleTrkDef
						};
                        hiddenOutputStaff.Voices.Add(outputVoice);
                        system.Staves.Add(hiddenOutputStaff);
                    }
                }
                #endregion create invisible staves

                for(int printedStaffIndex = 0; printedStaffIndex < nVisibleOutputStaves; printedStaffIndex++)
                {
                    string staffname = StaffName(i, printedStaffIndex);
                    OutputStaff outputStaff = new OutputStaff(system, staffname, _pageFormat.StafflinesPerStaff[printedStaffIndex], _pageFormat.Gap, _pageFormat.StafflineStemStrokeWidth);

                    List<byte> outputVoiceIndices = _pageFormat.VisibleOutputVoiceIndicesPerStaff[printedStaffIndex];
                    for(int ovIndex = 0; ovIndex < outputVoiceIndices.Count; ++ovIndex)
                    {
                        Trk trkDef = barDef[outputVoiceIndices[ovIndex]] as Trk;
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
            }
        }

        private List<byte> InvisibleOutputVoiceIndices(List<List<byte>> visibleOutputVoiceIndicesPerStaff, List<VoiceDef> voiceDefs)
        {
            List<byte> visibleOutputVoiceIndices = new List<byte>();
            foreach(List<byte> voiceIndices in visibleOutputVoiceIndicesPerStaff)
            {
                visibleOutputVoiceIndices.AddRange(voiceIndices);
            }
            List<byte> invisibleOutputVoiceIndices = new List<byte>();
            for(byte voiceIndex = 0; voiceIndex < voiceDefs.Count; ++voiceIndex)
            {
                if(voiceDefs[voiceIndex] is Trk)
                {
                    if(!visibleOutputVoiceIndices.Contains(voiceIndex))
                    {
                        invisibleOutputVoiceIndices.Add(voiceIndex);
                    }
                }
                else break;
            }
            return invisibleOutputVoiceIndices;
        }

        private void CreateEmptyInputStaves(List<List<VoiceDef>> barDefsInOneSystem)
        {
            int nPrintedOutputStaves = _pageFormat.VisibleOutputVoiceIndicesPerStaff.Count;
            int nPrintedInputStaves = _pageFormat.VisibleInputVoiceIndicesPerStaff.Count;
            int nStaffNames = _pageFormat.ShortStaffNames.Count;

            for(int i = 0; i < Systems.Count; i++)
            {
                SvgSystem system = Systems[i];
                List<VoiceDef> barDef = barDefsInOneSystem[i];

                for(int staffIndex = 0; staffIndex < nPrintedInputStaves; staffIndex++)
                {
                    int staffNameIndex = nPrintedOutputStaves + staffIndex;
                    string staffname = StaffName(i, staffNameIndex);

                    float gap = _pageFormat.Gap * _pageFormat.InputSizeFactor;
                    float stafflineStemStrokeWidth = _pageFormat.StafflineStemStrokeWidth * _pageFormat.InputSizeFactor;
                    InputStaff inputStaff = new InputStaff(system, staffname, _pageFormat.StafflinesPerStaff[staffIndex], gap, stafflineStemStrokeWidth);

                    List<byte> inputVoiceIndices = _pageFormat.VisibleInputVoiceIndicesPerStaff[staffIndex];
                    for(int ivIndex = 0; ivIndex < inputVoiceIndices.Count; ++ivIndex)
                    {
                        InputVoiceDef inputVoiceDef = barDef[inputVoiceIndices[ivIndex] + _algorithm.MidiChannelIndexPerOutputVoice.Count] as InputVoiceDef;
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
