using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    public class Tombeau1Block : Block
	{
        public Tombeau1Block(int blockMsDuration, Tombeau1TemplateTrk tombeau1TemplateTrk, double transformationPercent, int trk0InitialDelay, int nBars,
            IReadOnlyList<int> MidiChannelIndexPerOutputVoice)
            : base()
        {
            Trk trk0 = GetTrk0(blockMsDuration, tombeau1TemplateTrk, transformationPercent, trk0InitialDelay, MidiChannelIndexPerOutputVoice[0]);
            Trk trk1 = GetTrk1(blockMsDuration, tombeau1TemplateTrk, transformationPercent, MidiChannelIndexPerOutputVoice[1]);

            List<Trk> level3Trks = new List<Trk>();
            level3Trks.Add(trk0);
            level3Trks.Add(trk1);

            Seq seq = new Seq(0, level3Trks, MidiChannelIndexPerOutputVoice);

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

        private Trk GetTrk0(int blockMsDuration, Tombeau1TemplateTrk tombeau1TemplateTrk, double transformationPercent, int trk0InitialDelay, int midiChannel)
        {
            Tombeau1TemplateTrk trk0 = tombeau1TemplateTrk.Clone();

            List<byte> velocityPerAbsolutePitch = ((MidiChordDef)trk0[0]).Gamut.GetVelocityPerAbsolutePitch(25, true);

            trk0.MidiChannel = 0;
            trk0.TransposeInGamut(8);
            trk0.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, transformationPercent);

            trk0.MsDuration = blockMsDuration - trk0InitialDelay;
            ((MidiChordDef)trk0[0]).PanMsbs = new List<byte>() { 0 };
            if(trk0InitialDelay > 0)
            {
                trk0.Insert(0, new RestDef(0, trk0InitialDelay));
            }

            return trk0;
        }

        private Trk GetTrk1(int blockMsDuration, Tombeau1TemplateTrk tombeau1TemplateTrk, double transformationPercent, int midiChannel)
        {
            Tombeau1TemplateTrk trk1 = tombeau1TemplateTrk.Clone();
            trk1.MidiChannel = 1;
            trk1.MsDuration = blockMsDuration;
            ((MidiChordDef)trk1[0]).PanMsbs = new List<byte>() { 127 };

            return trk1;
        }
    }
}
