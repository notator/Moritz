using Moritz.Spec;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Moritz.Symbols
{
	public class BarlinesMsPositions
	{
		/// <summary>
		/// Saves the indices and msPositions of the bar's enclosing Barlines.
		/// </summary>
		public BarlinesMsPositions(List<Bar> bars, int barIndex)
		{
			Debug.Assert(barIndex >= 0 && barIndex < bars.Count);
			this.leftBarlineIndex = barIndex;
			this.rightBarlineIndex = barIndex + 1;

			leftBarlineMsPos = 0;
			for(int i = 0; i < barIndex; ++i)
			{
				leftBarlineMsPos += bars[i].MsDuration;
			}
			rightBarlineMsPos = leftBarlineMsPos + bars[barIndex].MsDuration;
		}

		public readonly int leftBarlineIndex;
		public readonly int leftBarlineMsPos;
		public readonly int rightBarlineIndex;
		public readonly int rightBarlineMsPos;
	}
}
