using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;

using Multimedia.Midi;

namespace Moritz.Score.Midi
{
    /// <summary>
    /// MidiChords are contained in MidiMoments which can be triggered by a live performer or "assistant".
    /// </summary>
    public class MidiChord : MidiDurationSymbol
    {
        public MidiChord(int channel, MidiChordDef midiChordDef, int msPosition, int msDuration, ChannelState channelState, int minimumOrnamentChordMsDuration)
            : base(channel, msPosition, msDuration)
        {
            List<BasicMidiChordDef> basicMidiChordDefs = midiChordDef.BasicMidiChordDefs;
            Debug.Assert(basicMidiChordDefs.Count > 0);
            List<int> realBasicMidiChordDurations = MidiChordDef.GetIntDurations(msDuration, midiChordDef.BasicChordDurations, basicMidiChordDefs.Count);

            _firstChordNotes = new List<byte>(midiChordDef.BasicMidiChordDefs[0].Pitches);
            var notesToStop = new SortedSet<byte>();
            int i = 0;
            foreach(BasicMidiChordDef basicMidiChordDef in midiChordDef.BasicMidiChordDefs)
            {
                this.BasicMidiChords.Add(new BasicMidiChord(channel, this, basicMidiChordDef, realBasicMidiChordDurations[i++]));
                if(basicMidiChordDef.HasChordOff)
                {
                    foreach(byte note in basicMidiChordDef.Pitches)
                    {
                        if(!notesToStop.Contains(note))
                            notesToStop.Add(note);
                    }
                }
            }

            Repeat = midiChordDef.Repeat;

            if(midiChordDef.HasChordOff && notesToStop.Count > 0)
                SetChordOff(channel, notesToStop);

            if(basicMidiChordDefs[0].BankIndex == null && midiChordDef.Bank != null && midiChordDef.Bank != channelState.Bank)
            {
                Bank = new BankControl(channel, (byte)midiChordDef.Bank);
            }
            if(basicMidiChordDefs[0].PatchIndex == null && midiChordDef.Patch != null && midiChordDef.Patch != channelState.Patch)
            {
                Patch = new PatchControl(channel, (byte)midiChordDef.Patch);
            }
            if(midiChordDef.Volume != null && midiChordDef.Volume != channelState.Volume)
            {
                Volume = new Volume(channel, (byte)midiChordDef.Volume, ControlContinuation.NoChange);
            }
            if(midiChordDef.PitchWheelDeviation != null && midiChordDef.PitchWheelDeviation != channelState.PitchWheelDeviation)
            {
                PitchWheelDeviation = new PitchWheelDeviation(channel, (byte)midiChordDef.PitchWheelDeviation);
            }
            if(midiChordDef.MidiChordSliderDefs != null)
                CreateSliders(channel, midiChordDef.MidiChordSliderDefs, msDuration, channelState);
        }

        /// <summary>
        /// Recalculate msDurations according to minimumOrnamentChordMsDuration,
        /// and return a possibly shortened list of BasicMidiChordDefs
        /// </summary>
        /// <param name="svgMidiChordDefs"></param>
        /// <param name="minimumOrnamentChordMsDuration"></param>
        /// <returns></returns>
        //private List<BasicMidiChordDef> GetRealBasicMidiChordDefs(List<BasicMidiChordDef> basicMidiChordDefs, int minimumOrnamentChordMsDuration)
        //{
        //    //Debug.Assert(svgMidiChordDefs.Count > 1);

        //    List<float> basicDurations = new List<float>();
        //    // basicDurations are the durations between *positions* in svgMidiChordDefs. 
        //    #region get basic durations
        //    int totalMsDuration = 0;
        //    for(int i = 0; i < basicMidiChordDefs.Count; ++i)
        //    {
        //        basicDurations.Add((float)basicMidiChordDefs[i].MsDuration);
        //        totalMsDuration += basicMidiChordDefs[i].MsDuration;
        //    }
        //    #endregion

        //    float durationFactor = GetDurationFactor(basicDurations, minimumOrnamentChordMsDuration);
        //    List<BasicMidiChordDef> realMidiChordDefs = null;
        //    if(durationFactor > 1F)
        //    {
        //        realMidiChordDefs = new List<BasicMidiChordDef>();
        //        List<int> durations = GetDurations(totalMsDuration, basicDurations, durationFactor);
        //        // durations is the right length (may be shorter than basicMidiChordDefs)
        //        for(int i = 0; i < durations.Count; ++i)
        //        {
        //            BasicMidiChordDef b = basicMidiChordDefs[i];
        //            BasicMidiChordDef bmcd = new BasicMidiChordDef(durations[i], b.BankIndex, b.PatchIndex, b.HasChordOff, b.Notes, b.Velocities);
        //            realMidiChordDefs.Add(bmcd);
        //        }
        //    }
        //    else
        //        realMidiChordDefs = basicMidiChordDefs; 

