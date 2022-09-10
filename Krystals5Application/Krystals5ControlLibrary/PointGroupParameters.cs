using Krystals5ObjectLibrary;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Krystals5ControlLibrary
{
    internal partial class PointGroupParameters : UserControl
    {
        public PointGroupParameters()
        {
            InitializeComponent();
            this.Enabled = false;
            SetInitialValues();
            PlanetValueUIntControl.updateContainer += new UnsignedIntControl.UintControlReturnKeyHandler(UpdatePointGroupParameters);
            StartMomentUIntControl.updateContainer += new UnsignedIntControl.UintControlReturnKeyHandler(UpdatePointGroupParameters);
            FixedPointsValuesUIntSeqControl.updateContainer += new UnsignedIntSeqControl.UnsignedIntSeqControlReturnKeyHandler(UpdatePointGroupParameters);
            ShiftAngleFloatControl.updateContainer = new FloatControl.FloatControlReturnKeyHandler(UpdatePointGroupParameters);
            ShiftRadiusFloatControl.updateContainer = new FloatControl.FloatControlReturnKeyHandler(UpdatePointGroupParameters);
            RotationFloatControl.updateContainer = new FloatControl.FloatControlReturnKeyHandler(UpdatePointGroupParameters);
            ToAngleFloatControl.updateContainer = new FloatControl.FloatControlReturnKeyHandler(UpdatePointGroupParameters);
            ToRadiusFloatControl.updateContainer = new FloatControl.FloatControlReturnKeyHandler(UpdatePointGroupParameters);
            FromRadiusFloatControl.updateContainer = new FloatControl.FloatControlReturnKeyHandler(UpdatePointGroupParameters);
            FromAngleFloatControl.updateContainer = new FloatControl.FloatControlReturnKeyHandler(UpdatePointGroupParameters);
        }

        #region public functions
        public void InitSamplePanel(
                        float inputDotSize,
                        float outputDotSize,
                        Pen theLinePen,
                        Brush theOutputFillBrush)
        {
            _editingOutputPoints = true;
            _editingFixedPoints = true;
            _sg = SamplePanel.CreateGraphics();
            _sg.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            _theLinePen = theLinePen;
            _theLinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            _inputDotSize = inputDotSize;
            _outputDotSize = outputDotSize;
            _theOutputFillBrush = theOutputFillBrush; // used for filling dots
            // The following line causes this control to call DrawPanel(),
            // so requires the above fields to have been set.
            ColorComboBox.SelectedIndex = 0; // black
        }

        public void SetControl()
        {
            if(_editingOutputPoints)
            {
                if(_editingFixedPoints)
                {
                    TitleText = "Fixed Ouput Points";
                    CountLabelText = "            Count:";
                }
                else
                {
                    TitleText = "Output Planet Subpath";
                    CountLabelText = "Start Moment:";
                }
            }
            else // editing input (=expansion) points
            {
                if(_editingFixedPoints)
                {
                    TitleText = "Fixed Input Points";
                    CountLabelText = "            Count:";
                    //                    this.ShapeComboBox.SelectedIndex = 0; // Circle is default (when editing new fixed input points)
                }
                else
                {
                    TitleText = "Input Planet Subpath";
                    CountLabelText = "Start Moment:";
                    //                    this.ShapeComboBox.SelectedIndex = 1; // Spiral is default (when editing new input planets
                }
            }
            SetValueControls();
            DrawSamplePanel();
        }
        private void SetValueControls()
        {
            if(_editingFixedPoints)
            {
                CountValueLabel.Visible = true;
                StartMomentUIntControl.Visible = false;
                FixedPointsValuesUIntSeqControl.Visible = true;
                FixedPointsValuesLabel.Visible = true;
                PlanetValueUIntControl.Visible = false;
                PlanetValueLabel.Visible = false;
            }
            else
            {
                CountValueLabel.Visible = false;
                StartMomentUIntControl.Visible = true;
                FixedPointsValuesUIntSeqControl.Visible = false;
                FixedPointsValuesLabel.Visible = false;
                PlanetValueUIntControl.Visible = true;
                PlanetValueLabel.Visible = true;
            }
        }

        public PointGroup GetPointGroup()
        {
            PointGroup pGroup = new PointGroup();
            pGroup.Shape = (K.PointGroupShape)ShapeComboBox.SelectedIndex;

            if(_editingFixedPoints)
            {
                pGroup.Value = K.GetUIntList(FixedPointsValuesUIntSeqControl.Sequence.ToString());
                pGroup.Count = (uint)pGroup.Value.Count; // the number of values in the Value string
            }
            else
            {
                pGroup.StartMoment = uint.Parse(StartMomentUIntControl.UnsignedInteger.ToString());	// MidiMoments
                //if (pGroup.StartMoment == 0)
                //    pGroup.StartMoment = 1;
                pGroup.Value = K.GetUIntList(PlanetValueUIntControl.UnsignedInteger.ToString());
                //pGroup.Count = 1; // this value requires knowledge of the other pointGroups in the planet
            }

            pGroup.FromRadius = float.Parse(FromRadiusFloatControl.Float.ToString());
            pGroup.FromAngle = float.Parse(FromAngleFloatControl.Float.ToString());

            if(pGroup.Shape == K.PointGroupShape.circle)
            {
                pGroup.ToRadius = float.Parse(CircleToRadiusValueLabel.Text);
                pGroup.ToAngle = float.Parse(CircleToAngleValueLabel.Text);
            }
            else
            {
                pGroup.ToRadius = float.Parse(ToRadiusFloatControl.Float.ToString());
                pGroup.ToAngle = float.Parse(ToAngleFloatControl.Float.ToString());
            }
            pGroup.RotateAngle = float.Parse(RotationFloatControl.Float.ToString());
            pGroup.TranslateRadius = float.Parse(ShiftRadiusFloatControl.Float.ToString());
            pGroup.TranslateAngle = float.Parse(ShiftAngleFloatControl.Float.ToString());

            pGroup.Visible = VisibleCheckBox.Checked;
            pGroup.Color = (K.DisplayColor)ColorComboBox.SelectedIndex;
            return pGroup;
        }
        public void SetPointGroup(PointGroup pg)
        {
            if(pg == null) // disable this control
            {
                this.Enabled = false;
                // some objects are rendered black even when this control is disabled,
                // so hide them as well
                this.CircleToAngleValueLabel.Visible = false;
                this.CircleToRadiusValueLabel.Visible = false;
                this.CountValueLabel.Visible = false;
                this.SamplePanel.Visible = false;
            }
            else
            {
                this.Enabled = true;
                this.CircleToAngleValueLabel.Visible = true;
                this.CircleToRadiusValueLabel.Visible = true;
                this.CountValueLabel.Visible = true;
                this.SamplePanel.Visible = true;

                this.ShapeComboBox.SelectedIndex = (int)pg.Shape;

                if(_editingFixedPoints)
                {
                    FixedPointsValuesUIntSeqControl.Sequence = new StringBuilder(K.GetStringOfUnsignedInts(pg.Value));
                    CountValueLabel.Text = pg.Value.Count.ToString();
                }
                else
                {
                    StartMomentUIntControl.UnsignedInteger = new StringBuilder(pg.StartMoment.ToString());
                    PlanetValueUIntControl.UnsignedInteger = new StringBuilder(pg.Value[0].ToString());
                }
                FromRadiusFloatControl.Float = new StringBuilder(pg.FromRadius.ToString());
                FromAngleFloatControl.Float = new StringBuilder(pg.FromAngle.ToString());
                if(ShapeComboBox.SelectedIndex == (int)K.PointGroupShape.circle)
                {
                    CircleToRadiusValueLabel.Visible = true;
                    CircleToAngleValueLabel.Visible = true;
                    ToRadiusFloatControl.Visible = false;
                    ToAngleFloatControl.Visible = false;
                    CircleToRadiusValueLabel.Text = pg.FromRadius.ToString();
                    CircleToAngleValueLabel.Text = (pg.FromAngle + 360f).ToString();
                }
                else
                {
                    CircleToRadiusValueLabel.Visible = false;
                    CircleToAngleValueLabel.Visible = false;
                    ToRadiusFloatControl.Visible = true;
                    ToAngleFloatControl.Visible = true;
                    ToRadiusFloatControl.Float = new StringBuilder(pg.ToRadius.ToString());
                    ToAngleFloatControl.Float = new StringBuilder(pg.ToAngle.ToString());
                }
                RotationFloatControl.Float = new StringBuilder(pg.RotateAngle.ToString());
                ShiftRadiusFloatControl.Float = new StringBuilder(pg.TranslateRadius.ToString());
                ShiftAngleFloatControl.Float = new StringBuilder(pg.TranslateAngle.ToString());
                VisibleCheckBox.Checked = pg.Visible;
                ColorComboBox.SelectedIndex = (int)pg.Color;
            }
        }
        public List<uint> GetValue()
        {
            if(_editingFixedPoints)
                return (K.GetUIntList(FixedPointsValuesUIntSeqControl.Sequence.ToString()));
            else
                return (K.GetUIntList(PlanetValueUIntControl.UnsignedInteger.ToString()));
        }

        /// <summary>
        /// Set by the containing control when these properties change.
        /// </summary>
        public bool EditingOutputPoints { set { _editingOutputPoints = value; } }
        public bool EditingFixedPoints { set { _editingFixedPoints = value; } }

        public bool Display { get { return VisibleCheckBox.Checked; } } // called when displaying the point group

        #endregion public functions
        #region Event Handlers
        private void VisibleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePointGroupParameters();
        }
        private void ShapeComboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if(e.KeyData == Keys.Return || e.KeyData == Keys.Enter)
                UpdatePointGroupParameters();
        }
        private void ColorComboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if(e.KeyData == Keys.Return || e.KeyData == Keys.Enter)
                UpdatePointGroupParameters();
        }
        private void ColorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DrawSamplePanel();
            if(UpdateFieldEditor != null)
                UpdateFieldEditor();
        }
        private void GraphicsControl_Paint(object sender, PaintEventArgs e)
        {
            DrawSamplePanel();
        }
        #endregion Event Handlers
        #region Properties
        /// <summary>
        /// The PointGroupParameters block should only be accessed by one event-handler at a time, so this flag
        /// is checked by every event handler prior to accessing the block. If the block is not busy, the Busy
        /// flag is set to true before the event handler begins its work. The flag is cleared again when the
        /// event handler is finished.
        /// If this flag has not been set, the function GetPointGroupParameters() throws an exception.
        /// </summary>
        public bool Busy
        {
            get { return _busy; }
            set { _busy = value; }
        }
        #endregion Properties
        #region Delegates
        /// <summary>
        /// The UpdateFieldEditor function is delegated to this control's container (the FieldEditorWindow).
        /// It is called here when any of the following events happen:
        ///     1) The "Visible" checkbox is checked or unchecked
        ///     2) The 'return' or 'enter' key is pressed while
        ///         a) the "Shape" or "Colour" combobox is active
        ///         b) one of the contained UintControl, UnsignedIntSeqControl or FloatClontrols is active
        /// This is done via this.UpdateDisplay(), which is the delegate used diectly by the contained
        /// controls in 2b.
        /// </summary>
        public delegate void PointGroupParametersChangedHandler();
        public PointGroupParametersChangedHandler UpdateFieldEditor;
        private void UpdatePointGroupParameters() // called by subordinate controls as their delegate
        {
            if(UpdateFieldEditor != null)
                UpdateFieldEditor(); // delegated to the FieldEditor (this control's container)
        }
        #endregion Delegates
        #region private functions
        private void GetShape(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if(cb.SelectedIndex == -1)
                cb.SelectedIndex = 0;
        }
        private void ShapeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(ShapeComboBox.SelectedIndex == (int)K.PointGroupShape.circle)
            {
                CircleToRadiusValueLabel.Visible = true;
                CircleToAngleValueLabel.Visible = true;
                ToRadiusFloatControl.Visible = false;
                ToAngleFloatControl.Visible = false;
            }
            else
            {
                CircleToRadiusValueLabel.Visible = false;
                CircleToAngleValueLabel.Visible = false;
                ToRadiusFloatControl.Visible = true;
                ToAngleFloatControl.Visible = true;
            }
        }
        private void GetStartMoment(object sender, EventArgs e)
        {
            UnsignedIntControl uic = sender as UnsignedIntControl;
            if(uic.UnsignedInteger.Length == 0)
                uic.UnsignedInteger = new StringBuilder("0");
        }
        private void GetFloat(object sender, EventArgs e)
        {
            FloatControl fc = sender as FloatControl;
            if(fc.Float.Length == 0)
                fc.Float = new StringBuilder("0");
            if(ShapeComboBox.SelectedIndex == (int)K.PointGroupShape.circle)
            {
                if(fc.Tag.ToString() == "FromRadiusControlTag")
                    CircleToRadiusValueLabel.Text = fc.Float.ToString();
                if(fc.Tag.ToString() == "FromAngleControlTag")
                {
                    float fval = float.Parse(fc.Float.ToString());
                    fval += 360f;
                    CircleToAngleValueLabel.Text = fval.ToString();
                }
            }
        }
        private void UIntSeqControl_Leave(object sender, EventArgs e)
        {
            string val = FixedPointsValuesUIntSeqControl.Sequence.ToString();
            string[] values = val.Split(' ', '\r', '\n');
            if(values[0].Equals(""))
                CountValueLabel.Text = "0";
            else CountValueLabel.Text = values.Length.ToString();
        }
        private string TitleText { set { PointGroupGroupBox.Text = value; } }
        private string CountLabelText { set { CountLabel.Text = value; } }
        private void SetInitialValues()
        {
            SamplePanel.Visible = false;
            ShapeComboBox.SelectedIndex = 0;
            StartMomentUIntControl.UnsignedInteger = new StringBuilder("0");
            if(_editingFixedPoints)
                FixedPointsValuesUIntSeqControl.Sequence = new StringBuilder("0");
            else PlanetValueUIntControl.UnsignedInteger = new StringBuilder("0");
            FromRadiusFloatControl.Float = new StringBuilder("0");
            FromAngleFloatControl.Float = new StringBuilder("0");
            ToRadiusFloatControl.Float = new StringBuilder("0");
            ToAngleFloatControl.Float = new StringBuilder("0");
            RotationFloatControl.Float = new StringBuilder("0");
            ShiftRadiusFloatControl.Float = new StringBuilder("0");
            ShiftAngleFloatControl.Float = new StringBuilder("0");
            //VisibleCheckBox.Checked = false; cant do this because VisibleCheckBox.Checked is being watched!
            //ColorComboBox.SelectedIndex = 0; cant do this because ColorComboBox.SelectedIndex is being watched!
        }
        /// <summary>
        /// Called when the containing control sets either the Field or Group properties.
        /// </summary>
        private void DrawSamplePanel()
        {
            if(_sg != null)
            {
                _sg.Clear(Color.White);
                float _inputDotOffset = ((_inputDotSize - 1) / 2) + 0.5f;
                float _outputDotOffset = ((_outputDotSize - 1) / 2) + 0.5f;

                float lineOriginX = 11f;  // 10f for d == 6
                float distance = 31f;  // 19f for d == 6
                float lineHeight = 8f;
                GetTheDotPen();
                GetTheLinePen();
                for(float d = 0; d < 4; d++)
                {
                    float lineX = lineOriginX + d * distance;
                    if(!_editingFixedPoints && d < 3) // connecting lines are only drawn for planets
                        _sg.DrawLine(_theLinePen, lineX, lineHeight, lineX + distance, lineHeight);
                    if(_editingOutputPoints)	// output points
                    {
                        _sg.FillEllipse(_theOutputFillBrush, lineOriginX + (d * distance) - _outputDotOffset, lineHeight - _outputDotOffset, _outputDotSize, _outputDotSize);
                        _sg.DrawEllipse(_theDotPen, lineOriginX + (d * distance) - _outputDotOffset, lineHeight - _outputDotOffset, _outputDotSize, _outputDotSize);
                    }
                    else // input points
                    {
                        _sg.FillEllipse(_theDotPen.Brush, lineOriginX + (d * distance) - _inputDotOffset, lineHeight - _inputDotOffset, _inputDotSize, _inputDotSize);
                        _sg.DrawEllipse(_theDotPen, lineOriginX + (d * distance) - _inputDotOffset, lineHeight - _inputDotOffset, _inputDotSize, _inputDotSize);
                    }
                }
            }
        }
        /// <summary>
        /// This control owns these pens.
        /// </summary>
        public Pen TheDotPen { get { return _theDotPen; } }
        public Pen TheLinePen { get { return _theLinePen; } }
        private void GetTheDotPen()
        {
            switch(ColorComboBox.SelectedIndex)
            {
                case 0: _theDotPen.Brush = Brushes.Black; break;
                case 1: _theDotPen.Brush = Brushes.Red; break;
                case 2: _theDotPen.Brush = Brushes.MediumSeaGreen; break;
                case 3: _theDotPen.Brush = Brushes.Blue; break;
                case 4: _theDotPen.Brush = Brushes.DarkOrange; break;
                case 5: _theDotPen.Brush = Brushes.DarkViolet; break;
                case 6: _theDotPen.Brush = Brushes.Magenta; break;
                default: throw new ApplicationException("Unknown point colour.");
            }
        }
        private void GetTheLinePen()
        {
            switch(ColorComboBox.SelectedIndex)
            {
                case 0: _theLinePen.Brush = Brushes.Gray; break;
                case 1: _theLinePen.Brush = Brushes.OrangeRed; break;
                case 2: _theLinePen.Brush = Brushes.MediumSeaGreen; break;
                case 3: _theLinePen.Brush = Brushes.MediumSlateBlue; break;
                case 4: _theLinePen.Brush = Brushes.Orange; break;
                case 5: _theLinePen.Brush = Brushes.Violet; break;
                case 6: _theLinePen.Brush = Brushes.HotPink; break;
                default: throw new ApplicationException("Unknown point colour.");
            }
        }
        #endregion private functions
        #region private variables
        /// <summary>
        /// The _busy flag is set to true by an external event handler (using the corresponding Property) when it
        /// begins working with the PointGroupParameters. The flag must be reset to false when the event handler
        /// is finished.
        /// </summary>
        private bool _busy;
        private Graphics _sg; // the Graphics object for the Samples Panel
        private bool _editingOutputPoints;
        private bool _editingFixedPoints;
        private Pen _theDotPen = new Pen(Brushes.Black);
        private Pen _theLinePen;
        private float _inputDotSize;
        private float _outputDotSize;
        private Brush _theOutputFillBrush;
        #endregion private variables
    }
}
