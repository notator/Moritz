﻿using System;
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
        private Block WarpDurationsTestBlock(Block originalBlock)
        {
            Block block = originalBlock.Clone();

            // Blocks can be warped...
            List<double> warp = new List<double>() { 0, 0.1, 0.3, 0.6, 1 };
            block.WarpDurations(warp);

            return block;
        }

    }
}