
using Moritz.Globals;
using Moritz.Spec;

using Sanford.Multimedia.Midi;

using System;
using System.Collections.Generic;
using System.Threading;

namespace Moritz.Midi
{
    /// <summary>
    /// MidiChords are used to perform MidiChordDefs defined in palettes.
    /// </summary>
    public class MidiChord : MidiDurationSymbol
    {
        public MidiChord(int channel, MidiChordDef midiChordDef, OutputDevice midiOutputDevice)
            : base(channel, 0, midiChordDef.MsDuration)
        {
            _midiOutputDevice = midiOutputDevice;

            List<BasicDurationDef> basicDurationDefs = midiChordDef.BasicDurationDefs;
            //Debug.Assert(basicDurationDefs.Count > 0);
            if(basicDurationDefs.Count < 1)
            {
                throw new ApplicationException();
            }
            List<int> realBasicMidiChordDurations = M.IntDivisionSizes(MsDuration, midiChordDef.BasicDurations);

            var notesToStop = new SortedSet<byte>();
            int i = 0;
            foreach(BasicDurationDef basicDurationDef in midiChordDef.BasicDurationDefs)
            {
                if(basicDurationDef is BasicMidiChordDef basicMidiChordDef)
                {
                    this._basicMidiChords.Add(new BasicMidiChord(channel, this, basicMidiChordDef, realBasicMidiChordDurations[i++]));
                    if(basicMidiChordDef.HasChordOff)
                    {
                        foreach(byte note in basicMidiChordDef.Pitches)
                        {
                            if(!notesToStop.Contains(note))
                                notesToStop.Add(note);
                        }
                    }
                }
            }

            if(midiChordDef.Bank != null)
            {
                _bank = new Bank(channel, (byte)midiChordDef.Bank);
            }
            if(midiChordDef.Patch != null)
            {
                _patch = new PresetCommand(channel, (byte)midiChordDef.Patch);
            }

            // Moritz currently never repeats MidiChords, so the _repeat field is unnecessary.
            // However: the value of midiChordDef.Repeat is saved in SVG-MIDI files,
            // and may be used by the web AssistantPerformer.
            //_repeat = midiChordDef.Repeat;

            if(midiChordDef.PitchWheelDeviation != null)
            {
                _pitchWheelDeviation = new PitchWheelSensitivity(channel, (byte)midiChordDef.PitchWheelDeviation);
            }
            if(midiChordDef.MidiChordSliderDefs != null)
                CreateSliders(channel, midiChordDef.MidiChordSliderDefs, MsDuration);

            SetMessagesDict();
        }

        private void CreateSliders(int channel, MidiChordSliderDefs sliderDefs, int msDuration)
        {
            if(sliderDefs.ModulationWheelMsbs != null && sliderDefs.ModulationWheelMsbs.Count > 0)
                this._modulationWheelSlider = new MidiModulationWheelSlider(sliderDefs.ModulationWheelMsbs, channel, msDuration);
            if(sliderDefs.PanMsbs != null && sliderDefs.PanMsbs.Count > 0)
                this._panSlider = new MidiPanSlider(sliderDefs.PanMsbs, channel, msDuration);
            if(sliderDefs.PitchWheelMsbs != null && sliderDefs.PitchWheelMsbs.Count > 0)
                this._pitchWheelSlider = new MidiPitchWheelSlider(sliderDefs.PitchWheelMsbs, channel, msDuration);
            if(sliderDefs.ExpressionMsbs != null && sliderDefs.ExpressionMsbs.Count > 0)
                this._expressionSlider = new MidiExpressionSlider(sliderDefs.ExpressionMsbs, channel, msDuration);
        }

        private void SetMessagesDict()
        {
            _messagesDict = new SortedDictionary<int, List<ChannelMessage>>();

            int msPosition = 0;
            List<ChannelMessage> startMessages = new List<ChannelMessage>();
            if(_bank != null)
                startMessages.AddRange(_bank.ChannelMessages);
            if(_patch != null)
                startMessages.AddRange(_patch.ChannelMessages);
            if(_pitchWheelDeviation != null)
                startMessages.AddRange(_pitchWheelDeviation.ChannelMessages);

            if(!_messagesDict.ContainsKey(0))
                _messagesDict.Add(msPosition, new List<ChannelMessage>());
            _messagesDict[0].AddRange(startMessages);

            foreach(BasicMidiChord bmc in _basicMidiChords)
            {
                if(!_messagesDict.ContainsKey(msPosition))
                    _messagesDict.Add(msPosition, new List<ChannelMessage>());

                if(bmc.BankControl != null)
                    _messagesDict[msPosition].AddRange(bmc.BankControl.ChannelMessages);
                if(bmc.PatchControl != null)
                    _messagesDict[msPosition].AddRange(bmc.PatchControl.ChannelMessages);
                if(bmc.ChordOn != null)
                    _messagesDict[msPosition].AddRange(bmc.ChordOn.ChannelMessages);

                msPosition += bmc.MsDuration;

                if(!_messagesDict.ContainsKey(msPosition))
                    _messagesDict.Add(msPosition, new List<ChannelMessage>());
                if(bmc.ChordOff != null)
                    _messagesDict[msPosition].AddRange(bmc.ChordOff.ChannelMessages);
            }

            int totalMsDuration = msPosition;

            if(_modulationWheelSlider != null)
                AddSliderMessages(_modulationWheelSlider, totalMsDuration);
            if(_panSlider != null)
                AddSliderMessages(_panSlider, totalMsDuration);
            if(_pitchWheelSlider != null)
                AddSliderMessages(_pitchWheelSlider, totalMsDuration);
            if(_expressionSlider != null)
                AddSliderMessages(_expressionSlider, totalMsDuration);
        }

        private void AddSliderMessages(MidiChordSlider slider, int msDuration)
        {
            List<MidiSliderTime> msts = slider.MidiSliderTimes;
            int msPosition = 0;
            foreach(MidiSliderTime mst in msts)
            {
                if(!_messagesDict.ContainsKey(msPosition))
                {
                    _messagesDict.Add(msPosition, new List<ChannelMessage>());
                }
                _messagesDict[msPosition].AddRange(mst.MidiSlider.ChannelMessages);
                msPosition += mst.MsDuration;
            }
        }

        /// <summary>
        /// Sends the MidiChord using its defined MsDuration
        /// </summary>
        /// <param name="SendChordMessage"></param>
        public void Send()
        {
            int currentMsPos = 0; ;
            foreach(KeyValuePair<int, List<ChannelMessage>> kvp in _messagesDict)
            {
                int delay = kvp.Key - currentMsPos;
                List<ChannelMessage> messages = kvp.Value;
                Thread.Sleep(delay);
                foreach(ChannelMessage message in messages)
                {
                    _midiOutputDevice.Send(message);
                }
                currentMsPos = kvp.Key;
            }
        }

        private Bank _bank = null;
        private PresetCommand _patch = null;
        private PitchWheelSensitivity _pitchWheelDeviation = null;
        private readonly List<BasicMidiChord> _basicMidiChords = new List<BasicMidiChord>();
        private MidiChordSlider _pitchWheelSlider = null;
        private MidiChordSlider _panSlider = null;
        private MidiChordSlider _modulationWheelSlider = null;
        private MidiChordSlider _expressionSlider = null;

        private readonly OutputDevice _midiOutputDevice = null;

        // msPosInChord, messageMoment
        private SortedDictionary<int, List<ChannelMessage>> _messagesDict;

    }
}
