using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Krystals4ObjectLibrary;

namespace Moritz.Krystals
{
    internal class KrystalChildrenNode : TreeNode
    {
        public KrystalChildrenNode(string name, KrystalFamily krystalFamily)
        {
            List<Dependency> dependencyList = krystalFamily.DependencyList;
            Dictionary<string, Color> nameColors = krystalFamily.NameColors;

            Text = name;
            ForeColor = krystalFamily.GetNameColor(Text);

            int listIndex;
            List<Dependency> dependencies;

            dependencies = dependencyList.FindAll(d => d.Input1 == Text);
            foreach(var dependency in dependencies)
            {
                KrystalChildrenNode input1Node = new KrystalChildrenNode(dependency.Name, krystalFamily);
                listIndex = FindChildFollower(input1Node.Text, this.Nodes);
                if(K.IsExpansionKrystalFilename(dependency.Name))
                    input1Node.Text = input1Node.Text.Insert(0, "d: "); // density
                else if(K.IsModulationKrystalFilename(dependency.Name))
                    input1Node.Text = input1Node.Text.Insert(0, "x: "); // xInput
                else if(K.IsPermutationKrystalFilename(dependency.Name))
                    input1Node.Text = input1Node.Text.Insert(0, "o: "); // original
                else if(K.IsPathKrystalFilename(dependency.Name))
                    input1Node.Text = input1Node.Text.Insert(0, "s: "); /// svg

                this.Nodes.Insert(listIndex, (TreeNode)input1Node);
            }

            dependencies = dependencyList.FindAll(d => d.Input2 == Text);
            foreach(var dependency in dependencies)
            {
                KrystalChildrenNode input2Node = new KrystalChildrenNode(dependency.Name, krystalFamily);
                listIndex = FindChildFollower(input2Node.Text, this.Nodes);
                if(K.IsExpansionKrystalFilename(dependency.Name))
                    input2Node.Text = input2Node.Text.Insert(0, "p: "); // points input
                else if(K.IsModulationKrystalFilename(dependency.Name))
                    input2Node.Text = input2Node.Text.Insert(0, "y: "); // yInput
                else if(K.IsPermutationKrystalFilename(dependency.Name))
                    input2Node.Text = input2Node.Text.Insert(0, "a: "); // axis
                else if(K.IsPathKrystalFilename(dependency.Name))
                    input2Node.Text = input2Node.Text.Insert(0, "d: "); // density

                this.Nodes.Insert(listIndex, (TreeNode)input2Node);
            }

            dependencies = dependencyList.FindAll(d => d.Input3 == Text);
            foreach(var dependency in dependencies)
            {
                KrystalChildrenNode input3Node = new KrystalChildrenNode(dependency.Name, krystalFamily);
                listIndex = FindChildFollower(input3Node.Text, this.Nodes);
                if(K.IsPermutationKrystalFilename(dependency.Name))
                    input3Node.Text = input3Node.Text.Insert(0, "c: "); // contour

                this.Nodes.Insert(listIndex, (TreeNode)input3Node);
            }

            dependencies = dependencyList.FindAll(d => d.Input4 == Text);
            foreach(var dependency in dependencies)
            {
                KrystalChildrenNode input4Node = new KrystalChildrenNode(dependency.Name, krystalFamily);
                listIndex = FindChildFollower(input4Node.Text, this.Nodes);
                this.Nodes.Insert(listIndex, (TreeNode)input4Node);
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
        public KrystalFamilyTreeView(KrystalFamily krystalFamily, int? domainFilter, List<int> shapeListFilter)
        {
            bool DisplayCompatibleShape(KrystalChildrenNode node, List<int> filter)
            {
                if(filter == null || filter[0] == 0)
                {
                    return true;
                }
                List<int> nodeShape = K.GetShapeFromKrystalName(node.Text);
                if(nodeShape.Count > filter.Count)
                {
                    return false;
                }
                for(int i = 0; i < nodeShape.Count; i++)
                {
                    if(nodeShape[i] != filter[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            bool DisplayCompatibleDomain(KrystalChildrenNode node, int? filter)
            {
                if(filter == null)
                {
                    return true; // display everything
                }

                int domain = K.GetDomainFromFirstComponent(node.Text);

                return (domain <= (int) filter);
            }

            TreeNode constantRoot, lineRoot, expansionRoot, modulationRoot, permutationRoot, pathRoot;
            InitializeKrystalFamilyTreeView(out constantRoot, out lineRoot, out expansionRoot, out modulationRoot, out permutationRoot, out pathRoot);

            int index;
            for(int i = 0; i < krystalFamily.DependencyList.Count; i++)
            {
                string name = krystalFamily.DependencyList[i].Name;
                KrystalChildrenNode rootNode = new KrystalChildrenNode(name, krystalFamily);
                if(DisplayCompatibleShape(rootNode, shapeListFilter) && DisplayCompatibleDomain(rootNode, domainFilter))
                {
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
                    else if(K.IsPathKrystalFilename(rootNode.Text))
                    {
                        index = FindFollower(rootNode.Text, pathRoot.Nodes);
                        pathRoot.Nodes.Insert(index, rootNode);
                    }
                }
            }
            this.EndUpdate();
            this.CollapseAll();
        }

        private void InitializeKrystalFamilyTreeView(out TreeNode constantRoot, out TreeNode lineRoot, out TreeNode expansionRoot, out TreeNode modulationRoot, out TreeNode permutationRoot, out TreeNode pathRoot)
        {
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Location = new System.Drawing.Point(0, 0);

            this.BeginUpdate();
            this.Nodes.Clear();
            constantRoot = new TreeNode("Constants");
            lineRoot = new TreeNode("Lines");
            expansionRoot = new TreeNode("Expansions");
            modulationRoot = new TreeNode("Modulations");
            permutationRoot = new TreeNode("Permutations");
            pathRoot = new TreeNode("Paths");
            this.Nodes.Add(constantRoot);
            this.Nodes.Add(lineRoot);
            this.Nodes.Add(expansionRoot);
            this.Nodes.Add(modulationRoot);
            this.Nodes.Add(permutationRoot);
            this.Nodes.Add(pathRoot);
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
