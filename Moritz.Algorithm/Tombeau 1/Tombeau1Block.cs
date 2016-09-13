using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    public class Tombeau1Block : Block
	{
        public Tombeau1Block(int blockNum, int nBlocks, int blockMsDuration, Tombeau1BaseTrk tombeau1BaseTrk, int nBarsInBlock,
            IReadOnlyList<int> MidiChannelIndexPerOutputVoice)
            : base()
        {
            List<Trk> trks = new List<Trk>();
            Trk trk0 = GetTrk0(blockNum, nBlocks, blockMsDuration, tombeau1BaseTrk, MidiChannelIndexPerOutputVoice[0]);
            trks.Add(trk0);
            if(blockNum > 1)
            {
                Trk trk1 = GetTrk1(blockNum, nBlocks, blockMsDuration, tombeau1BaseTrk, MidiChannelIndexPerOutputVoice[1]);
                trks.Add(trk1);
            }
            if(blockNum > 3)
            {
                //Trk trk2 = GetTrk1(blockNum, nBlocks, blockMsDuration, tombeau1BaseTrk, MidiChannelIndexPerOutputVoice[2]);
                //trks.Add(trk2);

                //Trk trk2 = GetTrk2(blockNum, nBlocks, blockMsDuration, tombeau1BaseTrk,, MidiChannelIndexPerOutputVoice[2]);
                //trks.Add(trk2)
            }
            if(blockNum > 7)
            {
                //Trk trk3 = GetTrk1(blockNum, nBlocks, blockMsDuration, tombeau1BaseTrk, MidiChannelIndexPerOutputVoice[3]);
                //trks.Add(trk3);

                //Trk trk3 = GetTrk3(blockNum, nBlocks, blockMsDuration, tombeau1BaseTrk, MidiChannelIndexPerOutputVoice[3]);
                //trks.Add(trk3);
            }
            if(blockNum > 13)
            {
                //Trk trk4 = GetTrk1(blockNum, nBlocks, blockMsDuration, tombeau1BaseTrk, MidiChannelIndexPerOutputVoice[4]);
                //trks.Add(trk4);

                //Trk trk4 = GetTrk4(blockNum, nBlocks, blockMsDuration, tombeau1BaseTrk, MidiChannelIndexPerOutputVoice[4]);
                //trks.Add(trk4);
            }

            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice);

            seq.AlignTrkAxes();

            List<int> barlineMsPositionsReBlock = GetBarlineMsPositionsReBlock(blockMsDuration, nBarsInBlock);
            FinalizeBlock(seq, barlineMsPositionsReBlock);
        }

        private List<int> GetBarlineMsPositionsReBlock(int blockMsDuration, int nBarsInBlock)
        {
            List<int> barlineMsPositionsReBlock = new List<int>();

            int barMsDuration = blockMsDuration / nBarsInBlock;
            int currentBarlineMsPosition = barMsDuration;
            int nMidBars = nBarsInBlock - 1;
            for(int i = 0; i < nMidBars; ++i)
            {
                barlineMsPositionsReBlock.Add(currentBarlineMsPosition);
                currentBarlineMsPosition += barMsDuration;
            }
            barlineMsPositionsReBlock.Add(blockMsDuration); // Done here to prevent rounding errors.

            return barlineMsPositionsReBlock;
        }

        private Trk GetTrk0(int blockNum, int nBlocks, int blockMsDuration, Tombeau1BaseTrk tombeau1BaseTrk, int midiChannel)
        {
            Tombeau1BaseTrk trk = tombeau1BaseTrk.Clone();
            trk.MidiChannel = midiChannel;
            trk.MsDuration = blockMsDuration;
            ((MidiChordDef)trk[0]).PanMsbs = new List<byte>() { 127 };

            int maxAxisIndex = trk.Count - 1;
            List<int> axisOffsets = new List<int>()
            { 0, 0, maxAxisIndex - 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Debug.Assert(axisOffsets.Count == nBlocks);
            trk.AxisIndex = maxAxisIndex - axisOffsets[blockNum - 1];

            double velocityFactor = 1 - ((double)(blockNum - 1) / (nBlocks * 1.5));
            trk.AdjustVelocities(velocityFactor);

            return trk;
        }

        private Trk GetTrk1(int blockNum, int nBlocks, int blockMsDuration, Tombeau1BaseTrk tombeau1TemplateTrk, int midiChannel)
        {
            Tombeau1BaseTrk trk = tombeau1TemplateTrk.Clone();
            List<int> durationCompressions = new List<int>()
            { 0, 1100, 1700, 1600, 1500, 1400, 1300, 1200, 1100, 1000, 900, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900 };
            Debug.Assert(durationCompressions.Count == nBlocks);
            List<int> transformationPercents = new List<int>()
            { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            Debug.Assert(transformationPercents.Count == nBlocks);

            int maxAxisIndex = trk.Count - 1;
            List<int> axisOffsets = new List<int>()
            { 0, 0, maxAxisIndex - 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Debug.Assert(axisOffsets.Count == nBlocks);
            trk.AxisIndex = maxAxisIndex - axisOffsets[blockNum - 1];

            ((MidiChordDef)trk[0]).PanMsbs = new List<byte>() { 0 };

            List<byte> velocityPerAbsolutePitch = ((MidiChordDef)trk[0]).Gamut.GetVelocityPerAbsolutePitch(25, true);

            trk.MidiChannel = midiChannel;
            trk.TransposeInGamut(8);
            trk.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, transformationPercents[blockNum - 1]);

            switch (blockNum)
            {
                case 2:
                    trk.AdjustVelocitiesHairpin(0, trk.MsDuration, 0.7, 1.0);
                    break;
                case 3:
                    trk.AdjustVelocitiesHairpin(0, trk.MsDuration, 1.0, 0.7);
                    break;
                default:
                    break;
            }

            trk.MsDuration = blockMsDuration - durationCompressions[blockNum - 1];

            return trk;
        }

    }
}
