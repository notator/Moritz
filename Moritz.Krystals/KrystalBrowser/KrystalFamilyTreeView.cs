using System.Collections.Generic;
using System.Windows.Forms;

using Krystals4ObjectLibrary;

namespace Moritz.Krystals
{
    internal class KrystalChildrenNode : TreeNode
    {
        public KrystalChildrenNode(int index, List<Dependency> dependencyList)
        {
            Text = dependencyList[index].Name;
            for(int i = index; i < dependencyList.Count; i++)
            {
                int listIndex;
                if(dependencyList[i].Input1 != null && dependencyList[i].Input1.Equals(Text))
                {
                    KrystalChildrenNode input1Node = new KrystalChildrenNode(i, dependencyList);
                    listIndex = FindChildFollower(input1Node.Text, this.Nodes);
                    if(K.IsExpansionKrystalFilename(dependencyList[i].Name)
                    || K.IsShapedExpansionKrystalFilename(dependencyList[i].Name))
                        input1Node.Text = input1Node.Text.Insert(0, "d: ");
                    else if(K.IsModulationKrystalFilename(dependencyList[i].Name))
                        input1Node.Text = input1Node.Text.Insert(0, "x: ");
                    else if(K.IsPermutationKrystalFilename(dependencyList[i].Name))
                        input1Node.Text = input1Node.Text.Insert(0, "s: ");

                    this.Nodes.Insert(listIndex, (TreeNode)input1Node);
                }
                if(dependencyList[i].Input2 != null && dependencyList[i].Input2.Equals(Text))
                {
                    KrystalChildrenNode input2Node = new KrystalChildrenNode(i, dependencyList);
                    listIndex = FindChildFollower(input2Node.Text, this.Nodes);
                    if(K.IsExpansionKrystalFilename(dependencyList[i].Name)
                    || K.IsShapedExpansionKrystalFilename(dependencyList[i].Name))
                        input2Node.Text = input2Node.Text.Insert(0, "p: ");
                    else if(K.IsModulationKrystalFilename(dependencyList[i].Name))
                        input2Node.Text = input2Node.Text.Insert(0, "y: ");
                    else if(K.IsPermutationKrystalFilename(dependencyList[i].Name))
                        input2Node.Text = input2Node.Text.Insert(0, "a: ");

                    this.Nodes.Insert(listIndex, (TreeNode)input2Node);
                }
                if(dependencyList[i].Input3 != null && dependencyList[i].Input3.Equals(Text))
                {
                    KrystalChildrenNode input3Node = new KrystalChildrenNode(i, dependencyList);
                    listIndex = FindChildFollower(input3Node.Text, this.Nodes);
                    if(K.IsShapedExpansionKrystalFilename(dependencyList[i].Name))
                        input3Node.Text = input3Node.Text.Insert(0, "a: ");
                    else if(K.IsPermutationKrystalFilename(dependencyList[i].Name))
                        input3Node.Text = input3Node.Text.Insert(0, "c: ");

                    this.Nodes.Insert(listIndex, (TreeNode)input3Node);
                }
                if(dependencyList[i].Input4 != null && dependencyList[i].Input4.Equals(Text))
                {
                    KrystalChildrenNode input4Node = new KrystalChildrenNode(i, dependencyList);
                    listIndex = FindChildFollower(input4Node.Text, this.Nodes);
                    if(K.IsShapedExpansionKrystalFilename(dependencyList[i].Name))
                        input4Node.Text = input4Node.Text.Insert(0, "c: ");

                    this.Nodes.Insert(listIndex, (TreeNode)input4Node);
                }
            }
        }
        /// <summary>
        /// Returns the index of the first node in nodes whose Text ends with a krystal name greater than
        /// the first argument. "Greater than" is defined by K.CompareKrystalNames().
        /// If the TreeNodeCollection is empty, this function returns 0.
        /// If none of the nodes' Text fields contains a krystal name greater than krystalName, this function
        /// returns childNodes.Count.
        /// </summary>
        public int FindChildFollower(string krystalName, TreeNodeCollection childNodes)
        {
            if(childNodes.Count == 0)
                return 0;
            else
            {
                int returnValue = childNodes.Count;
                for(int index = 0; index < childNodes.Count; index++)
                {
                    string krystalName2 = childNodes[index].Text;

                    if(krystalName2.IndexOf(":") == 1)
                    {
                        krystalName2 = krystalName2.Substring(3);
                    }

                    if(K.CompareKrystalNames(krystalName, krystalName2) < 0)
                    {
                        returnValue = index;
                        break;
                    }
                }
                return returnValue;
            }
        }

    }

