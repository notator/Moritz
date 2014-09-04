using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;

namespace Moritz.AssistantPerformer
{
    internal partial class Keyboard : UserControl
    {
        public Keyboard(KeyboardSettingsDialog form, Panel scrollablePanel, Point location, int globalIndex,
            List<KeyType> keyTypes)
        {
            InitializeComponent();
            InitializeKeyboard(keyTypes);

            Size = new Size(Keyboard.MidiKeyboardWidth, Keyboard.KeyboardHeight);

            Form = form;
            ScrollablePanel = scrollablePanel;
            Location = location;

            SetNumberBoxText((globalIndex + 1).ToString());
            _borderPen = _unselectedBorderPen;

            this.MouseDown += new MouseEventHandler(Keyboard_MouseDown);
            this.KeyboardNumberPanel.MouseDown += new MouseEventHandler(Keyboard_MouseDown);
            this.KeyboardNumberLabel.MouseDown += new MouseEventHandler(Keyboard_MouseDown);
        }

        private void InitializeKeyboard(List<KeyType> keyTypes)
        {
            AddKeys();
            if(keyTypes != null)
            {
                Debug.Assert(Keys.Length == keyTypes.Count);
                for(int i = 0; i < Keys.Length; i++)
                    Keys[i].KeyType = keyTypes[i];
            }
        }
        public int Length { get { return Keys.Length; } }

        void Keyboard_MouseDown(object sender, MouseEventArgs e)
        {
            if(this.Selected)
                this.Selected = false;
            else
            {
                if(!Form.ShiftKeyIsDown)
                {
                    Form.DeselectAllKeyboards();
                }
                this.Selected = true;
            }
            if(e.Button == MouseButtons.Right)
            {
                // select the range of keyboards between and including the lowest and highest selected.
                Form.SelectKeyboardRange();
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if(_backBuffer == null)
            {
                _backBuffer = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            }
            Graphics g = Graphics.FromImage(_backBuffer);

            g.Clear(Color.White);

            foreach(Key key in Keys)
            {
                key.Invalidate();
            }
            g.DrawRectangle(_borderPen, 1, 1, Size.Width - 3, Size.Height - 4);
            g.DrawLine(Pens.Red, 174, (Size.Height - 6), 174, Size.Height - 1);
            g.DrawLine(Pens.Red, 175, (Size.Height - 6), 175, Size.Height - 1);
            g.DrawLine(Pens.Magenta, 279, (Size.Height - 6), 279, Size.Height - 1);
            g.DrawLine(Pens.Magenta, 280, (Size.Height - 6), 280, Size.Height - 1);
            g.DrawLine(Pens.Blue, 453, (Size.Height - 6), 453, Size.Height - 1);
            g.DrawLine(Pens.Blue, 454, (Size.Height - 6), 454, Size.Height-1);
            g.DrawLine(Pens.Magenta, 623, (Size.Height - 6), 623, Size.Height - 1);
            g.DrawLine(Pens.Magenta, 624, (Size.Height - 6), 624, Size.Height - 1);
            g.DrawLine(Pens.Red, 791, (Size.Height - 6), 791, Size.Height - 1);
            g.DrawLine(Pens.Red, 792, (Size.Height - 6), 792, Size.Height-1);

            g.Dispose();
            //Copy the back buffer to the screen 
            e.Graphics.DrawImageUnscaled(_backBuffer, 0, 0);

            this.KeyboardNumberPanel.Invalidate();
        }
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //Don't allow the background to paint 
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            if(_backBuffer != null)
            {
                _backBuffer.Dispose();
                _backBuffer = null;
            }
            base.OnSizeChanged(e);
        }

