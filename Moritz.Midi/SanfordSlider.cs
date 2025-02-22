using Sanford.Multimedia.Midi;

namespace Moritz.Midi
{
    /// <summary>
    /// SanfordMessages are eventually sent to the Microsoft GS Wavetable Synth.
    ///	A SanfordSlider is a message that can be sent multiple times inside a chord.
    /// </summary>
    public abstract class SanfordSlider : SanfordMessage
    {
        protected SanfordSlider(int channel, ChannelCommand command, int msb, int lsb)
            : base(channel, command, msb, lsb)
        {
        }
        protected SanfordSlider(int channel, ControllerType controller, int msb)
            : base(channel, controller, msb)
        {
        }
    }

    public class SSPitchWheelMsg : SanfordSlider
    {
        /// <summary>
        /// Achtung: this slider needs its lsb to be set!
        /// </summary>
        public SSPitchWheelMsg(int channel, int lsb)
            : base(channel, ChannelCommand.PitchWheel, lsb, lsb)
        {
        }
    }
    public class SSModulationWheelMsg : SanfordSlider
    {
        public SSModulationWheelMsg(int channel, int msb)
            : base(channel, ControllerType.ModulationWheel, msb)
        {
        }
    }
    public class SSPanMsg : SanfordSlider
    {
        public SSPanMsg(int channel, int msb)
            : base(channel, ControllerType.Pan, msb)
        {
            //  ControllerType.PanFine is ignored.
        }
    }
    public class SSExpressionMsg : SanfordSlider
    {
        public SSExpressionMsg(int channel, int msb)
            : base(channel, ControllerType.Expression, msb)
        {
            //  ControllerType.ExpressionFine is ignored
        }
    }
}
