using Moritz.Globals;

using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Spec
{
	/// <summary>
	/// Created when a note or rest straddles a barline.
	/// This class is created while splitting systems.
	/// It is used when notating them. this class has no content!
	/// </summary>
	public class CautionaryChordDef : IUniqueChordDef
	{
		public CautionaryChordDef(IUniqueChordDef uniqueChordDef, int msPositionReFirstIUD, int msDuration)
		{
			Pitches = uniqueChordDef.Pitches;
			if(uniqueChordDef is MidiChordDef midiChordDef)
			{
				Velocities = midiChordDef.Velocities;
			}

			MsPositionReFirstUD = msPositionReFirstIUD;
			MsDuration = msDuration;
		}

		#region IUniqueChordDef
		/// <summary>
		/// Cautionary chords should never be transposed. They always have the same pitches as the chord from which they are constructed.
		/// </summary>
		public void Transpose(int interval) { }

		/// <summary>
		/// This Pitches field is used when displaying the chord's noteheads.
		/// </summary>
		public List<int> Pitches
		{
			get { return _pitches; }
			set
			{
				foreach(int pitch in value)
				{
					M.Assert(pitch == M.MidiValue(pitch));
				}
				_pitches = new List<int>(value);
			}
		}
		private List<int> _pitches = null;

		/// <summary>
		/// This Velocities field may be used when displaying the chord's noteheads.
		/// </summary>
		public List<int> Velocities
		{
			get
			{
				return _velocities;
			}
			set
			{
				foreach(int velocity in value)
				{
					M.Assert(velocity == M.MidiValue(velocity));
				}
				_velocities = new List<int>(value);
			}
		}
		private List<int> _velocities = null;

		#region IUniqueDef
		public override string ToString()
		{
			return ("MsPositionReFirstIUD=" + MsPositionReFirstUD.ToString() + " MsDuration=" + MsDuration.ToString() + " CautionaryChordDef");
		}

		public object Clone()
		{
			CautionaryChordDef deepClone = new CautionaryChordDef(this, this._msPositionReFirstIUD, this._msDuration);
			return deepClone;
		}

		public int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
		private int _msDuration = 0;
		public int MsPositionReFirstUD { get { return _msPositionReFirstIUD; } set { _msPositionReFirstIUD = value; } }
		private int _msPositionReFirstIUD = 0;
		#endregion IUniqueDef
		#endregion IUniqueChordDef

	}
}