        private void KeyboardNumberPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel p = sender as Panel;
            Graphics g = e.Graphics;
            g.DrawRectangle(_borderPen, 0, 0, p.Size.Width - 1, p.Size.Height - 1);
            //base.OnPaint(e);
        }
        public void MoveNumberLabelTo(int lableX)
        {    
            this.KeyboardNumberPanel.Location = new Point(lableX, this.KeyboardNumberPanel.Location.Y);
            this.KeyboardNumberPanel.BringToFront();
        }
        public void SetNumberBoxText(string numberString)
        {
            KeyboardNumberLabel.Text = numberString;
            switch(numberString.Length)
            {
                case 1:
                    KeyboardNumberPanel.Size = new Size(24, 29);
                    KeyboardNumberLabel.Location = new Point(4, KeyboardNumberLabel.Location.Y);
                    break;
                case 2:
                    KeyboardNumberPanel.Size = new Size(24, 29);
                    KeyboardNumberLabel.Location = new Point(0, KeyboardNumberLabel.Location.Y);
                    break;
                case 3:
                    KeyboardNumberPanel.Size = new Size(32, 29);
                    KeyboardNumberLabel.Location = new Point(0, KeyboardNumberLabel.Location.Y);
                    break;
                default:
                    KeyboardNumberPanel.Size = new Size(40, 29);
                    KeyboardNumberLabel.Location = new Point(0, KeyboardNumberLabel.Location.Y);
                    break;
            }
        }

        private void AddKeys()
        {
            int keysX = 28;
            int keysY = 3;
            this.SuspendLayout();
            int octaveX = 0;
            int keyIndex = 0;
            // the location of a key is the top left corner of its bounding box
            for(int octaveNumber = 0; octaveNumber < 10; octaveNumber++)
            {
                octaveX = keysX + octaveNumber * (WhiteKeyWidth * 7);
                CKey ckey = new CKey(this);
                Controls.Add(ckey);
                Keys[keyIndex++] = ckey;
                ckey.Location = new Point(octaveX, keysY);
                CSharpKey cskey = new CSharpKey(this);
                Controls.Add(cskey);
                Keys[keyIndex++] = cskey;
                cskey.Location = new Point(octaveX + 7, keysY);
                DKey dkey = new DKey(this);
                Controls.Add(dkey);
                Keys[keyIndex++] = dkey;
                dkey.Location = new Point(octaveX + 12, keysY);
                DSharpKey dskey = new DSharpKey(this);
                Controls.Add(dskey);
                Keys[keyIndex++] = dskey;
                dskey.Location = new Point(octaveX + 21, keysY);
                EKey ekey = new EKey(this);
                Controls.Add(ekey);
                Keys[keyIndex++] = ekey;
                ekey.Location = new Point(octaveX + 24, keysY);
                FKey fkey = new FKey(this);
                Controls.Add(fkey);
                Keys[keyIndex++] = fkey;
                fkey.Location = new Point(octaveX + 36, keysY);
                FSharpKey fskey = new FSharpKey(this);
                Controls.Add(fskey);
                Keys[keyIndex++] = fskey;
                fskey.Location = new Point(octaveX + 43, keysY);
                GKey gkey = new GKey(this);
                Controls.Add(gkey);
                Keys[keyIndex++] = gkey;
                gkey.Location = new Point(octaveX + 48, keysY);
                GSharpKey gskey = new GSharpKey(this);
                Controls.Add(gskey);
                Keys[keyIndex++] = gskey;
                gskey.Location = new Point(octaveX + 56, keysY);
                AKey akey = new AKey(this);
                Controls.Add(akey);
                Keys[keyIndex++] = akey;
                akey.Location = new Point(octaveX + 60, keysY);
                ASharpKey askey = new ASharpKey(this);
                Controls.Add(askey);
                Keys[keyIndex++] = askey;
                askey.Location = new Point(octaveX + 69, keysY);
                BKey bkey = new BKey(this);
                Controls.Add(bkey);
                Keys[keyIndex++] = bkey;
                bkey.Location = new Point(octaveX + 72, keysY);
            }
            // the top 8 keys
            octaveX = keysX + (10 * (WhiteKeyWidth * 7));
            CKey ctkey = new CKey(this);
            Controls.Add(ctkey);
            Keys[keyIndex++] = ctkey;
            ctkey.Location = new Point(octaveX, keysY);
            CSharpKey cstkey = new CSharpKey(this);
            Controls.Add(cstkey);
            Keys[keyIndex++] = cstkey;
            cstkey.Location = new Point(octaveX + 7, keysY);
            DKey dtkey = new DKey(this);
            Controls.Add(dtkey);
            Keys[keyIndex++] = dtkey;
            dtkey.Location = new Point(octaveX + 12, keysY);
            DSharpKey dstkey = new DSharpKey(this);
            Controls.Add(dstkey);
            Keys[keyIndex++] = dstkey;
            dstkey.Location = new Point(octaveX + 21, keysY);
            EKey etkey = new EKey(this);
            Controls.Add(etkey);
            Keys[keyIndex++] = etkey;
            etkey.Location = new Point(octaveX + 24, keysY);
            FKey ftkey = new FKey(this);
            Controls.Add(ftkey);
            Keys[keyIndex++] = ftkey;
            ftkey.Location = new Point(octaveX + 36, keysY);
            FSharpKey fstkey = new FSharpKey(this);
            Controls.Add(fstkey);
            Keys[keyIndex++] = fstkey;
            fstkey.Location = new Point(octaveX + 43, keysY);
            GTopKey gtkey = new GTopKey(this);
            Controls.Add(gtkey);
            Keys[keyIndex++] = gtkey;
            gtkey.Location = new Point(octaveX + 48, keysY);

            this.ResumeLayout(false);
        }
        public bool IsDefault()
        {
            bool isDefault = true;
            foreach(Key key in Keys)
            {
                if(key.KeyType != KeyType.Assisted)
                {
                    isDefault = false;
                    break;
                }
            }
            return isDefault;
        }

