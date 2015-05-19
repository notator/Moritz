using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Algorithm;
using Moritz.Palettes;
using Moritz.Symbols;
using Moritz.Spec;

namespace Moritz.Algorithm.Study3Sketch2
{
    /// <summary>
    /// Algorithm for testing the new input staves.
    /// This may develope as composition progresses...
    /// </summary>
    public class Study3Sketch2Algorithm : CompositionAlgorithm
    {
        /// <summary>
        /// This constructor can be called with both parameters null,
        /// just to get the overridden properties.
        /// </summary>
        public Study3Sketch2Algorithm()
            : base()
        {
        }

        public override List<int> MidiChannelIndexPerOutputVoice { get { return new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 }; } }
        public override List<int> MasterVolumePerOutputVoice { get { return new List<int>() { 100, 100, 100, 100, 100, 100, 100, 100 }; } }
        public override int NumberOfInputVoices { get { return 1; } }  
        public override int NumberOfBars { get { return 5; } }

        /// <summary>
        /// See CompositionAlgorithm.DoAlgorithm()
        /// </summary>
        public override List<List<VoiceDef>> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            _krystals = krystals;
            _palettes = palettes;

            List<List<VoiceDef>> bars = new List<List<VoiceDef>>();
            List<VoiceDef> bar1 = CreateBar1();
            bars.Add(bar1);
            int bar2StartMsPos = GetEndMsPosition(bar1);
            List<VoiceDef> bar2 = CreateBar2(bar2StartMsPos);
            bars.Add(bar2);
            int bar3StartMsPos = GetEndMsPosition(bar2);
            List<List<VoiceDef>> bars3to5 = CreateBars3to5(bar3StartMsPos);
            foreach(List<VoiceDef> bar in bars3to5)
            {
                bars.Add(bar);
            }
            Debug.Assert(bars.Count == NumberOfBars);
            base.SetOutputVoiceChannelsAndMasterVolumes(bars[0]);
            List<InputChordDef> inputChordSymbolsInBar2 = GetInputChordDefsInBar(bars[1]);
            SetBar2NoteOnNoteOffControls(inputChordSymbolsInBar2);
            List<InputChordDef> inputChordSymbolsInBar3 = GetInputChordDefsInBar(bars[2]);
            SetBar3PitchWheelToVolumeControls(inputChordSymbolsInBar3);
            List<InputChordDef> inputChordSymbolsInBar4 = GetInputChordDefsInBar(bars[3]);
            SetBar4LimitedFadeControls(inputChordSymbolsInBar4);
            List<InputChordDef> inputChordSymbolsInBar5 = GetInputChordDefsInBar(bars[4]);
            SetBar5SpeedControls(inputChordSymbolsInBar5);

