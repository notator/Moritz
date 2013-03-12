using System.Collections.Generic;
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
            _pageMetrics = null;
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
                                    currentVoice.LocalizedMidiDurationDefs.Add(new LocalizedMidiDurationDef(midiChordDef));
                                    // msPositions are set later
                                    break;
                                case "rest":
                                    id = attributesDict["id"];
                                    msDuration = int.Parse(attributesDict["score:msDuration"]);
                                    MidiRestDef midiRestDef = new MidiRestDef(id, msDuration);
                                    currentVoice.LocalizedMidiDurationDefs.Add(new LocalizedMidiDurationDef(midiRestDef));
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

        //private void GetMidiChordDef(XmlReader r, Voice voice, int msPosition, int msDuration)
        //{
        //    MidiChordDef midiChordDef = new MidiChordDef(r);
        //    voice.MidiDurationDefs.Add(midiChordDef);
        //    //newChord.ReadMidiInfo(r, _score.MidiChordDefs);
        //}

        //}
        /// <summary>
        /// The systems contain Metrics info, but their top staffline is at 0.
        /// The systems are moved to their correct vertical positions on the page here.
        /// </summary>
        /// <param name="Systems"></param>
        public SvgPage(SvgScore containerScore, PageFormat pageFormat, int pageNumber, TextInfo infoTextInfo, List<SvgSystem> pageSystems, bool lastPage)
        {
            _score = containerScore;
            _pageMetrics = pageFormat;
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
                    deltaY = deltaY - (deltaY % _pageMetrics.Gap) + (_pageMetrics.Gap / 3F);
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
            WriteSvgPageHeader(w);
            metadata.WriteSVG(w);

            _score.WriteSymbolDefinitions(w);
            _score.WriteMidiChordDefinitions(w);
            _score.WriteJavaScriptDefinitions(w);

            #region pageObjects
            w.SvgRect("frame", 0, 0, _pageMetrics.Right, _pageMetrics.Bottom, "black", 1, "white", null);
            w.SvgText(_infoTextInfo, 80, 80);

            if(_pageNumber == 1)
            {
                WriteMainTitleAndAuthor(w, metadata);
            }
            #endregion

            int systemNumber = 1;
            foreach(SvgSystem system in Systems)
            {
                system.WriteSVG(w, pageNumber, systemNumber++, _pageMetrics);
            }
            w.WriteEndElement(); // closes the svg element
            w.WriteEndDocument();
        }

        public void WriteSvgPageHeader(SvgWriter w)
        {
            w.WriteStartDocument(); // standalone="no"

            w.WriteProcessingInstruction("xml-stylesheet", "href=\"http://james-ingram-act-two.de/fontsStyleSheet.css\" type=\"text/css\"");

            w.WriteStartElement("svg", "http://www.w3.org/2000/svg");
            w.WriteAttributeString("version", "1.1");
            w.WriteAttributeString("baseProfile", "full");
            w.WriteAttributeString("width", _pageMetrics.ScreenRight.ToString()); // the intended screen display size (100%)
            w.WriteAttributeString("height", _pageMetrics.ScreenBottom.ToString()); // the intended screen display size (100%)
            string viewBox = "0 0 " + _pageMetrics.Right.ToString() + " " + _pageMetrics.Bottom.ToString();
            w.WriteAttributeString("viewBox", viewBox); // the size of SVG's internal drawing surface (400%)
            w.WriteAttributeString("xmlns", "xlink", null, "http://www.w3.org/1999/xlink");
            w.WriteAttributeString("xmlns", "score", null, "http://www.james-ingram-act-two.de/open-source/svgScoreExtensions.html");
        }

        /// <summary>
        /// Adds the main title and the author to the first page.
        /// </summary>
        protected void WriteMainTitleAndAuthor(SvgWriter w, Metadata metadata)
        {
            TextInfo titleInfo =
                new TextInfo(metadata.MainTitle, "Estrangelo Edessa", _pageMetrics.Page1TitleHeight,
                    null, TextHorizAlign.center);
            TextInfo authorInfo =
              new TextInfo(metadata.Author, "Estrangelo Edessa", _pageMetrics.Page1AuthorHeight,
                  null, TextHorizAlign.right);
            w.WriteStartElement("g");
            w.WriteAttributeString("id", "page1TitleAndAuthor" + SvgScore.UniqueID_Number);
            w.SvgText(titleInfo, _pageMetrics.Right / 2F, _pageMetrics.Page1TitleY);
            w.SvgText(authorInfo, _pageMetrics.RightMarginPos, _pageMetrics.Page1TitleY);
            w.WriteEndElement(); // group
        }

        #region used when creating graphic score
        private readonly SvgScore _score;
        private PageFormat _pageMetrics;
        private readonly int _pageNumber;
        private TextInfo _infoTextInfo;
        #endregion

        public List<SvgSystem> Systems = new List<SvgSystem>();
    }
}
