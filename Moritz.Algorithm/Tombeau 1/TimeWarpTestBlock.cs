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
        private Block TimeWarpTestBlock(Block originalBlock)
        {
            Block block = originalBlock.Clone();

            double distortion = 32;
            block.TimeWarp(new Envelope(new List<byte> { 0, 127, 0 }), distortion);

            return block;
        }

    }
}
