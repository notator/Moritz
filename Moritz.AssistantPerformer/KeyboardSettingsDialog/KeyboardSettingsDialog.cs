using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;

namespace Moritz.AssistantPerformer
{
    internal partial class KeyboardSettingsDialog : Form
    {
        public KeyboardSettingsDialog(MoritzPerformanceOptions performanceOptions)
        {
            InitializeComponent();
            InitializeDialog();

            this.Text = "Performer's keyboard settings for " + performanceOptions.FileName;

            _moritzPerformanceOptions = performanceOptions;

            AddKeyboards(_moritzPerformanceOptions.KeyboardSettings);

            MidiFrameButton_Click(null, null);
        }

        private void AddKeyboards(List<List<KeyType>> keyboardSettings)
        {
            int y = 3;
            for(int i = 0; i < keyboardSettings.Count; i++)
            {
                Keyboard keyboard = new Keyboard(this, Panel, new Point(_keyboardXinPanel, y), i, keyboardSettings[i]);
                _keyboards.Add(keyboard);
                this.Panel.Controls.Add(keyboard);
                y += keyboard.Size.Height;
            }
        }

        private void InitializeDialog()
        {
            this.Width = Panel.Width + 24;
            Panel.Width = Keyboard.MidiKeyboardWidth + 6;
            Panel.Location = new Point(12, 12);

            this.MouseClick += new MouseEventHandler(KeyboardSettingsDialog_MouseClick);
            this.InvisibleTextBox.KeyDown += new KeyEventHandler(KeyboardSettingsDialog_KeyDown);
            this.InvisibleTextBox.KeyUp += new KeyEventHandler(KeyboardSettingsDialog_KeyUp);
            this.InvisibleTextBox.MouseWheel += new MouseEventHandler(InvisibleTextBox_MouseWheel);
            InvisibleTextBox.Focus();
            //this.Panel.MouseWheel += new MouseEventHandler(Panel_MouseWheel);

            VScrollBar.Height = (Keyboard.KeyboardHeight * 7) + 6;
            VScrollBar.Top = Panel.Top;
            VScrollBar.Left = Panel.Left + Panel.Width;
            VScrollBar.LargeChange = MaximumVisibleKeyboards;

            this.DeadKeysButton.Enabled = false;
            this.Solo_AssistantHearsNothingButton.Enabled = false;
            this.Solo_AssistantHearsFirstButton.Enabled = false;
            this.AssistedKeysButton.Enabled = false;
        }


