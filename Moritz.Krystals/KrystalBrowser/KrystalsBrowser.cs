using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Krystals4ObjectLibrary;

using Moritz.Globals;

namespace Moritz.Krystals
{
    public delegate void KrystalDelegate(Krystal krystal);

    public partial class KrystalsBrowser : Form
    {
        /// <summary>
        /// The returnKrystalNameDelegate is called with the current krystal name when this krystalBrowser is closed.
        /// If krystal != null, the krystalBrowser opens with the given krystal selected.
        /// </summary>
        public KrystalsBrowser()
        {
            InitializeComponent();
            this.Text = "All Krystals";
            _krystalsFolder = M.LocalMoritzKrystalsFolder;
            _returnKrystalNameDelegate = null;
            InitializeKrystalBrowser(null, null);
        }

        /// <summary>
        /// The returnKrystalNameDelegate is called with the current krystal name when this krystalBrowser is closed.
        /// </summary>
        public KrystalsBrowser(string title, KrystalDelegate returnKrystalNameDelegate)
        {
            InitializeComponent();
            this.Text = title;
            ControlBox = false;
            _krystalsFolder = M.LocalMoritzKrystalsFolder;
            _returnKrystalNameDelegate = returnKrystalNameDelegate;
            InitializeKrystalBrowser(null, null); // display all krystals
        }

        /// <summary>
        /// Only Krystals whose shape is contained in the shapeListFilter are included in the displayed KrystaFamilyTree. 
        /// The returnKrystalNameDelegate is called with the current krystal name when this krystalBrowser is closed.
        /// </summary>
        public KrystalsBrowser(string title, int? domainFilter, List<int> shapeListFilter, KrystalDelegate returnKrystalNameDelegate)
        {
            InitializeComponent();
            this.Text = title;
            ControlBox = false;
            _krystalsFolder = M.LocalMoritzKrystalsFolder;
            _returnKrystalNameDelegate = returnKrystalNameDelegate;
            InitializeKrystalBrowser(domainFilter, shapeListFilter);
        }

        /// <summary>
        /// The KrystalBrowser opens with the selectKrystal selected. 
        /// The returnKrystalNameDelegate is called with the current krystal name when this krystalBrowser is closed.
        /// </summary>
        public KrystalsBrowser(string title, Krystal selectKrystal, KrystalDelegate returnKrystalNameDelegate)
        {
            InitializeComponent();
            this.Text = title;
            ControlBox = false;
            _krystalsFolder = M.LocalMoritzKrystalsFolder;
            _returnKrystalNameDelegate = returnKrystalNameDelegate;
            _krystal = selectKrystal;
            RenameKrystalButton.Visible = true;
            InitializeKrystalBrowser(null, null);
        }

        private void InitializeKrystalBrowser(int? domainFilter, List<int> shapeListFilter)
        {
            this.Width = Screen.PrimaryScreen.WorkingArea.Width - 100;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Location = new Point(0, 0);
            this.splitContainer1.SplitterDistance = Width / 6;
            this.splitContainer2.SplitterDistance = Width / 6;
            this.splitContainer3.SplitterDistance = Width / 6;
            this.Krystal2DTextBox.Text = "";

            if(_returnKrystalNameDelegate != null)
            {
                this.ReturnKrystalButton.Show();
                this.CloseButton.Hide();
            }
            else
            {
                this.ReturnKrystalButton.Hide();
                this.CloseButton.Show();
            }

            _krystalFamily = new KrystalFamily(this._krystalsFolder);

            SetKrystalFamilyTree(domainFilter, shapeListFilter);
            SetForKrystal(null);
        }  

        private void SetKrystalFamilyTree(int? domainFilter, List<int> shapeListFilter)
        {
            _krystalFamilyTreeView = new KrystalFamilyTreeView(_krystalFamily.DependencyList, domainFilter, shapeListFilter);
            _krystalFamilyTreeView.AfterSelect += new TreeViewEventHandler(this.KrystalFamilyTreeView_AfterSelect);
            this.splitContainer1.Panel1.Controls.Add(_krystalFamilyTreeView);
        }

        private void RenameKrystalButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The ReturnKrystalButton is shown if _backPanel == null;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReturnKrystalButton_Click(object sender, EventArgs e)
        {
            if(this._returnKrystalNameDelegate != null)
                _returnKrystalNameDelegate(_krystal);
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            //StrandsTreeView.Nodes.Clear();
            Hide();
        }

        public void SetForKrystal(string filename)
        {
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
            _ancestorsTreeView = new KrystalAncestorsTreeView(_krystal, _krystalFamily.DependencyList);
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
                if(_selectedTreeView == null || _selectedTreeView.Equals(this._krystalFamilyTreeView) == false)
                    SelectNodeInFamilyTree(_krystal.Name);
                SetFirstAncestorAppearance();
                Krystal2DTextBox.Lines = Get2DText(_krystal);
                RenameKrystalButton.Visible = true;
            }
            this.ResumeLayout();
        }

