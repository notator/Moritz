using Moritz.Spec;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Moritz.Symbols
{
	public class ScoreCoordinates
	{
		/// <summary>
		/// Saves barIndex, trkIndex and uniqueDefIndex in the corresponding members
		/// and saves the msPosition of the corresponding UniqueDef in this.msPos.
		/// </summary>
		/// <param name="bars"></param>
		/// <param name="barIndex"></param>
		/// <param name="trkIndex"></param>
		/// <param name="uniqueDefIndex">This index currently counts only MidiChordDefs and MidiRestDefs in the Trk</param>
		public ScoreCoordinates(List<Bar> bars, int barIndex, int trkIndex, int uniqueDefIndex)
		{
			msPos = GetMsPos(bars, barIndex, trkIndex, uniqueDefIndex);

			this.barIndex = barIndex;
			this.trkIndex = trkIndex;
			this.uniqueDefIndex = uniqueDefIndex;
		}

		/// <summary>
		/// Sets msPos to the msPosition of the final barline in the score.
		/// The other coordinates are all set to -1. 
		/// </summary>
		/// <param name="bars"></param>
		public ScoreCoordinates(List<Bar> bars)
		{
			barIndex = bars.Count - 1;
			trkIndex = 0;
			uniqueDefIndex = bars[barIndex].Trks[trkIndex].UniqueDefs.Count - 1;

			msPos = GetMsPos(bars, barIndex, trkIndex, uniqueDefIndex);

			IUniqueDef lastIUD = bars[barIndex].Trks[trkIndex].UniqueDefs[uniqueDefIndex];
			CheckIUD(lastIUD);
			msPos += lastIUD.MsDuration;

			barIndex = -1;
			trkIndex = -1;
			uniqueDefIndex = -1;
		}

		/// <summary>
		/// Sets msPos to the msPosition of the UniqueDef at bars[barIndex].Trks[trkIndex].UniqueDefs[uniqueDefIndex]. 
		/// </summary>
		private static int GetMsPos(List<Bar> bars, int barIndex, int trkIndex, int uniqueDefIndex)
		{
			Debug.Assert(barIndex >= 0 && barIndex < bars.Count);
			Debug.Assert(trkIndex >= 0 && trkIndex < bars[0].Trks.Count);
			Debug.Assert(uniqueDefIndex >= 0 && uniqueDefIndex < bars[barIndex].Trks[trkIndex].UniqueDefs.Count);

			int msPos = 0;
			for(int b = 0; b < barIndex; ++b)
			{
				Trk trk = bars[b].Trks[trkIndex];
				for(int m = 0; m < trk.UniqueDefs.Count; ++m)
				{
					IUniqueDef iud = trk.UniqueDefs[m];
					CheckIUD(iud);
					msPos += iud.MsDuration;
				}
			}

			Trk lastTrk = bars[barIndex].Trks[trkIndex];
			for(int u = 0; u < uniqueDefIndex; ++u)
			{
				IUniqueDef iud = lastTrk.UniqueDefs[u];
				CheckIUD(iud);
				msPos += iud.MsDuration;
			}

			return msPos;
		}

		/// <summary>
		/// The uniqueDefIndex (used in this class' constructor) currently assumes that
		/// there are only either MidiChordDefs or MidiRestDefs in the bars.
		/// If that should not be the case, then add the new type to the counted UniqueDefs,
		/// and adjust the uniqueDefIndex accordingly (to address the intended IUniqueDef).
		/// Also update the comment about the uniqueDefIndex parameter on the first constructor.
		/// </summary>
		private static void CheckIUD(IUniqueDef iud)
		{
			if(!(iud is MidiChordDef || iud is MidiRestDef))
			{
				Debug.Assert(false, $"Unexpectedly, the bars contain {iud.GetType().ToString()} objects.");
			}
		}

		public readonly int msPos;
		public readonly int barIndex;
		public readonly int trkIndex;
		public readonly int uniqueDefIndex;
	}
}
