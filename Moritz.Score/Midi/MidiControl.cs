using System;
using System.Collections.Generic;

using Multimedia.Midi;

namespace Moritz.Score.Midi
{
	public enum ControlContinuation
	{
		NoChange, // ' '
		StepChangeEqualPerStaffMoment, // '-'
		StepChangeEqualPerLogicalPosition, // '_'
		ContinuousChangeEqualPerStaffMoment, // '+'
		ContinuousChangeEqualPerLogicalPosition, // '~'
	}

	/// <summary>
	/// There are currently 4 kinds of MIDI controls:
	///   1. Command: No argument.
	///			e.g.: allSoundOff, Ped, PedUp, 3Ped, 3PedUp (3rd pedal down and up). 
	///   2. Switch: these take a byte argument which is used unchanged.
	///			e.g.: Bank Change, Patch Change, bank3, patch56, harpsichord, celesta etc.
	///			(harpsichord, celesta etc. instrument names are aliases for patch change commands.)
	///   3. Slider: these also take a byte argument. (See comment in MidiSlider.cs)
	///   4. Articulation: controls which change the MIDI "Expression" value in their own time.
	///			e.g.: accents, staccato, sf, fz, fp etc. If a chord has more than one Articulation, all but the first
    ///			are ignored.
	/// </summary>
	public abstract class MidiControl
	{
		/// <summary>
		/// Used by MoritzArticulations
		/// </summary>
		protected MidiControl()
		{
		}
        /// <summary>
        /// Used by PitchWheelDeviation controller, which must set both its coarse and fine controllers.
        /// </summary>
        protected MidiControl(int channel, ControllerType coarseController, ControllerType fineController, byte msb)
        {
            ChannelMessages.Add(BuildMessage(ChannelCommand.Controller, channel, (int)coarseController, msb));
            ChannelMessages.Add(BuildMessage(ChannelCommand.Controller, channel, (int)fineController, msb));
        }

        /// <summary>
        /// Used by controllers which only need to set their coarse controller.
        /// </summary>
        protected MidiControl(int channel, ControllerType coarseController, byte msb)
        {
            ChannelMessages.Add(BuildMessage(ChannelCommand.Controller, channel, (int)coarseController, msb));
        }

        /// <summary>
		/// Sets the control's ChannelMessage MSB. LSBs are not used.
		/// </summary>
		protected MidiControl(int channel, ChannelCommand command, byte value)
		{
            ChannelMessages.Add(BuildMessage(command, channel, value, 0));
		}
        /// <summary>
        /// Sets the control's ChannelMessage MSB and LSB.
        /// </summary>
        protected MidiControl(int channel, ChannelCommand command, byte msb, byte lsb)
        {
            ChannelMessages.Add(BuildMessage(command, channel, msb, lsb));
        }
        
        protected ChannelMessage BuildMessage(ChannelCommand command, int channel, int data1, int data2)
		{
			if(channel > 15 || channel < 0)
				throw new ApplicationException("Channel index must be in range 0..15");
			if(data1 > 127 || data1 < 0)
				throw new ApplicationException("Value must be in range 0..127");
			if(data2 > 127 || data2 < 0)
				throw new ApplicationException("Value must be in range 0..127");

			ChannelMessageBuilder builder = new ChannelMessageBuilder();
			builder.Command = command;
			builder.MidiChannel = channel;
			builder.Data1 = data1;
			builder.Data2 = data2;
			builder.Build();
            return builder.Result;
		}

		public List<ChannelMessage> ChannelMessages = new List<ChannelMessage>();
	}
}
