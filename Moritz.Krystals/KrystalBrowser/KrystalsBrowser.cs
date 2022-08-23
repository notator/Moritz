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
                Krystal2DTextBox.Text = Get2DText(_krystal);
                RenameKrystalButton.Visible = true;
            }
            this.ResumeLayout();
        }

        private string Get2DText(Krystal krystal)
        {
            var sb = new StringBuilder();

            int kLevel = (int)krystal.Level;
            List<List<string>> allBlocks = new List<List<string>>();
            List<string> block = new List<string>(); // a block consists of a single list of consecutive strands (usually 7 or less)
            allBlocks.Add(block);
            foreach(var strand in krystal.Strands)
            {
                if(strand.Level > 1 && strand.Level < kLevel)
                {
                    allBlocks.Add(block);
                    block = new List<string>();
                }
                block.Add(K.GetStringOfUnsignedInts(strand.Values));
            }

            return sb.ToString();
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