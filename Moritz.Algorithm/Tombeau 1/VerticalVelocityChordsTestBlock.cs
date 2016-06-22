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
        private Block VerticalVelocityChordsTestBlock(List<MidiChordDef> majorCircularPalette, List<MidiChordDef> minorCircularPalette)
        {
            List<Block> blocks = new List<Block>();

            blocks.Add(VerticalVelocityTestBlock(majorCircularPalette));
            blocks.Add(VerticalVelocityTestBlock(minorCircularPalette));

            Block mainBlock = blocks[0];
            for(int i = 1; i < blocks.Count; ++i)
            {
                mainBlock.Concat(blocks[i]);
            }

            return mainBlock;           
        }

        private Block VerticalVelocityTestBlock(List<MidiChordDef> staffMidiChordDefs)
        {
            List<Trk> trks = new List<Trk>();
            List<byte> topVelocities = new List<byte>();
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.ppp]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.pp]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.p]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.mp]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.mf]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.f]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.ff]);
            topVelocities.Add(M.MaxMidiVelocity[M.Dynamic.fff]);

            for(int vfIndex = 0; vfIndex < topVelocities.Count; ++vfIndex)
            {
                Trk trk = new Trk(vfIndex);
                byte rootVelocity = M.MaxMidiVelocity[M.Dynamic.fff];
                byte topVelocity = topVelocities[vfIndex];

                foreach(MidiChordDef paletteMcd in staffMidiChordDefs)
                {
                    MidiChordDef mcd = ((MidiChordDef)paletteMcd.Clone());
                    mcd.SetVerticalVelocityGradient(rootVelocity, topVelocity);

                    trk.Add(mcd);
                }

                trks.Add(trk);
            }


            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.

            List<int> barlineMsPositions = new List<int>();
            barlineMsPositions.Add(seq.MsDuration);

            Block block = new Block(seq, barlineMsPositions);

            return block;
        }

        //private List<MidiChordDef> UpsideDown(List<MidiChordDef> midiChordDefs, int density)
        //{
        //    List<MidiChordDef> minor = new List<MidiChordDef>();
        //    foreach(MidiChordDef mcd in midiChordDefs)
        //    {
        //        List<byte> intervals = new List<byte>();

        //        for(int i = 1; i < density; ++i)
        //        {
        //            intervals.Add((byte)(mcd.NotatedMidiPitches[i] - mcd.NotatedMidiPitches[i - 1]));
        //        }
        //        intervals.Reverse();
        //        List<byte> pitches = new List<byte>() { mcd.NotatedMidiPitches[0] };
        //        List<byte> velocities = new List<byte>() { mcd.NotatedMidiVelocities[0] }; ;
        //        for(int i = 0; i < intervals.Count; ++i)
        //        {
        //            byte interval = intervals[i];
        //            pitches.Add((byte)(pitches[pitches.Count - 1] + interval));
        //            velocities.Add(mcd.NotatedMidiVelocities[i]);
        //        }

        //        MidiChordDef mcdInverted = new MidiChordDef(pitches, velocities, mcd.MsDuration, true);

        //        minor.Add(mcdInverted);
        //    }
        //    return minor;
        //}

        //private List<MidiChordDef> GetMajorPalette(List<List<int>> pitchHierarchies, int chordDensity)
        //{
        //    List<MidiChordDef> majorCircularPalette = new List<MidiChordDef>();
        //    for(int j = 0; j < pitchHierarchies.Count; ++j)
        //    {
        //        List<int> cph = pitchHierarchies[j];

        //        MidiChordDef mcd = new MidiChordDef(chordDensity, 5, cph, 127, 1200, true);
        //        mcd.Lyric = (j).ToString();
        //        majorCircularPalette.Add(mcd);
        //    }
        //    return majorCircularPalette;
        //}

        //private List<MidiChordDef> GetMajorCircularPalette(int chordDensity)
        //{
        //    List<MidiChordDef> majorCircularPalette = GetMajorPalette(circularPitchHierarchies, chordDensity);
        //    return majorCircularPalette;
        //}

        //private List<MidiChordDef> GetMajorLinearPalette(int chordDensity)
        //{
        //    List<MidiChordDef> majorLinearPalette = GetMajorPalette(linearPitchHierarchies, chordDensity);
        //    return majorLinearPalette;
        //}

    }
}
