using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

//using Multimedia;
using Multimedia.Midi;

using Krystals4ObjectLibrary;
using Moritz.Globals;
using Moritz.Symbols;
using Moritz.Midi;
using Moritz.Spec;
using Moritz.Palettes;
using Moritz.Algorithm;
using Moritz.Algorithm.PaletteDemo;
using Moritz.Algorithm.SongSix;
using Moritz.Algorithm.Study2c3_1;
using Moritz.Algorithm.Study3Sketch1;
using Moritz.Algorithm.Study3Sketch2;

namespace Moritz.Composer
{
    public partial class AssistantComposerMainForm : Form
    {
        public AssistantComposerMainForm(string settingsPath, IMoritzForm1 moritzForm1)
        {
            InitializeComponent();
            _moritzForm1 = moritzForm1;

            _dimensionsAndMetadataForm = new DimensionsAndMetadataForm(this);

            M.PopulateComboBox(ChordTypeComboBox, M.ChordTypes);

            this._allTextBoxes = GetAllTextBoxes();
            SetDefaultValues();
            DeselectAll();

            Debug.Assert(File.Exists(settingsPath));

            _settingsPath = settingsPath;
            _settingsFolderPath = Path.GetDirectoryName(settingsPath);

            _algorithmName = Path.GetFileNameWithoutExtension(settingsPath);
            _algorithm = Algorithm(_algorithmName);
            _outputChannels = GetOutputChannels(_algorithm.NumberOfOutputVoices);

            SetScoreComboBoxItems(_settingsFolderPath);
            string scoreName = Path.GetFileNameWithoutExtension(_settingsPath);

            ScoreComboBox.SelectedIndexChanged -= ScoreComboBox_SelectedIndexChanged;
            ScoreComboBox.SelectedIndex = ScoreComboBox.Items.IndexOf(scoreName);
            GetSelectedSettings(_settingsPath, _algorithm);
            ScoreComboBox.SelectedIndexChanged += ScoreComboBox_SelectedIndexChanged;
            _dimensionsAndMetadataForm.Text = scoreName + ": Page Dimensions and Metadata";

            this.QuitAlgorithmButton.Text = "Quit " + _algorithmName;

            if(MidiChannelsPerStaffTextBox.Text == "")
            {
                SetDefaultMidiChannelsPerStaff(_algorithm.NumberOfOutputVoices);
            }
        }

        private void SetDefaultMidiChannelsPerStaff(int nMidiChannels)
        {
            StringBuilder channelList = new StringBuilder();
            for(int i = 0; i < nMidiChannels; ++i)
            {
                channelList.Append(i.ToString());
                channelList.Append(", ");
            }
            channelList.Remove(channelList.Length - 2, 2);
            MidiChannelsPerStaffTextBox.Text = channelList.ToString();
            MidiChannelsPerStaffTextBox_Leave(null, null);
        }

        private void InitMidiChannelsPerStaffFields(int nMidiChannels)
        {
            if(nMidiChannels == 1)
            {
                MidiChannelsPerStaffHelpLabel.Text = "channel: 0";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < nMidiChannels; ++i)
                {
                    sb.Append(", ");
                    sb.Append(i.ToString());
                }
                sb.Remove(0, 2);
                sb.Insert(0, "channels: ");
                MidiChannelsPerStaffHelpLabel.Text = sb.ToString();
            }
        }
        private void InitInputVoiceIndicesPerStaffFields(int nInputVoices)
        {
            if(nInputVoices == 0)
            {
                InputVoiceIndicesPerStaffHelpLabel.Text = "This algorithm does not provide input voices.";
                InputVoiceIndicesPerStaffHelpLabel.Enabled = false;
                InputVoiceIndicesPerStaffLabel.Enabled = false;
                InputVoiceIndicesPerStaffTextBox.Enabled = false;
                InputVoiceIndicesPerStaffHelpLabel2.Enabled = false;
            }
            else
            {
                InputVoiceIndicesPerStaffHelpLabel.Enabled = true;
                InputVoiceIndicesPerStaffLabel.Enabled = true;
                InputVoiceIndicesPerStaffTextBox.Enabled = true;
                InputVoiceIndicesPerStaffHelpLabel2.Enabled = true;

                if(nInputVoices == 1)
                {
                    InputVoiceIndicesPerStaffHelpLabel.Text = "input voice: 0";
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    for(int i = 0; i < nInputVoices; ++i)
                    {
                        sb.Append(", ");
                        sb.Append(i);
                    }
                    sb.Remove(0, 2);
                    sb.Insert(0, "input voices: ");
                    InputVoiceIndicesPerStaffHelpLabel.Text = sb.ToString();
                }
            }
        }
        private void SetSystemStartBarsHelpLabel(int numberOfBars)
        {
            SystemStartBarsHelpLabel.Text = "(" + numberOfBars.ToString() + " bars. Default is 5 bars per system)";
        }

