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
		public Seq(int seqMsPosition, List<Trk> trks, IReadOnlyList<int> midiChannelIndexPerOutputVoice)
		{
			_msPosition = seqMsPosition;
			foreach(int midiChannel in midiChannelIndexPerOutputVoice)
			{
				Trk trk = new Trk((byte)midiChannel, new List<IUniqueDef>());
				_trks.Add(trk);
			}
			MidiChannelIndexPerOutputVoice = midiChannelIndexPerOutputVoice;

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

			AssertConsistency();
		}

		/// <summary>
		/// A new Seq that is the concatenation of (deep)clones of the argument seqs.
		/// </summary>
		public Seq(Seq seqArg1, Seq seqArg2)
		{
			Seq seq1 = seqArg1.Clone();

			seq1.Concat(seqArg2);

			_msPosition = seq1.MsPosition;
			_trks = seq1.Trks;
			MidiChannelIndexPerOutputVoice = seq1.MidiChannelIndexPerOutputVoice; 
		}

		/// <summary>
		/// Concatenates a deep clone of seq2 to the caller (seq1). Returns a pointer to the caller.
		/// Seq2.MsPosition is the earliest position, relative to seq1, at which it can be concatenated.
		/// For example:
		/// If seq2.MsPosition==0, it will be concatenated such that there will be at least one trk concatenation without an
		/// intervening rest.
		/// If seq2.MsPosition == seq1.MsDuration, the seqs will be juxtaposed.
		/// If seq2.MsPosition > seq1.MsDuration, the seqs will be concatenated with an intervening rest.
		/// Redundant clef changes are silently removed.
		/// </summary>
		public Seq Concat(Seq seq2)
		{
			#region assertions
			Debug.Assert(MidiChannelIndexPerOutputVoice.Count == seq2.MidiChannelIndexPerOutputVoice.Count);
			for(int i = 0; i < MidiChannelIndexPerOutputVoice.Count; ++i)
			{
				Debug.Assert(MidiChannelIndexPerOutputVoice[i] == seq2.MidiChannelIndexPerOutputVoice[i]);
			}
			#endregion

			Seq seq2Clone = seq2.Clone();
			int nTrks = _trks.Count;
			int concatMsPos = seq2Clone.MsPosition;
			if(seq2Clone.MsPosition < MsDuration)
			{
				for(int i = 0; i < nTrks; ++i)
				{
					Trk trk1 = _trks[i];
					Trk trk2 = seq2Clone.Trks[i];
					int earliestConcatPos = trk1.EndMsPosition - trk2.MsPosition;
					concatMsPos = (earliestConcatPos > concatMsPos) ? earliestConcatPos : concatMsPos;
				}
			}
		
			for(int i = 0; i < nTrks; ++i)
			{
				Trk trk2 = seq2Clone.Trks[i];
				if(trk2.UniqueDefs.Count > 0)
				{
					Trk trk1 = Trks[i];
					int trk2StartMsPosition = concatMsPos + trk2.MsPosition;
					if(trk1.EndMsPosition < trk2StartMsPosition)
					{
						trk1.Add(new RestDef(trk1.EndMsPosition, trk2StartMsPosition - trk1.EndMsPosition));
					}
					trk1.AddRange(trk2);
				}
			}

			RemoveRedundantClefChanges();

			return this;
		}

		private void RemoveRedundantClefChanges()
		{

			int nTrks = _trks.Count;

			for(int i = 0; i < nTrks; ++i)
			{
				string currentClef = "";
				List<int> redundantClefIndices = new List<int>();
				Trk trk = _trks[i];
				string trkNumber = (i + 1).ToString();
				List<IUniqueDef> iuds = trk.UniqueDefs;
				if(iuds.Count > 0)
				{
					for(int iudIndex = 0; iudIndex < iuds.Count; ++iudIndex)
					{
						ClefChangeDef ccd = iuds[iudIndex] as ClefChangeDef;
						if(ccd != null)
						{
							if(ccd.ClefType == currentClef)
							{
								redundantClefIndices.Add(iudIndex);
							}
							currentClef = ccd.ClefType;
						}
					}
				}
				for(int j = redundantClefIndices.Count - 1; j >= 0; --j)
				{
					iuds.RemoveAt(redundantClefIndices[j]);
				}
			}
		}

		public Seq Clone()
		{
			List<Trk> trks = new List<Trk>();
			for(int i = 0; i < _trks.Count; ++i)
			{
				trks.Add(_trks[i].Clone());
			}

			Seq clone = new Seq(_msPosition, trks, MidiChannelIndexPerOutputVoice);

			return clone;
		}

		/// <summary>
		/// Every Trk in trks is either empty, or begins with a MidiChordDef or ClefChangeDef, and ends with a MidiChordDef.
		/// There is at least one Trk having MsPosition == 0.
		/// The trks list does not have to be complete, but each MidiChannel must be set to a valid value.
		/// </summary>
		private void AssertConsistency()
		{
			Debug.Assert(_msPosition >= 0);
			Debug.Assert(_trks != null && _trks.Count > 0);
			Debug.Assert(MidiChannelIndexPerOutputVoice != null && MidiChannelIndexPerOutputVoice.Count > 0);
			#region trks all start and end with a MidiChordDef
			bool okay = true;
			foreach(Trk trk in _trks)
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
			foreach(Trk trk in _trks)
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
			foreach(Trk trk in _trks)
			{
				bool midiChannelOkay = false;
				foreach(int midiChannel in MidiChannelIndexPerOutputVoice)
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
		public List<Trk> Trks { get { return _trks; } }

		private int _msPosition;
		public int MsPosition
		{	
			get	{ return _msPosition; }
			set
			{
				Debug.Assert(value >= 0);
				_msPosition = value;
			}
		}

		/// <summary>
		/// The duration between the beginning of the first UniqueDef in the Seq and the end of the last UniqueDef in the Seq.
		/// Setting this value stretches or compresses the msDurations of all the trks and their contained UniqueDefs.
		/// </summary>
		public int MsDuration
		{ 
			get
			{
				AssertConsistency();  // there is a trk that begins at msPosition==0.
				int msDuration = 0;
				foreach(Trk trk in _trks)
				{
					if(trk.UniqueDefs.Count > 0)
					{
						IUniqueDef lastIUD = trk.UniqueDefs[trk.UniqueDefs.Count - 1];
						int endMsPos = lastIUD.MsPosition + lastIUD.MsDuration;
						msDuration = (msDuration < endMsPos) ? endMsPos : msDuration;
					}
				}
				return msDuration;
			}
			set
			{
				Debug.Assert(_trks.Count > 0);
				// there is a trk that begins at msPosition==0.
				int currentDuration = MsDuration;
				double factor = ((double)value) / currentDuration;
				foreach(Trk trk in _trks)
				{
					trk.MsDuration = (int) Math.Round(trk.MsDuration * factor);
					trk.MsPosition = (int) Math.Round(trk.MsPosition * factor);
				}
				int roundingError = value - MsDuration;
				if(roundingError != 0)
				{
					foreach(Trk trk in _trks)
					{
						if((trk.EndMsPosition + roundingError) == value)
						{
							trk.EndMsPosition += roundingError;
						}
					}
				}
				Debug.Assert(MsDuration == value); 
			}
		}

		private IReadOnlyList<int> MidiChannelIndexPerOutputVoice;
	}
}
