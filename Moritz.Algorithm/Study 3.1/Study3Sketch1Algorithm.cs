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
		/// This function returns null or a SortedDictionary per VoiceDef in each bar.
		/// An empty clefChanges list of the returned type can be
		///     1. created by calling the protected function GetEmptyClefChangesPerBar(int nBars, int nVoicesPerBar) and
		///     2. populated with code such as clefChanges[barIndex][voiceIndex].Add(9, "t3"). 
		/// The dictionary contains the index at which the clef will be inserted in the VoiceDef's IUniqueDefs,
		/// and the clef ID string ("t", "t1", "b3" etc.).
		/// Clefs will be inserted in reverse order of the Sorted dictionary, so that the indices are those of
		/// the existing IUniqueDefs before which the clef will be inserted.
		/// The SortedDictionaries should not contain the initial clefs per voiceDef - those will be included
		/// automatically.
		/// Note that a CautionaryChordDef counts as an IUniqueDef at the beginning of a bar, and that clefs
		/// cannot be inserted in front of them.
		/// Clefs should not be inserted here in the lower of two voices in a staff. Lower voices automatically have the
		/// SmallClefs that are defined for the upper voice.
		/// </summary>
		protected override List<List<SortedDictionary<int, string>>> GetClefChangesPerBar(int nBars, int nVoicesPerBar)
		{
			return null;

			//var clefChangesPerBar = GetEmptyStringExtrasPerBar(nBars, nVoicesPerBar);

			//SortedDictionary<int, string> voiceDef0Bar0 = clefChangesPerBar[0][0];
			//voiceDef0Bar0.Add(9, "b3");
			//voiceDef0Bar0.Add(8, "b2");
			//voiceDef0Bar0.Add(6, "b");
			//voiceDef0Bar0.Add(4, "t2");
			//voiceDef0Bar0.Add(2, "t");

			//// The following were redundant in this score, since they only apply to rests!
			//// voiceDef0Bar0.Add(7, "b1");
			//// voiceDef0Bar0.Add(5, "t3");
			//// voiceDef0Bar0.Add(3, "t1");

			//return clefChangesPerBar;
		}

		/// <summary>
		/// This function returns null or a SortedDictionary per VoiceDef in each bar.
		/// The dictionary contains the index of the IUniqueDef in the bar to which the associated lyric string
		/// will be attached. The index begins at 0 at the beginning of each bar (immediately after the barline).
		/// Lyrics may not be attached to a voice if there are two voices on the staff.
		/// </summary>
		protected override List<List<SortedDictionary<int, string>>> GetLyricsPerBar(int nBars, int nVoicesPerBar)
		{
			//return null;

			var lyricsPerBar = GetEmptyStringExtrasPerBar(nBars, nVoicesPerBar);

			SortedDictionary<int, string> voiceDef0Bar0 = lyricsPerBar[0][0];
			voiceDef0Bar0.Add(9, "lyric9");
			voiceDef0Bar0.Add(8, "lyric8");
			voiceDef0Bar0.Add(6, "lyric6");
			voiceDef0Bar0.Add(4, "lyric4");
			voiceDef0Bar0.Add(2, "lyric2");

			return lyricsPerBar;
		}

		#region CreateBar1Seq()
		private Seq CreateBar1Seq()
        {
            List<Trk> bar = new List<Trk>();

            byte channel = 0;
            foreach(Palette palette in _palettes)
            {
                Trk trk = new Trk(channel, 0, new List<IUniqueDef>());
                bar.Add(trk);
                WriteVoiceMidiDurationDefs1(trk, palette);
                channel++;
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
                IUniqueDef durationDef = palette.UniqueDurationDef(i);
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

            byte channel = 0;
            foreach(Palette palette in _palettes)
            {
                Trk trk = palette.NewTrk(channel);
                trk.MsPositionReContainer = 0;
                trk.MsDuration = 6000; // stretches or compresses the trk duration to 6000ms
                bar.Add(trk);
                ++channel;
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
