namespace Krystals5ControlLibrary
{
    partial class PointGroupParameters
    {
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
            if (disposing)
            {
                _theDotPen.Dispose();
                _theLinePen.Dispose();

                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PointGroupParameters));
            this.ShapeComboBox = new System.Windows.Forms.ComboBox();
            this.CountLabel = new System.Windows.Forms.Label();
            this.RotationLabel = new System.Windows.Forms.Label();
            this.FromRadiusLabel = new System.Windows.Forms.Label();
            this.ToAngleLabel = new System.Windows.Forms.Label();
            this.ShiftAngleLabel = new System.Windows.Forms.Label();
            this.PointGroupGroupBox = new System.Windows.Forms.GroupBox();
            this.PlanetValueLabel = new System.Windows.Forms.Label();
            this.SamplePanel = new System.Windows.Forms.Panel();
            this.ColorComboBox = new System.Windows.Forms.ComboBox();
            this.VisibleCheckBox = new System.Windows.Forms.CheckBox();
            this.ToRadiusLabel = new System.Windows.Forms.Label();
            this.ShiftRadiusLabel = new System.Windows.Forms.Label();
            this.FromAngleLabel = new System.Windows.Forms.Label();
            this.CircleToAngleValueLabel = new System.Windows.Forms.Label();
            this.CircleToRadiusValueLabel = new System.Windows.Forms.Label();
            this.CountValueLabel = new System.Windows.Forms.Label();
            this.FixedPointsValuesLabel = new System.Windows.Forms.Label();
            this.ShapeLabel = new System.Windows.Forms.Label();
            this.PlanetValueUIntControl = new Krystals5ControlLibrary.UnsignedIntControl();
            this.FixedPointsValuesUIntSeqControl = new Krystals5ControlLibrary.UnsignedIntSeqControl();
            this.ShiftAngleFloatControl = new Krystals5ControlLibrary.FloatControl();
            this.ShiftRadiusFloatControl = new Krystals5ControlLibrary.FloatControl();
            this.RotationFloatControl = new Krystals5ControlLibrary.FloatControl();
            this.ToAngleFloatControl = new Krystals5ControlLibrary.FloatControl();
            this.ToRadiusFloatControl = new Krystals5ControlLibrary.FloatControl();
            this.FromRadiusFloatControl = new Krystals5ControlLibrary.FloatControl();
            this.FromAngleFloatControl = new Krystals5ControlLibrary.FloatControl();
            this.StartMomentUIntControl = new Krystals5ControlLibrary.UnsignedIntControl();
            this.PointGroupGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ShapeComboBox
            // 
            this.ShapeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ShapeComboBox.FormattingEnabled = true;
            this.ShapeComboBox.Items.AddRange(new object[] {
            "Circle",
            "Spiral",
            "Straight Line"});
            this.ShapeComboBox.Location = new System.Drawing.Point(46, 19);
            this.ShapeComboBox.Name = "ShapeComboBox";
            this.ShapeComboBox.Size = new System.Drawing.Size(82, 21);
            this.ShapeComboBox.TabIndex = 0;
            this.ShapeComboBox.Tag = "";
            this.ShapeComboBox.Leave += new System.EventHandler(this.GetShape);
            this.ShapeComboBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.ShapeComboBox_PreviewKeyDown);
            this.ShapeComboBox.SelectedIndexChanged += new System.EventHandler(this.ShapeComboBox_SelectedIndexChanged);
            // 
            // CountLabel
            // 
            this.CountLabel.AutoSize = true;
            this.CountLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CountLabel.Location = new System.Drawing.Point(8, 71);
            this.CountLabel.Name = "CountLabel";
            this.CountLabel.Size = new System.Drawing.Size(73, 13);
            this.CountLabel.TabIndex = 38;
            this.CountLabel.Text = "Start Moment:";
            this.CountLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // RotationLabel
            // 
            this.RotationLabel.AutoSize = true;
            this.RotationLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RotationLabel.Location = new System.Drawing.Point(31, 200);
            this.RotationLabel.Name = "RotationLabel";
            this.RotationLabel.Size = new System.Drawing.Size(50, 13);
            this.RotationLabel.TabIndex = 49;
            this.RotationLabel.Text = "Rotation:";
            this.RotationLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // FromRadiusLabel
            // 
            this.FromRadiusLabel.AutoSize = true;
            this.FromRadiusLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.FromRadiusLabel.Location = new System.Drawing.Point(12, 98);
            this.FromRadiusLabel.Name = "FromRadiusLabel";
            this.FromRadiusLabel.Size = new System.Drawing.Size(69, 13);
            this.FromRadiusLabel.TabIndex = 35;
            this.FromRadiusLabel.Text = "From Radius:";
            this.FromRadiusLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ToAngleLabel
            // 
            this.ToAngleLabel.AutoSize = true;
            this.ToAngleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ToAngleLabel.Location = new System.Drawing.Point(28, 173);
            this.ToAngleLabel.Name = "ToAngleLabel";
            this.ToAngleLabel.Size = new System.Drawing.Size(53, 13);
            this.ToAngleLabel.TabIndex = 40;
            this.ToAngleLabel.Text = "To Angle:";
            this.ToAngleLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ShiftAngleLabel
            // 
            this.ShiftAngleLabel.AutoSize = true;
            this.ShiftAngleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ShiftAngleLabel.Location = new System.Drawing.Point(20, 250);
            this.ShiftAngleLabel.Name = "ShiftAngleLabel";
            this.ShiftAngleLabel.Size = new System.Drawing.Size(61, 13);
            this.ShiftAngleLabel.TabIndex = 46;
            this.ShiftAngleLabel.Text = "Shift Angle:";
            this.ShiftAngleLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // PointGroupGroupBox
            // 
            this.PointGroupGroupBox.Controls.Add(this.PlanetValueLabel);
            this.PointGroupGroupBox.Controls.Add(this.SamplePanel);
            this.PointGroupGroupBox.Controls.Add(this.ColorComboBox);
            this.PointGroupGroupBox.Controls.Add(this.VisibleCheckBox);
            this.PointGroupGroupBox.Controls.Add(this.PlanetValueUIntControl);
            this.PointGroupGroupBox.Controls.Add(this.FixedPointsValuesUIntSeqControl);
            this.PointGroupGroupBox.Controls.Add(this.ShiftAngleFloatControl);
            this.PointGroupGroupBox.Controls.Add(this.ShiftRadiusFloatControl);
            this.PointGroupGroupBox.Controls.Add(this.RotationFloatControl);
            this.PointGroupGroupBox.Controls.Add(this.ToAngleFloatControl);
            this.PointGroupGroupBox.Controls.Add(this.ToRadiusFloatControl);
            this.PointGroupGroupBox.Controls.Add(this.FromRadiusFloatControl);
            this.PointGroupGroupBox.Controls.Add(this.FromAngleFloatControl);
            this.PointGroupGroupBox.Controls.Add(this.StartMomentUIntControl);
            this.PointGroupGroupBox.Controls.Add(this.ShapeComboBox);
            this.PointGroupGroupBox.Controls.Add(this.CountLabel);
            this.PointGroupGroupBox.Controls.Add(this.RotationLabel);
            this.PointGroupGroupBox.Controls.Add(this.FromRadiusLabel);
            this.PointGroupGroupBox.Controls.Add(this.ToAngleLabel);
            this.PointGroupGroupBox.Controls.Add(this.ToRadiusLabel);
            this.PointGroupGroupBox.Controls.Add(this.ShiftAngleLabel);
            this.PointGroupGroupBox.Controls.Add(this.ShiftRadiusLabel);
            this.PointGroupGroupBox.Controls.Add(this.FromAngleLabel);
            this.PointGroupGroupBox.Controls.Add(this.CircleToAngleValueLabel);
            this.PointGroupGroupBox.Controls.Add(this.CircleToRadiusValueLabel);
            this.PointGroupGroupBox.Controls.Add(this.CountValueLabel);
            this.PointGroupGroupBox.Controls.Add(this.FixedPointsValuesLabel);
            this.PointGroupGroupBox.Controls.Add(this.ShapeLabel);
            this.PointGroupGroupBox.Location = new System.Drawing.Point(0, 0);
            this.PointGroupGroupBox.Name = "PointGroupGroupBox";
            this.PointGroupGroupBox.Size = new System.Drawing.Size(138, 352);
            this.PointGroupGroupBox.TabIndex = 79;
            this.PointGroupGroupBox.TabStop = false;
            this.PointGroupGroupBox.Text = "Fixed Field Points";
            // 
            // PlanetValueLabel
            // 
            this.PlanetValueLabel.AutoSize = true;
            this.PlanetValueLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.PlanetValueLabel.Location = new System.Drawing.Point(11, 48);
            this.PlanetValueLabel.Name = "PlanetValueLabel";
            this.PlanetValueLabel.Size = new System.Drawing.Size(70, 13);
            this.PlanetValueLabel.TabIndex = 56;
            this.PlanetValueLabel.Text = "Planet Value:";
            this.PlanetValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.PlanetValueLabel.Visible = false;
            // 
            // SamplePanel
            // 
            this.SamplePanel.BackColor = System.Drawing.SystemColors.Window;
            this.SamplePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SamplePanel.Location = new System.Drawing.Point(10, 296);
            this.SamplePanel.Name = "SamplePanel";
            this.SamplePanel.Size = new System.Drawing.Size(118, 19);
            this.SamplePanel.TabIndex = 58;
            this.SamplePanel.Tag = "";
            this.SamplePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.GraphicsControl_Paint);
            // 
            // ColorComboBox
            // 
            this.ColorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ColorComboBox.FormattingEnabled = true;
            this.ColorComboBox.Items.AddRange(new object[] {
            "Black",
            "Red",
            "Green",
            "Blue",
            "Orange",
            "Purple",
            "Magenta"});
            this.ColorComboBox.Location = new System.Drawing.Point(36, 322);
            this.ColorComboBox.MaxDropDownItems = 25;
            this.ColorComboBox.Name = "ColorComboBox";
            this.ColorComboBox.Size = new System.Drawing.Size(66, 21);
            this.ColorComboBox.TabIndex = 11;
            this.ColorComboBox.Tag = "";
            this.ColorComboBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.ColorComboBox_PreviewKeyDown);
            this.ColorComboBox.SelectedIndexChanged += new System.EventHandler(this.ColorComboBox_SelectedIndexChanged);
            // 
            // VisibleCheckBox
            // 
            this.VisibleCheckBox.AutoSize = true;
            this.VisibleCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.VisibleCheckBox.Checked = true;
            this.VisibleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.VisibleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.VisibleCheckBox.Location = new System.Drawing.Point(32, 273);
            this.VisibleCheckBox.Name = "VisibleCheckBox";
            this.VisibleCheckBox.Size = new System.Drawing.Size(65, 18);
            this.VisibleCheckBox.TabIndex = 11;
            this.VisibleCheckBox.TabStop = false;
            this.VisibleCheckBox.Text = "Visible:";
            this.VisibleCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.VisibleCheckBox.CheckedChanged += new System.EventHandler(this.VisibleCheckBox_CheckedChanged);
            // 
            // ToRadiusLabel
            // 
            this.ToRadiusLabel.AutoSize = true;
            this.ToRadiusLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ToRadiusLabel.Location = new System.Drawing.Point(22, 149);
            this.ToRadiusLabel.Name = "ToRadiusLabel";
            this.ToRadiusLabel.Size = new System.Drawing.Size(59, 13);
            this.ToRadiusLabel.TabIndex = 36;
            this.ToRadiusLabel.Text = "To Radius:";
            this.ToRadiusLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ShiftRadiusLabel
            // 
            this.ShiftRadiusLabel.AutoSize = true;
            this.ShiftRadiusLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ShiftRadiusLabel.Location = new System.Drawing.Point(14, 226);
            this.ShiftRadiusLabel.Name = "ShiftRadiusLabel";
            this.ShiftRadiusLabel.Size = new System.Drawing.Size(67, 13);
            this.ShiftRadiusLabel.TabIndex = 45;
            this.ShiftRadiusLabel.Text = "Shift Radius:";
            this.ShiftRadiusLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // FromAngleLabel
            // 
            this.FromAngleLabel.AutoSize = true;
            this.FromAngleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.FromAngleLabel.Location = new System.Drawing.Point(18, 122);
            this.FromAngleLabel.Name = "FromAngleLabel";
            this.FromAngleLabel.Size = new System.Drawing.Size(63, 13);
            this.FromAngleLabel.TabIndex = 39;
            this.FromAngleLabel.Text = "From Angle:";
            this.FromAngleLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // CircleToAngleValueLabel
            // 
            this.CircleToAngleValueLabel.AutoSize = true;
            this.CircleToAngleValueLabel.Location = new System.Drawing.Point(84, 173);
            this.CircleToAngleValueLabel.Name = "CircleToAngleValueLabel";
            this.CircleToAngleValueLabel.Size = new System.Drawing.Size(13, 13);
            this.CircleToAngleValueLabel.TabIndex = 61;
            this.CircleToAngleValueLabel.Text = "0";
            this.CircleToAngleValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // CircleToRadiusValueLabel
            // 
            this.CircleToRadiusValueLabel.AutoSize = true;
            this.CircleToRadiusValueLabel.Location = new System.Drawing.Point(84, 149);
            this.CircleToRadiusValueLabel.Name = "CircleToRadiusValueLabel";
            this.CircleToRadiusValueLabel.Size = new System.Drawing.Size(13, 13);
            this.CircleToRadiusValueLabel.TabIndex = 60;
            this.CircleToRadiusValueLabel.Text = "0";
            this.CircleToRadiusValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // CountValueLabel
            // 
            this.CountValueLabel.AutoSize = true;
            this.CountValueLabel.Location = new System.Drawing.Point(84, 71);
            this.CountValueLabel.Name = "CountValueLabel";
            this.CountValueLabel.Size = new System.Drawing.Size(13, 13);
            this.CountValueLabel.TabIndex = 59;
            this.CountValueLabel.Text = "0";
            this.CountValueLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // FixedPointsValuesLabel
            // 
            this.FixedPointsValuesLabel.AutoSize = true;
            this.FixedPointsValuesLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.FixedPointsValuesLabel.Location = new System.Drawing.Point(2, 48);
            this.FixedPointsValuesLabel.Name = "FixedPointsValuesLabel";
            this.FixedPointsValuesLabel.Size = new System.Drawing.Size(42, 13);
            this.FixedPointsValuesLabel.TabIndex = 52;
            this.FixedPointsValuesLabel.Text = "Values:";
            this.FixedPointsValuesLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ShapeLabel
            // 
            this.ShapeLabel.AutoSize = true;
            this.ShapeLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ShapeLabel.Location = new System.Drawing.Point(3, 22);
            this.ShapeLabel.Name = "ShapeLabel";
            this.ShapeLabel.Size = new System.Drawing.Size(41, 13);
            this.ShapeLabel.TabIndex = 50;
            this.ShapeLabel.Text = "Shape:";
            this.ShapeLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // PlanetValueUIntControl
            // 
            this.PlanetValueUIntControl.Location = new System.Drawing.Point(84, 44);
            this.PlanetValueUIntControl.Name = "PlanetValueUIntControl";
            this.PlanetValueUIntControl.Size = new System.Drawing.Size(44, 20);
            this.PlanetValueUIntControl.TabIndex = 1;
            this.PlanetValueUIntControl.UnsignedInteger = ((System.Text.StringBuilder)(resources.GetObject("PlanetValueUIntControl.UnsignedInteger")));
            this.PlanetValueUIntControl.Visible = false;
            // 
            // FixedPointsValuesUIntSeqControl
            // 
            this.FixedPointsValuesUIntSeqControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.FixedPointsValuesUIntSeqControl.BackColor = System.Drawing.SystemColors.Window;
            this.FixedPointsValuesUIntSeqControl.Location = new System.Drawing.Point(46, 44);
            this.FixedPointsValuesUIntSeqControl.Name = "FixedPointsValuesUIntSeqControl";
            this.FixedPointsValuesUIntSeqControl.Sequence = ((System.Text.StringBuilder)(resources.GetObject("FixedPointsValuesUIntSeqControl.Sequence")));
            this.FixedPointsValuesUIntSeqControl.Size = new System.Drawing.Size(82, 20);
            this.FixedPointsValuesUIntSeqControl.TabIndex = 2;
            this.FixedPointsValuesUIntSeqControl.Leave += new System.EventHandler(this.UIntSeqControl_Leave);
            // 
            // ShiftAngleFloatControl
            // 
            this.ShiftAngleFloatControl.Float = ((System.Text.StringBuilder)(resources.GetObject("ShiftAngleFloatControl.Float")));
            this.ShiftAngleFloatControl.Location = new System.Drawing.Point(84, 247);
            this.ShiftAngleFloatControl.Name = "ShiftAngleFloatControl";
            this.ShiftAngleFloatControl.Size = new System.Drawing.Size(44, 20);
            this.ShiftAngleFloatControl.TabIndex = 10;
            this.ShiftAngleFloatControl.Tag = "";
            this.ShiftAngleFloatControl.Leave += new System.EventHandler(this.GetFloat);
            // 
            // ShiftRadiusFloatControl
            // 
            this.ShiftRadiusFloatControl.Float = ((System.Text.StringBuilder)(resources.GetObject("ShiftRadiusFloatControl.Float")));
            this.ShiftRadiusFloatControl.Location = new System.Drawing.Point(84, 223);
            this.ShiftRadiusFloatControl.Name = "ShiftRadiusFloatControl";
            this.ShiftRadiusFloatControl.Size = new System.Drawing.Size(44, 20);
            this.ShiftRadiusFloatControl.TabIndex = 9;
            this.ShiftRadiusFloatControl.Tag = "";
            this.ShiftRadiusFloatControl.Leave += new System.EventHandler(this.GetFloat);
            // 
            // RotationFloatControl
            // 
            this.RotationFloatControl.Float = ((System.Text.StringBuilder)(resources.GetObject("RotationFloatControl.Float")));
            this.RotationFloatControl.Location = new System.Drawing.Point(84, 197);
            this.RotationFloatControl.Name = "RotationFloatControl";
            this.RotationFloatControl.Size = new System.Drawing.Size(44, 20);
            this.RotationFloatControl.TabIndex = 8;
            this.RotationFloatControl.Tag = "";
            this.RotationFloatControl.Leave += new System.EventHandler(this.GetFloat);
            // 
            // ToAngleFloatControl
            // 
            this.ToAngleFloatControl.Float = ((System.Text.StringBuilder)(resources.GetObject("ToAngleFloatControl.Float")));
            this.ToAngleFloatControl.Location = new System.Drawing.Point(84, 170);
            this.ToAngleFloatControl.Name = "ToAngleFloatControl";
            this.ToAngleFloatControl.Size = new System.Drawing.Size(44, 20);
            this.ToAngleFloatControl.TabIndex = 7;
            this.ToAngleFloatControl.Tag = "";
            this.ToAngleFloatControl.Visible = false;
            this.ToAngleFloatControl.Leave += new System.EventHandler(this.GetFloat);
            // 
            // ToRadiusFloatControl
            // 
            this.ToRadiusFloatControl.Float = ((System.Text.StringBuilder)(resources.GetObject("ToRadiusFloatControl.Float")));
            this.ToRadiusFloatControl.Location = new System.Drawing.Point(84, 146);
            this.ToRadiusFloatControl.Name = "ToRadiusFloatControl";
            this.ToRadiusFloatControl.Size = new System.Drawing.Size(44, 20);
            this.ToRadiusFloatControl.TabIndex = 6;
            this.ToRadiusFloatControl.Tag = "";
            this.ToRadiusFloatControl.Visible = false;
            this.ToRadiusFloatControl.Leave += new System.EventHandler(this.GetFloat);
            // 
            // FromRadiusFloatControl
            // 
            this.FromRadiusFloatControl.Float = ((System.Text.StringBuilder)(resources.GetObject("FromRadiusFloatControl.Float")));
            this.FromRadiusFloatControl.Location = new System.Drawing.Point(84, 95);
            this.FromRadiusFloatControl.Name = "FromRadiusFloatControl";
            this.FromRadiusFloatControl.Size = new System.Drawing.Size(44, 20);
            this.FromRadiusFloatControl.TabIndex = 4;
            this.FromRadiusFloatControl.Tag = "FromRadiusControlTag";
            this.FromRadiusFloatControl.Leave += new System.EventHandler(this.GetFloat);
            // 
            // FromAngleFloatControl
            // 
            this.FromAngleFloatControl.Float = ((System.Text.StringBuilder)(resources.GetObject("FromAngleFloatControl.Float")));
            this.FromAngleFloatControl.Location = new System.Drawing.Point(84, 119);
            this.FromAngleFloatControl.Name = "FromAngleFloatControl";
            this.FromAngleFloatControl.Size = new System.Drawing.Size(44, 20);
            this.FromAngleFloatControl.TabIndex = 5;
            this.FromAngleFloatControl.Tag = "FromAngleControlTag";
            this.FromAngleFloatControl.Leave += new System.EventHandler(this.GetFloat);
            // 
            // StartMomentUIntControl
            // 
            this.StartMomentUIntControl.Location = new System.Drawing.Point(84, 68);
            this.StartMomentUIntControl.Name = "StartMomentUIntControl";
            this.StartMomentUIntControl.Size = new System.Drawing.Size(44, 20);
            this.StartMomentUIntControl.TabIndex = 3;
            this.StartMomentUIntControl.UnsignedInteger = ((System.Text.StringBuilder)(resources.GetObject("StartMomentUIntControl.UnsignedInteger")));
            this.StartMomentUIntControl.Leave += new System.EventHandler(this.GetStartMoment);
            // 
            // PointGroupParameters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PointGroupGroupBox);
            this.Name = "PointGroupParameters";
            this.Size = new System.Drawing.Size(138, 352);
            this.PointGroupGroupBox.ResumeLayout(false);
            this.PointGroupGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox ShapeComboBox;
        private System.Windows.Forms.Label CountLabel;
        private System.Windows.Forms.Label RotationLabel;
        private System.Windows.Forms.Label FromRadiusLabel;
        private System.Windows.Forms.Label ToAngleLabel;
        private System.Windows.Forms.Label ShiftAngleLabel;
        private System.Windows.Forms.GroupBox PointGroupGroupBox;
        private System.Windows.Forms.Label ToRadiusLabel;
        private System.Windows.Forms.Label ShiftRadiusLabel;
        private System.Windows.Forms.Label FromAngleLabel;
		private System.Windows.Forms.Label ShapeLabel;
		private System.Windows.Forms.Label FixedPointsValuesLabel;
		private System.Windows.Forms.Label PlanetValueLabel;
		private System.Windows.Forms.CheckBox VisibleCheckBox;
		private System.Windows.Forms.Panel SamplePanel;
		private System.Windows.Forms.ComboBox ColorComboBox;
		private System.Windows.Forms.Label CountValueLabel;
		private System.Windows.Forms.Label CircleToRadiusValueLabel;
		private UnsignedIntControl PlanetValueUIntControl;
		private UnsignedIntSeqControl FixedPointsValuesUIntSeqControl;
		private FloatControl ShiftAngleFloatControl;
		private FloatControl ShiftRadiusFloatControl;
		private FloatControl RotationFloatControl;
		private FloatControl ToAngleFloatControl;
		private FloatControl ToRadiusFloatControl;
		private FloatControl FromRadiusFloatControl;
		private FloatControl FromAngleFloatControl;
		private UnsignedIntControl StartMomentUIntControl;
		private System.Windows.Forms.Label CircleToAngleValueLabel;
    }
}
