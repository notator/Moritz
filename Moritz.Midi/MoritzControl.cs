using System;

namespace Moritz.Midi
{
	/// <summary>
	/// This class could be extended to provide "function pointers" to functions which would be called when the
	/// chord is executed.
	/// </summary>
	public abstract class MoritzControl
	{
		protected MoritzControl()
        { 
        }
	}

    public abstract class MoritzIntControl : MoritzControl
	{
		protected MoritzIntControl(int value)
		{
			IntegerValue = value;
		}
		public int IntegerValue;
	}

    public class MoritzOrnament : MoritzIntControl
	{
		public MoritzOrnament(int value)
            : base(value)
		{
            if(value < 1)
                throw new ApplicationException("Attempt to create a MoritzOrnament with a value less than 1.");
		}
	}

    public class MoritzKeyboardNumberControl : MoritzIntControl
	{
        public MoritzKeyboardNumberControl(int value)
            : base(value)
		{
            if(value < 1)
                throw new ApplicationException("Attempt to create a MoritzKeyboardNumberControl with a value less than 1.");
		}
	}

}
