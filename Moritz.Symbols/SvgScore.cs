using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using Moritz.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
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
			/// The Metadata.MainTitle is the large title displayed at the top centre of page 1
			/// of the score. It is also the name of the folder inside the standard Moritz scores folder
			/// used for saving all the scores components.
			Metadata = new Metadata
			{
				MainTitle = scoreTitleName,
				Keywords = keywords,
				Comment = comment,
				Date = M.NowString
			};
		}

        protected virtual byte MidiChannel(int staffIndex) { throw new NotImplementedException(); }

        #region save multi-page score
        /// <summary>
        /// Silently overwrites the .html and all .svg pages.
        /// An SVGScore consists of an .html file which references one .svg file per page of the score. 
        /// </summary>
        public void SaveMultiPageScore()
        {
            List<string> svgPagenames = SaveSVGPages();

            if(File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }

            XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = ("\t"),
				CloseOutput = true,
				NewLineOnAttributes = true,
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
				Encoding = Encoding.GetEncoding("utf-8")
			};


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
                w.WriteAttributeString("class", "centredReferenceDiv");
                string styleString = "position:relative; text-align: left; top: 0px; padding-top: 0px; margin-top: 0px; width: " + 
                    _pageFormat.ScreenRight.ToString() + "px; margin-left: auto; margin-right: auto;";
                w.WriteAttributeString("style", styleString);

                w.WriteStartElement("div");
                w.WriteAttributeString("class", "svgPages");
                w.WriteAttributeString("style", "line-height: 0px;");

                foreach(string svgPagename in svgPagenames)
                {
                    w.WriteStartElement("embed");
                    w.WriteAttributeString("src", svgPagename);
                    w.WriteAttributeString("content-type", "image/svg+xml");
                    w.WriteAttributeString("class", "svgPage");
                    w.WriteAttributeString("width", M.FloatToShortString(_pageFormat.ScreenRight));
                    w.WriteAttributeString("height", M.FloatToShortString(_pageFormat.ScreenBottom));
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

                SaveSVGPage(pagePath, page, this.Metadata);
                pageNumber++;
            }

            return pageFilenames;
        }
        /// <summary>
        /// Writes an SVG file containing one page of the score.
        /// </summary>
        public void SaveSVGPage(string pagePath, SvgPage page, Metadata metadata)
        {
            if(File.Exists(pagePath))
            {
                File.Delete(pagePath);
            }

            XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = ("\t"),
				CloseOutput = true,
				NewLineOnAttributes = true,
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
				Encoding = Encoding.GetEncoding("utf-8")
			};

            using(SvgWriter w = new SvgWriter(pagePath, settings))
            {
                page.WriteSVG(w, metadata);
            }
        }

        public void WriteDefs(SvgWriter w, int pageNumber)
        {
            Debug.Assert(Notator != null);
			w.SvgStartDefs(null);
			WriteStyle(w, pageNumber);
			Notator.SymbolSet.WriteSymbolDefinitions(w, _pageFormat);
			w.SvgEndDefs(); // end of defs
		}

        private void WriteStyle(SvgWriter w, int pageNumber)
		{
            StringBuilder css = GetStyles(_pageFormat, pageNumber);

            w.WriteStartElement("style");
			w.WriteAttributeString("type", "text/css");
            w.WriteString(css.ToString());
            w.WriteEndElement();
		}

        private StringBuilder GetStyles(PageFormat pageFormat, int pageNumber)
        {

			List<CSSObjectClass> usedCSSObjectClasses = new List<CSSObjectClass>(Metrics.UsedCSSObjectClasses);
			List<CSSColorClass> usedCSSColorClasses = new List<CSSColorClass>(Metrics.UsedCSSColorClasses);
			List<ClefID> usedClefIDs = new List<ClefID>(ClefMetrics.UsedClefIDs) as List<ClefID>;
            List<FlagID> usedFlagIDs = new List<FlagID>(FlagsMetrics.UsedFlagIDs) as List<FlagID>;

            string fontDefs =
            #region fontDefs string
@"
			@font-face
			{
				font-family: 'CLicht';
				src: url('http://james-ingram-act-two.de/fonts/clicht_plain-webfont.eot');
				src: url('http://james-ingram-act-two.de/fonts/clicht_plain-webfont.eot?#iefix') format('embedded-opentype'),
				url('http://james-ingram-act-two.de/fonts/clicht_plain-webfont.woff') format('woff'),
				url('http://james-ingram-act-two.de/fonts/clicht_plain-webfont.ttf') format('truetype'),
				url('http://james-ingram-act-two.de/fonts/clicht_plain-webfont.svg#webfontl9D2oOyX') format('svg');
				font-weight: normal;
				font-style: normal;
			}
			@font-face
			{
				font-family: 'Arial';
				src: url('http://james-ingram-act-two.de/fonts/arial.ttf') format('truetype');
				font-weight:400;
				font-style: normal;
			}
			@font-face
			{
				font-family: 'Open Sans';
				src: url('http://james-ingram-act-two.de/fonts/OpenSans-Regular.ttf') format('truetype');
				font-weight:400;
				font-style: normal;
			}
			@font-face
			{
				font-family: 'Open Sans Condensed';
				src: url('http://james-ingram-act-two.de/fonts/OpenSans-CondBold.ttf') format('truetype');
				font-weight:600;
				font-style: normal;
			}
		";
            #endregion fontDefs string
            StringBuilder stylesSB = new StringBuilder(fontDefs);
            stylesSB.Append("    "); // just for formatting
            stylesSB.Append(FontStyles(pageFormat, pageNumber, usedCSSObjectClasses, usedCSSColorClasses, usedClefIDs));
            bool defineFlagStyle = HasFlag(usedFlagIDs);
            stylesSB.Append(LineStyles(pageFormat, usedCSSObjectClasses, pageNumber, defineFlagStyle));
            if(usedCSSObjectClasses.Contains(CSSObjectClass.inputStaff))
            {
                bool defineInputFlagStyle = HasInputFlag(usedFlagIDs);
                stylesSB.Append(InputLineStyles(pageFormat, usedCSSObjectClasses, defineInputFlagStyle));
            }

            return stylesSB;
        }

        private bool HasFlag(List<FlagID> usedFlagIDs)
        {
            bool rval = false;
            foreach(FlagID flagID in usedFlagIDs)
            {
                if((flagID.ToString().StartsWith("input")) == false)
                {
                    rval = true;
                    break;
                }
            }
            return rval;
        }
        private bool HasInputFlag(List<FlagID> usedFlagIDs)
        {
            bool rval = false;
            foreach(FlagID flagID in usedFlagIDs)
            {
                if(flagID.ToString().StartsWith("input"))
                {
                    rval = true;
                    break;
                }
            }
            return rval;
        }

        #region font styles

        private StringBuilder FontStyles(PageFormat pageFormat, int pageNumber, List<CSSObjectClass> usedCSSObjectClasses,
											List<CSSColorClass> usedCSSColorClasses, List<ClefID> usedClefIDs)
        {
            StringBuilder fontStyles = new StringBuilder();
            #region text
            #region Open Sans (Titles)        
            if(pageNumber < 2) // pageNumber is 0 for scroll score.
            {
                string openSans = "\"Open Sans\"";
                string page1TitleHeight = M.FloatToShortString(pageFormat.Page1TitleHeight);
                StringBuilder mainTitleType = TextStyle("." + CSSObjectClass.mainTitle.ToString(), openSans, page1TitleHeight, "middle");
                fontStyles.Append(mainTitleType);

                string page1AuthorHeight = M.FloatToShortString(pageFormat.Page1AuthorHeight);
                StringBuilder authorType = TextStyle("." + CSSObjectClass.author.ToString(), openSans, page1AuthorHeight, "end");
                fontStyles.Append(authorType);
            } // end if(pageNumber < 2)
            #endregion Open Sans (Titles)

            #region CLicht
            string musicFontHeight = M.FloatToShortString(pageFormat.MusicFontHeight);
            StringBuilder existingCLichtClasses = GetExistingClichtClasses(usedCSSObjectClasses, usedClefIDs);
            StringBuilder cLichtStyle = TextStyle(existingCLichtClasses.ToString(), "CLicht", "", "");
            fontStyles.Append(cLichtStyle);

            //".rest, .notehead, .accidental, .clef"
            StringBuilder standardSizeClasses = GetStandardSizeClasses(usedCSSObjectClasses, usedClefIDs);
            StringBuilder fontSizeStyle = TextStyle(standardSizeClasses.ToString(), "", musicFontHeight, "");
            fontStyles.Append(fontSizeStyle);

			StringBuilder colorStyles = GetColorStyles(usedCSSColorClasses);
			fontStyles.Append(colorStyles);

			if(usedCSSObjectClasses.Contains(CSSObjectClass.dynamic))
            {
                string dynamicFontHeight = M.FloatToShortString(pageFormat.DynamicFontHeight);
                fontSizeStyle = TextStyle("." + CSSObjectClass.dynamic.ToString(), "", dynamicFontHeight, "");
                fontStyles.Append(fontSizeStyle);
            }
            if(usedCSSObjectClasses.Contains(CSSObjectClass.inputDynamic))
            {
                string dynamicFontHeight = M.FloatToShortString(pageFormat.DynamicFontHeight * pageFormat.InputSizeFactor);
                fontSizeStyle = TextStyle("." + CSSObjectClass.inputDynamic.ToString(), "", dynamicFontHeight, "");
                fontStyles.Append(fontSizeStyle);
            }

            // .smallClef, .cautionaryNotehead, .cautionaryAccidental
            StringBuilder smallClasses = GetSmallClasses(usedCSSObjectClasses, usedClefIDs);
            if(smallClasses.Length > 0)
            {
                string smallMusicFontHeight = M.FloatToShortString(pageFormat.MusicFontHeight * pageFormat.SmallSizeFactor);
                fontSizeStyle = TextStyle(smallClasses.ToString(), "", smallMusicFontHeight, "");
                fontStyles.Append(fontSizeStyle);
            }

            // .inputClef, inputNotehead, .inputAccidental, .inputRest,
            StringBuilder inputClasses = GetInputClasses(usedCSSObjectClasses, usedClefIDs);
            if(inputClasses.Length > 0)
            {
                string inputMusicFontHeight = M.FloatToShortString(pageFormat.MusicFontHeight * pageFormat.InputSizeFactor);
                fontSizeStyle = TextStyle(inputClasses.ToString(), "", inputMusicFontHeight, "");
                fontStyles.Append(fontSizeStyle);
            }

            if(usedCSSObjectClasses.Contains(CSSObjectClass.inputSmallClef))
            {
                string inputSmallMusicFontHeight = M.FloatToShortString(pageFormat.MusicFontHeight * pageFormat.InputSizeFactor * _pageFormat.SmallSizeFactor);
                fontSizeStyle = TextStyle("." + CSSObjectClass.inputSmallClef.ToString(), "", inputSmallMusicFontHeight, "");
                fontStyles.Append(fontSizeStyle);
            }

            if(OctavedClefExists(usedClefIDs))
            {
                string clefOctaveNumberFontSize = M.FloatToShortString(pageFormat.ClefOctaveNumberHeight);
                fontSizeStyle = TextStyle("." + CSSObjectClass.clefOctaveNumber.ToString(), "", clefOctaveNumberFontSize, "");
                fontStyles.Append(fontSizeStyle);
            }
            if(OctavedSmallClefExists(usedClefIDs))
            {
                string smallClefOctaveNumberFontSize = M.FloatToShortString(pageFormat.ClefOctaveNumberHeight * pageFormat.SmallSizeFactor);
                fontSizeStyle = TextStyle("." + CSSObjectClass.smallClefOctaveNumber.ToString(), "", smallClefOctaveNumberFontSize, "");
                fontStyles.Append(fontSizeStyle);
            }
            if(OctavedInputClefExists(usedClefIDs))
            {
                string inputClefOctaveNumberFontSize = M.FloatToShortString(pageFormat.ClefOctaveNumberHeight * pageFormat.InputSizeFactor);
                fontSizeStyle = TextStyle("." + CSSObjectClass.inputClefOctaveNumber.ToString(), "", inputClefOctaveNumberFontSize, "");
                fontStyles.Append(fontSizeStyle);
            }
            if(OctavedInputSmallClefExists(usedClefIDs))
            {
                string inputSmallClefOctaveNumberFontSize = M.FloatToShortString(pageFormat.ClefOctaveNumberHeight * pageFormat.SmallSizeFactor * pageFormat.InputSizeFactor);
                fontSizeStyle = TextStyle("." + CSSObjectClass.inputSmallClefOctaveNumber.ToString(), "", inputSmallClefOctaveNumberFontSize, "");
                fontStyles.Append(fontSizeStyle);
            }
            #endregion CLicht

            #region Arial
            StringBuilder existingArialClasses = GetExistingArialClasses(usedCSSObjectClasses, usedClefIDs);
            StringBuilder arialStyle = TextStyle(existingArialClasses.ToString(), "Arial", "", "");
            fontStyles.Append(arialStyle);

            string timeStampHeight = M.FloatToShortString(pageFormat.TimeStampFontHeight);
            StringBuilder timeStampType = TextStyle("." + CSSObjectClass.timeStamp.ToString(), "", timeStampHeight, "");
            fontStyles.Append(timeStampType);

            if(usedCSSObjectClasses.Contains(CSSObjectClass.staffName))
            {
                string staffNameFontHeight = M.FloatToShortString(pageFormat.StaffNameFontHeight);
                StringBuilder staffNameHeight = TextStyle("." + CSSObjectClass.staffName.ToString(), "", staffNameFontHeight, "middle");
                fontStyles.Append(staffNameHeight);
            }
            if(usedCSSObjectClasses.Contains(CSSObjectClass.inputStaffName))
            {
                string staffNameFontHeight = M.FloatToShortString(pageFormat.StaffNameFontHeight * pageFormat.InputSizeFactor);
                StringBuilder staffNameHeight = TextStyle("." + CSSObjectClass.inputStaffName.ToString(), "", staffNameFontHeight, "middle");
                fontStyles.Append(staffNameHeight);
            }
            if(usedCSSObjectClasses.Contains(CSSObjectClass.lyric))
            {
                string lyricFontHeight = M.FloatToShortString(pageFormat.LyricFontHeight);
                StringBuilder lyricHeight = TextStyle("." + CSSObjectClass.lyric.ToString(), "", lyricFontHeight, "middle");
                fontStyles.Append(lyricHeight);
            }
            if(usedCSSObjectClasses.Contains(CSSObjectClass.inputLyric))
            {
                string lyricFontHeight = M.FloatToShortString(pageFormat.LyricFontHeight * pageFormat.InputSizeFactor);
                StringBuilder lyricHeight = TextStyle("." + CSSObjectClass.inputLyric.ToString(), "", lyricFontHeight, "middle");
                fontStyles.Append(lyricHeight);
            }
            if(usedCSSObjectClasses.Contains(CSSObjectClass.barNumber))
            {
                string barNumberNumberFontHeight = M.FloatToShortString(pageFormat.BarNumberNumberFontHeight);
                StringBuilder barNumberNumberHeight = TextStyle("." + CSSObjectClass.barNumberNumber.ToString(), "", barNumberNumberFontHeight, "middle");
                fontStyles.Append(barNumberNumberHeight);
            }

            if(ClefXExists(usedClefIDs))
            {
                string clefXFontHeight = M.FloatToShortString(pageFormat.ClefXFontHeight);
                StringBuilder clefXStyle = TextStyle("." + CSSObjectClass.clefX.ToString(), "", clefXFontHeight, "");
                fontStyles.Append(clefXStyle);
            }
            if(SmallClefXExists(usedClefIDs))
            {
                string smallClefXFontHeight = M.FloatToShortString(pageFormat.ClefXFontHeight * pageFormat.SmallSizeFactor);
                StringBuilder smallClefXStyle = TextStyle("." + CSSObjectClass.smallClefX.ToString(), "", smallClefXFontHeight, "");
                fontStyles.Append(smallClefXStyle);
            }
            if(InputClefXExists(usedClefIDs))
            {
                string inputClefXFontHeight = M.FloatToShortString(pageFormat.ClefXFontHeight * pageFormat.InputSizeFactor);
                StringBuilder inputClefXStyle = TextStyle("." + CSSObjectClass.inputClefX.ToString(), "", inputClefXFontHeight, "");
                fontStyles.Append(inputClefXStyle);
            }
            if(InputSmallClefXExists(usedClefIDs))
            {
                string smallClefXFontHeight = M.FloatToShortString(pageFormat.ClefXFontHeight * pageFormat.SmallSizeFactor * pageFormat.InputSizeFactor);
                StringBuilder inputSmallClefXStyle = TextStyle("." + CSSObjectClass.inputSmallClefX.ToString(), "", smallClefXFontHeight, "");
                fontStyles.Append(inputSmallClefXStyle);
            }
            #endregion Arial

            #region Open Sans Condensed (ornament)
            if(usedCSSObjectClasses.Contains(CSSObjectClass.ornament))
            {
                string openSansCondensed = "\"Open Sans Condensed\"";
                string ornamentFontHeight = M.FloatToShortString(pageFormat.OrnamentFontHeight);
                StringBuilder ornamentType = TextStyle("." + CSSObjectClass.ornament.ToString(), openSansCondensed, ornamentFontHeight, "middle");
                fontStyles.Append(ornamentType);
            }
            #endregion Open Sans Condensed (ornament)
            #endregion text

            return fontStyles;
        }

		private StringBuilder GetColorStyles(List<CSSColorClass> usedCSSColorClasses)
		{
			StringBuilder rval = new StringBuilder();
			rval.Append(FillDefinition(CSSColorClass.fffColor, usedCSSColorClasses));
			rval.Append(FillDefinition(CSSColorClass.ffColor, usedCSSColorClasses));
			rval.Append(FillDefinition(CSSColorClass.fColor, usedCSSColorClasses));
			rval.Append(FillDefinition(CSSColorClass.mfColor, usedCSSColorClasses));
			rval.Append(FillDefinition(CSSColorClass.mpColor, usedCSSColorClasses));
			rval.Append(FillDefinition(CSSColorClass.pColor, usedCSSColorClasses));
			rval.Append(FillDefinition(CSSColorClass.ppColor, usedCSSColorClasses));
			rval.Append(FillDefinition(CSSColorClass.pppColor, usedCSSColorClasses));
			rval.Append(FillDefinition(CSSColorClass.ppppColor, usedCSSColorClasses));

			return rval;
		}

		/// <summary>
		/// Returns an empty StringBuilder if the cssColorClass is not in the usedCSSClasses
		/// </summary>
		private StringBuilder FillDefinition(CSSColorClass cssColorClass, List<CSSColorClass> usedCSSClasses)
		{
			StringBuilder def = new StringBuilder();
			if(usedCSSClasses.Contains(cssColorClass))
			{
				def.Append("." + cssColorClass.ToString());
				def.Append(@"
			{");
				def.Append($@"
			    fill:{M.GetEnumDescription(cssColorClass)}");
				def.Append(@"
			}
			");
			}

			return def;
		}

		private StringBuilder GetExistingClichtClasses(List<CSSObjectClass> usedCSSClasses, List<ClefID> usedClefIDs)
        {
            //.rest, .inputRest, .notehead, .accidental,
            //.cautionaryNotehead, .cautionaryAccidental,
            //.inputNotehead, .inputAccidental,
            //.clef, .smallClef, .inputClef, .inputSmallClef,
            //.dynamic, .inputDynamic
            //.clefOctaveNumber, .smallClefOctaveNumber

            StringBuilder rval = new StringBuilder();
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.rest);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.inputRest);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.notehead);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.accidental);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.cautionaryNotehead);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.cautionaryAccidental);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.inputNotehead);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.inputAccidental);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.clef);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.smallClef);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.inputClef);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.inputSmallClef);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.dynamic);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.inputDynamic);
            if(OctavedClefExists(usedClefIDs))
            {
                ExtendRval(rval, "." + CSSObjectClass.clefOctaveNumber.ToString());
            }
            if(OctavedSmallClefExists(usedClefIDs))
            {
                ExtendRval(rval, "." + CSSObjectClass.smallClefOctaveNumber.ToString());
            }

            return rval;
        }
        
        private StringBuilder GetStandardSizeClasses(List<CSSObjectClass> usedCSSClasses, List<ClefID> usedClefIDs)
        {
            //".rest, .notehead, .accidental, .clef"

            StringBuilder rval = new StringBuilder();
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.rest);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.notehead);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.accidental);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.clef);
            return rval;
        }

        private StringBuilder GetSmallClasses(List<CSSObjectClass> usedCSSClasses, List<ClefID> usedClefIDs)
        {
            // .smallClef, .cautionaryNotehead, .cautionaryAccidental

            StringBuilder rval = new StringBuilder();
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.cautionaryNotehead);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.cautionaryAccidental);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.smallClef);

            return rval;
        }
        
        private StringBuilder GetInputClasses(List<CSSObjectClass> usedCSSClasses, List<ClefID> usedClefIDs)
        {
            // .inputClef, inputNotehead, .inputAccidental, .inputRest,

            StringBuilder rval = new StringBuilder();
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.inputClef);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.inputNotehead);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.inputAccidental);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.inputRest);

            return rval;
        }

        private StringBuilder GetExistingArialClasses(List<CSSObjectClass> usedCSSClasses, List<ClefID> usedClefIDs)
        {
            //.timeStamp,
            //.staffName, .inputStaffName,
            //.lyric, .inputLyric,
            //.barNumberNumber,
            //.clefX, .smallClefX, .inputClefX, .inputSmallClefX

            // timestamp is not recorded, but exists on every page
            StringBuilder rval = new StringBuilder("." + CSSObjectClass.timeStamp.ToString());

            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.staffName);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.inputStaffName);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.lyric);
            ExtendRvalWith(rval, usedCSSClasses, CSSObjectClass.inputLyric);
            if(usedCSSClasses.Contains(CSSObjectClass.barNumber))
            {
                ExtendRval(rval, "." + CSSObjectClass.barNumberNumber.ToString());
            }
            if(ClefXExists(usedClefIDs))
            {
                ExtendRval(rval, "." + CSSObjectClass.clefX.ToString());
            }
            if(SmallClefXExists(usedClefIDs))
            {
                ExtendRval(rval, "." + CSSObjectClass.smallClefX.ToString());
            }
            if(InputClefXExists(usedClefIDs))
            {
                ExtendRval(rval, "." + CSSObjectClass.inputClefX.ToString());
            }
            if(InputSmallClefXExists(usedClefIDs))
            {
                ExtendRval(rval, "." + CSSObjectClass.inputSmallClefX.ToString());
            }

            return rval;
        }

        private void ExtendRvalWith(StringBuilder rval, List<CSSObjectClass> usedCSSClasses, CSSObjectClass cssClass)
        {
            if(usedCSSClasses.Contains(cssClass))
            {
                ExtendRval(rval, "." + cssClass.ToString());
            }
        }
        private void ExtendRval(StringBuilder rval, string className)
        {
            if(rval.Length > 0)
            {
                rval.Append(", ");
            }
            rval.Append(className);
        }

        private StringBuilder TextStyle(string element, string fontFamily, string fontSize, string textAnchor)
        {
            StringBuilder local = new StringBuilder();
            if(!String.IsNullOrEmpty(fontFamily))
            {
                local.Append($@"font-family:{fontFamily}");
                if(!String.IsNullOrEmpty(fontSize) || !String.IsNullOrEmpty(textAnchor))
                {
                    local.Append($@";
                ");
                }
            }
            if(!String.IsNullOrEmpty(fontSize))
            {
                local.Append($@"font-size:{fontSize}px");
                if(!String.IsNullOrEmpty(textAnchor))
                {
                    local.Append($@";
                ");
                }
            }
            if(!String.IsNullOrEmpty(textAnchor))
            {
                local.Append($@"text-anchor:{textAnchor}");
            }

            StringBuilder rval = new StringBuilder(
            $@"{element}
            {{
                {local.ToString()}
            }}
            ");

            return rval;
        }

        private bool OctavedClefExists(List<ClefID> usedClefIDs)
        {
            bool rval = usedClefIDs.Contains(ClefID.trebleClef8)
            || usedClefIDs.Contains(ClefID.bassClef8)
            || usedClefIDs.Contains(ClefID.trebleClef2x8)
            || usedClefIDs.Contains(ClefID.bassClef2x8)
            || usedClefIDs.Contains(ClefID.trebleClef3x8)
            || usedClefIDs.Contains(ClefID.bassClef3x8);

            return rval;
        }
        private bool OctavedSmallClefExists(List<ClefID> usedClefIDs)
        {
            bool rval = usedClefIDs.Contains(ClefID.smallTrebleClef8)
            || usedClefIDs.Contains(ClefID.smallBassClef8)
            || usedClefIDs.Contains(ClefID.smallTrebleClef2x8)
            || usedClefIDs.Contains(ClefID.smallBassClef2x8)
            || usedClefIDs.Contains(ClefID.smallTrebleClef3x8)
            || usedClefIDs.Contains(ClefID.smallBassClef3x8);

            return rval;
        }
        private bool OctavedInputClefExists(List<ClefID> usedClefIDs)
        {
            bool rval = usedClefIDs.Contains(ClefID.inputTrebleClef8)
            || usedClefIDs.Contains(ClefID.inputBassClef8)
            || usedClefIDs.Contains(ClefID.inputTrebleClef2x8)
            || usedClefIDs.Contains(ClefID.inputBassClef2x8)
            || usedClefIDs.Contains(ClefID.inputTrebleClef3x8)
            || usedClefIDs.Contains(ClefID.inputBassClef3x8);

            return rval;
        }
        private bool OctavedInputSmallClefExists(List<ClefID> usedClefIDs)
        {
            bool rval = usedClefIDs.Contains(ClefID.inputSmallTrebleClef8)
            || usedClefIDs.Contains(ClefID.inputSmallBassClef8)
            || usedClefIDs.Contains(ClefID.inputSmallTrebleClef2x8)
            || usedClefIDs.Contains(ClefID.inputSmallBassClef2x8)
            || usedClefIDs.Contains(ClefID.inputSmallTrebleClef3x8)
            || usedClefIDs.Contains(ClefID.inputSmallBassClef3x8);

            return rval;
        }

        private bool ClefXExists(List<ClefID> usedClefIDs)
        {
            bool rval = usedClefIDs.Contains(ClefID.trebleClef2x8)
            || usedClefIDs.Contains(ClefID.bassClef2x8)
            || usedClefIDs.Contains(ClefID.trebleClef3x8)
            || usedClefIDs.Contains(ClefID.bassClef3x8);

            return rval;
        }
        private bool SmallClefXExists(List<ClefID> usedClefIDs)
        {
            bool rval = usedClefIDs.Contains(ClefID.smallTrebleClef2x8)
            || usedClefIDs.Contains(ClefID.smallBassClef2x8)
            || usedClefIDs.Contains(ClefID.smallTrebleClef3x8)
            || usedClefIDs.Contains(ClefID.smallBassClef3x8);

            return rval;
        }
        private bool InputClefXExists(List<ClefID> usedClefIDs)
        {
            bool rval = usedClefIDs.Contains(ClefID.inputTrebleClef2x8)
            || usedClefIDs.Contains(ClefID.inputBassClef2x8)
            || usedClefIDs.Contains(ClefID.inputTrebleClef3x8)
            || usedClefIDs.Contains(ClefID.inputBassClef3x8);

            return rval;
        }
        private bool InputSmallClefXExists(List<ClefID> usedClefIDs)
        {
            bool rval = usedClefIDs.Contains(ClefID.inputSmallTrebleClef2x8)
            || usedClefIDs.Contains(ClefID.inputSmallBassClef2x8)
            || usedClefIDs.Contains(ClefID.inputSmallTrebleClef3x8)
            || usedClefIDs.Contains(ClefID.inputSmallBassClef3x8);

            return rval;
        }
        #endregion font styles

        #region line styles
        private StringBuilder LineStyles(PageFormat pageFormat, List<CSSObjectClass> usedCSSClasses, int pageNumber, bool defineFlagStyle)
        {
            StringBuilder lineStyles = new StringBuilder();
            
            string strokeWidth = M.FloatToShortString(pageFormat.StafflineStemStrokeWidth);
            StringBuilder standardLineClasses = GetStandardLineClasses(usedCSSClasses, defineFlagStyle);
            //".staffline, .ledgerline, .stem, .beam, .flag
            lineStyles.Append($@"{standardLineClasses.ToString()}
            {{
                stroke:black;
                stroke-width:{strokeWidth}px;
                fill:black
            }}
            ");

            if(usedCSSClasses.Contains(CSSObjectClass.stem))
            {
                lineStyles.Append($@".stem
            {{
                stroke-linecap:round                
            }}
            ");
            }

            strokeWidth = M.FloatToShortString(pageFormat.BarlineStrokeWidth);
            lineStyles.Append($@".barline
            {{
                stroke:black;
                stroke-width:{strokeWidth}px
            }}
            ");

            strokeWidth = M.FloatToShortString(pageFormat.ThickBarlineStrokeWidth);
            lineStyles.Append($@".thickBarline
            {{
                stroke:black;
                stroke-width:{strokeWidth}px
            }}
            ");

            if(usedCSSClasses.Contains(CSSObjectClass.noteExtender))
            {
                strokeWidth = M.FloatToShortString(pageFormat.NoteheadExtenderStrokeWidth);
                lineStyles.Append($@".noteExtender
            {{
                stroke:black;
                stroke-width:{strokeWidth}px
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.barNumber))
            {
                strokeWidth = M.FloatToShortString(pageFormat.BarNumberFrameStrokeWidth);
                lineStyles.Append($@".barNumberFrame
            {{
                stroke:black;
                stroke-width:{strokeWidth}px;
                fill:none
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.cautionaryBracket))
            {
                strokeWidth = M.FloatToShortString(pageFormat.StafflineStemStrokeWidth);
                lineStyles.Append($@".cautionaryBracket
            {{
                stroke:black;
                stroke-width:{strokeWidth};
                fill:none                
            }}
            ");
            }

            if(pageNumber > 0) // pageNumber is 0 for scroll
            {
                strokeWidth = M.FloatToShortString(pageFormat.StafflineStemStrokeWidth);
                lineStyles.Append($@".frame
            {{
                stroke:black;
                stroke-width:{strokeWidth}px;
                fill:white                
            }}
            ");
            }

            strokeWidth = M.FloatToShortString(pageFormat.StafflineStemStrokeWidth);
            if(usedCSSClasses.Contains(CSSObjectClass.beamBlock))
            {
                lineStyles.Append($@".opaqueBeam
            {{
                stroke:white;
                stroke-width:{strokeWidth}px;
                fill:white;
                opacity:0.65                
            }}
            ");
            }

            return lineStyles;
        }
        private StringBuilder InputLineStyles(PageFormat pageFormat, List<CSSObjectClass> usedCSSClasses, bool defineInputFlagStyle)
        {
            float inputSizeFactor = pageFormat.InputSizeFactor;
            StringBuilder lineStyles = new StringBuilder();

            string standardInputStrokeWidth = M.FloatToShortString(pageFormat.StafflineStemStrokeWidth * inputSizeFactor);
            StringBuilder standardInputLineClasses = GetStandardInputLineClasses(usedCSSClasses, defineInputFlagStyle);
            //".inputStaffline, .inputLedgerline, .inputStem, .inputBeam, .inputFlag
            lineStyles.Append($@"{standardInputLineClasses.ToString()} 
            {{
                stroke:black;
                stroke-width:{standardInputStrokeWidth}px;
                fill:black
            }}
            ");

            if(usedCSSClasses.Contains(CSSObjectClass.inputStem))
            {
                lineStyles.Append($@".inputStem
            {{
                stroke-linecap:round
            }}
            ");
            }

            if(usedCSSClasses.Contains(CSSObjectClass.inputBeamBlock))
            {
                lineStyles.Append($@".inputOpaqueBeam
            {{
                stroke:white;
                stroke-width:{standardInputStrokeWidth}px;
                fill:white;
                opacity:0.65
            }}
            ");
            } // end of if(usedCSSClasses.Contains(CSSClass.inputBeamBlock))

            return lineStyles;
        }
        private StringBuilder GetStandardLineClasses(List<CSSObjectClass> usedCSSClasses, bool defineFlagStyle)
        {
            //.staffline, .ledgerline, .stem, .beam
            StringBuilder rval = new StringBuilder();
            if(usedCSSClasses.Contains(CSSObjectClass.staff))
            {
                ExtendRval(rval, ".staffline");
            }
            if(usedCSSClasses.Contains(CSSObjectClass.ledgerlines))
            {
                ExtendRval(rval, ".ledgerline");
            }
            if(usedCSSClasses.Contains(CSSObjectClass.stem))
            {
                ExtendRval(rval, ".stem");
            }
            if(usedCSSClasses.Contains(CSSObjectClass.beamBlock))
            {
                ExtendRval(rval, ".beam");
            }
            if(defineFlagStyle)
            {
                ExtendRval(rval, ".flag");
            }

            return rval;
        }
        private StringBuilder GetStandardInputLineClasses(List<CSSObjectClass> usedCSSClasses, bool defineInputFlagStyle)
        {
            //.inputStaffline, .inputLedgerline, .inputStem, .inputBeam
            StringBuilder rval = new StringBuilder();
            if(usedCSSClasses.Contains(CSSObjectClass.inputStaff))
            {
                ExtendRval(rval, ".inputStaffline");
            }
            if(usedCSSClasses.Contains(CSSObjectClass.inputLedgerlines))
            {
                ExtendRval(rval, ".inputLedgerline");
            }
            if(usedCSSClasses.Contains(CSSObjectClass.inputStem))
            {
                ExtendRval(rval, ".inputStem");
            }
            if(usedCSSClasses.Contains(CSSObjectClass.inputBeamBlock))
            {
                ExtendRval(rval, ".inputBeam");
            }
            if(defineInputFlagStyle)
            {
                ExtendRval(rval, ".inputFlag");
            }

            return rval;
        }

        #endregion line styles

        #endregion save multi-page score

        #region save single svg score
        public void SaveSingleSVGScore()
		{
			string pageFilename = Path.GetFileNameWithoutExtension(FilePath) + " (scroll).svg";
			string pagePath = Path.GetDirectoryName(FilePath) + @"\" + pageFilename;

			TextInfo infoTextInfo = GetInfoTextAtTopOfPage(0);

			SvgPage singlePage = new SvgPage(this, _pageFormat, 0, infoTextInfo, this.Systems, true);

			SaveSVGPage(pagePath, singlePage, this.Metadata);
		}


		#endregion save single svg score
		#region fields loaded from .capx files
		public Metadata Metadata = null;
        public List<SvgSystem> Systems = new List<SvgSystem>();
        #endregion
        #region moritz-specific private fields

        protected string _filename = "";
        public string Filename { get { return _filename; } }
        #endregion

        /// <summary>
        /// Adds the staff name to the first barline of each visible staff in the score.
        /// </summary>
        private void SetStaffNames()
        {
            foreach(SvgSystem system in Systems)
            {
                for(int staffIndex = 0; staffIndex < system.Staves.Count; staffIndex++)
                {
                    Staff staff = system.Staves[staffIndex];
                    if(!(staff is HiddenOutputStaff))
                    {
                        foreach(NoteObject noteObject in staff.Voices[0].NoteObjects)
                        {
                            if(noteObject is Barline firstBarline)
                            {
                                float fontHeight = _pageFormat.StaffNameFontHeight;

                                if(staff is InputStaff)
                                {
                                    fontHeight *= _pageFormat.InputSizeFactor;
                                }
								StaffNameText staffNameText = new StaffNameText(firstBarline, staff.Staffname, fontHeight);
                                firstBarline.DrawObjects.Add(staffNameText);
                                break;
                            }
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
                foreach(Staff staff in system.Staves)
                {
                    if(!(staff is HiddenOutputStaff))
                    {
                        foreach(Voice voice in staff.Voices)
                        {
                            Debug.Assert(voice.NoteObjects[voice.NoteObjects.Count - 1] is Barline);
                            // contains lists of consecutive rest indices
                            List<List<int>> restIndexLists = new List<List<int>>();
                            #region find the consecutive rests
                            List<int> consecutiveRestIndexList = new List<int>();
                            for(int i = 0; i < voice.NoteObjects.Count - 1; i++)
                            {
                                Debug.Assert(!(voice.NoteObjects[i] is Barline));

                                RestSymbol rest1 = voice.NoteObjects[i] as RestSymbol;
                                RestSymbol rest2 = voice.NoteObjects[i + 1] as RestSymbol;
                                if(rest1 != null && rest2 != null)
                                {
                                    if(!consecutiveRestIndexList.Contains(i))
                                    {
                                        consecutiveRestIndexList.Add(i);
                                    }
                                    consecutiveRestIndexList.Add(i + 1);
                                }
                                else
                                {
                                    if(consecutiveRestIndexList != null && consecutiveRestIndexList.Count > 0)
                                    {
                                        restIndexLists.Add(consecutiveRestIndexList);
                                        consecutiveRestIndexList = new List<int>();
                                    }
                                }
                            }
                            #endregion
                            #region replace the consecutive rests
                            if(restIndexLists.Count > 0)
                            {
                                for(int i = restIndexLists.Count - 1; i >= 0; i--)
                                {
                                    List<int> consecutiveRestIndices = restIndexLists[i];
                                    int msDuration = 0;
                                    RestSymbol rest = null;
                                    // remove all but the first rest
                                    for(int j = consecutiveRestIndices.Count - 1; j > 0; j--)
                                    {
                                        rest = voice.NoteObjects[consecutiveRestIndices[j]] as RestSymbol;
                                        Debug.Assert(rest != null);
                                        msDuration += rest.MsDuration;
                                        voice.NoteObjects.RemoveAt(consecutiveRestIndices[j]);
                                    }
                                    rest = voice.NoteObjects[consecutiveRestIndices[0]] as RestSymbol;
                                    msDuration += rest.MsDuration;
                                    rest.MsDuration = msDuration;
                                }
                            }
                            #endregion
                        }
                    }
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
                bool staffIsNotHidden = !(system1.Staves[staffIndex] is HiddenOutputStaff);
                string clefTypeAtEndOfStaff1 = null;
                if(staffIsNotHidden)
                {
                    // If a staff has two voices, both contain the same clefTypes (some clefs may be invisible).
                    clefTypeAtEndOfStaff1 = FindClefTypeAtEndOfStaff1(system1.Staves[staffIndex].Voices[0]);
                }

                for(int voiceIndex = 0; voiceIndex < system2.Staves[staffIndex].Voices.Count; voiceIndex++)
                {
                    Voice voice1 = system1.Staves[staffIndex].Voices[voiceIndex];
                    Voice voice2 = system2.Staves[staffIndex].Voices[voiceIndex];
                    if(staffIsNotHidden)
                    {
                        Clef voice2FirstClef = voice2.NoteObjects[0] as Clef;
                        Debug.Assert(voice2FirstClef != null && clefTypeAtEndOfStaff1 == voice2FirstClef.ClefType);
                        voice2.NoteObjects.Remove(voice2FirstClef);
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
            system2 = null;
        }

        private string FindClefTypeAtEndOfStaff1(Voice staff1voice0)
        {
            Clef mainStaff1Clef = staff1voice0.NoteObjects[0] as Clef;
            Debug.Assert(mainStaff1Clef != null);

            string clefTypeAtEndOfStaff1 = mainStaff1Clef.ClefType;
            foreach(NoteObject noteObject in staff1voice0.NoteObjects)
            {
                if(noteObject is SmallClef smallClef)
                {
                    clefTypeAtEndOfStaff1 = smallClef.ClefType;
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
                                voiceInPreviousSystem.NoteObjects.Add(new Barline(voiceInPreviousSystem));
                            }
                            voice.NoteObjects.Remove(barline);
                        }
                    }
                }
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
                    if(!(staff is HiddenOutputStaff))
                    {
                        foreach(Voice voice in staff.Voices)
                        {
                            Debug.Assert(voice.NoteObjects.Count > 0
                                && !(voice.NoteObjects[voice.NoteObjects.Count - 1] is Barline));

                            Barline barline;
                            if(systemIndex == Systems.Count - 1)
                                barline = new EndBarline(voice);
                            else
                                barline = new Barline(voice);

                            voice.NoteObjects.Add(barline);
                        }
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

            SetSystemAbsEndMsPositions();

            NormalizeSmallClefPositions();

            FinalizeAccidentals();
            AddBarlineAtStartOfEachSystem();

            AddBarNumbers();
            SetStaffNames();
        }

        private void SetSystemAbsEndMsPositions()
        {
            int totalMsDuration = 0;
            foreach(SvgSystem system in Systems)
            {
                List<NoteObject> noteObjects = system.Staves[0].Voices[0].NoteObjects;
                foreach(NoteObject noteObject in noteObjects)
                {
                    if(noteObject is DurationSymbol ds)
                    {
                        totalMsDuration += ds.MsDuration;
                    }
                }
            }
            int endMsPosition = totalMsDuration;
            for(int i = Systems.Count - 1; i >= 0; --i)
            {
                Systems[i].AbsEndMsPosition = endMsPosition;
                endMsPosition = Systems[i].AbsStartMsPosition;
            }
        }

        /// <summary>
        /// If SmallClefs are followed by a rest, they are moved in front of the following chord or the final barline.
        /// </summary>
        private void NormalizeSmallClefPositions()
        {
            foreach(SvgSystem system in Systems)
            {
                foreach(Staff staff in system.Staves)
                {
                    if(!(staff is HiddenOutputStaff))
                    {
                        foreach(Voice voice in staff.Voices)
                        {
                            MoveSmallClefsToNextChordOrFinalBarline(voice);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If a SmallClef is followed by a rest, it is moved in front of the following chord or the final barline.
        /// This function is called recursively, to move all the SmallClefs to their correct positions.
        /// </summary>
        private void MoveSmallClefsToNextChordOrFinalBarline(Voice voice)
        {
            bool chordFollows = false;
            SmallClef smallClef = null;
            int smallClefIndex = -1;
            Debug.Assert(voice.NoteObjects[voice.NoteObjects.Count - 1] is Barline);
            int indexOfFollowingChordOrFinalBarline = voice.NoteObjects.Count - 1; // final barline

            for(int i = voice.NoteObjects.Count - 1; i >= 0; --i)
            {
                if(voice.NoteObjects[i] is ChordSymbol)
                {
                    indexOfFollowingChordOrFinalBarline = i;
                    chordFollows = true;
                }
                if(voice.NoteObjects[i] is RestSymbol)
                {
                    chordFollows = false;
                }
                smallClef = voice.NoteObjects[i] as SmallClef;
                if(smallClef != null)
                {
                    smallClefIndex = i;
                    break;
                }
            }

            if(smallClef != null && chordFollows == false)
            {            
                Debug.Assert(smallClefIndex < indexOfFollowingChordOrFinalBarline);
                voice.NoteObjects.Insert(indexOfFollowingChordOrFinalBarline, smallClef);
                voice.NoteObjects.RemoveAt(smallClefIndex);
                MoveSmallClefsToNextChordOrFinalBarline(voice); // recursive function
            }
        }

		/// <summary>
		/// Puts up a Warning MessageBox, and returns false if systems cannot be fit
		/// vertically on the page. Otherwise true.
		/// </summary>
		protected bool CreatePages()
        {
            bool success = true;
            int pageNumber = 1;
            int systemIndex = 0;
            while(systemIndex < Systems.Count)
            {
                int oldSystemIndex = systemIndex;
                SvgPage newPage = NewSvgPage(pageNumber++, ref systemIndex);
                if(oldSystemIndex == systemIndex)
                {
                    MessageBox.Show("The systems are too high for the page height.\n\n" +
                        "Reduce the height of the systems, or increase the page height.",
                        "Height Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    success = false;
                    break;
                }
                _pages.Add(newPage);
            }
            return success;
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
                Debug.Assert(Systems[systemIndex].Metrics != null);
                Debug.Assert(Systems[systemIndex].Metrics.StafflinesTop == 0);

                systemHeight = Systems[systemIndex].Metrics.NotesBottom - Systems[systemIndex].Metrics.NotesTop;

                systemHeightsTotal += systemHeight;
                if(systemHeightsTotal > frameHeight)
                {
                    lastPage = false;
                    break;
                }

                systemHeightsTotal += _pageFormat.DefaultDistanceBetweenSystems;

                systemsOnPage.Add(Systems[systemIndex]);

                systemIndex++;
            }

            return new SvgPage(this, _pageFormat, pageNumber, infoTextInfo, systemsOnPage, lastPage);
        }

        private TextInfo GetInfoTextAtTopOfPage(int pageNumber)
        {
            StringBuilder infoAtTopOfPageSB = new StringBuilder();

            if(!String.IsNullOrEmpty(_filename))
                infoAtTopOfPageSB.Append(Path.GetFileNameWithoutExtension(_filename));

			if(pageNumber == 0)
				infoAtTopOfPageSB.Append(" (scroll)");
			else
				infoAtTopOfPageSB.Append(" page " + pageNumber.ToString());

            if(Metadata != null)
                infoAtTopOfPageSB.AppendFormat(", " + Metadata.Date);

            return new TextInfo(infoAtTopOfPageSB.ToString(), "Arial", _pageFormat.TimeStampFontHeight, TextHorizAlign.left);
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
                if(!(Systems[0].Staves[staffIndex] is HiddenOutputStaff))
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
        }

        /// <summary>
        /// Adds a bar number to the first Barline in the top visible voice of each system except the first.
        /// </summary>
        private void AddBarNumbers()
        {
            int barNumber = 1;
            foreach(SvgSystem system in Systems)
            {
                Voice barnumberVoice = system.VoiceForBarnumber();
                bool isFirstBarline = true;
                for(int i = 0; i < barnumberVoice.NoteObjects.Count - 1; i++)
                {
                    if(barnumberVoice.NoteObjects[i] is Barline barline)
                    {
                        if(isFirstBarline && system != Systems[0])
                        {
                            FramedBarNumberText framedBarNumber = new FramedBarNumberText(this, barNumber.ToString(), _pageFormat.Gap, _pageFormat.StafflineStemStrokeWidth);

                            barline.DrawObjects.Add(framedBarNumber);
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
                    if(!(staff is HiddenOutputStaff))
                    {
                        foreach(Voice voice in staff.Voices)
                        {
                            if(voice.NoteObjects[0] is Clef)
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
        }

		public int PageCount { get { return _pages.Count; }}
        protected List<SvgPage> _pages = new List<SvgPage>();

        public PageFormat PageFormat { get { return _pageFormat; } } 
        protected PageFormat _pageFormat = null;

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

