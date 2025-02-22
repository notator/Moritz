using Sanford.Multimedia.Midi;

namespace Moritz.Midi
{
    /// <summary>
    /// SanfordMessages are eventually sent to the Microsoft GS Wavetable Synth.
    ///	A SanfordSwitch is a message that can only be sent once at the beginning of a chord.
    /// </summary>
    public class SanfordSwitch : SanfordMessage
    {
        protected SanfordSwitch(int channel, ControllerType controllerCoarse, ControllerType controllerFine, byte value)
            : base(channel, controllerCoarse, controllerFine, value)
        {
        }
        protected SanfordSwitch(int channel, ControllerType controller, byte value)
            : base(channel, controller, value)
        {
        }
        protected SanfordSwitch(int channel, ChannelCommand command, byte value)
            : base(channel, command, value)
        {
        }
    }

    public class SSBankMsg : SanfordSwitch
    {
        public SSBankMsg(int channel, byte value)
            : base(channel, ControllerType.BankSelect, value)
        {
            // ControllerType.BankSelectFine is ignored
        }
    }
    public class SSPresetMsg : SanfordSwitch
    {
        public SSPresetMsg(int channel, byte value)
            : base(channel, ChannelCommand.ProgramChange, value)
        {
        }
    }
    public class SSPitchWheelSensitivityMsg : SanfordSwitch
    {
        private class SSDataEntryCoarseMsg : SanfordSwitch
        {
            // Used only by PitchWheelSensitivity.
            public SSDataEntryCoarseMsg(int channel, byte value)
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
        public SSPitchWheelSensitivityMsg(int channel, byte value)
            : base(channel, ControllerType.RegisteredParameterCoarse, ControllerType.RegisteredParameterFine, 0)
        {
            SSDataEntryCoarseMsg valueData = new SSDataEntryCoarseMsg(channel, value);
            this.ChannelMessages.AddRange(valueData.ChannelMessages);
        }
    }
}
