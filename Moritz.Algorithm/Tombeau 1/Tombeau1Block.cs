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
                //Trk trk2 = GetTrk2(blockNum, nBlocks, blockMsDuration, tombeau1BaseTrk,, MidiChannelIndexPerOutputVoice[2]);
                //trks.Add(trk2);
            }
            if(blockNum > 7)
            {
                //Trk trk3 = GetTrk3(blockNum, nBlocks, blockMsDuration, tombeau1BaseTrk, MidiChannelIndexPerOutputVoice[3]);
                //trks.Add(trk3);
            }
            if(blockNum > 13)
            {
                //Trk trk4 = GetTrk4(blockNum, nBlocks, blockMsDuration, tombeau1BaseTrk, MidiChannelIndexPerOutputVoice[4]);
                //trks.Add(trk4);
            }

            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice);

            List<int> barlineMsPositionsReBlock = new List<int>();
            #region set barlines
            int barMsDuration = blockMsDuration / nBarsInBlock;
            int currentBarlineMsPosition = barMsDuration;
            int nMidBars = nBarsInBlock - 1;
            for(int i = 0; i < nMidBars; ++i)
            {
                barlineMsPositionsReBlock.Add(currentBarlineMsPosition);
                currentBarlineMsPosition += barMsDuration;
            }
            barlineMsPositionsReBlock.Add(blockMsDuration); // Done here to prevent rounding errors.
            #endregion set barlines
            FinalizeBlock(seq, barlineMsPositionsReBlock);
        }

        private Trk GetTrk0(int blockNum, int nBlocks, int blockMsDuration, Tombeau1BaseTrk tombeau1BaseTrk, int midiChannel)
        {
            Tombeau1BaseTrk trk = tombeau1BaseTrk.Clone();
            trk.MidiChannel = midiChannel;
            trk.MsDuration = blockMsDuration;
            ((MidiChordDef)trk[0]).PanMsbs = new List<byte>() { 127 };

            return trk;
        }

        private Trk GetTrk1(int blockNum, int nBlocks, int blockMsDuration, Tombeau1BaseTrk tombeau1TemplateTrk, int midiChannel)
        {
            Tombeau1BaseTrk trk = tombeau1TemplateTrk.Clone();
            List<int> initialDelays = new List<int>()
            { 0, 1800, 1700, 1600, 1500, 1400, 1300, 1200, 1100, 1000, 900, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900 };
            Debug.Assert(initialDelays.Count == nBlocks);
            List<int> transformationPercents = new List<int>()
            { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            Debug.Assert(transformationPercents.Count == nBlocks);

            int trk0InitialDelay = initialDelays[blockNum - 1];
            int transformationPercent = transformationPercents[blockNum - 1];

           List<byte> velocityPerAbsolutePitch = ((MidiChordDef)trk[0]).Gamut.GetVelocityPerAbsolutePitch(25, true);

            trk.MidiChannel = midiChannel;
            trk.TransposeInGamut(8);
            trk.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, transformationPercent);

            trk.MsDuration = blockMsDuration - trk0InitialDelay;
            ((MidiChordDef)trk[0]).PanMsbs = new List<byte>() { 0 };
            if(trk0InitialDelay > 0)
            {
                trk.Insert(0, new RestDef(0, trk0InitialDelay));
            }

            return trk;
        }

    }
}
