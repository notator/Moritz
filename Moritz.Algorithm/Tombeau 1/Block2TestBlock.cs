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
        private Block Block2TestBlock(IReadOnlyList<IReadOnlyList<MidiChordDef>> pitchWheelMidiChordDefs,
                                    IReadOnlyList<IReadOnlyList<MidiChordDef>> ornamentMidiChordDefs)
        {

            Block b2bar1 = GetBlock2BarFromMidiChordDefLists(pitchWheelMidiChordDefs);

            Envelope envelope = new Envelope(new List<byte>() { 0, 127 }, 127, 127, 2);

            b2bar1.SetPitchWheelSliders(envelope);

            Block b2bar2 = GetBlock2BarFromMidiChordDefLists(ornamentMidiChordDefs);

            b2bar2.SetPitchWheelSliders(envelope);

            Block block1 = b2bar1;
            block1.Concat(b2bar2);

            return block1;
            //return null;
        }

        private Block GetBlock2BarFromMidiChordDefLists(IReadOnlyList<IReadOnlyList<MidiChordDef>> midiChordDefLists)
        {
            int midiChannel = 0;
            List<Trk> trks = new List<Trk>();

            foreach(List<MidiChordDef> pwmcdList in midiChordDefLists)
            {
                List<IUniqueDef> mcds = GetBlock2OrderedMidiChordDefs(pwmcdList);
                Trk trk = new Trk(midiChannel++, 0, mcds);
                trks.Add(trk);
            }

            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice);
            Block block = new Block(seq, new List<int>() { seq.MsDuration });

            return block;
        }

        private List<IUniqueDef> GetBlock2OrderedMidiChordDefs(IReadOnlyList<MidiChordDef> mcdList)
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
    }
}
