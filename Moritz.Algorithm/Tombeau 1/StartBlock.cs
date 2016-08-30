using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	public partial class Tombeau1Algorithm : CompositionAlgorithm
	{
        private Block StartBlock(List<List<Trk>> TTTrks)
        {
            List<int> barlineMsPositionsReBlock = new List<int>();

            int midiChannel = 1;
            Trk trk1a = StartBlockGetChannelTrk(midiChannel++, TTTrks[0][0]);
            trk1a.AdjustVelocitiesHairpin(0, trk1a.EndMsPositionReFirstIUD, 0.1, 1);
            MidiChordDef lastTrk0MidiChordDef = (MidiChordDef) trk1a[trk1a.Count - 1];
            lastTrk0MidiChordDef.BeamContinues = false;

            Gamut gamut = ((MidiChordDef)trk1a[0]).Gamut;

            int initialDelay = 1500;
            Trk trk0a = trk1a.Clone();
            trk0a.MidiChannel = 0; // N.B. midichannel constructed out of order.
            trk0a.TransposeInGamut(8);
            trk0a.MsDuration = trk1a.MsDuration - (initialDelay / 2);

            barlineMsPositionsReBlock.Add(trk1a.MsDuration);

            Trk trk0b = trk0a.Clone();           
            ((MidiChordDef)trk0a[0]).PanMsbs = new List<byte>() { 0 };
            trk0a.Insert(0, new RestDef(0, initialDelay));

            Trk trk1b = trk1a.Clone();
            ((MidiChordDef)trk1a[0]).PanMsbs = new List<byte>() { 127 };

            trk0a.AddRange(trk0b);
            trk1a.AddRange(trk1b);

            List<Trk> trks = new List<Trk>();
            trks.Add(trk0a);
            trks.Add(trk1a);

            barlineMsPositionsReBlock.Add(trk1a.MsDuration);

            Seq startSeq = new Seq(0, trks, MidiChannelIndexPerOutputVoice);

            Block startBlock = new Block(startSeq, barlineMsPositionsReBlock);

            return startBlock;
        }

        private Trk StartBlockGetChannelTrk(int midiChannel, Trk trkArg)
        {
            Trk trk = trkArg.Clone();
            trk.MidiChannel = midiChannel;
            Trk tt1 = trk.Clone();
            tt1.TransposeInGamut(2);
            Trk tt2 = tt1.Clone();
            tt2.TransposeInGamut(1);
            Trk tt3 = tt2.Clone();
            tt3.TransposeInGamut(2);
            Trk tt4 = tt3.Clone();
            tt4.TransposeInGamut(2);
            Trk tt5 = tt4.Clone();
            tt5.TransposeInGamut(2);
            Trk tt6 = tt5.Clone();
            tt6.TransposeInGamut(1);

            tt1.Permute(1, 7);
            tt3.Permute(1, 7);
            tt5.Permute(1, 7);

            trk.AddRange(tt1);
            trk.AddRange(tt2);
            trk.AddRange(tt3);
            trk.AddRange(tt4);
            trk.AddRange(tt5);
            trk.AddRange(tt6);
            trk.MsDuration = 6500;

            return trk;
        }
    }
}
