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
        private Block Block1TestBlock()
        {
            // Each of the Tombeau1Templates objects is cloned automatically. 
            Block displayBlock = GetDisplayBlock(Tombeau1Templates.PitchWheelTestMidiChordDefs,
                                                Tombeau1Templates.OrnamentTestMidiChordDefs,
                                                Tombeau1Templates.Trks);

            return displayBlock;
        }

        #region GetDisplayBlock()
        private Block GetDisplayBlock(List<List<MidiChordDef>> pitchWheelCoreMidiChordDefs,
                                    List<List<MidiChordDef>> ornamentCoreMidiChordDefs,
                                    List<List<Trk>> TTTrks)
        {
            Block bar1 = GetBarFromTTTrks(TTTrks);

            Block bar2 = GetBarFromMidiChordDefLists(pitchWheelCoreMidiChordDefs);

            Envelope envelope = new Envelope(new List<byte>() { 0, 127 }, 127, 127, 2);

            bar2.SetPitchWheelSliders(envelope);

            Block bar3 = GetBarFromMidiChordDefLists(ornamentCoreMidiChordDefs);

            bar1.Concat(bar2);
            bar1.Concat(bar3);

            return bar1;
        }

        private Block GetBarFromTTTrks(List<List<Trk>> TTTrks)
        {
            int midiChannel = 0;
            List<Trk> trks = new List<Trk>();

            if(TTTrks != null)
            {
                Trk tt0 = TTTrks[0][0];
                tt0.MidiChannel = midiChannel++;
                Gamut gamut = (tt0.UniqueDefs[0] as MidiChordDef).Gamut;
                Trk tt1 = tt0.Clone();
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

                tt4.Permute(1, 7);

                tt0.AddRange(tt1);
                tt0.AddRange(tt2);
                tt0.AddRange(tt3);
                tt0.AddRange(tt4);
                tt0.AddRange(tt5);
                tt0.AddRange(tt6);
                tt0.MsDuration = 6500;
                //tt0.TransposeInGamut(13);

                tt0.AdjustVelocitiesHairpin(0, tt0.EndMsPositionReFirstIUD, 0.25, 1);

                trks.Add(tt0);
            }

            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice);
            Block block = new Block(seq, new List<int>() { seq.MsDuration });

            return block;
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
