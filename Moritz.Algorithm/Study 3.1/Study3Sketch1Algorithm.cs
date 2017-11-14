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

        public override IReadOnlyList<int> MidiChannelIndexPerOutputVoice { get { return new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 }; } }
        public override int NumberOfInputVoices { get { return 0; } }
        public override int NumberOfBars { get { return 5; } }

        /// <summary>
        /// See CompositionAlgorithm.DoAlgorithm()
        /// </summary>
        public override List<Bar> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            _krystals = krystals;
            _palettes = palettes;

            List<int> barlineMsPositions = new List<int>();

            Seq bar1Seq = CreateBar1Seq();
            barlineMsPositions.Add(bar1Seq.MsDuration);

            Seq bar2Seq = CreateBar2Seq();
            barlineMsPositions.Add(bar1Seq.MsDuration + bar2Seq.MsDuration);

            Seq bars345Seq = bar2Seq.Clone();
            barlineMsPositions.Add(bar1Seq.MsDuration + bar2Seq.MsDuration + 5950);
            barlineMsPositions.Add(bar1Seq.MsDuration + bar2Seq.MsDuration + 10500);
            barlineMsPositions.Add(bar1Seq.MsDuration + bar2Seq.MsDuration + bars345Seq.MsDuration);

            Seq mainSeq = bar1Seq;
            mainSeq.Concat(bar2Seq);
            mainSeq.Concat(bars345Seq);

			Bar mainBar = new Bar(mainSeq, null, InitialClefPerChannel);

			List<Bar> bars = mainBar.GetBars(barlineMsPositions);

            InsertClefChanges(bars);

            return bars;
        }

        protected override void InsertClefChanges(List<Bar> bars)
        {
            //VoiceDef voiceDef = bars[0][bars[0].Count - 1];
            //voiceDef.InsertClefDef(5, "b");
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

            Seq seq = new Seq(0, bar, MidiChannelIndexPerOutputVoice);

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

            Seq seq = new Seq(0, bar, MidiChannelIndexPerOutputVoice);

            return seq;
        }
    }
}
