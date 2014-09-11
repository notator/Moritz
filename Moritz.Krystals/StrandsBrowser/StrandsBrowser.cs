using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Krystals4ObjectLibrary;

namespace Moritz.Krystals
{
    public partial class StrandsBrowser : Form
    {
        public StrandsBrowser(Krystal krystal, Point location)
		{
			InitializeComponent();
            this.Text = krystal.Name;
            _krystal = krystal;

            StrandsTreeView.Nodes.Clear();

            if(krystal is LineKrystal)
                SetForLineKrystal((LineKrystal) krystal);
            else if(krystal is ModulationKrystal)
                SetForModulationKrystal((ModulationKrystal)krystal);
            else if(krystal is PermutationKrystal)
                SetForPermutationKrystal((PermutationKrystal)krystal);
            else if(krystal is ExpansionKrystal || krystal is ShapedExpansionKrystal)
                SetForExpansionKrystal((ExpansionKrystalBase) krystal);

            this.Location = location;
  		}

        public string KrystalName { get { return _krystal.Name; } }

        private void SetForLineKrystal(LineKrystal lineKrystal)
        {
            this.Controls.Remove(StrandsTreeView);
            StringBuilder sb = new StringBuilder();
            int nValues = 0;
            foreach(uint value in lineKrystal.Strands[0].Values)
            {
                sb.Append(", ");
                sb.Append(value.ToString());
                nValues++;
            }
            sb.Remove(0, 2);
            sb.Insert(0, "Line: ");
            LineStrandLabel.Text = sb.ToString();
            StrandsTreeView.Nodes.Clear();
            TreeNode tn = new TreeNode(LineStrandLabel.Text);
            StrandsTreeView.Nodes.Add(tn);
            this.Width = LineStrandLabel.Width + 14;
            this.Height = 48;
        }

        private void SetForExpansionKrystal(ExpansionKrystalBase xk)
        {
            this.Controls.Remove(LineStrandLabel);
            ExpansionTreeView expansionTreeView = new ExpansionTreeView(StrandsTreeView, xk.StrandNodeList(),
                xk.DensityInputKrystal.Level,
                xk.PointsInputKrystal.MissingAbsoluteValues);
            expansionTreeView.DisplayStrands(xk.Strands);
            StrandsTreeView.ExpandAll();
            this.Height = Screen.GetWorkingArea(this).Height;
        }
        private void SetForPermutationKrystal(PermutationKrystal pk)
        {
            this.Controls.Remove(LineStrandLabel);
            PermutationTreeView permutationTreeView = new PermutationTreeView(StrandsTreeView, pk);
            permutationTreeView.DisplayStrands(pk.Strands, pk.PermutationNodeList);
            StrandsTreeView.ExpandAll();
            this.Height = Screen.GetWorkingArea(this).Height;
        }
        private void SetForModulationKrystal(ModulationKrystal mk)
        {
            this.Controls.Remove(LineStrandLabel);
            ModulationTreeView modulationTreeView = new ModulationTreeView(StrandsTreeView, mk);
            mk.Modulate();
            modulationTreeView.DisplayModulationResults(mk);
            StrandsTreeView.ExpandAll();
            this.Height = Screen.GetWorkingArea(this).Height;
        }

        private Krystal _krystal;
    }
}
