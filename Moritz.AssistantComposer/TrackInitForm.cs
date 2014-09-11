using System;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;

namespace Moritz.AssistantComposer
{
    public partial class TrackInitForm : Form
    {
        /// <summary>
        /// Creates a new, empty TrackInitValuesForm for setting the following options.
        /// These are all defaults, that will be adjustable (as before) in the AP:
        /// </summary>
        public TrackInitForm(AssistantComposerMainForm assistantComposerMainForm, int nTracks)
        {
            InitializeComponent();
            this._nTracks = nTracks;

            int x = 118;
            int y = 35;
            int heightDiff = 24;
            AddIntListControls(x, y, nTracks, heightDiff);

            _assistantComposerMainForm = assistantComposerMainForm;

            SaveSettingsButton.Enabled = false;
        }

        private void AddIntListControls(int x, int y, int nTracks, int heightDiff)
        {
            int textBoxWidth = 30;

            _ilcVolume = new IntListControl(x, y, textBoxWidth, 0, 127, nTracks, ContainedControlHasChanged);
            y += heightDiff;
            _ilcPWDeviation = new IntListControl(x, y, textBoxWidth, 0, 127, nTracks, ContainedControlHasChanged);
            y += heightDiff;
            _ilcPitchWheel = new IntListControl(x, y, textBoxWidth, 0, 127, nTracks, ContainedControlHasChanged);
            y += heightDiff;
            _ilcPan = new IntListControl(x, y, textBoxWidth, 0, 127, nTracks, ContainedControlHasChanged);
            y += heightDiff;
            _ilcExpression = new IntListControl(x, y, textBoxWidth, 0, 127, nTracks, ContainedControlHasChanged);
            y += heightDiff;
            _ilcModulation = new IntListControl(x, y, textBoxWidth, 0, 127, nTracks, ContainedControlHasChanged);
            y += heightDiff;

            _intListControls.Add(_ilcVolume);
            _intListControls.Add(_ilcPWDeviation);
            _intListControls.Add(_ilcPitchWheel);
            _intListControls.Add(_ilcPan);
            _intListControls.Add(_ilcExpression);
            _intListControls.Add(_ilcModulation);

            this.SuspendLayout();
            this.Controls.Add(_ilcVolume);
            this.Controls.Add(_ilcPWDeviation);
            this.Controls.Add(_ilcPitchWheel);
            this.Controls.Add(_ilcPan);
            this.Controls.Add(_ilcExpression);
            this.Controls.Add(_ilcModulation);
            this.ResumeLayout();

            foreach(IntListControl ilc in _intListControls)
            {
                ilc.Visible = true;
                ilc.Show();
            }

            SetTrackLabelsVisibility(nTracks);
        }

        private void SetTrackLabelsVisibility(int nTracks)
        {
            List<Label> trackLabels = new List<Label>(){TrackLabel1,TrackLabel2,TrackLabel3,TrackLabel4,TrackLabel5,TrackLabel6,TrackLabel7,TrackLabel8,
                TrackLabel9,TrackLabel10,TrackLabel11,TrackLabel12,TrackLabel13,TrackLabel14,TrackLabel15,TrackLabel16};

            for(int i = 0; i < 16; ++i)
            {
                if(i >= nTracks)
                {
                    trackLabels[i].Visible = false;
                }
            }
        }

        #region main buttons
        private void ShowMainScoreFormButton_Click(object sender, EventArgs e)
        {
            this._assistantComposerMainForm.BringToFront();
        }

        private void SaveSettingsButton_Click(object sender, EventArgs e)
        {
            this._assistantComposerMainForm.SaveSettings(0);
            SetSettingsHaveBeenSaved();
        }
        #endregion

        #region public interface

