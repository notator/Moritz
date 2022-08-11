using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Reflection;

namespace Krystals4ObjectLibrary
{
    /// <summary>
    /// Krystal dependencies.
    /// Name: The name of the krystal which was constructed using the objects named in the other fields.
    /// ExpansionKrystal:       
    ///     Input1: density input krystal
    ///     Input2: points input krystal
    ///     Field: the name of the expander
    /// ShapedExpansionKrystal:
    ///     Input1: density input krystal
    ///     Input2: points input krystal
    ///     Input3: axis input krystal
    ///     Input4: contour input krystal
    ///     Field: the name of the expander
    /// ModulationKrystal:
    ///     Input1: X input krystal
    ///     Input2: Y input krystal
    ///     Field: the name of the modulator
    /// PermutationKrystal:
    ///     Input1: the krystal to be permuted
    ///     Input2: axis input krystal
    ///     Input3: contour input krystal
    ///     (The level and sortFirst parameters are not represented here because they are not krystals.)
    /// </summary>
    public class Dependency
    {
        public string Name;
        public string Input1;
        public string Input2;
        public string Input3;
        public string Input4;
        public string Field;
    }

    /// <summary>
    /// A KrystalFamily is the group of (related) krystals in the K.KrystalsFolder.
    /// The krystals may be of any type (they may have been created in any way: constant, line,
    /// expansion, modulation etc.), and they may be of any size or shape.
    /// </summary>
    public class KrystalFamily
    {
        /// <summary>
        /// Loads all the krystals in K.KrystalsFolder into a list of dependencies,
        /// in which later entries in the list are dependent on earlier entries. 
        /// </summary>
        public KrystalFamily(string krystalsFolder)
        {
            DirectoryInfo dir = new DirectoryInfo(krystalsFolder);
            string allConstants = "*.constant.krys";
            var fileInfos = dir.GetFiles(allConstants);
            foreach(FileInfo f in fileInfos)
            {
				Dependency d = new Dependency
				{
					Name = f.Name
				};
				_dependencyList.Add(d);
            }
            string allLines = "*.line.krys";
            fileInfos = dir.GetFiles(allLines);
            foreach(FileInfo f in fileInfos)
            {
				Dependency d = new Dependency
				{
					Name = f.Name
				};
				_dependencyList.Add(d);
            }

            #region add expansions to the _unknownParentsList
            string expansions = "*.exp.krys";
            fileInfos = dir.GetFiles(expansions);
            foreach(FileInfo f in fileInfos)
            {
                string path = K.KrystalsFolder + @"\" +  f.Name;
                var xk = new ExpansionKrystal(path, true);
				Dependency d = new Dependency
				{
					Name = f.Name
				};
				if(xk != null)
                {
                    d.Input1 = xk.DensityInputFilename;
                    d.Input2 = xk.PointsInputFilename;
                    d.Field = xk.ExpanderFilename;
                }
                _unknownParentsList.Add(d);
            }
            #endregion add expansions to the _unknownParentsList

            #region add modulations to the _unknownParentsList
            string allModulations = "*.mod.krys";
            fileInfos = dir.GetFiles(allModulations);
            foreach(FileInfo f in fileInfos)
            {
                string path = K.KrystalsFolder + @"\" + f.Name;
                var mk = new ModulationKrystal(path, true);
				Dependency d = new Dependency
				{
					Name = f.Name
				};
				if(mk != null)
                {
                    d.Input1 = mk.XInputFilename;
                    d.Input2 = mk.YInputFilename;
                    d.Field = mk.ModulatorFilename;
                }
                _unknownParentsList.Add(d);
            }
            #endregion add modulations to the _unknownParentsList

            #region add permutation krystals to the _unknownParentsList
            string allPermutations = "*.perm.krys";
            fileInfos = dir.GetFiles(allPermutations);
            foreach(FileInfo f in fileInfos)
            {
                string path = K.KrystalsFolder + @"\" + f.Name;
                var pk = new PermutationKrystal(path, true);
                Dependency d = new Dependency
                {
                    Name = f.Name
                };
                if(pk != null)
                {
                    d.Input1 = pk.SourceInputFilename;
                    d.Input2 = pk.AxisInputFilename;
                    d.Input3 = pk.ContourInputFilename;
                }
                _unknownParentsList.Add(d);
            }
            #endregion add path krystals to the _unknownParentsList

            #region add path krystals to the _unknownParentsList
            string allPaths = "*.path.krys";
            fileInfos = dir.GetFiles(allPaths);
            foreach(FileInfo f in fileInfos)
            {
                string path = K.KrystalsFolder + @"\" + f.Name;
                var pk = new PathKrystal(path, true);
                Dependency d = new Dependency
                {
                    Name = f.Name
                };
                if(pk != null)
                {
                    d.Input1 = pk.DensityInputKrystalName;
                    d.Input2 = pk.SVGInputFilename;
                }
                _unknownParentsList.Add(d);
            }
            #endregion add path krystals to the _unknownParentsList

            #region insert Dependencies from the _unknownParentsList in the sorted _dependencyList

            // The _dependency list is going to be in order of dependence. In other words:
            // Each dependency object's _inputs_ (if it has any) will have an index
            // less than the index of the dependency object itself.
            int[] inputIndex = new int[4];
            foreach(Dependency d in _unknownParentsList)
            {
                inputIndex[0] = inputIndex[1] = inputIndex[2] = inputIndex[3] = -1;
                if(string.IsNullOrEmpty(d.Input1) == false)
                {
                    inputIndex[0] = _dependencyList.FindIndex(x => d.Input1.Equals(x.Name));
                }

                if(string.IsNullOrEmpty(d.Input2) == false)
                {
                    inputIndex[1] = _dependencyList.FindIndex(x => d.Input2.Equals(x.Name));
                }

                if(string.IsNullOrEmpty(d.Input3) == false)
                {
                    inputIndex[2] = _dependencyList.FindIndex(x => d.Input3.Equals(x.Name));
                }

                if(string.IsNullOrEmpty(d.Input4) == false)
                {
                    inputIndex[3] = _dependencyList.FindIndex(x => d.Input4.Equals(x.Name));
                }

                var maxIndex = -1;
                for(int j = 0; j < inputIndex.Length; j++)
                {
                    var index = inputIndex[j];
                    if(index != -1)
                    {
                        maxIndex = (maxIndex > index) ? maxIndex : index;
                    }
                }

                if(maxIndex == -1)
                {
                    // None of this dependency's inputs were found in the _dependencyList
                    // but it may be the input of a dependency that has not yet been added.
                    _dependencyList.Add(d);
                }
                else
                {
                    _dependencyList.Insert(maxIndex + 1, d);
                }
            }
            #endregion move Dependencies from the _unknownParentsList to the sorted _dependencyList
        }
        public void Rebuild()
        {
            if(_unknownParentsList.Count > 0)
            {
                StringBuilder orphans = new StringBuilder();
                foreach(Dependency d in _unknownParentsList)
                {
                    orphans.Append("\n");
                    orphans.Append(d.Name);
                }
                string msg =
					"The following krystals contain fatal errors,\n" +
					"which mean that they cannot be (re)constructed:\n" + orphans.ToString() +
					"\n\nDelete?";
                DialogResult result = MessageBox.Show(msg, "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if(result == DialogResult.Yes)
                    foreach(Dependency d in _unknownParentsList)
                        File.Delete(K.KrystalsFolder + @"\" +  d.Name);
            }

            foreach(Dependency d in _dependencyList)
            {
                string path = K.KrystalsFolder + @"\" +  d.Name;
                if(K.IsExpansionKrystalFilename(d.Name))
                {
                    ExpansionKrystal xk = new ExpansionKrystal(path, true);
                }
                if(K.IsModulationKrystalFilename(d.Name))
                {
                    ModulationKrystal mk = new ModulationKrystal(path, true);
                }
                if(K.IsPermutationKrystalFilename(d.Name))
                {
                    PermutationKrystal pk = new PermutationKrystal(path, true);
                }
                if(K.IsPathKrystalFilename(d.Name))
                {
                    PathKrystal pk = new PathKrystal(path, true);
                }
            }
        }
        public List<Dependency> DependencyList { get { return _dependencyList; } }
        private List<Dependency> _dependencyList = new List<Dependency>();
        private List<Dependency> _unknownParentsList = new List<Dependency>();
    }
}
