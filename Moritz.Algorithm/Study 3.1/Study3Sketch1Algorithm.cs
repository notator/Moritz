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
        /// <summary>
        /// This constructor can be called with both parameters null,
        /// just to get the overridden properties.
        /// </summary>
        public Study3Sketch1Algorithm()
            : base()
        {
        }

        public override IReadOnlyList<int> MidiChannelIndexPerOutputVoice { get { return new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 }; } }
        public override IReadOnlyList<int> MasterVolumePerOutputVoice { get { return new List<int>() { 100, 100, 100, 100, 100, 100, 100, 100 }; } }
        public override int NumberOfInputVoices { get { return 0; } }
        public override int NumberOfBars { get { return 5; } }

        /// <summary>
        /// See CompositionAlgorithm.DoAlgorithm()
        /// </summary>
        public override List<List<VoiceDef>> DoAlgorithm(List<Krystal> krystals, List<Palette> palettes)
        {
            _krystals = krystals;
            _palettes = palettes;

            Seq bar1 = CreateBar1();

            int bar2StartMsPos = bar1.MsDuration;
            Seq bar2 = CreateBar2(bar2StartMsPos);
            
            Seq bars3to5 = bar2.Clone();
            int bar3StartMsPos = bar2StartMsPos + bar2.MsDuration;
            int bar5EndMsPos = bar3StartMsPos + bars3to5.MsDuration; 

            List<int> absMsPositionsOfRightBarlines = GetAbsMsPositionsOfRightBarlines( bar2StartMsPos, bar3StartMsPos, bar5EndMsPos );

            Seq mainSeq = bar1;
            mainSeq.Concat(bar2);
            mainSeq.Concat(bars3to5);

            // Blocks contain a list of VoiceDefs
            Block sequence = new Block(mainSeq, null); // converts mainSeq to a block (There are no InputVoiceDefs in this score.)

            List<List<VoiceDef>> bars = ConvertBlockToBars(sequence, absMsPositionsOfRightBarlines);

            return bars;
        }

        private List<int> GetAbsMsPositionsOfRightBarlines(int bar2StartMsPos, int bar3StartMsPos, int bar5EndMsPos)
        {
            List<int> rightBarlinePositions = new List<int>() { bar2StartMsPos, bar3StartMsPos };

            int bar4StartPos = bar3StartMsPos + 5950;
            int bar5StartPos = bar3StartMsPos + 10500;

            rightBarlinePositions.Add(bar4StartPos);
            rightBarlinePositions.Add(bar5StartPos);
            rightBarlinePositions.Add(bar5EndMsPos);

            return rightBarlinePositions;
        }

        #region CreateBar1()
        private Seq CreateBar1()
        {
            List<Trk> bar = new List<Trk>();

			byte channel = 0;
            foreach(Palette palette in _palettes)
            {
                Trk voice = new Trk(channel, new List<IUniqueDef>());
                bar.Add(voice);
                WriteVoiceMidiDurationDefs1(voice, palette);
				channel++;
            }

            Seq seq = new Seq(0, bar, MidiChannelIndexPerOutputVoice);

            return seq;
        }

        private void WriteVoiceMidiDurationDefs1(Trk trk, Palette palette)
        {
            int msPositionReTrk = 0;
            int bar1ChordMsSeparation = 1500;
            for(int i = 0; i < palette.Count;++i)
            {
                IUniqueDef durationDef = palette.UniqueDurationDef(i);
                durationDef.MsPositionReTrk = msPositionReTrk;
                RestDef restDef = new RestDef(msPositionReTrk + durationDef.MsDuration, bar1ChordMsSeparation - durationDef.MsDuration);
                msPositionReTrk += bar1ChordMsSeparation;
                trk.UniqueDefs.Add(durationDef);
                trk.UniqueDefs.Add(restDef);
            }
        }
        #endregion CreateBar1()

        /// <summary>
        /// This function creates only one bar, using Trk objects. 
        /// </summary>
        private Seq CreateBar2(int barStartAbsMsPos)
        {
            List<Trk> bar = new List<Trk>();

            byte channel = 0;
            foreach(Palette palette in _palettes)
            {
                Trk trk = palette.NewTrk(channel);
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
                    RestDef restDef = new RestDef(0, restMsDuration);
                    trk.Insert(0, restDef);
                }
                restMsDuration += 1500;
                maxMsPosReBar = trk.EndMsPositionReTrk;
            }

            // add the final rest in the bar
            foreach(Trk trk in bar)
            {
                int trkEndMsPosReBar = trk.EndMsPositionReTrk;
                if(maxMsPosReBar > trkEndMsPosReBar)
                {
                    RestDef restDef = new RestDef(trkEndMsPosReBar, maxMsPosReBar - trkEndMsPosReBar);
                    trk.Add(restDef);
                }
            }

            Seq seq = new Seq(barStartAbsMsPos, bar, MidiChannelIndexPerOutputVoice);

            return seq;
        }
    }
}
