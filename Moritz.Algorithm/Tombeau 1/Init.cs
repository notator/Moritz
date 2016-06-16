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
        #region envelopes
        private static List<List<byte>> Envelopes2 = new List<List<byte>>()
        {
            { new List<byte>() {64, 0} },
            { new List<byte>() {64, 18} },
            { new List<byte>() {64, 36} },
            { new List<byte>() {64, 54} },
            { new List<byte>() {64, 72} },
            { new List<byte>() {64, 91} },
            { new List<byte>() {64, 109} },
            { new List<byte>() {64, 127} }
        };
        private static List<List<byte>> Envelopes3 = new List<List<byte>>()
        {
            { new List<byte>() {64, 0, 64} },
            { new List<byte>() {64, 18, 64} },
            { new List<byte>() {64, 36, 64} },
            { new List<byte>() {64, 54, 64} },
            { new List<byte>() {64, 72, 64} },
            { new List<byte>() {64, 91, 64} },
            { new List<byte>() {64, 109, 64} },
            { new List<byte>() {64, 127, 64} }
        };
        private static List<List<byte>> Envelopes4 = new List<List<byte>>()
        {
            { new List<byte>() {64, 0, 64, 64} },
            { new List<byte>() {64, 22, 64, 64} },
            { new List<byte>() {64, 22, 96, 64} },
            { new List<byte>() {64, 64, 0, 64} },
            { new List<byte>() {64, 64, 22, 64} },
            { new List<byte>() {64, 64, 80, 64} },
            { new List<byte>() {64, 80, 64, 64} },
            { new List<byte>() {64, 96, 22, 64 } }
        };
        private static List<List<byte>> Envelopes5 = new List<List<byte>>()
        {
            { new List<byte>() {64, 50, 72, 50, 64} },
            { new List<byte>() {64, 64, 0, 64, 64} },
            { new List<byte>() {64, 64, 64, 80, 64} },
            { new List<byte>() {64, 64, 64, 106, 64} },
            { new List<byte>() {64, 64, 127, 64, 64} },
            { new List<byte>() {64, 70, 35, 105, 64} },
            { new List<byte>() {64, 72, 50, 70, 64} },
            { new List<byte>() {64, 80, 64, 64, 64} },
            { new List<byte>() {64, 105, 35, 70, 64} },
            { new List<byte>() {64, 106, 64, 64, 64} }
        };
        private static List<List<byte>> Envelopes6 = new List<List<byte>>()
        {
            { new List<byte>() {64, 22, 43, 64, 64, 64} },
            { new List<byte>() {64, 30, 78, 64, 40, 64} },
            { new List<byte>() {64, 40, 64, 78, 30, 64} },
            { new List<byte>() {64, 43, 106, 64, 64, 64} },
            { new List<byte>() {64, 64, 64, 43, 22, 64} },
            { new List<byte>() {64, 64, 64, 64, 106, 64} },
            { new List<byte>() {64, 64, 64, 64, 127, 64} },
            { new List<byte>() {64, 64, 64, 106, 43, 64} },
            { new List<byte>() {64, 106, 64, 64, 64, 64} },
            { new List<byte>() {64, 127, 127, 22, 64, 64} }
        };
        private static List<List<byte>> Envelopes7 = new List<List<byte>>()
        {
            { new List<byte>() {64, 0, 0, 106, 106, 64, 64} },
            { new List<byte>() {64, 28, 68, 48, 108, 88, 64} },
            { new List<byte>() {64, 40, 20, 80, 60, 100, 64} },
            { new List<byte>() {64, 55, 50, 75, 50, 64, 64} },
            { new List<byte>() {64, 64, 64, 64, 64, 32, 64} },
            { new List<byte>() {64, 64, 50, 75, 50, 55, 64} },
            { new List<byte>() {64, 73, 78, 53, 78, 64, 64} },
            { new List<byte>() {64, 85, 64, 106, 64, 127, 64} },
            { new List<byte>() {64, 88, 108, 48, 68, 28, 64} },
            { new List<byte>() {64, 100, 60, 80, 20, 40, 64} },
            { new List<byte>() {64, 127, 127, 64, 64, 64, 64} }
        };
        private static List<List<byte>> EnvelopesLong = new List<List<byte>>()
        {
            { new List<byte>() {64, 0, 64, 96, 127, 30, 0, 64} },
            { new List<byte>() {64, 64, 64, 127, 64, 106, 43, 64} },
            { new List<byte>() {64, 64, 43, 43, 64, 64, 85, 22, 64} },
            { new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 64} },
            { new List<byte>() {64, 80, 64, 92, 64, 64, 64, 98, 64} },
            { new List<byte>() {64, 98, 64, 64, 64, 92, 64, 80, 64} },
            { new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 127, 0, 64, 64} },
            { new List<byte>() {64, 64, 64, 64, 64, 64, 64, 64, 64, 100, 50, 100} },
            { new List<byte>() {64, 64, 64, 64, 64, 64, 64, 64, 64, 127, 43, 127, 64} },
            { new List<byte>() {64, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} },
            { new List<byte>() {64, 64, 64, 64, 64, 64, 64, 127, 43, 127, 64, 127, 43, 127, 64} },
            { new List<byte>() {64, 127, 43, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} },
            { new List<byte>() {64, 64, 64, 0, 64, 127, 0, 64, 127, 64, 0, 64, 127, 64, 0, 64} },
            { new List<byte>() {64, 127, 64, 64, 0, 64, 127, 0, 64, 127, 64, 0, 64, 127, 64, 0, 64} },
            { new List<byte>() {64, 127, 43, 127, 43, 127, 64, 127, 43, 127, 43, 127, 64, 64, 64, 64, 64, 64, 64, 64, 64} }
        };
        #endregion envelopes

        private List<List<MidiChordDef>> PitchWheelCoreMidiChordDefs = new List<List<MidiChordDef>>();
        private List<List<MidiChordDef>> OrnamentCoreMidiChordDefs = new List<List<MidiChordDef>>();

        /// <summary>
        /// Sets up the standard MidiChordDefs and Trks that will be used in the composition.
        /// </summary>
        private Block Init()
        {
            List<List<List<byte>>> envelopes = new List<List<List<byte>>>()
            {
                Envelopes2, Envelopes3, Envelopes4, Envelopes5, Envelopes6, Envelopes7, EnvelopesLong
            };

            int cphIndex = 10;
            foreach(List<List<byte>> envList in envelopes)
            {
                List<MidiChordDef> pwmcds = GetPitchWheelCoreMidiChordDefs(envList);
                PitchWheelCoreMidiChordDefs.Add(pwmcds);
                List<MidiChordDef> omcds = GetOrnamentCoreMidiChordDefs(envList, circularPitchHierarchies[cphIndex++]);
                OrnamentCoreMidiChordDefs.Add(omcds);
            }

            Block displayBlock = GetDisplayBlock(PitchWheelCoreMidiChordDefs, OrnamentCoreMidiChordDefs);

            return displayBlock;
        }


        private List<MidiChordDef> GetPitchWheelCoreMidiChordDefs(List<List<byte>> envList)
        {
            List<MidiChordDef> rval = new List<MidiChordDef>();
            foreach(List<byte> envelope in envList)
            {
                MidiChordDef mcd = new MidiChordDef(new List<byte>() { 60 }, new List<byte>() { 127 }, 1000, true);
                mcd.SetPitchWheelSliderEnvelope(new Envelope(envelope, 127));
                rval.Add(mcd);
            }
            return rval;
        }

        private List<MidiChordDef> GetOrnamentCoreMidiChordDefs(List<List<byte>> envList, List<int> circularPitchHierarchy)
        {
            List<MidiChordDef> rval = new List<MidiChordDef>();
            foreach(List<byte> envelope in envList)
            {
                MidiChordDef mcd = new MidiChordDef(new List<byte>() { 60 }, new List<byte>() { 127 }, 1000, true);
                mcd.SetOrnament(new Envelope(envelope, 127), circularPitchHierarchy, 20, 8);
                rval.Add(mcd);
            }
            return rval;
        }

        #region GetDisplayBlock()
        private Block GetDisplayBlock(List<List<MidiChordDef>> pitchWheelCoreMidiChordDefs, List<List<MidiChordDef>> ornamentCoreMidiChordDefs)
        {
            Block block = GetBlockFromMidiChordDefLists(pitchWheelCoreMidiChordDefs);
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