        //    return realMidiChordDefs;
        //}
        private float GetDurationFactor(List<float> basicDurations, int minimumOrnamentChordMsDuration)
        {
            float smallest = float.MaxValue;
            foreach(float f in basicDurations)
                smallest = smallest < f ? smallest : f;
            // smallest is the smallest basicDuration

            float durationFactor = 1F;
            if(smallest < (float)minimumOrnamentChordMsDuration)
            {
                durationFactor = (float)minimumOrnamentChordMsDuration / smallest;
                // durationfactor > 1, so there will be less items in actualFloatDurations than in relativeDurations
            }
            return durationFactor;
        }
        private List<int> GetDurations(int totalMsDuration, List<float> basicDurations, float durationFactor)
        {
            // basicDurations are the float durations as they would be without a minimumOrnamentChordMsDuration. 

            List<float> actualFloatDurations = new List<float>();
            float fSum = 0F;
            int index = 0;
            while(index < basicDurations.Count && fSum < (float)totalMsDuration)
            {
                float fDuration = durationFactor * basicDurations[index++];
                fSum += fDuration;
                actualFloatDurations.Add(fDuration);
            }

            if(fSum > (float)totalMsDuration)
            {
                actualFloatDurations.RemoveAt(actualFloatDurations.Count - 1);
            }

            if(actualFloatDurations.Count == 0)
            {
                actualFloatDurations.Add(totalMsDuration);
            }

            // actualFloatDurations now has the actual final length.

            fSum = 0F;
            float fSumBefore = 0F;
            List<int> actualIntDurations = new List<int>();
            foreach(float fd in actualFloatDurations)
            {
                fSum += fd;
                actualIntDurations.Add((int)(fSum - fSumBefore));
                fSumBefore += fd;
            }

            int intSum = 0;
            foreach(int i in actualIntDurations)
                intSum += i;
            Debug.Assert(intSum <= totalMsDuration);
            if(intSum < totalMsDuration)
            {
                int lastDuration = actualIntDurations[actualIntDurations.Count - 1];
                lastDuration += (totalMsDuration - intSum);
                actualIntDurations.RemoveAt(actualIntDurations.Count - 1);
                actualIntDurations.Add(lastDuration);
            }

            return actualIntDurations;
        }

        private void SetChordOff(int channel, SortedSet<byte> notesToStop)
        {
            this.ChordOff = new ChordOff(null);
            foreach(byte note in notesToStop)
            {
                NoteOff noteOff = new NoteOff(channel, note, 64);
                ChordOff.AddNote(noteOff);
            }
        }

        private void CreateSliders(int channel, MidiChordSliderDefs sliderDefs, int msDuration, ChannelState channelState)
        {
            if(sliderDefs.ModulationWheelMsbs != null)
                this.ModulationWheelSlider = new MidiModulationWheelSlider(sliderDefs.ModulationWheelMsbs, channel, msDuration);
            if(sliderDefs.PanMsbs != null)
                this.PanSlider = new MidiPanSlider(sliderDefs.PanMsbs, channel, msDuration);
            if(sliderDefs.PitchWheelMsbs != null)
                this.PitchWheelSlider = new MidiPitchWheelSlider(sliderDefs.PitchWheelMsbs, channel, msDuration);
            if(sliderDefs.ExpressionMsbs != null)
                this.ExpressionSlider = new MidiExpressionSlider(sliderDefs.ExpressionMsbs, channel, msDuration);
        }

        public byte Velocity
        {
            get { return BasicMidiChords[0].Velocity; }
            set
            {
                Debug.Assert(value <= 127);
                foreach(BasicMidiChord basicMidiChord in BasicMidiChords)
                {
                    basicMidiChord.Velocity = value;
                }
            }
        }
        public void SetMidiPitches(List<ChannelMessage> chordOnMessages)
        {
            BasicMidiChordDef bmcd = new BasicMidiChordDef();
            foreach(ChannelMessage channelmessage in chordOnMessages)
            {
                bmcd.Pitches.Add((byte)channelmessage.Data1);
                bmcd.Velocities.Add((byte)channelmessage.Data2);
            }
            bmcd.HasChordOff = true;

            BasicMidiChords.Clear();
            BasicMidiChords.Add(new BasicMidiChord(this.Channel, this, bmcd, this.MsDuration));
        }

