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
			Debug.Assert(regionDefs[0].startCs.msPos == 0);

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
		/// <![CDATA[
		///		<regions>
		///			<defs>
		///				<!--
		///					Each name must be a(any) single character.
		///				    The first regionDef must have startMsPos= "0".
		///					int.MaxValue can be used to indicate the msPos of the final barline in the score.
		///				-->
		///				<regionDef class="regionDef" name="a" startMsPos="0" endMsPos="4447" />
		///				<regionDef class="regionDef" name="b" startMsPos="1234" endMsPos="4447" />
		///				<regionDef class="regionDef" name="c" startMsPos="2769" endMsPos="7338" />
		///				<regionDef class="regionDef" name="d" startMsPos="2769" endMsPos="2147483647" />
		///			</defs>
		///			<!-- Not all the defined regions have to be present in the sequence(d is missing here). -->
		///			<regionSequence class="regionSequence" sequence="abcbd" />
		///		</regions>		
		///	]]>
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
		/// <param name="name">Any single character</param>
		/// <param name="startCs">Greater than or equa to 0. (The first RegionDef must have startMsPos=0.)</param>
		/// <param name="endCs">Greater than startMsPos. (Use int.MaxValue for the msPos of the final barline in the score.)</param>
		public RegionDef(char name, ScoreCoordinates startCs, ScoreCoordinates endCs)
		{
			Debug.Assert(startCs != null && startCs.msPos >= 0);
			Debug.Assert(endCs != null && endCs.msPos > startCs.msPos);

			this.name = name;
			this.startCs = startCs;
			this.endCs = endCs;
		}

		public void WriteSVG(SvgWriter w)
		{
			w.WriteComment($@" startMsPos: barIndex={startCs.barIndex} trkIndex={startCs.trkIndex} uniqueDefIndex={startCs.uniqueDefIndex} ");
			if(endCs.barIndex == -1)
			{
				w.WriteComment($@"   endMsPos: msPosition of final barline ");
			}
			else
			{
				w.WriteComment($@"   endMsPos: barIndex={endCs.barIndex} trkIndex={endCs.trkIndex} uniqueDefIndex={endCs.uniqueDefIndex} ");
			}
			w.WriteStartElement("regionDef");
			w.WriteAttributeString("class", "regionDef");
			w.WriteAttributeString("name", name.ToString());
			w.WriteAttributeString("startMsPos", startCs.msPos.ToString());
			w.WriteAttributeString("endMsPos", endCs.msPos.ToString());
			w.WriteEndElement();
		}

		public readonly char name;
		public readonly ScoreCoordinates startCs;
		public readonly ScoreCoordinates endCs;
	}
}
