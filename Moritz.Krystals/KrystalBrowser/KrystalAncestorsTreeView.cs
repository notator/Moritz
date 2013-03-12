using System.Collections.Generic;
using System.Windows.Forms;

using Krystals4ObjectLibrary;

namespace Moritz.Krystals
{
	internal class AncestorsNode : TreeNode
	{
		public AncestorsNode(string name, List<Dependency> dependencyList)
		{
			Text = name;
			for(int i = dependencyList.Count - 1 ; i >= 0 ; i--)
			{
				if(dependencyList[i].Name.Equals(Text))
				{
                    if(K.IsExpansionKrystalFilename(Text))
                    {
                        AncestorsNode densityNode = new AncestorsNode(dependencyList[i].Input1, dependencyList);
                        densityNode.Text = densityNode.Text.Insert(0, "d: ");
                        this.Nodes.Add((TreeNode) densityNode);
                        AncestorsNode pointsNode = new AncestorsNode(dependencyList[i].Input2, dependencyList);
                        pointsNode.Text = pointsNode.Text.Insert(0, "p: ");
                        this.Nodes.Add((TreeNode) pointsNode);
                        AncestorsNode fieldNode = new AncestorsNode(dependencyList[i].Field, dependencyList);
                        fieldNode.Text = fieldNode.Text.Insert(0, "e: ");
                        this.Nodes.Add((TreeNode) fieldNode);
                    }
                    if(K.IsShapedExpansionKrystalFilename(Text))
                    {
                        AncestorsNode densityNode = new AncestorsNode(dependencyList[i].Input1, dependencyList);
                        densityNode.Text = densityNode.Text.Insert(0, "d: ");
                        this.Nodes.Add((TreeNode) densityNode);
                        AncestorsNode pointsNode = new AncestorsNode(dependencyList[i].Input2, dependencyList);
                        pointsNode.Text = pointsNode.Text.Insert(0, "p: ");
                        this.Nodes.Add((TreeNode) pointsNode);
                        AncestorsNode axisNode = new AncestorsNode(dependencyList[i].Input3, dependencyList);
                        axisNode.Text = axisNode.Text.Insert(0, "a: ");
                        this.Nodes.Add((TreeNode) axisNode);
                        AncestorsNode contourNode = new AncestorsNode(dependencyList[i].Input4, dependencyList);
                        contourNode.Text = contourNode.Text.Insert(0, "c: ");
                        this.Nodes.Add((TreeNode) contourNode);
                        AncestorsNode fieldNode = new AncestorsNode(dependencyList[i].Field, dependencyList);
                        fieldNode.Text = fieldNode.Text.Insert(0, "e: ");
                        this.Nodes.Add((TreeNode) fieldNode);
                    }
                    else if(K.IsModulationKrystalFilename(Text))
                    {
                        AncestorsNode xNode = new AncestorsNode(dependencyList[i].Input1, dependencyList);
                        xNode.Text = xNode.Text.Insert(0, "x: ");
                        this.Nodes.Add((TreeNode)xNode);
                        AncestorsNode yNode = new AncestorsNode(dependencyList[i].Input2, dependencyList);
                        yNode.Text = yNode.Text.Insert(0, "y: ");
                        this.Nodes.Add((TreeNode)yNode);
                        AncestorsNode fieldNode = new AncestorsNode(dependencyList[i].Field, dependencyList);
                        fieldNode.Text = fieldNode.Text.Insert(0, "m: ");
                        this.Nodes.Add((TreeNode)fieldNode);
                    }
                    else if(K.IsPermutationKrystalFilename(Text))
                    {
                        AncestorsNode sourceNode = new AncestorsNode(dependencyList[i].Input1, dependencyList);
                        sourceNode.Text = sourceNode.Text.Insert(0, "s: ");
                        this.Nodes.Add((TreeNode)sourceNode);
                        AncestorsNode axisNode = new AncestorsNode(dependencyList[i].Input2, dependencyList);
                        axisNode.Text = axisNode.Text.Insert(0, "a: ");
                        this.Nodes.Add((TreeNode)axisNode);
                        AncestorsNode contourNode = new AncestorsNode(dependencyList[i].Input3, dependencyList);
                        contourNode.Text = contourNode.Text.Insert(0, "c: ");
                        this.Nodes.Add((TreeNode)contourNode);
                    }
                    break;
				}
			}
		}
    }


	public class KrystalAncestorsTreeView : TreeView
	{
		public KrystalAncestorsTreeView(Krystal krystal, List<Dependency> dependencyList)
		{
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Location = new System.Drawing.Point(0, 0);

			this.BeginUpdate();
			this.Nodes.Clear();
			AncestorsNode rootNode = null;
			if(krystal == null )
				rootNode = new AncestorsNode("", dependencyList);
			else
				rootNode = new AncestorsNode(krystal.Name, dependencyList); // recursively creates a tree of LineageNodes
			this.Nodes.Add(rootNode);
			this.EndUpdate();
			this.ExpandAll();
		}
	}
}