	public class KrystalFamilyTreeView : TreeView
	{
		public KrystalFamilyTreeView(List<Dependency> dependencyList)
		{
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Location = new System.Drawing.Point(0, 0);

			this.BeginUpdate();
			this.Nodes.Clear();
			TreeNode constantRoot = new TreeNode("Constants");
			TreeNode lineRoot = new TreeNode("Lines");
            TreeNode expansionRoot = new TreeNode("Expansions");
            TreeNode shapedExpansionRoot = new TreeNode("Shaped Expansions");
            TreeNode modulationRoot = new TreeNode("Modulations");
            TreeNode permutationRoot = new TreeNode("Permutations");
            this.Nodes.Add(constantRoot);
			this.Nodes.Add(lineRoot);
            this.Nodes.Add(expansionRoot);
            this.Nodes.Add(shapedExpansionRoot);
            this.Nodes.Add(modulationRoot);
            this.Nodes.Add(permutationRoot);

            int index = 0;
			for(int i = 0 ; i < dependencyList.Count ; i++)
			{
                KrystalChildrenNode rootNode = new KrystalChildrenNode(i, dependencyList);
                // rootNode contains a tree of KrystalChildrenNodes
                if(K.IsConstantKrystalFilename(rootNode.Text))
                {
                    index = FindFollower(rootNode.Text, constantRoot.Nodes);
                    constantRoot.Nodes.Insert(index, rootNode);
                }
                else if(K.IsLineKrystalFilename(rootNode.Text))
                {
                    index = FindFollower(rootNode.Text, lineRoot.Nodes);
                    lineRoot.Nodes.Insert(index, rootNode);
                }
                else if(K.IsExpansionKrystalFilename(rootNode.Text))
                {
                    index = FindFollower(rootNode.Text, expansionRoot.Nodes);
                    expansionRoot.Nodes.Insert(index, rootNode);
                }
                else if(K.IsShapedExpansionKrystalFilename(rootNode.Text))
                {
                    index = FindFollower(rootNode.Text, shapedExpansionRoot.Nodes);
                    shapedExpansionRoot.Nodes.Insert(index, rootNode);
                }
                else if(K.IsModulationKrystalFilename(rootNode.Text))
                {
                    index = FindFollower(rootNode.Text, modulationRoot.Nodes);
                    modulationRoot.Nodes.Insert(index, rootNode);
                }
                else if(K.IsPermutationKrystalFilename(rootNode.Text))
                {
                    index = FindFollower(rootNode.Text, permutationRoot.Nodes);
                    permutationRoot.Nodes.Insert(index, rootNode);
                }
            }
			this.EndUpdate();
			this.CollapseAll();
		}
        /// <summary>
        /// Returns the index of the first node in nodes whose Text (a krystal name) is greater than
        /// the first argument (krystalName). "Greater than" is defined by K.CompareKrystalNames().
        /// If the TreeNodeCollection is empty, this function returns 0.
        /// If none of the nodes' Text fields is greater than krystalName, this function returns nodes.Count.
        /// </summary>
        public int FindFollower(string krystalName, TreeNodeCollection nodes)
        {
            if(nodes.Count == 0)
                return 0;
            else
            {
                int returnValue = nodes.Count;
                for(int index = 0 ; index < nodes.Count ; index++)
                {
                    if(K.CompareKrystalNames(krystalName, nodes[index].Text) < 0)
                    {
                        returnValue = index;
                        break;
                    }
                }
                return returnValue;
            }
        }
	}
}
