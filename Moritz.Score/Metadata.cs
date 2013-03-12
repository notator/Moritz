using System;
using System.Xml;
using System.Diagnostics;
using System.IO;

using Moritz.Globals;

namespace Moritz.Score
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
        /// Metadata is the infromation contained in the [title] and [desc] elements in the (first) svg file.
        /// page1Path is the path to the first .svg file in the score (which may be the score itself).
        /// </summary>
        /// <param name="page1Path"></param>
        public Metadata(string page1Path)
        {
            Debug.Assert(Path.GetExtension(page1Path) == ".svg");
            try
            {
                Stream scoreStream = File.OpenRead(page1Path);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Ignore;
                using(XmlReader r = XmlReader.Create(scoreStream, settings))
                {
                    M.ReadToXmlElementTag(r, "title");
                    MainTitle = r.ReadElementContentAsString();

                    M.ReadToXmlElementTag(r, "desc");
                    GetFields(r.ReadElementContentAsString());
                }
                scoreStream.Close();
            }
            catch(Exception ex)
            {
                string msg = ex.Message + "\r\rException thrown while loading Metadata.";
                throw new ApplicationException(msg);
            }
        }

        private void GetFields(string desc)
        {
            desc = desc.Replace("\t", "");
            string[] strings = desc.Split(new char[] { '\n' });
            foreach(string s in strings)
            {
                string line = s.Trim();
                if(line.Contains("Author: "))
                {
                    Author = line.Remove(0, "Author: ".Length);
                }
                else if(line.Contains("Website: "))
                {
                    Website = line.Remove(0, "Website: ".Length);
                }
                else if(line.Contains("Date: "))
                {
                    Date = line.Remove(0, "Date: ".Length);
                }
                else if(line.Contains("Software: "))
                {
                    Software = line.Remove(0, "Software: ".Length);
                }
                else if(line.Contains("Keywords: "))
                {
                    Keywords = line.Remove(0, "Keywords: ".Length);
                }
                else if(line.Contains("Comment: "))
                {
                    Comment = line.Remove(0, "Comment: ".Length);
                }
                else
                {
                    Comment = Comment + "\r\n" + line;
                }
            }
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
