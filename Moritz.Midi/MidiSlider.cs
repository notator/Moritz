using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;

using Multimedia.Midi;

namespace Moritz.Midi
{
	/// <summary>
	/// MidiSliders are MidiControls whose value can change gradually over time. In scores, each of these takes
	/// a byte argument (in the range 0..127) denoting the MSB values for the particular slider.
	/// Continuation characters: In addition to the msb argument, each slider text may end with a
	/// character which shows if and how the value subequently changes (until the next instance of the same
	/// slider:
	///   a) '-' the slider's value changes once per chord, in equal steps per chord
	///   b) '_' the slider's value changes once per chord, in equal steps per logical position
	///   c) '+' the slider's value changes continuously, in equal steps per chord
	///   d) '~' the slider's value changes continuously, in equal steps per logical position
	///   Examples:
	///         pan50~   pan23   pan80-  pan20
	///	The initial Instrument and Volume settings come from the capella file.
	///	All Sliders have a default value of 0, except
	///			volume 127 (alias v, vol)
	///			expression 64 (alias e, exp, expr)
	///			balance 64 (alias b, bal)
	///			pan 64 (alias p)
	///			pitchwheel 64 (alias pw)
	///	The default value is set when the control has no numeric argument. 
	/// </summary>
	public abstract class MidiSlider : MidiControl
	{
        protected MidiSlider(int channel, ChannelCommand command, byte msb, ControlContinuation continuation)
            : base(channel, command, msb)
        {
        }
        protected MidiSlider(int channel, ChannelCommand command, byte msb, byte lsb, ControlContinuation continuation)
            : base(channel, command, msb, lsb)
        {
        }
        protected MidiSlider(int channel, ControllerType controller, byte msb, ControlContinuation continuation)
			: base(channel, controller, msb)
		{
		}
	}

	#region long (2-byte) sliders
	public class PitchWheel : MidiSlider
	{
        /// <summary>
        /// Achtung: this slider needs its lsb to be set!
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="lsb"></param>
        /// <param name="continuation"></param>
        public PitchWheel(int channel, byte lsb, ControlContinuation continuation)
            : base(channel, ChannelCommand.PitchWheel, 0, lsb, continuation)
        {
        }
    }
	public class ModulationWheel : MidiSlider
	{
		public ModulationWheel(int channel, byte msb, ControlContinuation continuation)
			: base(channel, ControllerType.ModulationWheel, msb, continuation)
		{
		}
	}
	public class BreathController : MidiSlider
	{
		public BreathController(int channel, byte msb, ControlContinuation continuation)
			: base(channel, ControllerType.BreathControl, msb, continuation)
		{
		}
	}
	public class FootPedal : MidiSlider
	{
        public FootPedal(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.FootPedal, msb, continuation)
        {
        }
    }
	public class PortamentoTime : MidiSlider
	{
        public PortamentoTime(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.PortamentoTime, msb, continuation)
        {
        }
    }
	public class Volume : MidiSlider
	{
        public Volume(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.Volume, msb, continuation)
        {
            //  ControllerType.VolumeFine is ignored
        }
    }
	public class Balance : MidiSlider
	{
        public Balance(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.Balance, msb, continuation)
        {
             //  ControllerType.BalanceFine is ignored.
        }
    }
	public class Pan : MidiSlider
	{
        public Pan(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.Pan, msb, continuation)
        {
            //  ControllerType.PanFine is ignored.
        }
    }
	public class Expression : MidiSlider
	{
        public Expression(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.Expression, msb, continuation)
        {
            //  ControllerType.ExpressionFine is ignored
        }
    }
	public class EffectControl1 : MidiSlider
	{
        public EffectControl1(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.EffectControl1, msb, continuation)
        {
        }
    }
	public class EffectControl2 : MidiSlider
	{
        public EffectControl2(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.EffectControl2, msb, continuation)
        {
        }
    }
	#endregion long (2-byte) sliders
	#region short (1-byte) sliders
	public class ChannelPressure : MidiSlider
	{
        public ChannelPressure(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ChannelCommand.ChannelPressure, msb, continuation)
        {
        }
    }
	public class GeneralPurposeSlider1 : MidiSlider
	{
        public GeneralPurposeSlider1(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.GeneralPurposeSlider1, msb, continuation)
        {
        }
    }
	public class GeneralPurposeSlider2 : MidiSlider
	{
        public GeneralPurposeSlider2(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.GeneralPurposeSlider2, msb, continuation)
        {
        }
    }
	public class GeneralPurposeSlider3 : MidiSlider
	{
        public GeneralPurposeSlider3(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.GeneralPurposeSlider3, msb, continuation)
        {
        }
    }
	public class GeneralPurposeSlider4 : MidiSlider
	{
        public GeneralPurposeSlider4(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.GeneralPurposeSlider4, msb, continuation)
        {
        }
    }
	public class SoundVariation : MidiSlider
	{
        public SoundVariation(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundVariation, msb, continuation)
        {
        }
    }
	public class SoundTimbre : MidiSlider
	{
        public SoundTimbre(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundTimbre, msb, continuation)
        {
        }

	}
	public class SoundReleaseTime : MidiSlider
	{
        public SoundReleaseTime(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundReleaseTime, msb, continuation)
        {
        }
    }
	public class SoundAttackTime : MidiSlider
	{
        public SoundAttackTime(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundAttackTime, msb, continuation)
        {
        }
    }
	public class SoundBrightness : MidiSlider
	{
        public SoundBrightness(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundBrightness, msb, continuation)
        {
        }
    }
	public class SoundControl6 : MidiSlider
	{
        public SoundControl6(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundControl6, msb, continuation)
        {
        }
    }
	public class SoundControl7 : MidiSlider
	{
        public SoundControl7(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundControl7, msb, continuation)
        {
        }
    }
	public class SoundControl8 : MidiSlider
	{
        public SoundControl8(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundControl8, msb, continuation)
        {
        }
    }
	public class SoundControl9 : MidiSlider
	{
        public SoundControl9(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundControl9, msb, continuation)
        {
        }
    }
	public class SoundControl10 : MidiSlider
	{
        public SoundControl10(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.SoundControl10, msb, continuation)
        {
        }
    }
	public class EffectsLevel : MidiSlider
	{
        public EffectsLevel(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.EffectsLevel, msb, continuation)
        {
        }
    }
	public class TremoloLevel : MidiSlider
	{
        public TremoloLevel(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.TremeloLevel, msb, continuation)
        {
        }
    }
	public class ChorusLevel : MidiSlider
	{
        public ChorusLevel(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.ChorusLevel, msb, continuation)
        {
        }
    }
	public class CelesteLevel : MidiSlider
	{
        public CelesteLevel(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.CelesteLevel, msb, continuation)
        {
        }
    }
    public class PhaserLevel : MidiSlider
    {
        public PhaserLevel(int channel, byte msb, ControlContinuation continuation)
            : base(channel, ControllerType.PhaserLevel, msb, continuation)
        {
        }
    }
    #endregion short (1-byte) sliders
}
