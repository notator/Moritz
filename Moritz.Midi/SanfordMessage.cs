using Sanford.Multimedia.Midi;

using System.Collections.Generic;

namespace Moritz.Midi
{
    /// <summary>
    /// This class is the interface to the Microsoft GS Wavetable Synth (the system synthesizer),
    /// so it can only use MIDI messages with their original, standard meaning. 
    /// Each SanfordMessage converts its arguments to a list of Sanford.Multimedia.Midi.ChannelMessage objects
    /// that are later sent by the Sanford.Multimedia.Midi dll to the system synth.
    /// Moritz uses them for demonstration purposes while editing Palettes. They are never stored in scores.
    /// There are two sub-types defined for use in Moritz' Palettes: 
    ///   1. SanfordSwitch: Bank, SSPresetMsg, PitchWheelSensitivity 
    ///	  2. SanfordSlider: PitchWheelCommand, ModulationWheel, Pan, Expression
    ///	See the comments in SanfordSwitch.cs and SanfordSlider.cs
    /// </summary>
    public abstract class SanfordMessage
    {
        /// <summary>
        /// Used by MidiChordSlider
        /// </summary>
        protected SanfordMessage()
        {
        }
        /// <summary>
        /// Used by PitchWheelDeviation controller, which must set both its coarse and fine controllers.
        /// </summary>
        protected SanfordMessage(int channel, ControllerType coarseController, ControllerType fineController, int msb)
        {
            ChannelMessages.Add(new ChannelMessage(ChannelCommand.Controller, channel, (int)coarseController, msb));
            ChannelMessages.Add(new ChannelMessage(ChannelCommand.Controller, channel, (int)fineController, msb));
        }

        /// <summary>
        /// Used by controllers which only need to set their coarse controller.
        /// </summary>
        protected SanfordMessage(int channel, ControllerType coarseController, int msb)
        {
            ChannelMessages.Add(new ChannelMessage(ChannelCommand.Controller, channel, (int)coarseController, msb));
        }

        /// <summary>
		/// Sets the control's ChannelMessage MSB. LSBs are not used.
		/// </summary>
		protected SanfordMessage(int channel, ChannelCommand command, int value)
        {
            ChannelMessages.Add(new ChannelMessage(command, channel, value, 0));
        }
        /// <summary>
        /// Sets the control's ChannelMessage MSB and LSB.
        /// </summary>
        protected SanfordMessage(int channel, ChannelCommand command, int msb, int lsb)
        {
            ChannelMessages.Add(new ChannelMessage(command, channel, msb, lsb));
        }

        public List<ChannelMessage> ChannelMessages = new List<ChannelMessage>();
    }
}
