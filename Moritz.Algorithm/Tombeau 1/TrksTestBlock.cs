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
        private Block TrksTestBlock()
        {
            Seq seq = new Seq(0, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.

            List<int> velocityPerAbsolutePitch =
                M.GetVelocityPerAbsolutePitch(5,    // The base pitch for the pitch hierarchy.
                                              127,  // the velocity given to any absolute base pitch (if it exists) in the MidiChordDef
                                              0,    // index in M.RelativePitchHierarchies: in range [0..21]
                                              0     // index in M.VelocityFactors: in range [0..7]
                                             );

            Palette palette = GetPaletteByName("Tombeau1.1");
            for(int i = 0; i < MidiChannelIndexPerOutputVoice.Count; ++i)
            {
                int chordDensity = 3;
                List<IUniqueDef> sys1mcds = PaletteMidiChordDefs(palette);
                List<IUniqueDef> midiChordDefs = new List<IUniqueDef>();

                for(int j = 0; j < 12; ++j)
                {
                    MidiChordDef mcd = sys1mcds[j] as MidiChordDef;
                    midiChordDefs.Add(mcd);

                    mcd.Transpose(i + j - 7);
                    mcd.Lyric = (j).ToString() + "." + chordDensity.ToString();

                    mcd.SetVerticalDensity(chordDensity);

                    mcd.SetVelocityPerAbsolutePitch(velocityPerAbsolutePitch);
                }
                Trk trk = new Trk(MidiChannelIndexPerOutputVoice[i], 0, midiChordDefs);
                trk.SortVelocityIncreasing();
                //trk.SortVelocityDecreasing();
                //trk.SortRootNotatedPitchAscending();
                //trk.SortRootNotatedPitchDescending();
                //trk.Permute(0, new List<int>() { 1,1,1,1,1,1,2 }, i + 1, 7);
                seq.SetTrk(trk);
            }

            //systemSeq.AlignTrkUniqueDefs(new List<int>() { 0,1,2,3,4,5,6,7 });
            //systemSeq.ShiftTrks(new List<int>() { 0, 100, 200, 300, 400, 500, 600, 700 });

            //Trk trk2 = systemSeq.Trks[2];
            //trk2.MsPositionReContainer = -500;
            //trk2.SortRootNotatedPitchDescending();
            //trk2.Insert(4, new RestDef(0, 444));

            /******/

            for(int i = 0; i < seq.Trks.Count; ++i)
            {
                Trk trk = seq.Trks[i];
                trk.Permute(i + 1, 7); // sets trk.AxisIndex
            }

            //for(int i = 0; i < systemSeq.Trks.Count; ++i)
            //{
            //    Trk trk = systemSeq.Trks[i];
            //    trk.AxisUDIndex = i;
            //}

            seq.AlignTrkAxes();

            /*******/

            Trk trk3 = seq.Trks[3];
            trk3.PermutePartitions(7, 1, new List<int>() { 2, 2, 1, 2, 2, 1, 2 });
            //Trk trk4 = systemSeq.Trks[4];
            //trk4.SortVelocityDecreasing();

            seq.Normalize();

            // Implemented and tested these: (They just call the corresponding function on all the Trks in the seq.)
            //systemSeq.SortVelocityIncreasing();
            //systemSeq.SortVelocityDecreasing();
            //systemSeq.SortRootNotatedPitchAscending();
            //systemSeq.SortRootNotatedPitchDescending();



            // Can something like this be done? Using the Alignment positions?
            //systemSeq.Permute(0, new List<int>() { 1,1,1,1,1,1,2 }, i + 1, 7);

            Block block = new Block(seq, new List<int>() { seq.MsDuration });

            return block;
        }

    }
}
