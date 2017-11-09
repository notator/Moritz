
namespace Moritz.Spec
{
	/// <summary>
	/// This class is immutable.
	/// </summary>
	public class MsValues
	{
		public MsValues(int msPosition, int msDuration)
		{
			MsPosition = msPosition;
			MsDuration = msDuration;
		}

		public int MsPosition { get; }
		public int MsDuration { get; }
		public int EndMsPosition { get => MsPosition + MsDuration; }
	}
}
