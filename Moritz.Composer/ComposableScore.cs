using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

using Krystals4ObjectLibrary;
using Moritz.Spec;
using Moritz.Symbols;
using Moritz.Palettes;
using Moritz.Algorithm;
using Moritz.Algorithm.PaletteDemo;
using Moritz.Algorithm.Study2c3_1;
using Moritz.Algorithm.SongSix;
using Moritz.Algorithm.Study3Sketch1;
using Moritz.Algorithm.Study3Sketch2;
using Moritz.Xml;

namespace Moritz.Composer
{
    public class ComposableSvgScore : SvgScore
    {
        public ComposableSvgScore(string folder, string scoreTitleName, string keywords, string comment, PageFormat pageFormat)
            : base(folder, scoreTitleName, keywords, comment, pageFormat)
        {
        }

        /// <summary>
        /// The krystals and paletteDefs arguments can be null if the algorithm whose name is algorithmName does not use them.
        /// </summary>
        /// <param name="algorithmName"></param>
        /// <param name="krystals"></param>
        /// <param name="paletteDefs"></param>
        /// <returns></returns>
        protected CompositionAlgorithm Algorithm(string algorithmName, List<Krystal> krystals, List<Palette> palettes)
        {
            CompositionAlgorithm algorithm = null;
            switch(algorithmName)
            {
                case "Study 2c3.1":
                    algorithm = new Study2c3_1Algorithm(krystals, palettes);
                    break;
                case "Song Six":
                    algorithm = new SongSixAlgorithm(krystals, palettes);
                    break;
                case "Study 3 sketch 1":
                    algorithm = new Study3Sketch1Algorithm(krystals, palettes);
                    break;
                case "Study 3 sketch 2":
                    algorithm = new Study3Sketch2Algorithm(krystals, palettes);
                    break;
                case "paletteDemo":
                    algorithm = new PaletteDemoAlgorithm(palettes);
                    break;
                default:
                    throw new ApplicationException("unknown algorithm");
            }
            return algorithm;
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
            if(NOutputVoices(bar1) != _algorithm.NumberOfOutputVoices)
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
                else // voice is an InputVoice
                {
                    foreach(OutputVoiceDef ov1 in oVoices)
                    {
                        if(ov1.InputControls == null)
                        {
                            errorString = "\nThis score contains InputVoice(s),\n" +
                                          "so every OutputVoice in the first bar must have an InputControls definition.";
                        }
                    }
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
                            if(ov.InputControls != null)
                            {
                                errorString = "\nNo OutputVoice except the first may ever have an InputControls definition.";
                            }
                        }
                    }
                }
            }
            return errorString;
        }
        #endregion

        /// <summary>
        /// Called by the derived class after setting _midiAlgorithm and Notator
        /// </summary>
        protected bool CreateScore()
        {
            bool success = true;
            List<List<VoiceDef>> barDefsInOneSystem = _algorithm.DoAlgorithm();

            CheckBars(barDefsInOneSystem);

            CreateEmptySystems(barDefsInOneSystem); // one system per bar

            if(_pageFormat.ChordSymbolType != "none") // set by AudioButtonsControl
            {
                Notator.ConvertVoiceDefsToNoteObjects(this.Systems);

                if(ReferencedOutputVoicesAreBeingPrinted())
                {
                    FinalizeSystemStructure(); // adds barlines, joins bars to create systems, etc.

                    /// The systems do not yet contain Metrics info.
                    /// The systems are given Metrics inside the following function then justified internally,
                    /// both horizontally and vertically.
                    Notator.CreateMetricsAndJustifySystems(this.Systems);

                    CreatePages();
                }
                else
                {
                    MessageBox.Show("Score error:\n\nAll output voices that are referenced by an input voice must be printed.");
                    success = false;
                }
            }
            return success;
        }

        #region ReferencedOutputVoicesAreBeingPrinted

        private bool ReferencedOutputVoicesAreBeingPrinted()
        {
            bool rval = true;
            List<byte> printedOutputVoiceIDs = PrintedOutputVoiceIDs();
            List<byte> referencedOutputVoiceIDs = ReferencedOutputVoiceIDs();
            foreach(byte outputVoiceID in referencedOutputVoiceIDs)
            {
                if(!printedOutputVoiceIDs.Contains(outputVoiceID))
                {
                    rval = false;
                }
            }
            return rval;
        }

        private List<byte> PrintedOutputVoiceIDs()
        {
            List<byte> printedOutputVoiceIDs = new List<byte>();
            foreach(SvgSystem system in this.Systems)
            {
                foreach(Staff staff in system.Staves)
                {
                    OutputStaff oStaff = staff as OutputStaff;
                    if(oStaff != null)
                    {
                        foreach(Voice voice in oStaff.Voices)
                        {
                            OutputVoice oVoice = voice as OutputVoice;
                            Debug.Assert(oVoice != null && oVoice.VoiceID != null);
                            byte voiceID = (byte) oVoice.VoiceID;
                            if(!printedOutputVoiceIDs.Contains(voiceID))
                            {
                                printedOutputVoiceIDs.Add(voiceID);
                            }
                        }
                    }
                }
            }
            return printedOutputVoiceIDs;
        }

        private List<byte> ReferencedOutputVoiceIDs()
        {
            List<byte> referencedOutputVoiceIDs = new List<byte>();
            foreach(SvgSystem system in this.Systems)
            {
                foreach(Staff staff in system.Staves)
                {
                    InputStaff iStaff = staff as InputStaff;
                    if(iStaff != null)
                    {
                        foreach(Voice voice in iStaff.Voices)
                        {
                            InputVoice iVoice = voice as InputVoice;
                            Debug.Assert(iVoice != null);
                            foreach(NoteObject noteObject in iVoice.NoteObjects)
                            {
                                InputChordSymbol ics = noteObject as InputChordSymbol;
                                if(ics != null)
                                {
                                    List<byte> seqVoiceIDs = ics.InputChordDef.SeqVoiceIDs;
                                    foreach(byte voiceID in seqVoiceIDs)
                                    {  
                                        if(!referencedOutputVoiceIDs.Contains(voiceID))
                                        {
                                            referencedOutputVoiceIDs.Add(voiceID);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return referencedOutputVoiceIDs;
        }
        #endregion CheckThatReferencedOutputVoicesAreBeingPrinted

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
        public void CreateEmptySystems(List<List<VoiceDef>> barDefsInOneSystem)
        {
            foreach(List<VoiceDef> barVoiceDefs in barDefsInOneSystem)
            {
                SvgSystem system = new SvgSystem(this);
                this.Systems.Add(system);
            }

            CreateEmptyOutputStaves(barDefsInOneSystem);
            CreateEmptyInputStaves(barDefsInOneSystem);
        }

        private void CreateEmptyOutputStaves(List<List<VoiceDef>> barDefsInOneSystem)
        {
            int nPrintedOutputStaves = _pageFormat.OutputVoiceIndicesPerStaff.Count;

            for(int i = 0; i < Systems.Count; i++)
            {
                SvgSystem system = Systems[i];
                List<VoiceDef> barDef = barDefsInOneSystem[i];

                byte midiChannel = 0;
                for(int printedStaffIndex = 0; printedStaffIndex < nPrintedOutputStaves; printedStaffIndex++)
                {
                    string staffname = StaffName(i, printedStaffIndex);
                    OutputStaff outputStaff = new OutputStaff(system, staffname, _pageFormat.StafflinesPerStaff[printedStaffIndex], _pageFormat.Gap, _pageFormat.StafflineStemStrokeWidth);

                    List<byte> outputVoiceIndices = _pageFormat.OutputVoiceIndicesPerStaff[printedStaffIndex];
                    for(int ovIndex = 0; ovIndex < outputVoiceIndices.Count; ++ovIndex)
                    {
                        byte? voiceID = null;
                        if(_pageFormat.InputVoiceIndicesPerStaff.Count > 0)
                        {
                            voiceID = (byte?)outputVoiceIndices[ovIndex];
                        }

                        OutputVoiceDef outputVoiceDef = barDef[outputVoiceIndices[ovIndex]] as OutputVoiceDef;
                        Debug.Assert(outputVoiceDef != null);
                        OutputVoice outputVoice = new OutputVoice(outputStaff, midiChannel++, voiceID, outputVoiceDef.MasterVolume);
                        outputVoice.VoiceDef = outputVoiceDef;
                        outputStaff.Voices.Add(outputVoice);
                    }
                    SetStemDirections(outputStaff);
                    system.Staves.Add(outputStaff);
                }
            }
        }

        private void CreateEmptyInputStaves(List<List<VoiceDef>> barDefsInOneSystem)
        {
            int nPrintedOutputStaves = _pageFormat.OutputVoiceIndicesPerStaff.Count;
            int nPrintedInputStaves = _pageFormat.InputVoiceIndicesPerStaff.Count;
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

                    List<byte> inputVoiceIndices = _pageFormat.InputVoiceIndicesPerStaff[staffIndex];
                    for(int ivIndex = 0; ivIndex < inputVoiceIndices.Count; ++ivIndex)
                    {
                        InputVoiceDef inputVoiceDef = barDef[inputVoiceIndices[ivIndex] + _algorithm.NumberOfOutputVoices] as InputVoiceDef;
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

