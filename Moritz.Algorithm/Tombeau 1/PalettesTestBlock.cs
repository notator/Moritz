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
        private Block PalettesTestBlock(string prefix, int firstPaletteNr, int lastPaletteNr)
        {
            List<Trk> trks = new List<Trk>();

            int midiChannel = 0;
            for(int i = firstPaletteNr; i <= lastPaletteNr; ++i)
            {
                string paletteName = prefix + i.ToString();
                Palette palette = GetPaletteByName(paletteName);
                List<IUniqueDef> sys1mcds = PaletteMidiChordDefs(palette);
                Trk trk = new Trk(midiChannel++, 0, sys1mcds);
                trks.Add(trk);
            }

            Seq seq = new Seq(0, trks, MidiChannelIndexPerOutputVoice); // The Seq's MsPosition can change again later.

            Block block = new Block(seq, new List<int>() { seq.MsDuration });

            return block;
        }
    }
}
