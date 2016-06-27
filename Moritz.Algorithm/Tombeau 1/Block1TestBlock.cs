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
            Block displayBlock = GetDisplayBlock(Tombeau1Templates.PitchWheelCoreMidiChordDefs, Tombeau1Templates.OrnamentCoreMidiChordDefs);

            return displayBlock;
        }

        #region GetDisplayBlock()
        private Block GetDisplayBlock(List<List<MidiChordDef>> pitchWheelCoreMidiChordDefs, List<List<MidiChordDef>> ornamentCoreMidiChordDefs)
        {
            Block block = GetBlockFromMidiChordDefLists(pitchWheelCoreMidiChordDefs);

            Envelope envelope = new Envelope(new List<byte>() { 0, 127 }, 127, 127, 2);

            block.SetPitchWheelSliders(envelope);

            Block block2 = GetBlockFromMidiChordDefLists(ornamentCoreMidiChordDefs);

            block.Concat(block2);

            return block;
        }

        private Block GetBlockFromMidiChordDefLists(List<List<MidiChordDef>> pitchWheelCoreMidiChordDefs)
        {
            int midiChannel = 0;
            List<Trk> trks = new List<Trk>();
            foreach(List<MidiChordDef> pwmcdList in pitchWheelCoreMidiChordDefs)
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
