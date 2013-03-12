using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Moritz.Globals;

namespace Moritz
{
    internal partial class NewScoreDialog : Form
    {
        public NewScoreDialog()
        {
            InitializeComponent();

            M.PopulateComboBox(AlgorithmComboBox, M.Algorithms);
            _allScoreNames = M.ScoreNames(M.Preferences.LocalScoresRootFolder);
            PopulateExistingScoresListBox();
            ScoreNameTextBox.BackColor = M.TextBoxErrorColor;
            OKButton.Enabled = false;
        }

        private List<string> _allScoreNames = null;

        private void PopulateExistingScoresListBox()
        {
            ExistingScoresListBox.SuspendLayout();
            ExistingScoresListBox.Items.Clear();
            foreach(string scoreName in _allScoreNames)
            {
                ExistingScoresListBox.Items.Add(scoreName);
            }
            ExistingScoresListBox.ResumeLayout();
            ExistingScoresListBox.SetSelected(0, false);
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ScoreNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if((ScoreNameTextBox.Text.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                || (ScoreNameTextBox.Text.Length == 0)
                || _allScoreNames.Contains(ScoreNameTextBox.Text))
            {
                ScoreNameTextBox.BackColor = M.TextBoxErrorColor; ;
                OKButton.Enabled = false;
            }
            else
            {
                ScoreNameTextBox.BackColor = Color.White;
                OKButton.Enabled = true;
            }
        }
    }
}