        private Pen _unselectedBorderPen = new Pen(Brushes.Black);
        private Pen _selectedBorderPen = new Pen(Brushes.Blue, 3);
        private Pen _borderPen;

        public void Deselect()
        {
            this.Selected = false;
            foreach(Key key in Keys)
            {
                key.Selected = false;
            }
        }

        public void DeselectAllKeyboards()
        {
            Form.DeselectAllKeyboards();
        }
        /// <summary>
        /// select the range of keys between and including the current lowest and highest selected.
        /// </summary>
        public void SelectKeyRange()
        {
            int lowIndex = 0;
            int highIndex = 0;
            for(int low = 0; low < Keys.Length; low++)
            {
                if(Keys[low].Selected)
                {
                    lowIndex = low;
                    break;
                }
            }
            for(int high = Keys.Length - 1; high >= 0; high--)
            {
                if(Keys[high].Selected)
                {
                    highIndex = high;
                    break;
                }
            }
            if(highIndex > lowIndex)
            {
                while(lowIndex++ < highIndex)
                    Keys[lowIndex].Selected = true;
            }
        }
        public void SetSelectedKeys(KeyType keyType)
        {
            foreach(Key key in Keys)
            {
                if(key.Selected)
                    key.KeyType = keyType;
            }
        }
        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                if(_selected)
                {
                    _borderPen = _selectedBorderPen;
                }
                else
                {
                    _borderPen = _unselectedBorderPen;
                }
                Form.SetDeleteKeyboardButtonState();
                Form.SetButtonsState();

                Invalidate();
            }
        }
        private bool _selected;

        public static int WhiteKeyWidth = 12;
        public static int WhiteKeyHeight = 48;
        public static int BlackKeyWidth = 8;
        public static int BlackKeyHeight = 32;
        public static int LeftMidiKeyboardMargin = 28;
        public static int KeyboardSelectionBorderThickness = 3;
        public static int PianoKeyboardWidth = WhiteKeyWidth * 51 + LeftMidiKeyboardMargin + 4;
        public static int MidiKeyboardWidth = (WhiteKeyWidth * 75) + LeftMidiKeyboardMargin + KeyboardSelectionBorderThickness;
        public static int KeyboardHeight = WhiteKeyHeight + 8;

        public bool ShiftKeyIsDown { get { return Form.ShiftKeyIsDown; } }

        public KeyboardSettingsDialog Form = null;
        public Panel ScrollablePanel = null;

        public Key[] Keys = new Key[128];

        private Bitmap _backBuffer;

    }
}