        //public void SetMidiPitches(List<byte> midiPitches)
        //{
        //    int channel = this.Channel;
        //    byte velocity = (byte)this.Velocity;
        //    BasicMidiChordDef bmcd = new BasicMidiChordDef();
        //    foreach(byte pitch in midiPitches)
        //    {
        //        bmcd.Notes.Add(pitch);
        //        bmcd.Velocities.Add(velocity);
        //    }
        //    bmcd.HasChordOff = true;

        //    BasicMidiChords.Clear();
        //    BasicMidiChords.Add(new BasicMidiChord(this.Channel, this, bmcd));
        //}
        /// <summary>
        /// Used when performanceOptions.PerformersPitchesType == PerformersPitchesType.AsPerformed
        /// </summary>
        /// <param name="midiPitches"></param>
        public void ResetMidiPitches(List<byte> midiPitches)
        {
            Debug.Assert(BasicMidiChords != null && BasicMidiChords.Count > 0);
            List<BasicMidiChord> newBasicMidiChords = new List<BasicMidiChord>();
            List<int> existingBasePitches = new List<int>();

            foreach(BasicMidiChord mbc in BasicMidiChords)
            {
                existingBasePitches.Add(mbc.ChordOn.Notes[0].Pitch);
            }
            int originalRoot = existingBasePitches[0];
            for(int i = 0; i < existingBasePitches.Count; ++i)
            {
                int originalDiff = existingBasePitches[i] - originalRoot;
                List<byte> newPitches = new List<byte>();
                foreach(int midiPitch in midiPitches)
                {
                    int newPitch = midiPitch + originalDiff;
                    newPitch = newPitch < 0 ? 0 : newPitch;
                    newPitch = newPitch > 127 ? 127 : newPitch;
                    newPitches.Add((byte)newPitch);
                }

                BasicMidiChord bmc = BasicMidiChords[i];
                int msDuration = bmc.MsDuration;
                byte? bank = null;
                if(bmc.BankControl != null)
                    bank = (byte)bmc.BankControl.ChannelMessages[0].Data1;
                byte? patch = null;
                if(bmc.PatchControl != null)
                    patch = (byte)bmc.PatchControl.ChannelMessages[0].Data1;
                bool hasChordOff = (bmc.ChordOff != null);
                List<byte> velocities = new List<byte>();
                foreach(int pitch in newPitches)
                    velocities.Add(bmc.Velocity);

                BasicMidiChordDef bmcd = new BasicMidiChordDef(msDuration, bank, patch, hasChordOff, newPitches, velocities);
                newBasicMidiChords.Add(new BasicMidiChord(this.Channel, this, bmcd, this.MsDuration));
            }
            this.BasicMidiChords = newBasicMidiChords;
        }
        public int StartTimeMilliseconds;
        public int Delay;
        private List<byte> _firstChordNotes = null; // used in the log when recording

        #region runtime
        /// <summary>
        /// Called when the user issues a ChordOff
        /// </summary>
        public void Cancel()
        {
            FreeWorker(this.BasicMidiChordsBackgroundWorker);
            if(ModulationWheelSlider != null)
                FreeWorker(this.ModulationWheelSlider.BackgroundWorker);
            if(PanSlider != null)
                FreeWorker(this.PanSlider.BackgroundWorker);
            if(PitchWheelSlider != null)
                FreeWorker(this.PitchWheelSlider.BackgroundWorker);
            if(ExpressionSlider != null)
                FreeWorker(this.ExpressionSlider.BackgroundWorker);
        }

        private void FreeWorker(BackgroundWorker worker)
        {
            if(worker != null && worker.IsBusy)
            {
                if(!worker.CancellationPending)
                    worker.CancelAsync();
                while(worker.IsBusy)
                    Thread.Sleep(0);
            }
        }

        public BackgroundWorker BasicMidiChordsBackgroundWorker = null;

        #endregion runtime

        // Volume, PitchWheelDeviation and BasicMidiChords are sent in their own thread.
        public BankControl Bank = null;
        public PatchControl Patch = null;
        public Volume Volume = null;
        // If Repeat is true, this MidiChord repeats in assisted performances if the
        // performed duration is longer than the default duration.
        public bool Repeat; 
        
        public PitchWheelDeviation PitchWheelDeviation = null;
        public List<BasicMidiChord> BasicMidiChords = new List<BasicMidiChord>();

        // Each Slider has its own BackgroundWorker, and is sent in its own thread
        public MidiChordSlider PitchWheelSlider = null;
        public MidiChordSlider PanSlider = null;
        public MidiChordSlider ModulationWheelSlider = null;
        public MidiChordSlider ExpressionSlider = null;

        // The ChordOff is sent by a ChordOff manager thread.
        public ChordOff ChordOff = null;
    }
}