        private string[] Get2DText(Krystal krystal)
        {
            if(_krystal.Level == 0)
            {
                return new string[] { _krystal.MaxValue.ToString() };
            }

            int kLevel = (int)krystal.Level;

            int[] clock = new int[kLevel + 1];
            int[] nextClock = new int[kLevel + 1];
            for(int i = 0; i < clock.Length; i++)
            {
                clock[i] = nextClock[i] = 1;
            }
            List<uint> section = null;
            List<uint> nextSection = null;
            if(kLevel > 4)
            {
                section = new List<uint>();
                nextSection = new List<uint>();
                for(int i = 0; i < kLevel - 3; i++)
                {
                    section.Add(1);
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
                    paragraph = GetParagraph(krystal.Name, section, clock, blocksList);
                    paragraphs.Add(paragraph);
                    for(int i = 0; i < clock.Length; i++)
                    {
                        clock[i] = nextClock[i];
                    }
                    if(kLevel > 4)
                    {
                        for(int i = 0; i < section.Count; i++)
                        {
                            section[i] = nextSection[i];
                        }
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
                if(kLevel > 4)
                {
                    Console.WriteLine("Strand=" + strand.ToString());
                    if(strand.Level < 3)
                    {
                        nextSection[0]++;
                        for(int j = 1; j < nextSection.Count; j++)
                        {
                            nextSection[j] = 1;
                        }
                    }
                    else if(strand.Level <= kLevel - 2)
                    {
                        for(int j = (int)strand.Level - 2; j < nextSection.Count; j++)
                        {
                            nextSection[j]++;
                        }
                    }
                }
            }

            blocksList.Add(block);
            paragraph = GetParagraph(krystal.Name, section, clock, blocksList);
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

            if(!krystalName.Contains(K.KrystalType.perm.ToString())) // TODO permutation display with axes.
            {
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
                _previouslySelectedAncestorsNode.ForeColor = Color.FromKnownColor(System.Drawing.KnownColor.Black);
            }

            AncestorsNode n = e.Node as AncestorsNode;
            _previouslySelectedAncestorsNode = n;
            n.BackColor = Color.FromKnownColor(System.Drawing.KnownColor.Highlight);
            n.ForeColor = Color.FromKnownColor(System.Drawing.KnownColor.Window);

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

        private void KrystalFamilyTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(_previouslySelectedChildrenNode != null)
            {
                _previouslySelectedChildrenNode.BackColor = Color.FromKnownColor(System.Drawing.KnownColor.Window);
                _previouslySelectedChildrenNode.ForeColor = Color.FromKnownColor(System.Drawing.KnownColor.Black);
            }

            KrystalChildrenNode n = e.Node as KrystalChildrenNode;
            _previouslySelectedChildrenNode = n;

            if(n != null)
            {
                n.BackColor = Color.FromKnownColor(System.Drawing.KnownColor.Highlight);
                n.ForeColor = Color.FromKnownColor(System.Drawing.KnownColor.Window);
            }

            if(_krystalFamilyTreeView.Focused)
            {
                _selectedTreeView = _krystalFamilyTreeView;
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
                foreach(TreeNode tn in _krystalFamilyTreeView.Nodes)
                    if(tn.Text.Equals("Constants"))
                    {
                        rootNode = tn;
                        break;
                    }
            }
            else if(K.IsLineKrystalFilename(filename))
            {
                foreach(TreeNode tn in _krystalFamilyTreeView.Nodes)
                    if(tn.Text.Equals("Lines"))
                    {
                        rootNode = tn;
                        break;
                    }
            }
            else if(K.IsExpansionKrystalFilename(filename))
            {
                foreach(TreeNode tn in _krystalFamilyTreeView.Nodes)
                    if(tn.Text.Equals("Expansions"))
                    {
                        rootNode = tn;
                        break;
                    }
            }
            else if(K.IsModulationKrystalFilename(filename))
            {
                foreach(TreeNode tn in _krystalFamilyTreeView.Nodes)
                    if(tn.Text.Equals("Modulations"))
                    {
                        rootNode = tn;
                        break;
                    }
            }
            else if(K.IsPermutationKrystalFilename(filename))
            {
                foreach(TreeNode tn in _krystalFamilyTreeView.Nodes)
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
                        _krystalFamilyTreeView.CollapseAll();
                        _krystalFamilyTreeView.SelectedNode = t;
                        t.Expand();
                        t.EnsureVisible();
                        break;
                    }
                }
            }
        }

        private void SetFirstAncestorAppearance()
        {
            if(_previouslySelectedAncestorsNode != null)
            {
                _previouslySelectedAncestorsNode.BackColor = Color.FromKnownColor(System.Drawing.KnownColor.Window);
                _previouslySelectedAncestorsNode.ForeColor = Color.FromKnownColor(System.Drawing.KnownColor.Black);
            }

			if(this._ancestorsTreeView.Nodes[0] is AncestorsNode n)
			{
				_previouslySelectedAncestorsNode = n;
				n.BackColor = Color.FromKnownColor(System.Drawing.KnownColor.Highlight);
				n.ForeColor = Color.FromKnownColor(System.Drawing.KnownColor.Window);
			}
		}

        private KrystalFamily _krystalFamily = null;
        private Krystal _krystal = null;
        private string _krystalsFolder = null;

        private KrystalAncestorsTreeView _ancestorsTreeView = null;
        private KrystalFamilyTreeView _krystalFamilyTreeView = null;

        private AncestorsNode _previouslySelectedAncestorsNode = null;
        private KrystalChildrenNode _previouslySelectedChildrenNode = null;
        private TreeView _selectedTreeView = null;

        private KrystalDelegate _returnKrystalNameDelegate = null;
    }
}