        private void LoadSettings(string settingsPathname)
        {
            try
            {
                ClearSettings();
                ReadFile(settingsPathname);
            }
            catch(Exception ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            EnableBasicControls();
            SetSettingsHaveBeenSaved();
        }

        private void ClearSettings()
        {
            KrystalsListBox.SuspendLayout();
            KrystalsListBox.Items.Clear();
            KrystalsListBox.ResumeLayout();

            PalettesListBox.SuspendLayout();
            PalettesListBox.Items.Clear();
            PalettesListBox.ResumeLayout();

            RemoveSelectedKrystalButton.Enabled = false;
            ShowSelectedKrystalButton.Enabled = false;
            DeleteSelectedPaletteButton.Enabled = false;
            ShowSelectedPaletteButton.Enabled = false;
        }

        private List<TextBox> GetAllTextBoxes()
        {
            List<TextBox> textBoxes = new List<TextBox>();

            textBoxes.Add(MinimumGapsBetweenStavesTextBox);
            textBoxes.Add(MinimumGapsBetweenSystemsTextBox);
            textBoxes.Add(MinimumCrotchetDurationTextBox);
 
            textBoxes.Add(this.MidiChannelsPerStaffTextBox);
            textBoxes.Add(this.InputVoiceIndicesPerStaffTextBox);  
            textBoxes.Add(this.ClefsPerStaffTextBox);
            textBoxes.Add(this.StafflinesPerStaffTextBox);
            textBoxes.Add(this.StaffGroupsTextBox);

       
            textBoxes.Add(this.LongStaffNamesTextBox);
            textBoxes.Add(this.ShortStaffNamesTextBox);

            textBoxes.Add(SystemStartBarsTextBox);
            return textBoxes;
        }
        private void SetDefaultValues()
        {
            this.StafflineStemStrokeWidthComboBox.SelectedIndex = 0;
            this.GapPixelsComboBox.SelectedIndex = 0;
            this.MinimumGapsBetweenStavesTextBox.Text = "8";
            this.MinimumGapsBetweenSystemsTextBox.Text = "11";
            this.MinimumCrotchetDurationTextBox.Text = "800";

            KrystalsListBox.Items.Clear();
            PalettesListBox.Items.Clear();
        }

        public void SetSettingsHaveNotBeenSaved()
        {
            if(SettingsHaveBeenSaved())
            {
                this.Text = _algorithmName + " algorithm*";
                SetSaveAndCreateButtons(true);
            }
        }
        public void SetSettingsHaveBeenSaved()
        {
            if(!SettingsHaveBeenSaved())
            {
                this.Text = _algorithmName + " algorithm";
                SetSaveAndCreateButtons(false);

                List<IPaletteForm> kpForms = AllIPalleteForms;
                foreach(IPaletteForm kpf in kpForms)
                    kpf.SetSettingsHaveBeenSaved();
            }
        }
        private bool SettingsHaveBeenSaved()
        {
            return SaveSettingsButton.Enabled == false;
        }

        private bool InputValueErrors()
        {
            bool error = false;
            foreach(TextBox textBox in this._allTextBoxes)
            {
                if(textBox.Visible && textBox.BackColor == M.TextBoxErrorColor)
                {
                    error = true;
                    break;
                }
            }

            if(!error)
                error = _dimensionsAndMetadataForm.HasError();

            if(!error)
            {
                foreach(IPaletteForm iPaletteForm in AllIPalleteForms)
                {
                    if(iPaletteForm.HasError())
                    {
                        error = true;
                        break;
                    }
                }
            }

            if(error)
                MessageBox.Show("Cannot create a score because there are illegal (pink) input values.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return error;
        }

        public void SetSaveAndCreateButtons(bool saveButtonEnabled)
        {
            this.SaveSettingsButton.Enabled = saveButtonEnabled;
            CreateScoreButton.Enabled = !saveButtonEnabled;
        }

        #region form events
        private void DeselectAll()
        {
            bool settingsHaveBeenSaved = Text[Text.Length - 1] != '*';

            DeselectAllKrystalListBoxItems();
            DeselectAllKrystalPalettesListBoxItems();
            this.Focus(); // deletes the dotted frame around the last selected item.

            if(settingsHaveBeenSaved)
                SetSettingsHaveBeenSaved();
            else
                SetSettingsHaveNotBeenSaved();
        }

        private void DeselectAllKrystalListBoxItems()
        {
            for(int i = 0; i < KrystalsListBox.Items.Count; i++)
            {
                KrystalsListBox.SetSelected(i, false);
            }
            RemoveSelectedKrystalButton.Enabled = false;
        }

        private void DeselectAllKrystalPalettesListBoxItems()
        {
            for(int i = 0; i < PalettesListBox.Items.Count; i++)
            {
                PalettesListBox.SetSelected(i, false);
            }
            DeleteSelectedPaletteButton.Enabled = false;
            ShowSelectedPaletteButton.Enabled = false;
        }


        #endregion form events
        #region buttons
        private void EnableBasicControls()
        {
            if(KrystalsListBox.Items.Count > 0)
            {
                KrystalsListBox.SetSelected(0, true);
            }
            if(PalettesListBox.Items.Count > 0)
            {
                PalettesListBox.SetSelected(0, true);
            }
        }

        private void ReadFile(string pathname)
        {
            try
            {
                using(XmlReader r = XmlReader.Create(pathname))
                {
                    M.ReadToXmlElementTag(r, "moritzKrystalScore"); // check that this is a moritz preferences file

                    _dimensionsAndMetadataForm.Read(r);

                    Debug.Assert(r.Name == "notation" || r.Name == "krystals" || r.Name == "palettes");

                    while(r.Name == "notation" || r.Name == "krystals" || r.Name == "palettes")
                    {
                        if(r.NodeType != XmlNodeType.EndElement)
                        {
                            switch(r.Name)
                            {
                                case "notation":
                                    GetNotation(r);
                                    break;
                                case "krystals":
                                    GetKrystals(r);
                                    break;
                                case "palettes":
                                    GetPalettes(r);
                                    break;
                            }
                        }
                        M.ReadToXmlElementTag(r, "notation", "krystals", "palettes", "moritzKrystalScore");
                    }
                    Debug.Assert(r.Name == "moritzKrystalScore"); // end of krystal score
                }
            }
            catch
            {
                string msg = "The krystal score settings file\n\n" + pathname + "\n\ncould not be found.";
                MessageBox.Show(msg, "Error reading krystal score settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetNotation(XmlReader r)
        {
            Debug.Assert(r.Name == "notation");
            int item = 0;
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "chordSymbolType":
                        this.ChordTypeComboBox.SelectedItem = r.Value;
                        StandardChordsOptionsPanel.Visible = false;
                        switch(r.Value)
                        {
                            case "standard":

                                StandardChordsOptionsPanel.Visible = true;
                                break;
                            case "study2b2":
                                break;
                        }
                        break;
                    case "minimumCrotchetDuration":
                        MinimumCrotchetDurationTextBox.Text = r.Value;
                        break;
                    case "beamsCrossBarlines":
                        if(r.Value == "true")
                            BeamsCrossBarlinesCheckBox.Checked = true;
                        else
                            BeamsCrossBarlinesCheckBox.Checked = false;
                        break;
                    case "stafflineStemStrokeWidth":
                        item = 0;
                        do
                        {
                            StafflineStemStrokeWidthComboBox.SelectedIndex = item++;
                        } while(r.Value != StafflineStemStrokeWidthComboBox.SelectedItem.ToString());
                        break;
                    case "gap":
                        item = 0;
                        do
                        {
                            GapPixelsComboBox.SelectedIndex = item++;
                        } while(r.Value != GapPixelsComboBox.SelectedItem.ToString());
                        break;
                    case "minGapsBetweenStaves":
                        MinimumGapsBetweenStavesTextBox.Text = r.Value;
                        break;
                    case "minGapsBetweenSystems":
                        MinimumGapsBetweenSystemsTextBox.Text = r.Value;
                        break;
                    case "midiChannelsPerVoicePerStaff":
                        MidiChannelsPerStaffTextBox.Text = r.Value;
                        break;
                    case "inputVoicesPerVoicePerStaff":
                        InputVoiceIndicesPerStaffTextBox.Text = r.Value;
                        break;
                    case "clefsPerStaff":
                        ClefsPerStaffTextBox.Text = r.Value;
                        break;
                    case "stafflinesPerStaff":
                        StafflinesPerStaffTextBox.Text = r.Value;
                        break;
                    case "staffGroups":
                        StaffGroupsTextBox.Text = r.Value;
                        break;
                    case "longStaffNames":
                        LongStaffNamesTextBox.Text = r.Value;
                        break;
                    case "shortStaffNames":
                        ShortStaffNamesTextBox.Text = r.Value;
                        break;
                    case "systemStartBars":
                        SystemStartBarsTextBox.Text = r.Value;
                        break;
                }
            }
        }

        /// <summary>
        /// Loads the combo box with the names of all the scores which use this algorithm.
        /// The names are the root names of all .mkss files in subdirectories of the
        /// algorithmFolderPath.
        /// </summary>
        /// <param name="algorithmFolderPath"></param>
        private void SetScoreComboBoxItems(string algorithmFolderPath)
        {
            List<string> scoreNames = M.ScoreNames(algorithmFolderPath);
            ScoreComboBox.SelectedIndexChanged -= ScoreComboBox_SelectedIndexChanged;
            ScoreComboBox.SuspendLayout();
            ScoreComboBox.Items.Clear();
            foreach(string scoreName in scoreNames)
            {
                ScoreComboBox.Items.Add(scoreName);
            }
            ScoreComboBox.ResumeLayout();
            ScoreComboBox.SelectedIndex = 0;
            ScoreComboBox.SelectedIndexChanged += ScoreComboBox_SelectedIndexChanged;
        }

        private void GetKrystals(XmlReader r)
        {
            Debug.Assert(r.Name == "krystals");

            M.ReadToXmlElementTag(r, "krystal");
            while(r.Name == "krystal")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    r.MoveToAttribute(0);
                    string krystalName = r.Value;
                    Krystal krystal = GetKrystal(krystalName);
                    this.KrystalsListBox.Items.Add(krystal);
                }
                M.ReadToXmlElementTag(r, "krystal", "krystals");
            }
        }
        private Krystal GetKrystal(string krystalFileName)
        {
            Krystal krystal = null;
            try
            {
                string krystalPath = M.Preferences.LocalMoritzKrystalsFolder + @"\" + krystalFileName;
                krystal = K.LoadKrystal(krystalPath);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error loading krystal.\n\n" + ex.Message);
                krystal = null;
            }
            return krystal;
        }

        private void GetPalettes(XmlReader r)
        {
            ComposerFormCallbacks callbacks = GetCallbacks();
            Debug.Assert(r.Name == "palettes");
            M.ReadToXmlElementTag(r, "palette");
            while(r.Name == "palette")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    string name = "";
                    int domain = 1;
                    bool isPercussionPalette = false;

                    int count = r.AttributeCount;
                    for(int i = 0; i < count; i++)
                    {
                        r.MoveToAttribute(i);
                        switch(r.Name)
                        {
                            case "name":
                                name = r.Value;
                                break;
                            case "domain":
                                domain = int.Parse(r.Value);
                                break;
                            case "percussion":
                                if(r.Value == "true")
                                    isPercussionPalette = true;
                                break;
                        }
                    }

                    IPaletteForm paletteForm = null;
                    if(isPercussionPalette)
                        paletteForm = new PercussionPaletteForm(r, name, domain, callbacks);
                    else
                        paletteForm = new PaletteForm(r, name, domain, callbacks);

                    PalettesListBox.Items.Add(paletteForm);

                    M.ReadToXmlElementTag(r, "palette", "palettes");
                }
            }
            Debug.Assert(r.Name == "palettes");
        }

        private ComposerFormCallbacks GetCallbacks()
        {
            ComposerFormCallbacks callbacks = new ComposerFormCallbacks();
            callbacks.MainFormBringToFront = this.BringToFront;
            callbacks.MainFormHasBeenSaved = ThisHasBeenSaved;
            callbacks.SaveSettings = SaveSettings;
            callbacks.SetSaveAndCreateButtons = SetSaveAndCreateButtons;
            callbacks.SetSettingsHaveBeenSaved = SetSettingsHaveBeenSaved;
            callbacks.SetSettingsHaveNotBeenSaved = SetSettingsHaveNotBeenSaved;
            callbacks.SettingsFolderPath = GetSettingsFolderPath;
            return callbacks;
        }
        private string GetSettingsFolderPath()
        {
            return SettingsFolderPath;
        }
        private void SaveSettings()
        {
            SaveSettingsButton_Click(null, null);
        }
        private bool ThisHasBeenSaved()
        {
            return (!(this.Text.EndsWith("*")));
        }

        public void SaveSettingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveSettings(0);
            }
            catch(Exception ex)
            {
                string msg = "Failed to save score.\r\n\r\n"
                    + "Exception message: " + ex.Message;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void CreateScoreButton_Click(object sender, EventArgs e)
        {
            if(!InputValueErrors())
            {
                CreateSVGScore();
            }
        }

        public void SaveSettings(int numberOfPages)
        {
            Debug.Assert(!string.IsNullOrEmpty(_settingsPath));

            M.CreateDirectoryIfItDoesNotExist(this._settingsFolderPath);

            #region do the save
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.NewLineOnAttributes = true;
            settings.CloseOutput = false;
            using(XmlWriter w = XmlWriter.Create(_settingsPath, settings))
            {
                w.WriteStartDocument();
                w.WriteComment("file created: " + M.NowString);

                w.WriteStartElement("moritzKrystalScore");
                w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, M.OnlineXMLSchemasFolder + "/moritzKrystalScore.xsd");

                _dimensionsAndMetadataForm.WriteMetadata(w);
                _dimensionsAndMetadataForm.WriteDimensions(w);
                WriteNotation(w);
                WriteKrystals(w);
                WritePalettes(w);
                w.WriteEndElement(); // closes the moritzKrystalScore element
                w.Close(); // close unnecessary because of the using statement?
            }
            #endregion do the save
            SetSettingsHaveBeenSaved();
        }

        private void WriteNotation(XmlWriter w)
        {
            w.WriteStartElement("notation");
            w.WriteAttributeString("chordSymbolType", ChordTypeComboBox.SelectedItem.ToString());
            if(StandardChordsOptionsPanel.Visible)
            {
                w.WriteAttributeString("minimumCrotchetDuration", MinimumCrotchetDurationTextBox.Text);
                if(BeamsCrossBarlinesCheckBox.Checked)
                    w.WriteAttributeString("beamsCrossBarlines", "true");
                else
                    w.WriteAttributeString("beamsCrossBarlines", "false");
            }
            w.WriteAttributeString("stafflineStemStrokeWidth", StafflineStemStrokeWidthComboBox.SelectedItem.ToString());
            w.WriteAttributeString("gap", GapPixelsComboBox.SelectedItem.ToString());
            w.WriteAttributeString("minGapsBetweenStaves", this.MinimumGapsBetweenStavesTextBox.Text);
            w.WriteAttributeString("minGapsBetweenSystems", MinimumGapsBetweenSystemsTextBox.Text);
            w.WriteAttributeString("midiChannelsPerVoicePerStaff", MidiChannelsPerStaffTextBox.Text);
            w.WriteAttributeString("inputVoicesPerVoicePerStaff", InputVoiceIndicesPerStaffTextBox.Text);
            w.WriteAttributeString("clefsPerStaff", ClefsPerStaffTextBox.Text);
            w.WriteAttributeString("stafflinesPerStaff", StafflinesPerStaffTextBox.Text);
            w.WriteAttributeString("staffGroups", StaffGroupsTextBox.Text);
            w.WriteAttributeString("longStaffNames", LongStaffNamesTextBox.Text);
            w.WriteAttributeString("shortStaffNames", ShortStaffNamesTextBox.Text);
            w.WriteAttributeString("systemStartBars", SystemStartBarsTextBox.Text);
            w.WriteEndElement(); // notation
        }

        private void WriteKrystals(XmlWriter w)
        {
            if(this.KrystalsListBox.Items.Count > 0)
            {
                w.WriteStartElement("krystals");

                foreach(object o in KrystalsListBox.Items)
                {
                    Krystal krystal = o as Krystal;
                    if(krystal != null)
                    {
                        w.WriteStartElement("krystal");
                        w.WriteAttributeString("name", krystal.Name);
                        w.WriteEndElement();// krystal
                    }
                }
                w.WriteEndElement(); // krystals
            }
        }
        private void WritePalettes(XmlWriter w)
        {
            if(this.PalettesListBox.Items.Count > 0)
            {
                w.WriteStartElement("palettes");

                foreach(object o in PalettesListBox.Items)
                {
                    IPaletteForm paletteForm = o as IPaletteForm;
                    if(paletteForm != null)
                    {
                        paletteForm.WritePalette(w);
                    }
                }
                w.WriteEndElement(); // palettes
            }
        }

        /// <summary>
        /// This function
        /// 1) creates the score, 
        /// 2) opens it in the program currently set in Windows for opening SVG files.
        /// </summary>
        private void CreateSVGScore()
        {
            try
            {
                List<Krystal> krystals = null;
                List<Palette> palettes = null;
                GetKrystalsAndPalettes(out krystals, out palettes);
                PageFormat pageFormat = GetPageFormat();
                ComposableSvgScore score =
                    new KrystalPaletteScore(this.ScoreComboBox.Text,
                                            _algorithmName,
                                            pageFormat,
                                            krystals, palettes,
                                            _settingsFolderPath,
                                            _dimensionsAndMetadataForm.Keywords,
                                            _dimensionsAndMetadataForm.Comment);
                if(score != null)
                {
                    int numberOfPages = score.SaveSVGScore();
                    score.OpenSVGScore();
                    // if SaveSettings is called before OpenSVGScore(), the score does not open.
                    // Maybe because threading is involved...
                    SaveSettings(numberOfPages); // saves numberOfPages in runtimeInfo:nPages
                }
            }
            catch(Exception ex)
            {
                string msg = "Failed to create score, or to open it in the browser.\r\n\r\n"
                    + "Exception message: " + ex.Message;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #region PageFormat
        private PageFormat GetPageFormat()
        {
            PageFormat pageFormat = new PageFormat();
            SetPaperSize(pageFormat);

            SetTitleSizeAndPosition(pageFormat);

            SetFrame(pageFormat);

            SetWebsiteLink(pageFormat);

            SetNotation(pageFormat);

            return pageFormat;
        }
        private void SetPaperSize(PageFormat pageFormat)
        {
            pageFormat.PaperSize = _dimensionsAndMetadataForm.PaperSize;
            pageFormat.IsLandscape = _dimensionsAndMetadataForm.Landscape;
            float bottomPX;
            float rightPX;
            Debug.Assert(Regex.Matches(pageFormat.PaperSize, @"^(A4|B4|A5|B5|A3|Letter|Legal|Tabloid)$") != null);
            if(pageFormat.IsLandscape == true)
            {
                bottomPX = (int)(pageFormat.VerticalPixelsPerMillimeter * M.PaperSizes[pageFormat.PaperSize].ShortDimension_MM);
                rightPX = (int)(pageFormat.HorizontalPixelsPerMillimeter * M.PaperSizes[pageFormat.PaperSize].LongDimension_MM);
            }
            else
            {
                rightPX = (int)(pageFormat.HorizontalPixelsPerMillimeter * M.PaperSizes[pageFormat.PaperSize].ShortDimension_MM);
                bottomPX = (int)(pageFormat.VerticalPixelsPerMillimeter * M.PaperSizes[pageFormat.PaperSize].LongDimension_MM);
            }
            pageFormat.RightVBPX = rightPX * pageFormat.ViewBoxMagnification;
            pageFormat.BottomVBPX = bottomPX * pageFormat.ViewBoxMagnification;
        }
        private void SetTitleSizeAndPosition(PageFormat pageFormat)
        {
            DimensionsAndMetadataForm damf = _dimensionsAndMetadataForm;
            pageFormat.Page1TitleHeight = damf.TitleHeight * pageFormat.ViewBoxMagnification;
            pageFormat.Page1AuthorHeight = damf.AuthorHeight * pageFormat.ViewBoxMagnification;
            pageFormat.Page1TitleY = damf.TitleY * pageFormat.ViewBoxMagnification;
        }
        private void SetFrame(PageFormat pageFormat)
        {
            DimensionsAndMetadataForm damf = _dimensionsAndMetadataForm;
            pageFormat.TopMarginPage1 = damf.TopMarginWidthPage1 * pageFormat.ViewBoxMagnification;
            pageFormat.TopMarginOtherPages = damf.TopMarginWidthOtherPages * pageFormat.ViewBoxMagnification;
            pageFormat.RightMarginPos = pageFormat.RightVBPX - (damf.RightMarginWidth * pageFormat.ViewBoxMagnification);
            pageFormat.LeftMarginPos = damf.LeftMarginWidth * pageFormat.ViewBoxMagnification;
            pageFormat.BottomMarginPos = pageFormat.BottomVBPX - (damf.BottomMarginWidth * pageFormat.ViewBoxMagnification);
        }
        private void SetWebsiteLink(PageFormat pageFormat)
        {
            DimensionsAndMetadataForm damf = _dimensionsAndMetadataForm;
            pageFormat.AboutLinkText = damf.AboutLinkText;
            pageFormat.AboutLinkURL = damf.AboutLinkURL;
        }

        private void SetNotation(PageFormat pageFormat)
        {
            pageFormat.ChordSymbolType = this.ChordTypeComboBox.SelectedItem.ToString();
            pageFormat.MinimumCrotchetDuration = int.Parse(this.MinimumCrotchetDurationTextBox.Text);
            pageFormat.BeamsCrossBarlines = this.BeamsCrossBarlinesCheckBox.Checked;

            float strokeWidth = float.Parse(StafflineStemStrokeWidthComboBox.Text, M.En_USNumberFormat) * pageFormat.ViewBoxMagnification;
            pageFormat.StafflineStemStrokeWidth = strokeWidth;
            pageFormat.Gap = int.Parse(GapPixelsComboBox.Text) * pageFormat.ViewBoxMagnification;
            pageFormat.DefaultDistanceBetweenStaves = int.Parse(MinimumGapsBetweenStavesTextBox.Text) * pageFormat.Gap;
            pageFormat.DefaultDistanceBetweenSystems = int.Parse(MinimumGapsBetweenSystemsTextBox.Text) * pageFormat.Gap;

            pageFormat.MidiChannelsPerStaff = _midiChannelsPerStaff; // one value per output staff
            pageFormat.InputVoiceIndicesPerStaff = _inputVoiceIndicesPerStaff; // one value per input staff
            pageFormat.ClefsList = M.StringToStringList(this.ClefsPerStaffTextBox.Text, ',');
            pageFormat.StafflinesPerStaff = M.StringToIntList(this.StafflinesPerStaffTextBox.Text, ',');
            pageFormat.StaffGroups = M.StringToIntList(this.StaffGroupsTextBox.Text, ',');

            pageFormat.LongStaffNames = M.StringToStringList(this.LongStaffNamesTextBox.Text, ',');
            pageFormat.ShortStaffNames = M.StringToStringList(this.ShortStaffNamesTextBox.Text, ',');
            pageFormat.SystemStartBars = M.StringToIntList(SystemStartBarsTextBox.Text, ',');
        }
        #endregion


        private void GetKrystalsAndPalettes(out List<Krystal> krystals, out List<Palette> palettes)
        {
            krystals = new List<Krystal>();
            palettes = new List<Palette>();
            foreach(object o in KrystalsListBox.Items)
            {
                Krystal krystal = o as Krystal;
                if(krystal != null)
                {
                    krystals.Add(krystal);
                }
            }
            foreach(object o in PalettesListBox.Items)
            {
                PaletteForm paletteForm = o as PaletteForm;
                if(paletteForm != null)
                {
                    palettes.Add(new Palette(paletteForm));
                }
                PercussionPaletteForm percussionPaletteForm = o as PercussionPaletteForm;
                if(percussionPaletteForm != null)
                {
                    palettes.Add(new Palette(percussionPaletteForm));
                }
            }
        }

        private void ShowMoritzButton_Click(object sender, EventArgs e)
        {
            _moritzForm1.Show();

        }
        private void DimensionsAndMetadataButton_Click(object sender, EventArgs e)
        {
            _dimensionsAndMetadataForm.Show();
            _dimensionsAndMetadataForm.BringToFront();
        }

        private void QuitMoritzButton_Click(object sender, EventArgs e)
        {
            CheckSaved();
            _moritzForm1.Close();
        }
        private void QuitAssistantComposerButton_Click(object sender, EventArgs e)
        {
            CheckSaved();
            _moritzForm1.CloseAssistantComposer();
        }

        private void CheckSaved()
        {
            if(Text[Text.Length - 1] == '*')
            {
                DialogResult result = MessageBox.Show("Save settings?", "Save?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if(result == DialogResult.Yes)
                {
                    SaveSettings(0);
                }
            }
        }

        private void AssistantComposerMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<IPaletteForm> ipfs = AllIPalleteForms;
            foreach(IPaletteForm ipf in ipfs)
                ipf.Close();

            if(_dimensionsAndMetadataForm != null)
            {
                _dimensionsAndMetadataForm.Close();
            }
        }

        #endregion buttons
        #region text box events
        private void SystemStartBarsTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
            // Passing uint.MaxValue means that the list can have any number of values (including none).
            M.LeaveIntRangeTextBox(SystemStartBarsTextBox, true, uint.MaxValue, 1, int.MaxValue, SetTextBoxState);
            if((SystemStartBarsTextBox.BackColor != M.TextBoxErrorColor)
            && (SystemStartBarsTextBox.Text.Length > 0))
            {
                SystemStartBarsTextBox.Text = NormalizedSystemStartBars();
            }
        }
        /// <summary>
        /// This function sorts the start bars into ascending order, and silently removes duplicates.
        /// </summary>
        /// <returns></returns>
        private string NormalizedSystemStartBars()
        {
            Debug.Assert(SystemStartBarsTextBox.Text.Length > 0);
            List<int> startBars = M.StringToIntList(SystemStartBarsTextBox.Text, ',');
            StringBuilder sb = new StringBuilder();
            startBars.Sort();

            if(startBars.Count > 1)
            {   // remove duplicates
                int currentStartBar = startBars[startBars.Count - 1];
                for(int i = startBars.Count - 2; i >= 0; --i)
                {
                    if(startBars[i] == currentStartBar)
                        startBars.RemoveAt(i);
                    else
                        currentStartBar = startBars[i];
                }
                if(startBars[0] != 1 || startBars[startBars.Count - 1] > _algorithm.NumberOfBars)
                    SetTextBoxState(SystemStartBarsTextBox, false);
                else
                    SetTextBoxState(SystemStartBarsTextBox, true);
            }

            foreach(int val in startBars)
            {
                sb.Append(", ");
                sb.Append(val.ToString());
            }
            sb.Remove(0, 2);
            return sb.ToString();
        }

        private void SetTextBoxState(TextBox textBox, bool okay)
        {
            if(okay)
                textBox.BackColor = Color.White;
            else
                textBox.BackColor = M.TextBoxErrorColor;
        }
        #endregion text box events
        private void BeamsCrossBarlinesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
        }
        #region list box events and functions
        private void KrystalsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(KrystalsListBox.SelectedIndex >= 0)
            {
                ShowSelectedKrystalButton.Enabled = true;
                RemoveSelectedKrystalButton.Enabled = true;
            }
            else
            {
                ShowSelectedKrystalButton.Enabled = false;
                RemoveSelectedKrystalButton.Enabled = false;
            }

            ListBox listBox = sender as ListBox;
            listBox.Refresh();
        }
        private void KrystalsListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            DoSelectionColor(sender, e);
        }
        private void DoSelectionColor(object sender, DrawItemEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            Graphics g = e.Graphics;

            if(listBox != null && listBox.Items.Count > e.Index)
            {
                string text = listBox.Items[e.Index].ToString();
                Point textOrigin = new Point(e.Bounds.Left, e.Bounds.Top);

                e.DrawBackground();

                if(listBox.SelectedIndex == e.Index)
                {
                    g.FillRectangle(_systemHighlightBrush, e.Bounds);
                    g.DrawString(text, e.Font, _whiteBrush, textOrigin);
                }
                else
                {
                    g.FillRectangle(_whiteBrush, e.Bounds);
                    g.DrawString(text, e.Font, _blackBrush, textOrigin);
                }
                e.DrawFocusRectangle();
            }
        }

        private void AddKrystalButton_Click(object sender, EventArgs e)
        {
            Krystal selectedKrystal = null;
            if(KrystalsListBox.SelectedIndex >= 0)
                selectedKrystal = KrystalsListBox.SelectedItem as Krystal;
            Moritz.Krystals.KrystalBrowser krystalBrowser = new Moritz.Krystals.KrystalBrowser(selectedKrystal, M.Preferences.LocalMoritzKrystalsFolder, SetKrystal);
            krystalBrowser.Show();
            // the krystalBrowser calls SetKrystal() as a delegate just before it closes.
        }
        /// <summary>
        /// Called as a delegate by a krystalBrowser just before it closes.
        /// The current krystal name in the browser is passed to this function.
        /// </summary>
        /// <param name="krystalname"></param>
        private void SetKrystal(Krystal newKrystal)
        {
            List<Krystal> allKrystals = AllKrystals;

            bool krystalAlreadyPresent = false;
            foreach(Krystal krystal in allKrystals)
            {
                if(krystal.Name == newKrystal.Name)
                {
                    krystalAlreadyPresent = true;
                    break;
                }
            }
            if(krystalAlreadyPresent)
            {
                MessageBox.Show(newKrystal.Name + "is already in the score.", "krystal already present", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                string staffKrystalPath = M.Preferences.LocalMoritzKrystalsFolder + @"\" + newKrystal.Name;
                Krystal krystal = K.LoadKrystal(staffKrystalPath);
                this.KrystalsListBox.Items.Add(krystal);

                SetSettingsHaveNotBeenSaved();

                KrystalsListBox.SetSelected(KrystalsListBox.Items.Count - 1, true); // triggers KrystalsListBox_SelectedIndexChanged()
            }

            this.BringToFront();
        }

        #region krystal buttons
        private void ShowSelectedKrystalButton_Click(object sender, EventArgs e)
        {
            Krystal krystal = this.KrystalsListBox.SelectedItem as Krystal;
            if(krystal != null)
            {
                _krystalBrowser =
                    new Moritz.Krystals.KrystalBrowser(krystal, M.Preferences.LocalMoritzKrystalsFolder, null);
                _krystalBrowser.Show();
            }
        }

        private void RemoveSelectedKrystalButton_Click(object sender, EventArgs e)
        {
            if(KrystalsListBox.SelectedIndex >= 0)
            {
                Krystal selectedKrystal = KrystalsListBox.SelectedItem as Krystal;
                string msg = "The krystal\n\n     " + selectedKrystal.Name +
                    "\n\nwill be removed from this score.\n" +
                    "It will not be deleted from the main krystals folder.\n\n" +
                    "Proceed?\n\n";
                DialogResult proceed = MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if(proceed == DialogResult.Yes)
                {
                    KrystalsListBox.Items.RemoveAt(KrystalsListBox.SelectedIndex);

                    if(AllKrystals.Count > 0)
                    {
                        SetSettingsHaveNotBeenSaved();
                    }
                    else
                    {
                        SetSettingsHaveBeenSaved();
                    }
                }
            }
        }

        private List<Krystal> AllKrystals
        {
            get
            {
                List<Krystal> allKrystals = new List<Krystal>();
                foreach(object o in KrystalsListBox.Items)
                {
                    Krystal krystal = o as Krystal;
                    if(krystal != null)
                    {
                        allKrystals.Add(krystal);
                    }
                }
                return allKrystals;
            }
        }
        private List<IPaletteForm> AllIPalleteForms
        {
            get
            {
                List<IPaletteForm> allIPalletForms = new List<IPaletteForm>();
                foreach(object o in PalettesListBox.Items)
                {
                    IPaletteForm ipf = o as IPaletteForm;
                    if(ipf != null)
                    {
                        allIPalletForms.Add(ipf);
                    }
                }
                return allIPalletForms;
            }
        }

        private List<IPaletteForm> CurrentIPaletteForms
        {
            get
            {
                List<IPaletteForm> currentPalletForms = new List<IPaletteForm>();

                foreach(object o in PalettesListBox.Items)
                {
                    IPaletteForm ipf = o as IPaletteForm;
                    currentPalletForms.Add(ipf);
                }
                return currentPalletForms;
            }
            set
            {
                PalettesListBox.SuspendLayout();
                PalettesListBox.Items.Clear();
                foreach(IPaletteForm ipf in value)
                {
                    PalettesListBox.Items.Add(ipf);
                }
                PalettesListBox.ResumeLayout();
                UpdateForChangedPaletteList();
            }
        }
        #endregion krystalButtons

        #region palette buttons
        private void PalettesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(PalettesListBox.SelectedIndex >= 0)
            {
                DeleteSelectedPaletteButton.Enabled = true;
                ShowSelectedPaletteButton.Enabled = true;
            }
            else
            {
                DeleteSelectedPaletteButton.Enabled = false;
                ShowSelectedPaletteButton.Enabled = false;
            }

            ListBox listBox = sender as ListBox;
            listBox.Refresh();
        }
        private void ShowSelectedPaletteButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = -1;
            if(PalettesListBox.SelectedIndices.Count > 0)
                selectedIndex = PalettesListBox.SelectedIndices[0];
            if(selectedIndex >= 0)
            {
                List<IPaletteForm> currentForms = CurrentIPaletteForms;
                currentForms[selectedIndex].Show();
                currentForms[selectedIndex].BringToFront();
            }
        }
        private void AddPaletteButton_Click(object sender, EventArgs e)
        {
            GetNewPalette(false);
        }
        private void AddPercussionPaletteButton_Click(object sender, EventArgs e)
        {
            GetNewPalette(true);
        }
        private void GetNewPalette(bool isPercussionPalette)
        {
            string message = null;
            if(isPercussionPalette)
                message = "Domain of the new percussion palette [1..20]:";
            else
                message = "Domain of the new palette [1..20]:";

            // Palette domains are limited by the width of the palette forms.
            // There has to be enough space for the demo buttons.
            ComposerFormCallbacks callbacks = GetCallbacks();
            GetStringDialog getStringDialog = new GetStringDialog("Get Domain", message);
            if(getStringDialog.ShowDialog() == DialogResult.OK)
            {
                int domain;
                if(int.TryParse(getStringDialog.String, out domain) && domain > 0 && domain < 21)
                {
                    IPaletteForm iPaletteForm = null;
                    if(isPercussionPalette)
                    {
                        string name = PercussionPaletteForm.NewPaletteName(PalettesListBox.Items.Count + 2, domain);
                        iPaletteForm = new PercussionPaletteForm(name, domain, callbacks);
                    }
                    else
                    {
                        string newname = PaletteForm.NewPaletteName(PalettesListBox.Items.Count + 2, domain);
                        iPaletteForm = new PaletteForm(newname, domain, callbacks);
                    }
                    List<IPaletteForm> currentPaletteForms = CurrentIPaletteForms;
                    currentPaletteForms.Add(iPaletteForm);
                    CurrentIPaletteForms = currentPaletteForms;
                    PalettesListBox.SelectedIndex = PalettesListBox.Items.Count - 1;
                    this.SetSettingsHaveNotBeenSaved();
                }
                else
                {
                    MessageBox.Show("Illegal domain.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

        }
        private void DeletePaletteButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = -1;
            if(PalettesListBox.SelectedIndices.Count > 0)
                selectedIndex = PalettesListBox.SelectedIndices[0];
            if(selectedIndex >= 0)
            {
                string toDelete = PalettesListBox.Items[selectedIndex].ToString();
                string msg = "The " + toDelete + " will be deleted completely.\n\n" +
                    "Other palettes will be renumbered accordingly.\n\n" +
                    "Proceed?\n\n";
                DialogResult proceed = MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if(proceed == DialogResult.Yes)
                {
                    List<IPaletteForm> currentPaletteForms = CurrentIPaletteForms;
                    currentPaletteForms[selectedIndex].Close();
                    currentPaletteForms.RemoveAt(selectedIndex);
                    CurrentIPaletteForms = currentPaletteForms;
                    this.SetSettingsHaveNotBeenSaved();
                    UpdateForChangedPaletteList();
                }
            }
        }
        /// <summary>
        /// This function should be called after adding or removing a PaletteForm from the PaletteFormsList.
        /// It changes the Titles of all the PaletteForms, and updates the items in the PalettesListBox.
        /// Palettes should always be consecutively numbered, starting at 1.
        /// </summary>
        private void UpdateForChangedPaletteList()
        {
            PalettesListBox.SuspendLayout();
            int paletteNumber = 1;
            foreach(IPaletteForm ipf in CurrentIPaletteForms)
            {
                string name = null;
                PercussionPaletteForm ppf = ipf as PercussionPaletteForm;
                if(ppf != null)
                    name = PercussionPaletteForm.NewPaletteName(paletteNumber, ppf.Domain);
                PaletteForm pf = ipf as PaletteForm;
                if(pf != null)
                    name = PaletteForm.NewPaletteName(paletteNumber, pf.Domain);
                ipf.SetName(name);
                paletteNumber++;
            }
            PalettesListBox.ResumeLayout();
        }

        private void PalettesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            DoSelectionColor(sender, e);
        }
        #endregion palette buttons


        #endregion list box events

        #region private variables
        SolidBrush _systemHighlightBrush = new SolidBrush(SystemColors.Highlight);
        SolidBrush _whiteBrush = new SolidBrush(Color.White);
        SolidBrush _blackBrush = new SolidBrush(Color.Black);

        /// <summary>
        /// _scoreBaseName is the name of the score without any folder or extension. For example "Study 2b2".
        /// </summary>
        string _algorithmName = null;
        CompositionAlgorithm _algorithm = null;

        /// <summary>
        /// The folder in which the score and all its associated files is saved
        /// </summary>
        string _settingsFolderPath = null;
        string _settingsPath = null;
        const int _minimumMsDuration = 5;
        #endregion private variables

        public string SettingsFolderPath { get { return _settingsFolderPath; } }

        public string ScoreFolder { get { return _settingsFolderPath; } }

        #region page format controls


        private void StafflineStemStrokeWidthComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
        }
        private void GapPixelsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
        }
        private void MinimumGapsBetweenStavesTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckTextBoxIsUInt(this.MinimumGapsBetweenStavesTextBox);
            SetSettingsHaveNotBeenSaved();
        }
        private void MinimumGapsBetweenSystemsTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckTextBoxIsUInt(this.MinimumGapsBetweenSystemsTextBox);
            SetSettingsHaveNotBeenSaved();
        }

        private void ScoreComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _dimensionsAndMetadataForm.Hide();
            foreach(IPaletteForm palette in CurrentIPaletteForms)
            {
                palette.Close();
            }
            if(_krystalBrowser != null)
            {
                _krystalBrowser.Close();
                _krystalBrowser = null;
            }

            string scoreName = ScoreComboBox.SelectedItem.ToString();
            _dimensionsAndMetadataForm.Text = scoreName + ": Page Dimensions and Metadata";

            GetSelectedSettings(_settingsPath, _algorithm);
        }

        private void GetSelectedSettings(string settingsPath, CompositionAlgorithm algorithm)
        {
            LoadSettings(settingsPath);

            InitMidiChannelsPerStaffFields(algorithm.NumberOfOutputVoices);
            InitInputVoiceIndicesPerStaffFields(algorithm.NumberOfInputVoices);

            SetSystemStartBarsHelpLabel(algorithm.NumberOfBars);
            MidiChannelsPerStaffTextBox_Leave(null, null); // sets _numberOfOutputStaves _numberOfStaves
            InputVoiceIndicesPerStaffTextBox_Leave(null, null); // sets _numberOfInputStaves, _numberOfStaves
            SetSettingsHaveBeenSaved();
        }

        private void ChordTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.StandardChordsOptionsPanel.Visible = false;
            if(((string)ChordTypeComboBox.SelectedItem) == "standard")
                this.StandardChordsOptionsPanel.Visible = true;

            this.MidiChannelsPerStaffTextBox_Leave(null, null);
            SetSettingsHaveNotBeenSaved();
        }

        private void MinimumCrotchetDurationTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckTextBoxIsUInt(this.MinimumCrotchetDurationTextBox);
            SetSettingsHaveNotBeenSaved();
        }
        private void CheckTextBoxIsUInt(TextBox textBox)
        {
            bool okay = true;
            textBox.Text.Trim();
            try
            {
                uint i = uint.Parse(textBox.Text);
            }
            catch
            {
                okay = false;
            }

            if(okay)
            {
                textBox.BackColor = Color.White;
            }
            else
            {
                textBox.BackColor = M.TextBoxErrorColor;
            }
        }

        private void MidiChannelsPerStaffHelp_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Right)
                ShowMidiChannelsPerStaffHelp();
        }

        private void ShowMidiChannelsPerStaffHelp()
        {
            StringBuilder midiChannelsSB = MidiChannelsSB();
            MessageBox.Show(midiChannelsSB.ToString(), "Help for 'midi channels per voice per staff'", MessageBoxButtons.OK);
        }

        private StringBuilder MidiChannelsSB()
        {
            string mainText = "Input fields 'midi channels per voice per staff' and 'input voices per voice per staff'.\n\n" +

                "All algorithms create voices in staves in systems. Output voices are always created, but input voices " +
                "are optional.\n\n" +

                "Each output voice contains information that is to be sent to a midi output device under control of the performer's input. " +
                "So each output voice is associated with a unique midi output channel.\n" +
                "Each input voice contains information describing how a performer's input affects the assistant performer's output. An input " +
                "voice can be indexed here (as if it had a midi channel), but the Assistant Performer actually ignores the channel on which midi " +
                "information arrives.\n\n" +

                "By convention, algorithms create both midi channel indices and input voice indices allocated in increasing " +
                "order beginning with 0. Neither midi channels nor input voice indices may occur twice in the same system. " +
                "Each algorithm declares which midi channels (and input voice indices, if any) it provides, and these appear in the help " +
                "text above these input fields. (Not all the midi channels or input voices need to be notated in a particular score. But " +
                "scores must contain at least one output voice.)\n\n" +

                "The number of staves and voices in the score is the sum of the number of staves and voices defined in these two input fields.\n\n" +

                "The two input fields work in the same way: The numbers must be selected from those given in the " +
                "help text above the field, and determine the top to bottom order in the score. Staves are separated by commas, " +
                "voices are separated by colons. Input staves (if they exist) are always notated below the output staves.\n\n" +

                "For example, if the algorithm uses midi channels 0 1 2 3 4 5, and these are to be notated using standard " +
                "chord symbols, they can be notated on 3 staves by entering '3:1, 0:2, 4:5' into the field. Midi channels 2 " +
                "and 3 can be notated on separate staves, omitting the other channels, by entering '2, 3'.";

            StringBuilder midiChannelsSB = new StringBuilder();
            midiChannelsSB.Append(mainText);
            midiChannelsSB.Append("\n\nThe current algorithm ");
            if(_outputChannels.Count == 1)
                midiChannelsSB.Append("only uses\noutput channel index 0");
            else
                midiChannelsSB.Append("uses\noutput channel indices ");

            for(int i = 0; i < _outputChannels.Count; ++i)
            {
                if(i > 0)
                {
                    if(i == (_outputChannels.Count - 1))
                        midiChannelsSB.Append(" and ");
                    else
                        midiChannelsSB.Append(", ");
                }
                midiChannelsSB.Append(_outputChannels[i].ToString());
            }
            int nInputVoices = _algorithm.NumberOfInputVoices;
            if(nInputVoices == 0)
                midiChannelsSB.Append(",\nand has no input voices.\n\n");
            else if(nInputVoices == 1)
            {
                midiChannelsSB.Append(",\nand input voice index 0\n\n");
            }
            else
            {
                midiChannelsSB.Append(",\nand input voice indices " );
                for(int i = 0; i < nInputVoices; ++i)
                {
                    midiChannelsSB.Append(i.ToString());
                    midiChannelsSB.Append(", ");
                }
                midiChannelsSB.Replace(", ", ".", midiChannelsSB.Length - 2, 2);
            }

            return midiChannelsSB;
        }

        private List<byte> GetOutputChannels(int nOutputChannels)
        {
            List<byte> rval = new List<byte>();
            for(byte i = 0; i < nOutputChannels; ++i)
            {
                rval.Add(i);
            }
            return rval;
        }

        #region helper functions
        private string NormalizedByteListsString(List<List<byte>> byteLists)
        {
            StringBuilder sb = new StringBuilder();
            if(byteLists.Count > 0)
            {
                foreach(List<byte> bytes in byteLists)
                {
                    sb.Append(", ");
                    StringBuilder bytesListSB = new StringBuilder();
                    foreach(byte b in bytes)
                    {
                        bytesListSB.Append(":");
                        bytesListSB.Append(b.ToString());
                    }
                    bytesListSB.Remove(0, 1);
                    sb.Append(bytesListSB.ToString());
                }
                sb.Remove(0, 2);
            }
            return sb.ToString();
        }
        private void EnableStaffDependentControls(bool enabled)
        {
            if(enabled)
            {
                SetHelpTexts();
                ClefsPerStaffLabel.Enabled = true;
                StafflinesPerStaffLabel.Enabled = true;
                StaffGroupsLabel.Enabled = true;
                LongStaffNamesLabel.Enabled = true;
                ShortStaffNamesLabel.Enabled = true;

                ClefsPerStaffTextBox.Enabled = true;
                StafflinesPerStaffTextBox.Enabled = true;
                StaffGroupsTextBox.Enabled = true;
                LongStaffNamesTextBox.Enabled = true;
                ShortStaffNamesTextBox.Enabled = true;

                ClefsPerStaffTextBox_Leave(null, null);
                StafflinesPerStaffTextBox_Leave(null, null);

                StaffGroupsTextBox_Leave(null, null);
                LongStaffNamesTextBox_Leave(null, null);
                ShortStaffNamesTextBox_Leave(null, null);
            }
            else
            {
                ClearHelpTexts();
                ClefsPerStaffLabel.Enabled = false;
                StafflinesPerStaffLabel.Enabled = false;
                StaffGroupsLabel.Enabled = false;
                LongStaffNamesLabel.Enabled = false;
                ShortStaffNamesLabel.Enabled = false;

                ClefsPerStaffTextBox.Enabled = false;
                StafflinesPerStaffTextBox.Enabled = false;
                StaffGroupsTextBox.Enabled = false;
                LongStaffNamesTextBox.Enabled = false;
                ShortStaffNamesTextBox.Enabled = false;
            }
        }
        private void SetHelpTexts()
        {
            StringBuilder clefsSB = new StringBuilder();
            foreach(string clef in M.Clefs)
            {
                clefsSB.Append(", ");
                clefsSB.Append(clef);
            }
            clefsSB.Remove(0, 2);

            if(_numberOfStaves == 1)
            {
                this.ClefsPerStaffHelpLabel.Text = "1 clef\n" +
                                                   "available clefs: " + clefsSB.ToString();
                this.StafflinesPerStaffHelpLabel.Text = "1 integer\n" +
                                                    "standard clefs must have 5 lines";
                this.StaffGroupsHelpLabel.Text = "must be 1";
                this.LongStaffNamesHelpLabel.Text = "1 name (for first system)";
                this.ShortStaffNamesHelpLabel.Text = "1 name (for other systems)";
            }
            else
            {
                this.ClefsPerStaffHelpLabel.Text = _numberOfStaves.ToString() + " clefs separated by commas.\n" +
                                                    "available clefs: " + clefsSB.ToString();
                this.StafflinesPerStaffHelpLabel.Text = _numberOfStaves.ToString() + " integers separated by commas.\n" +
                                                    "standard clefs must have 5 lines";
                this.StaffGroupsHelpLabel.Text = "integers (whose total is " + _numberOfStaves.ToString() + ")";
                this.LongStaffNamesHelpLabel.Text = _numberOfStaves.ToString() + " names (for first system)";
                this.ShortStaffNamesHelpLabel.Text = _numberOfStaves.ToString() + " names (for other systems)";
            }
        }
        private void ClearHelpTexts()
        {
            this.ClefsPerStaffHelpLabel.Text = "";
            this.StafflinesPerStaffHelpLabel.Text = "";
            this.StaffGroupsHelpLabel.Text = "";
            this.LongStaffNamesHelpLabel.Text = "";
            this.ShortStaffNamesHelpLabel.Text = "";
        }
        #endregion

        /// <summary>
        /// This function sets _numberOfOutputStaves (and consequetially _numberOfStaves).
        /// It also sets _staffIsInput.
        /// It also sets the help texts on the dialog.
        /// </summary>
        private void MidiChannelsPerStaffTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
            EnableStaffDependentControls(false);

            bool error = false;
            List<List<byte>> byteLists = new List<List<byte>>();

            try
            {
                byteLists = M.StringToByteLists(MidiChannelsPerStaffTextBox.Text);
            }
            catch
            {
                error = true;
            }

            if(byteLists.Count == 0)
                    error = true;

            if(!error)
            {
                foreach(List<byte> channels in byteLists)
                {
                    // channels are output staff channels here.
                    if(channels.Count == 0 || channels.Count > 2
                    || (channels.Count > 1 && ((string)ChordTypeComboBox.SelectedItem) != "standard"))
                    {
                        error = true;
                        break;
                    }
                    foreach(byte channel in channels)
                    {
                        if(!_outputChannels.Contains(channel))
                        {
                            error = true;
                            break;
                        }
                    }
                    if(error)
                        break;
                }
            }

            if(error)
            {
                SetTextBoxState(MidiChannelsPerStaffTextBox, false);
            }
            else
            {
                _midiChannelsPerStaff = byteLists;
                EnableStaffDependentControls(true);
                MidiChannelsPerStaffTextBox.Text = NormalizedByteListsString(byteLists);
                SetTextBoxState(MidiChannelsPerStaffTextBox, true);
            }
        }

        /// <summary>
        /// This function sets _numberOfinputStaves (and consequetially _numberOfStaves).
        /// It also sets the help texts on the dialog.
        /// </summary>
        private void InputVoiceIndicesPerStaffTextBox_Leave(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
            EnableStaffDependentControls(false);

            bool error = false;
            List<List<byte>> byteLists = new List<List<byte>>();

            try
            {
                byteLists = M.StringToByteLists(InputVoiceIndicesPerStaffTextBox.Text);
            }
            catch
            {
                error = true;
            }
            
            // byteLists.Count==0 (number of staves == 0) is not an error for inputs
            if(byteLists.Count > 0 && !error)
            {
                foreach(List<byte> outputVoiceIndices in byteLists)
                {
                    if(outputVoiceIndices.Count == 0 || outputVoiceIndices.Count > 2)
                    {
                        error = true;
                        break;
                    }
                    foreach(byte channel in outputVoiceIndices)
                    {
                        if(channel < 0 || channel >= _algorithm.NumberOfInputVoices)
                        {
                            error = true;
                            break;
                        }
                    }
                    if(error)
                        break;
                }
            }

            if(error)
            {
                SetTextBoxState(InputVoiceIndicesPerStaffTextBox, false);
            }
            else
            {
                _inputVoiceIndicesPerStaff = byteLists;
                EnableStaffDependentControls(true);
                InputVoiceIndicesPerStaffTextBox.Text = NormalizedByteListsString(byteLists);
                SetTextBoxState(InputVoiceIndicesPerStaffTextBox, true);
            }
        }

        /// <summary>
        /// This function _uses_ _numberOfStaves.
        /// </summary>
        private void ClefsPerStaffTextBox_Leave(object sender, EventArgs e)
        {
            List<string> trimmedClefs = M.StringToStringList(ClefsPerStaffTextBox.Text, ',');

            bool okay = true;
            foreach(string clef in trimmedClefs)
            {
                if(!M.Clefs.Contains(clef))
                {
                    okay = false;
                    break;
                }
            }

            if(okay && trimmedClefs.Count == _numberOfStaves)
            {
                ClefsPerStaffTextBox.Text = NormalizedText(trimmedClefs);
                SetTextBoxState(ClefsPerStaffTextBox, true);
                CheckClefsAndStafflineNumbers();
            }
            else
                SetTextBoxState(ClefsPerStaffTextBox, false);
        }

        /// <summary>
        /// This function _uses_ _numberOfStaves.
        /// </summary>
        private void StafflinesPerStaffTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(StafflinesPerStaffTextBox, false, (uint)_numberOfStaves, 1, 127, SetTextBoxState);
            CheckClefsAndStafflineNumbers();
        }

        /// <summary>
        /// This function _uses_ _numberOfStaves.
        /// </summary>
        private void StaffGroupsTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(StaffGroupsTextBox, false, uint.MaxValue, 1, 127, SetTextBoxState);
            if(StaffGroupsTextBox.ForeColor != M.TextBoxErrorColor)
            {
                List<byte> bytes = M.StringToByteList(StaffGroupsTextBox.Text, ',');
                int sum = 0;
                foreach(byte b in bytes)
                    sum += (int)b;
                if(sum != _numberOfStaves)
                    SetTextBoxState(StaffGroupsTextBox, false);
            }
        }

        private void CheckClefsAndStafflineNumbers()
        {
            string[] clefs = ClefsPerStaffTextBox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] stafflinesArray = StafflinesPerStaffTextBox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if(clefs.Length == stafflinesArray.Length)
            {
                for(int i = 0; i < stafflinesArray.Length; ++i)
                {
                    if(stafflinesArray[i].Trim() != "5")
                    {
                        if(clefs[i].Trim() == "n")
                            SetTextBoxState(StafflinesPerStaffTextBox, true);
                        else
                            SetTextBoxState(StafflinesPerStaffTextBox, false);
                    }
                }
            }
            else
            {
                SetTextBoxState(StafflinesPerStaffTextBox, false);
            }
        }

        private string NormalizedText(List<string> texts)
        {
            StringBuilder sb = new StringBuilder();
            foreach(string text in texts)
            {
                sb.Append(", ");
                sb.Append(text);
            }
            sb.Remove(0, 2);
            return sb.ToString();
        }



        private void LongStaffNamesTextBox_Leave(object sender, EventArgs e)
        {
            CheckStaffNames(LongStaffNamesTextBox);
        }
        private void ShortStaffNamesTextBox_Leave(object sender, EventArgs e)
        {
            CheckStaffNames(ShortStaffNamesTextBox);
        }
        private void CheckStaffNames(TextBox textBox)
        {
            string[] names = textBox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> trimmedNames = new List<string>();
            foreach(string name in names)
            {
                trimmedNames.Add(name.Trim());
            }
            if(trimmedNames.Count == _numberOfStaves)
            {
                textBox.Text = NormalizedText(trimmedNames);
                SetTextBoxState(textBox, true);
            }
            else
            {
                SetTextBoxState(textBox, false);
            }
        }

        private CompositionAlgorithm Algorithm(string algorithmName)
        {
            CompositionAlgorithm algorithm = null;
            switch(algorithmName)
            {
                case "Study 2c3.1":
                    algorithm = new Study2c3_1Algorithm(null, null);
                    break;
                case "Song Six":
                    algorithm = new SongSixAlgorithm(null, null);
                    break;
                case "Study 3 sketch 1":
                    algorithm = new Study3Sketch1Algorithm(null, null);
                    break;
                case "Study 3 sketch 2":
                    algorithm = new Study3Sketch2Algorithm(null, null);
                    break;
                default:
                    throw new ApplicationException("unknown algorithm");
            }
            return algorithm;
        }

        #endregion page format controls

        #region set text boxes to white, and unsaved settings
        private void MidiChannelsPerStaffTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
            M.SetToWhite(MidiChannelsPerStaffTextBox);
        }

        private void InputVoiceIndicesPerStaffTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
            M.SetToWhite(InputVoiceIndicesPerStaffTextBox);
        }


        private void ClefsPerStaffTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
            M.SetToWhite(ClefsPerStaffTextBox);
        }

        private void StafflinesPerStaffTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
            M.SetToWhite(StafflinesPerStaffTextBox);
        }

        private void StaffGroupsTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
            M.SetToWhite(StaffGroupsTextBox);
        }

        private void LongStaffNamesTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
            M.SetToWhite(LongStaffNamesTextBox);
        }

        private void ShortStaffNamesTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
            M.SetToWhite(ShortStaffNamesTextBox);
        }

        private void SystemStartBarsTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSettingsHaveNotBeenSaved();
            M.SetToWhite(SystemStartBarsTextBox);
        }
        #endregion

        private List<TextBox> _allTextBoxes = null;
        private IMoritzForm1 _moritzForm1;
        private Moritz.Krystals.KrystalBrowser _krystalBrowser = null;
        private DimensionsAndMetadataForm _dimensionsAndMetadataForm;

        private List<byte> _outputChannels = null;
        
        private List<List<byte>> _midiChannelsPerStaff; // set in MidiChannelsPerStaffTextBox_Leave
        private List<List<byte>> _inputVoiceIndicesPerStaff; // set in InputVoiceIndicesPerStaffTextBox_Leave
        
        private int _numberOfStaves 
        { 
            get 
            {
                if(_midiChannelsPerStaff != null && _inputVoiceIndicesPerStaff != null)
                {
                    return _midiChannelsPerStaff.Count + _inputVoiceIndicesPerStaff.Count;
                }
                else
                {
                    return 0;
                }
            } 
        }

        public PageFormat PageFormat = null;
    }
}