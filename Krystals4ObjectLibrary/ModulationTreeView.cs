using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace Krystals5ObjectLibrary
{
	public class ModulationTreeView
	{
		public ModulationTreeView(TreeView treeView, ModulationKrystal modulationKrystal)
		{
			_modulationNodeList = modulationKrystal.ModulationNodeList;
			_treeView = treeView;
			#region constant krystal inputs
			// It is pointless to have two constant inputs for a modulation
			// so I have forbidden this combination in the ModulationKrystal constructor.
			#endregion constant krystal inputs

			#region line krystal inputs
			if(modulationKrystal.Level == 1) // line krystal
			{
				_treeView.BeginUpdate();
				TreeNode root = new TreeNode
				{
					Text = "1: m1"
				};
				_treeView.Nodes.Add(root);
				foreach(ModulationNode modulationNode in _modulationNodeList)
				{
					int m = modulationNode.ModMoment;
					int x = modulationNode.X;
					int y = modulationNode.Y;
					modulationNode.Text = m.ToString()
						+ ": m" + m.ToString()
						+ ", x" + x.ToString()
						+ ", y" + y.ToString();
					root.Nodes.Add(modulationNode);
				}
				_treeView.EndUpdate();
			}
			#endregion line krystal inputs
			else
			#region higher level krystal inputs
			{
				// Construct the levels of the tree above the modulationNodeList, adding the ModulationNodes
				// where necessary. (The upper levels consist of pure TreeNodes and include the single root
				// node.) 
				TreeNode[] currentNode = new TreeNode[modulationKrystal.Level];
				int[] localSectionNumber = new int[modulationKrystal.Level];// does not include the local strand section numbers
				int localModulationSectionNumber = 0;
				foreach(ModulationNode modulationNode in _modulationNodeList)
				{
					if(modulationNode.ModLevel <= modulationKrystal.Level)
					{
						localModulationSectionNumber = 0;
						int levelIndex = modulationNode.ModLevel - 1;
						while(levelIndex < modulationKrystal.Level)
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
					localModulationSectionNumber++;
					currentNode[modulationKrystal.Level - 1].Nodes.Add(modulationNode);
					localSectionNumber[modulationKrystal.Level - 1]++;
					int m = modulationNode.ModMoment;
					int x = modulationNode.X;
					int y = modulationNode.Y;
					modulationNode.Text = modulationNode.Parent.Text
						+ "." + localModulationSectionNumber.ToString()
						+ ": m" + m.ToString()
						+ ", x" + x.ToString()
						+ ", y" + y.ToString();
				}
				// Now add the moment numbers to the pure TreeNode.Texts
				// collapsing the level above the modulationNodes.
				foreach(ModulationNode modulationNode in _modulationNodeList)
				{
					if(modulationNode.ModLevel <= modulationKrystal.Level)
					{
						TreeNode tn = modulationNode.Parent;
						tn.Collapse();
						bool continueUp = true;
						while(continueUp)
						{
							if(tn.Text.EndsWith(".1") && tn.Level > 0)
								continueUp = true;
							else continueUp = false;
							int m = modulationNode.ModMoment;
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
		/// <summary>
		/// Appends the modulation results to the existing tree view of the input values.
		/// The values are displayed in round () brackets.
		/// </summary>
		public void DisplayModulationResults(ModulationKrystal modulationKrystal)
		{
			//(List<ModulationNode> modulationNodeList, uint outputKrystalLevel)
			_treeView.BeginUpdate();
			#region display single results
			foreach(ModulationNode modulationNode in _modulationNodeList)
			{
				int resultStartIndex = modulationNode.Text.IndexOf(" (");
				if(resultStartIndex > 0)
					modulationNode.Text = modulationNode.Text.Remove(resultStartIndex);
				int result = modulationNode.ModResult;
				modulationNode.Text = modulationNode.Text + " (" + result.ToString() + ")";
			}
			#endregion display single results
			#region display strand values (one level higher)
			uint strandValueLevel = modulationKrystal.Level + 1;
			StringBuilder strandSB = new StringBuilder("  (");
			ModulationNode m;
			for(int index = 0 ; index < _modulationNodeList.Count ; index++)
			{
				m = _modulationNodeList[index];
				if(m.ModLevel < strandValueLevel && index > 0)
				{
					strandSB[strandSB.Length - 1] = ')'; // overwrites final comma
					int resultStartIndex = _modulationNodeList[index - 1].Parent.Text.IndexOf("  (");
					if(resultStartIndex > 0)
						_modulationNodeList[index - 1].Parent.Text =
							_modulationNodeList[index - 1].Parent.Text.Remove(resultStartIndex);
					_modulationNodeList[index - 1].Parent.Text = _modulationNodeList[index - 1].Parent.Text + strandSB.ToString();
					strandSB = new StringBuilder("  (");
				}
				int result = m.ModResult;
				strandSB.Append(result.ToString());
				strandSB.Append(",");
			}
			strandSB[strandSB.Length - 1] = ')'; // overwrites final comma
			int bracketIndex = _modulationNodeList[_modulationNodeList.Count - 1].Parent.Text.IndexOf("  (");
			if(bracketIndex > 0)
				_modulationNodeList[_modulationNodeList.Count - 1].Parent.Text =
					_modulationNodeList[_modulationNodeList.Count - 1].Parent.Text.Remove(bracketIndex);
			_modulationNodeList[_modulationNodeList.Count - 1].Parent.Text =
				_modulationNodeList[_modulationNodeList.Count - 1].Parent.Text + strandSB.ToString();
			#endregion display strand values (one level higher)
			_treeView.EndUpdate();
		}
		#endregion public functions
		#region private variables
		private readonly TreeView _treeView;
		private List<ModulationNode> _modulationNodeList;
		#endregion private variables
	}
}
