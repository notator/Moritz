using Krystals5ObjectLibrary;

using Moritz.Globals;

using System.Diagnostics;
using System.Text;

namespace DeleteUnusedDuplicateKrystalsApp
{
    public partial class UtilitiesForm1 : Form
    {
        public UtilitiesForm1()
        {
            InitializeComponent();
        }

        #region DeleteUnusedDuplicateKrystals
        private void DeleteUnusedDuplicateKrystalsButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Delete all unused duplicate Krystals?\n\n" +
                "If unused duplicate krystals are found in the krystals\n" +
                "folder, a backup of that folder will first be created\n" +
                "in a new, parallel \"_MorizBackup\" folder.",
                "Delete Unused Duplicate Krystals", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            if(result == DialogResult.OK)
            {
                List<string> filesToDelete = new List<string>();
                
                try
                {
                    Cursor.Current = Cursors.WaitCursor; // Note that the wait cursor is only displayed when over UtilitiesForm1

                    filesToDelete.AddRange(FindFilesToDelete(Directory.EnumerateFiles(_krystalsFolder, "*.constant.krys")));
                    filesToDelete.AddRange(FindFilesToDelete(Directory.EnumerateFiles(_krystalsFolder, "*.line.krys")));
                    filesToDelete.AddRange(FindFilesToDelete(Directory.EnumerateFiles(_krystalsFolder, "*.exp.krys")));
                    filesToDelete.AddRange(FindFilesToDelete(Directory.EnumerateFiles(_krystalsFolder, "*.mod.krys")));
                    filesToDelete.AddRange(FindFilesToDelete(Directory.EnumerateFiles(_krystalsFolder, "*.perm.krys")));
                    filesToDelete.AddRange(FindFilesToDelete(Directory.EnumerateFiles(_krystalsFolder, "*.path.krys")));
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                    StringBuilder msgStrB = new();
                    if(filesToDelete.Count == 0)
                    {
                        MessageBox.Show("No unused duplicate krystals were found", "Result", MessageBoxButtons.OK);
                    }
                    else
                    {
                        string backupDirectoryName = K.CopyDirectoryToMoritzBackup(_krystalsFolder);
                        msgStrB.Append("The following unused, duplicate krystals have been deleted:\n\n");
                        foreach(var filename in filesToDelete)
                        {
                            msgStrB.Append(filename + "\n");
                            var deletePath = _krystalsFolder + @"//" + filename;
                            File.Delete(deletePath);
                        }
                        msgStrB.Append($"\nA backup of the original krystals folder has been created in\n    {backupDirectoryName}");
                        MessageBox.Show(msgStrB.ToString(), "Deleted Krystals", MessageBoxButtons.OK);
                    }
                }
            }
        }

        private List<string> FindFilesToDelete(IEnumerable<string> iEnumKrystalPaths)
        {
            List<string> filesToDelete = new List<string>();

            if(iEnumKrystalPaths.Count<string>() > 1)
            {
                List<string> krystalPaths = iEnumKrystalPaths.ToList<string>();

                List<List<string>> listsOfDuplicates = GetListsOfDuplicates(krystalPaths);

                if(listsOfDuplicates.Count > 0 && listsOfDuplicates[0].Count > 0)
                {
                    // Now remove all krystalNames, from the listsOfDuplicates, that have children in the KrystalChildrenTreeView
                    Dictionary<string, List<string>> krystalScoresDict = K.GetKrystalScoresDict();

                    var krystalFamily = new KrystalFamily(this._krystalsFolder, krystalScoresDict);

                    var krystalChildrenTreeView = new KrystalChildrenTreeView(krystalFamily, null, null);

                    var rootNode = GetRootNode(krystalChildrenTreeView, listsOfDuplicates[0][0]);

                    foreach(var listOfDuplicates in listsOfDuplicates)
                    {
                        List<TreeNode> nodesToDelete = GetNodesToDelete(rootNode, listOfDuplicates);
                        foreach(var node in nodesToDelete)
                        {
                            string krystalPath = _krystalsFolder + "//" + node.Text;
                            filesToDelete.Add(node.Text);
                        }
                    }
                }
            }
            return filesToDelete;
        }

