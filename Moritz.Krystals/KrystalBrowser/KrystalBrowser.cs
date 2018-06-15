using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Krystals4ObjectLibrary;

namespace Moritz.Krystals
{
    public delegate void KrystalDelegate(Krystal krystal);

    public partial class KrystalBrowser : Form
    {

        /// <summary>
        /// The krystalNameDelegate is called with the current krystal name when this krystalBrowser is closed.
        /// The krystalBrowser opens with no krystal selected .
        /// </summary>
        public KrystalBrowser(string krystalsFolder, KrystalDelegate krystalNameDelegate)
        {
            InitializeComponent();
            _krystal = null;
            _krystalsFolder = krystalsFolder;
            _sendKrystal = krystalNameDelegate;
            InitializeKrystalBrowser();
        }

        /// <summary>
        /// The krystalNameDelegate is called with the current krystal name when this krystalBrowser is closed.
        /// The krystalBrowser opens with the given krystal selected.
        /// </summary>
        public KrystalBrowser(Krystal krystal, string krystalsFolder, KrystalDelegate krystalNameDelegate)
        {
            InitializeComponent();
            _krystal = krystal;
            _krystalsFolder = krystalsFolder;
            _sendKrystal = krystalNameDelegate;
            InitializeKrystalBrowser();
        }

        private void InitializeKrystalBrowser()
        {
            this.Width = 650;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Location = new Point(0, 0);
            this.splitContainer1.SplitterDistance = 190;
            this.splitContainer2.SplitterDistance = this.splitContainer1.SplitterDistance;

            if(_sendKrystal != null)
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
            SetKrystalFamilyTree();
            SetForKrystal(null);
        }  

        private void SetKrystalFamilyTree()
        {
            _krystalFamilyTreeView = new KrystalFamilyTreeView(_krystalFamily.DependencyList);
            _krystalFamilyTreeView.AfterSelect += new TreeViewEventHandler(this.KrystalFamilyTreeView_AfterSelect);
            this.splitContainer1.Panel1.Controls.Add(_krystalFamilyTreeView);
        }

        /// <summary>
        /// The ReturnKrystalButton is shown if _backPanel == null;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReturnKrystalButton_Click(object sender, EventArgs e)
        {
            if(this._sendKrystal != null)
                _sendKrystal(_krystal);
            StrandsTreeView.Nodes.Clear();
            Close();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            StrandsTreeView.Nodes.Clear();
            Close();
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

			LineKrystal lk = _krystal as LineKrystal;
			ExpansionKrystal xk = _krystal as ExpansionKrystal;
            ShapedExpansionKrystal sk = _krystal as ShapedExpansionKrystal;
            ModulationKrystal mk = _krystal as ModulationKrystal;
            PermutationKrystal pk = _krystal as PermutationKrystal;

			if(_krystal is ConstantKrystal ck)
			{
				this.BasicData.Text = string.Format("Constant Krystal: {0}   Level: {1}   Value: {2}",
					_krystal.Name, _krystal.Level.ToString(), _krystal.MaxValue.ToString());
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
			else if(sk != null)
			{
				this.BasicData.Text = string.Format("Shaped Expansion Krystal: {0}   Expander: {1}   Level: {2}   Range of Values: {3}..{4}",
					_krystal.Name, sk.Expander.Name, _krystal.Level.ToString(), _krystal.MinValue.ToString(), _krystal.MaxValue.ToString());
				SetForShapedExpansionKrystal(sk);
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
            }
            this.ResumeLayout();
        }

        private void SetForNoKrystal()
        {
            MissingValues.Text = "";
            Shape.Text = "";
            StrandsTreeView.Nodes.Clear();
            TreeNode tn = new TreeNode("  ");
            StrandsTreeView.Nodes.Add(tn);
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

        private void SetForShapedExpansionKrystal(ShapedExpansionKrystal sk)
        {
            MissingValues.Text = "Missing Values:  " + _krystal.MissingValues;
            Shape.Text = "Shape:  " + _krystal.Shape;

            StrandsTreeView.Nodes.Clear();
            ExpansionTreeView expansionTreeView = new ExpansionTreeView(StrandsTreeView, sk.StrandNodeList(),
                sk.DensityInputKrystal.Level,
                sk.PointsInputKrystal.MissingAbsoluteValues);
            expansionTreeView.DisplayStrands(sk.Strands);
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
            else if(K.IsShapedExpansionKrystalFilename(filename))
            {
                foreach(TreeNode tn in _krystalFamilyTreeView.Nodes)
                    if(tn.Text.Equals("Shaped Expansions"))
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

        private KrystalDelegate _sendKrystal = null;

    }
}