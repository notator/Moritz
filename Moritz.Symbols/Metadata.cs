using Moritz.Globals;
using Moritz.Xml;

using System;
using System.Diagnostics;
using System.Text;

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
        public void WriteSVG(SvgWriter w, int pageNumber, int nScorePages, string aboutThePieceLinkURL, int nVoices)
        {
            Debug.Assert(!String.IsNullOrEmpty(Page1Title));

            string pageTitle;
            if(pageNumber == 0)
            {
                pageTitle = Page1Title + " (scroll)";
            }
            else
            {
                pageTitle = Page1Title + ", page " + pageNumber.ToString() + " of " + nScorePages.ToString();
            }

            w.WriteStartElement("title");
            w.WriteAttributeString("class", "pageTitle");
            w.WriteString(pageTitle);
            w.WriteEndElement();

            w.WriteStartElement(CSSObjectClass.metadata.ToString()); // Inkscape compatible
            w.WriteAttributeString("class", CSSObjectClass.metadata.ToString());
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
            w.WriteString("Website: https://www.james-ingram-act-two.de");
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

            StringBuilder desc = new StringBuilder("About: " + aboutThePieceLinkURL);
            if(pageNumber == 0)
            {
                desc.Append("\nNumber of pages in the score: 1");
            }
            else
            {
                desc.Append("\nNumber of pages in the score: " + nScorePages.ToString());
            }
            desc.Append("\nNumber of output voices: " + nVoices.ToString());
            if(!String.IsNullOrEmpty(Comment))
                desc.Append("\nComments: " + Comment);

            w.WriteStartElement("dc", "description", null);
            w.WriteString(desc.ToString());
            w.WriteEndElement(); // ends the dc:description element

            string contributor = "Originally created using Assistant Composer software:" +
                            "\nhttps://james-ingram-act-two.de/moritz3/assistantComposer/assistantComposer.html" +
                            "\nAnnotations, if there are any, have been made using Inkscape.";
            w.WriteStartElement("dc", "contributor", null);
            w.WriteStartElement("cc", "Agent", null);
            w.WriteStartElement("dc", "title", null);
            w.WriteString(contributor);
            w.WriteEndElement(); // ends the dc:title element
            w.WriteEndElement(); // ends the cc:Agent element
            w.WriteEndElement(); // ends the dc:creator element

            w.WriteEndElement(); // ends the cc:Work element
            w.WriteEndElement(); // ends the rdf:RDF element
            w.WriteEndElement(); // ends the metadata element            
        }

        public string Page1Title = "";
        public string Comment = "";
        public string Keywords = "";
        public string Page1Author = "";
        public string Date = "";
    }

}