        private static List<TreeNode> GetNodesToDelete(TreeNode rootNode, List<string> listOfDuplicates)
        {
            List<TreeNode> rval = new List<TreeNode>();
            foreach(string krystalFilename in listOfDuplicates)
            {
                foreach(TreeNode node in rootNode.Nodes)
                {
                    if(node.Text == krystalFilename)
                    {
                        if(node.Nodes.Count == 0)
                        {
                            rval.Add(node);
                        }
                        break;
                    }
                }
            }
            if(rval.Count > 0)
            {
                rval.RemoveAt(rval.Count - 1); // the final duplicate is not deletable
            }
            return rval;
        }

        private TreeNode GetRootNode(KrystalChildrenTreeView krystalChildrenTreeView, string krystalFilename)
        {
            TreeNode rval = new();
            var krystalType = K.GetKrystalTypeFromKrystalName(krystalFilename);
            foreach(TreeNode node in krystalChildrenTreeView.Nodes)
            {
                if(node.Text == K.BrowserChildrenTreeViewRootNodeName(krystalType))
                {
                    rval = node;
                    break;
                }
            }

            return rval;
        }

        /// <summary>
        /// returns lists of filenames of duplicate krystals.
        /// Each list is in reverse order of index.
        /// </summary>
        /// <param name="allKrystalPaths"></param>
        /// <returns></returns>
        private List<List<string>> GetListsOfDuplicates(List<string> allKrystalPaths)
        {
            List<List<string>> allDuplicatesLists = new List<List<string>>();
            int allKrystalsCount = allKrystalPaths.Count();
            List<string> currentDuplicates = new List<string>();
            List<int> touchedKrystalIndices = new List<int>();

            for(int i = 0; i < allKrystalsCount; i++)
            {
                if(touchedKrystalIndices.Contains(i))
                {
                    continue;
                }
                else
                {
                    touchedKrystalIndices.Add(i);
                    var testKrystal = new DensityInputKrystal(allKrystalPaths[i]);
                    for(int j = i + 1; j < allKrystalsCount; j++)
                    {
                        if(touchedKrystalIndices.Contains(j))
                        {
                            continue;
                        }
                        else
                        {
                            var otherKrystal = new DensityInputKrystal(allKrystalPaths[j]);
                            if(testKrystal.Equals(otherKrystal)) // Equals only compares the strands in both krystals
                            {
                                if(currentDuplicates.Contains(testKrystal.Name) == false)
                                {
                                    if(currentDuplicates.Count > 0)
                                    {
                                        currentDuplicates.Sort(K.CompareKrystalNames);
                                        currentDuplicates.Reverse();
                                        allDuplicatesLists.Add(currentDuplicates);
                                    }
                                    currentDuplicates = new List<string>() { testKrystal.Name };
                                }
                                currentDuplicates.Add(otherKrystal.Name);
                                touchedKrystalIndices.Add(j);
                            }
                        }

                    }
                }
            }
            if(currentDuplicates.Count > 0)
            {
                allDuplicatesLists.Add(currentDuplicates);
            }

            return allDuplicatesLists;
        }

        #endregion

