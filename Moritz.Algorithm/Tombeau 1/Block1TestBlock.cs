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
        private Block Block1TestBlock(Tombeau1Templates t1t)
        {
            // Each of the Tombeau1Templates objects is cloned automatically. 
            Block displayBlock = GetDisplayBlock(t1t.PitchWheelTestMidiChordDefs,
                                                t1t.OrnamentTestMidiChordDefs,
                                                t1t.Trks);

            return displayBlock;
        }

        #region GetDisplayBlock()
        private Block GetDisplayBlock(List<List<MidiChordDef>> pitchWheelCoreMidiChordDefs,
                                    List<List<MidiChordDef>> ornamentCoreMidiChordDefs,
                                    List<List<Trk>> TTTrks)
        {
            Block bars1and2 = GetBars1and2FromTTTrks(TTTrks);

            Block bar3 = GetBarFromMidiChordDefLists(pitchWheelCoreMidiChordDefs);

            Envelope envelope = new Envelope(new List<byte>() { 0, 127 }, 127, 127, 2);

            bar3.SetPitchWheelSliders(envelope);

            Block bar4 = GetBarFromMidiChordDefLists(ornamentCoreMidiChordDefs);

            Block block1 = bars1and2;
            block1.Concat(bar3);
            block1.Concat(bar4);

            return block1;
        }

        private Block GetBars1and2FromTTTrks(List<List<Trk>> TTTrks)
        {
            Trk channel0Trk = GetChannelTrk(0, TTTrks[0][0]); // absolutePitchHierarchy(0, 0); NPitchesPerOctave = 9
            channel0Trk.AdjustVelocitiesHairpin(0, channel0Trk.EndMsPositionReFirstIUD, 0.1, 1);

            Trk channel1Trk = GetChannelTrk(1, TTTrks[0][1]);
            channel1Trk.AdjustVelocitiesHairpin(0, channel1Trk.EndMsPositionReFirstIUD, 1, 0.1);
            List<byte> velocityPerAbsolutePitch = ((MidiChordDef)channel1Trk[0]).Gamut.GetVelocityPerAbsolutePitch(5, false);
            channel1Trk.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
            channel1Trk.TransposeInGamut(-9);
            channel1Trk.MsPositionReContainer = 231;

            List<Trk> trks = new List<Trk>();
            trks.Add(channel0Trk);
            trks.Add(channel1Trk);
            Seq seq0 = new Seq(0, trks, MidiChannelIndexPerOutputVoice);
            int barline1MsPosition = channel0Trk.MsDuration;

            Seq seq1 = seq0.Clone();
            seq1.Trks[1].Transpose(-18);
            seq1.Trks[1].InsertClefChange(0, "b");

            seq0.Concat(seq1);
            int barline2MsPosition = seq0.MsDuration;

            Block bars1and2 = new Block(seq0, new List<int>() { barline1MsPosition, barline2MsPosition });

            return bars1and2;
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

            tt4.Permute(1, 5);

            trk.AddRange(tt1);
            trk.AddRange(tt2);
            trk.AddRange(tt3);
            trk.AddRange(tt4);
            trk.AddRange(tt5);
            trk.AddRange(tt6);
            trk.MsDuration = 6500;

            return trk;
        }

        private Block GetBarFromMidiChordDefLists(List<List<MidiChordDef>> midiChordDefLists)
        {
            int midiChannel = 0;
            List<Trk> trks = new List<Trk>();

            foreach(List<MidiChordDef> pwmcdList in midiChordDefLists)
            {
                List<IUniqueDef> mcds = GetOrderedCoreMidiChordDefs(pwmcdList);
                Trk trk = new Trk(midiChannel++, 0, mcds);
                trks.Add(trk);
            }

            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice);
            Block block = new Block(seq, new List<int>() { seq.MsDuration });

            return block;
        }

        private List<IUniqueDef> GetOrderedCoreMidiChordDefs(List<MidiChordDef> mcdList)
        {
            List<IUniqueDef> rval = new List<IUniqueDef>();
            int msPositionReFirstIUD = 0;
            for(int index = 0; index < mcdList.Count; ++index)
            {
                MidiChordDef originalMcd = mcdList[index];
                MidiChordDef mcd = originalMcd.Clone() as MidiChordDef;
                mcd.Lyric = index.ToString();
                mcd.MsPositionReFirstUD = msPositionReFirstIUD;
                msPositionReFirstIUD += mcd.MsDuration;
                rval.Add(mcd);
            }
            return rval;
        }
        #endregion GetDisplayBlock()
    }
}
