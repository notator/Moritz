using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    internal class PaletteGrps
    {
		protected PaletteGrps(int rootOctave, int basePitch, int barLengthMsDurationLimit)
		{
			for(int i = 0; i < Gamut.RelativePitchHierarchiesCount; ++i)
			{
				List<PaletteGrp> paletteGrps = GetPaletteGrpList(rootOctave, basePitch, i);
				_baseGrps.Add(paletteGrps as IReadOnlyList<PaletteGrp>);
			}
			_barLengthMsDurationLimit = barLengthMsDurationLimit;
		}

		/// <summary>
		/// Each Grp in the returned list has the same Gamut.
		/// </summary>
		private List<PaletteGrp> GetPaletteGrpList(int rootOctave, int gamutBasePitch, int relativePitchHierarchyIndex)
		{
			//const int gamutBasePitch = 9;
			List<PaletteGrp> grps = new List<PaletteGrp>();

			for(int i = 0, domain = 12; domain >= 1; --domain, ++i) // domain is both Gamut.PitchesPerOctave and nChords per Grp
			{
				Gamut gamut = new Gamut(relativePitchHierarchyIndex, gamutBasePitch, domain);

				PaletteGrp tpg = new PaletteGrp(gamut, rootOctave);
				int minMsDuration = 230;
				int maxMsDuration = 350;
				tpg.SetDurationsFromPitches(maxMsDuration, minMsDuration, true);

				tpg.SortRootNotatedPitchAscending();

				grps.Add(tpg);
			}

			return (grps);
		}

		/// <summary>
		/// will be overridden in derived classes
		/// </summary>
		/// <param name="baseGrps"></param>
		/// <returns></returns>
		protected virtual List<PaletteGrp> Compose(IReadOnlyList<PaletteGrp> baseGrps)
		{
			var cgs = new List<PaletteGrp>(baseGrps);
			return cgs;
		}

		/// <summary>
		/// The returned list contains the positions of both the first barline (msPos=0) and the final barline.
		/// All the returned barline positions are at the boundaries of composed PaletteGrps.
		/// A barline is returned at the end of each list of PaletteGrp (i.e. gamut), with intermediate
		/// barlines such that bars have durations that are as equal as possible.
		/// All bars will have a duration less than _barLengthMsDurationLimit.
		/// _barLengthMsDurationLimit is a readonly int, set using a PaletteGrps constructor argument.
		/// </summary>
		internal List<int> ComposedBarlinePositions()
		{
			int GetMaxBarLength(int availableMsDur)
			{
				int maxBarLength = int.MaxValue;
				int divisor = 1;
				while(maxBarLength > _barLengthMsDurationLimit)
				{
					maxBarLength = availableMsDur / divisor++;
				}
				return maxBarLength;
			}

			List<int> barlinePositions = new List<int>() { 0 };
			List<int> pGrpsEndMsPositions = new List<int>();
			List<int> intermedateBarlinePositions = new List<int>();

			int endMsPos = 0;
			for(int i = 0; i < _composedGrps.Count; ++i)
			{
				IReadOnlyList<PaletteGrp> gamut = _composedGrps[i];
				pGrpsEndMsPositions.Clear();
				foreach(PaletteGrp pGrp in gamut)
				{
					endMsPos += pGrp.MsDuration;
					pGrpsEndMsPositions.Add(endMsPos);
				}

				int maxBarLength = GetMaxBarLength(endMsPos - barlinePositions[i]);

				int potentialBarlineMsPos = barlinePositions[i] + maxBarLength;
				for(int j = 0; j < pGrpsEndMsPositions.Count - 1; ++j)
				{
					int pos1 = pGrpsEndMsPositions[j];
					int pos2 = pGrpsEndMsPositions[j + 1];
					if(potentialBarlineMsPos >= pos1 && potentialBarlineMsPos < pos2)
					{
						intermedateBarlinePositions.Add(pos1);
						potentialBarlineMsPos = pos1 + GetMaxBarLength(endMsPos - pos1);
						if(potentialBarlineMsPos >= endMsPos)
						{
							break;
						}
					}
				}

				barlinePositions.Add(endMsPos);
			}

			barlinePositions.AddRange(intermedateBarlinePositions);
			barlinePositions.Sort();

			return barlinePositions;
		}

		private readonly int _barLengthMsDurationLimit;

		internal IReadOnlyList<IReadOnlyList<PaletteGrp>> BaseGrps { get => _baseGrps as IReadOnlyList<IReadOnlyList<PaletteGrp>>; }
		private List<IReadOnlyList<PaletteGrp>> _baseGrps = new List<IReadOnlyList<PaletteGrp>>();

		internal IReadOnlyList<IReadOnlyList<PaletteGrp>> ComposedGrps { get => _composedGrps as IReadOnlyList<IReadOnlyList<PaletteGrp>>; }
		protected List<IReadOnlyList<PaletteGrp>> _composedGrps = new List<IReadOnlyList<PaletteGrp>>();

		internal List<List<PaletteGrp>> ComposedGrpsClone()
		{
			var grps = new List<List<PaletteGrp>>();
			foreach(IReadOnlyList<PaletteGrp> irog in ComposedGrps)
			{
				grps.Add(new List<PaletteGrp>(irog));
			}
			return grps;
		}
	}
}