using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using Moritz.Globals;
using Moritz.Score.Midi;
using Moritz.Score.Notation;

namespace Moritz.Score
{
    public class SvgScore
    {
        public SvgScore(string folder, string scoreTitleName, string keywords, string comment, PageFormat pageFormat)
        {
            _pageFormat = pageFormat;
            SetFilePathAndMetadata(folder, scoreTitleName, keywords, comment);
        }

        private void SetFilePathAndMetadata(string folder, string scoreTitleName, string keywords, string comment)
        {
            if(!String.IsNullOrEmpty(scoreTitleName))
            {
                M.CreateDirectoryIfItDoesNotExist(folder);
                this._filename = scoreTitleName + ".html";
                FilePath = folder + @"\" + _filename;
                SetScoreMetadata(scoreTitleName, keywords, comment);
            }
        }


        public virtual void WriteMidiChordDefinitions(SvgWriter w)
        {
            throw new NotImplementedException("This function should be overridden by derived classes, and only called while composing.");
        }

        private void SetScoreMetadata(string scoreTitleName, string keywords, string comment)
        {
            Metadata = new Metadata();

            /// The Metadata.MainTitle is the large title displayed at the top centre of page 1
            /// of the score. It is also the name of the folder inside the standard Moritz scores folder
            /// used for saving all the scores components.
            Metadata.MainTitle = scoreTitleName;
            Metadata.Keywords = keywords;
            Metadata.Comment = comment;

            Metadata.Date = M.NowString;
        }

        public SvgScore(string filepath)
        {
            FilePath = filepath;

            _filename = Path.GetFileName(filepath);

            LoadSVGScore(filepath); // copes with .html, .htm, and .svg files
        }

