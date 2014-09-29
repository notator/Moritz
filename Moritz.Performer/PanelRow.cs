using System;
using System.Drawing;
using System.Windows.Forms;

namespace Moritz.Performer
{
    internal delegate void SelectedChanged(PanelRow row);
    /// <summary>
    /// A component of the PerformanceOptionsDialog
    /// </summary>
    internal class PanelRow : Control
    {
        public PanelRow(Point location, string channel, string instrument, int maxInstrumentWidth, SelectedChanged selectedChanged)
        {
            SelectedChanged = selectedChanged;

            channelLabel.Text = channel;
            instrumentLabel.Text = instrument;
            performerLabel.Text = "";

            channelLabel.TextAlign = ContentAlignment.MiddleCenter;
            instrumentLabel.TextAlign = ContentAlignment.MiddleLeft;
            performerLabel.TextAlign = ContentAlignment.MiddleLeft;

            SetWidthForInstrumentWidth(maxInstrumentWidth);

            channelLabel.Click += new EventHandler(LinkControl_Click);
            performerLabel.Click += new EventHandler(LinkControl_Click);
            instrumentLabel.Click += new EventHandler(LinkControl_Click);

            this.Location = location;
            this.Size = new Size(performerLabel.Location.X + performerLabel.Size.Width, Height);

            SuspendLayout();
            this.Controls.Add(channelLabel);
            this.Controls.Add(instrumentLabel);
            this.Controls.Add(performerLabel);
            ResumeLayout();
        }

        public void LinkControl_Click(object sender, EventArgs e)
        {
            if(_selected)
            {
                SetSelected(false);
            }
            else
            {
                SetSelected(true);
            }
            if(SelectedChanged != null)
                SelectedChanged(this); // call delegate in Panel
        }

        public MoritzPlayer Performer
        {
            set
            {
                switch(value)
                {
                    case MoritzPlayer.None:
                        performerLabel.Text = "Silent";
                        performerLabel.ForeColor = Color.Red;
                        channelLabel.ForeColor = Color.Red;
                        instrumentLabel.ForeColor = Color.Red;
                        break;
                    case MoritzPlayer.Assistant:
                        performerLabel.Text = "Assistant";
                        performerLabel.ForeColor = Color.Black;
                        channelLabel.ForeColor = Color.Black;
                        instrumentLabel.ForeColor = Color.Black;
                        break;
                    case MoritzPlayer.LivePerformer:
                        performerLabel.Text = "Performer";
                        performerLabel.ForeColor = Color.Blue;
                        channelLabel.ForeColor = Color.Blue;
                        instrumentLabel.ForeColor = Color.Blue;
                        break;
                }
            }
            get
            {
                MoritzPlayer performer = MoritzPlayer.Assistant;
                switch(performerLabel.Text)
                {
                    case "Silent":
                        performer = MoritzPlayer.None;
                        break;
                    case "Assistant":
                        performer = MoritzPlayer.Assistant;
                        break;
                    case "Performer":
                        performer = MoritzPlayer.LivePerformer;
                        break;
                }
                return performer;
            }
        }
        public void SetWidthForInstrumentWidth(int instrumentWidth)
        {
            channelLabel.Size = new Size(20, Height);
            instrumentLabel.Size = new Size(instrumentWidth + 1, Height);
            performerLabel.Size = new Size(57, Height);

            channelLabel.Location = new Point(0, 0);
            instrumentLabel.Location = new Point(channelLabel.Size.Width, 0);
            performerLabel.Location = new Point(channelLabel.Size.Width + instrumentLabel.Size.Width, 0);

            this.Size = new Size(performerLabel.Location.X + performerLabel.Width, Height);
        }
        public string InstrumentName { get { return instrumentLabel.Text; } }

        private void SetSelected(bool selected)
        {
            if(selected)
            {
                _selected = true;
                channelLabel.BackColor = _selectedColor;
                instrumentLabel.BackColor = _selectedColor;
                performerLabel.BackColor = _selectedColor;
            }
            else
            {
                _selected = false;
                channelLabel.BackColor = Color.White;
                instrumentLabel.BackColor = Color.White;
                performerLabel.BackColor = Color.White;
            }

        }

        public bool IsSelected
        {
            get
            {
                return _selected;
            }
            set
            {
                if(value == false)
                    SetSelected(false);
                else
                    SetSelected(true);
            }
        }
        private SelectedChanged SelectedChanged = null;
        public new static readonly int Height = 23;
        private Label channelLabel = new Label();
        private Label instrumentLabel = new Label();
        private Label performerLabel = new Label();
        private bool _selected = false;
        private Color _selectedColor = Color.FromArgb(210, 255, 210);

    }

}