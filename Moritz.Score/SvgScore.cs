using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using Moritz.Globals;
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
                        foreach(IUniqueDef iu in voice.UniqueDefs)
                        {
                            iu.MsPosition = msPosition;
                            msPosition += iu.MsDuration;
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
        /// Returns the number of SVG pages saved.
        /// </summary>
        public int SaveSVGScore()
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

                w.WriteEndElement(); // end centredReferenceDiv div element

                w.WriteEndElement(); // end body element
                w.WriteEndElement(); // end html element

                w.Close(); // close actually unnecessary because of the using statement.
            }
            
            return svgPagenames.Count;
        }

        private void WriteHTMLScoreHead(XmlWriter w, string title)
        {
            w.WriteStartElement("head");
            w.WriteStartElement("title");
            w.WriteString(title);
            w.WriteEndElement(); // title
            w.WriteEndElement(); // head
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

                SaveSVGPage(pagePath, page, this.Metadata, this._performanceControlDef, pageNumber);
                pageNumber++;
            }

            return pageFilenames;
        }
        /// <summary>
        /// Writes an SVG file containing one page of the score.
        /// The metadata and scorePerformanceControlDef are only written to page 1.
        /// </summary>
        public void SaveSVGPage(string pagePath, SvgPage page, Metadata metadata, PerformanceControlDef scorePerfCtlDef, int pageNumber)
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
                page.WriteSVG(w, metadata, scorePerfCtlDef, pageNumber);
            }
        }

        public void WriteSymbolDefinitions(SvgWriter w)
        {
            Debug.Assert(Notator != null);
            Notator.SymbolSet.WriteSymbolDefinitions(w);
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
                            RestDef umrd = new RestDef(msPos, msDuration);
                            RestSymbol newRest = new RestSymbol(voice, umrd, minimumCrotchetDuration, _pageFormat.MusicFontHeight);
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
                // If a staff has two voices, both contain the same clefTypes (some clefs may be invisible).
                string clefTypeAtEndOfStaff1 = FindClefTypeAtEndOfStaff1(system1.Staves[staffIndex].Voices[0]);

                for(int voiceIndex = 0; voiceIndex < system2.Staves[staffIndex].Voices.Count; voiceIndex++)
                {
                    Voice voice1 = system1.Staves[staffIndex].Voices[voiceIndex];
                    Voice voice2 = system2.Staves[staffIndex].Voices[voiceIndex];
                    ClefSymbol voice2FirstClef = voice2.NoteObjects[0] as ClefSymbol;

                    Debug.Assert(voice2FirstClef != null && clefTypeAtEndOfStaff1 == voice2FirstClef.ClefType);

                    voice2.NoteObjects.Remove(voice2FirstClef);

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
            system2 = null;
        }

        private string FindClefTypeAtEndOfStaff1(Voice staff1voice0)
        {
            ClefSymbol mainStaff1Clef = staff1voice0.NoteObjects[0] as ClefSymbol;
            Debug.Assert(mainStaff1Clef != null);

            string clefTypeAtEndOfStaff1 = mainStaff1Clef.ClefType;
            foreach(NoteObject noteObject in staff1voice0.NoteObjects)
            {
                ClefChangeSymbol ccs = noteObject as ClefChangeSymbol;
                if(ccs != null)
                {
                    clefTypeAtEndOfStaff1 = ccs.ClefType;
                }

            }
            return clefTypeAtEndOfStaff1;
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

            MoveRestClefChangesToEndsOfSystems();

            FinalizeAccidentals();
            AddBarlineAtStartOfEachSystem();
            AddBarNumbers();
            SetStaffNames();
        }

        /// <summary>
        /// If small ClefSigns have no following chord in their voice, they are moved before the final barline.
        /// </summary>
        private void MoveRestClefChangesToEndsOfSystems()
        {
            foreach(SvgSystem system in Systems)
            {
                foreach(Voice voice in system.Voices)
                {
                    MoveRestClefChangesToEndOfVoice(voice);
                }
            }
        }

        /// <summary>
        /// If a small ClefSymbol has no following chord in the voice, it is moved before the final barline.
        /// </summary>
        private void MoveRestClefChangesToEndOfVoice(Voice voice)
        {
            bool restFound = false;
            ClefSymbol smallClef = null;
            int smallClefIndex = -1;
            for(int i = voice.NoteObjects.Count - 1; i >= 0; --i)
            {
                if(voice.NoteObjects[i] is OutputChordSymbol)
                {
                    break;
                }
                if(voice.NoteObjects[i] is RestSymbol)
                {
                    restFound = true;
                }
                ClefSymbol clef = voice.NoteObjects[i] as ClefSymbol;
                if(clef != null && clef.FontHeight == _pageFormat.CautionaryNoteheadsFontHeight)
                {
                    smallClef = clef;
                    smallClefIndex = i;
                    break;
                }
            }
            if(restFound && (smallClef != null))
            {
                voice.NoteObjects.RemoveAt(smallClefIndex);
                Debug.Assert(voice.NoteObjects[voice.NoteObjects.Count - 1] is Barline);
                voice.NoteObjects.Insert(voice.NoteObjects.Count - 1, smallClef);
            }
        }

        protected void CreatePages(PerformanceControlDef globalPerformanceControlDef)
        {
            int pageNumber = 1;
            int systemIndex = 0;
            this._performanceControlDef = globalPerformanceControlDef;

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
                        if(voice.NoteObjects[0] is ClefSymbol)
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

        private PerformanceControlDef _performanceControlDef;

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

