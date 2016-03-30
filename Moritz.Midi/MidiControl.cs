using System;
using System.Collections.Generic;

using Sanford.Multimedia.Midi;

namespace Moritz.Midi
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
        protected MidiControl(int channel, ControllerType coarseController, ControllerType fineController, int msb)
        {
            ChannelMessages.Add(new ChannelMessage(ChannelCommand.Controller, channel, (int)coarseController, msb));
            ChannelMessages.Add(new ChannelMessage(ChannelCommand.Controller, channel, (int)fineController, msb));
        }

        /// <summary>
        /// Used by controllers which only need to set their coarse controller.
        /// </summary>
        protected MidiControl(int channel, ControllerType coarseController, int msb)
        {
            ChannelMessages.Add(new ChannelMessage(ChannelCommand.Controller, channel, (int)coarseController, msb));
        }

        /// <summary>
		/// Sets the control's ChannelMessage MSB. LSBs are not used.
		/// </summary>
		protected MidiControl(int channel, ChannelCommand command, int value)
		{
            ChannelMessages.Add(new ChannelMessage(command, channel, value, 0));
		}
        /// <summary>
        /// Sets the control's ChannelMessage MSB and LSB.
        /// </summary>
        protected MidiControl(int channel, ChannelCommand command, int msb, int lsb)
        {
            ChannelMessages.Add(new ChannelMessage(command, channel, msb, lsb));
        }

		public List<ChannelMessage> ChannelMessages = new List<ChannelMessage>();
	}
}
