using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krystals4ControlLibrary;
using Krystals4ObjectLibrary;

namespace Krystals4Application
{
    internal partial class ModulationEditor : Form
    {
        /// <summary>
        /// Constructor is never used. Just added to stop VS Designer warnings!
        /// </summary>
        public ModulationEditor()
        {
            InitializeComponent();
        }

        public ModulationEditor(ModulationKrystal outputKrystal)
        {
            InitializeComponent();

            _outputKrystal = outputKrystal;
            _modulator = _outputKrystal.Modulator;
            if(string.IsNullOrEmpty(_outputKrystal.Name))
                _originalKrystalName = "";
            else
                _originalKrystalName = _outputKrystal.Name;
			_originalModulatorName = _modulator.Name;

            //_modulator.Name = "";
            //_outputKrystal.Name = "";

			if(String.IsNullOrEmpty(_originalModulatorName)) // the modulator has not been loaded from a file
				_modulatorWasOriginallyLoadedFromAFile = false;
			else
				_modulatorWasOriginallyLoadedFromAFile = true;

            //if(String.IsNullOrEmpty(_originalKrystalName)) // the krystal has not been loaded from a file
            //    _krystalWasOriginallyLoadedFromAFile = false;
            //else
            //    _krystalWasOriginallyLoadedFromAFile = true;

            _modulationTreeView = new ModulationTreeView(TreeView, _outputKrystal );

			_uintTable = new UIntTable(_modulator.XDim, _modulator.YDim,
										outputKrystal.XInputKrystal, outputKrystal.YInputKrystal);

            this.splitContainer.Panel2.Controls.Add(_uintTable);

			if(_modulatorWasOriginallyLoadedFromAFile) // the modulator has been loaded from a file
				_uintTable.IntArray = _modulator.Array;

            _uintTable.EventHandler += new UIntTable.UIntTableEventHandler(HandleUIntTableEvents);

			this.DoModulation();
            _saved = true;

			SetFormTextAndButtons();
		}
        #region private functions
		private void SetFormTextAndButtons()
		{
            #region Form.Text
			StringBuilder text = new StringBuilder();
            if(string.IsNullOrEmpty(_outputKrystal.Name))
            {
                text.Append("output krystal:  Untitled.krys");
                if(_saved == false)
                    text.Append("*");
            }
            else
            {
                text.Append("output krystal:  ");
                text.Append(_outputKrystal.Name);
                if(_saved == false)
                    text.Append("*");
            }
            //if(_krystalWasOriginallyLoadedFromAFile)
            //    text.Append(" - was " + _originalKrystalName);
                
            text.Append("      ||      ");

			if(string.IsNullOrEmpty(_modulator.Name))
			{
				text.Append("modulator:  Untitled.kmod");
                if(_saved == false)
                    text.Append("*");
            }
            else
            {
				text.Append("modulator:  ");
				text.Append(_modulator.Name);
				if(_saved == false)
					text.Append("*");
            }
            //if(_modulatorWasOriginallyLoadedFromAFile)
            //{
            //    text.Append(" - was ");
            //    text.Append(_originalModulatorName);
            //}

			this.Text = text.ToString();
			#endregion form text
			#region buttons
            if(_modulated)
            {
                this.ModulateButton.Enabled = false;
                if(_saved)
                    this.SaveButton.Enabled = false;
                else
                    this.SaveButton.Enabled = true;
            }
            else
            {
                this.ModulateButton.Enabled = true;
                this.SaveButton.Enabled = false;
            }
			#endregion buttons
		}
        #endregion private functions
		#region Events
        #region Delegates
        public delegate void ModulationEditorEventHandler(object sender, ModulationEditorEventArgs e);
        public ModulationEditorEventHandler EventHandler;
        private void HandleUIntTableEvents(object sender, UITableEventArgs e)
        {
            switch (e.Message)
            {
                case UITableMessage.Return:
					_returnKey = true;
                    DoModulation(); // sets	_modulated = true, _saved = false
					break;
                case UITableMessage.ValueChanged:
					if( ! _returnKey) // _returnKey is set in the above case
					{
						_modulated = false;
						_saved = false;
                        if(_modulator.Name == _originalModulatorName)
                        {
                            _modulator.Name = "";
                            _outputKrystal.Name = "";
                        }

						this.SetFormTextAndButtons();
						//this.Refresh();
					}
					_returnKey = false;
					break;
            }
        }
        #endregion Delegates
        #region Form
        private void ModulationEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
			if(_saved == false )
			{
				string msg = "Save the current output krystal and modulator?\r\n\r\n";
				DialogResult result = MessageBox.Show(msg, "Closing", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
				if(result == DialogResult.Yes)
				{
					try
					{
						DoModulation();
						SaveModulator();
                        SaveKrystal();
						msg = "The output krystal was saved as\r\n   " + _outputKrystal.Name + "\r\n\r\n" +
                              "The modulator was saved as\r\n   " +  _modulator.Name + "\r\n";
						MessageBox.Show(msg, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					catch(Exception ex)
					{
						string message = "Error while saving:\r\n\r\n" + ex.Message;
						MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
				}
			}
		}

        private void SplitContainer_Panel2_Resize(object sender, EventArgs e)
        {
            if (_outputKrystal == null)
                return;
            SplitterPanel c = sender as SplitterPanel;
            using (Graphics g = c.CreateGraphics())
            {
                Font font = SystemFonts.DefaultFont;
                string yInputFilename = _outputKrystal.YInputFilename;
                SizeF nameSize = g.MeasureString(yInputFilename, font);

                int xPos = (int)((c.Width - _uintTable.Width + nameSize.Width) / 2);
                if (xPos - nameSize.Width - 12 < 0)
                    xPos = (int)((c.Width - _uintTable.Width) / 2);
                int yPos = (int)((c.Height - _uintTable.Height) / 2);
                _uintTable.Location = new Point(xPos, yPos);
            }
        }
        private void SplitContainer_Panel2_Paint(object sender, PaintEventArgs e)
        {
            DrawKrystalNames(e.Graphics);
        }
        public void DrawKrystalNames(Graphics g)
        {
            Font font = SystemFonts.DefaultFont;
            System.Drawing.SolidBrush drawBrush = Brushes.Black as SolidBrush;

            #region draw y-input filename
            string yInputFilename = _outputKrystal.YInputFilename;
            SizeF nameSize = g.MeasureString(yInputFilename, font);
            float x = _uintTable.Location.X - nameSize.Width - 10;
            float y = _uintTable.Location.Y + _uintTable.ArrayCentreY - (nameSize.Height / 2);
            if (x < 2) // draw the name under the left edge of the table
            {
                x = _uintTable.Location.X;
                y = _uintTable.Location.Y + _uintTable.Height + 8;
            }
            g.DrawString(yInputFilename, font, drawBrush, x, y);
            #endregion draw y-input filename
            #region draw x-input filename
            string xInputFilename = _outputKrystal.XInputFilename;
            nameSize = g.MeasureString(xInputFilename, font);
            x = _uintTable.Location.X + _uintTable.ArrayCentreX - (nameSize.Width / 2);
            y = _uintTable.Location.Y - nameSize.Height - 8;
            g.DrawString(xInputFilename, font, drawBrush, x, y);
            #endregion draw x-input filename
       }
        #endregion Form
        #region Buttons
		private void NewButton_Click(object sender, EventArgs e)
		{
			this.Close(); // see FormClosing event handler above
			EventHandler?.Invoke(this, new ModulationEditorEventArgs(ModulationEditorMessage.New));
		}
		private void OpenButton_Click(object sender, EventArgs e)
		{
			this.Close(); // see FormClosing event handler above
			EventHandler?.Invoke(this, new ModulationEditorEventArgs(ModulationEditorMessage.Open));
		}
        private void ModulateButton_Click(object sender, EventArgs e)
        {
            DoModulation();
        }
		private void SaveButton_Click(object sender, EventArgs e)
		{
            SaveModulator();
			SaveKrystal();
            _saved = true;
            this.SetFormTextAndButtons();
		}
 		private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close(); // see FormClosing event handler above
        }

		#region Button helper functions
        /// <summary>
        /// Called when the "Save Modulator" button is pressed, or when the return key is pressed in the array
        /// </summary>
        private void DoModulation()
        {
            try
            {
				_modulator.Array = _uintTable.IntArray; // throws an exception if there is something wrong with the array
				_outputKrystal.Modulator = _modulator;
				_outputKrystal.Modulate();
                _modulationTreeView.DisplayModulationResults(_outputKrystal);
                _modulated = true; // disable the 'modulate' button
				_saved = false;

				SetFormTextAndButtons();
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Modulator error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
		private void SaveModulator()
		{
			_modulator.Array = _uintTable.IntArray;
			_modulator.Save();
		}
		private void SaveKrystal()
		{
			bool saveWithOriginalModulatorName = false;
			if(_modulatorWasOriginallyLoadedFromAFile && string.IsNullOrEmpty(_outputKrystal.Modulator.Name))
			{
				_outputKrystal.Modulator.Name = _originalModulatorName;
				saveWithOriginalModulatorName = true;
			}
			_outputKrystal.Save();
			if( saveWithOriginalModulatorName )
				_outputKrystal.Modulator.Name = "";
			this.SetFormTextAndButtons();
		}
		#endregion Button helper functions
        #endregion Buttons
        #endregion Events

		protected ModulationKrystal _outputKrystal;
		#region private variables
        private Modulator _modulator;
        private UIntTable _uintTable;
        private ModulationTreeView _modulationTreeView;
		private static string _originalKrystalName;
		private string _originalModulatorName;
		private bool _modulated = false;
		private bool _saved = false;
		//private bool _modulatorHasChanged = false;
		//private bool _outputKrystalSaved = false;
		private bool _modulatorWasOriginallyLoadedFromAFile = false;
		//private bool _krystalWasOriginallyLoadedFromAFile = false;
		private bool _returnKey = false;
		#endregion private variables
    }

    public enum ModulationEditorMessage { Open, New };
    public class ModulationEditorEventArgs : EventArgs
    {
        public ModulationEditorEventArgs(ModulationEditorMessage m)
        {
            Message = m;
        }
        public ModulationEditorMessage Message;
    }
}