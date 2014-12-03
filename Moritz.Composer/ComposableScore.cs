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
    public partial class ComposableSvgScore : SvgScore
    {
        public ComposableSvgScore(string folder, string scoreTitleName, CompositionAlgorithm algorithm, string keywords, string comment, PageFormat pageFormat)
            : base(folder, scoreTitleName, keywords, comment, pageFormat)
        {
            _algorithm = algorithm;
        }

        private void CheckBars(List<List<VoiceDef>> voiceDefsPerSystemPerBar)
        {
            string errorString = null;
            if(voiceDefsPerSystemPerBar.Count == 0)
                errorString = "The algorithm has not created any bars!";
            else
            {
                errorString = BasicChecks(voiceDefsPerSystemPerBar);
            }
            if(string.IsNullOrEmpty(errorString))
            {
                errorString = CheckInputControlsDef(voiceDefsPerSystemPerBar);
            }
            if(!string.IsNullOrEmpty(errorString))
            {
                throw new ApplicationException("\nComposableScore.CheckBars(): Algorithm error:\n" + errorString);
            }
        }
        #region private to CheckBars(...)
        private string BasicChecks(List<List<VoiceDef>> voiceDefsPerSystemPerBar)
        {
            string errorString = null;
            List<VoiceDef> bar1 = voiceDefsPerSystemPerBar[0];
            if(NOutputVoices(bar1) != _algorithm.MidiChannelIndexPerOutputVoice.Count)
            {
                return "The algorithm does not declare the correct number of output voices.";
            }
            if(NInputVoices(bar1) != _algorithm.NumberOfInputVoices)
            {
                return "The algorithm does not declare the correct number of input voices.";
            }
            foreach(List<VoiceDef> bar in voiceDefsPerSystemPerBar)
            {
                if(bar.Count == 0)
                {
                    errorString = "One bar (at least) contains no voices.";
                    break;
                }
                if(!(bar[0] is OutputVoiceDef))
                {
                    errorString = "The top (first) voice in every bar must be an output voice.";
                    break;
                }
                for(int voiceIndex = 0; voiceIndex < bar.Count; ++voiceIndex)
                {
                    VoiceDef voiceDef = bar[voiceIndex];
                    if(voiceDef.UniqueDefs.Count == 0)
                    {
                        errorString = "A voiceDef (voiceIndex=" + voiceIndex.ToString() + ") has an empty UniqueDefs list.";
                        break;
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
                if(voiceDef is OutputVoiceDef)
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

        private string CheckInputControlsDef(List<List<VoiceDef>> voiceDefsPerSystemPerBar)
        {
            string errorString = null;
            List<OutputVoiceDef> oVoices = new List<OutputVoiceDef>();
            foreach(VoiceDef voice in voiceDefsPerSystemPerBar[0])
            {
                OutputVoiceDef ov = voice as OutputVoiceDef;
                if(ov != null)
                {
                    if(ov.MasterVolume == null)
                    {
                        errorString = "\nEvery OutputVoice in the first bar of a score\n" +
                                      "must have a MasterVolume value.";
                    }
                    oVoices.Add(ov);
                }
            }
            if(string.IsNullOrEmpty(errorString) && voiceDefsPerSystemPerBar.Count > 1)
            {
                for(int bar = 1; bar < voiceDefsPerSystemPerBar.Count; ++bar)
                {
                    foreach(VoiceDef voice in voiceDefsPerSystemPerBar[bar])
                    {
                        OutputVoiceDef ov = voice as OutputVoiceDef;
                        if(ov != null)
                        {
                            if(ov.MasterVolume != null)
                            {
                                errorString = "\nNo OutputVoice except the first may have a MasterVolume.";
                            }
                        }
                    }
                }
            }
            return errorString;
        }
        #endregion

        /// <summary>
        /// Called by the derived class after setting _midiAlgorithm and Notator.
        /// Returns false if it fails for some reason. Otherwise true.
        /// </summary>
        protected bool CreateScore(List<Krystal> krystals, List<Palette> palettes)
        {
            bool success = true;
            List<List<VoiceDef>> barDefsInOneSystem = _algorithm.DoAlgorithm(krystals, palettes);
            CheckBars(barDefsInOneSystem);
            CreateEmptySystems(barDefsInOneSystem, _pageFormat.VisibleInputVoiceIndicesPerStaff.Count); // one system per bar
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
                        byte? voiceID = null;
                        if(_pageFormat.VisibleInputVoiceIndicesPerStaff.Count > 0)
                        {
                            voiceID = (byte?)invisibleOutputVoiceIndex;
                        }
                        OutputVoiceDef invisibleOutputVoiceDef = barDef[invisibleOutputVoiceIndex] as OutputVoiceDef;
                        InvisibleOutputStaff invisibleOutputStaff = new InvisibleOutputStaff(system);
                        OutputVoice outputVoice = new OutputVoice(invisibleOutputStaff, invisibleOutputVoiceDef.MidiChannel, voiceID, invisibleOutputVoiceDef.MasterVolume);
                        outputVoice.VoiceDef = invisibleOutputVoiceDef;
                        invisibleOutputStaff.Voices.Add(outputVoice);
                        system.Staves.Add(invisibleOutputStaff);
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
                        byte? voiceID = null;
                        if(_pageFormat.VisibleInputVoiceIndicesPerStaff.Count > 0)
                        {
                            voiceID = (byte?)outputVoiceIndices[ovIndex];
                        }

                        OutputVoiceDef outputVoiceDef = barDef[outputVoiceIndices[ovIndex]] as OutputVoiceDef;
                        Debug.Assert(outputVoiceDef != null);
                        OutputVoice outputVoice = new OutputVoice(outputStaff, outputVoiceDef.MidiChannel, voiceID, outputVoiceDef.MasterVolume);
                        outputVoice.VoiceDef = outputVoiceDef;
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
                if(voiceDefs[voiceIndex] is OutputVoiceDef)
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

                    float gap = _pageFormat.Gap * _pageFormat.InputStavesSizeFactor;
                    float stafflineStemStrokeWidth = _pageFormat.StafflineStemStrokeWidth * _pageFormat.InputStavesSizeFactor;
                    InputStaff inputStaff = new InputStaff(system, staffname, _pageFormat.StafflinesPerStaff[staffIndex], gap, stafflineStemStrokeWidth);

                    List<byte> inputVoiceIndices = _pageFormat.VisibleInputVoiceIndicesPerStaff[staffIndex];
                    for(int ivIndex = 0; ivIndex < inputVoiceIndices.Count; ++ivIndex)
                    {
                        InputVoiceDef inputVoiceDef = barDef[inputVoiceIndices[ivIndex] + _algorithm.MidiChannelIndexPerOutputVoice.Count] as InputVoiceDef;
                        Debug.Assert(inputVoiceDef != null);
                        InputVoice inputVoice = new InputVoice(inputStaff);
                        inputVoice.VoiceDef = inputVoiceDef;
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
