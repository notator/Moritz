using System.Collections.Generic;
using System.Windows.Forms;

namespace Krystals5ObjectLibrary
{
	public class PermutationTreeView
	{
		public PermutationTreeView(TreeView treeView, PermutationKrystal pk)
		{
			_treeView = treeView;
            int dKrystalLevel = (int)pk.Level - 1;

			#region line krystal inputs
            if(dKrystalLevel == 1) // line krystal
			{
				_treeView.BeginUpdate();

				foreach(PermutationNode permutationNode in pk.PermutationNodeList)
				{
                    permutationNode.Text =
                            ": m1, a" + permutationNode.Axis.ToString() +
                            ", c" + permutationNode.Contour.ToString();
					_treeView.Nodes.Add(permutationNode);
				}
				_treeView.EndUpdate();
			}
			#endregion line krystal inputs
			else
			#region higher level krystal inputs
			{
				// Construct the levels of the tree above the permutationNodeList, adding the PermutationNodes where
				// necessary. (The upper levels consist of pure TreeNodes and include the single root node.) 
				TreeNode[] currentNode = new TreeNode[dKrystalLevel];
				int[] localSectionNumber = new int[dKrystalLevel];// does not include the local strand section numbers
				int localStrandSectionNumber = 0;
                for(int i = 0; i < pk.Strands.Count; i++)
                {
                    Strand strand = pk.Strands[i];
                    PermutationNode permutationNode = pk.PermutationNodeList[i];
					if(strand.Level <= dKrystalLevel)
					{
						localStrandSectionNumber = 0;
                        int levelIndex = (int) strand.Level - 1;
						while(levelIndex < dKrystalLevel)
						{
							TreeNode tn = new TreeNode();
							tn.Expand();
							currentNode[levelIndex] = tn;
							if(levelIndex > 0) // there is only one node at levelIndex == 0, and it has no text
							{
								currentNode[levelIndex - 1].Nodes.Add(tn);
								localSectionNumber[levelIndex - 1]++;
								localSectionNumber[levelIndex] = 0;
								if(levelIndex == 1)
									tn.Text = localSectionNumber[levelIndex - 1].ToString();
								else
									tn.Text = tn.Parent.Text + "." + localSectionNumber[levelIndex - 1].ToString();
							}
							levelIndex++;
						}
					}
					localStrandSectionNumber++;
					currentNode[dKrystalLevel - 1].Nodes.Add(permutationNode);
					localSectionNumber[dKrystalLevel - 1]++;
					int moment = i + 1;
                    int sourceMoment = permutationNode.SourceStrandNumber;
					int axis = permutationNode.Axis;
					int contour = permutationNode.Contour;
                    permutationNode.Text = permutationNode.Parent.Text
                        + "." + localStrandSectionNumber.ToString()
                        + ": m" + moment.ToString();
                    if(sourceMoment > 0)
                    {
                        permutationNode.Text = permutationNode.Text +
                            ", sm" + sourceMoment.ToString();
                    }
                    else
                    {
                        permutationNode.Text = permutationNode.Text +
                            ", a" + axis.ToString() +
                            ", c" + contour.ToString();
                    }
				}
				// Now add the moment numbers to the pure TreeNode.Texts
                for(int i = 0; i < pk.Strands.Count; i++)
                {
                    Strand strand = pk.Strands[i];
                    PermutationNode permutationNode = pk.PermutationNodeList[i];
                    if(strand.Level <= dKrystalLevel)
					{
						TreeNode tn = permutationNode.Parent;
						bool continueUp = true;
						while(continueUp)
						{
							if(tn.Text.EndsWith(".1") && tn.Level > 0)
								continueUp = true;
							else continueUp = false;
							int m = i + 1;
							tn.Text = tn.Text + ": m" + m.ToString();
							if(continueUp)
								tn = tn.Parent;
						}
					}
				}
				_treeView.BeginUpdate();
				_treeView.Nodes.Clear();
				foreach(TreeNode n in currentNode[0].Nodes)
				{
					_treeView.Nodes.Add(n);
				}
				_treeView.EndUpdate();
			}
			#endregion higher level krystal inputs
		}

        #region public functions
        public void DisplayStrands(List<Strand> strands, List<PermutationNode> permutationNodeList)
        {
            _treeView.BeginUpdate();
            int momentIndex = 0;
            foreach(PermutationNode permutationNode in permutationNodeList)
            {
                string valueString = K.GetStringOfUnsignedInts(strands[momentIndex++].Values);
                valueString = valueString.Replace(" ", ",");
                permutationNode.Text = permutationNode.Text + " (" + valueString + ")";
            }
            _treeView.EndUpdate();
        }
        #endregion public functions

		#region private variables
		private readonly TreeView _treeView;
		#endregion private variables
	}
}
