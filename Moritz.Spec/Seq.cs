using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Collections.ObjectModel;

namespace Moritz.Spec
{
	public class Seq
	{
		/// <summary>
		/// <para>Every Trk in trks is either empty or begins and ends with a MidiChordDef.</para>
		/// <para>Trk msPositions are relative to the start of the Seq.</para>
		/// <para>There is at least one non-empty Trk having MsPosition = 0.</para>
		/// <para>The trks list does not have to be complete, but each tkr.MidiChannel must be
		/// unique and present in the midiChannelIndexPerOutputVoice list.</para>
		/// </summary>
		public Seq(int seqMsPosition, List<Trk> trks, List<int> midiChannelIndexPerOutputVoice)
		{
			AssertConsistency(seqMsPosition, trks, midiChannelIndexPerOutputVoice);

			_msPosition = seqMsPosition;

			foreach(int midiChannel in midiChannelIndexPerOutputVoice)
			{
				Trk trk = new Trk((byte)midiChannel, new List<IUniqueDef>());
				_trks.Add(trk);
			}

			bool success = false;
			foreach(Trk trk in trks)
			{
				for(int i = 0; i < _trks.Count; ++i)
				{
					if(trk.MidiChannel == _trks[i].MidiChannel)
					{
						if(_trks[i].UniqueDefs.Count > 0)
						{
							Debug.Assert(false, "Duplicate midiChannel in trks.");
						}
						_trks[i] = trk;
						success = true;
						break;
					}
				}
			}
			Debug.Assert(success == true);
		}

		/// <summary>
		/// Every Trk in trks is either empty, or begins with a MidiChordDef or ClefChangeDef, and ends with a MidiChordDef.
		/// There is at least one Trk having MsPosition == 0.
		/// The trks list does not have to be complete, but each MidiChannel must be set to a valid value.
		/// </summary>
		public void AssertConsistency(int seqMsPosition, List<Trk> trks, List<int> midiChannelIndexPerOutputVoice)
		{
			Debug.Assert(seqMsPosition >= 0);
			Debug.Assert(trks != null && trks.Count > 0);
			Debug.Assert(midiChannelIndexPerOutputVoice != null && midiChannelIndexPerOutputVoice.Count > 0);
			#region trks all start and end with a MidiChordDef
			bool okay = true;
			foreach(Trk trk in trks)
			{
				if(trk.UniqueDefs.Count > 0)
				{
					IUniqueDef firstIUD = trk.UniqueDefs[0];
					IUniqueDef lastIUD = trk.UniqueDefs[trk.UniqueDefs.Count - 1];
					if(!((firstIUD is MidiChordDef || firstIUD is ClefChangeDef) && lastIUD is MidiChordDef))
					{
						okay = false;
						break;
					}
				}
			}
			Debug.Assert(okay == true, "All non-empty trks must begin with a MidiChordDef or ClefChangeDef, and end with a MidiChordDef");
			#endregion
			#region position of earliest IUniqueDef
			bool found = false;
			foreach(Trk trk in trks)
			{
				if(trk.UniqueDefs.Count > 0 && trk.MsPosition == 0)
				{
					found = true;
					break;
				}
			}
			Debug.Assert(found == true, "The first UniqueDef in at least one trk must have MsPosition=0");
			#endregion
			#region all trks must have a valid midiChannel		
			foreach(Trk trk in trks)
			{
				bool midiChannelOkay = false;
				foreach(int midiChannel in midiChannelIndexPerOutputVoice)
				{
					if(midiChannel == trk.MidiChannel)
					{
						midiChannelOkay = true;
						break;
					}
				}
				Debug.Assert(midiChannelOkay == true, "All trks must have a valid midiChannel");
			}
			#endregion
		}

		private List<Trk> _trks = new List<Trk>();
		public IReadOnlyList<Trk> Trks
		{
			get { return _trks.AsReadOnly(); }
		}

		public int MsPosition
		{
			get	{ return _msPosition; }
			set
			{
				Debug.Assert(value >= 0);
				_msPosition = value;
			}
		}

		private int _msPosition;
	}
}
