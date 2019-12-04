using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;
using System.Collections.Generic;

namespace Moritz.Algorithm.Study4
{
	public partial class Study4Algorithm : CompositionAlgorithm
	{
		private void GetTrksAndBarlines(GamutVector gamutVector, out List<Trk> trks, out List<int> barlineMsPositions)
		{
			trks = new List<Trk>();
			barlineMsPositions = new List<int>();
			for(int i = 0; i < this.MidiChannelPerOutputVoice.Count; i++)
			{
				trks.Add(new Trk(i));
			}

			List<List<Trk>> trkListList = GetTrkListList(gamutVector, out barlineMsPositions);

			for(int i = 0; i < trkListList.Count; i++)
			{
				var trkList = trkListList[i];
				var currentMsPos = 0;
				foreach(Trk barTrk in trkList)
				{
					trks[i].ConcatCloneAt(barTrk, currentMsPos);

					currentMsPos += barTrk.MsDuration;
					M.Assert(barlineMsPositions.Contains(currentMsPos));
				}
			}
		}

		/// <summary>
		/// Creates a list of 8 parallel Trk lists, each of which contains 55 Trks (=Bars).
		/// </summary>
		/// <param name="gamutVector"></param>
		/// <returns></returns>
		private List<List<Trk>> GetTrkListList(GamutVector gamutVector, out List<int> barlineMsPositions)
		{
			var nBars = gamutVector.Gamuts.Count;
			var nChannels = this.MidiChannelPerOutputVoice.Count;

			//var nMidiChordDefs = palette.Count;
			//var midiChordDef = palette.MidiChordDef(0);
			//var basicMidiChordDef = midiChordDef.BasicMidiChordDefs[0];

			List<List<Trk>> trkListList = new List<List<Trk>>();
			for(var channel = 0; channel < nChannels; ++channel)
			{
				trkListList.Add(new List<Trk>());
			}

			// the returned barlineMsPositions do not include 0, but do include the final barline position.
			trkListList[0] = CreateChannel0BarTrks(gamutVector.Gamuts, out barlineMsPositions);

			#region fill the other trks with rests
			int currentMsPos = 0;
			for(int i = 1; i < nChannels; i++)
			{
				currentMsPos = 0;
				var trkList = trkListList[i];
				for(int j = 0; j < nBars; j++)
				{
					Trk trk = new Trk(i);
					trkList.Add(trk);
					trk.Add(new MidiRestDef(currentMsPos, barlineMsPositions[j] - currentMsPos));
					currentMsPos = barlineMsPositions[j];
				}
			}
			#endregion

			return trkListList;
		}

		// the returned barlineMsPositions do not include 0, but do include the final barline position.
		private List<Trk> CreateChannel0BarTrks(IReadOnlyList<Gamut> gamuts, out List<int> barlineMsPositions)
		{			
			barlineMsPositions = new List<int>(); // out
			List<Trk> topTrks = new List<Trk>(); // return

			List<Trk> ch0RegionTrks = null;
			var rSBI = RegionStartBarIndices;
			List<int> nBarsPerRegion = new List<int>();
			for(int i = 0; i < rSBI.Count - 1; i++)
			{
				ch0RegionTrks = GetCh0RegionTrks(i, gamuts, rSBI[i], rSBI[i+1]);
				topTrks.AddRange(ch0RegionTrks);
			}
			ch0RegionTrks = GetCh0RegionTrks(rSBI.Count - 1, gamuts, rSBI[rSBI.Count - 1], gamuts.Count);
			topTrks.AddRange(ch0RegionTrks);

			int msPos = 0;
			foreach(Trk trk in topTrks)
			{
				msPos += trk.MsDuration;
				barlineMsPositions.Add(msPos);
			}			

			return topTrks;
		}

		// endIndex is non-inclusive
		private List<Trk> GetCh0RegionTrks(int regionIndex, IReadOnlyList<Gamut> gamuts, int startGamutIndex, int endGamutIndex)
		{
			List<Trk> regionTrks = new List<Trk>(); // return
			switch(regionIndex)
			{
				case 0:
					Palette palette = _palettes[0];
					int iudIndex = 0;
					int minBarMsDuration = 5000;

					for(int i = startGamutIndex; i < endGamutIndex; i++)
					{
						Trk barTrk = new Trk(0);
						regionTrks.Add(barTrk); // region 0 only has one bar
						var barMsDuration = 0;
						while(barMsDuration <= minBarMsDuration)
						{
							var iud = palette.GetIUniqueDef((iudIndex++) % palette.Count);
							if(iud is MidiRestDef)
							{
								iud = palette.GetIUniqueDef(0);
							}
							iud = FitToGamut(gamuts[i], iud);

							barMsDuration += iud.MsDuration;

							barTrk.Add(iud);
						}
					}
					break;
				default:
					Palette defaultPalette = _palettes[0];
					iudIndex = 0;
					minBarMsDuration = 1000;

					for(int i = startGamutIndex; i < endGamutIndex; i++)
					{
						Trk barTrk = new Trk(0);
						regionTrks.Add(barTrk);
						var barMsDuration = 0;
						while(barMsDuration <= minBarMsDuration)
						{
							var iud = defaultPalette.GetIUniqueDef((iudIndex++) % defaultPalette.Count);
							if(iud is MidiRestDef)
							{
								iud = defaultPalette.GetIUniqueDef(0);
							}
							iud = FitToGamut(gamuts[i], iud);

							barMsDuration += iud.MsDuration;

							barTrk.Add(iud);
						}
					}
					break;

			}
			return regionTrks;
		}

		private IUniqueDef FitToGamut(Gamut gamut, IUniqueDef iud)
		{
			return iud.Clone() as IUniqueDef;
		}
	}
}
