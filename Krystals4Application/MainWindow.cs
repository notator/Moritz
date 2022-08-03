using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Krystals;

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Krystals4Application
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NewConstantKrystalMenuItem_Click(object sender, EventArgs e)
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
                        ConstantKrystal ck = new ConstantKrystal(K.UntitledKrystalName, constantValue);
                        ck.Save(false);  // false: do not overwrite existing files
                    }
                }
            }
        }
        private void OpenConstantKrystalMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string constantKrystalFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.constant);
                if (constantKrystalFilepath.Length > 0)
                {
                    ConstantKrystal constantKrystal = new ConstantKrystal(constantKrystalFilepath);
					NewConstantKrystalDialog dlg = new NewConstantKrystalDialog
					{
						Text = constantKrystal.Name
					};
					dlg.SetButtons();
                    dlg.ConstantKrystalValue = constantKrystal.MaxValue.ToString();
                    dlg.Show();
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void NewLineKrystalMenuItem_Click(object sender, EventArgs e)
        {
            using(NewLineKrystalDialog dlg = new NewLineKrystalDialog())
            {
                DialogResult result = dlg.ShowDialog();
                if(result == DialogResult.OK)
                {
                    string lineValue = dlg.LineKrystalValue;
                    if(lineValue.Length > 0)
                    {
                        LineKrystal lk = new LineKrystal(K.UntitledKrystalName, lineValue);
                        lk.Save(false);  // false: do not ovewrite existing files
                    }
                }
            }
        }
        private void OpenLineKrystalMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string lineKrystalFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.line);
                if (lineKrystalFilepath.Length > 0)
                {
                    LineKrystal lineKrystal = new LineKrystal(lineKrystalFilepath);
					NewLineKrystalDialog dlg = new NewLineKrystalDialog
					{
						Text = lineKrystal.Name
					};
					dlg.SetButtons();
                    dlg.LineKrystalValue = K.GetStringOfUnsignedInts(lineKrystal.Strands[0].Values);
                    dlg.Show();
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void NewPathKrystalMenuItem_Click(object sender, EventArgs e)
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
                pathKrystal.Save(true);

                string msg = "Saved Path Krystal:\n\n" + pathKrystal.Name; 
                MessageBox.Show(msg, "Saved Path Krystal", MessageBoxButtons.OK);
            }
        }

        private void NewExpansionKrystalMenuItem_Click(object sender, EventArgs e)
        {
            NewExpansionKrystal();
        }
        private void OpenExpansionKrystalMenuItem_Click(object sender, EventArgs e)
        {
            OpenExpansionKrystal();
        }
        private void HandleExpansionEditorEvents(object sender, ExpansionEditorEventArgs e)
        {
            switch (e.Message)
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
                ExpansionEditor editor = new ExpansionEditor();
                editor.EventHandler += new ExpansionEditor.ExpansionEditorEventhandler(HandleExpansionEditorEvents);
                editor.Show();
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void OpenExpansionKrystal()
        {
            try
            {
                string expansionKrystalFilepath = K.GetFilepathFromOpenFileDialog(K.DialogFilterIndex.expansion);
                if (expansionKrystalFilepath.Length > 0)
                {
                    ExpansionKrystal outputKrystal = new ExpansionKrystal(expansionKrystalFilepath);

                    ExpansionEditor editor = new ExpansionEditor(outputKrystal);
                    editor.EventHandler += new ExpansionEditor.ExpansionEditorEventhandler(HandleExpansionEditorEvents);
                    editor.Show();
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void NewModulatedKrystalMenuItem_Click(object sender, EventArgs e)
        {
            NewModulatedKrystal();
        }
        private void OpenModulatedKrystalMenuItem_Click(object sender, EventArgs e)
        {
            OpenModulatedKrystal();
        }
        private void HandleModulationEditorEvents(object sender, ModulationEditorEventArgs e)
        {
            switch (e.Message)
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
                if (modulatedKrystalFilepath.Length > 0)
                {
                    ModulationKrystal outputKrystal = new ModulationKrystal(modulatedKrystalFilepath);

					ModulationEditor editor = new ModulationEditor(outputKrystal)
					{
						EventHandler = new ModulationEditor.ModulationEditorEventHandler(HandleModulationEditorEvents)
					};
					editor.Show();
                }
            }
            catch (ApplicationException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void OpenKrystalsBrowserButton_Click(object sender, EventArgs e)
        {
            KrystalBrowser krystalBrowser = new KrystalBrowser(null, M.LocalMoritzKrystalsFolder, null);
            krystalBrowser.Show();
        }

        private void MenuItemRebuildKrystalFamily_Click(object sender, EventArgs e)
        {
			DialogResult result = MessageBox.Show(
				"Re-expand and re-modulate all expansion and modulation krystals\n" +
				"in the krystals directory?\n\n" +
				"This will ensure that all krystal dependencies are up to date.",
				"Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);

			if(result == DialogResult.OK)
			{
                KrystalFamily kFamily = new KrystalFamily(K.KrystalsFolder);
				kFamily.Rebuild();
				MessageBox.Show("All expansion and modulation krystals have been successfully recreated",
					"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
        }

        #region SaveKrystalsWithNewNames
        /// <summary>
        /// ACHTUNG: Before running this function, delete all the newly named krystals in the krystals folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveKrystalsWithNewNamesButton_Click(object sender, EventArgs e)
        {
            var dirPath = M.LocalMoritzKrystalsFolder;
            var krysFilePaths = Directory.EnumerateFiles(dirPath, "*.krys");
            // ACHTUNG: Before running this function, delete any newly named
            // krystals in the krystals folder, otherwise the name indices get confused.
            Dictionary<string, string> namesDict = SaveNewFilesAndGetNamesDict(krysFilePaths);
            CorrectHeredity(namesDict);
        }

        private static void CorrectHeredity(Dictionary<string, string> namesDict)
        {
            foreach(var pair in namesDict)
            {
                var newName = pair.Value;
                var path = M.LocalMoritzKrystalsFolder + "/" + newName;
                if(newName.Contains(K.KrystalType.exp.ToString()))
                {
                    var krystal = new ExpansionKrystal(path);
                    if(namesDict.ContainsKey(krystal.DensityInputFilename))
                    {
                        krystal.DensityInputFilename = namesDict[krystal.DensityInputFilename];
                    }
                    if(namesDict.ContainsKey(krystal.PointsInputFilename))
                    {
                        krystal.PointsInputFilename = namesDict[krystal.PointsInputFilename];
                    }
                    krystal.Save(true);
                }
                if(newName.Contains(K.KrystalType.mod.ToString()))
                {
                    var krystal = new ModulationKrystal(path);
                    if(namesDict.ContainsKey(krystal.XInputFilename))
                    {
                        krystal.XInputFilename = namesDict[krystal.XInputFilename];
                    }
                    if(namesDict.ContainsKey(krystal.YInputFilename))
                    {
                        krystal.YInputFilename = namesDict[krystal.YInputFilename];
                    }
                    krystal.Save(true);
                }
                if(newName.Contains(K.KrystalType.perm.ToString()))
                {
                    var krystal = new PermutationKrystal(path);
                    if(namesDict.ContainsKey(krystal.AxisInputFilename))
                    {
                        krystal.AxisInputFilename = namesDict[krystal.AxisInputFilename];
                    }
                    if(namesDict.ContainsKey(krystal.SourceInputFilename))
                    {
                        krystal.SourceInputFilename = namesDict[krystal.SourceInputFilename];
                    }
                    if(namesDict.ContainsKey(krystal.ContourInputFilename))
                    {
                        krystal.ContourInputFilename = namesDict[krystal.ContourInputFilename];
                    }
                    krystal.Save(true);
                }
                if(newName.Contains(K.KrystalType.shaped.ToString()))
                {
                    var krystal = new ShapedExpansionKrystal(path);
                    if(namesDict.ContainsKey(krystal.DensityInputFilename))
                    {
                        krystal.DensityInputFilename = namesDict[krystal.DensityInputFilename];
                    }
                    if(namesDict.ContainsKey(krystal.PointsInputFilename))
                    {
                        krystal.PointsInputFilename = namesDict[krystal.PointsInputFilename];
                    }
                    if(namesDict.ContainsKey(krystal.AxisInputFilename))
                    {
                        krystal.AxisInputFilename = namesDict[krystal.AxisInputFilename];
                    }
                    if(namesDict.ContainsKey(krystal.ContourInputFilename))
                    {
                        krystal.ContourInputFilename = namesDict[krystal.ContourInputFilename];
                    }
                    krystal.Save(true);
                }
                if(newName.Contains(K.KrystalType.path.ToString()))
                {
                    var krystal = new PathKrystal(path);
                    if(namesDict.ContainsKey(krystal.DensityInputKrystalName))
                    {
                        krystal.DensityInputKrystalName = namesDict[krystal.DensityInputKrystalName];
                    }
                    krystal.Save(true);
                }
            }
        }

        private Dictionary<string, string> SaveNewFilesAndGetNamesDict(IEnumerable<string> krysFilePaths)
        {
            Dictionary<string, string> rval = new Dictionary<string, string>();

            foreach(var krysFilePath in krysFilePaths)
            {
                var originalKrysFilename = Path.GetFileName(krysFilePath);
                if(originalKrysFilename.StartsWith("ck"))
                {
                    ConstantKrystal krystal = new ConstantKrystal(krysFilePath);
                    krystal.SetName(K.KrystalType.constant);
                    krystal.Save(true);

                    rval.Add(originalKrysFilename, krystal.Name);
                }
                if(originalKrysFilename.StartsWith("lk"))
                {
                    LineKrystal krystal = new LineKrystal(krysFilePath);
                    krystal.SetName(K.KrystalType.line);
                    krystal.Save(true);

                    rval.Add(originalKrysFilename, krystal.Name);
                }
                if(originalKrysFilename.StartsWith("xk"))
                {
                    ExpansionKrystal krystal = new ExpansionKrystal(krysFilePath);
                    krystal.SetName(K.KrystalType.exp);
                    krystal.Save(true);

                    rval.Add(originalKrysFilename, krystal.Name);
                }
                if(originalKrysFilename.StartsWith("mk"))
                {
                    ModulationKrystal krystal = new ModulationKrystal(krysFilePath);
                    krystal.SetName(K.KrystalType.mod);
                    krystal.Save(true);

                    rval.Add(originalKrysFilename, krystal.Name);
                }
                if(originalKrysFilename.StartsWith("pk"))
                {
                    PermutationKrystal krystal = new PermutationKrystal(krysFilePath);
                    krystal.SetName(K.KrystalType.perm);
                    krystal.Save(true);

                    rval.Add(originalKrysFilename, krystal.Name);
                }
                if(originalKrysFilename.StartsWith("sk"))
                {
                    ShapedExpansionKrystal krystal = new ShapedExpansionKrystal(krysFilePath);
                    krystal.SetName(K.KrystalType.shaped);
                    krystal.Save(true);

                    rval.Add(originalKrysFilename, krystal.Name);
                }
            }

            return rval;
        }
        #endregion SaveKrystalsWithNewNames
        private void MenuItemQuit_Click(object sender, EventArgs e)
        {
            M.Preferences.Dispose();
            Close();
        }

        private void About_Click(object sender, EventArgs e)
        {
            Form about = new AboutKrystals4();
            about.ShowDialog();
        }
    }
}