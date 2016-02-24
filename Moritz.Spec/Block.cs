using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Collections.ObjectModel;

namespace Moritz.Spec
{
	public class Block : Seq
	{
		/// <summary>
		/// This constructor converts its argument to a Block so, if the argument needs to be preserved, pass seq.Clone().
		/// <para>A Block is a Seq whose Trks are padded at the beginning and end with rests so that they all start at
		/// the MsPosition=0, and have the same MsDuration.</para>
		/// <para>The Block's MsPosition is set to the seqClone's MsPosition.</para>
		/// <para>There is at least one MidiChordDef at the start of the Block, and at least one MidiChordDef ends at its end.</para>
		/// <para>If the original seqClone.trk.UniqueDefs list is empty or contains a single restDef, the corresponding
		/// voiceDef will contain a single rest having the same duration as the other trks.</para>
		/// </summary>
		public Block(Seq seqClone)
		 : base(seqClone.MsPosition, seqClone.Trks, seqClone.MidiChannelIndexPerOutputVoice)
		{
			List<VoiceDef> voiceDefs = new List<VoiceDef>();

			int msDuration = MsDuration; // MsDuration is a property that looks at UniqueDefs in this seq. 

			foreach(Trk trk in Trks)
			{
				Trk voiceDef = new Trk(trk.MidiChannel, trk.UniqueDefs); // this is not a clone...

				if(voiceDef.UniqueDefs.Count > 0)
				{
					IUniqueDef firstIUD = trk.UniqueDefs[0];
					int startRestMsDuration = firstIUD.MsPosition;
					if(startRestMsDuration > 0)
					{
						voiceDef.UniqueDefs.Insert(0, new RestDef(0, startRestMsDuration));
					}

					int endOfTrkMsPosition = trk.EndMsPosition;
					int endRestMsDuration = msDuration - endOfTrkMsPosition;
					if(endRestMsDuration > 0)
					{
						voiceDef.UniqueDefs.Add(new RestDef(endOfTrkMsPosition, endRestMsDuration));
					}
					voiceDef.AgglomerateRests();
				}
				else
				{
					voiceDef.UniqueDefs.Add(new RestDef(0, msDuration));
				}

				voiceDefs.Add(voiceDef);
			}

			for(int i = 0; i < voiceDefs.Count; ++i)
			{
				Trks[i] = (Trk)voiceDefs[i];
			}

			AssertBlockConsistency();
		}

		/// <summary>
		/// A Seq is a Block if it fulfills the following criteria:
		/// The Trks may contain any combination of RestDef, MidiChordDef and ClefChangeDef, except that:
		/// <para>1. trks all start at MsPosition=0 and have the same MsDuration.</para>
		/// <para>2. A restDef is never followed by another RestDef (RestDefs are agglomerated).</para>
		/// <para>3. No voiceDef may have a ClefChangeDef before the first MidiChordDef.</para>
		/// <para>General Seqs can be converted to sequences by calling seq.ToSequence().</para>
		/// </summary>
		private void AssertBlockConsistency()
		{
			#region All voiceDefs must begin at MsPosition=0 and have the same MsDuration
			int sequenceMsDuration = MsDuration;
			foreach(Trk trk in Trks)
			{
				if(trk.UniqueDefs.Count > 0)
				{
					IUniqueDef firstIUD = trk.UniqueDefs[0];
					Debug.Assert((firstIUD.MsPosition == 0 && trk.MsDuration == sequenceMsDuration),
						"All voiceDefs in a block must begin at MsPosition=0 and have the same MsDuration.");
				}
			}
			#endregion
			#region There is a MidiChordDef at the beginning and end of the Sequence
			bool foundStartMidiChordDef = false;
			bool foundEndMidiChordDef = false;
			foreach(Trk trk in Trks)
			{
				if(trk.UniqueDefs.Count > 0)
				{
					IUniqueDef firstIUD = trk.UniqueDefs[0];
					if(firstIUD is MidiChordDef)
					{
						foundStartMidiChordDef = true;
					}
					IUniqueDef lastIUD = trk.UniqueDefs[trk.UniqueDefs.Count - 1];
					if(lastIUD is MidiChordDef && trk.MsDuration == sequenceMsDuration)
					{
						foundEndMidiChordDef = true;
					}
				}
			}
			Debug.Assert((foundStartMidiChordDef == true && foundEndMidiChordDef == true),
						"A sequence must begin and end with at least one MidiChordDef.");
			#endregion
		}

		/// <summary>
		/// A Block may not have a ClefChangeDef before the first MidiChordDef if the Block is at MsPosition 0.
		/// </summary>
		private void AssertBlockClefChangeConsistency()
		{
			if(MsPosition == 0)
			{
				foreach(Trk trk in Trks)
				{
					foreach(IUniqueDef iud in trk.UniqueDefs)
					{

						if(iud is MidiChordDef || iud is ClefChangeDef)
						{
							Debug.Assert((iud is MidiChordDef),
								"A Block may not have a ClefChangeDef before the first MidiChordDef if the Block is at MsPosition 0.");
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// The argument warp is a list of doubles, in ascending order, beginning with 0 and ending with 1.
		/// The doubles represent moments in the original duration that will be separated from each other
		/// by equal durations when the function returns. The MsDuration of the Seq is not changed.
		/// </summary>
		public void WarpDurations(List<double> warp)
		{
			AssertBlockConsistency();
			int sequenceMsDuration = MsDuration;
			foreach(Trk trk in Trks)
			{
				trk.WarpDurations(warp);
			}
			Debug.Assert(sequenceMsDuration == MsDuration);
			AssertBlockConsistency();
		}

		public List<VoiceDef> VoiceDefs
		{
			get
			{
				AssertBlockClefChangeConsistency();
				List<VoiceDef> voiceDefs = new List<VoiceDef>();
				foreach(Trk trk in Trks)
				{
					voiceDefs.Add((VoiceDef)trk);
				}
				return voiceDefs;
			}
		}
	}
}
