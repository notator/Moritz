using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
	public class Regions
	{
		public Regions(IReadOnlyList<RegionDef> regionDefs, string regionSequence)
		{
			Debug.Assert(!(regionDefs == null || regionDefs.Count == 0));
			Debug.Assert(!String.IsNullOrEmpty(regionSequence));
			Debug.Assert(regionDefs[0].startMsPos == 0);

			for(int i = 0; i < regionDefs.Count - 1; ++i)
			{
				for(int j = i + 1; j < regionDefs.Count; ++j)
				{
					Debug.Assert(regionDefs[i].name != regionDefs[j].name);
				}
			}

			this.regionDefs = regionDefs;
			this.regionSequence = regionSequence;
		}

		/// <summary>
		/// Writes a regions element
		/// </summary>
		public void WriteSVG(SvgWriter w)
		{
			w.WriteStartElement("regions");

			w.WriteStartElement("defs");
			foreach(RegionDef regionDef in regionDefs)
			{
				regionDef.WriteSVG(w);
			}
			w.WriteEndElement();

			w.WriteStartElement("regionSequence");
			w.WriteAttributeString("class", "regionSequence");
			w.WriteAttributeString("sequence", regionSequence);
			w.WriteEndElement();

			w.WriteEndElement(); // end regions            
		}

		private IReadOnlyList<RegionDef> regionDefs;
		private readonly string regionSequence;
	}

	public class RegionDef
	{
		/// <summary>
		/// A region in the score that will be played as a continuous sequence.
		/// </summary>
		public RegionDef(char name, BarlinesMsPositions startBarBarlinePositions, BarlinesMsPositions endBarBarlinePositions)
		{
			int startBarIndex = startBarBarlinePositions.barIndex;
			int startMsPos = startBarBarlinePositions.leftBarlineMsPos;
			int endBarIndex = endBarBarlinePositions.barIndex;
			int endMsPos = endBarBarlinePositions.rightBarlineMsPos;			

			Debug.Assert(startMsPos >= 0 && endMsPos > startMsPos);

			this.name = name;
			this.startBarIndex = startBarIndex;
			this.startMsPos = startMsPos;
			this.endBarIndex = endBarIndex;
			this.endMsPos = endMsPos;
		}

		public void WriteSVG(SvgWriter w)
		{
			w.WriteStartElement("regionDef");
			w.WriteAttributeString("class", "regionDef");
			w.WriteAttributeString("name", name.ToString());
			w.WriteAttributeString("fromStartOfBar", (startBarIndex + 1).ToString());
			w.WriteAttributeString("startMsPos", startMsPos.ToString());
			w.WriteAttributeString("toEndOfBar", (endBarIndex + 1).ToString());
			w.WriteAttributeString("endMsPos", endMsPos.ToString());
			w.WriteEndElement();
		}

		public readonly char name;
		public readonly int startBarIndex;
		public readonly int startMsPos;
		public readonly int endBarIndex;
		public readonly int endMsPos;
	}
}
