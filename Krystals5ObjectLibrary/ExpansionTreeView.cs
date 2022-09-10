using System.Collections.Generic;
using System.Windows.Forms;

namespace Krystals5ObjectLibrary
{
	public class ExpansionTreeView
	{
		public ExpansionTreeView(TreeView treeView, List<StrandNode> strandNodeList,
			uint dKrystalLevel, List<int> missingPointValues)
		{
			_treeView = treeView;
			_strandNodeList = strandNodeList;

			string missingPointValuesStr = "";
			if(missingPointValues.Count > 0)
				missingPointValuesStr = K.GetStringOfUnsignedInts(missingPointValues);
			#region constant krystal inputs
			if(dKrystalLevel == 0) // constant krystal inputs
			{
				int p = strandNodeList[0].StrandPoint;
				int d = strandNodeList[0].StrandDensity;
				strandNodeList[0].Text = "1: m1"
					+ ", p" + p.ToString()
					+ ", d" + d.ToString();
				_treeView.Nodes.Add(strandNodeList[0]);
			}
			#endregion constant krystal inputs
			else
				#region line krystal inputs
				if(dKrystalLevel == 1) // line krystal
				{
					_treeView.BeginUpdate();
					if(missingPointValues.Count > 0)
						_treeView.Nodes.Add("Missing p value(s): " + missingPointValuesStr);
					foreach(StrandNode strandNode in strandNodeList)
					{
						int m = strandNode.StrandMoment;
						int p = strandNode.StrandPoint;
						int d = strandNode.StrandDensity;
						strandNode.Text = m.ToString()
							+ ": m" + m.ToString()
							+ ", p" + p.ToString()
							+ ", d" + d.ToString();
						_treeView.Nodes.Add(strandNode);
					}
					_treeView.EndUpdate();
				}
				#endregion line krystal inputs
				else
				#region higher level krystal inputs
				{
					// Construct the levels of the tree above the strandNodeList, adding the StrandNodes where
					// necessary. (The upper levels consist of pure TreeNodes and include the single root node.) 
					TreeNode[] currentNode = new TreeNode[dKrystalLevel];
					int[] localSectionNumber = new int[dKrystalLevel];// does not include the local strand section numbers
					int localStrandSectionNumber = 0;
					foreach(StrandNode strandNode in strandNodeList)
					{
						if(strandNode.StrandLevel <= dKrystalLevel)
						{
							localStrandSectionNumber = 0;
							int levelIndex = strandNode.StrandLevel - 1;
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
						currentNode[dKrystalLevel - 1].Nodes.Add(strandNode);
						localSectionNumber[dKrystalLevel - 1]++;
						int m = strandNode.StrandMoment;
						int p = strandNode.StrandPoint;
						int d = strandNode.StrandDensity;
						strandNode.Text = strandNode.Parent.Text
							+ "." + localStrandSectionNumber.ToString()
							+ ": m" + m.ToString()
							+ ", p" + p.ToString()
							+ ", d" + d.ToString();
					}
					// Now add the moment numbers to the pure TreeNode.Texts
					foreach(StrandNode strandNode in strandNodeList)
					{
						if(strandNode.StrandLevel <= dKrystalLevel)
						{
							TreeNode tn = strandNode.Parent;
							bool continueUp = true;
							while(continueUp)
							{
								if(tn.Text.EndsWith(".1") && tn.Level > 0)
									continueUp = true;
								else continueUp = false;
								int m = strandNode.StrandMoment;
								tn.Text = tn.Text + ": m" + m.ToString();
								if(continueUp)
									tn = tn.Parent;
							}
						}
					}
					_treeView.BeginUpdate();
					_treeView.Nodes.Clear();
					if(missingPointValues.Count > 0)
						_treeView.Nodes.Add("Missing p value(s): " + missingPointValuesStr);
					foreach(TreeNode n in currentNode[0].Nodes)
					{
						_treeView.Nodes.Add(n);
					}
					_treeView.EndUpdate();
				}
				#endregion higher level krystal inputs
		}
		#region public functions
		/// <summary>
		/// Appends the expanded strand values to the existing tree view of the input values
		/// The strand values are displayed separated by commas and enclosed in round () brackets.
		/// </summary>
		public void DisplayStrands(List<Strand> strands)
		{
			_treeView.BeginUpdate();
			int momentIndex = 0;
			foreach(StrandNode strandNode in _strandNodeList)
			{
				string valueString = K.GetStringOfUnsignedInts(strands[momentIndex++].Values);
				valueString = valueString.Replace(" ", ",");
				strandNode.Text = strandNode.Text + " (" + valueString + ")";
			}
			_treeView.EndUpdate();
		}
		/// <summary>
		/// Removes displayed strand values from the existing tree view.
		/// This function is called by FieldEditorWindow.ConfigureEditorControls().
		/// </summary>
		public void RemoveStrands()
		{
			_treeView.BeginUpdate();
			foreach(StrandNode strandNode in _strandNodeList)
			{
				int strandStartIndex = strandNode.Text.IndexOf(" (");
				if(strandStartIndex > 0)
					strandNode.Text = strandNode.Text.Remove(strandStartIndex);
			}
			_treeView.EndUpdate();
		}
		public void Clear()
		{
			_treeView.Nodes.Clear();
		}
		public void SelectNode(TreeNode node)
		{
			node.EnsureVisible();
			_treeView.SelectedNode = node;
		}
		/// <summary>
		/// Selects the tree node having the given moment number. If the selected node is not visible,
		/// the treeview is scrolled so that the selected node's parent node is the top visible node.
		/// </summary>
		/// <param name="momentNumber">the moment number</param>
		public void SelectMoment(int momentNumber)
		{
			_treeView.BeginUpdate();
			_treeView.SelectedNode = _strandNodeList[momentNumber - 1];
			if(_treeView.SelectedNode.IsVisible == false)
				_treeView.TopNode = _treeView.SelectedNode.Parent;
			_treeView.EndUpdate();
		}
		/// <summary>
		/// Returns a string containing the section number at this (global) moment. This is the address of the
		/// moment with respect to its containing sections. The first number is the number of the level 2 section
		/// within which this moment occurs. The second number is the number of the level 3 section within this
		/// level 2 section (this number is reset to 1 at the beginning of a new level 2 section).
		/// The final number is the (1-based) index of this moment within its containing section. 
		/// </summary>
		/// <param name="moment"></param>
		/// <returns></returns>
		public string SectionNumberString(int momentNumber)
		{
			string s = _strandNodeList[momentNumber - 1].Text;
			int endOfSectionNumber = s.IndexOf(":");
			if(endOfSectionNumber > 0)
				s = s.Remove(endOfSectionNumber);
			return s;
		}
		#endregion public functions
		#region private variables
		private readonly TreeView _treeView;
		private readonly List<StrandNode> _strandNodeList;
		#endregion private variables
	}
}
