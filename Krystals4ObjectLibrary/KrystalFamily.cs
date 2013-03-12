using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;

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
            string allConstants = "ck*.krys";
            foreach(FileInfo f in dir.GetFiles(allConstants))
            {
                Dependency d = new Dependency();
                d.Name = f.Name;
                _dependencyList.Add(d);
            }
            string allLines = "lk*.krys";
            foreach(FileInfo f in dir.GetFiles(allLines))
            {
                Dependency d = new Dependency();
                d.Name = f.Name;
                _dependencyList.Add(d);
            }

            #region add expansions to the _unknownParentsList
            string expansions = "xk*.krys";
            ExpansionKrystal xk = null;
            foreach(FileInfo f in dir.GetFiles(expansions))
            {
                string path = K.KrystalsFolder + @"\" +  f.Name;
                xk = new ExpansionKrystal(path);
                Dependency d = new Dependency();
                d.Name = f.Name;
                if(xk != null)
                {
                    d.Input1 = xk.DensityInputFilename;
                    d.Input2 = xk.PointsInputFilename;
                    d.Field = xk.Expander.Name;
                }
                _unknownParentsList.Add(d);
            }
            #endregion add expansions to the _unknownParentsList

            #region add shaped expansions to the _unknownParentsList
            string shapedExpansions = "sk*.krys";
            ShapedExpansionKrystal sk = null;
            foreach(FileInfo f in dir.GetFiles(shapedExpansions))
            {
                string path = K.KrystalsFolder + @"\" +  f.Name;
                sk = new ShapedExpansionKrystal(path);
                Dependency d = new Dependency();
                d.Name = f.Name;
                if(sk != null)
                {
                    d.Input1 = sk.DensityInputFilename;
                    d.Input2 = sk.PointsInputFilename;
                    d.Input3 = sk.AxisInputFilename;
                    d.Input4 = sk.ContourInputFilename;
                    d.Field = sk.Expander.Name;
                }
                _unknownParentsList.Add(d);
            }
            #endregion add shaped expansions to the _unknownParentsList

            #region add modulations to the _unknownParentsList
            string allModulations = "mk*.krys";
            ModulationKrystal mk = null;
            foreach(FileInfo f in dir.GetFiles(allModulations))
            {
                string path = K.KrystalsFolder + @"\" + f.Name;
                mk = new ModulationKrystal(path);
                Dependency d = new Dependency();
                d.Name = f.Name;
                if(mk != null)
                {
                    d.Input1 = mk.XInputFilename;
                    d.Input2 = mk.YInputFilename;
                    d.Field = mk.Modulator.Name;
                }
                _unknownParentsList.Add(d);
            }
            #endregion add modulations to the _unknownParentsList
            #region add permutation krystals to the _unknownParentsList
            string allPermutations = "pk*.krys";
            PermutationKrystal pk = null;
            foreach(FileInfo f in dir.GetFiles(allPermutations))
            {
                string path = K.KrystalsFolder + @"\" + f.Name;
                pk = new PermutationKrystal(path);
                Dependency d = new Dependency();
                d.Name = f.Name;
                if(pk != null)
                {
                    d.Input1 = pk.SourceInputFilename;
                    d.Input2 = pk.AxisInputFilename;
                    d.Input3 = pk.ContourInputFilename;
                }
                _unknownParentsList.Add(d);
            }
            #endregion add permutation krystals to the _unknownParentsList
            #region insert Dependencies from the _unknownParentsList in the sorted _dependencyList
            bool found = true;
            int[] inputIndex = new int[4];
            int minIndex = -1;
            int maxIndex = -1;
            while(_unknownParentsList.Count > 0 && found)
            {
                inputIndex[0] = inputIndex[1] = inputIndex[2] = inputIndex[3] = -1;
                found = false;
                foreach(Dependency d in _unknownParentsList)
                {
                    if(string.IsNullOrEmpty(d.Input1) == false)
                    {
                        for(int index = 0 ; index < _dependencyList.Count ; index++)
                        {
                            if(d.Input1.Equals(_dependencyList[index].Name)) // InputIndex[inputNameIndex] is currently -1
                            {
                                inputIndex[0] = index; // save the index of the input file in the dependency list
                                break;
                            }
                        }

                        if(string.IsNullOrEmpty(d.Input2) == false)
                        {
                            for(int index = 0 ; index < _dependencyList.Count ; index++)
                            {
                                if(d.Input2.Equals(_dependencyList[index].Name)) // InputIndex[inputNameIndex] is currently -1
                                {
                                    inputIndex[1] = index; // save the index of the input file in the dependency list
                                    break;
                                }
                            }
                        }

                        if(string.IsNullOrEmpty(d.Input3) == false)
                        {
                            for(int index = 0 ; index < _dependencyList.Count ; index++)
                            {
                                if(d.Input3.Equals(_dependencyList[index].Name)) // InputIndex[inputNameIndex] is currently -1
                                {
                                    inputIndex[2] = index; // save the index of the input file in the dependency list
                                    break;
                                }
                            }
                        }

                        if(string.IsNullOrEmpty(d.Input4) == false)
                        {
                            for(int index = 0 ; index < _dependencyList.Count ; index++)
                            {
                                if(d.Input4.Equals(_dependencyList[index].Name)) // InputIndex[inputNameIndex] is currently -1
                                {
                                    inputIndex[3] = index; // save the index of the input file in the dependency list
                                    break;
                                }
                            }
                        }

                        if(inputIndex[0] < inputIndex[1])
                        {
                            minIndex = inputIndex[0];
                            maxIndex = inputIndex[1];
                        }
                        else
                        {
                            minIndex = inputIndex[1];
                            maxIndex = inputIndex[0];
                        }
                        if(string.IsNullOrEmpty(d.Input3) == false || string.IsNullOrEmpty(d.Input4) == false)
                        {
                            minIndex = minIndex < inputIndex[2] ? minIndex : inputIndex[2];
                            minIndex = minIndex < inputIndex[3] ? minIndex : inputIndex[3];
                            maxIndex = maxIndex > inputIndex[2] ? maxIndex : inputIndex[2];
                            maxIndex = maxIndex > inputIndex[3] ? maxIndex : inputIndex[3];
                        }

                        if(minIndex >= 0) // all the inputs are currently in the _dependencyList
                        {
                            _dependencyList.Insert(maxIndex + 1, d);
                        }
                    }
                }
                int removed = 0;
                foreach(Dependency d in _dependencyList)
                    if(_unknownParentsList.Remove(d))
                        removed++;
                if(removed > 0)
                    found = true;
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
                    ExpansionKrystal xk = new ExpansionKrystal(path);
                    xk.Rebuild();
                }
                if(K.IsShapedExpansionKrystalFilename(d.Name))
                {
                    ShapedExpansionKrystal sk = new ShapedExpansionKrystal(path);
                    sk.Rebuild();
                }
                if(K.IsModulationKrystalFilename(d.Name))
                {
                    ModulationKrystal mk = new ModulationKrystal(path);
                    mk.Rebuild();
                }
                if(K.IsPermutationKrystalFilename(d.Name))
                {
                    PermutationKrystal pk = new PermutationKrystal(path);
                    pk.Rebuild();
                }
            }
        }
        public List<Dependency> DependencyList { get { return _dependencyList; } }
        private List<Dependency> _dependencyList = new List<Dependency>();
        private List<Dependency> _unknownParentsList = new List<Dependency>();
    }
}
