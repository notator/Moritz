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
		public RegionDef(string name, (int index, int msPosition) startBarline, (int index, int msPosition) endBarline)
		{
			this.name = name;
			startBarlineIndex = startBarline.index;
			startBarlineMsPos = startBarline.msPosition;
			endBarlineIndex = endBarline.index;
			endBarlineMsPos = endBarline.msPosition;
			Debug.Assert(startBarlineIndex >= 0 && endBarlineIndex > startBarlineIndex);
			Debug.Assert(startBarlineMsPos >= 0 && endBarlineMsPos > startBarlineMsPos);
		}

		public void WriteSVG(SvgWriter w)
		{
			w.WriteStartElement("regionDef");
			w.WriteAttributeString("class", "regionDef");
			w.WriteAttributeString("name", name);
			w.WriteAttributeString("fromStartOfBar", (startBarlineIndex + 1).ToString());
			w.WriteAttributeString("startMsPos", startBarlineMsPos.ToString());
			w.WriteAttributeString("toEndOfBar", (endBarlineIndex + 1).ToString());
			w.WriteAttributeString("endMsPos", endBarlineMsPos.ToString());
			w.WriteEndElement();
		}

		public readonly string name;
		public readonly int startBarlineIndex;
		public readonly int startBarlineMsPos;
		public readonly int endBarlineIndex;
		public readonly int endBarlineMsPos;
	}
}
