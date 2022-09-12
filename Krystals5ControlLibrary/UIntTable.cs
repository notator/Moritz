using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Krystals5ControlLibrary
{
    public partial class UIntTable : UserControl
    {
        public UIntTable(int xDim, int yDim,
            //ModulationInputKrystal xInputKrystal, ModulationInputKrystal yInputKrystal)
            List<int> missingXValues, List<int> missingYValues,
            uint maxXValue, uint maxYValue)
        {
            InitializeComponent();

            //_missingXValues = xInputKrystal.MissingAbsoluteValues;
            //_missingYValues = yInputKrystal.MissingAbsoluteValues;
            _missingXValues = missingXValues;
            _missingYValues = missingYValues;

            //if((xDim < xInputKrystal.MaxValue) || (yDim < yInputKrystal.MaxValue))
            if((xDim < maxXValue) || (yDim < maxYValue))
            {
                string msg = "Table size exceeded."
                    + "\nThe maximum number of rows is " + yDim.ToString()
                    + "\nThe maximum number of columns is " + xDim.ToString();
                throw new ApplicationException(msg);
            }

            //if(xDim > xInputKrystal.MaxValue)
            //    for(int i = (int)xInputKrystal.MaxValue + 1; i <= xDim; i++)
            //        _missingXValues.Add(i);
            //if(yDim > yInputKrystal.MaxValue)
            //    for(int i = (int)yInputKrystal.MaxValue + 1; i <= yDim; i++)
            //        _missingYValues.Add(i);

            if(xDim > maxXValue)
                for(int i = (int)maxXValue + 1; i <= xDim; i++)
                    _missingXValues.Add(i);
            if(yDim > maxYValue)
                for(int i = (int)maxYValue + 1; i <= yDim; i++)
                    _missingYValues.Add(i);

            this.SuspendLayout();
            XDim = xDim;
            YDim = yDim;
            for(int y = 0; y < yDim; y++)
                for(int x = 0; x < xDim; x++)
                {
                    SimpleUIntControl uic = new SimpleUIntControl();
                    uic.Text = "1";
                    //uic.ValueHasChanged += new SimpleUIntControl.SimpleUintControlValueChanged(ValueHasChanged);
                    //uic.ReturnKeyPressed += new SimpleUIntControl.SimpleUintControlReturnKeyHandler(ReturnKeyPressed);
                    uic.EventHandler += new SimpleUIntControl.SimpleUintControlEventHandler(HandleSimpleUIntControlEvent);
                    TableLayoutPanel.Controls.Add(uic, x, y);
                }
            TableLayoutPanel.Location = new Point(_yLabelsWidth, _xLabelsHeight);
            //TableLayoutPanel.Width = _yLabelsWidth + ((_cellWidth + 1) * xDim);
            //TableLayoutPanel.Height = _xLabelsHeight + ((_cellHeight + 1) * yDim);
            this.ResumeLayout();
        }

        #region events
        #region delegates
        #region to notify the container of events which happen in the table
        public delegate void UIntTableEventHandler(object sender, UITableEventArgs e);
        public UIntTableEventHandler EventHandler;
        #endregion return key
        #endregion delegates
        /// <summary>
        /// Redraws the x- and y-labels when a Paint event happens
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            SuspendLayout();
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Font font = SystemFonts.DefaultFont;
            #region draw y-labels
            RectangleF yLabelsRect = new RectangleF(0, _xLabelsHeight, _yLabelsWidth, TableLayoutPanel.Height);
            g.FillRectangle(LabelsBackgroundBrush, yLabelsRect);
            float fontHeight = g.MeasureString("1", font).Height;
            float labelYPos = ((_cellHeight - fontHeight) / 2) + _xLabelsHeight + 1; // distance from top of control to top of first label
            g.DrawLine(Pens.Gray, new Point(0, _xLabelsHeight), new Point(_yLabelsWidth - 1, _xLabelsHeight));
            int lineYPos = (_xLabelsHeight + _cellHeight + 1); // height of next Y-Labels line separator
            int xPadding = 2; // distance from right-aligned label to first table cell

            for(int row = 0; row < YDim; row++)
            {
                if(_missingYValues.Contains(row + 1))
                {
                    g.FillRectangle(MissingValueBackgroundBrush, 0, lineYPos - _cellHeight, _yLabelsWidth, _cellHeight);
                }
                string label = (row + 1).ToString();
                int labelWidth = (int)g.MeasureString(label, font).Width;
                int xPos = _yLabelsWidth - xPadding - labelWidth;
                g.DrawString(label, font, Brushes.Black, xPos, labelYPos);
                labelYPos = labelYPos + _cellHeight + 1;
                g.DrawLine(Pens.Gray, new Point(0, lineYPos), new Point(_yLabelsWidth, lineYPos));
                lineYPos += (_cellHeight + 1);
            }
            #endregion draw y-labels
            #region draw x-labels
            RectangleF xLabelsRect = new RectangleF(_yLabelsWidth, 0, TableLayoutPanel.Width, _xLabelsHeight);
            g.FillRectangle(Brushes.Gainsboro, xLabelsRect);
            int fontWidth = (int)g.MeasureString("1", font).Width;
            int lineXPos = _yLabelsWidth; // position of first X-Labels line separator
            labelYPos = _xLabelsHeight - 1 - fontHeight; // ((_cellHeight - fontHeight) / 2); 
            g.DrawLine(Pens.Gray, new Point(_yLabelsWidth, 0), new Point(_yLabelsWidth, _xLabelsHeight));
            for(int column = 0; column < XDim; column++)
            {
                if(_missingXValues.Contains(column + 1))
                {
                    g.FillRectangle(MissingValueBackgroundBrush, lineXPos + 1, 0, _cellWidth, _xLabelsHeight);
                }
                string label = (column + 1).ToString();
                int labelWidth = (int)g.MeasureString(label, font).Width;
                lineXPos += (_cellWidth + 1);
                int labelXPos = lineXPos - xPadding - labelWidth;
                g.DrawString(label, font, Brushes.Black, labelXPos, labelYPos);
                g.DrawLine(Pens.Gray, new Point(lineXPos, 0), new Point(lineXPos, _xLabelsHeight));
            }
            #endregion draw x-labels
            ResumeLayout();
        }
        #endregion events
        #region private functions
        private void HandleSimpleUIntControlEvent(object sender, SUICEventArgs e)
        {
            Control UIntControl = sender as Control;
            TableLayoutPanelCellPosition pos = TableLayoutPanel.GetCellPosition(UIntControl);
            if(EventHandler != null)
            {
                switch(e.Message)
                {
                    case SUICMessage.Return:
                        EventHandler(this, new UITableEventArgs(UITableMessage.Return));
                        break;
                    case SUICMessage.ValueChanged:
                        EventHandler(this, new UITableEventArgs(UITableMessage.ValueChanged));
                        break;
                    case SUICMessage.Left:
                        if(pos.Column > 0)
                            pos.Column--;
                        else if(pos.Column == 0 && pos.Row > 0)
                        {
                            pos.Column = TableLayoutPanel.ColumnCount - 1;
                            pos.Row--;
                        }
                        TableLayoutPanel.GetControlFromPosition(pos.Column, pos.Row).Select();
                        break;
                    case SUICMessage.Right:
                        if(pos.Column < TableLayoutPanel.ColumnCount - 1)
                            pos.Column++;
                        else if(pos.Column == TableLayoutPanel.ColumnCount - 1 && pos.Row < TableLayoutPanel.RowCount - 1)
                        {
                            pos.Column = 0;
                            pos.Row++;
                        }
                        TableLayoutPanel.GetControlFromPosition(pos.Column, pos.Row).Select();
                        break;
                    case SUICMessage.Up:
                        if(pos.Row > 0)
                            pos.Row--;
                        TableLayoutPanel.GetControlFromPosition(pos.Column, pos.Row).Select();
                        break;
                    case SUICMessage.Down:
                        if(pos.Row < TableLayoutPanel.RowCount - 1) pos.Row++;
                        TableLayoutPanel.GetControlFromPosition(pos.Column, pos.Row).Select();
                        break;
                    case SUICMessage.Pos1:
                        TableLayoutPanel.GetControlFromPosition(0, 0).Select();
                        break;
                }
            }
        }
        #endregion private functions
        #region properties
        public int[,] IntArray
        {
            get
            {
                int[,] returnArray = new int[XDim, YDim];
                for(int y = 0; y < YDim; y++)
                    for(int x = 0; x < XDim; x++)
                    {
                        SimpleUIntControl c = TableLayoutPanel.GetControlFromPosition(x, y) as SimpleUIntControl;
                        if(c.ValueString.Length == 0)
                            throw new ApplicationException(String.Format("Cell not set at [ {0}, {1} ]", (x + 1), (y + 1)));
                        returnArray[x, y] = int.Parse(c.ValueString); // an empty string parses as zero
                    }
                return returnArray;
            }
            set
            {
                if(value.Length != XDim * YDim)
                    throw new ApplicationException("Attempt to allocate an array of the wrong size to the table of unsigned integers.");
                for(int y = 0; y < YDim; y++)
                    for(int x = 0; x < XDim; x++)
                    {
                        SimpleUIntControl c = TableLayoutPanel.GetControlFromPosition(x, y) as SimpleUIntControl;
                        if(value[x, y] < 0)
                            throw new ApplicationException("Attempt to set a negative value in the table of unsigned integers.");
                        c.ValueString = value[x, y].ToString();
                    }
            }
        }
        public int MaxY
        {
            get { return ((_maxPixelHeight - _xLabelsHeight) / _cellHeight); }
        }
        public int MaxX
        {
            get { return ((_maxPixelWidth - _yLabelsWidth) / _cellWidth); }
        }
        public int YDim
        {
            get { return TableLayoutPanel.RowCount; }
            set
            {
                TableLayoutPanel.RowStyles.Clear();
                TableLayoutPanel.RowCount = value;
                for(int i = 0; i < value; i++)
                    this.TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, _cellHeight));
            }
        }
        public int XDim
        {
            get { return TableLayoutPanel.ColumnCount; }
            set
            {
                TableLayoutPanel.ColumnStyles.Clear();
                TableLayoutPanel.ColumnCount = value;
                for(int i = 0; i < value; i++)
                    this.TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, _cellWidth));
            }
        }
        public new int Width
        {
            get { return (TableLayoutPanel.Width + _yLabelsWidth); }
        }
        public new int Height
        {
            get { return (TableLayoutPanel.Height + _xLabelsHeight); }
        }
        public float ArrayCentreX
        {
            get { return _yLabelsWidth + TableLayoutPanel.Width / 2; }
        }
        public float ArrayCentreY
        {
            get { return _xLabelsHeight + TableLayoutPanel.Height / 2; }
        }
        #endregion properties
        #region private variables
        // with these values, MaxRows is 35, MaxColumns is 30
        private readonly int _maxPixelHeight = 650;
        private readonly int _maxPixelWidth = 860;
        private readonly int _xLabelsHeight = 18;
        private readonly int _yLabelsWidth = 20;
        private readonly int _cellHeight = 18;
        private readonly int _cellWidth = 28;
        private readonly Brush LabelsBackgroundBrush = Brushes.Gainsboro;
        private readonly Brush MissingValueBackgroundBrush = Brushes.Pink;
        private List<int> _missingXValues = new List<int>();
        private List<int> _missingYValues = new List<int>();
        #endregion private variables
    }
    public enum UITableMessage { ValueChanged, Return };
    public class UITableEventArgs : EventArgs
    {
        public UITableEventArgs(UITableMessage m)
        {
            Message = m;
        }
        public UITableMessage Message;
    }

}
