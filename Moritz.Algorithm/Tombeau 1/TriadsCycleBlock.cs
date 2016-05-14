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
        private Tuple<Block, List<int>> TriadsCycleBlock()
        {
            Palette triads1Palette = GetPaletteByName("triads1");
            Palette triads2Palette = GetPaletteByName("triads2");
            Palette triads1widePalette = GetPaletteByName("triads1.wide");
            Palette triads2widePalette = GetPaletteByName("triads2.wide");

            List<IUniqueDef> triadsCycle = new List<IUniqueDef>();
            List<IUniqueDef> triads1 = new List<IUniqueDef>();
            for(int i = 0; i < 3; ++i)
            {
                MidiChordDef mcd = triads1Palette.MidiChordDef(i);
                mcd.MsPositionReFirstUD = 0;
                triads1.Add(mcd);
            }
            List<IUniqueDef> triads2 = new List<IUniqueDef>();
            for(int i = 0; i < 3; ++i)
            {
                MidiChordDef mcd = triads2Palette.MidiChordDef(i);
                mcd.MsPositionReFirstUD = 0;
                triads2.Add(mcd);
            }
            List<IUniqueDef> triads1wide = new List<IUniqueDef>();
            for(int i = 0; i < 3; ++i)
            {
                MidiChordDef mcd = triads1widePalette.MidiChordDef(i);
                mcd.MsPositionReFirstUD = 0;
                triads1wide.Add(mcd);
            }
            List<IUniqueDef> triads2wide = new List<IUniqueDef>();
            for(int i = 0; i < 3; ++i)
            {
                MidiChordDef mcd = triads2widePalette.MidiChordDef(i);
                mcd.MsPositionReFirstUD = 0;
                triads2wide.Add(mcd);
            }
            Trk trk = new Trk(0);
            for(int i = 0; i < 2; ++i)
            {
                trk.Add(triads1[1].Clone());
                trk.Add(triads1[0].Clone());
                trk.Add(triads2[0].Clone());
                trk.Add(triads2[1].Clone());
                trk.Add(triads2[2].Clone());
                trk.Add(triads1[2].Clone());

                trk.Add(triads2wide[0].Clone());
                trk.Add(triads1wide[0].Clone());
                trk.Add(triads1wide[1].Clone());
                trk.Add(triads1wide[2].Clone());
                trk.Add(triads2wide[2].Clone());
                trk.Add(triads2wide[1].Clone());
            }
            List<Trk> sys1Trks = new List<Trk>() { trk };
            //Trk ch1Trk = trk.Clone();
            //ch1Trk.MidiChannel = 1;
            //sys1Trks.Add(ch1Trk);
            Seq seq = new Seq(0, sys1Trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.

            Block block = new Block(seq);
            List<int> barlineMsPositions = new List<int>() { block.MsDuration };             
            return new Tuple<Block, List<int>>(block, barlineMsPositions);
        }
    }
}
