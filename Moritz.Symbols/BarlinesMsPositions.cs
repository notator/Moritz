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
		/// Saves the barIndex and the msPositions of the bar's enclosing Barlines.
		/// </summary>
		public BarlinesMsPositions(List<Bar> bars, int barIndex)
		{
			Debug.Assert(barIndex >= 0 && barIndex < bars.Count);
			this.barIndex = barIndex;

			leftBarlineMsPos = 0;
			for(int i = 0; i < barIndex; ++i)
			{
				leftBarlineMsPos += bars[i].MsDuration;
			}
			rightBarlineMsPos = leftBarlineMsPos + bars[barIndex].MsDuration;
		}

		public readonly int barIndex;
		public readonly int leftBarlineMsPos;
		public readonly int rightBarlineMsPos;
	}
}
