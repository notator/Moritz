﻿using Moritz.Xml;

namespace Moritz.Symbols
{
    public class ScoreData
    {
        public ScoreData(RegionSequence regionSequence)
        {
            this.RegionSequence = regionSequence;
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
            w.WriteStartElement("score", "scoreData", null);

            RegionSequence.WriteSVG(w);

            w.WriteEndElement(); // end "score:scoreData"           
        }

        public readonly RegionSequence RegionSequence;
    }
}
