using Multimedia.Midi;

namespace Moritz.Midi
{
    public class MidiSwitch : MidiControl
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

    public class BankControl : MidiSwitch
	{
		public BankControl(int channel, byte value)
            : base(channel, ControllerType.BankSelect, value)
        {
            // ControllerType.BankSelectFine is ignored
		}
	}
    public class PatchControl : MidiSwitch
	{
		public PatchControl(int channel, byte value)
			: base(channel, ChannelCommand.ProgramChange, value)
		{
		}
	}
    public class PitchWheelDeviation : MidiSwitch
    {
        /// <remarks>
        /// Sets both RegisteredParameter controls to 0 (zero). This is standard MIDI for selecting the
        /// pitch wheel so that it can be set by the subsequent DataEntry messages.
        /// A DataEntryFine message is not set in this constructor, because it is not needed, and has no effect anyway.
        /// However, RegisteredParameterFine MUST be set, otherwise the messages as a whole have no effect!
        /// </remarks>
        public PitchWheelDeviation(int channel, byte value)
            : base(channel, ControllerType.RegisteredParameterCoarse, ControllerType.RegisteredParameterFine, 0)
        {
            DataEntryCoarse valueData = new DataEntryCoarse(channel, value);
            this.ChannelMessages.AddRange(valueData.ChannelMessages);
        }
    }

    public class DataEntryCoarse : MidiSwitch
    {
        // Not used directly by an AssistantPerformer script, but by PitchWheelDeviation (above).
        public DataEntryCoarse(int channel, byte value)
            : base(channel, ControllerType.DataEntrySlider, value)
        {
        }
    }
}
