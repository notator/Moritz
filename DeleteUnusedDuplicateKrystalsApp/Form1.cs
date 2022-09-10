using Krystals5ObjectLibrary;

using Moritz.Globals;

using System.Text;

namespace DeleteUnusedDuplicateKrystalsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region DeleteUnusedDuplicateKrystals
        private void DeleteUnusedDuplicateKrystalsButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Delete all unused duplicate Krystals?\n\n" +
                "A backup of the original krystals folder will be created\nin a new, parallel \"_MorizBackup\" folder.",
                "Delete Unused Duplicate Krystals", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if(result == DialogResult.OK)
            {
                List<string> deletedFiles = new List<string>();
                string backupDirectoryName = "";
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    backupDirectoryName = K.CopyDirectoryToMoritzBackup(_krystalsFolder);
                    deletedFiles.AddRange(RemoveDuplicates(Directory.EnumerateFiles(_krystalsFolder, "*.constant.krys")));
                    deletedFiles.AddRange(RemoveDuplicates(Directory.EnumerateFiles(_krystalsFolder, "*.line.krys")));
                    deletedFiles.AddRange(RemoveDuplicates(Directory.EnumerateFiles(_krystalsFolder, "*.exp.krys")));
                    deletedFiles.AddRange(RemoveDuplicates(Directory.EnumerateFiles(_krystalsFolder, "*.mod.krys")));
                    deletedFiles.AddRange(RemoveDuplicates(Directory.EnumerateFiles(_krystalsFolder, "*.perm.krys")));
                    deletedFiles.AddRange(RemoveDuplicates(Directory.EnumerateFiles(_krystalsFolder, "*.path.krys")));
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    StringBuilder msgStrB = new StringBuilder();
                    msgStrB.Append("The following unused, duplicate krystals have been deleted:\n\n");
                    foreach(var filename in deletedFiles)
                    {
                        msgStrB.Append(filename + "\n");
                    }
                    msgStrB.Append($"\nA backup of the original krystals folder has been created in\n    {backupDirectoryName}");
                    MessageBox.Show(msgStrB.ToString(), "Deleted", MessageBoxButtons.OK);
                }
            }
        }

        private List<string> RemoveDuplicates(IEnumerable<string> iEnumKrystalPaths)
        {
            List<string> deletedFiles = new List<string>();

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
                            deletedFiles.Add(node.Text);
                            File.Delete(krystalPath);
                        }
                    }
                }
            }
            return deletedFiles;
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

        private void QuitButton_Click(object sender, EventArgs e)
        {
            M.Preferences.Dispose();
            Close();
        }

        private readonly string _krystalsFolder = M.LocalMoritzKrystalsFolder;
    }
}