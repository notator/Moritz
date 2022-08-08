using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Krystals;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                        ConstantKrystal ck = new ConstantKrystal(constantValue);
                        ck.Save();
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
                        LineKrystal lk = new LineKrystal(lineValue);
                        lk.Save();
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
                pathKrystal.Save();

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
                K.RebuildKrystalFamily();
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
            Dictionary<string, string> namesDict = SaveKrystalsWithNewNames(krysFilePaths);
            // CorrectHeredity(namesDict);
        }

        private Dictionary<string, string> SaveKrystalsWithNewNames(IEnumerable<string> krysFilePaths)
        {
            Dictionary<string, string> rval = new Dictionary<string, string>();

            void SetRvalDict(string oldName, string newName)
            {
                if(!rval.ContainsKey(oldName))
                    rval.Add(oldName, newName);
            }

            string NewKrystalFilename(string oldName)
            {
                /// <summary>
                /// A Krystal's name root consists of its domain (=MaxValue) followed by a '.' character,
                ///   followed by a shapeNameString followed by a '.' character,
                ///   followed by an index followed by a '.' character.
                /// The shapeNameString contains one or more integers separated by '_' characters.
                /// The first int in the shapeNameString is the number of level 1 and level 2 strands, so:
                ///  "0" is a constant krystal -- containing one strand having level 0 and one value (=domain) (no level 1 or level 2 strands).
                ///  "1_[nValues]" is a line krystal -- containing one strand having level 1 and [nValues] values (no level 2 strands).
                ///  "7_[nValues)" is a level 2 krystal -- containing 7 strands (1 level 1 and 6 level 2 strands) and [nValues] values.
                ///  "7_28_[nValues]" is a level 3 krystal - containing 28 strands having level 1, 2, or 3, and [nValues] values.
                ///  "7_28_206_[nValues]" is a level 4 krystal - containing 206 strands having level 1, 2, 3 or 4, and [nValues] values. 
                /// </summary>
                /// <returns></returns>
                string GetNameRoot(Krystal k)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(k.MaxValue.ToString() + ".");

                    if(k.Level == 0)
                    {
                        sb.Append("0");
                    }
                    else if(k.Level == 1)
                    {
                        sb.Append("1");
                        sb.Append("_");
                        sb.Append(k.Strands[0].Values.Count().ToString());
                    }
                    else
                    {
                        for(int i = 1; i < k.ShapeArray.Length; i++)
                        {
                            sb.Append(k.ShapeArray[i].ToString() + '_');
                        }
                        sb.Remove(sb.Length - 1, 1);
                    }

                    sb.Append('.');

                    #region get index
                    int iInd = k.Name.IndexOf("-");
                    int dotInd = k.Name.LastIndexOf(".");
                    string iStr = k.Name.Substring(iInd + 1, dotInd - iInd - 1);
                    int.TryParse(iStr, out int index);

                    sb.Append(index.ToString());
                    sb.Append('.');

                    #endregion

                    return sb.ToString();
                }

                string NewKrystalName(Krystal k, K.KrystalType type )
                {
                    var nameRoot = GetNameRoot(k);
                    string lkName = String.Format($"{nameRoot}{type}.krys");

                    return lkName;
                }

                //string UniquePathKrystalName()
                //{
                //    var nameRoot = GetNameRoot();

                //    IEnumerable<string> similarKrystalPaths = GetSimilarKrystalPaths(nameRoot, K.KrystalType.path);

                //    string rval = String.Format($"{nameRoot}{(similarKrystalPaths.Count() + 1)}.{K.KrystalType.path}{K.KrystalFilenameSuffix}");

                //    foreach(var existingPath in similarKrystalPaths)
                //    {
                //        var existingKrystal = new PathKrystal(existingPath);
                //        if(existingKrystal.DensityInputKrystalName == this.DensityInputKrystalName
                //        && existingKrystal.SVGInputFilename == this.SVGInputFilename)
                //        {
                //            rval = Path.GetFileName(existingPath);
                //            break;
                //        }
                //    }
                //    return rval;
                //}

                string newName = null;
                if(rval.ContainsKey(oldName))
                {
                    newName = rval[oldName];
                }
                else
                {
                    string typeString = oldName.Substring(0, 2);
                    switch(typeString)
                    {
                        case "ck":
                            var cKrystal = new ConstantKrystal(K.KrystalsFolder + "//" + oldName);
                            newName = cKrystal.Name;
                            break;
                        case "lk":
                            var lk = new LineKrystal(K.KrystalsFolder + "//" + oldName);
                            newName = NewKrystalName(lk, K.KrystalType.line);
                            break;
                        case "xk":
                            var xk = new ExpansionKrystal(K.KrystalsFolder + "//" + oldName);
                            newName = NewKrystalName(xk, K.KrystalType.exp);
                            break;
                        case "sk":
                            var sk = new ShapedExpansionKrystal(K.KrystalsFolder + "//" + oldName);
                            newName = NewKrystalName(sk, K.KrystalType.shaped);
                            break;
                        case "mk":
                            var mk = new ModulationKrystal(K.KrystalsFolder + "//" + oldName);
                            newName = NewKrystalName(mk, K.KrystalType.mod);
                            break;
                        case "pk":
                            var pk = new PermutationKrystal(K.KrystalsFolder + "//" + oldName);
                            newName = NewKrystalName(pk, K.KrystalType.perm);
                            break;
                    }
                }
                return newName;
            }

            string NewModulatorFilename(string oldName)
            {
                string newName = null;
                if(rval.ContainsKey(oldName))
                {
                    newName = rval[oldName];
                }
                else
                {
                    string on = oldName;
                    if(on.StartsWith("m"))
                    {
                        int mInd = on.IndexOf("m"); // = 0
                        int xInd = on.IndexOf("x");
                        int bInd = on.IndexOf("(");
                        int iInd = on.IndexOf("-");
                        int dotInd = on.IndexOf(".");

                        string xStr = on.Substring(mInd + 1, xInd - mInd - 1);
                        string yStr = on.Substring(xInd + 1, bInd - xInd - 1);
                        string dStr = on.Substring(bInd + 1, iInd - bInd - 2);
                        string iStr = on.Substring(iInd + 1, dotInd - iInd - 1);

                        int.TryParse(xStr, out int xDim);
                        int.TryParse(yStr, out int yDim);
                        int.TryParse(dStr, out int maxValue);
                        int.TryParse(iStr, out int index);
                        newName = String.Format($"{xDim}.{yDim}.{maxValue}.{index}.kmod");
                        rval.Add(oldName, newName);
                    }
                    else newName = oldName;                    
                }
                return newName;
            }

            foreach(var krysFilePath in krysFilePaths)
            {
                var originalKrysFilename = Path.GetFileName(krysFilePath);
                if(originalKrysFilename.StartsWith("ck"))
                {
                    ConstantKrystal cKrystal = new ConstantKrystal(krysFilePath);
                    string newCName = cKrystal.Name;
                    SetRvalDict(originalKrysFilename, newCName);
                    cKrystal.Save();
                }
                if(originalKrysFilename.StartsWith("lk"))
                {
                    LineKrystal lKrystal = new LineKrystal(krysFilePath);
                    string newLName = NewKrystalFilename(lKrystal.Name);
                    lKrystal.Name = newLName;
                    SetRvalDict(originalKrysFilename, newLName);
                    lKrystal.Save();
                }
                if(originalKrysFilename.StartsWith("xk"))
                {
                    ExpansionKrystal oldKrystal = new ExpansionKrystal(krysFilePath);
                    //string dInputFilepath = K.KrystalsFolder + @"/" + NewKrystalFilename(oldKrystal.DensityInputFilename);
                    //string pInputFilepath = K.KrystalsFolder + @"/" + NewKrystalFilename(oldKrystal.PointsInputFilename);
                    //// decided not to change Expander names
                    ////string eInputFilepath = K.ExpansionOperatorsFolder + @"/" + NewExpanderFilename(oldKrystal.ExpanderFilename);
                    ////ExpansionKrystal newKrystal = new ExpansionKrystal(dInputFilepath, pInputFilepath, eInputFilepath); // generates new krystal name
                    //newKrystal.Save();
                    //newKrystal.Expander.Save(); // This would be better put in newKrystal.Save().

                    //SetRvalDict(originalKrysFilename, newKrystal.Name);
                    //SetRvalDict(oldKrystal.DensityInputFilename, newKrystal.DensityInputFilename);
                    //SetRvalDict(oldKrystal.PointsInputFilename, newKrystal.PointsInputFilename);
                    //SetRvalDict(oldKrystal.ExpanderFilename, newKrystal.ExpanderFilename);
                }
                if(originalKrysFilename.StartsWith("mk"))
                {
                    ModulationKrystal k = new ModulationKrystal(krysFilePath);
                    string oldName = k.Name;
                    string newName = NewKrystalFilename(k.Name);
                    k.Name = newName;
                    SetRvalDict(oldName, newName);
                    string oldXInput = k.XInputFilename;
                    string newXInput = NewKrystalFilename(k.XInputFilename);
                    k.XInputFilename = newXInput;
                    SetRvalDict(oldXInput, newXInput);
                    string oldYInput = k.YInputFilename;
                    string newYInput = NewKrystalFilename(k.YInputFilename);
                    k.YInputFilename = newYInput;
                    SetRvalDict(oldYInput, newYInput);
                    string oldModName = k.ModulatorFilename; 
                    string newModName = NewModulatorFilename(k.ModulatorFilename);                    
                    k.ModulatorFilename = newYInput;
                    SetRvalDict(oldModName, newModName);

                    k.Save();  
                }
                if(originalKrysFilename.StartsWith("pk"))
                {
                    PermutationKrystal k = new PermutationKrystal(krysFilePath);

                    string oldName = k.Name;
                    string newName = NewKrystalFilename(k.Name);
                    k.Name = newName;
                    SetRvalDict(oldName, newName);
                    string oldSInput = k.SourceInputFilename;
                    string newSInput = NewKrystalFilename(k.SourceInputFilename);
                    k.SourceInputFilename = newSInput;
                    SetRvalDict(oldSInput, newSInput);
                    string oldAInput = k.AxisInputFilename;
                    string newAInput = NewKrystalFilename(k.AxisInputFilename);
                    k.AxisInputFilename = newAInput;
                    SetRvalDict(oldAInput, newAInput);
                    string oldCName = k.ContourInputFilename;
                    string newCName = NewKrystalFilename(k.ContourInputFilename);
                    k.ContourInputFilename = newCName;
                    SetRvalDict(oldCName, newCName);

                    k.Save();
                }
                if(originalKrysFilename.StartsWith("sk"))
                {
                    ShapedExpansionKrystal k = new ShapedExpansionKrystal(krysFilePath);

                    string oldName = k.Name;
                    string newName = NewKrystalFilename(k.Name);
                    k.Name = newName;
                    SetRvalDict(oldName, newName);
                    string oldPInput = k.PointsInputFilename;
                    string newPInput = NewKrystalFilename(k.PointsInputFilename);
                    k.PointsInputFilename = newPInput;
                    SetRvalDict(oldPInput, newPInput);
                    string oldDInput = k.DensityInputFilename;
                    string newDInput = NewKrystalFilename(k.DensityInputFilename);
                    k.DensityInputFilename = newDInput;
                    SetRvalDict(oldDInput, newDInput);
                    string oldAInput = k.AxisInputFilename;
                    string newAInput = NewKrystalFilename(k.AxisInputFilename);
                    k.AxisInputFilename = newAInput;
                    SetRvalDict(oldAInput, newAInput);
                    string oldCName = k.ContourInputFilename;
                    string newCName = NewKrystalFilename(k.ContourInputFilename);
                    k.ContourInputFilename = newCName;
                    SetRvalDict(oldCName, newCName);
                    // decided not to change expander names

                    k.Save();
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