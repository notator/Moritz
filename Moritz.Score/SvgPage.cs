using System;
using System.Collections.Generic;
using System.Windows.Forms; // MessageBox
using System.Diagnostics;
using System.IO;
using System.Xml;

using Moritz.Globals;
using Moritz.Score.Midi;

namespace Moritz.Score
{
    public class SvgPage
    {
        /// <summary>
        /// The Metadata contained in the [title] and [desc] elements has already been read.
        /// This constructor reads information in the 'score' namespace, ignoring the graphics.
        /// </summary>
        /// <param name="pagePath"></param>
        public SvgPage(SvgScore containerScore, int pageNumber, string pagePath)
        {
            _score = containerScore;
            _pageFormat = null;
            _pageNumber = pageNumber;
            _infoTextInfo = null;

            int msDuration;
            string id;
            Stream pageStream = File.OpenRead(pagePath);
            Dictionary<string, string> attributesDict = null;
            SvgSystem currentSystem = null;
            Staff currentStaff = null;
            Voice currentVoice = null;
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            using(XmlReader r = XmlReader.Create(pageStream, settings))
            {
                do
                {
                    M.ReadToXmlElementTag(r, "g", "svg");
                    if(r.Name == "g" && r.IsStartElement())
                    {
                        attributesDict = M.AttributesDict(r);
                        if(attributesDict != null && attributesDict.ContainsKey("score:object"))
                        {
                            switch(attributesDict["score:object"])
                            {
                                case "system":
                                    currentSystem = new SvgSystem(_score);
                                    this.Systems.Add(currentSystem);
                                    _score.Systems.Add(currentSystem);
                                    break;
                                case "staff":
                                    currentStaff = GetStaff(r, currentSystem,
                                            attributesDict["score:staffName"],attributesDict["score:stafflines"],attributesDict["score:gap"]);
                                    break;
                                case "voice":
                                    currentVoice = GetVoice(r, currentStaff, attributesDict["score:midiChannel"]);
                                    //currentVoice = new Voice(currentStaff);
                                    //currentStaff.Voices.Add(currentVoice);
                                    break;
                                case "chord":
                                    id = attributesDict["id"];
                                    msDuration = int.Parse(attributesDict["score:msDuration"]);
                                    MidiChordDef midiChordDef = GetNewMidiChordDef(r, id, msDuration);
                                    currentVoice.LocalMidiDurationDefs.Add(new LocalMidiDurationDef(midiChordDef));
                                    // msPositions are set later
                                    break;
                                case "rest":
                                    id = attributesDict["id"];
                                    msDuration = int.Parse(attributesDict["score:msDuration"]);
                                    MidiRestDef midiRestDef = new MidiRestDef(id, msDuration);
                                    currentVoice.LocalMidiDurationDefs.Add(new LocalMidiDurationDef(midiRestDef));
                                    // msPositions are set later
                                break;
                            }
                        }
                    }
                } while(!(r.Name == "svg" && !r.IsStartElement()));
            }         
            pageStream.Close();
        }

        private MidiChordDef GetNewMidiChordDef(XmlReader r, string id, int msDuration)
        {
            MidiChordDef midiChordDef = null;
            M.ReadToXmlElementTag(r, "score:midiChord");
            midiChordDef = new MidiChordDef(r, id, this._score.MidiDurationDefs, msDuration);
            return midiChordDef;
        }

        private Staff GetStaff(XmlReader r, SvgSystem system, string staffName, string stafflines, string gap)
        {
            Staff newStaff = new Staff(system, staffName, int.Parse(stafflines), float.Parse(gap, M.En_USNumberFormat));
            system.Staves.Add(newStaff);
            return newStaff;
        }

        private Voice GetVoice(XmlReader r, Staff staff, string midiChannel)
        {
            Voice newVoice = new Voice(staff, byte.Parse(midiChannel));
            staff.Voices.Add(newVoice);
            return newVoice;
        }

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
                    // Found by experiment that adding (_pageMetrics.Gap / 3F) gives good results in both studies.
                    // old version: deltaY -= (deltaY % _pageMetrics.Gap);
                    deltaY = deltaY - (deltaY % _pageFormat.Gap) + (_pageFormat.Gap / 3F);
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
            w.WriteProcessingInstruction("xml-stylesheet", "href=\"http://james-ingram-act-two.de/fontsStyleSheet.css\" type=\"text/css\"");
            w.WriteStartElement("svg", "http://www.w3.org/2000/svg");

            WriteSvgHeader(w, pageNumber);

            metadata.WriteSVG(w);

            _score.WriteSymbolDefinitions(w);
            _score.WriteMidiChordDefinitions(w);

            #region pageObjects
            w.SvgRect("frame", 0, 0, _pageFormat.Right, _pageFormat.Bottom, "#CCCCCC", 1, "white", null);

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
            w.WriteEndElement(); // closes the svg element
            w.WriteEndDocument();
        }

        private void WriteSvgHeader(SvgWriter w, int pageNumber)
        {
            w.WriteAttributeString("version", "1.1");
            w.WriteAttributeString("baseProfile", "full");
            w.WriteAttributeString("width", _pageFormat.ScreenRight.ToString()); // the intended screen display size (100%)
            w.WriteAttributeString("height", _pageFormat.ScreenBottom.ToString()); // the intended screen display size (100%)
            string viewBox = "0 0 " + _pageFormat.Right.ToString() + " " + _pageFormat.Bottom.ToString();
            w.WriteAttributeString("viewBox", viewBox); // the size of SVG's internal drawing surface (400%)
            w.WriteAttributeString("xmlns", "xlink", null, "http://www.w3.org/1999/xlink");
            w.WriteAttributeString("xmlns", "score", null, "http://www.james-ingram-act-two.de/open-source/svgScoreExtensions.html");
            if(pageNumber == 1)
            {
                w.WriteAttributeString("onload", "onLoad()"); // function called when page 1 has loaded
                WriteScriptLink(w);
            }
        }

        // writes the line: <script xlink:href="../../ap/SVG.js" type="text/javascript"/>
        private void WriteScriptLink(SvgWriter w)
        {
            w.WriteStartElement("script");
            w.WriteAttributeString("xlink", "href", null, "../../ap/SVG.js");
            w.WriteAttributeString("type", "text/javascript");
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
                w.WriteAttributeString("text-anchor", "left");
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
