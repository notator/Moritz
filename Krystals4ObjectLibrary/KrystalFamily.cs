using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Krystals4ObjectLibrary
{
    /// <summary>
    /// Krystal dependencies.
    /// Name: The name of the krystal which was constructed using the objects named in the other fields.
    /// ScoreNames: The names of the scores that directly use the krystal whose name is Name.
    /// ExpansionKrystal:       
    ///     Input1: density input krystal
    ///     Input2: points input krystal
    ///     Field: the name of the expander
    /// ModulationKrystal:
    ///     Input1: X input krystal
    ///     Input2: Y input krystal
    ///     Field: the name of the modulator
    /// PermutationKrystal:
    ///     Input1: the krystal to be permuted
    ///     Input2: axis input krystal
    ///     Input3: contour input krystal
    ///     (The level and sortFirst parameters are not represented here.)
    /// PathKrystal
    ///     Input1 = density input krystal;
    ///     Input2 = SVG input;
    /// </summary>
    public class Dependency
    {
        public string Name;
        public List<string> ScoreNames { get; internal set; }
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
        public KrystalFamily(string krystalsFolder, Dictionary<string, List<string>> krystalScoresDict)
        {
            DirectoryInfo dir = new DirectoryInfo(krystalsFolder);
            List<Dependency> unsortedDependencies = new List<Dependency>();
            string krystalNames = "*.krys";
            var allKrystalFileInfos = dir.GetFiles(krystalNames);
            foreach(FileInfo fileInfo in allKrystalFileInfos)
            {
                GetUnsortedDependencies(unsortedDependencies, krystalScoresDict, allKrystalFileInfos, fileInfo);
            }

            #region set the sorted _dependencyList

            // The _dependency list will be in order of dependence:
            // Each dependency object's _inputs_ (if it has any) will have an index
            // less than the index of the dependency object itself.
            int[] inputIndex = new int[4];
            foreach(Dependency d in unsortedDependencies)
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

            #endregion set the sorted _dependencyList

            foreach(var dependency in _dependencyList)
            {
                SetNameColors(dependency, krystalScoresDict);
            }
        }

        private void GetUnsortedDependencies(List<Dependency> unsortedDependencies, Dictionary<string, List<string>> krystalScoresDict, FileInfo[] fileInfos, FileInfo fileInfo)
        {
            Dependency d = unsortedDependencies.Find(x => x.Name == fileInfo.Name);
            if(d == null)
            {
                d = new Dependency
                {
                    Name = fileInfo.Name
                };
            }

            if(fileInfo.Name.Contains(".constant.") || fileInfo.Name.Contains(".line."))
            {
                if(unsortedDependencies.Find(x => x.Name == d.Name) == null)
                {
                    unsortedDependencies.Insert(0, d);
                }
            }
            else if(fileInfo.Name.Contains(".exp."))
            {
                AddExpansionDependency(d, unsortedDependencies, krystalScoresDict, fileInfos, fileInfo);
            }
            else if(fileInfo.Name.Contains(".mod."))
            {
                AddModulationDependency(d, unsortedDependencies, krystalScoresDict, fileInfos, fileInfo);
            }
            else if(fileInfo.Name.Contains(".perm."))
            {
                AddPermutationDependency(d, unsortedDependencies, krystalScoresDict, fileInfos, fileInfo);
            }
            else if(fileInfo.Name.Contains(".perm."))
            {
                AddPathDependency(d, unsortedDependencies, krystalScoresDict, fileInfos, fileInfo);
            }
        }

        private void AddExpansionDependency(Dependency d, List<Dependency> unsortedDependencies, Dictionary<string, List<string>> krystalScoresDict, FileInfo[] fileInfos, FileInfo fileInfo)
        {
            Debug.Assert(d != null);

            string path = K.KrystalsFolder + @"\" + fileInfo.Name;
            var xk = new ExpansionKrystal(path, true);
            d.Input1 = xk.DensityInputFilename;
            d.Input2 = xk.PointsInputFilename;
            d.Field = xk.ExpanderFilename;

            var fileInfo1 = Array.Find(fileInfos, x => x.Name == d.Input1);
            GetUnsortedDependencies(unsortedDependencies, krystalScoresDict, fileInfos, fileInfo1);
            var fileInfo2 = Array.Find(fileInfos, x => x.Name == d.Input2);
            GetUnsortedDependencies(unsortedDependencies, krystalScoresDict, fileInfos, fileInfo2);

            if(unsortedDependencies.Find(x => x.Name == fileInfo.Name) == null)
            {
                unsortedDependencies.Add(d);
            }
        }
        private void AddModulationDependency(Dependency d, List<Dependency> unsortedDependencies, Dictionary<string, List<string>> krystalScoresDict, FileInfo[] fileInfos, FileInfo fileInfo)
        {
            Debug.Assert(d != null);

            string path = K.KrystalsFolder + @"\" + fileInfo.Name;
            var mk = new ModulationKrystal(path, true);
            d.Input1 = mk.XInputFilename;
            d.Input2 = mk.YInputFilename;
            d.Field = mk.ModulatorFilename;

            var fileInfo1 = Array.Find(fileInfos, x => x.Name == d.Input1);
            GetUnsortedDependencies(unsortedDependencies, krystalScoresDict, fileInfos, fileInfo1);
            var fileInfo2 = Array.Find(fileInfos, x => x.Name == d.Input2);
            GetUnsortedDependencies(unsortedDependencies, krystalScoresDict, fileInfos, fileInfo2);

            if(unsortedDependencies.Find(x => x.Name == fileInfo.Name) == null)
            {
                unsortedDependencies.Add(d);
            }
        }
        private void AddPermutationDependency(Dependency d, List<Dependency> unsortedDependencies, Dictionary<string, List<string>> krystalScoresDict, FileInfo[] fileInfos, FileInfo fileInfo)
        {
            Debug.Assert(d != null);

            string path = K.KrystalsFolder + @"\" + fileInfo.Name;
            var pk = new PermutationKrystal(path, true);
            d.Input1 = pk.SourceInputFilename;
            d.Input2 = pk.AxisInputFilename;
            d.Input3 = pk.ContourInputFilename;

            var fileInfo1 = Array.Find(fileInfos, x => x.Name == d.Input1);
            GetUnsortedDependencies(unsortedDependencies, krystalScoresDict, fileInfos, fileInfo1);
            var fileInfo2 = Array.Find(fileInfos, x => x.Name == d.Input2);
            GetUnsortedDependencies(unsortedDependencies, krystalScoresDict, fileInfos, fileInfo2);
            var fileInfo3 = Array.Find(fileInfos, x => x.Name == d.Input3);
            GetUnsortedDependencies(unsortedDependencies, krystalScoresDict, fileInfos, fileInfo3);

            if(unsortedDependencies.Find(x => x.Name == fileInfo.Name) == null)
            {
                unsortedDependencies.Add(d);
            }
        }
        private void AddPathDependency(Dependency d, List<Dependency> unsortedDependencies, Dictionary<string, List<string>> krystalScoresDict, FileInfo[] fileInfos, FileInfo fileInfo)
        {
            Debug.Assert(d != null);

            string path = K.KrystalsFolder + @"\" + fileInfo.Name;
            var pk = new PathKrystal(path, true);
            d.Input1 = pk.DensityInputKrystalName;
            d.Input2 = pk.SVGInputFilename;

            var fileInfo1 = Array.Find(fileInfos, x => x.Name == d.Input1);
            GetUnsortedDependencies(unsortedDependencies, krystalScoresDict, fileInfos, fileInfo1);

            if(unsortedDependencies.Find(x => x.Name == fileInfo.Name) == null)
            {
                unsortedDependencies.Add(d);
            }
        }

        private void SetNameColors(Dependency d, Dictionary<string, List<string>> krystalScoresDict)
        {
            if(krystalScoresDict.ContainsKey(d.Name))
            {
                d.ScoreNames = krystalScoresDict[d.Name];

                SetUsedInScoreColor(d.Name);
                SetAncestorColors(d, krystalScoresDict);
            }
        }

        private void SetAncestorColors(Dependency d, Dictionary<string, List<string>> krystalScoresDict)
        {
            SetAncestorColor(d.Name);
            if(d.Field != null)
            {
                SetAncestorColor(d.Field);
            }

            var inputdep1 = _dependencyList.Find(x => x.Name == d.Input1);
            var inputdep2 = _dependencyList.Find(x => x.Name == d.Input2);
            var inputdep3 = _dependencyList.Find(x => x.Name == d.Input3);
            var inputdep4 = _dependencyList.Find(x => x.Name == d.Input4);

            if(inputdep1 != null)
            {
                SetNameColors(inputdep1, krystalScoresDict);
            }
            if(inputdep2 != null)
            {
                SetNameColors(inputdep2, krystalScoresDict);
            }
            if(inputdep3 != null)
            {
                SetNameColors(inputdep3, krystalScoresDict);
            }
            if(inputdep4 != null)
            {
                SetNameColors(inputdep4, krystalScoresDict);
            }
        }
        private void SetAncestorColor(string name)
        {
            if(NameColors.ContainsKey(name) && NameColors[name] == UsedInScoreColor)
            {
                return;
            }
            if(!NameColors.ContainsKey(name))
            {
                this.NameColors.Add(name, AncestorColor);
            }
        }

        private void SetUsedInScoreColor(string name)
        {
            if(!NameColors.ContainsKey(name))
            {
                this.NameColors.Add(name, UsedInScoreColor);
            }
            else
            {
                NameColors[name] = UsedInScoreColor;
            }
        }

        public Color GetNameColor(string name)
        {
            if(NameColors.ContainsKey(name))
            {
                return NameColors[name];
            }
            else
            {
                return Color.Black;
            }
        }

        public void Rebuild()
        {
            throw new NotImplementedException("TODO");

            // old code
            //if(_unknownParentsList.Count > 0)
            //{
            //    StringBuilder orphans = new StringBuilder();
            //    foreach(Dependency d in _unknownParentsList)
            //    {
            //        orphans.Append("\n");
            //        orphans.Append(d.Name);
            //    }
            //    string msg =
            //        "The following krystals contain fatal errors,\n" +
            //        "which mean that they cannot be (re)constructed:\n" + orphans.ToString() +
            //        "\n\nDelete?";
            //    DialogResult result = MessageBox.Show(msg, "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            //    if(result == DialogResult.Yes)
            //        foreach(Dependency d in _unknownParentsList)
            //            File.Delete(K.KrystalsFolder + @"\" + d.Name);
            //}

            //foreach(Dependency d in _dependencyList)
            //{
            //    string path = K.KrystalsFolder + @"\" + d.Name;
            //    if(K.IsExpansionKrystalFilename(d.Name))
            //    {
            //        ExpansionKrystal xk = new ExpansionKrystal(path, true);
            //    }
            //    if(K.IsModulationKrystalFilename(d.Name))
            //    {
            //        ModulationKrystal mk = new ModulationKrystal(path, true);
            //    }
            //    if(K.IsPermutationKrystalFilename(d.Name))
            //    {
            //        PermutationKrystal pk = new PermutationKrystal(path, true);
            //    }
            //    if(K.IsPathKrystalFilename(d.Name))
            //    {
            //        PathKrystal pk = new PathKrystal(path, true);
            //    }
            //}
        }
        public List<Dependency> DependencyList { get { return _dependencyList; } }
        private List<Dependency> _dependencyList = new List<Dependency>();

        private readonly Color UsedInScoreColor = Color.Red;
        private readonly Color AncestorColor = Color.FromArgb(255, 0, 170, 0);
        /// <summary>
        /// Contains only name/color pairs for names of krystals used in scores and their ancestors (krystals and fields).
        /// The default Color for other node names will be Color.Black.
        /// </summary>
        public Dictionary<string, Color> NameColors = new Dictionary<string, Color>();
    }
}
