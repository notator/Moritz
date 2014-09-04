using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Moritz.Globals;
using Moritz.Krystals;
using Moritz.Score;
using Moritz.Score.Midi;
using Moritz.Score.Notation;

namespace Moritz.AssistantPerformer
{
    internal abstract class Key : UserControl
    {
        public Key(Keyboard container)
        {
            InitializeComponent();
            _keyboard = container;
            _blackKeyWidth = Keyboard.BlackKeyWidth;
            _blackKeyHeight = Keyboard.BlackKeyHeight;
            _whiteKeyWidth = Keyboard.WhiteKeyWidth;
            _whiteKeyHeight = Keyboard.WhiteKeyHeight;
            _borderPen = _unselectedBorderPen;
            SetColors();
            KeyType = KeyType.Assisted; // default, sets brushes for painting
            this.MouseDown += new MouseEventHandler(Key_MouseDown);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && (components != null))
            {
                _silentBlackKeyBrush.Dispose();
                _solo_AssistantDeaf_BlackKeyBrush.Dispose();
                _solo_AssistantHearsFirst_BlackKeyBrush.Dispose();
                _unselectedBorderPen.Dispose();
                _solo_AssistantHearsFirst_WhiteKeyBrush.Dispose();
                _solo_AssistantDeaf_WhiteKeyBrush.Dispose();
                _silentWhiteKeyBrush.Dispose();
                _selectedBorderPen.Dispose();
                _assistedWhiteKeyBrush.Dispose();
                _assistedBlackKeyBrush.Dispose();

                components.Dispose();


            }
            base.Dispose(disposing);
        }


        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }

        #endregion

        /// <summary>
        /// Toggle the selected state of the key, using the shift key for multiple selection.
        /// </summary>
        /// <param name="e"></param>
        private void Key_MouseDown(object sender, MouseEventArgs e)
        {
            if(this.Selected)
                this.Selected = false;
            else
            {
                if(!ShiftKeyDown)
                {
                    _keyboard.DeselectAllKeyboards();
                }
                this.Selected = true;
            }
            if(e.Button == MouseButtons.Right)
            {
                // select the range of keys between and including the lowest and highest selected.
                _keyboard.SelectKeyRange();
            }
            _keyboard.Form.SetButtonsState();
            Invalidate();
        }

        //private void DeselectAllKeys()
        //{
        //    this.Selected = false;
        //    Key previousKey = PreviousKey;
        //    while(previousKey != null)
        //    {
        //        previousKey.Selected = false;
        //        previousKey = previousKey.PreviousKey;
        //    }
        //    Key nextKey = NextKey;
        //    while(nextKey != null)
        //    {
        //        nextKey.Selected = false;
        //        nextKey = nextKey.NextKey;
        //    }
        //}

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="pe">
        /// A PaintEventArgs that contains the event data. 
        /// </param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            g.FillRegion(_fillBrush, Region);
            base.OnPaint(pe);
            // Draw border last
            g.DrawPolygon(_borderPen, _borderPoints);
            if(_selected)
            {
                if(this is BlackKey)
                {
                    g.DrawLine(_borderPen, _borderPoints[0], _borderPoints[2]);
                    g.DrawLine(_borderPen, _borderPoints[1], _borderPoints[3]);
                }
                else
                {
                    g.DrawLine(_borderPen,
                        new Point(0, Keyboard.WhiteKeyHeight),
                        new Point(Keyboard.WhiteKeyWidth, Keyboard.BlackKeyHeight));
                    g.DrawLine(_borderPen,
                        new Point(0, Keyboard.BlackKeyHeight),
                        new Point(Keyboard.WhiteKeyWidth, Keyboard.WhiteKeyHeight));
                }
            }
        }

        /// <summary>
        /// Sets the points for drawing the type of key.
        /// The origin and starting point for drawing all keys is the left corner at the top of the key.
        /// </summary>
        protected abstract void SetPoints();

        /// <summary>
        /// Create region for piano key based on initialized points.
        /// </summary>
        protected void CreateRegion()
        {
            byte[] types = new byte[_regionPoints.Length];

            for(int i = 0; i < types.Length; i++)
            {
                types[i] = (byte)PathPointType.Line;
            }

            GraphicsPath path = new GraphicsPath(_regionPoints, types);

            Region = new Region(path);

            Invalidate(Region);
        }

        public KeyType KeyType
        {
            get { return _keyType; }
            set
            {
                _keyType = value;
                switch(_keyType)
                {
                    case KeyType.Silent:
                        if(this is WhiteKey)
                            _fillBrush = _silentWhiteKeyBrush;
                        else
                            _fillBrush = _silentBlackKeyBrush;
                        break;
                     case KeyType.Assisted:
                        if(this is WhiteKey)
                            _fillBrush = _assistedWhiteKeyBrush;
                        else
                            _fillBrush = _assistedBlackKeyBrush;
                        break;
                     case KeyType.Solo_AssistantHearsNothing:
                        if(this is WhiteKey)
                            _fillBrush = _solo_AssistantDeaf_WhiteKeyBrush;
                        else
                            _fillBrush = _solo_AssistantDeaf_BlackKeyBrush;
                        break;
                     case KeyType.Solo_AssistantHearsFirst:
                        if(this is WhiteKey)
                            _fillBrush = _solo_AssistantHearsFirst_WhiteKeyBrush;
                        else
                            _fillBrush = _solo_AssistantHearsFirst_BlackKeyBrush;
                        break;
                }
                Invalidate();
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
                    if(this is BlackKey)
                    {
                        switch(KeyType)
                        {
                            case KeyType.Silent:
                                _fillBrush = _silentWhiteKeyBrush;
                                break;
                            case KeyType.Assisted:
                                _fillBrush = _assistedWhiteKeyBrush;
                                break;
                            case KeyType.Solo_AssistantHearsNothing:
                                _fillBrush = _solo_AssistantDeaf_WhiteKeyBrush;
                                break;
                            case KeyType.Solo_AssistantHearsFirst:
                                _fillBrush = _solo_AssistantHearsFirst_WhiteKeyBrush;
                                break;
                        }
                    }
                }
                else
                {
                    _borderPen = _unselectedBorderPen;
                    if(this is BlackKey)
                    {
                        switch(KeyType)
                        {
                            case KeyType.Silent:
                                _fillBrush = _silentBlackKeyBrush;
                                break;
                            case KeyType.Assisted:
                                _fillBrush = _assistedBlackKeyBrush;
                                break;
                            case KeyType.Solo_AssistantHearsNothing:
                                _fillBrush = _solo_AssistantDeaf_BlackKeyBrush;
                                break;
                            case KeyType.Solo_AssistantHearsFirst:
                                _fillBrush = _solo_AssistantHearsFirst_BlackKeyBrush;
                                break;
                        }
                    }
                }
                Invalidate();
                _keyboard.Form.InvisibleTextBox.Focus();
            }
        }

        public bool ShiftKeyDown { get { return _keyboard.ShiftKeyIsDown; } }

        private void SetColors()
        {
            _assistedWhiteKeyBrush = new SolidBrush(Color.White);
            _assistedBlackKeyBrush = new SolidBrush(Color.FromArgb(80,80,80));
            _solo_AssistantDeaf_WhiteKeyBrush = new SolidBrush(Color.FromArgb(190, 255, 190));
            _solo_AssistantDeaf_BlackKeyBrush = new SolidBrush(Color.FromArgb(50, 210, 50));
            _solo_AssistantHearsFirst_WhiteKeyBrush = new SolidBrush(Color.FromArgb(190, 190, 255));
            _solo_AssistantHearsFirst_BlackKeyBrush = new SolidBrush(Color.FromArgb(50, 50, 210));
            _silentWhiteKeyBrush = new SolidBrush(Color.FromArgb(255, 190, 190));
            _silentBlackKeyBrush = new SolidBrush(Color.FromArgb(230, 90, 90));
        }

        // Brushes used to paint the keys
        private SolidBrush _solo_AssistantDeaf_WhiteKeyBrush;
        private SolidBrush _solo_AssistantDeaf_BlackKeyBrush;
        private SolidBrush _solo_AssistantHearsFirst_WhiteKeyBrush;
        private SolidBrush _solo_AssistantHearsFirst_BlackKeyBrush;
        private SolidBrush _assistedWhiteKeyBrush;
        private SolidBrush _assistedBlackKeyBrush;
        protected SolidBrush _silentWhiteKeyBrush;
        protected SolidBrush _silentBlackKeyBrush;
        protected SolidBrush _fillBrush;

        // Pens used to draw border.
        private Pen _selectedBorderPen = new Pen(Color.Blue);
        private Pen _unselectedBorderPen = new Pen(Color.Gainsboro);
        private Pen _borderPen;

        // Points representing the shape of the key.
        protected Point[] _regionPoints;
        protected Point[] _borderPoints;

        private KeyType _keyType = KeyType.Assisted;
        private bool _selected = false;

        protected int _blackKeyWidth = 0;
        protected int _blackKeyHeight = 0;
        protected int _whiteKeyWidth = 0;
        protected int _whiteKeyHeight = 0;
        protected Keyboard _keyboard = null;
    }

    internal abstract class BlackKey : Key
    {
        protected BlackKey(Keyboard container)
            : base(container)
        {
        }

        protected override void SetPoints()
        {
            _regionPoints = new Point[5];
            _regionPoints[0] = new Point(0, 0);
            _regionPoints[1] = new Point(_blackKeyWidth, 0);
            _regionPoints[2] = new Point(_blackKeyWidth, _blackKeyHeight);
            _regionPoints[3] = new Point(0, _blackKeyHeight);
            _regionPoints[4] = _regionPoints[0];
            CreateRegion();
            _borderPoints = new Point[5];
            _borderPoints[0] = new Point(0, 0);
            _borderPoints[1] = new Point(_blackKeyWidth - 1, 0);
            _borderPoints[2] = new Point(_blackKeyWidth - 1, _blackKeyHeight - 1);
            _borderPoints[3] = new Point(0, _blackKeyHeight - 1);
            _borderPoints[4] = _borderPoints[0];

        }
    }

    internal abstract class WhiteKey : Key
    {
        protected WhiteKey(Keyboard container)
            : base(container)
        {
        }
    }

    internal class CKey : WhiteKey
    {
        public CKey(Keyboard container)
            : base(container)
        {
            SetPoints();
        }
        // L-Key
        protected override void SetPoints()
        {
            _regionPoints = new Point[7];
            _regionPoints[0] = new Point(0, 0);
            _regionPoints[1] = new Point(7, 0);
            _regionPoints[2] = new Point(7, _blackKeyHeight);
            _regionPoints[3] = new Point(_whiteKeyWidth, _blackKeyHeight);
            _regionPoints[4] = new Point(_whiteKeyWidth, _whiteKeyHeight);
            _regionPoints[5] = new Point(0, _whiteKeyHeight);
            _regionPoints[6] = _regionPoints[0];
            CreateRegion();
            _borderPoints = new Point[7];
            _borderPoints[0] = new Point(0, 0);
            _borderPoints[1] = new Point(6, 0);
            _borderPoints[2] = new Point(6, _blackKeyHeight);
            _borderPoints[3] = new Point(_whiteKeyWidth - 1, _blackKeyHeight);
            _borderPoints[4] = new Point(_whiteKeyWidth - 1, _whiteKeyHeight - 1);
            _borderPoints[5] = new Point(0, _whiteKeyHeight - 1);
            _borderPoints[6] = _borderPoints[0];
        }
    }
    internal class CSharpKey : BlackKey
    {
        public CSharpKey(Keyboard container)
            : base(container)
        {
            SetPoints();
        }
    }
    internal class DKey : WhiteKey
    {
        public DKey(Keyboard container)
            : base(container)
        {
            SetPoints();
        }
        // T-Key
        protected override void SetPoints()
        {
            _regionPoints = new Point[9];
            _regionPoints[0] = new Point(3, 0);
            _regionPoints[1] = new Point(9, 0);
            _regionPoints[2] = new Point(9, _blackKeyHeight);
            _regionPoints[3] = new Point(_whiteKeyWidth, _blackKeyHeight);
            _regionPoints[4] = new Point(_whiteKeyWidth, _whiteKeyHeight);
            _regionPoints[5] = new Point(0, _whiteKeyHeight);
            _regionPoints[6] = new Point(0, _blackKeyHeight);
            _regionPoints[7] = new Point(3, _blackKeyHeight);
            _regionPoints[8] = _regionPoints[0];
            CreateRegion();
            _borderPoints = new Point[9];
            _borderPoints[0] = new Point(3, 0);
            _borderPoints[1] = new Point(8, 0);
            _borderPoints[2] = new Point(8, _blackKeyHeight);
            _borderPoints[3] = new Point(_whiteKeyWidth - 1, _blackKeyHeight);
            _borderPoints[4] = new Point(_whiteKeyWidth - 1, _whiteKeyHeight - 1);
            _borderPoints[5] = new Point(0, _whiteKeyHeight - 1);
            _borderPoints[6] = new Point(0, _blackKeyHeight);
            _borderPoints[7] = new Point(3, _blackKeyHeight);
            _borderPoints[8] = _borderPoints[0];
        }
    }
    internal class DSharpKey : BlackKey
    {
        public DSharpKey(Keyboard container)
            : base(container)
        {
            SetPoints();
        }
    }
    internal class EKey : WhiteKey
    {
        public EKey(Keyboard container)
            : base(container)
        {
            SetPoints();
        }
        // backwards L-Key
        protected override void SetPoints()
        {
            _regionPoints = new Point[7];
            _regionPoints[0] = new Point(5, 0);
            _regionPoints[1] = new Point(_whiteKeyWidth, 0);
            _regionPoints[2] = new Point(_whiteKeyWidth, _whiteKeyHeight);
            _regionPoints[3] = new Point(0, _whiteKeyHeight);
            _regionPoints[4] = new Point(0, _blackKeyHeight);
            _regionPoints[5] = new Point(5, _blackKeyHeight);
            _regionPoints[6] = _regionPoints[0];
            CreateRegion();
            _borderPoints = new Point[7];
            _borderPoints[0] = new Point(5, 0);
            _borderPoints[1] = new Point(_whiteKeyWidth - 1, 0);
            _borderPoints[2] = new Point(_whiteKeyWidth - 1, _whiteKeyHeight - 1);
            _borderPoints[3] = new Point(0, _whiteKeyHeight - 1);
            _borderPoints[4] = new Point(0, _blackKeyHeight);
            _borderPoints[5] = new Point(5, _blackKeyHeight);
            _borderPoints[6] = _borderPoints[0];
        }
    }
    internal class FKey : WhiteKey
    {
        public FKey(Keyboard container)
            : base(container)
        {
            SetPoints();
        }
        // L-Key
        protected override void SetPoints()
        {
            _regionPoints = new Point[7];
            _regionPoints[0] = new Point(0, 0);
            _regionPoints[1] = new Point(7, 0);
            _regionPoints[2] = new Point(7, _blackKeyHeight);
            _regionPoints[3] = new Point(_whiteKeyWidth, _blackKeyHeight);
            _regionPoints[4] = new Point(_whiteKeyWidth, _whiteKeyHeight);
            _regionPoints[5] = new Point(0, _whiteKeyHeight);
            _regionPoints[6] = _regionPoints[0];
            CreateRegion();
            _borderPoints = new Point[7];
            _borderPoints[0] = new Point(0, 0);
            _borderPoints[1] = new Point(6, 0);
            _borderPoints[2] = new Point(6, _blackKeyHeight);
            _borderPoints[3] = new Point(_whiteKeyWidth - 1, _blackKeyHeight);
            _borderPoints[4] = new Point(_whiteKeyWidth - 1, _whiteKeyHeight - 1);
            _borderPoints[5] = new Point(0, _whiteKeyHeight - 1);
            _borderPoints[6] = _borderPoints[0];

        }
    }
    internal class FSharpKey : BlackKey
    {
        public FSharpKey(Keyboard container)
            : base(container)
        {
            SetPoints();
        }
    }
    internal class GKey : WhiteKey
    {
        public GKey(Keyboard container)
            : base(container)
        {
            SetPoints();
        }
        // T-Key
        protected override void SetPoints()
        {
            _regionPoints = new Point[9];
            _regionPoints[0] = new Point(3, 0);
            _regionPoints[1] = new Point(8, 0);
            _regionPoints[2] = new Point(8, _blackKeyHeight);
            _regionPoints[3] = new Point(_whiteKeyWidth, _blackKeyHeight);
            _regionPoints[4] = new Point(_whiteKeyWidth, _whiteKeyHeight);
            _regionPoints[5] = new Point(0, _whiteKeyHeight);
            _regionPoints[6] = new Point(0, _blackKeyHeight);
            _regionPoints[7] = new Point(3, _blackKeyHeight);
            _regionPoints[8] = _regionPoints[0];
            CreateRegion();
            _borderPoints = new Point[9];
            _borderPoints[0] = new Point(3, 0);
            _borderPoints[1] = new Point(7, 0);
            _borderPoints[2] = new Point(7, _blackKeyHeight);
            _borderPoints[3] = new Point(_whiteKeyWidth - 1, _blackKeyHeight);
            _borderPoints[4] = new Point(_whiteKeyWidth - 1, _whiteKeyHeight - 1);
            _borderPoints[5] = new Point(0, _whiteKeyHeight - 1);
            _borderPoints[6] = new Point(0, _blackKeyHeight);
            _borderPoints[7] = new Point(3, _blackKeyHeight);
            _borderPoints[8] = _borderPoints[0];
        }
    }
    internal class GSharpKey : BlackKey
    {
        public GSharpKey(Keyboard container)
            : base(container)
        {
            SetPoints();
        }
    }
    internal class AKey : WhiteKey
    {
        public AKey(Keyboard container)
            : base(container)
        {
            SetPoints();
        }
        // T-Key
        protected override void SetPoints()
        {
            _regionPoints = new Point[9];
            _regionPoints[0] = new Point(4, 0);
            _regionPoints[1] = new Point(9, 0);
            _regionPoints[2] = new Point(9, _blackKeyHeight);
            _regionPoints[3] = new Point(_whiteKeyWidth, _blackKeyHeight);
            _regionPoints[4] = new Point(_whiteKeyWidth, _whiteKeyHeight);
            _regionPoints[5] = new Point(0, _whiteKeyHeight);
            _regionPoints[6] = new Point(0, _blackKeyHeight);
            _regionPoints[7] = new Point(4, _blackKeyHeight);
            _regionPoints[8] = _regionPoints[0];
            CreateRegion();
            _borderPoints = new Point[9];
            _borderPoints[0] = new Point(4, 0);
            _borderPoints[1] = new Point(8, 0);
            _borderPoints[2] = new Point(8, _blackKeyHeight);
            _borderPoints[3] = new Point(_whiteKeyWidth - 1, _blackKeyHeight);
            _borderPoints[4] = new Point(_whiteKeyWidth - 1, _whiteKeyHeight - 1);
            _borderPoints[5] = new Point(0, _whiteKeyHeight - 1);
            _borderPoints[6] = new Point(0, _blackKeyHeight);
            _borderPoints[7] = new Point(4, _blackKeyHeight);
            _borderPoints[8] = _borderPoints[0];

        }
    }
    internal class ASharpKey : BlackKey
    {
        public ASharpKey(Keyboard container)
            : base(container)
        {
            SetPoints();
        }
    }
    internal class BKey : WhiteKey
    {
        public BKey(Keyboard container)
            : base(container)
        {
            SetPoints();
        }
        // backwards L-Key
        protected override void SetPoints()
        {
            _regionPoints = new Point[7];
            _regionPoints[0] = new Point(5, 0);
            _regionPoints[1] = new Point(_whiteKeyWidth, 0);
            _regionPoints[2] = new Point(_whiteKeyWidth, _whiteKeyHeight);
            _regionPoints[3] = new Point(0, _whiteKeyHeight);
            _regionPoints[4] = new Point(0, _blackKeyHeight);
            _regionPoints[5] = new Point(5, _blackKeyHeight);
            _regionPoints[6] = _regionPoints[0];
            CreateRegion();
            _borderPoints = new Point[7];
            _borderPoints[0] = new Point(5, 0);
            _borderPoints[1] = new Point(_whiteKeyWidth - 1, 0);
            _borderPoints[2] = new Point(_whiteKeyWidth - 1, _whiteKeyHeight - 1);
            _borderPoints[3] = new Point(0, _whiteKeyHeight - 1);
            _borderPoints[4] = new Point(0, _blackKeyHeight);
            _borderPoints[5] = new Point(5, _blackKeyHeight);
            _borderPoints[6] = _borderPoints[0];
        }
    }
    /// <summary>
    /// The very top G of each keyboard
    /// </summary>
    internal class GTopKey : WhiteKey
    {
        public GTopKey(Keyboard container)
            : base(container)
        {
            SetPoints();
        }
        // backwards L-Key
        protected override void SetPoints()
        {
            _regionPoints = new Point[7];
            _regionPoints[0] = new Point(3, 0);
            _regionPoints[1] = new Point(_whiteKeyWidth, 0);
            _regionPoints[2] = new Point(_whiteKeyWidth, _whiteKeyHeight);
            _regionPoints[3] = new Point(0, _whiteKeyHeight);
            _regionPoints[4] = new Point(0, _blackKeyHeight);
            _regionPoints[5] = new Point(3, _blackKeyHeight);
            _regionPoints[6] = _regionPoints[0];
            CreateRegion();
            _borderPoints = new Point[7];
            _borderPoints[0] = new Point(3, 0);
            _borderPoints[1] = new Point(_whiteKeyWidth - 1, 0);
            _borderPoints[2] = new Point(_whiteKeyWidth - 1, _whiteKeyHeight - 1);
            _borderPoints[3] = new Point(0, _whiteKeyHeight - 1);
            _borderPoints[4] = new Point(0, _blackKeyHeight);
            _borderPoints[5] = new Point(3, _blackKeyHeight);
            _borderPoints[6] = _borderPoints[0];
        }
    }

}
