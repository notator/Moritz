using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	public class Type1Block : Tombeau1Block
	{
        public Type1Block(CommonArgs commonArgs, int blockMsDuration, Trk templateTrk, int trk0InitialDelay)
            : base(commonArgs)
        {
            List<int> _barlineMsPositionsReSeq = new List<int>();

            int midiChannel = 1;
            Trk trk1a = GetChannelTrk(midiChannel++, templateTrk);
            trk1a.AdjustVelocitiesHairpin(0, trk1a.EndMsPositionReFirstIUD, 0.1, 1);
            MidiChordDef lastTrk0MidiChordDef = (MidiChordDef)trk1a[trk1a.Count - 1];
            lastTrk0MidiChordDef.BeamContinues = false;

            Trk trk0a = trk1a.Clone();
            trk0a.MidiChannel = 0; // N.B. midichannel constructed out of order.
            trk0a.TransposeInGamut(8);

            trk0a.AddRange(trk0a.Clone());
            trk0a.MsDuration = blockMsDuration - trk0InitialDelay;
            ((MidiChordDef)trk0a[0]).PanMsbs = new List<byte>() { 0 };
            trk0a.Insert(0, new RestDef(0, trk0InitialDelay));

            Trk trk1b = trk1a.Clone();
            trk1a.AddRange(trk1b);
            trk1a.MsDuration = blockMsDuration;
            ((MidiChordDef)trk1a[0]).PanMsbs = new List<byte>() { 127 };

            List<Trk> trks = new List<Trk>();
            trks.Add(trk0a);
            trks.Add(trk1a);

            _barlineMsPositionsReSeq.Add(blockMsDuration / 2);
            _barlineMsPositionsReSeq.Add(blockMsDuration);

            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice);

            FinalizeBlock(seq, _barlineMsPositionsReSeq);
        }

        private Trk GetChannelTrk(int midiChannel, Trk trkArg)
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