        void InvisibleTextBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.None)
            {
                if(e.Delta > 0 && _currentTopKeyboardIndexInPanel > 0)
                    ScrollKeyboardsTo(_currentTopKeyboardIndexInPanel - 1);
                else if(e.Delta < 0 && _currentTopKeyboardIndexInPanel < (_keyboards.Count - MaximumVisibleKeyboards))
                    ScrollKeyboardsTo(_currentTopKeyboardIndexInPanel + 1);
            }    
        }

        public void DeselectAllKeyboards()
        {
            foreach(Keyboard keyboard in _keyboards)
                keyboard.Deselect();
            SetButtonsState();
        }
        /// <summary>
        /// select the range of keys between and including the current lowest and highest selected.
        /// </summary>
        public void SelectKeyboardRange()
        {
            int lowIndex = 0;
            int highIndex = 0;
            for(int low = 0; low < _keyboards.Count; low++)
            {
                if(_keyboards[low].Selected)
                {
                    lowIndex = low;
                    break;
                }
            }
            for(int high = _keyboards.Count - 1; high >= 0; high--)
            {
                if(_keyboards[high].Selected)
                {
                    highIndex = high;
                    break;
                }
            }
            if(highIndex > lowIndex)
            {
                while(lowIndex++ < highIndex)
                    _keyboards[lowIndex].Selected = true;
            }
            //Panel.Focus()
        }
        public void SetDeleteKeyboardButtonState()
        {
            bool selectedKeyboards = false;
            foreach(Keyboard keyboard in _keyboards)
            {
                if(keyboard.Selected)
                {
                    selectedKeyboards = true;
                    break;
                }
            }
            if(selectedKeyboards)
                this.DeleteKeyboardButton.Enabled = true;
            else
                this.DeleteKeyboardButton.Enabled = false;
        }

        private int NumberOfSelectedKeyboards()
        {
            int keyboardsSelected = 0;
            foreach(Keyboard keyboard in _keyboards)
            {
                if(keyboard.Selected)
                    keyboardsSelected++;
            }
            return keyboardsSelected;
        }

        public void SetButtonsState()
        {
            int keyboardsSelected = NumberOfSelectedKeyboards();
            bool keysSelected = false;
            if(keyboardsSelected == 0)
            {
                foreach(Keyboard keyboard in _keyboards)
                {
                    foreach(Key key in keyboard.Keys)
                    {
                        if(key.Selected)
                        {
                            keysSelected = true;
                            break;
                        }
                    }
                    if(keysSelected)
                        break;
                }
            }
            if(keyboardsSelected > 0 || keysSelected)
            {
                this.DeadKeysButton.Enabled = true;
                this.Solo_AssistantHearsFirstButton.Enabled = true;
                this.Solo_AssistantHearsNothingButton.Enabled = true;
                this.AssistedKeysButton.Enabled = true;
            }
            else
            {
                this.DeadKeysButton.Enabled = false;
                this.Solo_AssistantHearsFirstButton.Enabled = false;
                this.Solo_AssistantHearsNothingButton.Enabled = false;
                this.AssistedKeysButton.Enabled = false;
            }

            if(keyboardsSelected == 0)
                this.DeleteKeyboardButton.Enabled = false;
            else
                this.DeleteKeyboardButton.Enabled = true;

            if(keyboardsSelected < 2)
                this.DeleteKeyboardButton.Text = "Delete keyboard";
            else
                this.DeleteKeyboardButton.Text = "Delete keyboards";
        }

        private void KeyboardSettingsDialog_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if(e.Modifiers == Keys.Shift)
            {
                _shiftKeyIsDown = true;
            }
            //Panel.Focus()
        }
        private void KeyboardSettingsDialog_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if(e.Modifiers == Keys.None)
            {
                _shiftKeyIsDown = false;
            }
            //Panel.Focus()
        }
        private void KeyboardSettingsDialog_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            DeselectAllKeyboards();
            InvisibleTextBox.Focus();
        }
        private void Panel_Click(object sender, EventArgs e)
        {
            DeselectAllKeyboards();
            InvisibleTextBox.Focus();
        }

        private void VScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if(e.ScrollOrientation == ScrollOrientation.VerticalScroll && e.NewValue != e.OldValue)
            {
                ScrollKeyboardsTo(e.NewValue);
            }
            InvisibleTextBox.Focus();
        }
        private void ScrollKeyboardsTo(int topKeyboardIndex)
        {
            int y = (topKeyboardIndex * Keyboard.KeyboardHeight * -1) + 2; 
            foreach(Keyboard keyboard in _keyboards)
            {
                keyboard.Location = new Point(keyboard.Location.X, y);
                y += Keyboard.KeyboardHeight;
            }
            VScrollBar.Maximum = _keyboards.Count - 1;
            VScrollBar.Value = topKeyboardIndex;
            //Panel.Focus()
            _currentTopKeyboardIndexInPanel = topKeyboardIndex;
        }

        private void PianoFrameButton_Click(object sender, EventArgs e)
        {
            SetFrame(-146, Keyboard.PianoKeyboardWidth);
            SetButtonLocations(52);
            this.UsageGroupBox.Visible = false;
        }
        private void MidiFrameButton_Click(object sender, EventArgs e)
        {
            SetFrame(3, Keyboard.MidiKeyboardWidth);
            SetButtonLocations(350); 
            this.UsageGroupBox.Visible = true;
        }

        private void SetButtonLocations(int x)
        {
            AssistedKeysButton.Left = x;
            Solo_AssistantHearsNothingButton.Left = x + 141;
            Solo_AssistantHearsFirstButton.Left = x + 302;
            DeadKeysButton.Left = x + 463;
            PianoWidthButton.Left = x;
            MidiWidthButton.Left = x + 81;
            NewKeyboardButton.Left = x + 194;
            DeleteKeyboardButton.Left = x +302;
            OKButton.Left = x + 442;
            CancelSettingsButton.Left = x + 523;
        }

        private void AdjustHeights()
        {
            int nKeyboards = MaximumVisibleKeyboards;
            nKeyboards = (_keyboards.Count < nKeyboards) ? _keyboards.Count : nKeyboards;
            this.Size = new Size(this.Size.Width, (nKeyboards * Keyboard.KeyboardHeight) + 6 + 121);
            Panel.Size = new Size(Panel.Size.Width, (nKeyboards * Keyboard.KeyboardHeight) + 6);
            Panel.Location = new Point(12, 12);
        }
        private void SetFrame(int keyboardX, int keyboardWidth)
        {
            // _keyboardXinPanel is used when adding and deleting keyboards
            _keyboardXinPanel = keyboardX;

            AdjustHeights();

            this.Size = new Size(keyboardWidth + 6 + 27, this.Size.Height);
            Panel.Size = new Size(keyboardWidth + 6, Panel.Height );
            Panel.Location = new Point(12, 12);
            VScrollBar.Visible = false;
            //if(this.Panel.MouseWheel != null)
            //    this.Panel.MouseWheel -= new MouseEventHandler(Panel_MouseWheel);
            if(_keyboards.Count > MaximumVisibleKeyboards)
            {
                this.Size = new Size(this.Size.Width + VScrollBar.Width, this.Size.Height);
                Panel.Location = new Point(12, 12);
                VScrollBar.Location = new Point(this.Size.Width - 15 - VScrollBar.Width, 12);
                VScrollBar.Visible = true;
                //if(this.Panel.MouseWheel == null)
                //    this.Panel.MouseWheel += new MouseEventHandler(Panel_MouseWheel);
            }

            for(int i = 0; i < _keyboards.Count; i++)
            {
                _keyboards[i].Location = new Point(keyboardX, _keyboards[i].Location.Y);
                _keyboards[i].MoveNumberLabelTo(6 - keyboardX);
            }

            System.Drawing.Rectangle rect = Screen.GetWorkingArea(this);
            this.Location = new Point(((rect.Width - this.Width) / 2),((rect.Height - this.Height) / 2));

            InvisibleTextBox.Focus();
        }

        private void NewKeyboardButton_Click(object sender, EventArgs e)
        {
            int y = 3 + (Keyboard.KeyboardHeight * _keyboards.Count);
            Keyboard k = new Keyboard(this, this.Panel,
                new Point(Panel.DisplayRectangle.X + _keyboardXinPanel, y), _keyboards.Count, null);
            _keyboards.Add(k);
            Panel.Controls.Add(k);

            if(_keyboardXinPanel < 0)
                PianoFrameButton_Click(null, null);
            else
                MidiFrameButton_Click(this, null);

            if(_keyboards.Count >= MaximumVisibleKeyboards)
                ScrollKeyboardsTo(_keyboards.Count - MaximumVisibleKeyboards);
        }
        private void DeleteKeyboardButton_Click(object sender, EventArgs e)
        {
            int nKeyboards = NumberOfSelectedKeyboards();
            string lastWord = (nKeyboards > 1) ? "keyboards?" : "keyboard?";
            string msg = "Do you really want to delete the selected " + lastWord;
            DialogResult result = MessageBox.Show(msg, "Delete?", MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if(result == DialogResult.Yes)
            {
                List<Keyboard> toKeep = new List<Keyboard>();
                foreach(Keyboard keyboard in _keyboards)
                {
                    if(keyboard.Selected == false)
                        toKeep.Add(keyboard);
                }
                _keyboards = toKeep;
                this.Panel.Controls.Clear();
                int y = 3 + Panel.DisplayRectangle.Y;
                for(int i = 0; i < _keyboards.Count; i++)
                {
                    _keyboards[i].Location = new Point(Panel.DisplayRectangle.X + _keyboardXinPanel, y);
                    _keyboards[i].SetNumberBoxText((i + 1).ToString());
                    this.Panel.Controls.Add(_keyboards[i]);
                    y += _keyboards[i].Size.Height;
                }

                SetButtonsState();

                if(_keyboardXinPanel < 0)
                    PianoFrameButton_Click(null, null);
                else
                    MidiFrameButton_Click(this, null);
            }
        }

        private void DeadKeysButton_Click(object sender, EventArgs e)
        {
            foreach(Keyboard keyboard in _keyboards)
            {
                if(keyboard.Selected == true)
                {
                    foreach(Key key in keyboard.Keys)
                    {
                        key.KeyType = KeyType.Silent;
                    }
                }
                else
                {
                    keyboard.SetSelectedKeys(KeyType.Silent);
                }
            }
            InvisibleTextBox.Focus();
        }
        private void Solo_AssistantHearsNothingButton_Click(object sender, EventArgs e)
        {
            foreach(Keyboard keyboard in _keyboards)
            {
                if(keyboard.Selected == true)
                {
                    foreach(Key key in keyboard.Keys)
                    {
                        key.KeyType = KeyType.Solo_AssistantHearsNothing;
                    }
                }
                else
                {
                    keyboard.SetSelectedKeys(KeyType.Solo_AssistantHearsNothing);
                }
            }
            InvisibleTextBox.Focus();
        }
        private void Solo_AssistantHearsFirstButton_Click(object sender, EventArgs e)
        {
            foreach(Keyboard keyboard in _keyboards)
            {
                if(keyboard.Selected == true)
                {
                    foreach(Key key in keyboard.Keys)
                    {
                        key.KeyType = KeyType.Solo_AssistantHearsFirst;
                    }
                }
                else
                {
                    keyboard.SetSelectedKeys(KeyType.Solo_AssistantHearsFirst);
                }
            }
            InvisibleTextBox.Focus();
        }
        private void AssistedKeysButton_Click(object sender, EventArgs e)
        {
            foreach(Keyboard keyboard in _keyboards)
            {
                if(keyboard.Selected == true)
                {
                    foreach(Key key in keyboard.Keys)
                    {
                        key.KeyType = KeyType.Assisted;
                    }
                }
                else
                {
                    keyboard.SetSelectedKeys(KeyType.Assisted);
                }
            }
            InvisibleTextBox.Focus();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Close();    
        }

        private void SaveSettings()
        {
            List<List<KeyType>> keyboardSettings = new List<List<KeyType>>();
            foreach(Keyboard keyboard in _keyboards)
            {
                List<KeyType> keyboardSetting = new List<KeyType>();
                keyboardSettings.Add(keyboardSetting);
                foreach(Key key in keyboard.Keys)
                {
                    keyboardSetting.Add(key.KeyType);
                }
            }
            _moritzPerformanceOptions.KeyboardSettings = keyboardSettings;

            _moritzPerformanceOptions.Save();
        }

        private void CancelSettingsButton_Click(object sender, EventArgs e)
        {
            string msg = "Discard changes to the settings?";
            DialogResult result = MessageBox.Show(msg, "Cancel", MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if(result == DialogResult.Yes)
            {
                DialogResult = DialogResult.Cancel;
                Close();    
            }
            else
                InvisibleTextBox.Focus();
        }

        public bool ShiftKeyIsDown { get { return _shiftKeyIsDown; } }

        private List<Keyboard> _keyboards = new List<Keyboard>();
        private bool _shiftKeyIsDown = false;

        private const int MaximumVisibleKeyboards = 7;
        private int _keyboardXinPanel = 3;
        private int _currentTopKeyboardIndexInPanel = 0;

        private string _moritzOptionsPathname = string.Empty;
        MoritzPerformanceOptions _moritzPerformanceOptions = null;
    }
}
