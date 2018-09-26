using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
	public class RegionDef
	{
		/// <summary>
		/// A region in the score that will be played as a continuous sequence.
		/// </summary>
		public RegionDef(string name, Tuple<int, int> startBarline, Tuple<int, int> endBarline)
		{
			this.name = name;
			startBarIndex = startBarline.Item1;
			startMsPos = startBarline.Item2;
			endBarIndex = endBarline.Item1;
			endMsPos = endBarline.Item2;
			Debug.Assert(startBarIndex >= 0 && endBarIndex > startBarIndex);
			Debug.Assert(startMsPos >= 0 && endMsPos > startMsPos);
		}

		public void WriteSVG(SvgWriter w)
		{
			w.WriteStartElement("regionDef");
			w.WriteAttributeString("class", "regionDef");
			w.WriteAttributeString("name", name);
			w.WriteAttributeString("fromStartOfBar", (startBarIndex + 1).ToString());
			w.WriteAttributeString("startMsPos", startMsPos.ToString());
			w.WriteAttributeString("toEndOfBar", (endBarIndex + 1).ToString());
			w.WriteAttributeString("endMsPos", endMsPos.ToString());
			w.WriteEndElement();
		}

		public readonly string name;
		public readonly int startBarIndex;
		public readonly int startMsPos;
		public readonly int endBarIndex;
		public readonly int endMsPos;
	}
}
