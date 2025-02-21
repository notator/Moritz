using Sanford.Multimedia.Midi;

using System.Collections.Generic;

namespace Moritz.Midi
{
    /// <summary>
    /// MidiMessages are grouped into two types here: 
    ///   1. Switch: Bank, PresetCommand, PitchWheelSensitivity 
    ///	  2. Slider: PitchWheelCommand, ModulationWheel, Pan, Expression
    ///	See the comments in MidiSwitch.cs and MidiSlider.cs
    /// </summary>
    public abstract class MidiMessage
    {
        /// <summary>
        /// Used by MidiChordSlider
        /// </summary>
        protected MidiMessage()
        {
        }
        /// <summary>
        /// Used by PitchWheelDeviation controller, which must set both its coarse and fine controllers.
        /// </summary>
        protected MidiMessage(int channel, ControllerType coarseController, ControllerType fineController, int msb)
        {
            ChannelMessages.Add(new ChannelMessage(ChannelCommand.Controller, channel, (int)coarseController, msb));
            ChannelMessages.Add(new ChannelMessage(ChannelCommand.Controller, channel, (int)fineController, msb));
        }

        /// <summary>
        /// Used by controllers which only need to set their coarse controller.
        /// </summary>
        protected MidiMessage(int channel, ControllerType coarseController, int msb)
        {
            ChannelMessages.Add(new ChannelMessage(ChannelCommand.Controller, channel, (int)coarseController, msb));
        }

        /// <summary>
		/// Sets the control's ChannelMessage MSB. LSBs are not used.
		/// </summary>
		protected MidiMessage(int channel, ChannelCommand command, int value)
        {
            ChannelMessages.Add(new ChannelMessage(command, channel, value, 0));
        }
        /// <summary>
        /// Sets the control's ChannelMessage MSB and LSB.
        /// </summary>
        protected MidiMessage(int channel, ChannelCommand command, int msb, int lsb)
        {
            ChannelMessages.Add(new ChannelMessage(command, channel, msb, lsb));
        }

        public List<ChannelMessage> ChannelMessages = new List<ChannelMessage>();
    }
}
