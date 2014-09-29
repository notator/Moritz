using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;

using Krystals4ObjectLibrary;
using Moritz.Globals;

namespace Moritz.Palettes
{
    public partial class PercussionOrnamentsForm : Form
    {
        public PercussionOrnamentsForm(PercussionPaletteForm percussionPaletteForm)
        {
            InitializeOrnamentSettingsForm(null, percussionPaletteForm);
        }

        public PercussionOrnamentsForm(XmlReader r, PercussionPaletteForm percussionPaletteForm)
        {
            InitializeOrnamentSettingsForm(r, percussionPaletteForm);
        }

        private void InitializeOrnamentSettingsForm(XmlReader r, PercussionPaletteForm percussionPaletteForm)
        {
            InitializeComponent();
            _percPaletteForm = percussionPaletteForm;
            ConnectBasicPercussionControl();
            OrnamentKrystalNameLabel.Text = "";

            ReplaceLabels(_bpc.DurationsLabel.Location.X + _bpc.DurationsLabel.Size.Width);
 
            int numberOfChordValues = -1;
            if(r != null)
            {
                numberOfChordValues = ReadOrnamentSettingsForm(r);
                if(!String.IsNullOrEmpty(OrnamentKrystalNameLabel.Text))
                {
                    string ornamentsKrystalPath = M.Preferences.LocalMoritzKrystalsFolder + @"\" +
                        OrnamentKrystalNameLabel.Text;
                    _ornamentsKrystal = K.LoadKrystal(ornamentsKrystalPath);
                }
                _percPaletteForm.SetSettingsHaveBeenSaved();
                this.Text = _percPaletteForm.Text + ": ornaments";
            }
            else
            {
                numberOfChordValues = LoadNewOrnamentKrystal();
                _percPaletteForm.SetSettingsHaveBeenSaved(); // removes the '*'
                this.Text = _percPaletteForm.Text + ": ornaments";
                _percPaletteForm.SetSettingsNotSaved();
            }

            _bpc.SetHelpLabels(numberOfChordValues);
            _bpc.MidiPitchesHelpLabel.Text = numberOfChordValues.ToString() + " integer values in range [ 35..81 ]";
            _allTextBoxes = GetAllTextBoxes();
            TouchAllTextBoxes();
            
       }
        private int LoadNewOrnamentKrystal()
        {
            GetOrnamentKrystalButton_Click(null, null);
            _percPaletteForm.SetOrnamentControls();
            return _bpc.NumberOfChordValues; 
        }