        #region Old SaveKrystalsWithNewNames
        /// <summary>
        /// 09.08.2022
        /// ACHTUNG: The following procedure was used to change the naming of existing krystals and expanders:
        /// Note step 4, and that any krystals not in the old backups will have to be saved separately.
        /// 1. copy the original files (having the original names) into the appropriate folders:
        ///    krystals/expansion operators
        ///    krystals/krystals
        ///    krystals/modulation operators
        /// 2. call this function, to create new files parallel to the old ones, and printing a table
        ///    of the oldName-newName equivalences to the console.
        ///    This info has been copied to krystals/NameChanges.txt
        /// 3. Manually delete the old files.
        /// 4. ACHTUNG: Manually change the remaining names of krystals and expanders inside expander (.kexp) files.
        /// 5. There was an incorrect schema location (beginning J:) in one of the Expanders.Change it manually.
        /// 6. Removed the temporary code in ModulationKrystal.cs.
        ///    See https://github.com/notator/Moritz/commit/407e635bd37f71c434a4b66988973fa5716016e4 
        ///    
        /// 12.09.2022 Added the initial automatic backup of the krystals folder, now that I have a function for that.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveKrystalsWithNewNamesButton_Click(object sender, EventArgs e)
        {
            // 12.09.2022: These two lines added
            string backupDirectoryName = K.CopyDirectoryToMoritzBackup(_krystalsFolder);
            MessageBox.Show($"\nA backup of the original krystals folder has been created in\n    {backupDirectoryName}");

            M.Assert(false, "This function has only been kept for archiving purposes." +
                "Before running it, see the comment above it in the code.");

            Dictionary<string, string> expanderNamesDict = SaveExpandersWithNewNames();

            Console.WriteLine("=================================================");
            Console.WriteLine("Expander Name Changes");
            foreach(var pair in expanderNamesDict)
            {
                Console.WriteLine(pair.Key + " --> " + pair.Value);
            }

            var iEnumKrysFilePaths = Directory.EnumerateFiles(_krystalsFolder, "*.krys");

            List<string> krysFilePaths = iEnumKrysFilePaths.ToList();
            krysFilePaths.Sort();
            Dictionary<string, string> krystalNamesDict = SaveKrystalsWithNewNames(krysFilePaths, expanderNamesDict);

            Console.WriteLine("=================================================");
            Console.WriteLine("Krystal Name Changes");
            foreach(var pair in krystalNamesDict)
            {
                Console.WriteLine(pair.Key + " --> " + pair.Value);
            }
        }

        /// <summary>
        /// returns a dictionary of oldExpanderName, newExpanderName pairs.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> SaveExpandersWithNewNames()
        {
            var expandersNamesDict = new Dictionary<string, string>();
            var expanderFilePaths = Directory.EnumerateFiles(M.MoritzExpansionFieldsFolder, "e*.kexp");
            List<string> sortedExpanderFilePaths = expanderFilePaths.ToList();
            sortedExpanderFilePaths.Sort();

            int index = 1;
            char[] dot = new char[] { '.' };
            foreach(string path in sortedExpanderFilePaths)
            {
                string oldName = Path.GetFileName(path);
                string bracketContent = oldName.Substring(oldName.IndexOf('(') + 1, oldName.IndexOf(')') - oldName.IndexOf('(') - 1);
                string[] content = bracketContent.Split(dot);
                string newName = String.Format($"{content[0]}.{content[1]}.{index}.kexp");
                index++;

                expandersNamesDict.Add(oldName, newName);

                string newPath = K.ExpansionOperatorsFolder + "//" + newName;
                File.Delete(newPath);
                File.Copy(path, newPath);
            }
            return expandersNamesDict;
        }

        private Dictionary<string, string> SaveKrystalsWithNewNames(List<string> krysFilePaths, Dictionary<string, string> expanderNamesDict)
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
                ///   followed by a shapeNameString followed by a '.' character.
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