            return bars;
        }

        /// <summary>
        /// Returns all the InputChordDefs in the bar.
        /// </summary>
        private List<InputChordDef> GetInputChordDefsInBar(List<VoiceDef> bar)
        {
            List<InputChordDef> inputChordDefs = new List<InputChordDef>();

            foreach(VoiceDef voiceDef in bar)
            {
                InputVoiceDef inputVoiceDef = voiceDef as InputVoiceDef;
                if(inputVoiceDef != null)
                {
                    foreach(IUniqueDef uniqueDef in inputVoiceDef.UniqueDefs)
                    {
                        InputChordDef icd = uniqueDef as InputChordDef;
                        if(icd != null)
                        {
                            inputChordDefs.Add(icd);
                        }
                    }
                }
            }
            return inputChordDefs;
        }

        private void SetBar2NoteOnNoteOffControls(List<InputChordDef> bar2InputChordDefs)
        {
			InputControls ics1 = new InputControls();
			ics1.NoteOffOption = NoteOffOption.stopChord;
			bar2InputChordDefs[0].InputNoteDefs[0].InputControls = ics1;

			InputControls ics2 = new InputControls();
			ics2.NoteOffOption = NoteOffOption.fade;
			bar2InputChordDefs[1].InputNoteDefs[0].InputControls = ics2;
        }

        private void SetBar3PitchWheelToVolumeControls(List<InputChordDef> bar3InputChordDefs)
        {
            foreach(InputChordDef inputChordDef in bar3InputChordDefs)
            {
                InputControls ics = new InputControls();
                //ics.NoteOnKeyOption = NoteOnKeyOption.matchExactly; // this is the current value in the voice (has no effect)
                ics.NoteOffOption = NoteOffOption.fade; // this is the current value in the voice (has no effect)
                ics.PitchWheelOption = ControllerOption.volume;
                ics.MaximumVolume = 100;
                ics.MinimumVolume = 50;
				inputChordDef.InputNoteDefs[0].InputControls = ics;
            }
        }

        private void SetBar4LimitedFadeControls(List<InputChordDef> bar4InputChordDefs)
        {
            int numberOfObjectsInFade = 4;
            foreach(InputChordDef inputChordDef in bar4InputChordDefs)
            {
                InputControls ics = new InputControls();
                //ics.NoteOnKeyOption = NoteOnKeyOption.matchExactly; // this is the current value in the voice (has no effect)
                ics.NoteOffOption = NoteOffOption.stopChord;
                ics.NumberOfObjectsInFade = numberOfObjectsInFade++;
                ics.PitchWheelOption = ControllerOption.pitchWheel;
				inputChordDef.InputNoteDefs[0].InputControls = ics;
            }
        }

        private void SetBar5SpeedControls(List<InputChordDef> bar5InputChordDefs)
        {
            foreach(InputChordDef inputChordDef in bar5InputChordDefs)
            {
                InputControls ics = new InputControls();

                ics.PitchWheelOption = ControllerOption.pitchWheel; // this is the current value in the voice (has no effect)
                ics.SpeedOption = SpeedOption.noteOnKey;
                ics.MaxSpeedPercent = 500;
				inputChordDef.InputNoteDefs[0].InputControls = ics;
            }
        }
        #region CreateBar1()
        List<VoiceDef> CreateBar1()
        {
            List<VoiceDef> bar = new List<VoiceDef>();

            byte channel = 0;
            foreach(Palette palette in _palettes)
            {
                OutputVoiceDef outputVoice = new OutputVoiceDef();
                bar.Add(outputVoice);
                WriteVoiceMidiDurationDefs1(outputVoice, palette);
                ++channel;
            }

            InputVoiceDef inputVoiceDef = new InputVoiceDef();
            VoiceDef topVoice = bar[0];
            foreach(IUniqueDef iud in topVoice.UniqueDefs)
            {
                MidiChordDef mcd = iud as MidiChordDef;
                RestDef rd = iud as RestDef;
                if(mcd != null)
                {
                    byte pitch = (byte)(mcd.NotatedMidiPitches[0] + 36);
                    InputChordDef icd = new InputChordDef(mcd.MsPosition, mcd.MsDuration, pitch, null, 0, 1, null);
                    inputVoiceDef.UniqueDefs.Add(icd);
                }
                else if(rd != null)
                {
                    RestDef newRest = new RestDef(rd.MsPosition, rd.MsDuration);
                    inputVoiceDef.UniqueDefs.Add(newRest);
                }
            }

			#region set cascading inputControls on the first InputChordDef  (for testing)
			InputChordDef inputChordDef1 = inputVoiceDef.UniqueDefs[0] as InputChordDef; // no need to check for null here.

			InputControls chordInputControls = new InputControls();
			chordInputControls.NoteOnVelocityOption = NoteOnVelocityOption.overridden;
			inputChordDef1.InputControls = chordInputControls;

			InputControls noteInputControls = new InputControls();
			noteInputControls.NoteOnVelocityOption = NoteOnVelocityOption.scaled;
			inputChordDef1.InputNoteDefs[0].InputControls = noteInputControls;
			
			InputControls trkRefInputControls = new InputControls();
			trkRefInputControls.NoteOnVelocityOption = NoteOnVelocityOption.shared;
			inputChordDef1.InputNoteDefs[0].TrkRefs[0].InputControls = trkRefInputControls;
			 
			#endregion
				 
            bar.Add(inputVoiceDef);

            return bar;
        }

        private void WriteVoiceMidiDurationDefs1(OutputVoiceDef outputVoice, Palette palette)
        {
            int bar1ChordMsSeparation = 1500;
            int msPosition = 0;

            for(int i = 0; i < palette.Count;++i)
            {
                IUniqueDef noteDef = palette.UniqueDurationDef(i);
                noteDef.MsPosition = msPosition;
                RestDef restDef = new RestDef(msPosition + noteDef.MsDuration, bar1ChordMsSeparation - noteDef.MsDuration);
                msPosition += bar1ChordMsSeparation;
                outputVoice.UniqueDefs.Add(noteDef);
                outputVoice.UniqueDefs.Add(restDef);
            }
        }
        #endregion CreateBar1()

        #region CreateBar2()
        /// <summary>
        /// This function creates only one bar, but with VoiceDef objects. 
        /// </summary>
        List<VoiceDef> CreateBar2(int bar2StartMsPos)
        {
            List<VoiceDef> bar = new List<VoiceDef>();

            byte channel = 0;
            List<OutputVoiceDef> voiceDefs = new List<OutputVoiceDef>();
            foreach(Palette palette in _palettes)
            {
                bar.Add(new OutputVoiceDef());
                OutputVoiceDef voiceDef = palette.NewOutputVoiceDef();
                voiceDef.SetMsDuration(6000);
                voiceDefs.Add(voiceDef);
                ++channel;
            }

            int msPosition = bar2StartMsPos;
            int maxBarMsPos = 0;
            int startMsDifference = 1500;
            for(int i = 0; i < voiceDefs.Count; ++i)
            {
                int maxMsPos = WriteVoiceMidiDurationDefsInBar2(bar[i], voiceDefs[i], msPosition, bar2StartMsPos);
                maxBarMsPos = maxBarMsPos > maxMsPos ? maxBarMsPos : maxMsPos;
                msPosition += startMsDifference;
            }

            // now add the final rest in the bar
            for(int i = 0; i < voiceDefs.Count; ++i)
            {
                int mdsdEndPos = voiceDefs[i].EndMsPosition;
                if(maxBarMsPos > mdsdEndPos)
                {
                    RestDef rest2Def = new RestDef(mdsdEndPos, maxBarMsPos - mdsdEndPos);
                    bar[i].UniqueDefs.Add(rest2Def);
                }
            }

            InputVoiceDef inputVoiceDef = new InputVoiceDef(maxBarMsPos - bar2StartMsPos);
            inputVoiceDef.StartMsPosition = bar2StartMsPos; 
            int msPos = bar2StartMsPos;
            for(int i = 0; i < bar.Count; ++i)
            {
                InputChordDef inputChordDef = new InputChordDef(msPos, startMsDifference, 64, null, (byte)i, (byte)12, null);
                inputVoiceDef.InsertInRest(inputChordDef);
                msPos += startMsDifference;
            }

            bar.Add(inputVoiceDef);
            return bar;
        }

        /// <summary>
        /// Writes the first rest (if any) and the VoiceDef to the voice.
        /// Returns the endMsPos of the VoiceDef. 
        /// </summary>
        private int WriteVoiceMidiDurationDefsInBar2(VoiceDef voice, OutputVoiceDef voiceDef, int msPosition, int bar2StartMsPos)
        {
            if(msPosition > bar2StartMsPos)
            {
                RestDef rest1Def = new RestDef(bar2StartMsPos, msPosition - bar2StartMsPos);
                voice.UniqueDefs.Add(rest1Def);
            }

            voiceDef.StartMsPosition = msPosition;
            foreach(IUniqueDef iu in voiceDef)
            {
                voice.UniqueDefs.Add(iu);
            }

            return voiceDef.EndMsPosition;
        }
        #endregion CreateBar2()

        #region CreateBars3to5()
        /// <summary>
        /// This function creates three bars, identical to bar2 with two internal barlines.
        /// The VoiceDef objects cross barlines. 
        /// </summary>
        List<List<VoiceDef>> CreateBars3to5(int bar3StartMsPos)
        {
            List<List<VoiceDef>> bars = new List<List<VoiceDef>>();
            List<VoiceDef> threeBars = CreateBar2(bar3StartMsPos);

            //int bar4StartPos = bar3StartMsPos + 6000;
            int bar4StartPos = bar3StartMsPos + 5950;
            List<List<VoiceDef>> bars3And4Plus5 = SplitBar(threeBars, bar4StartPos);
            int bar5StartPos = bar3StartMsPos + 10500;
            List<List<VoiceDef>> bars4and5 = SplitBar(bars3And4Plus5[1], bar5StartPos);

            bars.Add(bars3And4Plus5[0]); // bar 3
            //bars.Add(bars3And4Plus5[1]); // bars 4 and 5
            bars.Add(bars4and5[0]); // bar 4
            bars.Add(bars4and5[1]); // bar 5
            return bars;
        }

        #endregion CreateBars3to5()
    }
}
