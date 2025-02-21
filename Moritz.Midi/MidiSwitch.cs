using Sanford.Multimedia.Midi;

namespace Moritz.Midi
{
    /// <summary>
    ///	A MidiSwitch is a message that can only be sent once at the beginning of a chord.
    ///	Currently, these are all standard MIDI messages that are sent to the
    ///	Microsoft GS WavetableSynth from Moritz's palettes.
    ///	New, non-standard ResidentSynth MidiSwitch messages can now be
    ///	defined for use in scores, but it should be understood that these
    ///	will not work with the Microsoft GS Wavetable Synth and so cannot be
    ///	used in palettes.
    /// </summary>
    public class MidiSwitch : MidiMessage
    {
        protected MidiSwitch(int channel, ControllerType controllerCoarse, ControllerType controllerFine, byte value)
            : base(channel, controllerCoarse, controllerFine, value)
        {
        }
        protected MidiSwitch(int channel, ControllerType controller, byte value)
            : base(channel, controller, value)
        {
        }
        protected MidiSwitch(int channel, ChannelCommand command, byte value)
            : base(channel, command, value)
        {
        }
    }

    #region standard switches
    // These switches can be used in both the
    // Microsoft GS Wavetable Synth and the ResidentSynth
    // i.e. in palettes and scores.
    public class Bank : MidiSwitch
    {
        public Bank(int channel, byte value)
            : base(channel, ControllerType.BankSelect, value)
        {
            // ControllerType.BankSelectFine is ignored
        }
    }
    public class PresetCommand : MidiSwitch
    {
        public PresetCommand(int channel, byte value)
            : base(channel, ChannelCommand.ProgramChange, value)
        {
        }
    }
    public class PitchWheelSensitivity : MidiSwitch
    {
        private class DataEntryCoarse : MidiSwitch
        {
            // Used only by PitchWheelSensitivity.
            public DataEntryCoarse(int channel, byte value)
                : base(channel, ControllerType.DataEntrySlider, value)
            {
            }
        }
        /// <remarks>
        /// Sets both RegisteredParameter controls to 0 (zero). This is standard MIDI for selecting the
        /// pitch wheel so that it can be set by the subsequent DataEntry messages.
        /// A DataEntryFine message is not set in this constructor, because it is not needed, and has no effect anyway.
        /// However, RegisteredParameterFine MUST be set, otherwise the messages as a whole have no effect!
        /// </remarks>
        public PitchWheelSensitivity(int channel, byte value)
            : base(channel, ControllerType.RegisteredParameterCoarse, ControllerType.RegisteredParameterFine, 0)
        {
            DataEntryCoarse valueData = new DataEntryCoarse(channel, value);
            this.ChannelMessages.AddRange(valueData.ChannelMessages);
        }
    }
    #endregion
}
