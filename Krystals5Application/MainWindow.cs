using Krystals5ObjectLibrary;

using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Krystals5Application
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
            this.BringToFront();
        }

        #region New Krystals
        private void NewConstantKrystalButton_Click(object sender, EventArgs e)
        {
            using(NewConstantKrystalDialog dlg = new NewConstantKrystalDialog())
            {
                DialogResult result = dlg.ShowDialog();
                if(result == DialogResult.OK)
                {
                    String constantValueStr = dlg.ConstantKrystalValue;
                    if(constantValueStr.Length > 0)
                    {
                        uint constantValue = uint.Parse(constantValueStr);
                        ConstantKrystal constantKrystal = new ConstantKrystal(constantValue);
                        var hasBeenSaved = constantKrystal.Save();
                        if(hasBeenSaved)
                        {
                            KrystalsBrowser kb = new KrystalsBrowser("New Krystal", constantKrystal, null);
                            kb.Show();
                        }
                    }
                }
            }
        }
        private void NewLineKrystalButton_Click(object sender, EventArgs e)
        {
            using(NewLineKrystalDialog dlg = new NewLineKrystalDialog())
            {
                DialogResult result = dlg.ShowDialog();
                if(result == DialogResult.OK)
                {
                    List<uint> lineValue = M.StringToUIntList(dlg.LineKrystalValue, ' ');
                    if(lineValue.Count > 0)
                    {
                        LineKrystal lineKrystal = new LineKrystal(lineValue);
                        var hasBeenSaved = lineKrystal.Save();
                        if(hasBeenSaved)
                        {
                            KrystalsBrowser kb = new KrystalsBrowser("New Krystal", lineKrystal, null);
                            kb.Show();
                        }
                    }
                }
            }
        }
        private void NewExpansionKrystalButton_Click(object sender, EventArgs e)
        {
            NewExpansionDialog kd = new NewExpansionDialog();
            if(kd.ShowDialog() == DialogResult.OK)
            {
                kd.Close();
            }
        }
        private void HandleExpansionEditorEvents(object sender, ExpansionEditorEventArgs e)
        {
            switch(e.Message)
            {
                case ExpansionEditorMessage.New:
                    NewExpansionKrystal();
                    break;
                case ExpansionEditorMessage.Open:
                    OpenExpansionKrystal();
                    break;
            }
        }
        private void NewExpansionKrystal()
        {
            try
            {
                ExpansionEditor editor = new ExpansionEditor(KrystalBrowser);
                editor.EventHandler += new ExpansionEditor.ExpansionEditorEventhandler(HandleExpansionEditorEvents);
                editor.Show();
            }
            catch(ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void OpenExpansionKrystal()
        {
            try
            {
                string expansionKrystalFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.expansion);
                if(expansionKrystalFilepath.Length > 0)
                {
                    ExpansionKrystal outputKrystal = new ExpansionKrystal(expansionKrystalFilepath);

                    ExpansionEditor editor = new ExpansionEditor(outputKrystal);
                    editor.EventHandler += new ExpansionEditor.ExpansionEditorEventhandler(HandleExpansionEditorEvents);
                    editor.Show();
                }
            }
            catch(ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void NewModulatedKrystalButton_Click(object sender, EventArgs e)
        {
            NewModulatedKrystal();
        }

        private void HandleModulationEditorEvents(object sender, ModulationEditorEventArgs e)
        {
            switch(e.Message)
            {
                case ModulationEditorMessage.New:
                    NewModulatedKrystal();
                    break;
                case ModulationEditorMessage.Open:
                    OpenModulatedKrystal();
                    break;
            }
        }
        private void NewModulatedKrystal()
        {
            using(NewModulationDialog kd = new NewModulationDialog())
            {
                DialogResult result = kd.ShowDialog();
                ModulationKrystal mKrystal = null;
                if(result == DialogResult.OK)
                {
                    if(string.IsNullOrEmpty(kd.XInputFilepath) || string.IsNullOrEmpty(kd.YInputFilepath))
                    {
                        MessageBox.Show("Both the XInput and the YInput krystals must be set.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        NewModulatedKrystal(); // recursive call
                    }
                    else if(kd.ModulationKrystal != null)
                        mKrystal = kd.ModulationKrystal;
                    else
                        mKrystal = new ModulationKrystal(kd.XInputFilepath,
                                                   kd.YInputFilepath,
                                                   kd.ModulatorFilepath);

                    ModulationEditor editor = new ModulationEditor(mKrystal)
                    {
                        EventHandler = new ModulationEditor.ModulationEditorEventHandler(HandleModulationEditorEvents)
                    };
                    editor.Show();
                }
            }
        }
        private void OpenModulatedKrystal()
        {
            try
            {
                string modulatedKrystalFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.modulation);
                if(modulatedKrystalFilepath.Length > 0)
                {
                    ModulationKrystal outputKrystal = new ModulationKrystal(modulatedKrystalFilepath);

                    ModulationEditor editor = new ModulationEditor(outputKrystal)
                    {
                        EventHandler = new ModulationEditor.ModulationEditorEventHandler(HandleModulationEditorEvents)
                    };
                    editor.Show();
                }
            }
            catch(ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void NewPermutedKrystalButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void NewPathKrystalButton_Click(object sender, EventArgs e)
        {
            string svgFilepath = "";
            string densityInputKrystalFilePath = "";

            using(NewPathExpansionDialog dlg = new NewPathExpansionDialog())
            {
                DialogResult result = dlg.ShowDialog();
                if(result == DialogResult.OK)
                {
                    svgFilepath = dlg.TrajectorySVGFilepath;
                    densityInputKrystalFilePath = dlg.DensityInputKrystalFilepath;
                }
            }

            if(!String.IsNullOrEmpty(svgFilepath) && !String.IsNullOrEmpty(densityInputKrystalFilePath))
            {
                var pathKrystal = new PathKrystal(svgFilepath, densityInputKrystalFilePath);
                var hasBeenSaved = pathKrystal.Save();
                if(hasBeenSaved)
                {
                    KrystalsBrowser kb = new KrystalsBrowser("New Krystal", pathKrystal, null);
                    kb.Show();
                }
            }
        }

        #endregion New Krystals

        #region For all krystals

        private void OpenKrystalsBrowserButton_Click(object sender, EventArgs e)
        {
            KrystalsBrowser KrystalBrowser = new KrystalsBrowser();
            KrystalBrowser.Show();
        }

        #endregion For all krystals

        private void About_Click(object sender, EventArgs e)
        {
            Form about = new AboutKrystals5();
            about.ShowDialog();
        }

        private void QuitButton_Click(object sender, EventArgs e)
        {
            M.Preferences.Dispose();
            Close();
        }

        private readonly string _krystalsFolder = M.LocalMoritzKrystalsFolder;
        private KrystalsBrowser KrystalBrowser = null;
    }
}