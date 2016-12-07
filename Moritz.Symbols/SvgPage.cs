using System;
using System.Collections.Generic;
using System.Windows.Forms; // MessageBox
using System.Diagnostics;
using System.Xml;
using System.Text;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    public class SvgPage
    {
        /// <summary>
        /// The systems contain Metrics info, but their top staffline is at 0.
        /// The systems are moved to their correct vertical positions on the page here.
		/// If pageNumber is set to 0, all the systems in pageSystems will be printed
		/// in a single .svg file, whose page height has been changed accordingly.
        /// </summary>
        /// <param name="Systems"></param>
        public SvgPage(SvgScore containerScore, PageFormat pageFormat, int pageNumber, TextInfo infoTextInfo, List<SvgSystem> pageSystems, bool lastPage)
        {
            _score = containerScore;
            _pageFormat = pageFormat;
            _pageNumber = pageNumber;
            _infoTextInfo = infoTextInfo;

            Systems = pageSystems;

			if(pageNumber == 0)
			{
				pageFormat.BottomVBPX = GetNewBottomVBPX(pageSystems);
				pageFormat.BottomMarginPos = (int) (pageFormat.BottomVBPX - pageFormat.DefaultDistanceBetweenSystems);
			}

            MoveSystemsVertically(pageFormat, pageSystems, (pageNumber == 1 || pageNumber == 0), lastPage);
        }

		private int GetNewBottomVBPX(List<SvgSystem> pageSystems)
		{
			int frameHeight = _pageFormat.TopMarginPage1 + 20;
			foreach(SvgSystem system in pageSystems)
			{
				SystemMetrics sm = system.Metrics;
				frameHeight += (int)((sm.Bottom - sm.Top) + _pageFormat.DefaultDistanceBetweenSystems);
			}

			return frameHeight;
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
				frameHeight = pageFormat.FirstPageFrameHeight; // property uses BottomMarginPos
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
        public void WriteSVG(SvgWriter w, Metadata metadata)
        {
			int nOutputVoices = 0;
			int nInputVoices = 0;
			GetNumbersOfVoices(Systems[0], ref nOutputVoices, ref nInputVoices);

            w.WriteStartDocument(); // standalone="no"
            //<?xml-stylesheet href="../../fontsStyleSheet.css" type="text/css"?>
            w.WriteProcessingInstruction("xml-stylesheet", "href=\"../../fontsStyleSheet.css\" type=\"text/css\"");
            w.WriteStartElement("svg", "http://www.w3.org/2000/svg");

            WriteSvgHeader(w);

			WriteSodipodiNamedview(w);

			metadata.WriteSVG(w, _pageNumber, _score.PageCount, _pageFormat.AboutLinkURL, nOutputVoices, nInputVoices);

            _score.WriteDefs(w);

			#region layers

			int layerNumber = 1;

			if(_pageNumber > 0)
			{ 
				WriteFrameLayer(w, layerNumber++, "frame", _pageFormat.Right, _pageFormat.Bottom);
			}

			WriteScoreLayer(w, layerNumber++, "score", _pageNumber, metadata);

			WriteEmptyLayer(w, layerNumber++, "user annotations", true);
			#endregion layers

			w.WriteEndElement(); // close the svg element
            w.WriteEndDocument();
        }

		private void GetNumbersOfVoices(SvgSystem svgSystem, ref int nOutputVoices, ref int nInputVoices)
		{
			nOutputVoices = 0;
			nInputVoices = 0;
			foreach(Staff staff in svgSystem.Staves)
			{
				foreach(Voice voice in staff.Voices)
				{
					if(voice is OutputVoice)
					{
						nOutputVoices++;
					}
					else if(voice is InputVoice)
					{
						nInputVoices++;
					}
				}
			}
		}


		private void WritePageSizedLayer(SvgWriter w, int layerNumber, string layerName, float width, float height, string style)
		{
			w.WriteStartElement("g"); // start layer (for Inkscape)
			WriteInkscapeLayerAttributes(w, layerNumber, layerName, true);

			w.WriteStartElement("rect");
			w.WriteAttributeString("x", "0");
			w.WriteAttributeString("y", "0");
			w.WriteAttributeString("width", width.ToString(M.En_USNumberFormat));
			w.WriteAttributeString("height", height.ToString(M.En_USNumberFormat));
			w.WriteAttributeString("style", style);

			w.WriteEndElement(); // rect
			w.WriteEndElement(); // end layer2 (for Inkscape)
		}

		private void WriteFrameLayer(SvgWriter w, int layerNumber, string layerName, float width, float height)
		{
			string style = "stroke:black; stroke-width:4; fill:#ffffff";
			WritePageSizedLayer(w, layerNumber, layerName, width, height, style);
		}

		private void WriteScoreLayer(SvgWriter w, int layerNumber, string layerName, int pageNumber, Metadata metadata)
		{
			w.WriteStartElement("g"); // start layer (for Inkscape)
			WriteInkscapeLayerAttributes(w, layerNumber, layerName, true);

			w.SvgText("timeStamp", _infoTextInfo, 32, 80);

			if(pageNumber == 1 || pageNumber == 0)
			{
				WritePage1TitleAndAuthor(w, metadata);
			}

			int systemNumber = 1;
			foreach(SvgSystem system in Systems)
			{
				system.WriteSVG(w, systemNumber++, _pageFormat);
			}

			w.WriteEndElement(); // end layer (for Inkscape)
		}

		private void WriteEmptyLayer(SvgWriter w, int layerNumber, string layerName, bool locked)
		{
			w.WriteStartElement("g"); // start layer (for Inkscape)
			WriteInkscapeLayerAttributes(w, layerNumber, layerName, locked);
			w.WriteEndElement(); // end layer (for Inkscape)
		}

		/// <summary>
		/// The presence of this element means that Inkscape opens the file at full screen size
		/// with level2, the annotations, selected.
		/// </summary>
		/// <param name="w"></param>
		private void WriteSodipodiNamedview(SvgWriter w)
		{
			string scoreLayerID = "layer2";

			w.WriteStartElement("sodipodi", "namedview", null);
			w.WriteAttributeString("inkscape", "window-maximized", null, "1");
			w.WriteAttributeString("inkscape", "current-layer", null, scoreLayerID);
			w.WriteEndElement(); // ends the sodipodi:namedview element
		}

        private void WriteSvgHeader(SvgWriter w)
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

            // The following data-scoreType attribute has been included because I think that all files that could be used as
            // input for client applications should document their structure with this (standardized) svg attribute.
            // The value happens here to be the same as for the "score" namespace above, but that is not necessarily so.
            // The value is not checked by the Assistant Performer, because I know that the AP only plays scores that I give it.
            // However, theoretically, anyone could write an app that uses this file, and they would need to be told the format
            // in this standardized attribute. 
            w.WriteAttributeString("data-scoreType", null, "http://www.james-ingram-act-two.de/open-source/svgScoreExtensions.html");

            w.WriteAttributeString("width", _pageFormat.ScreenRight.ToString()); // the intended screen display size (100%)
            w.WriteAttributeString("height", _pageFormat.ScreenBottom.ToString()); // the intended screen display size (100%)
            string viewBox = "0 0 " + _pageFormat.RightVBPX.ToString() + " " + _pageFormat.BottomVBPX.ToString();
            w.WriteAttributeString("viewBox", viewBox); // the size of SVG's internal drawing surface (800%)            
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
		private void WriteInkscapeLayerAttributes(SvgWriter w, int layerNumber, String layerName, bool insensitive )
		{
			//w.WriteAttributeString("score", "staffName", null, this.Staffname);
			w.WriteAttributeString("inkscape", "groupmode", null, "layer");
			w.WriteAttributeString("id", "layer" + layerNumber.ToString());
			w.WriteAttributeString("inkscape", "label", null, layerName);
			w.WriteAttributeString("style", "display:inline");
			if(insensitive == true)
				w.WriteAttributeString("sodipodi", "insensitive", null, "true");
		}

		/// <summary>
		/// Adds the link, main title and the author to the first page.
		/// </summary>
		protected void WritePage1TitleAndAuthor(SvgWriter w, Metadata metadata)
		{
			string titlesFontFamily = "Open Sans";

			TextInfo titleInfo =
				new TextInfo(metadata.MainTitle, titlesFontFamily, _pageFormat.Page1TitleHeight,
					null, TextHorizAlign.center);
			TextInfo authorInfo =
			  new TextInfo(metadata.Author, titlesFontFamily, _pageFormat.Page1AuthorHeight,
				  null, TextHorizAlign.right);
			w.WriteStartElement("g");
			w.WriteAttributeString("id", "titles");
			w.SvgText("mainTitle", titleInfo, _pageFormat.Right / 2F, _pageFormat.Page1TitleY);
			w.SvgText("author", authorInfo, _pageFormat.RightMarginPos, _pageFormat.Page1TitleY);
			w.WriteEndElement(); // group
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
