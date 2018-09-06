using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
	public class ScoreData
	{
		public ScoreData(Regions regions)
		{
			this.regions = regions;
		}

		/// <summary>
		/// Writes a score:scoreData element
		/// </summary>
		/// <![CDATA[
		/// <score:scoreData>
		///		<regions>
		///			<defs>
		///				<!--
		///					Each name must be a(any) single character.
		///				   The first regionDef must have startMsPos= "0".
		///					"msPosFinalBarline" can be used to indicate the msPos of the final barline in the score.
		///				-->
		///				<regionDef class="regionDef" name="a" startMsPos="0" endMsPos="4447" />
		///				<regionDef class="regionDef" name="b" startMsPos="1234" endMsPos="4447" />
		///				<regionDef class="regionDef" name="c" startMsPos="2769" endMsPos="7338" />
		///				<regionDef class="regionDef" name="d" startMsPos="2769" endMsPos="msPosFinalBarline" />
		///			</defs>
		///			<!-- Not all the defined regions have to be present in the sequence(d is missing here). -->
		///			<regionSequence class="regionSequence" sequence="abcbd" />
		///		</regions>		
		///	</score:scoreData>]]>
		public void WriteSVG(SvgWriter w)
		{
			w.WriteStartElement("score","scoreData", null);

			regions.WriteSVG(w);

			w.WriteEndElement(); // end "score:scoreData"           
		}

		private readonly Regions regions;
	}
}
