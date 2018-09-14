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
		public Regions(IReadOnlyList<RegionDef> basicRegionDefs, string regionSequence)
		{
			Debug.Assert(!(basicRegionDefs == null || basicRegionDefs.Count == 0));
			Debug.Assert(!String.IsNullOrEmpty(regionSequence));
			Debug.Assert(basicRegionDefs[0].startMsPos == 0);

			for(int i = 0; i < basicRegionDefs.Count - 1; ++i)
			{
				Debug.Assert(basicRegionDefs[i].name.Length == 1,
					"RegionDef names can only have one character here.");
				Debug.Assert(regionSequence.Contains(basicRegionDefs[i].name),
					"The regionSequence can only use defined RegionDefs.");

				for(int j = i + 1; j < basicRegionDefs.Count; ++j)
				{
					Debug.Assert(basicRegionDefs[i].startMsPos <= basicRegionDefs[j].startMsPos,
						"RegionDef startMsPositions may be the same, but must otherwise be in chronological order.");
					Debug.Assert(basicRegionDefs[i].name.CompareTo(basicRegionDefs[j].name) < 0,
						"The RegionDefs names must all be different and in alphabetical order here.");
				}
			}

			List<string> baseNames = GetBaseNames(basicRegionDefs);
			List<string> uniqueNames = GetUniqueNames(baseNames, regionSequence);
			List<RegionDef> regionDefSeq = GetUniqueRegionDefSequence(basicRegionDefs, regionSequence, uniqueNames);

			this.barlineStartRegionsDict = GetBarlineStartRegionsDict(regionDefSeq);
			this.barlineRegionLinksDict = GetBarlineRegionLinksDict(regionDefSeq);

			this.regionDefs = basicRegionDefs;
			this.regionSequence = regionSequence;
		}

		private SortedDictionary<int, List<string>> GetBarlineStartRegionsDict(List<RegionDef> regionDefSeq)
		{
			var rval = new SortedDictionary<int, List<string>>();

			foreach(RegionDef rd in regionDefSeq)
			{
				int barIndex = rd.startBarIndex;
				if(!rval.ContainsKey(barIndex))
				{
					rval.Add(barIndex, new List<string>() { rd.name });
				}
				else
				{
					rval[barIndex].Add(rd.name);
				}
			}

			return rval;
		}

		private SortedDictionary<int, List<string>> GetBarlineRegionLinksDict(List<RegionDef> regionDefSeq)
		{
			var rval = new SortedDictionary<int, List<string>>();

			for(int i = 0; i < regionDefSeq.Count; ++i)
			{
				string regionLink = (i < regionDefSeq.Count - 1) ? regionDefSeq[i].name + "➔" + regionDefSeq[i + 1].name : regionDefSeq[i].name + " end";
				int barIndex = regionDefSeq[i].endBarIndex;
				if(!rval.ContainsKey(barIndex))
				{
					rval.Add(barIndex, new List<string>() { regionLink });
				}
				else
				{
					rval[barIndex].Add(regionLink);
				}
			}

			return rval;
		}

		/// <summary>
		/// returns the regionDef sequence to be played, with each regionDef having a unique name. 
		/// </summary>
		/// <param name="regionDefs"></param>
		/// <param name="regionSequence"></param>
		/// <param name="uniqueNames"></param>
		/// <returns></returns>
		private static List<RegionDef> GetUniqueRegionDefSequence(IReadOnlyList<RegionDef> regionDefs, string regionSequence, List<string> uniqueNames)
		{
			List<RegionDef> regionDefSeq = new List<RegionDef>();
			for(int i = 0; i < regionSequence.Length; ++i)
			{
				string uniqueName = uniqueNames[i];
				RegionDef brd = FindBaseRegionDef(regionDefs, regionSequence[i]);
				RegionDef uniqueRegionDef = new RegionDef(uniqueName, brd.startBarIndex, brd.startMsPos, brd.endBarIndex, brd.endMsPos);
				regionDefSeq.Add(uniqueRegionDef);
			}
			return regionDefSeq;
		}

		private static RegionDef FindBaseRegionDef(IReadOnlyList<RegionDef> regionDefs, char c)
		{
			RegionDef rval = null;
			foreach(RegionDef rd in regionDefs)
			{
				if(string.Compare(rd.name, c.ToString()) == 0 )
				{
					rval = rd;
					break;
				}
			}
			return rval;
		}

		/// <summary>
		/// Returns unique regions in order of regionSequence. Each has a unique name.
		/// The returned list.Count == regionSequence.length.
		/// </summary>
		private List<string> GetUniqueNames(List<string> basenames, string regionSequence)
		{
			List<int> basenameCounts = new List<int>();
			foreach(string basename in basenames)
			{
				basenameCounts.Add(0);
			}

			var rval = new List<string>();
			for(int i = 0; i < regionSequence.Length; ++i)
			{
				int basenameIndex = 0;
				string baseName = regionSequence[i].ToString();
				for(int j = 0; j < basenames.Count; ++j)
				{
					if(baseName.CompareTo(basenames[j]) == 0)
					{
						basenameIndex = j;
						break;
					}
				}
				string uniqueName = (basenameCounts[basenameIndex] > 0) ? baseName + basenameCounts[basenameIndex].ToString() : baseName;
				basenameCounts[basenameIndex]++;
				rval.Add(uniqueName);
			}

			return rval;
		}

		private List<string> GetBaseNames(IReadOnlyList<RegionDef> basicRegionDefs)
		{
			List<string> rval = new List<string>();
			foreach(RegionDef rd in basicRegionDefs)
			{
				rval.Add(rd.name);
			}
			return rval;
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

		/// <summary>
		/// int is a barline index, List of string is the list of uniqueRegionNames that begin at this barline, in chronological order
		/// </summary>
		public readonly SortedDictionary<int, List<string>> barlineStartRegionsDict = null;

		/// <summary>
		/// int is a barline index, List of string is the list of regionLinks for this barline, in chronological order.
		/// A regionLink is a string of the form currentregionName->nextregionName (e.g. "A->D" or "B->A1", "A1end" etc.)
		/// </summary>
		public readonly SortedDictionary<int, List<string>> barlineRegionLinksDict = null;

		private readonly IReadOnlyList<RegionDef> regionDefs;
		private readonly string regionSequence;
	}
}
