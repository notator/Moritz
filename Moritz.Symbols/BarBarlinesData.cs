using Moritz.Spec;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Moritz.Symbols
{
	public class BarBarlinesData
	{
		/// <summary>
		/// The LeftBarline and RightBarline attributes are 2-Value Tuples containing the indices and msPositions of the bar's enclosing Barlines.
		/// </summary>
		public BarBarlinesData(List<Bar> bars, int barIndex)
		{
			Debug.Assert(barIndex >= 0 && barIndex < bars.Count);

			int leftBarlineIndex = barIndex;
			int rightBarlineIndex = barIndex + 1;

			int leftBarlineMsPos = 0;
			for(int i = 0; i < barIndex; ++i)
			{
				leftBarlineMsPos += bars[i].MsDuration;
			}
			int rightBarlineMsPos = leftBarlineMsPos + bars[barIndex].MsDuration;

			LeftBarline = new Tuple<int, int>(leftBarlineIndex, leftBarlineMsPos);
			RightBarline = new Tuple<int, int>(rightBarlineIndex, rightBarlineMsPos);
		}

		public readonly Tuple<int, int> LeftBarline;
		public readonly Tuple<int, int> RightBarline;	
	}
}
