using System;
using System.Collections.Generic;

using Krystals4ObjectLibrary;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Study3Sketch1
{
    /// <summary>
    /// Algorithm for testing Song 6's palettes.
    /// This may develope as composition progresses...
    /// </summary>
    public class Study3Sketch1Algorithm : CompositionAlgorithm
    {
        public Study3Sketch1Algorithm()
            : base()
        {
            CheckParameters();
        }

        public override IReadOnlyList<int> MidiChannelPerOutputVoice { get { return new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 }; } }
        public override IReadOnlyList<int> MidiChannelPerInputVoice { get { return null; } }
        public override int NumberOfBars { get { return 5; } }

        /// <summary>
        /// See CompositionAlgorithm.DoAlgorithm()
        /// </summary>
        public override List<Bar> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            _krystals = krystals;
            _palettes = palettes;

            var approximateBarlineMsPositions = new List<double>();

            Seq bar1Seq = CreateBar1Seq();
            approximateBarlineMsPositions.Add(bar1Seq.MsDuration);

            Seq bar2Seq = CreateBar2Seq();
            approximateBarlineMsPositions.Add(bar1Seq.MsDuration + bar2Seq.MsDuration);

            Seq bars345Seq = bar2Seq.Clone();
            approximateBarlineMsPositions.Add(bar1Seq.MsDuration + bar2Seq.MsDuration + 5950);
            approximateBarlineMsPositions.Add(bar1Seq.MsDuration + bar2Seq.MsDuration + 10500);
            approximateBarlineMsPositions.Add(bar1Seq.MsDuration + bar2Seq.MsDuration + bars345Seq.MsDuration);

			Seq mainSeq = bar1Seq;
            mainSeq.Concat(bar2Seq);
            mainSeq.Concat(bars345Seq);

			List<int> barlineMsPositions = GetBarlinePositions(mainSeq.Trks, null, approximateBarlineMsPositions);

			List<List<SortedDictionary<int, string>>> clefChangesPerBar = GetClefChangesPerBar(barlineMsPositions.Count, mainSeq.Trks.Count);

			List<List<SortedDictionary<int, string>>> lyricsPerBar = GetLyricsPerBar(barlineMsPositions.Count, mainSeq.Trks.Count);

			List<Bar> bars = GetBars(mainSeq, null, barlineMsPositions, clefChangesPerBar, lyricsPerBar);

			return bars;
        }

		/// <summary>
		/// See summary and example code on abstract definition in CompositionAlogorithm.cs
		/// </summary>
		protected override List<List<SortedDictionary<int, string>>> GetClefChangesPerBar(int nBars, int nVoicesPerBar)
		{
			return null;
		}

		/// <summary>
		/// Lyrics can be attached to MidiChordDefs or InputChordDefs earlier in the algorithm, but this function
		/// provides the possibility of adding them all in one place.
		/// This function returns null or a SortedDictionary per VoiceDef in each bar.
		/// The dictionary contains the index of the MidiChordDef or InputChordDef in the bar to which the associated
		/// lyric string will be attached. The index is of MidiChordDefs or InputChordDefs only, beginning with 0 for
		/// the first MidiChordDef or InptChordDef in the bar.
		/// Lyrics that are attached to top voices on a staff will, like dynamics, be automatically placed above the staff.
		/// </summary>
		protected override List<List<SortedDictionary<int, string>>> GetLyricsPerBar(int nBars, int nVoicesPerBar)
		{
			//var lyricsPerBar = GetEmptyStringExtrasPerBar(nBars, nVoicesPerBar);
			
			//SortedDictionary<int, string> bar2VoiceDef1 = lyricsPerBar[1][0]; // Bar 2 Voice 1.
			//bar2VoiceDef1.Add(9, "lyric9");
			//bar2VoiceDef1.Add(8, "lyric8");
			//bar2VoiceDef1.Add(6, "lyric6");
			//bar2VoiceDef1.Add(4, "lyric4");
			//bar2VoiceDef1.Add(2, "lyric2");
			
			//SortedDictionary<int, string> bar2VoiceDef2 = lyricsPerBar[1][1]; // Bar 2 Voice 2.
			//bar2VoiceDef2.Add(9, "lyric9a");
			//bar2VoiceDef2.Add(8, "lyric8a");
			//bar2VoiceDef2.Add(6, "lyric6a");
			//bar2VoiceDef2.Add(4, "lyric4a");
			//bar2VoiceDef2.Add(2, "lyric2a");

			//return lyricsPerBar;

			return null;
		}

		#region CreateBar1Seq()
		private Seq CreateBar1Seq()
        {
            List<Trk> bar = new List<Trk>();

            byte channel = (byte)(_palettes.Count - 1);
            foreach(Palette palette in _palettes)
            {
                Trk trk = new Trk(channel, 0, new List<IUniqueDef>());
                bar.Add(trk);
                WriteVoiceMidiDurationDefs1(trk, palette);
                channel--;
            }

            Seq seq = new Seq(0, bar, MidiChannelPerOutputVoice);

            return seq;
        }

        private void WriteVoiceMidiDurationDefs1(Trk trk, Palette palette)
        {
            trk.MsPositionReContainer = 0;

            int msPositionReFirstIUD = 0;
            int bar1ChordMsSeparation = 1500;
            for(int i = 0; i < palette.Count; ++i)
            {
                IUniqueDef durationDef = palette[i];
                durationDef.MsPositionReFirstUD = msPositionReFirstIUD;
                MidiRestDef restDef = new MidiRestDef(msPositionReFirstIUD + durationDef.MsDuration, bar1ChordMsSeparation - durationDef.MsDuration);
                msPositionReFirstIUD += bar1ChordMsSeparation;
                trk.UniqueDefs.Add(durationDef);
                trk.UniqueDefs.Add(restDef);
            }
        }
        #endregion CreateBar1Seq()

        /// <summary>
        /// This function creates only one bar, using Trk objects. 
        /// </summary>
        private Seq CreateBar2Seq()
        {
            List<Trk> bar = new List<Trk>();

			byte channel = (byte)(_palettes.Count - 1);
			foreach(Palette palette in _palettes)
            {
                Trk trk = palette.NewTrk(channel);
                trk.MsPositionReContainer = 0;
                trk.MsDuration = 6000; // stretches or compresses the trk duration to 6000ms
                bar.Add(trk);
                channel--;
            }

            int maxMsPosReBar = 0;
            // insert rests at the start of the Trks
            int restMsDuration = 0;
            foreach(Trk trk in bar)
            {
                if(restMsDuration > 0)
                {
                    MidiRestDef restDef = new MidiRestDef(0, restMsDuration);
                    trk.Insert(0, restDef);
                }
                restMsDuration += 1500;
                maxMsPosReBar = trk.EndMsPositionReFirstIUD;
            }

            // add the final rest in the bar
            foreach(Trk trk in bar)
            {
                int trkEndMsPosReBar = trk.EndMsPositionReFirstIUD;
                if(maxMsPosReBar > trkEndMsPosReBar)
                {
                    MidiRestDef restDef = new MidiRestDef(trkEndMsPosReBar, maxMsPosReBar - trkEndMsPosReBar);
                    trk.Add(restDef);
                }
            }

            Seq seq = new Seq(0, bar, MidiChannelPerOutputVoice);

            return seq;
        }
    }
}
