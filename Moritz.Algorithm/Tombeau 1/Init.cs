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
        private List<List<MidiChordDef>> PitchWheelCoreMidiChordDefs = new List<List<MidiChordDef>>();
        private List<List<MidiChordDef>> OrnamentCoreMidiChordDefs = new List<List<MidiChordDef>>();

        /// <summary>
        /// Sets up the standard MidiChordDefs and Trks that will be used in the composition.
        /// </summary>
        private Block Init()
        {
            int relativePitchHierarchyIndex = 10;
            foreach(List<List<byte>> envList in Tombeau1Statics.Envelopes)
            {
                List<MidiChordDef> pwmcds = GetPitchWheelCoreMidiChordDefs(envList);
                PitchWheelCoreMidiChordDefs.Add(pwmcds);

                List<int> absolutePitchHierarchy = M.GetAbsolutePitchHierarchy(relativePitchHierarchyIndex++, 7);
                Gamut gamut = new Gamut(absolutePitchHierarchy, 8);

                List<MidiChordDef> omcds = GetOrnamentCoreMidiChordDefs(envList, gamut);
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
                mcd.SetPitchWheelEnvelope(new Envelope(envelope, 127, 127, envelope.Count));
                rval.Add(mcd);
            }
            return rval;
        }

        private List<MidiChordDef> GetOrnamentCoreMidiChordDefs(List<List<byte>> envList, Gamut gamut)
        {
            List<MidiChordDef> rval = new List<MidiChordDef>();

            foreach(List<byte> envelope in envList)
            {
                Envelope env = new Envelope(envelope, 127, 8, 10);
                List<int> basicMidiChordRootPitches = null;

                int firstPitch = 60;
                if(gamut.IndexOf(firstPitch) >= 0)
                {
                    basicMidiChordRootPitches = env.PitchSequence(firstPitch, gamut);
                }
                else
                {
                    basicMidiChordRootPitches = new List<int>() { firstPitch };
                }

                MidiChordDef mcd = new MidiChordDef(1000, basicMidiChordRootPitches);

                if(basicMidiChordRootPitches.Count > 1)
                {
                    Envelope timeWarpEnvelope = new Envelope(new List<byte>() { 127, 64, 127 }, 127, 127, 3);
                    mcd.TimeWarp(timeWarpEnvelope, 16);
                }

                rval.Add(mcd);
            }
            return rval;
        }

        #region GetDisplayBlock()
        private Block GetDisplayBlock(List<List<MidiChordDef>> pitchWheelCoreMidiChordDefs, List<List<MidiChordDef>> ornamentCoreMidiChordDefs)
        {
            Block block = GetBlockFromMidiChordDefLists(pitchWheelCoreMidiChordDefs);

            Envelope envelope = new Envelope(new List<byte>() { 0, 127 }, 127, 127, 2);

            block.SetPitchWheelSliderEnvelope(envelope);

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
