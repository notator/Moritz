using System;
using System.Collections.Generic;
using System.Windows.Forms; // MessageBox
using System.Diagnostics;
using System.Xml;
using System.Text;

using Moritz.Globals;
using Moritz.Xml;
using Moritz.Spec;

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

			metadata.WriteSVG(w, _pageNumber, _score.PageCount, _pageFormat.AboutLinkURL, nOutputVoices, nInputVoices);

            _score.WriteDefs(w, _pageNumber);

			#region layers

			if(_pageNumber > 0)
			{ 
				WriteFrameLayer(w, _pageFormat.Right, _pageFormat.Bottom);
			}

			WriteSystemsLayer(w, _pageNumber, metadata);

            w.WriteComment(@" Annotations that are added here will be ignored by the AssistantPerformer. ");

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

		private void WriteFrameLayer(SvgWriter w, float width, float height)
		{
            w.SvgRect(CSSObjectClass.frame.ToString(), 0, 0, width, height);
        }

		private void WriteSystemsLayer(SvgWriter w, int pageNumber, Metadata metadata)
		{
            w.SvgStartGroup(CSSObjectClass.systems.ToString());

            //w.WriteAttributeString("style", "display:inline");

			w.SvgText(CSSObjectClass.timeStamp, _infoTextInfo.Text, 32, _infoTextInfo.FontHeight);

			if(pageNumber == 1 || pageNumber == 0)
			{
				WritePage1TitleAndAuthor(w, metadata);
			}

            List<CarryMsgs> carryMsgsPerChannel = new List<CarryMsgs>();
            foreach(Staff staff in Systems[0].Staves)
            {
                foreach(Voice voice in staff.Voices)
                {
                    carryMsgsPerChannel.Add(new CarryMsgs());
                }
            }

			int systemNumber = 1;
			foreach(SvgSystem system in Systems)
			{
				system.WriteSVG(w, systemNumber++, _pageFormat, carryMsgsPerChannel);
			}

			w.WriteEndElement(); // end layer
		}

        private void WriteSvgHeader(SvgWriter w)
        {
			w.WriteAttributeString("xmlns", "http://www.w3.org/2000/svg");
            // I think the following is redundant...
            //w.WriteAttributeString("xmlns", "svg", null, "http://www.w3.org/2000/svg");
            // Deleted the following, since it is only advisory, and I think the latest version is 2. See deprecated xlink below.
            //w.WriteAttributeString("version", "1.1");

            // Namespaces used for standard metadata
            w.WriteAttributeString("xmlns", "dc", null, "http://purl.org/dc/elements/1.1/");
			w.WriteAttributeString("xmlns", "cc", null, "http://creativecommons.org/ns#");
			w.WriteAttributeString("xmlns", "rdf", null, "http://www.w3.org/1999/02/22-rdf-syntax-ns#");

            // Namespace used for linking to svg defs (defined objects)
            // N.B.: xlink is deprecated in SVG 2 
			// w.WriteAttributeString("xmlns", "xlink", null, "http://www.w3.org/1999/xlink");

            // Standard definition of the "score" namespace.
            // The file documents the additional attributes and elements available in the "score" namespace.
            w.WriteAttributeString("xmlns", "score", null, "http://www.james-ingram-act-two.de/open-source/svgScoreExtensions.html");

            // The file defines and documents all the element classes used in this particular scoreType.
            // The definitions include information as to how the classes nest, and the directions in which they are read.
            // For example:
            // 1) in cmn_core files, systems are read from top to bottom on a page, and contain
            //    staves that are read in parallel, left to right.
            // 2) cmn_1950.html files might include elements having class="tupletBracket", but
            //    cmn_core files don't. As with the score namespace, the file does not actually
            //    need to be read by the client code in order to discover the scoreType. 
            w.WriteAttributeString("data-scoreType", null, "http://www.james-ingram-act-two.de/open-source/cmn_core.html");

            w.WriteAttributeString("width", M.FloatToShortString(_pageFormat.ScreenRight)); // the intended screen display size (100%)
            w.WriteAttributeString("height", M.FloatToShortString(_pageFormat.ScreenBottom)); // the intended screen display size (100%)
            string viewBox = "0 0 " + _pageFormat.RightVBPX.ToString() + " " + _pageFormat.BottomVBPX.ToString();
            w.WriteAttributeString("viewBox", viewBox); // the size of SVG's internal drawing surface (800%)            
        }

		/// <summary>
		/// Adds the main title and the author to the first page.
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
			w.WriteAttributeString("class", CSSObjectClass.titles.ToString());
			w.SvgText(CSSObjectClass.mainTitle, titleInfo.Text, _pageFormat.Right / 2F, _pageFormat.Page1TitleY);
			w.SvgText(CSSObjectClass.author, authorInfo.Text, _pageFormat.RightMarginPos, _pageFormat.Page1TitleY);
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
