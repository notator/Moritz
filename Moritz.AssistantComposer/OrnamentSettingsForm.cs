using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.AssistantComposer
{
    public partial class OrnamentSettingsForm : Form
    {
        public OrnamentSettingsForm(PaletteForm paletteForm)
        {
            InitializeOrnamentSettingsForm(null, paletteForm);
        }

        public OrnamentSettingsForm(XmlReader r, PaletteForm paletteForm)
        {
            InitializeOrnamentSettingsForm(r, paletteForm);
        }

        private void InitializeOrnamentSettingsForm(XmlReader r, PaletteForm paletteForm)
        {
            InitializeComponent();
            _paletteForm = paletteForm;
            ConnectBasicChordControl();
            OrnamentKrystalNameLabel.Text = "";

            ReplaceLabels(_bcc.DurationsLabel.Location.X + _bcc.DurationsLabel.Size.Width);
 
            int numberOfChordValues = -1;
            if(r != null)
            {
                numberOfChordValues = ReadOrnamentSettingsForm(r);
                if(!String.IsNullOrEmpty(OrnamentKrystalNameLabel.Text))
                {
                    string ornamentsKrystalPath = M.Preferences.LocalKrystalsFolder + @"\" +
                        OrnamentKrystalNameLabel.Text;
                    _ornamentsKrystal = K.LoadKrystal(ornamentsKrystalPath);
                }
                _paletteForm.SetSettingsHaveBeenSaved();
                this.Text = _paletteForm.Text + ": ornaments";
            }
            else
            {
                numberOfChordValues = LoadNewOrnamentKrystal();
                _paletteForm.SetSettingsHaveBeenSaved(); // removes the '*'
                this.Text = _paletteForm.Text + ": ornaments";
                _paletteForm.SetSettingsNotSaved();
            }

            SetDialogForDomain(numberOfChordValues);

            _allTextBoxes = GetAllTextBoxes();
            TouchAllTextBoxes();         
       }
        private int LoadNewOrnamentKrystal()
        {
            GetOrnamentKrystalButton_Click(null, null);
            _paletteForm.SetOrnamentControls();
            return _bcc.NumberOfChordValues; 
        }

        private int ReadOrnamentSettingsForm(XmlReader r)
        {
            int numberOfChordValues = -1;

            #region default values
            OrnamentsLevelTextBox.Text = "";
            BankIndicesTextBox.Text = "";
            PatchIndicesTextBox.Text = "";
            #endregion
            Debug.Assert(r.Name == "ornamentSettings");
            M.ReadToXmlElementTag(r, "ornamentKrystalName", "ornamentsLevel", "basicChord", "bankIndices", "patchIndices");

            while(r.Name == "ornamentKrystalName" || r.Name == "ornamentsLevel" || r.Name == "basicChord"
                || r.Name == "bankIndices" || r.Name == "patchIndices")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "ornamentKrystalName":
                            OrnamentKrystalNameLabel.Text = r.ReadElementContentAsString();
                            string ornamentsKrystalPath = M.Preferences.LocalKrystalsFolder + @"\" + OrnamentKrystalNameLabel.Text;
                            _ornamentsKrystal = K.LoadKrystal(ornamentsKrystalPath);
                            numberOfChordValues = (int)_ornamentsKrystal.MaxValue;
                            break;
                        case "ornamentsLevel":
                            Debug.Assert(_ornamentsKrystal != null);
                            this.OrnamentsLevelTextBox.Text = r.ReadElementContentAsString();
                            break;
                        case "basicChord":
                            Debug.Assert(numberOfChordValues != -1);
                            _bcc.ReadBasicChordControl(r);
                            break;
                        case "bankIndices":
                            BankIndicesTextBox.Text = r.ReadElementContentAsString();
                            break;
                        case "patchIndices":
                            PatchIndicesTextBox.Text = r.ReadElementContentAsString();
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "ornamentSettings",
                    "ornamentKrystalName", "ornamentsLevel", "basicChord", "bankIndices", "patchIndices");
            }
            Debug.Assert(r.Name == "ornamentSettings");
            return numberOfChordValues;
        }

        private void ConnectBasicChordControl()
        {
            _bcc = new BasicChordControl(SetDialogState);
            _bcc.Location = new Point(20, 72);
            this.Controls.Add(_bcc);
        }

        private void ReplaceLabels(int rightMargin)
        {
            ReplaceLabel(_bcc.DurationsLabel, "relative durations", rightMargin);
            ReplaceLabel(_bcc.VelocitiesLabel, "velocity increments", rightMargin);
            ReplaceLabel(_bcc.MidiPitchesLabel, "transpositions", rightMargin);
            //ReplaceLabel(_bcc.ChordOffsLabel, "( chord offs )", rightMargin);
            ReplaceLabel(_bcc.ChordDensitiesLabel, "note density factors", rightMargin);
        }

        private void ReplaceLabel(Label label, string newText, int rightMargin)
        {
            label.Text = newText;
            label.Location = new Point(rightMargin - label.Size.Width, label.Location.Y);
        }

        private List<TextBox> GetAllTextBoxes()
        {
            List<TextBox> textBoxes = new List<TextBox>();

            textBoxes.Add(OrnamentsLevelTextBox);
            textBoxes.Add(_bcc.DurationsTextBox);
            textBoxes.Add(_bcc.VelocitiesTextBox);
            textBoxes.Add(_bcc.MidiPitchesTextBox);
            textBoxes.Add(_bcc.ChordOffsTextBox);
            textBoxes.Add(_bcc.ChordDensitiesTextBox);
            textBoxes.Add(_bcc.RootInversionTextBox);
            textBoxes.Add(_bcc.InversionIndicesTextBox);
            textBoxes.Add(_bcc.VerticalVelocityFactorsTextBox);
            textBoxes.Add(BankIndicesTextBox);
            textBoxes.Add(PatchIndicesTextBox);

            return textBoxes;
        }

        #region buttons
        private void ShowContainingPaletteButton_Click(object sender, EventArgs e)
        {
            _paletteForm.BringToFront();
        }
        private void ShowMainScoreFormButton_Click(object sender, EventArgs e)
        {
            _paletteForm.AssistantComposerMainForm.BringToFront();
        }
        
        private void GetOrnamentKrystalButton_Click(object sender, EventArgs e)
        {
            Moritz.Krystals.KrystalBrowser krystalBrowser =
                new Moritz.Krystals.KrystalBrowser(M.Preferences.LocalKrystalsFolder, SetOrnamentKrystal);
            krystalBrowser.ShowDialog();
            // the krystalBrowser calls SetOrnamentKrystal() as a delegate just before it closes.
        }
        /// <summary>
        /// Called as a delegate by a krystalBrowser just before it closes.
        /// The current krystal name in the browser is passed to this class.
        /// </summary>
        /// <param name="krystalname"></param>
        private void SetOrnamentKrystal(Krystal ornamentKrystal)
        {
            DialogResult okCancel = DialogResult.OK;
            if(!String.IsNullOrEmpty(OrnamentKrystalNameLabel.Text))
            {
                okCancel = MessageBox.Show("Replace the current krystal?", "Set Krystal", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            }
            if(okCancel == DialogResult.OK)
            {
                _ornamentsKrystal = ornamentKrystal;
                this.OrnamentKrystalNameLabel.Text = ornamentKrystal.Name;
                this.OrnamentsLevelTextBox.Text = ornamentKrystal.Level.ToString();
                _bcc.NumberOfChordValues = (int) ornamentKrystal.MaxValue;
    
                this._paletteForm.SetSaveButton(true);

                this.ShowOrnamentKrystalStrandsButton.Enabled = true;

                EnableMainParameters();
                TouchAllTextBoxes();
                SetSettingsNotSaved();
            }

            this.BringToFront();
        }

        private void ShowOrnamentKrystalStrandsButton_Click(object sender, EventArgs e)
        {
            _ornamentsKrystalStrandsBrowser =
                ShowStrandsBrowser(OrnamentsKrystal, _ornamentsKrystalStrandsBrowser, new Point(0, 0));
        }

        private Moritz.Krystals.StrandsBrowser ShowStrandsBrowser(Krystal krystal, Moritz.Krystals.StrandsBrowser strandsBrowser, Point topLeft)
        {
            if(strandsBrowser != null)
            {
                strandsBrowser.Close();
                strandsBrowser = null;
            }

            if(krystal != null)
            {
                strandsBrowser = new Moritz.Krystals.StrandsBrowser(krystal, topLeft);
                strandsBrowser.Show();
            }
            return strandsBrowser;
        }

        /// <summary>
        /// This function enables the parameter inputs and sets this dialog's help texts accordingly.
        /// The contents of the Parameter TextBoxes are not changed.
        /// </summary>
        protected void SetDialogForDomain(int domain)
        {
            _bcc.NumberOfChordValues = domain;
            string countString = domain.ToString() + " ";

            EnableMainParameters();
            //DisableChordParameters();

            #region HelpLabels
            _bcc.SetHelpLabels();
            BankIndicesHelpLabel.Text = countString + "integer values in range [ 0..127 ]";
            PatchIndicesHelpLabel.Text = countString + "integer values in range [ 0..127 ]";
            #endregion HelpLabels
        }

        //private void DisableChordParameters()
        //{
        //    _bcc.RootInversionLabel.Enabled = false;
        //    _bcc.RootInversionTextBox.Enabled = false;
        //    _bcc.RootInversionTextBox.Text = "";
        //    _bcc.InversionIndicesLabel.Enabled = false;
        //    _bcc.InversionIndicesTextBox.Enabled = false;
        //    _bcc.InversionIndicesTextBox.Text = "";
        //    _bcc.VerticalVelocityFactorsLabel.Enabled = false;
        //    _bcc.VerticalVelocityFactorsTextBox.Enabled = false;
        //    _bcc.VerticalVelocityFactorsTextBox.Text = "";
        //}

        public void TouchAllTextBoxes()
        {
            OrnamentsLevelTextBox_Leave(OrnamentsLevelTextBox, null);
            _bcc.TouchAllTextBoxes();
            BankIndicesTextBox_Leave(PatchIndicesTextBox, null);
            PatchIndicesTextBox_Leave(PatchIndicesTextBox, null);
            this.OrnamentKrystalLabel.Focus();
        }

        private void EnableMainParameters()
        {
            OrnamentsLevelLabel.Enabled = true;
            OrnamentsLevelTextBox.Enabled = true;
            OrnamentsLevelHelpLabel.Enabled = true;

            _bcc.DurationsLabel.Enabled = true;
            _bcc.DurationsTextBox.Enabled = true;
            _bcc.DurationsHelpLabel.Enabled = true;

            _bcc.VelocitiesLabel.Enabled = true;
            _bcc.VelocitiesTextBox.Enabled = true;
            _bcc.VelocitiesHelpLabel.Enabled = true;

            _bcc.MidiPitchesTextBox.Enabled = true;
            _bcc.MidiPitchesTextBox.Enabled = true;
            _bcc.MidiPitchesTextBox.Enabled = true;

            _bcc.ChordOffsTextBox.Enabled = true;
            _bcc.ChordOffsTextBox.Enabled = true;
            _bcc.ChordOffsTextBox.Enabled = true;

            _bcc.ChordDensitiesLabel.Enabled = true;
            _bcc.ChordDensitiesTextBox.Enabled = true;
            _bcc.ChordDensitiesHelpLabel.Enabled = true;

            _bcc.SetChordControls();

            BankIndicesTextBox.Enabled = true;
            BankIndicesTextBox.Enabled = true;
            BankIndicesTextBox.Enabled = true;
 
            PatchIndicesTextBox.Enabled = true;
            PatchIndicesTextBox.Enabled = true;
            PatchIndicesTextBox.Enabled = true;
        }

        private string DefaultText(int count, string valueString)
        {
            StringBuilder defaultTextSB = new StringBuilder();
            for(int i = 0; i < count; i++)
            {
                defaultTextSB.Append(",  ");
                defaultTextSB.Append(valueString);
            }
            defaultTextSB.Remove(0, 3);
            return defaultTextSB.ToString();
        }

        private string DefaultEnvelopesText(int count)
        {
            StringBuilder defaultTextSB = new StringBuilder();
            for(int i = 0; i < count; i++)
            {
                defaultTextSB.Append("  :  ");
                defaultTextSB.Append("1, 1");
            }
            defaultTextSB.Remove(0, 5);
            return defaultTextSB.ToString();
        }

        private string DefaultRootInversionString(int count)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < count; i++)
            {
                switch(i)
                {
                    case 0:
                        sb.Append("12,");
                        break;
                    case 1:
                        sb.Append("7,");
                        break;
                    case 2:
                        sb.Append("5,");
                        break;
                    case 3:
                        sb.Append("4,");
                        break;
                    case 4:
                        sb.Append("3,");
                        break;
                    case 5:
                        sb.Append("2,");
                        break;
                    default:
                        sb.Append("1,");
                        break;
                }
            }
            sb.Remove(sb.Length - 1, 1); // final comma
            return sb.ToString();
        }

        private List<int> DefaultRootInversionIntList(int count)
        {
            List<int> intList = new List<int>();
            for(int i = 0; i < count; i++)
            {
                switch(i)
                {
                    case 0:
                        intList.Add(12);
                        break;
                    case 1:
                        intList.Add(7);
                        break;
                    case 2:
                        intList.Add(5);
                        break;
                    case 3:
                        intList.Add(4);
                        break;
                    case 4:
                        intList.Add(3);
                        break;
                    case 5:
                        intList.Add(2);
                        break;
                    default:
                        intList.Add(1);
                        break;
                }
            }
            return intList;
        }

        #endregion buttons
        #region text box events
 
        private void SetSettingsNotSaved()
        {
            if(!this.Text.EndsWith("*"))
                this.Text = this.Text + "*";
            _paletteForm.SetSettingsNotSaved();
        }

        /// <summary>
        /// Called only by _paletteForm.SetSettingsHaveBeenSaved().
        /// </summary>
        public void SetSettingsHaveBeenSaved()
        {
            if(this.Text.EndsWith("*"))
                this.Text = this.Text.Remove(this.Text.Length - 1);
        }

        public bool HasError
        {
            get
            {
                bool anyTextBoxHasErrorColour = false;
                foreach(TextBox textBox in _allTextBoxes)
                {
                    if(textBox.Enabled && textBox.BackColor == M.TextBoxErrorColor)
                    {
                        anyTextBoxHasErrorColour = true;
                        break;
                    }
                }
                return anyTextBoxHasErrorColour;
            }
        }

        private void OrnamentsLevelTextBox_TextChanged(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
        }

        private void OrnamentsLevelTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, (uint)1, 1, (int)OrnamentsKrystal.Level, SetDialogState);
            _paletteForm.SetOrnamentControls();
        }

        private void BankIndicesTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, true, (uint)_bcc.NumberOfChordValues, 0, 127, SetDialogState);
        }

        private void PatchIndicesTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, true, (uint)_bcc.NumberOfChordValues, 0, 127, SetDialogState);
        }

        #region helper functions
 
        public void SetDialogState(TextBox textBox, bool okay)
        {
            SetSettingsNotSaved();

            if(okay)
            {
                textBox.BackColor = Color.White;
             }
            else
            {
                textBox.BackColor = M.TextBoxErrorColor;
            }
        }

        private bool AnySettingHasErrorColor
        {
            get
            {
                bool anySettingHasErrorColor = false;
                foreach(TextBox textBox in _allTextBoxes)
                {
                    if(textBox.Enabled && textBox.BackColor == M.TextBoxErrorColor)
                    {
                        anySettingHasErrorColor = true;
                        break;
                    }
                }

                return anySettingHasErrorColor;
            }
        }

        #endregion helper functions
        #endregion text box events

        #region other events
        private void OrnamentSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_ornamentsKrystalStrandsBrowser != null)
            {
                _ornamentsKrystalStrandsBrowser.Close();
                _ornamentsKrystalStrandsBrowser.Dispose();
                _ornamentsKrystalStrandsBrowser = null;
            }

            if(_envelopesKrystalStrandsBrowser != null)
            {
                _envelopesKrystalStrandsBrowser.Close();
                _envelopesKrystalStrandsBrowser.Dispose();
                _envelopesKrystalStrandsBrowser = null;
            }
            TouchAllTextBoxes();
        }

        #endregion other events

        public void WriteOrnamentSettingsForm(XmlWriter w)
        {
            w.WriteStartElement("ornamentSettings");

            w.WriteStartElement("ornamentKrystalName");
            w.WriteString(OrnamentsKrystal.Name);
            w.WriteEndElement();

            if(!string.IsNullOrEmpty(OrnamentsLevelTextBox.Text))
            {
                w.WriteStartElement("ornamentsLevel");
                w.WriteString(OrnamentsLevelTextBox.Text);
                w.WriteEndElement();
            }

            BasicChordControl.WriteBasicChordControl(w);

            if(!string.IsNullOrEmpty(this.BankIndicesTextBox.Text))
            {
                w.WriteStartElement("bankIndices");
                w.WriteString(BankIndicesTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }
            if(!string.IsNullOrEmpty(this.PatchIndicesTextBox.Text))
            {
                w.WriteStartElement("patchIndices");
                w.WriteString(PatchIndicesTextBox.Text.Replace(" ", ""));
                w.WriteEndElement();
            }

            w.WriteEndElement(); // end of ornament settings
        }

        #region public properties
        public Krystal OrnamentsKrystal { get { return _ornamentsKrystal; } }
        public BasicChordControl BasicChordControl { get { return _bcc; } }
        #endregion public properties

        #region private variables
        private BasicChordControl _bcc = null;
        private PaletteForm _paletteForm = null;
        private Krystal _ornamentsKrystal = null;

        private Moritz.Krystals.StrandsBrowser _ornamentsKrystalStrandsBrowser = null;
        private Moritz.Krystals.StrandsBrowser _envelopesKrystalStrandsBrowser = null;

        #endregion private variables

        #region private readonly values
        private List<TextBox> _allTextBoxes;
        #endregion private readonly values
     }
}