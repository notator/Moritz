using Krystals5ObjectLibrary;

using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Moritz.Krystals
{
    public delegate void ReturnKrystalDelegate(Krystal krystal);

    public partial class KrystalsBrowser : Form
    {
        /// <summary>
        /// The returnKrystalDelegate is called with the current krystal name when this krystalBrowser is closed.
        /// If krystal != null, the krystalBrowser opens with the given krystal selected.
        /// </summary>
        public KrystalsBrowser()
        {
            InitializeComponent();
            this.Text = "All Krystals";
            _returnKrystalDelegate = null;
            InitializeKrystalBrowser(null, null, null);
        }

        /// <summary>
        /// The returnKrystalDelegate is called with the current krystal name when this krystalBrowser is closed.
        /// </summary>
        public KrystalsBrowser(string title, ReturnKrystalDelegate returnKrystalDelegate)
        {
            InitializeComponent();
            this.Text = title;
            ControlBox = false;
            _returnKrystalDelegate = returnKrystalDelegate;
            InitializeKrystalBrowser(null, null, null); // display all krystals
        }

        /// <summary>
        /// Only Krystals whose shape is contained in the shapeListFilter are included in the displayed KrystaFamilyTree. 
        /// The returnKrystalDelegate is called with the current krystal name when this krystalBrowser is closed.
        /// </summary>
        public KrystalsBrowser(string title, int? domainFilter, List<int> shapeListFilter, ReturnKrystalDelegate returnKrystalDelegate)
        {
            InitializeComponent();
            this.Text = title;
            ControlBox = false;
            _returnKrystalDelegate = returnKrystalDelegate;
            InitializeKrystalBrowser(null, domainFilter, shapeListFilter);
        }

        /// <summary>
        /// The KrystalBrowser opens with the selectKrystal selected. 
        /// The returnKrystalDelegate is called with the current krystal name when this krystalBrowser is closed.
        /// </summary>
        public KrystalsBrowser(string title, Krystal selectKrystal, ReturnKrystalDelegate returnKrystalDelegate)
        {
            InitializeComponent();
            this.Text = title;
            ControlBox = false;
            _returnKrystalDelegate = returnKrystalDelegate;
            SetDeleteKrystalButtonVisible = true;
            InitializeKrystalBrowser(selectKrystal.Name, null, null);
        }

        private void InitializeKrystalBrowser(string selectKrystalName, int? domainFilter, List<int> shapeListFilter)
        {
            this.Width = Screen.PrimaryScreen.WorkingArea.Width - 100;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Location = new Point(0, 0);
            this.splitContainer1.SplitterDistance = Width / 6;
            this.splitContainer2.SplitterDistance = Width / 6;
            this.splitContainer3.SplitterDistance = Width / 6;
            this.Krystal2DTextBox.Text = "";

            if(_returnKrystalDelegate != null)
            {
                this.ReturnKrystalButton.Show();
                this.CloseButton.Hide();
            }
            else
            {
                this.ReturnKrystalButton.Hide();
                this.CloseButton.Show();
            }

            Dictionary<string, List<string>> krystalScoresDict = GetKrystalScoresDict();

            _krystalFamily = new KrystalFamily(this._krystalsFolder, krystalScoresDict);

            _krystalChildrenTreeView = new KrystalChildrenTreeView(_krystalFamily, domainFilter, shapeListFilter);

            _krystalChildrenTreeView.AfterSelect += new TreeViewEventHandler(this.KrystalChildrenTreeView_AfterSelect);

            this.splitContainer1.Panel1.Controls.Add(_krystalChildrenTreeView);

            SetForKrystal(selectKrystalName);
        }

        #region GetKrystalScoresDict

        /// <summary>
        /// The returned dictionary contains krystal/listOfScoresContainingIt
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<string>> GetKrystalScoresDict()
        {
            var rval = new Dictionary<string, List<string>>();
            string scoresPath = M.LocalMoritzScoresFolder;
            var allScoreSettings = Directory.EnumerateFiles(scoresPath, "*.mkss", SearchOption.AllDirectories);
            foreach(var scoreSettings in allScoreSettings)
            {
                string scoreName = Path.GetFileName(scoreSettings);
                scoreName = scoreName.Remove(scoreName.IndexOf(".mkss"));

                var scoreKrystals = GetScoreKrystals(scoreSettings);
                foreach(var krystalName in scoreKrystals)
                {
                    if(!rval.ContainsKey(krystalName))
                    {
                        rval.Add(krystalName, new List<string>() {scoreName});
                    }
                    else if(!rval[krystalName].Contains(scoreName))
                    {
                        rval[krystalName].Add(scoreName);
                    }
                }
            }
            return rval;
        }
        private List<string> GetScoreKrystals(string scoreSettings)
        {
            List<string> scoreKrystals = null;
            try
            {
                using(XmlReader r = XmlReader.Create(scoreSettings))
                {
                    M.ReadToXmlElementTag(r, "moritzKrystalScore"); // check that this is a krystal score settings file

                    M.ReadToXmlElementTag(r, "krystals", "moritzKrystalScore");

                    if(r.Name == "moritzKrystalScore")
                    {
                        scoreKrystals = new List<string>();
                    }
                    else
                    {
                        Debug.Assert(r.Name == "krystals");
                        scoreKrystals = GetKrystals(r);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error reading krystal score settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return scoreKrystals;
        }
        private List<string> GetKrystals(XmlReader r)
        {
            List<string> scoreKrystals = new List<string>();
            Debug.Assert(r.Name == "krystals");

            M.ReadToXmlElementTag(r, "krystal");
            while(r.Name == "krystal")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    r.MoveToAttribute(0);
                    scoreKrystals.Add(r.Value);
                }
                M.ReadToXmlElementTag(r, "krystal", "krystals");
            }
            return scoreKrystals;
        }
        #endregion

        /// <summary>
        /// Krystals can only be deleted if they have no children (i.e. they are not used to create scores or other krystals)
        /// </summary>
        private void DeleteKrystalButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(_krystalChildrenTreeView.SelectedNode.Nodes.Count == 0);
            
            var selectedNode = _krystalChildrenTreeView.SelectedNode;
            string krystalToDelete = selectedNode.Text;

            string msg = $"Delete {krystalToDelete}?\n\nCaution: This action cannot be undone. Are you sure?";
            var dialogResult = MessageBox.Show(msg, "Delete Krystal", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if(dialogResult == DialogResult.Yes)
            {
                string krystalPath = M.LocalMoritzKrystalsFolder + "//" + krystalToDelete;
                File.Delete(krystalPath);

                _krystalChildrenTreeView.BeginUpdate();
                _krystalChildrenTreeView.SelectedNode = null;
                _krystalChildrenTreeView.Nodes.Remove(selectedNode);
                _krystalChildrenTreeView.EndUpdate();
                this.SetForKrystal(null);
                SetDeleteKrystalButtonVisible = false;
                
            }
        }

        private bool SetDeleteKrystalButtonVisible
        {
            set
            {
                this.DeleteKrystalButton.Visible = value;
                this.DeleteKrystalLabel.Visible = !value;
            }
        }

        /// <summary>
        /// The ReturnKrystalButton is shown if _backPanel == null;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReturnKrystalButton_Click(object sender, EventArgs e)
        {
            if(this._returnKrystalDelegate != null)
            {
                _returnKrystalDelegate(_krystal);
                Close();
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void SetForKrystal(string filename)
        {
            _krystal = null;

            this.SuspendLayout();
            if(filename != null && K.IsKrystalFilename(filename))
            {
                string pathname = _krystalsFolder + "\\" + filename;
                _krystal = K.LoadKrystal(pathname);
            }

            if(_ancestorsTreeView != null)
            {
                KrystalAncestorsTreeView tv = this.splitContainer2.Panel1.Controls[0] as KrystalAncestorsTreeView;
                this.splitContainer2.Panel1.Controls.Clear();
                tv.Dispose();
            }
            // if filename is null, _krystal is null here
            _ancestorsTreeView = new KrystalAncestorsTreeView(_krystal, _krystalFamily);
            _ancestorsTreeView.AfterSelect += new TreeViewEventHandler(this.AncestorsTreeView_AfterSelect);
            this.splitContainer2.Panel1.Controls.Add(_ancestorsTreeView);

            ConstantKrystal ck = _krystal as ConstantKrystal;
            LineKrystal lk = _krystal as LineKrystal;
            ExpansionKrystal xk = _krystal as ExpansionKrystal;
            ModulationKrystal mk = _krystal as ModulationKrystal;
            PermutationKrystal pk = _krystal as PermutationKrystal;
            PathKrystal pathK = _krystal as PathKrystal;

            if(ck != null)
            {
                this.BasicData.Text = string.Format("Constant Krystal: {0}   Level: {1}   Value: {2}",
                    ck.Name, ck.Level.ToString(), ck.MaxValue.ToString());
                SetForConstantKrystal();
            }
            else if(lk != null)
            {
                this.BasicData.Text = string.Format("Line Krystal: {0}    Level: {1}    Range of Values: {2}..{3}",
                    _krystal.Name, _krystal.Level.ToString(), _krystal.MinValue.ToString(), _krystal.MaxValue.ToString());
                SetForLineKrystal();
            }
            else if(xk != null)
            {
                this.BasicData.Text = string.Format("Expansion Krystal: {0}   Expander: {1}   Level: {2}   Range of Values: {3}..{4}",
                    _krystal.Name, xk.Expander.Name, _krystal.Level.ToString(), _krystal.MinValue.ToString(), _krystal.MaxValue.ToString());
                SetForExpansionKrystal(xk);
            }
            else if(mk != null)
            {
                this.BasicData.Text = string.Format("Modulation Krystal: {0}   Modulator: {1}   Level: {2}   Range of Values: {3}..{4}",
                    _krystal.Name, mk.Modulator.Name, _krystal.Level.ToString(), _krystal.MinValue.ToString(), _krystal.MaxValue.ToString());
                SetForModulationKrystal(mk);
            }
            else if(pk != null)
            {
                string sortFirstString;
                if(pk.SortFirst)
                    sortFirstString = "true";
                else
                    sortFirstString = "false";

                this.BasicData.Text = string.Format("Permutation Krystal: {0}   Level: {1}   pLevel: {2}   sortFirst: {3}   Range of Values: {4}..{5}",
                    _krystal.Name, _krystal.Level.ToString(), pk.PermutationLevel.ToString(), sortFirstString, _krystal.MinValue.ToString(), _krystal.MaxValue.ToString());
                SetForPermutationKrystal(pk);
            }
            else if(pathK != null)
            {
                throw new NotImplementedException();
            }
            else // _krystal == null
            {
                this.BasicData.Text = "";
                SetForNoKrystal();
            }

            if(_krystal != null)
            {
                if(_selectedTreeView == null || _selectedTreeView.Equals(this._krystalChildrenTreeView) == false)
                    SelectNodeInFamilyTree(_krystal.Name);
                //SetFirstAncestorAppearance();
                Krystal2DTextBox.Lines = Get2DText(_krystal);
                if(_krystalChildrenTreeView.SelectedNode.Nodes.Count > 0) // krystals can only be deleted if they are not used to construct other krystals or scores
                {
                    SetDeleteKrystalButtonVisible = false;
                }
                else
                {
                    SetDeleteKrystalButtonVisible = true;
                }
            }
            this.ResumeLayout();
        }

        private string[] Get2DText(Krystal krystal)
        {
            const int MaxHexValue = 16;

            if(_krystal.Level == 0)
            {
                return new string[] { _krystal.MaxValue.ToString() };
            }
            if(_krystal.MaxValue > MaxHexValue)
            {
                return new string[] { $"Cannot display krystals with a maximum value greater than {MaxHexValue} here." };
            }
            if(_krystal.Name.Contains(K.KrystalType.perm.ToString())) // TODO permutation display with axes.
            {
                MessageBox.Show("TODO: permutation display with axis and contour values.", "Permutation krystal", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            int kLevel = (int)krystal.Level;

            int[] clock = new int[kLevel + 1];
            int[] nextClock = new int[kLevel + 1];
            for(int i = 0; i < clock.Length; i++)
            {
                clock[i] = nextClock[i] = 1;
            }
            List<uint> nextSection = null;
            if(kLevel > 4)
            {
                nextSection = new List<uint>();
                for(int i = 0; i < kLevel - 3; i++)
                {
                    nextSection.Add(1);
                }
            }

            List<string> paragraph = new List<string>(); // a paragraph is a a clock line, followed by a horizontal group of blocks, followed by an empty line.
            List<List<string>> paragraphs = new List<List<string>>();
            List<string> block = new List<string>();  // a block consists of a single list of consecutive strands (usually 7 or less)
            List<List<string>> blocksList = new List<List<string>>();
            for(int k = 0; k < krystal.Strands.Count; k++)
            {
                Strand strand = krystal.Strands[k];

                if(strand.Level <= kLevel - 1 && block.Count > 0)
                {
                    blocksList.Add(block);
                    block = new List<string>();
                }

                if((strand.Level == 1 || strand.Level < kLevel - 1) && blocksList.Count > 0)
                {
                    paragraph = GetParagraph(krystal.Name, nextSection, clock, blocksList);
                    paragraphs.Add(paragraph);
                    for(int i = 0; i < clock.Length; i++)
                    {
                        clock[i] = nextClock[i];
                    }

                    blocksList = new List<List<string>>();
                    block = new List<string>();
                }

                block.Add(K.GetHexString(strand.Values));

                nextClock[nextClock.Length - 1] += strand.Values.Count;
                for(int i = nextClock.Length - 1; i >= strand.Level ; i--)
                {
                    nextClock[i-1] += 1;
                }
                if(kLevel > 4 && strand.Level > 1 && strand.Level <= kLevel - 2)
                {
                    if(strand.Level < kLevel - 2)
                    {
                        int resetlevel = (int)strand.Level - 2;
                        nextSection[resetlevel]++;
                        for(int j = resetlevel + 1; j < nextSection.Count; j++)
                        {
                            nextSection[j] = 1;
                        }
                    }
                    else
                    {
                        for(int j = (int)strand.Level - 2; j < nextSection.Count; j++)
                        {
                            nextSection[j]++;
                        }
                    }
                }
            }

            blocksList.Add(block);
            paragraph = GetParagraph(krystal.Name, nextSection, clock, blocksList);
            paragraphs.Add(paragraph);

            return ParagraphsToText(paragraphs, nextClock);
        }

        private string[] ParagraphsToText(List<List<string>> paragraphs, int[] nextClock)
        {
            List<string> rval = new List<string>();
            foreach(List<string> lines in paragraphs)
            {
                foreach(var line in lines)
                {
                    rval.Add(line);
                }
            }

            List<int> finalClock = new List<int>();
            foreach(var val in nextClock)
            {
                finalClock.Add(val - 1);
            }
            rval.Add(K.ClockToString(finalClock.ToArray()));

            return rval.ToArray();
        }

        /// <summary>
        /// Each string in the returned List is a line of text.
        /// The first line is a representation of the current state of the clock (=shape)
        /// Subsequent lines contain strand values in various blocks.
        /// The final line is empty (i.e. is a carriage return).
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="paragraphBlocks"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private List<string> GetParagraph(string krystalName, List<uint> section, int[] clock, List<List<string>> blocksList)
        {
            var sbLines = new List<StringBuilder>();

            int nParagraphBlockLines = 0;
            foreach(List<string> block in blocksList)
            {
                nParagraphBlockLines = (block.Count > nParagraphBlockLines) ? block.Count : nParagraphBlockLines;
            }
            for(int i = 0; i < nParagraphBlockLines; i++)
            {
                sbLines.Add(new StringBuilder());
            }

            int nBlocks = blocksList.Count;
            const int tab = 15;

            for(int j = 0; j < nBlocks; j++)
            {
                List<string> blockLines = blocksList[j];
                int nBlockLines = blockLines.Count;
                for(int k = 0; k < nBlockLines; k++)
                {
                    sbLines[k].Append(blockLines[k]);
                }
                if(j < nBlocks - 1)
                {
                    int tabstop = tab * (j + 1);
                    for(int k = 0; k < nParagraphBlockLines; k++)
                    {
                        while(sbLines[k].Length < tabstop)
                        {
                            sbLines[k].Append(' ');
                        }
                    }
                }
            }

            string sectionString = "";
            string clockString = K.ClockToString(clock);
            if(section != null)
            {
                int colonIndex = clockString.IndexOf(':');
                clockString = clockString.Remove(0, colonIndex);
                sectionString = K.GetStringOfUnsignedInts(section, '.') + ' ';
            }
            var rval = new List<string> {sectionString + clockString};

            foreach(var sbLine in sbLines)
            {
                rval.Add(sbLine.ToString());
            }

            rval.Add("");

            return rval;
        }

        private void SetForNoKrystal()
        {
            MissingValues.Text = "";
            Shape.Text = "";
            StrandsTreeView.Nodes.Clear();
            TreeNode tn = new TreeNode("  ");
            StrandsTreeView.Nodes.Add(tn);
            Krystal2DTextBox.Text = "";
        }

        private void SetForConstantKrystal()
        {
            MissingValues.Text = "";
            Shape.Text = "";
            StrandsTreeView.Nodes.Clear();
            TreeNode tn = new TreeNode("Value: " + _krystal.MaxValue.ToString());
            StrandsTreeView.Nodes.Add(tn);
        }

        private void SetForLineKrystal()
        {
            MissingValues.Text = "Missing Values:  " + _krystal.MissingValues;
            StringBuilder sb = new StringBuilder();
            int nValues = 0;
            foreach(uint value in _krystal.Strands[0].Values)
            {
                sb.Append(", ");
                sb.Append(value.ToString());
                nValues++;
            }
            Shape.Text = "Number of Values: " + nValues.ToString();

            sb.Remove(0, 2);
            sb.Insert(0, "Values: ");
            StrandsTreeView.Nodes.Clear();
            TreeNode tn = new TreeNode(sb.ToString());
            StrandsTreeView.Nodes.Add(tn);
        }

        private void SetForExpansionKrystal(ExpansionKrystal xk)
        {
            MissingValues.Text = "Missing Values:  " + _krystal.MissingValues;
            Shape.Text = "Shape:  " + _krystal.Shape;

            StrandsTreeView.Nodes.Clear();
            ExpansionTreeView expansionTreeView = new ExpansionTreeView(StrandsTreeView, xk.StrandNodeList(),
                xk.DensityInputKrystal.Level,
                xk.PointsInputKrystal.MissingAbsoluteValues);
            expansionTreeView.DisplayStrands(xk.Strands);
            StrandsTreeView.ExpandAll();
        }

        private void SetForModulationKrystal(ModulationKrystal mk)
        {
            MissingValues.Text = "Missing Values:  " + _krystal.MissingValues;
            Shape.Text = "Shape:  " + _krystal.Shape;

            StrandsTreeView.Nodes.Clear();
            ModulationTreeView modulationTreeView = new ModulationTreeView(StrandsTreeView, mk);
            mk.Modulate();
            modulationTreeView.DisplayModulationResults(mk);
            StrandsTreeView.ExpandAll();
        }

        private void SetForPermutationKrystal(PermutationKrystal pk)
        {
            MissingValues.Text = "Missing Values:  " + _krystal.MissingValues;
            Shape.Text = "Shape:  " + _krystal.Shape;

            StrandsTreeView.Nodes.Clear();
            PermutationTreeView permutationTreeView = new PermutationTreeView(StrandsTreeView, pk);
            //pk.Permute();
            permutationTreeView.DisplayStrands(pk.Strands, pk.PermutationNodeList);
            StrandsTreeView.ExpandAll();
        }

        #region Events
        private void AncestorsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(_previouslySelectedAncestorsNode != null)
            {
                _previouslySelectedAncestorsNode.BackColor = Color.FromKnownColor(System.Drawing.KnownColor.Window);
            }
            var n = e.Node as AncestorsNode;
            if(n != null)
            {
                _previouslySelectedAncestorsNode = n;
                n.BackColor = Color.FromKnownColor(System.Drawing.KnownColor.Highlight);
            }

            if(_ancestorsTreeView.Focused)
            {
                _selectedTreeView = _ancestorsTreeView;
                if(n != null)
                {
                    string filename;
                    if(n.Text.Contains(": "))
                        filename = n.Text.Remove(0, 3);
                    else filename = n.Text;

                    SetForKrystal(filename);
                }
            }
        }

        private void KrystalChildrenTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(_previouslySelectedChildrenNode != null)
            {
                _previouslySelectedChildrenNode.BackColor = Color.FromKnownColor(System.Drawing.KnownColor.Window);
            }
            var n = e.Node as KrystalChildrenNode;
            if(n != null)
            {
                _previouslySelectedChildrenNode = n;
                n.BackColor = Color.FromKnownColor(System.Drawing.KnownColor.Highlight);
            }

            if(_krystalChildrenTreeView.Focused)
            {
                _selectedTreeView = _krystalChildrenTreeView;
                if(n != null)
                {
                    string filename;
                    if(n.Text.Contains(": "))
                    {
                        filename = n.Text.Remove(0, 3);
                    }
                    else filename = n.Text;

                    this.SetForKrystal(filename);
                }
            }
        }
        #endregion Events

        private void SelectNodeInFamilyTree(string filename)
        {
            TreeNode rootNode = null;
            #region find root node
            if(K.IsConstantKrystalFilename(filename))
            {
                foreach(TreeNode tn in _krystalChildrenTreeView.Nodes)
                    if(tn.Text.Equals("Constants"))
                    {
                        rootNode = tn;
                        break;
                    }
            }
            else if(K.IsLineKrystalFilename(filename))
            {
                foreach(TreeNode tn in _krystalChildrenTreeView.Nodes)
                    if(tn.Text.Equals("Lines"))
                    {
                        rootNode = tn;
                        break;
                    }
            }
            else if(K.IsExpansionKrystalFilename(filename))
            {
                foreach(TreeNode tn in _krystalChildrenTreeView.Nodes)
                    if(tn.Text.Equals("Expansions"))
                    {
                        rootNode = tn;
                        break;
                    }
            }
            else if(K.IsModulationKrystalFilename(filename))
            {
                foreach(TreeNode tn in _krystalChildrenTreeView.Nodes)
                    if(tn.Text.Equals("Modulations"))
                    {
                        rootNode = tn;
                        break;
                    }
            }
            else if(K.IsPermutationKrystalFilename(filename))
            {
                foreach(TreeNode tn in _krystalChildrenTreeView.Nodes)
                    if(tn.Text.Equals("Permutations"))
                    {
                        rootNode = tn;
                        break;
                    }
            }
            #endregion find root node
            if(rootNode != null)
            {
                foreach(TreeNode t in rootNode.Nodes)
                {
                    if(t.Text.Contains(filename))
                    {
                        _krystalChildrenTreeView.CollapseAll();
                        _krystalChildrenTreeView.SelectedNode = t;
                        t.Expand();
                        t.EnsureVisible();
                        break;
                    }
                }
            }
        }

        private readonly string _krystalsFolder = M.LocalMoritzKrystalsFolder;

        private KrystalFamily _krystalFamily = null;
        private Krystal _krystal = null;

        private KrystalAncestorsTreeView _ancestorsTreeView = null;
        private KrystalChildrenTreeView _krystalChildrenTreeView = null;

        private AncestorsNode _previouslySelectedAncestorsNode = null;
        private KrystalChildrenNode _previouslySelectedChildrenNode = null;
        private TreeView _selectedTreeView = null;

        private ReturnKrystalDelegate _returnKrystalDelegate = null;
    }
}