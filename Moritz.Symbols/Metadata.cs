using System;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Text;

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

		/// <summary>
		/// Writes a metadata element compatible with Inkscape's
		/// </summary>
		/// <param name="w"></param>
		/// <param name="pageNumber"></param>
		/// <param name="nScorePages"></param>
        public void WriteSVG(SvgWriter w, int pageNumber, int nScorePages)
        {
			Debug.Assert(!String.IsNullOrEmpty(MainTitle));

			string pageTitle = MainTitle + ", page " + pageNumber.ToString() + " of " + nScorePages.ToString();

            w.WriteStartElement("title");
			w.WriteString(pageTitle);
            w.WriteEndElement();

            w.WriteStartElement("metadata"); // Inkscape compatible
			w.WriteStartElement("rdf", "RDF", null);
			w.WriteStartElement("cc", "Work", null);
			w.WriteAttributeString("rdf", "about", null, "");

			w.WriteStartElement("dc", "format", null);
			w.WriteString("image/svg+xml");
			w.WriteEndElement(); // ends the dc:format element

			w.WriteStartElement("dc", "type", null);
			w.WriteAttributeString("rdf", "resource", null, "http://purl.org/dc/dcmitype/StillImage");
			w.WriteEndElement(); // ends the dc:type element

			w.WriteStartElement("dc", "title", null);
			w.WriteString(pageTitle);
			w.WriteEndElement(); // ends the dc:title element

			w.WriteStartElement("dc", "date", null);
			w.WriteString(M.NowString);
			w.WriteEndElement(); // ends the dc:date element

			w.WriteStartElement("dc", "creator", null);
			w.WriteStartElement("cc", "Agent", null);
			w.WriteStartElement("dc", "title", null);
			w.WriteString("James Ingram");
			w.WriteEndElement(); // ends the dc:title element
			w.WriteEndElement(); // ends the cc:Agent element
			w.WriteEndElement(); // ends the dc:creator element

			w.WriteStartElement("dc", "source", null);
			w.WriteString("Website: http://www.james-ingram-act-two.de");
			w.WriteEndElement(); // ends the dc:source element

			if(!string.IsNullOrEmpty(Keywords))
			{
				w.WriteStartElement("dc", "subject", null);
				w.WriteStartElement("rdf", "Bag", null);
				w.WriteStartElement("rdf", "li", null);
				w.WriteString(Keywords);
				w.WriteEndElement(); // ends the rdf:li element
				w.WriteEndElement(); // ends the rdf:Bag element
				w.WriteEndElement(); // ends the dc:subject element
			}

			StringBuilder  desc = new StringBuilder("Originally created using my Assistant Composer software." +
							"\nAnnotations, if there are any, have been made using Inkscape.");

			if(!String.IsNullOrEmpty(Comment))
				desc.Append("\ncomments: " + Comment);

			w.WriteStartElement("dc", "description", null);
			w.WriteString(desc.ToString());
			w.WriteEndElement(); // ends the dc:description element

			w.WriteEndElement(); // ends the cc:Work element
			w.WriteEndElement(); // ends the rdf:RDF element
			w.WriteEndElement(); // ends the metadata element            
        }
 
        public string MainTitle = "";
		public string Comment = "";
		public string Keywords = "";
		public string Author = "James Ingram";
		public string Date = "";
	}

}
