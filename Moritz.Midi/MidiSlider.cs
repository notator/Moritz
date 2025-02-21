using Sanford.Multimedia.Midi;

namespace Moritz.Midi
{
    /// <summary>
    ///	A MidiSlider is a message that can be sent multiple times inside a chord.
    ///	Currently, these are all standard MIDI messages that are sent to the
    ///	Microsoft GS WavetableSynth from Moritz's palettes.
    ///	New, non-standard ResidentSynth MidiSlider messages can now be
    ///	defined for use in scores, but it should be understood that these
    ///	will not work with the Microsoft GS Wavetable Synth and so cannot be
    ///	used in palettes.
    /// </summary>
    public abstract class MidiSlider : MidiMessage
    {
        protected MidiSlider(int channel, ChannelCommand command, int msb, int lsb)
            : base(channel, command, msb, lsb)
        {
        }
        protected MidiSlider(int channel, ControllerType controller, int msb)
            : base(channel, controller, msb)
        {
        }
    }

    #region standard sliders
    // These sliders can be used in both the
    // Microsoft GS Wavetable Synth and the ResidentSynth
    // i.e. in palettes and scores.
    public class PitchWheelCommand : MidiSlider
    {
        /// <summary>
        /// Achtung: this slider needs its lsb to be set!
        /// </summary>
        public PitchWheelCommand(int channel, int lsb)
            : base(channel, ChannelCommand.PitchWheel, lsb, lsb)
        {
        }
    }
    public class ModulationWheel : MidiSlider
    {
        public ModulationWheel(int channel, int msb)
            : base(channel, ControllerType.ModulationWheel, msb)
        {
        }
    }
    public class Pan : MidiSlider
    {
        public Pan(int channel, int msb)
            : base(channel, ControllerType.Pan, msb)
        {
            //  ControllerType.PanFine is ignored.
        }
    }
    public class Expression : MidiSlider
    {
        public Expression(int channel, int msb)
            : base(channel, ControllerType.Expression, msb)
        {
            //  ControllerType.ExpressionFine is ignored
        }
    }
    #endregion
}
