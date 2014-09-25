using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;

namespace Moritz.AssistantComposer
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
        public Study3Sketch2Algorithm(List<Krystal> krystals, List<Palette> palettes)
            : base(krystals, palettes)
        {
        }

        public override int NumberOfInputVoices { get { return 1; } }
        public override int NumberOfOutputVoices { get { return 8; } }
        public override int NumberOfBars { get { return 5; } }

        /// <summary>
        /// The DoAlgorithm() function is special to a particular composition.
        /// The function returns a sequence of bar definitions. Each bar is a list of Voices (conceptually from top to bottom
        /// in a system, though the actual order can be changed in the Assistant Composer's options).
        /// Each bar in the sequence has the same number of Voices. Voices at the same position in each bar are continuations
        /// of the same overall voice, and may be concatenated later. OutputVoices at the same position in each bar have the
        /// same midi channel.
        /// Midi channels:
        /// By convention, algorithms use midi channels having indices which increase from top to bottom in the
        /// system, starting at 0. Midi channels may not occur twice in the same system. Each algorithm declares which midi
        /// channels it uses in the MidiChannels() function (see above). For an example, see Study2bAlgorithm.
        /// Each 'bar definition' is actually contained in the UniqueDefs list in each Voice (i.e. Voice.UniqueDefs).
        /// The Voice.NoteObjects lists are still empty when DoAlgorithm() returns.
        /// The Voice.UniqueDefs will be converted to NoteObjects having a specific notation later (in Notator.AddSymbolsToSystems()).
        /// ACHTUNG:
        /// The top (=first) Voice in each bar must be an OutputVoice.
        /// This can be followed by zero or more OutputVoices, followed by zero or more InputVoices.
        /// The chord definitions in OutputVoice.UniqueDefs must be MidiChordDefs.
        /// The chord definitions in InputVoice.UniqueDefs must be UniqueInputChordDefs.
        /// </summary>
        public override List<List<Voice>> DoAlgorithm()
        {
            List<List<Voice>> bars = new List<List<Voice>>();
            List<Voice> bar1 = CreateBar1();
            bars.Add(bar1);
            int bar2StartMsPos = GetEndMsPosition(bar1);
            List<Voice> bar2 = CreateBar2(bar2StartMsPos);
            bars.Add(bar2);
            int bar3StartMsPos = GetEndMsPosition(bar2);
            List<List<Voice>> bars3to5 = CreateBars3to5(bar3StartMsPos);
            foreach(List<Voice> bar in bars3to5)
            {
                bars.Add(bar);
            }

            Debug.Assert(bars.Count == NumberOfBars);

            List<byte> masterVolumes = new List<byte>(){ 100, 100, 100, 100, 100, 100, 100, 100 }; // 8 OutputVoices
            base.SetOutputVoiceMasterVolumes(bars[0], masterVolumes);

            #region initialize the OutputVoice InputControls
            List<InputControls> inputControlsList = new List<InputControls>();
            for(int i = 0; i < masterVolumes.Count; ++i)
            {
                InputControls ics = new InputControls();
                ics.NoteOnKeyOption = NoteOnKeyOption.matchExactly;
                inputControlsList.Add(ics);
            }           
            base.SetOutputVoiceInputControls(bars[0], inputControlsList);
            #endregion

            List<MidiChordDef> seqStartsInBar2 = SeqStartsInBar(bars[1]);
            List<MidiChordDef> seqStartsInBar3 = SeqStartsInBar(bars[2]);
            List<MidiChordDef> seqStartsInBar4 = SeqStartsInBar(bars[3]);
            List<MidiChordDef> seqStartsInBar5 = SeqStartsInBar(bars[4]);

            SetBar2NoteOnNoteOffControls(seqStartsInBar2);
            SetBar3PitchWheelToVolumeControls(seqStartsInBar3);
            SetBar4LimitedFadeControls(seqStartsInBar4);
            SetBar5SpeedControls(seqStartsInBar5);

            return bars;
        }

        /// <summary>
        /// Returns all the MidiChordDefs that are the beginnings of Seqs in the bar.
        /// </summary>
        private List<MidiChordDef> SeqStartsInBar(List<Voice> bar)
        {
            List<MidiChordDef> rval = new List<MidiChordDef>();

            List<InputChordDef> inputChordDefsInBar = InputChordDefsInBar(bar);
            foreach(InputChordDef inputChordDef in inputChordDefsInBar)
            {
                List<MidiChordDef> mcds = inputChordDef.SeqStarts(bar);
                rval.AddRange(mcds);
            }

            return rval;
        }

        /// <summary>
        /// Returns all the InputChordDefs in the bar.
        /// </summary>
        private List<InputChordDef> InputChordDefsInBar(List<Voice> bar)
        {
            List<InputChordDef> rval = new List<InputChordDef>();
            foreach(Voice v in bar)
            {
                InputVoice iv = v as InputVoice;
                if(iv != null)
                {
                    foreach(InputChordDef inputChordDef in iv.InputChordDefs)
                    {
                        rval.Add(inputChordDef);
                    }
                }
            }
            return rval;
        }

        private void SetBar2NoteOnNoteOffControls(List<MidiChordDef> bar2SeqStarts)
        {
            foreach(MidiChordDef mcd in bar2SeqStarts)
            {
                InputControls ics = new InputControls(); // all options are ignore
                ics.NoteOnKeyOption = NoteOnKeyOption.matchExactly;
                ics.NoteOffOption = NoteOffOption.fade;
                mcd.InputControls = ics;
            }
        }

        private void SetBar3PitchWheelToVolumeControls(List<MidiChordDef> bar3SeqStarts)
        {
            foreach(MidiChordDef mcd in bar3SeqStarts)
            {
                InputControls ics = new InputControls(); // created with all options set to ignore
                ics.NoteOnKeyOption = NoteOnKeyOption.matchExactly;
                ics.NoteOffOption = NoteOffOption.fade;
                ics.PitchWheelOption = ControllerOption.volume;
                ics.MaximumVolume = 100;
                ics.MinimumVolume = 50;
                mcd.InputControls = ics;
            }
        }

        private void SetBar4LimitedFadeControls(List<MidiChordDef> bar4SeqStarts)
        {
            int numberOfObjectsInFade = 4;
            foreach(MidiChordDef mcd in bar4SeqStarts)
            {
                InputControls ics = new InputControls(); // created with all options set to ignore
                ics.NoteOnKeyOption = NoteOnKeyOption.matchExactly;
                ics.NoteOffOption = NoteOffOption.limitedFade;
                ics.NumberOfObjectsInFade = numberOfObjectsInFade++;
                ics.PitchWheelOption = ControllerOption.pitchWheel;
                mcd.InputControls = ics;
            }
        }

        private void SetBar5SpeedControls(List<MidiChordDef> bar5SeqStarts)
        {
            foreach(MidiChordDef mcd in bar5SeqStarts)
            {
                InputControls ics = new InputControls(); // created with all options set to ignore
                ics.PitchWheelOption = ControllerOption.pitchWheel;
                ics.SpeedOption = SpeedOption.noteOnKey;
                ics.MaxSpeedPercent = 500;
                mcd.InputControls = ics;
            }
        }

        #region CreateBar1()
        List<Voice> CreateBar1()
        {
            List<Voice> bar = new List<Voice>();

            byte channel = 0;
            foreach(Palette palette in _palettes)
            {
                OutputVoice outputVoice = new OutputVoice(null, channel);
                bar.Add(outputVoice);
                WriteVoiceMidiDurationDefs1(outputVoice, palette);
                ++channel;
            }

            InputVoice inputVoice = new InputVoice(null);
            Voice topVoice = bar[0];
            foreach(IUniqueDef iud in topVoice.UniqueDefs)
            {
                MidiChordDef mcd = iud as MidiChordDef;
                RestDef rd = iud as RestDef;
                if(mcd != null)
                {
                    byte pitch = (byte)(mcd.MidiPitches[0] + 36);
                    InputChordDef icd = new InputChordDef(mcd.MsPosition, mcd.MsDuration, pitch, 0, 1, null);
                    inputVoice.UniqueDefs.Add(icd);
                }
                else if(rd != null)
                {
                    RestDef newRest = new RestDef(rd.MsPosition, rd.MsDuration);
                    inputVoice.UniqueDefs.Add(newRest);
                }
            }
            bar.Add(inputVoice);

            return bar;
        }

        private void WriteVoiceMidiDurationDefs1(OutputVoice outputVoice, Palette palette)
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
        List<Voice> CreateBar2(int bar2StartMsPos)
        {
            List<Voice> bar = new List<Voice>();

            byte channel = 0;
            List<OutputVoiceDef> voiceDefs = new List<OutputVoiceDef>();
            foreach(Palette palette in _palettes)
            {
                bar.Add(new OutputVoice(null, channel));
                OutputVoiceDef voiceDef = new OutputVoiceDef(palette);
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

            InputVoiceDef iVoiceDef = new InputVoiceDef(maxBarMsPos - bar2StartMsPos);
            iVoiceDef.StartMsPosition = bar2StartMsPos; 
            int msPos = bar2StartMsPos;
            for(int i = 0; i < bar.Count; ++i)
            {
                InputChordDef inputChordDef = new InputChordDef(msPos, startMsDifference, 64, (byte)i, (byte)12, null);
                iVoiceDef.InsertInRest(inputChordDef);
                msPos += startMsDifference;
            }

            InputVoice inputVoice = new InputVoice(null);
            inputVoice.UniqueDefs = iVoiceDef.UniqueDefs;

            bar.Add(inputVoice);

            return bar;
        }

        /// <summary>
        /// Writes the first rest (if any) and the VoiceDef to the voice.
        /// Returns the endMsPos of the VoiceDef. 
        /// </summary>
        private int WriteVoiceMidiDurationDefsInBar2(Voice voice, OutputVoiceDef voiceDef, int msPosition, int bar2StartMsPos)
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
        List<List<Voice>> CreateBars3to5(int bar3StartMsPos)
        {
            List<List<Voice>> bars = new List<List<Voice>>();
            List<Voice> threeBars = CreateBar2(bar3StartMsPos);

            //int bar4StartPos = bar3StartMsPos + 6000;
            int bar4StartPos = bar3StartMsPos + 5950;
            List<List<Voice>> bars3And4Plus5 = SplitBar(threeBars, bar4StartPos);
            int bar5StartPos = bar3StartMsPos + 10500;
            List<List<Voice>> bars4and5 = SplitBar(bars3And4Plus5[1], bar5StartPos);

            bars.Add(bars3And4Plus5[0]); // bar 3
            //bars.Add(bars3And4Plus5[1]); // bars 4 and 5
            bars.Add(bars4and5[0]); // bar 4
            bars.Add(bars4and5[1]); // bar 5
            return bars;
        }

        #endregion CreateBars3to5()

    }
}
