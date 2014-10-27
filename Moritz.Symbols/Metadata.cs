using System;
using System.Xml;
using System.Diagnostics;
using System.IO;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
	public class Metadata
	{
        /// <summary>
        /// Contains default values.
        /// </summary>
        /// <param name="score"></param>
        public Metadata()
        {
        }

        public void WriteSVG(SvgWriter w)
        {
            if(!string.IsNullOrEmpty(MainTitle))
            {
                w.WriteStartElement("title");
                w.WriteString(MainTitle);
                w.WriteEndElement();
            }

            w.WriteStartElement("desc");

            if(!string.IsNullOrEmpty(Author))
            {
                w.WriteString("\n\t\tAuthor: " + Author);
            }

            if(!string.IsNullOrEmpty(Website))
            {
                w.WriteString("\n\t\tWebsite: " + Website);
            }

            w.WriteString("\n\t\tDate: " + M.NowString);

            if(!string.IsNullOrEmpty(Software))
            {
                w.WriteString("\n\t\tSoftware: " + Software);
            }

            if(!string.IsNullOrEmpty(Keywords))
            {
                w.WriteString("\n\t\tKeywords: " + Keywords);
            }

            if(!string.IsNullOrEmpty(Comment))
            {
                w.WriteString("\n\t\tComment: " + Comment);
            }

            w.WriteEndElement(); // ends the desc element            
        }
 
        public string MainTitle = "";
 		public string Keywords = "";
		public string Comment = "";
        public string Date = "";

		public string Author = "James Ingram";
        public string Software = "Moritz/Assistant Composer";
        public string Website = "http://www.james-ingram-act-two.de";

	}
}
