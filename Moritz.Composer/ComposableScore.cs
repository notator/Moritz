using Krystals5ObjectLibrary;

using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Spec;
using Moritz.Symbols;
using Moritz.Xml;

using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Composer
{
    public partial class ComposableScore : SvgScore
    {
        public ComposableScore(string folder, string scoreFolderName, CompositionAlgorithm algorithm, string keywords, string comment, PageFormat pageFormat)
            : base(folder, scoreFolderName, keywords, comment, pageFormat)
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

            this.ScoreData = _algorithm.SetScoreRegionsData(bars);

            InsertInitialClefDefs(bars, _pageFormat.InitialClefPerMIDIChannel);

            CreateEmptySystems(bars); // one system per bar

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

        /// <summary>
        /// Inserts a ClefDef at the beginning of each Trk in each bar, taking any cautionaryChordDefs into account.
        /// </summary>
        /// <param name="bars"></param>
        /// <param name="initialClefPerMIDIChannel">The clefs at the beginning of the score.</param>
        private void InsertInitialClefDefs(List<Bar> bars, List<string> initialClefPerMIDIChannel)
        {
            // bars can currently contain cautionary clefs, but no initial clefs
            List<string> currentClefs = new List<string>(initialClefPerMIDIChannel);
            int nBars = bars.Count;
            int nVoiceDefs = bars[0].VoiceDefs.Count;
            Debug.Assert(nVoiceDefs == initialClefPerMIDIChannel.Count); // VoiceDefs are both Trks and InputVoiceDefs
            foreach(Bar bar in bars)
            {
                for(int i = 0; i < nVoiceDefs; ++i)
                {
                    ClefDef initialClefDef = new ClefDef(currentClefs[i], 0); // msPos is set later in Notator.ConvertVoiceDefsToNoteObjects()
                    bar.VoiceDefs[i].Insert(0, initialClefDef);
                    List<IUniqueDef> iuds = bar.VoiceDefs[i].UniqueDefs;
                    for(int j = 1; j < iuds.Count; ++j)
                    {
                        if(iuds[j] is ClefDef cautionaryClefDef)
                        {
                            currentClefs[i] = cautionaryClefDef.ClefType;
                        }
                    }
                }
            }
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
            //List<int> visibleLowerVoiceIndices = new List<int>();
            //Dictionary<int, string> upperVoiceClefDict = GetUpperVoiceClefDict(bars[0], _pageFormat, /*sets*/ visibleLowerVoiceIndices);

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
                }

                errorString = CheckThatLowerVoicesHaveNoSmallClefs(voiceDefs);

                if(!string.IsNullOrEmpty(errorString))
                    break;
            }
            return errorString;
        }

        private string CheckThatLowerVoicesHaveNoSmallClefs(IReadOnlyList<VoiceDef> voiceDefs)
        {
            string errorString = "";

            List<int> lowerVoiceIndices = GetOutputAndInputLowerVoiceIndices();

            foreach(int lowerVoiceIndex in lowerVoiceIndices)
            {
                var uniqueDefs = voiceDefs[lowerVoiceIndex].UniqueDefs;
                foreach(IUniqueDef iud in uniqueDefs)
                {
                    if(iud is ClefDef)
                    {
                        errorString = "Small Clefs may not be defined for lower voices on a staff.";
                        break;
                    }
                }
                if(!string.IsNullOrEmpty(errorString))
                    break;
            }

            return errorString;
        }

        private List<int> GetOutputAndInputLowerVoiceIndices()
        {
            List<int> lowerVoiceIndices = new List<int>();
            int voiceIndex = 0;

            List<List<byte>> outputChPerStaff = _pageFormat.OutputMIDIChannelsPerStaff;

            for(int staffIndex = 0; staffIndex < outputChPerStaff.Count; ++staffIndex)
            {
                if(outputChPerStaff[staffIndex].Count > 1)
                {
                    voiceIndex++;
                    lowerVoiceIndices.Add(voiceIndex);
                }
                voiceIndex++;
            }
            List<List<byte>> inputChPerStaff = _pageFormat.InputMIDIChannelsPerStaff;
            int nStaves = inputChPerStaff.Count + outputChPerStaff.Count;
            for(int staffIndex = 0; staffIndex < inputChPerStaff.Count; ++staffIndex)
            {
                if(inputChPerStaff[staffIndex].Count > 1)
                {
                    voiceIndex++;
                    lowerVoiceIndices.Add(voiceIndex);
                }
                voiceIndex++;
            }

            return lowerVoiceIndices;
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
        /// Check that each output track index (top to bottom) is the same as its MidiChannel (error is fatal)
        /// </summary>
        /// <param name="systems"></param>
        private void CheckSystems(List<SvgSystem> systems)
        {
            var outputTrackMidiChannels = new List<int>();
            for(int systemIndex = 0; systemIndex < systems.Count; systemIndex++)
            {
                var staves = systems[systemIndex].Staves;
                outputTrackMidiChannels.Clear();
                for(int staffIndex = 0; staffIndex < staves.Count; staffIndex++)
                {
                    var voices = staves[staffIndex].Voices;
                    foreach(var voice in voices)
                    {
                        if(voice is OutputVoice)
                        {
                            outputTrackMidiChannels.Add(voice.MidiChannel);
                        }
                        else break;
                    }
                }
                for(int trackIndex = 0; trackIndex < outputTrackMidiChannels.Count; trackIndex++)
                {
                    Debug.Assert(trackIndex == outputTrackMidiChannels[trackIndex], "Track index and MidiChannel must be identical.");
                }
            }
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
        public void CreateEmptySystems(List<Bar> bars)
        {
            foreach(Bar bar in bars)
            {
                SvgSystem system = new SvgSystem(this);
                this.Systems.Add(system);
            }

            CreateEmptyOutputStaves(bars);
            CreateEmptyInputStaves(bars);
        }

        private void CreateEmptyOutputStaves(List<Bar> bars)
        {
            int nStaves = _pageFormat.OutputMIDIChannelsPerStaff.Count;

            for(int systemIndex = 0; systemIndex < Systems.Count; systemIndex++)
            {
                SvgSystem system = Systems[systemIndex];
                IReadOnlyList<VoiceDef> voiceDefs = bars[systemIndex].VoiceDefs;

                #region create visible staves
                for(int staffIndex = 0; staffIndex < nStaves; staffIndex++)
                {
                    string staffname = StaffName(systemIndex, staffIndex);
                    OutputStaff outputStaff = new OutputStaff(system, staffname, _pageFormat.StafflinesPerStaff[staffIndex], _pageFormat.Gap, _pageFormat.StafflineStemStrokeWidth);

                    List<byte> outputVoiceIndices = _pageFormat.OutputMIDIChannelsPerStaff[staffIndex];
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
