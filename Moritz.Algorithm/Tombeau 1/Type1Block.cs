using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
    public class Type1Block : Block
	{
        public Type1Block(int blockMsDuration, Level2TemplateTrk level2TemplateTrk, double transformationPercent, int trk0InitialDelay, int nBars,
            IReadOnlyList<int> MidiChannelIndexPerOutputVoice)
            : base()
        {
            Level3Trk level3trk0 = GetLevel3Trk0(blockMsDuration, level2TemplateTrk, transformationPercent, trk0InitialDelay, MidiChannelIndexPerOutputVoice[0]);
            Level3Trk level3trk1 = GetLevel3Trk1(blockMsDuration, level2TemplateTrk, transformationPercent, MidiChannelIndexPerOutputVoice[1]);

            List<Trk> level3Trks = new List<Trk>();
            level3Trks.Add(level3trk0);
            level3Trks.Add(level3trk1);

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

        private Level3Trk GetLevel3Trk0(int blockMsDuration, Level2TemplateTrk level2TemplateTrk, double transformationPercent, int trk0InitialDelay, int midiChannel)
        {
            Level2TemplateTrk trk0 = level2TemplateTrk.Clone();

            List<byte> velocityPerAbsolutePitch = ((MidiChordDef)trk0[0]).Gamut.GetVelocityPerAbsolutePitch(25, true);

            trk0.MidiChannel = 0;
            trk0.TransposeInGamut(8);
            //trk0.SetVelocitiesFromDurations(75, 127, 40);
            //trk0.AdjustVelocitiesHairpin(0, trk0.EndMsPositionReFirstIUD, 0.1, 1);
            trk0.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch, transformationPercent);

            List<Level2TemplateTrk> level2TemplateTrks = new List<Tombeau1.Level2TemplateTrk>() { trk0 };
            Level3Trk level3trk0 = new Level3Trk(midiChannel, 0, level2TemplateTrks, transformationPercent);

            level3trk0.MsDuration = blockMsDuration - trk0InitialDelay;
            ((MidiChordDef)level3trk0[0]).PanMsbs = new List<byte>() { 0 };
            if(trk0InitialDelay > 0)
            {
                level3trk0.Insert(0, new RestDef(0, trk0InitialDelay));
            }

            return level3trk0;
        }

        private Level3Trk GetLevel3Trk1(int blockMsDuration, Level2TemplateTrk level2TemplateTrk, double transformationPercent, int midiChannel)
        {
            Level2TemplateTrk trk1 = level2TemplateTrk.Clone();

            List<byte> velocityPerAbsolutePitch = ((MidiChordDef)trk1[0]).Gamut.GetVelocityPerAbsolutePitch(25, true);
            trk1.MidiChannel = 1;
            //trk1.TransposeInGamut(7);
            //trk1.AdjustVelocitiesHairpin(0, trk1.EndMsPositionReFirstIUD, 0.1, 1);

            //Level2TemplateTrk trk1a = level2TemplateTrk.Clone();
            //trk1a.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
            //trk1a.AdjustVelocitiesHairpin(0, trk1a.EndMsPositionReFirstIUD, 0.5, 1.5);

            List<Level2TemplateTrk> level2TemplateTrks = new List<Level2TemplateTrk>() { trk1 /*, trk1a */ };
            Level3Trk level3trk1 = new Level3Trk(midiChannel, 0, level2TemplateTrks, transformationPercent);

            level3trk1.MsDuration = blockMsDuration;
            ((MidiChordDef)level3trk1[0]).PanMsbs = new List<byte>() { 127 };

            return level3trk1;
        }
    }
}
