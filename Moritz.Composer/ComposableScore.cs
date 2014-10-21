using System;
using System.Diagnostics;
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
                errorString = BasicCheck(voiceDefsPerSystemPerBar);
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
        private string BasicCheck(List<List<VoiceDef>> voiceDefsPerSystemPerBar)
        {
            string errorString = null;
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
        protected void CreateScore()
        {
            List<List<VoiceDef>> barDefsInOneSystem = _algorithm.DoAlgorithm();

            CheckBars(barDefsInOneSystem);

            CreateEmptySystems(barDefsInOneSystem); // one system per bar

            if(_pageFormat.ChordSymbolType != "none") // set by AudioButtonsControl
            {
                Notator.ConvertVoiceDefsToNoteObjects(this.Systems);

                FinalizeSystemStructure(); // adds barlines, joins bars to create systems, etc.

                /// The systems do not yet contain Metrics info.
                /// The systems are given Metrics inside the following function then justified internally,
                /// both horizontally and vertically.
                Notator.CreateMetricsAndJustifySystems(this.Systems);

                CreatePages();
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
        public void CreateEmptySystems(List<List<VoiceDef>> barDefsInOneSystem)
        {
            foreach(List<VoiceDef> barVoiceDefs in barDefsInOneSystem)
            {
                byte midiChannel = 0;
                int inputVoice_StaffIndex = 0;
                int outputVoice_StaffIndex = 0;
                int nOutputVoices = _pageFormat.OutputVoiceIndicesPerStaff.Count;
                SvgSystem system = new SvgSystem(this);
                this.Systems.Add(system);
                for(int staffIndex = 0; staffIndex < _pageFormat.StafflinesPerStaff.Count; staffIndex++)
                {
                    string staffname = null;
                    if(this.Systems.Count == 1)
                    {
                        staffname = _pageFormat.LongStaffNames[staffIndex];
                    }
                    else
                    {
                        staffname = _pageFormat.ShortStaffNames[staffIndex];
                    }
                    Staff staff;
                    if(staffIndex >= nOutputVoices)
                    {
                        float gap = _pageFormat.Gap * _pageFormat.InputStavesSizeFactor;
                        float stafflineStemStrokeWidth = _pageFormat.StafflineStemStrokeWidth * _pageFormat.InputStavesSizeFactor;
                        staff = new InputStaff(system, staffname, _pageFormat.StafflinesPerStaff[staffIndex], gap, stafflineStemStrokeWidth);

                        List<byte> inputVoiceIndices = _pageFormat.InputVoiceIndicesPerStaff[inputVoice_StaffIndex++];
                        for(int i = 0; i < inputVoiceIndices.Count; ++i)
                        {
                            InputVoice inputVoice = new InputVoice(staff as InputStaff);
                            inputVoice.VoiceDef = barVoiceDefs[nOutputVoices + inputVoiceIndices[i]];
                            Debug.Assert(inputVoice.VoiceDef is InputVoiceDef);
                            staff.Voices.Add(inputVoice);
                        }
                    }
                    else
                    {
                        staff = new OutputStaff(system, staffname, _pageFormat.StafflinesPerStaff[staffIndex], _pageFormat.Gap, _pageFormat.StafflineStemStrokeWidth);

                        List<byte> outputVoiceIndices = _pageFormat.OutputVoiceIndicesPerStaff[outputVoice_StaffIndex++];
                        for(int i = 0; i < outputVoiceIndices.Count; ++i)
                        {
                            int? voiceID = null;
                            if (_pageFormat.InputVoiceIndicesPerStaff.Count > 0)
                            {
                                voiceID = (int?)outputVoiceIndices[i];
                            }
                            OutputVoice outputVoice = new OutputVoice(staff as OutputStaff, voiceID, midiChannel++);
                            outputVoice.VoiceDef = barVoiceDefs[outputVoiceIndices[i]];
                            Debug.Assert(outputVoice.VoiceDef is OutputVoiceDef);
                            staff.Voices.Add(outputVoice);
                        }
                    }
                    SetStemDirections(staff);
                    system.Staves.Add(staff);
                }
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

