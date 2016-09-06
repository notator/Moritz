using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    public class Type1Block : Block
	{
        public Type1Block(int blockMsDuration, Trk level2TemplateTrk, int trk0InitialDelay,
            IReadOnlyList<IReadOnlyList<int>> durationModi, int nBars,
            IReadOnlyList<int> MidiChannelIndexPerOutputVoice)
            : base()
        {
            Trk trk0 = level2TemplateTrk.Clone();
            trk0.MidiChannel = 0;
            trk0.TransposeInGamut(8);
            trk0.AdjustVelocitiesHairpin(0, trk0.EndMsPositionReFirstIUD, 0.1, 1);
            trk0.AddRange(trk0.Clone());
            trk0.MsDuration = blockMsDuration - trk0InitialDelay;
            ((MidiChordDef)trk0[0]).PanMsbs = new List<byte>() { 0 };
            if(trk0InitialDelay > 0)
            {
                trk0.Insert(0, new RestDef(0, trk0InitialDelay));
            }

            Trk trk1 = level2TemplateTrk.Clone();
            trk1.MidiChannel = 1;
            //trk1.TransposeInGamut(7);
            trk1.AdjustVelocitiesHairpin(0, trk1.EndMsPositionReFirstIUD, 0.1, 1); 
            trk1.AddRange(trk1.Clone());
            trk1.MsDuration = blockMsDuration;
            ((MidiChordDef)trk1[0]).PanMsbs = new List<byte>() { 127 };

            List<Trk> trks = new List<Trk>();
            trks.Add(trk0);
            trks.Add(trk1);

            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice);

            List<int> barlineMsPositionsReBlock = new List<int>();
            int barMsDuration = blockMsDuration / nBars;
            int currentBarlineMsPosition = barMsDuration;
            int nMidBars = nBars - 1;
            for(int i = 0; i < nMidBars; ++i)
            {
                barlineMsPositionsReBlock.Add(currentBarlineMsPosition);
                currentBarlineMsPosition += barMsDuration;
            }
            barlineMsPositionsReBlock.Add(blockMsDuration); // Done here to prevent rounding errors.

            FinalizeBlock(seq, barlineMsPositionsReBlock);
        }
    }
}
