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
        private Block HarmonicVelocityChordsTestBlock()
        {
            List<Trk> trks = new List<Trk>();

            List<Block> blocks = new List<Block>();

            blocks.Add(HarmonicVelocityTestBlock(majorCircularPalette));
            blocks.Add(HarmonicVelocityTestBlock(minorCircularPalette));

            Block mainBlock = blocks[0];
            for(int i = 1; i < blocks.Count; ++i)
            {
                mainBlock.Concat(blocks[i]);
            }

            return mainBlock;           
        }

        private Block HarmonicVelocityTestBlock(List<MidiChordDef> staffMidiChordDefs)
        {
            List<int> pitchHierarchy = circularPitchHierarchies[0];

            List<Trk> trks = new List<Trk>();
            for(int vfIndex = 0; vfIndex < velocityFactors.Count; ++vfIndex)
            {
                Trk trk = new Trk(vfIndex);
                List<int> velocityPerAbsolutePitch =
                        GetVelocityPerAbsolutePitch(0,    // The base pitch for the pitch hierarchy.
                        127,  // the velocity given to any absolute base pitch (if it exists) in the MidiChordDef
                        pitchHierarchy, // the pitch hierarchy for the chords in the block,
                        velocityFactors[vfIndex]  // A list of 12 values in descending order, each value in range 1..0
                       );

                foreach(MidiChordDef paletteMcd in staffMidiChordDefs)
                {
                    MidiChordDef mcd = ((MidiChordDef)paletteMcd.Clone());
                    mcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);

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

        private List<MidiChordDef> UpsideDownChords(List<MidiChordDef> midiChordDefs, int density)
        {
            List<MidiChordDef> minor = new List<MidiChordDef>();
            foreach(MidiChordDef mcd in midiChordDefs)
            {
                List<byte> intervals = new List<byte>();

                for(int i = 1; i < density; ++i)
                {
                    intervals.Add((byte)(mcd.NotatedMidiPitches[i] - mcd.NotatedMidiPitches[i - 1]));
                }
                intervals.Reverse();
                List<byte> pitches = new List<byte>() { mcd.NotatedMidiPitches[0] };
                List<byte> velocities = new List<byte>() { mcd.NotatedMidiVelocities[0] }; ;
                for(int i = 0; i < intervals.Count; ++i)
                {
                    byte interval = intervals[i];
                    pitches.Add((byte)(pitches[pitches.Count - 1] + interval));
                    velocities.Add(mcd.NotatedMidiVelocities[i]);
                }

                MidiChordDef mcdInverted = new MidiChordDef(pitches, velocities, mcd.MsDuration, true);

                minor.Add(mcdInverted);
            }
            return minor;
        }



    }
}