        private int ReadOrnamentSettingsForm(XmlReader r)
        {
            int numberOfChordValues = -1;

            Debug.Assert(r.Name == "ornamentSettings");
            M.ReadToXmlElementTag(r, "ornamentKrystalName", "ornamentsLevel", "basicChord");

            while(r.Name == "ornamentKrystalName" || r.Name == "ornamentsLevel" || r.Name == "basicChord")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "ornamentKrystalName":
                            OrnamentKrystalNameLabel.Text = r.ReadElementContentAsString();
                            string ornamentsKrystalPath = M.Preferences.LocalMoritzKrystalsFolder + @"\" + OrnamentKrystalNameLabel.Text;
                            _ornamentsKrystal = K.LoadKrystal(ornamentsKrystalPath);
                            numberOfChordValues = (int)_ornamentsKrystal.MaxValue;
                            break;
                        case "ornamentsLevel":
                            Debug.Assert(_ornamentsKrystal != null);
                            this.OrnamentsLevelTextBox.Text = r.ReadElementContentAsString();
                            break;
                        case "basicChord":
                            Debug.Assert(numberOfChordValues != -1);
                            _bpc.ReadBasicChordControl(r);
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "ornamentSettings",
                    "ornamentKrystalName", "ornamentsLevel", "basicChord");
            }
            Debug.Assert(r.Name == "ornamentSettings");
            return numberOfChordValues;
        }

        private void ConnectBasicPercussionControl()
        {
            _bpc = new BasicPercussionControl(SetDialogState);
            _bpc.Location = new Point(20, 72);
            this.Controls.Add(_bpc);
        }

        private void ReplaceLabels(int rightMargin)
        {
            ReplaceLabel(_bpc.DurationsLabel, "relative durations", rightMargin);
            ReplaceLabel(_bpc.VelocitiesLabel, "velocity increments", rightMargin);
            ReplaceLabel(_bpc.MidiPitchesLabel, "overriding instruments", rightMargin);
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
            textBoxes.Add(_bpc.DurationsTextBox);
            textBoxes.Add(_bpc.VelocitiesTextBox);
            textBoxes.Add(_bpc.MidiPitchesTextBox);

            return textBoxes;
        }

        #region buttons
        private void ShowContainingPaletteButton_Click(object sender, EventArgs e)
        {
            _percPaletteForm.BringToFront();
        }
        private void ShowMainScoreFormButton_Click(object sender, EventArgs e)
        {
            _percPaletteForm.Callbacks.MainFormBringToFront();
        }
        
        private void GetOrnamentKrystalButton_Click(object sender, EventArgs e)
        {
            Moritz.Krystals.KrystalBrowser krystalBrowser =
                new Moritz.Krystals.KrystalBrowser(M.Preferences.LocalMoritzKrystalsFolder, SetOrnamentKrystal);
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
                _bpc.NumberOfChordValues = (int) ornamentKrystal.MaxValue;
    
                this._percPaletteForm.SetSaveButton(true);

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

        public void TouchAllTextBoxes()
        {
            OrnamentsLevelTextBox_Leave(OrnamentsLevelTextBox, null);
            _bpc.TouchAllTextBoxes();
            this.OrnamentKrystalLabel.Focus();
        }

        private void EnableMainParameters()
        {
            OrnamentsLevelLabel.Enabled = true;
            OrnamentsLevelTextBox.Enabled = true;
            OrnamentsLevelHelpLabel.Enabled = true;

            _bpc.DurationsLabel.Enabled = true;
            _bpc.DurationsTextBox.Enabled = true;
            _bpc.DurationsHelpLabel.Enabled = true;

            _bpc.VelocitiesLabel.Enabled = true;
            _bpc.VelocitiesTextBox.Enabled = true;
            _bpc.VelocitiesHelpLabel.Enabled = true;

            _bpc.MidiPitchesTextBox.Enabled = true;
            _bpc.MidiPitchesTextBox.Enabled = true;
            _bpc.MidiPitchesTextBox.Enabled = true;
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
            _percPaletteForm.SetSettingsNotSaved();
        }

        /// <summary>
        /// Called only by _krystalScoreStaffForm.SetSettingsHaveBeenSaved().
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
            _bpc.SetToWhite(sender as TextBox);
        }

        private void OrnamentsLevelTextBox_Leave(object sender, EventArgs e)
        {
            M.LeaveIntRangeTextBox(sender as TextBox, false, (uint)1, 1, (int)OrnamentsKrystal.Level, SetDialogState);
            _percPaletteForm.SetOrnamentControls();
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

            BasicPercussionControl.WriteBasicPercussionControl(w);

            w.WriteEndElement(); // end of ornament settings
        }

        #region public properties
        public Krystal OrnamentsKrystal { get { return _ornamentsKrystal; } }
        public BasicPercussionControl BasicPercussionControl { get { return _bpc; } }
        #endregion public properties
 
        #region private variables
        private BasicPercussionControl _bpc = null;
        private PercussionPaletteForm _percPaletteForm = null;
        private Krystal _ornamentsKrystal = null;

        private Moritz.Krystals.StrandsBrowser _ornamentsKrystalStrandsBrowser = null;
        private Moritz.Krystals.StrandsBrowser _envelopesKrystalStrandsBrowser = null;

        #endregion private variables

        #region private readonly values
        private List<TextBox> _allTextBoxes;
        #endregion private readonly values
     }
}