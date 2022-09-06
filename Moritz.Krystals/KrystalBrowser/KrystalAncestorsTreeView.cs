using System.Collections.Generic;
using System.Windows.Forms;

using Krystals4ObjectLibrary;

namespace Moritz.Krystals
{
	internal class AncestorsNode : TreeNode
	{
		public AncestorsNode(string name, KrystalFamily krystalFamily)
		{
            List<Dependency> dependencyList = krystalFamily.DependencyList;
            var nameColors = krystalFamily.NameColors;

            Text = name;
            ForeColor = krystalFamily.GetNameColor(Text);

            Dependency dependency = dependencyList.Find(d => d.Name == Text);

            if(K.IsExpansionKrystalFilename(Text))
            {
                AncestorsNode densityNode = new AncestorsNode(dependency.Input1, krystalFamily);
                densityNode.Text = densityNode.Text.Insert(0, "d: ");
                densityNode.ForeColor = krystalFamily.GetNameColor(dependency.Input1);
                this.Nodes.Add((TreeNode) densityNode);
                AncestorsNode pointsNode = new AncestorsNode(dependency.Input2, krystalFamily);
                pointsNode.Text = pointsNode.Text.Insert(0, "p: ");
                pointsNode.ForeColor = krystalFamily.GetNameColor(dependency.Input2);
                this.Nodes.Add((TreeNode) pointsNode);
                AncestorsNode fieldNode = new AncestorsNode(dependency.Field, krystalFamily);
                fieldNode.Text = fieldNode.Text.Insert(0, "e: ");
                fieldNode.ForeColor = krystalFamily.GetNameColor(dependency.Field);
                this.Nodes.Add((TreeNode) fieldNode);
            }
            else if(K.IsModulationKrystalFilename(Text))
            {
                AncestorsNode xNode = new AncestorsNode(dependency.Input1, krystalFamily);
                xNode.Text = xNode.Text.Insert(0, "x: ");
                xNode.ForeColor = krystalFamily.GetNameColor(dependency.Input1);
                this.Nodes.Add((TreeNode)xNode);
                AncestorsNode yNode = new AncestorsNode(dependency.Input2, krystalFamily);
                yNode.Text = yNode.Text.Insert(0, "y: ");
                yNode.ForeColor = krystalFamily.GetNameColor(dependency.Input2);
                this.Nodes.Add((TreeNode)yNode);
                AncestorsNode fieldNode = new AncestorsNode(dependency.Field, krystalFamily);
                fieldNode.Text = fieldNode.Text.Insert(0, "m: ");
                fieldNode.ForeColor = krystalFamily.GetNameColor(dependency.Field);
                this.Nodes.Add((TreeNode)fieldNode);
            }
            else if(K.IsPermutationKrystalFilename(Text))
            {
                AncestorsNode sourceNode = new AncestorsNode(dependency.Input1, krystalFamily);
                sourceNode.Text = sourceNode.Text.Insert(0, "s: ");
                sourceNode.ForeColor = krystalFamily.GetNameColor(dependency.Input1);
                this.Nodes.Add((TreeNode)sourceNode);
                AncestorsNode axisNode = new AncestorsNode(dependency.Input2, krystalFamily);
                axisNode.Text = axisNode.Text.Insert(0, "a: ");
                axisNode.ForeColor = krystalFamily.GetNameColor(dependency.Input2);
                this.Nodes.Add((TreeNode)axisNode);
                AncestorsNode contourNode = new AncestorsNode(dependency.Input3, krystalFamily);
                contourNode.Text = contourNode.Text.Insert(0, "c: ");
                contourNode.ForeColor = krystalFamily.GetNameColor(dependency.Input3);
                this.Nodes.Add((TreeNode)contourNode);
            }

		}
    }


	public class KrystalAncestorsTreeView : TreeView
	{
        public KrystalAncestorsTreeView(Krystal krystal, KrystalFamily krystalFamily)
        {
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Location = new System.Drawing.Point(0, 0);

            this.BeginUpdate();
            this.Nodes.Clear();

            AncestorsNode rootNode = null;
            if(krystal != null)
            {
                Dependency dependency = krystalFamily.DependencyList.Find(d => d.Name == krystal.Name);
                if(dependency.ScoreNames.Count > 0)
                {
                    List<TreeNode> childNodes = new List<TreeNode>();
                    foreach(var scoreName in dependency.ScoreNames)
                    {
                        childNodes.Add(new TreeNode(scoreName));
                    }
                    this.Nodes.Add(new TreeNode("Scores", childNodes.ToArray()));
                }
                rootNode = new AncestorsNode(krystal.Name, krystalFamily); // recursively creates a tree of AncestorNodes
            }
            else
            {
                rootNode = new AncestorsNode("", krystalFamily);
            }

			this.Nodes.Add(rootNode);
			this.EndUpdate();
			this.ExpandAll();
		}
	}
}
