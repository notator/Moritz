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

			ch0RegionTrks = GetCh0_Region0_Trks(gamuts, rSBI[0], rSBI[1]);
			topTrks.AddRange(ch0RegionTrks);

			ch0RegionTrks = GetCh0RegionTrks(1, gamuts, rSBI[1], rSBI[2]);
			topTrks.AddRange(ch0RegionTrks);
			ch0RegionTrks = GetCh0RegionTrks(2, gamuts, rSBI[2], rSBI[3]);
			topTrks.AddRange(ch0RegionTrks);
			ch0RegionTrks = GetCh0RegionTrks(3, gamuts, rSBI[3], rSBI[4]);
			topTrks.AddRange(ch0RegionTrks);
			ch0RegionTrks = GetCh0RegionTrks(4, gamuts, rSBI[4], rSBI[5]);
			topTrks.AddRange(ch0RegionTrks);
			ch0RegionTrks = GetCh0RegionTrks(5, gamuts, rSBI[5], rSBI[6]);
			topTrks.AddRange(ch0RegionTrks);
			ch0RegionTrks = GetCh0RegionTrks(6, gamuts, rSBI[6], gamuts.Count);
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
		private List<Trk> GetCh0_Region0_Trks(IReadOnlyList<Gamut> gamuts, int startGamutIndex, int endGamutIndex)
		{
			List<Trk> regionTrks = new List<Trk>(); // return

			Palette palette = _palettes[0];
			int iudIndex = 0;
			int minBarMsDuration = 6000;

			for(int i = startGamutIndex; i < endGamutIndex; i++)
			{
				Trk barTrk = new Trk(0);
				regionTrks.Add(barTrk); // region 0 only has one bar
				var barMsDuration = 0;
				while(barMsDuration <= minBarMsDuration)
				{
					var iud = palette.GetIUniqueDef((iudIndex++) % palette.Count);
					if(iud is MidiChordDef mcd)
					{
						mcd = FitToGamut(gamuts[i], mcd, 0);

						barMsDuration += mcd.MsDuration;

						barTrk.Add(mcd);
					}
				}
			}

			return regionTrks;
		}

		// endIndex is non-inclusive
		private List<Trk> GetCh0RegionTrks(int regionIndex, IReadOnlyList<Gamut> gamuts, int startGamutIndex, int endGamutIndex)
		{
			List<Trk> regionTrks = new List<Trk>(); // return
			Palette defaultPalette = _palettes[0];
			int iudIndex = 0;
			int minBarMsDuration = 1000;

			for(int i = startGamutIndex; i < endGamutIndex; i++)
			{
				Trk barTrk = new Trk(0);
				regionTrks.Add(barTrk);
				var barMsDuration = 0;
				while(barMsDuration <= minBarMsDuration)
				{
					var iud = defaultPalette.GetIUniqueDef((iudIndex++) % defaultPalette.Count);
					if(iud is MidiChordDef mcd)
					{
						mcd = FitToGamut(gamuts[i], mcd, 0);

						barMsDuration += mcd.MsDuration;

						barTrk.Add(mcd);
					}
				}
			}

			return regionTrks;
		}

		/// <summary>
		/// The argument MidiChordDef determines the following parameters in the returned MidiChordDef:
		/// (These should be set using a palette.)
		///    1. the number of BasicMidiChordDefs and their (initial) pitch density
		///    2. pitchWheelDeviation
		///    3. pitchWheel and pan slider definitions
		/// The lowest pitch in each BasicMidichordDef is found as follows:
		///    1. The lowest pitch in BasicMidiChordDef[0] determines the *register* of the output MidiChordDef.
		///       (The lowest pitch in the returned BasicMidiChordDef[0] is found as follows:
		///          a) find its absolute pitch using chordShapeindex in gamut.LinearChordShapeMatrix (see below)
		///          b) find the lowest possible corresponding relative pitch that is higher than or equal to the
		///             lowest pitch in the input .
		///    2. The lowest pitches of the input BasicMidiChordDefs determine the contour of the lowest pitches
		///       in the output BasicMidiChordDefs. A chromatic step in the input corresponds to a gamut step in the output.
		///       Gamut steps can simply be achieved by using the chromatic indices as indices in the gamut.PitchWeights list.
		/// The shape of BasicMidiChordDef[0] is determined by adding the absolute pitches in the ChordShape upwards from the base.
		/// The other BasicMidichords have the same shape, but are transposed by gamut step, not chromatically...
		/// 
		/// Probably, additional pitches can be added to chords, doubling octaves where weights are heavy...
		/// </summary>
		private MidiChordDef FitToGamut(Gamut gamut, MidiChordDef mcd, int chordShapeIndex)
		{
			/// todo... (see above)
			List<IUniqueDef> basicChords = new List<IUniqueDef>();

			foreach(var bmc in mcd.BasicMidiChordDefs)
			{
				List<byte> pitches = new List<byte>();
				List<byte> velocities = new List<byte>();
				for(int i = 0; i < bmc.Pitches.Count; i++)
				{ 
					// Do the pitch and velocity changes here.

					pitches.Add((byte)(bmc.Pitches[i] - 12));
					velocities.Add(bmc.Velocities[i]);
				}
				basicChords.Add(new MidiChordDef(pitches, velocities, mcd.MsDuration, mcd.HasChordOff));
			}

			var rval = new MidiChordDef(mcd.MsDuration, basicChords, mcd.OrnamentText)
			{
				PitchWheelDeviation = mcd.PitchWheelDeviation,
				MidiChordSliderDefs = mcd.MidiChordSliderDefs
			};

			return rval;
		}
	}
}