                    return sb.ToString();
                }

                string GetIndex(string name)
                {
                    StringBuilder sb = new StringBuilder();

                    int iInd = name.IndexOf("-");
                    int dotInd = name.LastIndexOf(".");
                    string iStr = name.Substring(iInd + 1, dotInd - iInd - 1);
                    int.TryParse(iStr, out int index);

                    sb.Append(index.ToString());
                    sb.Append('.');

                    return sb.ToString();
                }

                string NewExpandedKrystalName(Krystal k, int expanderID, K.KrystalType type)
                {
                    var nameRoot = GetNameRoot(k);
                    var index = GetIndex(k.Name);
                    string kName = String.Format($"{nameRoot}{expanderID}.{index}{type}.krys");

                    return kName;
                }

                string NewKrystalName(Krystal k, K.KrystalType type)
                {
                    var nameRoot = GetNameRoot(k);
                    var index = GetIndex(k.Name);
                    string kName = String.Format($"{nameRoot}{index}{type}.krys");

                    return kName;
                }

                int GetExpanderID(ExpansionKrystal k)
                {
                    string newEName = expanderNamesDict[k.ExpanderFilename];
                    char[] dot = new char[] { '.' };
                    string[] eComponents = newEName.Split(dot);
                    int.TryParse(eComponents[2], out int id);

                    return id;
                }

                string newName = "";
                if(rval.ContainsKey(oldName))
                {
                    newName = rval[oldName];
                }
                else
                {
                    int expanderID;
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
                            expanderID = GetExpanderID(xk);
                            newName = NewExpandedKrystalName(xk, expanderID, K.KrystalType.exp);
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

            foreach(var krysFilePath in krysFilePaths)
            {
                var originalKrysFilename = Path.GetFileName(krysFilePath);
                if(originalKrysFilename.StartsWith("ck"))
                {
                    ConstantKrystal cKrystal = new ConstantKrystal(krysFilePath);
                    string newCName = cKrystal.Name;
                    SetRvalDict(originalKrysFilename, newCName);
                    File.Delete(K.KrystalsFolder + "//" + newCName);
                    var hasBeenSaved = cKrystal.Save();
                    if(hasBeenSaved)
                    {
                        KrystalsBrowser kb = new KrystalsBrowser("New Krystal", cKrystal, null);
                        kb.Show();
                    }
                }
                if(originalKrysFilename.StartsWith("lk"))
                {
                    LineKrystal lKrystal = new LineKrystal(krysFilePath);
                    string newLName = NewKrystalFilename(lKrystal.Name);
                    lKrystal.Name = newLName;
                    SetRvalDict(originalKrysFilename, newLName);
                    File.Delete(K.KrystalsFolder + "//" + newLName);
                    var hasBeenSaved = lKrystal.Save();
                    if(hasBeenSaved)
                    {
                        KrystalsBrowser kb = new KrystalsBrowser("New Krystal", lKrystal, null);
                        kb.Show();
                    }
                }
                if(originalKrysFilename.StartsWith("xk"))
                {
                    ExpansionKrystal k = new ExpansionKrystal(krysFilePath);

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
                    string oldEName = k.ExpanderFilename;
                    string newEName = expanderNamesDict[oldEName];
                    k.ExpanderFilename = newEName;

                    File.Delete(K.KrystalsFolder + "//" + newName);

                    var hasBeenSaved = k.Save();
                    if(hasBeenSaved)
                    {
                        KrystalsBrowser kb = new KrystalsBrowser("New Krystal", k, null);
                        kb.Show();
                    }
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

                    File.Delete(K.KrystalsFolder + "//" + newName);

                    var hasBeenSaved = k.Save();
                    if(hasBeenSaved)
                    {
                        KrystalsBrowser kb = new KrystalsBrowser("New Krystal", k, null);
                        kb.Show();
                    }
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

                    File.Delete(K.KrystalsFolder + "//" + newName);

                    var hasBeenSaved = k.Save();
                    if(hasBeenSaved)
                    {
                        KrystalsBrowser kb = new KrystalsBrowser("New Krystal", k, null);
                        kb.Show();
                    }
                }
            }

            return rval;
        }
        #endregion SaveKrystalsWithNewNames

        private void QuitButton_Click(object sender, EventArgs e)
        {
            M.Preferences.Dispose();
            Close();
        }

        private readonly string _krystalsFolder = M.MoritzKrystalsFolder;
    }
}