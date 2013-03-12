
using Multimedia.Midi;

namespace Moritz.Score.Midi
{
    public abstract class MidiCommand : MidiControl
    {
        protected MidiCommand(int channel, ControllerType controller, byte value)
            : base(channel, controller, value)
        {
        }
    }
    /// <summary>
    /// If a MidiOffControl is attached to the right of its durationSymbol, its messages are put in the
    /// durationSymbol's OffMessages, otherwise it is put in the durationSymbol's list of MoritzControls.
    /// </summary>
    public abstract class MidiOffControl : MidiCommand
    {
        protected MidiOffControl(int channel, ControllerType controller, byte value, float xPos)
            : base(channel, controller, value)
        {
            if(xPos > 0.0)
                OffControl = true;
        }
        public bool OffControl = false;
    }

    public class PedalOn : MidiCommand
	{
		public PedalOn(int channel)
			: base(channel, ControllerType.HoldPedal1, 127)
		{
		}
	}
	public class PedalOff : MidiOffControl
	{
		public PedalOff(int channel, float xPos)
			: base(channel, ControllerType.HoldPedal1, 0, xPos)
		{
		}
	}
    public class PortamentoOn : MidiCommand
	{
		public PortamentoOn(int channel)
			: base(channel, ControllerType.Portamento, 127)
		{
		}
	}
	public class PortamentoOff : MidiOffControl
	{
		public PortamentoOff(int channel, float xPos)
			: base(channel, ControllerType.Portamento, 0, xPos)
		{
		}
	}
    public class ThirdPedalOn : MidiCommand
	{
		public ThirdPedalOn(int channel)
			: base(channel, ControllerType.SustenutoPedal, 127)
		{
		}
	}
	public class ThirdPedalOff : MidiOffControl
	{
		public ThirdPedalOff(int channel, float xPos)
			: base(channel, ControllerType.SustenutoPedal, 0, xPos)
		{
		}
	}
    public class SoftPedalOn : MidiCommand
	{
		public SoftPedalOn(int channel)
			: base(channel, ControllerType.SoftPedal, 127)
		{
		}
	}
	public class SoftPedalOff : MidiOffControl
	{
		public SoftPedalOff(int channel, float xPos)
			: base(channel, ControllerType.SoftPedal, 0, xPos)
		{
		}
	}
	public class LegatoPedalOn : MidiControl
	{
		public LegatoPedalOn(int channel)
			: base(channel, ControllerType.LegatoPedal, 127)
		{
		}
	}
	public class LegatoPedalOff : MidiOffControl
	{
		public LegatoPedalOff(int channel, float xPos)
			: base(channel, ControllerType.LegatoPedal, 0, xPos)
		{
		}
	}
	public class FadePedalOn : MidiControl
	{
		public FadePedalOn(int channel)
			: base(channel, ControllerType.HoldPedal2, 127)
		{
		}
	}
	public class FadePedalOff : MidiOffControl
	{
		public FadePedalOff(int channel, float xPos)
			: base(channel, ControllerType.HoldPedal2, 0, xPos)
		{
		}
	}
	public class GeneralPurposeButton1On : MidiControl
	{
		public GeneralPurposeButton1On(int channel)
			: base(channel, ControllerType.GeneralPurposeButton1, 127)
		{
		}
	}
	public class GeneralPurposeButton1Off : MidiOffControl
	{
		public GeneralPurposeButton1Off(int channel, float xPos)
			: base(channel, ControllerType.GeneralPurposeButton1, 0, xPos)
		{
		}
	}
	public class GeneralPurposeButton2On : MidiControl
	{
		public GeneralPurposeButton2On(int channel)
			: base(channel, ControllerType.GeneralPurposeButton2, 127)
		{
		}
	}
	public class GeneralPurposeButton2Off : MidiOffControl
	{
		public GeneralPurposeButton2Off(int channel, float xPos)
			: base(channel, ControllerType.GeneralPurposeButton2, 0, xPos)
		{
		}
	}
	public class GeneralPurposeButton3On : MidiControl
	{
		public GeneralPurposeButton3On(int channel)
			: base(channel, ControllerType.GeneralPurposeButton3, 127)
		{
		}
	}
	public class GeneralPurposeButton3Off : MidiOffControl
	{
		public GeneralPurposeButton3Off(int channel, float xPos)
			: base(channel, ControllerType.GeneralPurposeButton3, 0, xPos)
		{
		}
	}
	public class GeneralPurposeButton4On : MidiControl
	{
		public GeneralPurposeButton4On(int channel)
			: base(channel, ControllerType.GeneralPurposeButton4, 127)
		{
		}
	}
	public class GeneralPurposeButton4Off : MidiOffControl
	{
		public GeneralPurposeButton4Off(int channel, float xPos)
			: base(channel, ControllerType.GeneralPurposeButton4, 0, xPos)
		{
		}
	}
	public class DataButtonIncrement : MidiControl
	{
		public DataButtonIncrement(int channel)
			: base(channel, ControllerType.DataButtonIncrement, 0)
		{
		}
	}
	public class DataButtonDecrement : MidiControl
	{
		public DataButtonDecrement(int channel)
			: base(channel, ControllerType.DataButtonDecrement, 0)
		{
		}
	}
	public class AllSoundOff : MidiControl
	{
		public AllSoundOff(int channel)
			: base(channel, ControllerType.AllSoundOff, 0)
		{
		}
	}
	public class AllControllersOff : MidiControl
	{
		public AllControllersOff(int channel)
			: base(channel, ControllerType.AllControllersOff, 0)
		{
		}
	}
	public class LocalKeyboardOn : MidiControl
	{
		public LocalKeyboardOn(int channel)
			: base(channel, ControllerType.LocalKeyboard, 127)
		{
		}
	}
	public class LocalKeyboardOff : MidiOffControl
	{
		public LocalKeyboardOff(int channel, float xPos)
			: base(channel, ControllerType.LocalKeyboard, 0, xPos)
		{
		}
	}
	public class AllNotesOff : MidiControl
	{
		public AllNotesOff(int channel)
			: base(channel, ControllerType.AllNotesOff, 0)
		{
		}
	}
	public class OmniModeOn : MidiControl
	{
		public OmniModeOn(int channel)
			: base(channel, ControllerType.OmniModeOn, 0)
		{
		}
	}
	public class OmniModeOff : MidiControl
	{
		public OmniModeOff(int channel)
			: base(channel, ControllerType.OmniModeOff, 0)
		{
		}
	}
	// MonoOperation is a switch (it takes an argument from 1-16)
	public class PolyOperation : MidiControl
	{
		public PolyOperation(int channel)
			: base(channel, ControllerType.PolyOperation, 0)
		{
		}
	}
}
