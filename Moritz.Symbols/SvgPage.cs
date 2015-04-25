using System;
using System.Collections.Generic;
using System.Windows.Forms; // MessageBox
using System.Diagnostics;
using System.Xml;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    public class SvgPage
    {
        /// <summary>
        /// The systems contain Metrics info, but their top staffline is at 0.
        /// The systems are moved to their correct vertical positions on the page here.
        /// </summary>
        /// <param name="Systems"></param>
        public SvgPage(SvgScore containerScore, PageFormat pageFormat, int pageNumber, TextInfo infoTextInfo, List<SvgSystem> pageSystems, bool lastPage)
        {
            _score = containerScore;
            _pageFormat = pageFormat;
            _pageNumber = pageNumber;
            _infoTextInfo = infoTextInfo;

            Systems = pageSystems;

            MoveSystemsVertically(pageFormat, pageSystems, (pageNumber == 1), lastPage);
        }

        /// <summary>
        /// Moves the systems to their correct vertical position. Justifies on all but the last page.
        /// On the first page use pageFormat.FirstPageFrameHeight.
        /// On the last page (which may also be the first), the systems are separated by 
        /// pageFormat.MinimumDistanceBetweenSystems.
        /// </summary>
        private void MoveSystemsVertically(PageFormat pageFormat, List<SvgSystem> pageSystems, bool firstPage, bool lastPage)
        {
            float frameTop;
            float frameHeight;
            if(firstPage)
            {
                frameTop = pageFormat.TopMarginPage1;
                frameHeight = pageFormat.FirstPageFrameHeight;
            }
            else
            {
                frameTop = pageFormat.TopMarginOtherPages;
                frameHeight = pageFormat.OtherPagesFrameHeight;
            }

            MoveSystemsVertically(pageSystems, frameTop, frameHeight, pageFormat.DefaultDistanceBetweenSystems, lastPage);
        }

        private void MoveSystemsVertically(List<SvgSystem> pageSystems, float frameTop, float frameHeight, float defaultSystemSeparation, bool lastPage)
        {
            float systemSeparation = 0;
            if(lastPage) // dont justify
            {
                systemSeparation = defaultSystemSeparation;
            }
            else
            {                
                if(pageSystems.Count >= 1)
                {
                    float totalSystemHeight = 0;
                    foreach(SvgSystem system in pageSystems)
                    {
                        Debug.Assert(system.Metrics != null);
                        totalSystemHeight += (system.Metrics.NotesBottom - system.Metrics.NotesTop);
                    }
                    systemSeparation = (frameHeight - totalSystemHeight) / (pageSystems.Count - 1);
                }
            }

            float top = frameTop;
            foreach(SvgSystem system in pageSystems)
            {
                if(system.Metrics != null)
                {
                    float deltaY = top - system.Metrics.NotesTop;
                    // Limit stafflineHeight to multiples of _pageMetrics.Gap
                    // so that stafflines are not displayed as thick grey lines.
                    // The following works, because the top staffline of each system is currently at 0.
                    deltaY -= (deltaY % _pageFormat.Gap);
                    system.Metrics.Move(0F, deltaY);
                    top = system.Metrics.NotesBottom + systemSeparation;
                }
            }
        }

        /// <summary>
        /// Writes this page.
        /// </summary>
        /// <param name="w"></param>
        public void WriteSVG(SvgWriter w, Metadata metadata, int pageNumber)
        {
            w.WriteStartDocument(); // standalone="no"
            //<?xml-stylesheet href="../../fontsStyleSheet.css" type="text/css"?>
            w.WriteProcessingInstruction("xml-stylesheet", "href=\"../../fontsStyleSheet.css\" type=\"text/css\"");
            w.WriteStartElement("svg", "http://www.w3.org/2000/svg");

            WriteSvgHeader(w, pageNumber);

			WriteSodipodiNamedview(w); 

            metadata.WriteSVG(w, _pageNumber, _score.PageCount);

            _score.WriteSymbolDefinitions(w);

			w.WriteStartElement("g"); // start layer1 (for Inkscape)
			WriteInkscapeLayerAttributes(w, "layer1", "moritz", true );

            #region pageObjects
            w.SvgRect("frame", 0, 0, _pageFormat.Right, _pageFormat.Bottom, "#CCCCCC", 1, "white");

            WriteStyle(w);

            w.SvgText(_infoTextInfo, 80, 80);

            if(_pageNumber == 1)
            {
                WritePage1LinkTitleAndAuthor(w, metadata);
            }
            #endregion

            int systemNumber = 1;
            foreach(SvgSystem system in Systems)
            {
                system.WriteSVG(w, pageNumber, systemNumber++, _pageFormat);
            }

			w.WriteEndElement(); // end layer1 (for Inkscape)

			w.WriteStartElement("g"); // start layer2 (for Inkscape)
			WriteInkscapeLayerAttributes(w, "layer2", "annotations", false);
			w.WriteEndElement(); // end layer2 (for Inkscape)
			
			w.WriteEndElement(); // close the svg element
            w.WriteEndDocument();
        }

		/// <summary>
		/// The presence of this element means that Inkscape opens the file at full screen size
		/// with level2, the annotations, selected.
		/// </summary>
		/// <param name="w"></param>
		private void WriteSodipodiNamedview(SvgWriter w)
		{
			w.WriteStartElement("sodipodi", "namedview", null);
			w.WriteAttributeString("inkscape", "window-maximized", null, "1");
			w.WriteAttributeString("inkscape", "current-layer", null, "layer2");
			w.WriteEndElement(); // ends the sodipodi:namedview element
		}

        private void WriteSvgHeader(SvgWriter w, int pageNumber)
        {
			w.WriteAttributeString("xmlns", "http://www.w3.org/2000/svg");
			w.WriteAttributeString("xmlns", "score", null, "http://www.james-ingram-act-two.de/open-source/svgScoreExtensions.html");
			w.WriteAttributeString("xmlns", "dc", null, "http://purl.org/dc/elements/1.1/");
			w.WriteAttributeString("xmlns", "cc", null, "http://creativecommons.org/ns#");
			w.WriteAttributeString("xmlns", "rdf", null, "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			w.WriteAttributeString("xmlns", "svg", null, "http://www.w3.org/2000/svg");

			w.WriteAttributeString("xmlns", "xlink", null, "http://www.w3.org/1999/xlink");
			w.WriteAttributeString("xmlns", "sodipodi", null, "http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd");
			w.WriteAttributeString("xmlns", "inkscape", null, "http://www.inkscape.org/namespaces/inkscape");
            w.WriteAttributeString("version", "1.1");
            //w.WriteAttributeString("baseProfile", "full");
            w.WriteAttributeString("width", _pageFormat.ScreenRight.ToString()); // the intended screen display size (100%)
            w.WriteAttributeString("height", _pageFormat.ScreenBottom.ToString()); // the intended screen display size (100%)
            string viewBox = "0 0 " + _pageFormat.Right.ToString() + " " + _pageFormat.Bottom.ToString();
            w.WriteAttributeString("viewBox", viewBox); // the size of SVG's internal drawing surface (400%)            
            if(pageNumber == 1)
            {
                w.WriteAttributeString("onload", "onLoad()"); // function called when page 1 has loaded
                WriteScriptLink(w);
            }
        }

		/// <summary>
		/// writes the following attributes:
		///			inkscape:groupmode="layer"
		///			id="layer1"
		///			inkscape:label="moritz"
		///			style="display:inline"
		///			sodipodi:insensitive="true"
		/// </summary>
		/// <param name="w"></param>
		private void WriteInkscapeLayerAttributes(SvgWriter w, String layerID, String layerName, bool insensitive )
		{
			//w.WriteAttributeString("score", "staffName", null, this.Staffname);
			w.WriteAttributeString("inkscape", "groupmode", null, "layer");
			w.WriteAttributeString("id", layerID);
			w.WriteAttributeString("inkscape", "label", null, layerName);
			w.WriteAttributeString("style", "display:inline");
			if(insensitive == true)
				w.WriteAttributeString("sodipodi", "insensitive", null, "true");
		}

        // writes the line: <script xlink:href="../../ap/SVG.js" type="text/javascript"/>
        private void WriteScriptLink(SvgWriter w)
        {
            w.WriteStartElement("script");
            w.WriteAttributeString("xlink", "href", null, "../../ap/SVG.js");
            w.WriteAttributeString("type", "text/javascript");
			w.WriteString(""); // forces separate close tag to prevent warnings...
            w.WriteEndElement(); // script
        }

        // This style stops the cursor changing to an I-beam every time it
        // passes over text (such as noteheads, clefs, dynamics etc.)
        private void WriteStyle(SvgWriter w)
        {
            w.WriteStartElement("style");
            w.WriteString("text:hover{cursor:default;}");
            w.WriteEndElement();
        }

        /// <summary>
        /// Adds the link, main title and the author to the first page.
        /// </summary>
        protected void WritePage1LinkTitleAndAuthor(SvgWriter w, Metadata metadata)
        {
            string fontFamily = "Estrangelo Edessa";

            TextInfo titleInfo =
                new TextInfo(metadata.MainTitle, fontFamily, _pageFormat.Page1TitleHeight,
                    null, TextHorizAlign.center);
            TextInfo authorInfo =
              new TextInfo(metadata.Author, fontFamily, _pageFormat.Page1AuthorHeight,
                  null, TextHorizAlign.right);
            w.WriteStartElement("g");
            w.WriteAttributeString("id", "page1LinkTitleAndAuthor" + SvgScore.UniqueID_Number);

            WriteAboutLink(w, _pageFormat.AboutLinkURL, _pageFormat.AboutLinkText, fontFamily, _pageFormat.LeftMarginPos, _pageFormat.Page1TitleY);

            w.SvgText(titleInfo, _pageFormat.Right / 2F, _pageFormat.Page1TitleY);
            w.SvgText(authorInfo, _pageFormat.RightMarginPos, _pageFormat.Page1TitleY);
            w.WriteEndElement(); // group
        }

        /// <summary>
        /// If both _pageFormat.AboutLinkURL and _pageFormat.AboutLinkText are set, 
        /// creates a link to the "About" file for this score (on my website).
        /// Audio recordings should be included in the "About" documents for each composition.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="aboutURL"></param> 
        /// <param name="aboutLinkText"></param>
        /// <param name="fontFamily"></param>
        /// <param name="left"></param>
        /// <param name="titleBaseline"></param>
        private void WriteAboutLink(
            XmlWriter w,
            string aboutURL,
            string aboutLinkText,
            string fontFamily,
            float left,
            float titleBaseline)
        {
            if(!String.IsNullOrEmpty(aboutURL) && !String.IsNullOrEmpty(aboutLinkText))
            {
                w.WriteStartElement("a");
                w.WriteAttributeString("class", "linkClass");
                w.WriteAttributeString("xlink", "href", null, aboutURL);
                w.WriteAttributeString("xlink", "show", null, "new"); // open link in new window

                w.WriteStartElement("text");
                w.WriteAttributeString("x", left.ToString(M.En_USNumberFormat));
                w.WriteAttributeString("y", titleBaseline.ToString(M.En_USNumberFormat));
                w.WriteAttributeString("fill", "#1010C6");
                w.WriteAttributeString("text-anchor", "start");
                w.WriteAttributeString("font-size", _pageFormat.Page1AuthorHeight.ToString(M.En_USNumberFormat));
                w.WriteAttributeString("font-family", fontFamily);

                w.WriteStartElement("style");
                w.WriteString(".linkClass:hover{text-decoration:underline;}");
                w.WriteEndElement();

                w.WriteString(aboutLinkText);

                w.WriteEndElement(); // text
                w.WriteEndElement(); // a 
            }
            else
            {
                MessageBox.Show("A link to my website document about this score should be provided in the Assistant Composer's Metadata dialog.", "Reminder");
            }
        }

        #region used when creating graphic score
        private readonly SvgScore _score;
        private PageFormat _pageFormat;
        private readonly int _pageNumber;
        private TextInfo _infoTextInfo;
        #endregion

        public List<SvgSystem> Systems = new List<SvgSystem>();
    }
}