        public void Read(XmlReader r)
        {
            Debug.Assert(r.Name == "trackInit");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "volume":
                        _ilcVolume.Set(r.Value);
                        _ilcVolume.SetColoredDefaultTexts(100);
                        break;
                    case "pwDeviation":
                        _ilcPWDeviation.Set(r.Value);
                        _ilcPWDeviation.SetColoredDefaultTexts(2);
                        break;
                    case "pitchWheel":
                        _ilcPitchWheel.Set(r.Value);
                        _ilcPitchWheel.SetColoredDefaultTexts(64);
                        break;
                    case "pan":
                        _ilcPan.Set(r.Value);
                        _ilcPan.SetColoredDefaultTexts(64);
                        break;
                    case "expression":
                        _ilcExpression.Set(r.Value);
                        _ilcExpression.SetColoredDefaultTexts(127);
                        break;
                    case "modulation":
                        _ilcModulation.Set(r.Value);
                        _ilcModulation.SetColoredDefaultTexts(0);
                        break;
                }
            }
        }

        public void Write(XmlWriter w)
        {
            w.WriteStartElement("trackInit");

            if(!_ilcVolume.IsEmpty())
            {
                w.WriteAttributeString("volume", _ilcVolume.ValuesAsString());
            }
            if(!_ilcPWDeviation.IsEmpty())
            {
                w.WriteAttributeString("pwDeviation", _ilcPWDeviation.ValuesAsString());
            }
            if(!_ilcPitchWheel.IsEmpty())
            {
                w.WriteAttributeString("pitchWheel", _ilcPitchWheel.ValuesAsString());
            }
            if(!_ilcPan.IsEmpty())
            {
                w.WriteAttributeString("pan", _ilcPan.ValuesAsString());
            }
            if(!_ilcExpression.IsEmpty())
            {
                w.WriteAttributeString("expression", _ilcExpression.ValuesAsString());
            }
            if(!_ilcModulation.IsEmpty())
            {
                w.WriteAttributeString("modulation", _ilcModulation.ValuesAsString());
            }

            w.WriteEndElement(); // trackInit
        }

        public bool IsEmpty()
        {
            bool isEmpty = true;
            foreach(IntListControl ilc in _intListControls)
            {
                if(!ilc.IsEmpty())
                {
                    isEmpty = false;
                    break;
                }
            }
            return isEmpty;
        }

        /// <summary>
        /// Removes the '*' in Text, disables the SaveButton and informs _ornamentSettingsForm
        /// </summary>
        public void SetSettingsHaveBeenSaved()
        {
            if(this.Text.EndsWith("*"))
            {
                this.Text = this.Text.Remove(this.Text.Length - 1);
            }
            this.SaveSettingsButton.Enabled = false;
        }
        /// <summary>
        /// Sets the '*' in Text, enables the SaveButton and informs _assistantComposerMainForm
        /// </summary>
        public void SetSettingsNotSaved()
        {
            if(!this.Text.EndsWith("*"))
            {
                this.Text = this.Text + "*";
                if(this._assistantComposerMainForm != null)
                {
                    _assistantComposerMainForm.SetSettingsHaveNotBeenSaved();
                }
            }
            this.SaveSettingsButton.Enabled = true;
        }
        #endregion

        #region IContainedControlHasChanged implementation
        /// <summary>
        /// Called by a contained IntListControl when it changes
        /// </summary>
        public void ContainedControlHasChanged(IntListControl sender)
        {
            _hasError = false;
            foreach(IntListControl ilc in _intListControls)
            {
                if(ilc.HasError())
                {
                    _hasError = true;
                    break;
                }
            }

            if(!_hasError)
            {
                if(sender == _ilcVolume)
                {
                    _ilcVolume.SetColoredDefaultTexts(100);
                }
                else if(sender == _ilcPWDeviation)
                {
                    _ilcPWDeviation.SetColoredDefaultTexts(2);
                }
                else if(sender == _ilcPitchWheel)
                {
                    _ilcPitchWheel.SetColoredDefaultTexts(64);
                }
                else if(sender == _ilcPan)
                {
                    _ilcPan.SetColoredDefaultTexts(64);
                }
                else if(sender == _ilcExpression)
                {
                    _ilcExpression.SetColoredDefaultTexts(127);
                }
                else if(sender == _ilcModulation)
                {
                    _ilcModulation.SetColoredDefaultTexts(0);
                }
            }

            SaveSettingsButton.Enabled = !_hasError;
        }
        #endregion

        public bool HasError()
        {
            return _hasError;
        }

        private int _nTracks;
        static private bool _hasError = false;      
        IntListControl _ilcVolume;        
        IntListControl _ilcPWDeviation;        
        IntListControl _ilcPitchWheel;        
        IntListControl _ilcPan;        
        IntListControl _ilcExpression;
        IntListControl _ilcModulation;        
        private List<IntListControl> _intListControls = new List<IntListControl>(); 
        private AssistantComposerMainForm _assistantComposerMainForm = null;
    }
}
