using Moritz.Globals;
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
			const int minBarMsDuration = 5000;
			var nBars = gamutVector.Gamuts.Count;
			var nChannels = this.MidiChannelPerOutputVoice.Count;
			var palette = _palettes[0];
			//var nMidiChordDefs = palette.Count;
			//var midiChordDef = palette.MidiChordDef(0);
			//var basicMidiChordDef = midiChordDef.BasicMidiChordDefs[0];

			List<List<Trk>> trkListList = new List<List<Trk>>();
			for(var channel = 0; channel < nChannels; ++channel)
			{
				trkListList.Add(new List<Trk>());
			}

			// barlineMsPositions does not include 0, but does include the final barline position.
			barlineMsPositions = new List<int>();
			#region create trk0
			List<Trk> trks0 = trkListList[0];
			int iudIndex = 0;
			int currentMsPos = 0;
			for(int i = 0; i < nBars; i++)
			{
				Trk barTrk = new Trk(0);
				trks0.Add(barTrk);
				var barMsDuration = 0;
				while(barMsDuration <= minBarMsDuration)
				{
					var iud = palette.GetIUniqueDef((iudIndex++) % palette.Count);
					if(iud is MidiRestDef)
					{
						iud = palette.GetIUniqueDef(0);
					}
					barMsDuration += iud.MsDuration;
					barTrk.Add(iud);
				}
				currentMsPos += barTrk.MsDuration;
				barlineMsPositions.Add(currentMsPos);					
			}
			#endregion

			#region fill the other trks with rests
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

	}
}