        /// <summary>
        /// The filepath has either an .html, .htm or .svg extension.
        /// </summary>
        /// <param name="filepath"></param>
        private void LoadSVGScore(string filepath)
        {
            string page1Path = GetPage1Path(filepath);
            if(String.IsNullOrEmpty(page1Path))
            {
                string msg = "That was not an SVG score.\n\n" +
                    "An SVG score is either a single page having a .svg extension\n" +
                    "or a series of .svg pages embedded in an .htm or .html file.";
                MessageBox.Show(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Metadata = new Metadata(page1Path);
            ReadMidiDefs(page1Path);

            try
            {
                if(Path.GetExtension(filepath) == ".svg")
                {
                    _pages.Add(new SvgPage(this, 1, page1Path));
                }
                else // multipage score
                {
                    LoadMultiplePages(filepath);
                }

                // MsPositions are required when creating moments which can be performed.
                // MsPositions are used during the composition process (e.g. to spread durations across common boundaries)
                // and when loading .capx files (tuplets must be spread across MsDurations, quantizing single durations
                // to whole numbers of milliseconds.
                // But MsPositions are not saved in SVG-MIDI (or, in future, in .capx) files. They are set here.
                SetAllMsPositions();

            }
            catch(Exception ex)
            {
                string msg = ex.Message + "\r\rException thrown in SvgScore.LoadSVGScore().";
                throw new ApplicationException(msg);
            }
        }

        protected void SetAllMsPositions()
        {
            for(int staffIndex = 0; staffIndex < Systems[0].Staves.Count; ++staffIndex)
            {
                for(int voiceIndex = 0; voiceIndex < Systems[0].Staves[staffIndex].Voices.Count; ++voiceIndex)
                {
                    int msPosition = 0;
                    for(int sysIndex = 0; sysIndex < Systems.Count; ++sysIndex)
                    {
                        SvgSystem system = Systems[sysIndex];
                        Voice voice = system.Staves[staffIndex].Voices[voiceIndex];
                        foreach(LocalizedMidiDurationDef lmdd in voice.LocalizedMidiDurationDefs)
                        {
                            lmdd.MsPosition = msPosition;
                            msPosition += lmdd.MsDuration;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If the filepath is an .svg file
        ///     returns filepath
        /// Else if the filepath is an .htm or .html file containing one or more embedded .svg files
        ///     returns the filepath of the first embedded .svg file
        /// Else
        ///     returns null.     
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public string GetPage1Path(string filepath)
        {
            string page1Path = null;
            string extension = Path.GetExtension(filepath);
            if(extension == ".svg")
            {
                page1Path = filepath;
            }
            else if(extension == ".htm" || extension == ".html")
            {
                try
                {
                    string directory = Path.GetDirectoryName(filepath);

                    Stream scoreStream = File.OpenRead(filepath);
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.DtdProcessing = DtdProcessing.Ignore;
                    using(XmlReader r = XmlReader.Create(scoreStream, settings))
                    {
                        M.ReadToXmlElementTag(r, "embed");
                        int count = r.AttributeCount;
                        for(int i = 0; i < count; ++i)
                        {
                            r.MoveToAttribute(i);
                            if(r.Name == "src")
                            {
                                if(r.Value.EndsWith(".svg"))
                                    page1Path = directory + "\\" + r.Value;
                                break;
                            }
                        }
                    }
                    scoreStream.Close();
                }
                catch(Exception)
                {
                }
            }
            return page1Path;
        }

        private void ReadMidiDefs(string page1Path)
        {
            Debug.Assert(Path.GetExtension(page1Path) == ".svg");
            try
            {
                Stream pageStream = File.OpenRead(page1Path);
                using(XmlReader r = XmlReader.Create(pageStream))
                {
                    do
                    {
                        M.ReadToXmlElementTag(r, "score:midiDefs", "svg");
                        if(r.Name == "score:midiDefs" && r.IsStartElement())
                        {
                            GetMidiChordAndRestDefs(r);
                        }
                    } while(!(r.Name == "svg" && !r.IsStartElement()));
                }
                pageStream.Close();
            }
            catch(Exception ex)
            {
                string msg = ex.Message + "\r\rException thrown in SvgScore.ReadMidiDefs().";
                throw new ApplicationException(msg);
            }
        }

        /// <summary>
        /// Loads a dictionary of midiChords. In each case, the key is the object's id.
        /// The value in the _midiChordDefs is a MidiChordDef containing the values in the SVG.
        /// </summary>
        /// <param name="r"></param>
        private void GetMidiChordAndRestDefs(XmlReader r)
        {
            Debug.Assert(r.Name == "score:midiDefs" && r.IsStartElement());

            _midiDurationDefs = new Dictionary<string, MidiDurationDef>();

            M.ReadToXmlElementTag(r, "score:midiChord", "score:midiRest");
            while(r.Name == "score:midiChord" || r.Name == "score:midiRest")
            {
                if(r.IsStartElement())
                {
                    if(r.Name == "score:midiChord")
                    {
                        MidiChordDef cDef = new MidiChordDef(r, null, null, 0);
                        _midiDurationDefs.Add(cDef.ID, cDef);
                    }
                    else
                    {
                        MidiRestDef rDef = new MidiRestDef(r);
                        _midiDurationDefs.Add(rDef.ID, rDef);
                    }
                }
                M.ReadToXmlElementTag(r, "score:midiChord", "score:midiRest", "score:midiDefs");
            }
        }

        private void LoadMultiplePages(string filepath)
        {
            string directory = Path.GetDirectoryName(filepath);
            int pageNumber = 1;
            Stream scoreStream = File.OpenRead(filepath);
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            using(XmlReader r = XmlReader.Create(scoreStream, settings))
            {
                M.ReadToXmlElementTag(r, "embed");
                while(r.Name == "embed")
                {
                    if(r.NodeType != XmlNodeType.EndElement)
                    {
                        int count = r.AttributeCount;
                        for(int i = 0; i < count; ++i)
                        {
                            r.MoveToAttribute(i);
                            if(r.Name == "src")
                            {
                                string pagePath = directory + "\\" + r.Value;
                                _pages.Add(new SvgPage(this, pageNumber++, pagePath));
                                break;
                            }
                        }
                    }
                    M.ReadToXmlElementTag(r, "embed", "body");
                }
                // r.Name is "body" here (EndElement)
            }
            scoreStream.Close();
        }

        protected virtual byte MidiChannel(int staffIndex) { throw new NotImplementedException(); }

        #region save SVG score
        /// <summary>
        /// Silently overwrites the .html and all .svg pages.
        /// An SVGScore consists of an .html file which references one .svg file per page of the score. 
        /// </summary>
        public void SaveSVGScore()
        {
            List<string> svgPagenames = SaveSVGPages();

            if(File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.CloseOutput = true;
            settings.NewLineOnAttributes = true;
            settings.NamespaceHandling = NamespaceHandling.OmitDuplicates;
            settings.Encoding = Encoding.GetEncoding("utf-8");

            using(XmlWriter w = XmlWriter.Create(FilePath, settings))
            {
                w.WriteDocType("html", null, null, null);
                w.WriteStartElement("html", "http://www.w3.org/1999/xhtml");
                w.WriteAttributeString("lang", "en");
                w.WriteAttributeString("xml", "lang", null, "en");

                WriteHTMLScoreHead(w, Path.GetFileNameWithoutExtension(FilePath));

                w.WriteStartElement("body");
                w.WriteAttributeString("style", "text-align:center");
                w.WriteStartElement("div");
                w.WriteAttributeString("id", "centredReferenceDiv");
                string styleString = "position:relative; text-align: left; top: 0px; padding-top: 0px; margin-top: 0px; width: " + 
                    _pageFormat.ScreenRight.ToString() + "px; margin-left: auto; margin-right: auto;";
                w.WriteAttributeString("style", styleString);

                w.WriteStartElement("div");
                w.WriteAttributeString("id", "svgPages");
                w.WriteAttributeString("style", "line-height: 0px;");

                foreach(string svgPagename in svgPagenames)
                {
                    w.WriteStartElement("embed");
                    w.WriteAttributeString("src", svgPagename);
                    w.WriteAttributeString("content-type", "image/svg+xml");
                    w.WriteAttributeString("class", "svgPage");
                    w.WriteAttributeString("width", _pageFormat.ScreenRight.ToString());
                    w.WriteAttributeString("height", _pageFormat.ScreenBottom.ToString());
                    w.WriteEndElement();
                    w.WriteStartElement("br");
                    w.WriteEndElement();
                }

                w.WriteEndElement(); // end svgPages div element

                WriteLinksAndPlayer(w,
                        _pageFormat.AboutLinkText,
                        _pageFormat.AboutLinkURL,
                        _pageFormat.Recording,
                        _pageFormat.LeftScreenMarginPos,
                        _pageFormat.Page1ScreenTitleY);

                w.WriteEndElement(); // end centredReferenceDiv div element

                WriteJavascriptIncludes(w);

                w.WriteEndElement(); // end body element
                w.WriteEndElement(); // end html element

                w.Close(); // close actually unnecessary because of the using statement.
            }
        }

        private void WriteHTMLScoreHead(XmlWriter w, string title)
        {
            w.WriteStartElement("head");
            w.WriteStartElement("title");
            w.WriteString(title);
            w.WriteEndElement(); // title

            w.WriteStartElement("link");
            w.WriteAttributeString("href", "http://james-ingram-act-two.de/fontsStyleSheet.css");
            w.WriteAttributeString("media", "all");
            w.WriteAttributeString("rel", "stylesheet");
            w.WriteAttributeString("type", "text/css");
            w.WriteEndElement(); // link

            w.WriteEndElement(); // head
        }

        /// <summary>
        /// The mp3Recording argument is currently ignored. Though I've left it in the pageFormat definition
        /// and Moritz dialogs for the moment.
        /// MP3 recordings should be included in the "About" documents for each composition instead.
        /// The MIDIPlayer "about" link and start button are created instead. (10.6.2012)
        /// </summary>
        /// <param name="w"></param>
        /// <param name="aboutLinkText"></param>
        /// <param name="aboutURL"></param>
        /// <param name="mp3recording"></param>
        /// <param name="left"></param>
        /// <param name="titleBaseline"></param>
        private void WriteLinksAndPlayer(
            XmlWriter w,
            string aboutLinkText,
            string aboutURL,
            string mp3recording,
            float left,
            float titleBaseline)
        {
            w.WriteStartElement("div");
            w.WriteAttributeString("id", "linksAndPlayer");
            w.WriteAttributeString("style", "font-family: Lucida Sans Unicode, Verdana, Arial, Geneva, Sans-Serif; font-size: 11px;");

            Debug.Assert(!String.IsNullOrEmpty(aboutURL) && !String.IsNullOrEmpty(aboutLinkText));

            w.WriteStartElement("a");
            w.WriteAttributeString("id", "aboutTheScoreLink"); 
            w.WriteAttributeString("style", "position: absolute; top: 17px; left: 52px;");
            w.WriteAttributeString("href", aboutURL);
            w.WriteString(aboutLinkText);
            w.WriteEndElement(); // aboutTheScoreLink

            w.WriteStartElement("a");
            w.WriteAttributeString("id", "aboutMidiPlayerLink");
            w.WriteAttributeString("style", "position: absolute; top: 17px; left: 150px; visibility: hidden");
            w.WriteAttributeString("href", "http://james-ingram-act-two.de/open-source/midiPlayer/aboutMidiPlayer.html");
            w.WriteString("About MIDI Player");
            w.WriteEndElement(); // aboutMidiPlayerLink

            w.WriteStartElement("button");
            w.WriteAttributeString("id", "midiPlayerButton");
            w.WriteAttributeString("style", "position: absolute; margin: 0px; top: 33px; left: 50px;");
            w.WriteString("MIDI Player");
            w.WriteEndElement(); // button

            // the midi output device selector is connected here in Javascript
            w.WriteStartElement("select");
            w.WriteAttributeString("id", "midiOutputDeviceSelector");
            w.WriteAttributeString("style", "font-size: 9pt; font-family: Lucida Sans Unicode, Verdana, Arial, Geneva, Sans-Serif; " +
                                    "position: absolute; top: 32px; left: 25px; width: 220px; visibility: hidden");
            w.WriteEndElement(); // select

            w.WriteEndElement(); // linksAndPlayer div
            #region obsolete
            /*****************
             * This is the code which used to create the mp3 player at the top of the score
            float linkHeight = 11;
            float linkTop;
            int mp3PlayerWidth = 300;
            int mp3PlayerHeight = 18;
            if(!String.IsNullOrEmpty(mp3recording))
            {
                float playerTop = titleBaseline - mp3PlayerHeight;
                StringBuilder styleSB = new StringBuilder();
                styleSB.Append("position: absolute; top: 0; left: 0; margin-left:25px; margin-top:" + playerTop.ToString(M.En_USNumberFormat) + "px");
                w.WriteStartElement("object");
                w.WriteAttributeString("type", "application/x-shockwave-flash");
                w.WriteAttributeString("data", M.MoritzFlashPlayerFolder + "/player_mp3_maxi.swf");
                w.WriteAttributeString("style", styleSB.ToString());
                w.WriteAttributeString("width", mp3PlayerWidth.ToString());
                w.WriteAttributeString("height", mp3PlayerHeight.ToString());
                w.WriteStartElement("param");
                w.WriteAttributeString("name", "movie");
                w.WriteAttributeString("value", M.MoritzFlashPlayerFolder + "/player_mp3_maxi.swf");
                w.WriteEndElement(); // param 1
                w.WriteStartElement("param");
                w.WriteAttributeString("name", "wmode");
                w.WriteAttributeString("value", "transparent");
                w.WriteEndElement(); // param 2
                w.WriteStartElement("param");
                w.WriteAttributeString("name", "FlashVars");
                w.WriteAttributeString("value", "mp3=" + M.Preferences.OnlineUserAudioFolder + "/" + 
                    mp3recording + "&" + "config=" + M.MoritzFlashPlayerFolder + "/config_maxi.txt");
                w.WriteEndElement(); // param 3
                w.WriteEndElement(); // object
            }
            ***********************/
            #endregion
        }

        private void WriteJavascriptIncludes(XmlWriter w)
        {
            w.WriteComment("Midibridge software is copyright 2012 abudaan http://abumarkub.net \n" +
                "the code is licensed under MIT\n" + "http://abumarkub.net/midibridge/license" + "\n");
            w.WriteStartElement("script");
            w.WriteAttributeString("src", "lib/midibridge-0.6.3.min.js");
            w.WriteAttributeString("type", "text/javascript");
            w.WriteEndElement();

            w.WriteComment("Midiplayer software is copyright 2012 James Ingram\n" +
            "the code is licensed under MIT\n" + "http://james-ingram-act-two.de/open-source/license.html" + "\n");
            w.WriteStartElement("script");
            w.WriteAttributeString("src", "lib/midiPlayer-12.6.2012.min.js");
            w.WriteAttributeString("type", "text/javascript");
            w.WriteEndElement(); 
        }

        private List<string> SaveSVGPages()
        {
            List<string> pageFilenames = new List<string>();

            int pageNumber = 1;
            foreach(SvgPage page in _pages)
            {
                string pageFilename = Path.GetFileNameWithoutExtension(FilePath) +
                    " page " + (pageNumber).ToString() + ".svg";
                string pagePath = Path.GetDirectoryName(FilePath) + @"\" + pageFilename;

                pageFilenames.Add(pageFilename);

                SaveSVGPage(pagePath, page, this.Metadata, pageNumber);
                pageNumber++;
            }

            return pageFilenames;
        }
        /// <summary>
        /// Writes an SVG file containing one page of the score.
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="firstSystemIndex"></param>
        /// <param name="nSystemsPerPage"></param>
        public void SaveSVGPage(string pagePath, SvgPage page, Metadata metadata, int pageNumber)
        {
            if(File.Exists(pagePath))
            {
                File.Delete(pagePath);
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.CloseOutput = true;
            settings.NewLineOnAttributes = true;
            settings.NamespaceHandling = NamespaceHandling.OmitDuplicates;
            settings.Encoding = Encoding.GetEncoding("utf-8");

            using(SvgWriter w = new SvgWriter(pagePath, settings))
            {
                page.WriteSVG(w, metadata, pageNumber);
            }
        }

        public void WriteSymbolDefinitions(SvgWriter w)
        {
            Debug.Assert(Notator != null);
            Notator.SymbolSet.WriteSymbolDefinitions(w);
        }
        public void WriteJavaScriptDefinitions(SvgWriter w)
        {
            Debug.Assert(Notator != null);
            // Other sets of JavaScript definitions are also possible...
            Notator.SymbolSet.WriteJavaScriptDefinitions(w);
        }

        #endregion save SVG score

        #region fields loaded from .capx files
        public Metadata Metadata = null;
        internal List<DrawObject> PageObjects = new List<DrawObject>();
        public List<SvgSystem> Systems = new List<SvgSystem>();
        #endregion
        #region moritz-specific private fields

        protected string _filename = "";
        public string Filename { get { return _filename; } }

        #region Debugging fields
#if Debug
		/// <summary>
		/// The duration symbols to be played by the performer in system order
		/// </summary>
		public List<List<DurationSymbol>> PerformerDurationSymbolList = new List<List<DurationSymbol>>();
		/// <summary>
		/// The duration symbols to be played by the assistant in system order
		/// </summary>
		public List<List<DurationSymbol>> AssistantDurationSymbolList = new List<List<DurationSymbol>>();
#endif
        #endregion
        #endregion


        /// <summary>
        /// Adds the staff name to the first barline of each staff in the score.
        /// </summary>
        private void SetStaffNames()
        {
            foreach(SvgSystem system in Systems)
            {
                for(int staffNumber = 1; staffNumber <= system.Staves.Count; staffNumber++)
                {
                    foreach(NoteObject noteObject in system.Staves[staffNumber - 1].Voices[0].NoteObjects)
                    {
                        Barline firstBarline = noteObject as Barline;
                        if(firstBarline != null)
                        {
                            //string staffName = staffNumber.ToString();
                            string staffName = system.Staves[staffNumber - 1].Staffname;
                            TextInfo textInfo = new TextInfo(staffName, "Times New Roman", _pageFormat.StaffNameFontHeight, TextHorizAlign.center);
                            Text staffNameText = new Text(firstBarline, textInfo);
                            firstBarline.DrawObjects.Add(staffNameText);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// There is currently still one bar per system.
        /// Each voice ends with a barline.
        /// </summary>
        protected virtual void ReplaceConsecutiveRestsInBars(int minimumCrotchetDuration)
        {
            foreach(SvgSystem system in Systems)
            {
                foreach(Voice voice in system.Voices)
                {
                    Debug.Assert(voice.NoteObjects[voice.NoteObjects.Count - 1] is Barline);
                    // contains lists of consecutive rest indices
                    List<List<int>> restsToReplace = new List<List<int>>();
                    #region find the consecutive rests
                    List<int> consecRestIndices = new List<int>();
                    for(int i = 0; i < voice.NoteObjects.Count - 1; i++)
                    {
                        Debug.Assert(!(voice.NoteObjects[i] is Barline));

                        RestSymbol rest1 = voice.NoteObjects[i] as RestSymbol;
                        RestSymbol rest2 = voice.NoteObjects[i + 1] as RestSymbol;
                        if(rest1 != null && rest2 != null)
                        {
                            if(!consecRestIndices.Contains(i))
                            {
                                consecRestIndices.Add(i);
                            }
                            consecRestIndices.Add(i + 1);
                        }
                        else
                        {
                            if(consecRestIndices != null && consecRestIndices.Count > 0)
                            {
                                restsToReplace.Add(consecRestIndices);
                                consecRestIndices = new List<int>();
                            }
                        }
                    }
                    #endregion
                    #region replace the consecutive rests
                    if(restsToReplace.Count > 0)
                    {
                        for(int i = restsToReplace.Count - 1; i >= 0; i--)
                        {
                            List<int> indToReplace = restsToReplace[i];
                            int msDuration = 0;
                            int msPos = 0;
                            float fontSize = 0F;
                            for(int j = indToReplace.Count - 1; j >= 0; j--)
                            {
                                RestSymbol rest = voice.NoteObjects[indToReplace[j]] as RestSymbol;
                                Debug.Assert(rest != null);
                                msDuration += rest.MsDuration;
                                msPos = rest.MsPosition;
                                fontSize = rest.FontHeight;
                                voice.NoteObjects.RemoveAt(indToReplace[j]);
                            }
                            LocalizedMidiDurationDef lmdd = new LocalizedMidiDurationDef(msDuration);
                            lmdd.MsPosition = msPos;
                            RestSymbol newRest = new RestSymbol(voice, lmdd, minimumCrotchetDuration, _pageFormat.MusicFontHeight);
                            newRest.MsPosition = msPos;
                            voice.NoteObjects.Insert(indToReplace[0], newRest);
                        }
                    }
                    #endregion
                }
            }
        }
        /// <summary>
        /// If barNumbers is null, systems will be given 5 bars each by default.
        /// Otherwise:
        /// Each barnumber in the argument barNumbers will be at the start of a system when this function returns.
        /// All barNumbers must be greater than 0 and less or equal to than the current Systems.Count.
        /// Barnumber 1 must be present. No barNumbers may be repeated.
        /// barNumbers beyond the end of the score are silently ignored.
        /// </summary>
        public void SetSystemsToBeginAtBars(List<int> barNumbers)
        {
            List<int> barIndices = new List<int>();
            if(barNumbers == null || barNumbers.Count == 0)
            {
                int barIndex = 0;
                while(barIndex < Systems.Count)
                {
                    barIndices.Add(barIndex);
                    barIndex += _pageFormat.DefaultNumberOfBarsPerSystem;
                }
            }
            else
            {
                foreach(int barNumber in barNumbers)
                {
                    barIndices.Add(barNumber - 1);
                }
            }

            Debug.Assert(barIndices.Contains(0));
            // barIndices beyond the end of the score are silently ignored.

            List<int> systemIndices = new List<int>();
            for(int i = 0; i < Systems.Count; i++)
            {
                if(!barIndices.Contains(i))
                    systemIndices.Add(i);
            }

            DoJoinSystems(systemIndices);
        }
        /// <summary>
        /// The systemNumbers are the current numbers of the systems which are to be joined to the following system.
        /// All systemNumbers must be greater than 0 and less than the current Systems.Count.
        /// No systemNumbers may be repeated.
        /// </summary>
        public void JoinSystems(params int[] systemNumbers)
        {
            List<int> systemNums = new List<int>(systemNumbers);
            systemNums.Sort();

            #region conditions
            Debug.Assert(systemNums.Count > 0 && systemNums[0] > 0);
            for(int i = 1; i < systemNums.Count; i++)
            {
                Debug.Assert(
                    systemNums[i] > 0
                    && systemNums[i] != systemNums[i - 1]
                    && systemNums[i] < Systems.Count);
            }
            #endregion

            DoJoinSystems(systemNums);
        }
        /// <summary>
        /// Joins systems so as to put barsPerSystem bars on each system.
        /// </summary>
        /// <param name="barsPerSystem"></param>
        protected void SetBarsPerSystem(int barsPerSystem)
        {
            List<int> systemIndices = new List<int>();
            for(int i = 1; i < Systems.Count; i++)
                systemIndices.Add(i);
            for(int i = barsPerSystem; i < Systems.Count; i += barsPerSystem)
                systemIndices.Remove(i);

            DoJoinSystems(systemIndices);
        }

        #region private for JoinSystems() and SetBarsPerSystem()
        /// <summary>
        /// Copies Systems[systemIndex]'s content to the end of the previous system (taking account of clefs),
        /// then removes Systems[systemIndex] from the Systems list.
        /// </summary>
        /// <param name="barlineIndex"></param>
        private void JoinToPreviousSystem(int systemIndex)
        {
            Debug.Assert(Systems.Count > 1 && Systems.Count > systemIndex);
            SvgSystem system1 = Systems[systemIndex - 1];
            SvgSystem system2 = Systems[systemIndex];
            Debug.Assert(system1.Staves.Count == system2.Staves.Count);

            for(int staffIndex = 0; staffIndex < system2.Staves.Count; staffIndex++)
            {
                for(int voiceIndex = 0; voiceIndex < system2.Staves[staffIndex].Voices.Count; voiceIndex++)
                {
                    ClefSign currentSys1Clef = null;
                    ClefSign previousSys1Clef = null;
                    ClefSign firstSys2Clef = null;
                    Voice voice1 = system1.Staves[staffIndex].Voices[voiceIndex];
                    Voice voice2 = system2.Staves[staffIndex].Voices[voiceIndex];
                    #region find currentSys1Clef
                    foreach(NoteObject noteObject in voice1.NoteObjects)
                    {
                        ClefSign clef = noteObject as ClefSign;
                        if(clef != null)
                        {
                            previousSys1Clef = currentSys1Clef;
                            currentSys1Clef = clef;
                        }
                    }
                    if(currentSys1Clef != null && currentSys1Clef == voice1.NoteObjects[voice1.NoteObjects.Count - 1])
                    {
                        voice1.NoteObjects.Remove(currentSys1Clef);
                        currentSys1Clef = previousSys1Clef;
                    }
                    #endregion
                    firstSys2Clef = voice2.NoteObjects[0] as ClefSign;
                    if(currentSys1Clef != null && firstSys2Clef != null
                        && currentSys1Clef.ClefName == firstSys2Clef.ClefName)
                    {
                        voice2.NoteObjects.Remove(firstSys2Clef);
                    }

                    try
                    {
                        voice1.AppendNoteObjects(voice2.NoteObjects);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            Systems.Remove(system2);
            // system2.Dispose() would be a good idea here...
            system2 = null;
        }
        private void DoJoinSystems(List<int> systemIndices)
        {
            for(int i = 0; i < systemIndices.Count; i++)
            {
                int s = systemIndices[i];
                JoinToPreviousSystem(s);
                for(int j = i; j < systemIndices.Count; j++)
                    systemIndices[j]--;
            }
            MoveInitialBarlinesToPreviousSystem();
        }
        #endregion

        /// <summary>
        /// The score contains the correct number of bars per system.
        /// Now, if a barline comes before any chords in a staff, it is moved to the end of the corresponding
        /// staff in the previous system -- or deleted altogether if it is in the first System.
        /// </summary>
        private void MoveInitialBarlinesToPreviousSystem()
        {
            for(int systemIndex = 0; systemIndex < Systems.Count; systemIndex++)
            {
                SvgSystem system = Systems[systemIndex];
                for(int staffIndex = 0; staffIndex < system.Staves.Count; staffIndex++)
                {
                    Staff staff = system.Staves[staffIndex];
                    for(int voiceIndex = 0; voiceIndex < staff.Voices.Count; voiceIndex++)
                    {
                        Voice voice = staff.Voices[voiceIndex];
                        Barline barline = voice.InitialBarline;
                        if(barline != null)
                        {
                            if(systemIndex > 0)
                            {
                                Voice voiceInPreviousSystem = Systems[systemIndex - 1].Staves[staffIndex].Voices[voiceIndex];
                                voiceInPreviousSystem.NoteObjects.Add(new Barline(voiceInPreviousSystem, barline.BarlineType));
                            }
                            voice.NoteObjects.Remove(barline);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Opens the score in the program which is set by the system to open .svg files.
        /// This function was originally used to open capella files. See the comment. Maybe I'll have to
        /// stipulate the current browser, and open a new browser window somehow...
        /// </summary>
        public void OpenSVGScore()
        {
            try
            {
                global::System.Diagnostics.Process.Start(FilePath);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region protected functions

        /// <summary>
        /// There is currently one bar per System. 
        /// All Duration Symbols have been constructed in voice.NoteObjects.
        /// There are no barlines in the score yet.
        /// Add a barline to each Voice in the staff, adding a double bar to the final system
        /// </summary>
        /// <param name="barlineType"></param>
        /// <param name="systemNumbers"></param>
        private void SetBarlines()
        {
            for(int systemIndex = 0; systemIndex < Systems.Count; ++systemIndex)
            {
                foreach(Staff staff in Systems[systemIndex].Staves)
                {
                    foreach(Voice voice in staff.Voices)
                    {
                        Debug.Assert(voice.NoteObjects.Count > 0
                            && !(voice.NoteObjects[voice.NoteObjects.Count - 1] is Barline));

                        Barline barline = new Barline(voice);
                        if(systemIndex == Systems.Count - 1)
                            barline.BarlineType = BarlineType.end;
                        else
                            barline.BarlineType = BarlineType.single;

                        voice.NoteObjects.Add(barline);
                    }
                }
            }
        }

        private List<int> GetGroupBottomStaffIndices(List<int> groupSizes, int nStaves)
        {
            List<int> returnList = new List<int>();
            if(groupSizes == null || groupSizes.Count == 0)
            {
                for(int i = 0; i < nStaves; i++)
                    returnList.Add(i);
            }
            else
            {
                int bottomOfGroup = groupSizes[0] - 1;
                returnList.Add(bottomOfGroup);
                int i = 1;
                while(i < groupSizes.Count)
                {
                    bottomOfGroup += groupSizes[i++];
                    returnList.Add(bottomOfGroup);
                }
            }
            return returnList;
        }

        /// <summary>
        /// When this function is called, every system still contains one bar, and all systems have the same number
        /// of staves and voices as System[0].
        /// This function:
        /// 1. adds a barline at the end of each system=bar, 
        /// 2. joins the bars into systems according to the user's options,
        /// 3. sets the visibility of naturals (if the chords have any noteheads)
        /// 3. adds a barline at the start of each system, after the clef.
        /// 4. adds a barnumber to the first barline on each system.
        /// 5. adds the staff's name to the first barline on each staff.     
        /// </summary>
        protected void FinalizeSystemStructure()
        {
            #region preconditions
            int nStaves = Systems[0].Staves.Count;
            foreach(SvgSystem system in Systems)
            {
                Debug.Assert(system.Staves.Count == nStaves);
            }
            List<int> nVoices = new List<int>();
            foreach(Staff staff in Systems[0].Staves)
                nVoices.Add(staff.Voices.Count);
 
            foreach(SvgSystem system in Systems)
            {
                for(int i = 0; i < system.Staves.Count; ++i)
                {
                    Debug.Assert(system.Staves[i].Voices.Count == nVoices[i]);
                }
            }
            #endregion preconditions

            SetBarlines();

            ReplaceConsecutiveRestsInBars(_pageFormat.MinimumCrotchetDuration);
            SetSystemsToBeginAtBars(_pageFormat.SystemStartBars);

            FinalizeAccidentals();
            AddBarlineAtStartOfEachSystem();
            AddBarNumbers();
            SetStaffNames();
        }

        protected void CreatePages()
        {
            int pageNumber = 1;
            int systemIndex = 0;
            while(systemIndex < Systems.Count)
            {
                _pages.Add(NewSvgPage(pageNumber++, ref systemIndex));
            }
        }

        protected SvgPage NewSvgPage(int pageNumber, ref int systemIndex)
        {
            TextInfo infoTextInfo = GetInfoTextAtTopOfPage(pageNumber);

            List<SvgSystem> systemsOnPage = new List<SvgSystem>();
            bool lastPage = true;
            float systemHeight = 0;
            float frameHeight;
            if(pageNumber == 1)
                frameHeight = _pageFormat.FirstPageFrameHeight;
            else
                frameHeight = _pageFormat.OtherPagesFrameHeight;

            float systemHeightsTotal = 0;
            while(systemIndex < Systems.Count)
            {
                if(Systems[systemIndex].Metrics != null)
                {
                    systemHeight = Systems[systemIndex].Metrics.NotesBottom - Systems[systemIndex].Metrics.NotesTop;

                    systemHeightsTotal += systemHeight;
                    if(systemHeightsTotal > frameHeight)
                    {
                        lastPage = false;
                        break;
                    }

                    systemHeightsTotal += _pageFormat.DefaultDistanceBetweenSystems;
                }

                systemsOnPage.Add(Systems[systemIndex]);

                systemIndex++;
            }

            return new SvgPage(this, _pageFormat, pageNumber, infoTextInfo, systemsOnPage, lastPage);
        }

        private TextInfo GetInfoTextAtTopOfPage(int pageNumber)
        {
            string infoAtTopOfPage;
            string fileName;
            if(String.IsNullOrEmpty(_filename))
                fileName = "";
            else
                fileName = Path.GetFileNameWithoutExtension(_filename);

            if(String.IsNullOrEmpty(fileName))
                infoAtTopOfPage = "page " + pageNumber.ToString();
            else
                infoAtTopOfPage = fileName + ": page " + pageNumber.ToString();

            if(Metadata != null)
                infoAtTopOfPage = infoAtTopOfPage + ", " + Metadata.Date;

            return new TextInfo(infoAtTopOfPage, "Arial", 72F, TextHorizAlign.left);
        }


        /// <summary>
        /// The first duration symbol in the staff.
        /// </summary>
        /// <returns></returns>
        protected DurationSymbol FirstDurationSymbol(Staff staff)
        {
            DurationSymbol firstDurationSymbol = null;
            Voice voice = staff.Voices[0];
            foreach(NoteObject noteObject in voice.NoteObjects)
            {
                firstDurationSymbol = noteObject as DurationSymbol;
                if(firstDurationSymbol != null)
                    break;
            }
            return firstDurationSymbol;
        }

        #endregion protected functions
        private ChordSymbol ConvertLastChordSymbol(Voice voice, int durationToSubtract)
        {
            ChordSymbol lastChordSymbol = null;
            int lastChordIndex = 0;
            Barline lastBarline = null;
            for(int i = 0; i < voice.NoteObjects.Count; ++i)
            {
                ChordSymbol cs = voice.NoteObjects[i] as ChordSymbol;
                if(cs != null)
                {
                    lastChordSymbol = cs;
                    lastChordIndex = i;
                }
                Barline b = voice.NoteObjects[i] as Barline;
                if(b != null)
                    lastBarline = b;
            }
            LocalizedMidiDurationDef lmdd = lastChordSymbol.LocalizedMidiDurationDef;
            LocalizedMidiDurationDef newlmdd = new LocalizedMidiDurationDef(lmdd.MidiChordDef,lmdd.MsPosition,lmdd.MsDuration - durationToSubtract);
            ChordSymbol replacementChord = new ChordSymbol(voice, newlmdd, _pageFormat.MinimumCrotchetDuration, _pageFormat.MusicFontHeight);

            voice.NoteObjects.RemoveAt(lastChordIndex);
            voice.NoteObjects.Insert(lastChordIndex, replacementChord);
            return replacementChord;
        }

        private void FinalizeAccidentals()
        {
            for(int staffIndex = 0; staffIndex < Systems[0].Staves.Count; staffIndex++)
            {
                NoteObjectMoment previousStaffMoment = null;
                foreach(SvgSystem system in Systems)
                {
                    Staff staff = system.Staves[staffIndex];
                    {
                        previousStaffMoment = staff.FinalizeAccidentals(previousStaffMoment);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a bar number to the first Barline in the top voice of each system except the first.
        /// </summary>
        private void AddBarNumbers()
        {
            int barNumber = 1;
            foreach(SvgSystem system in Systems)
            {
                Voice voice = system.Staves[0].Voices[0];
                bool isFirstBarline = true;
                for(int i = 0; i < voice.NoteObjects.Count - 1; i++)
                {
                    Barline barline = voice.NoteObjects[i] as Barline;
                    if(barline != null)
                    {
                        if(isFirstBarline && system != Systems[0])
                        {
                            float fontHeight = _pageFormat.Gap * 2F;

                            float paddingX = 22F;
                            if(barNumber > 9)
                                paddingX = 10F;
                            float paddingY = 22F;

                            float strokeWidth = _pageFormat.StafflineStemStrokeWidth * 1.2F;
                            Text framedBarnumber = barline.SetFramedText(barNumber.ToString(), "Arial", fontHeight, paddingX, paddingY, strokeWidth);
                            barline.DrawObjects.Add(framedBarnumber);
                            isFirstBarline = false;
                        }
                        barNumber++;
                    }
                }
            }
        }

        /// <summary>
        /// Inserts a barline at the start of the first bar in each voice in each staff in each system.
        /// If the first Noteobject in the voice is a clef, the barline is inserted after the clef.
        /// </summary>
        private void AddBarlineAtStartOfEachSystem()
        {
            foreach(SvgSystem system in Systems)
            {
                foreach(Staff staff in system.Staves)
                {
                    foreach(Voice voice in staff.Voices)
                    {
                        if(voice.NoteObjects[0] is ClefSign)
                        {
                            voice.NoteObjects.Insert(1, new Barline(voice));
                        }
                        else
                        {
                            voice.NoteObjects.Insert(0, new Barline(voice));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This value is used to provide a unique id for objects in SVG files.
        /// </summary>
        public static string UniqueID_Number { get { return (++_uniqueID_Number).ToString(); } }
        private static int _uniqueID_Number = 0;

        protected List<SvgPage> _pages = new List<SvgPage>();

        public PageFormat PageFormat { get { return _pageFormat; } } 
        protected PageFormat _pageFormat = null;

        internal Dictionary<string, MidiDurationDef> MidiDurationDefs { get { return _midiDurationDefs; } } 
        private Dictionary<string, MidiDurationDef> _midiDurationDefs;

        public Notator Notator = null;

        /// <summary>
        /// Needed while creating the PerformanceOptionsDialog
        /// </summary>
        public IEnumerable<string> Staffnames
        {
            get
            {
                Debug.Assert(_pages.Count > 0);
                Debug.Assert(_pages[0].Systems.Count > 0);
                Debug.Assert(_pages[0].Systems[0].Staves.Count > 0);
                foreach(Staff staff in _pages[0].Systems[0].Staves)
                {
                    Debug.Assert(!String.IsNullOrEmpty(staff.Staffname));
                    yield return staff.Staffname;
                }
            }
        }
        public IEnumerable<Voice> Voices
        {
            get
            {
                foreach(SvgSystem system in Systems)
                    foreach(Staff staff in system.Staves)
                        foreach(Voice voice in staff.Voices)
                            yield return voice;
            }
        }
        public string FilePath;
    }

    internal class ErrorInScoreException : ApplicationException
    {
        public ErrorInScoreException(string message)
            : base(message)
        { }
    }

    internal class PerformanceErrorException : ApplicationException
    {
        public PerformanceErrorException(string message)
            : base(message)
        { }
    }
}

