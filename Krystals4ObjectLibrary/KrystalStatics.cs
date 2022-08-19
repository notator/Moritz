using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Krystals4ObjectLibrary
{
    /// <summary>
    /// The static class K contains application-wide constants and enum definitions,
    /// together with generally useful functions.
    /// </summary>
    public static class K
    {
		static K() // cribbed from CapXML.Utilities
		{
            CultureInfo ci = new CultureInfo("en-US", false);
            _numberFormat = ci.NumberFormat;
            _dateTimeFormat = ci.DateTimeFormat;

            KrystalsFolder = M.LocalMoritzKrystalsFolder;
            ExpansionOperatorsFolder = M.LocalMoritzExpansionFieldsFolder;
            ModulationOperatorsFolder = M.LocalMoritzModulationOperatorsFolder;
            // The Schemas location is a programmer's preference. The user need not bother with it.
			MoritzXmlSchemasFolder = M.OnlineXMLSchemasFolder;
            KrystalsSVGFolder = M.LocalMoritzKrystalsSVGFolder;
        }

		public static Krystal LoadKrystal(string pathname)
		{
			Krystal krystal = null;
            string filename = Path.GetFileName(pathname);
            if(IsConstantKrystalFilename(filename))
				krystal = new ConstantKrystal(pathname);
            else if(IsLineKrystalFilename(filename))
				krystal = new LineKrystal(pathname);
            else if(IsExpansionKrystalFilename(filename))
                krystal = new ExpansionKrystal(pathname);
            else if(IsModulationKrystalFilename(filename))
				krystal = new ModulationKrystal(pathname);
            else if(IsPermutationKrystalFilename(filename))
                krystal = new PermutationKrystal(pathname);
                
			else
			{
				string msg = pathname + "\r\n\r\n is not a known type of krystal.";
				MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

			return krystal;
		}

        public static Krystal GetKrystal(string krystalFileName)
        {
            Krystal krystal = null;
            try
            {
                string krystalPath = KrystalsFolder + @"\" + krystalFileName;
                krystal = K.LoadKrystal(krystalPath);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error loading krystal.\n\n" + ex.Message);
                krystal = null;
            }
            return krystal;
        }


        /// <summary>
        /// returns an array containing density values (greater than or equal to 1, and less than or equal
        /// to density), describing a contour.
        /// </summary>
        /// <param name="density">A value greater than 0, and less than or equal to 7</param>
        /// <param name="contourNumberMod12">A value greater than or equal to 1, and less than or equal to 12</param>
        /// <param name="axisNumberMod12">A value greater than or equal to 1, and less than or equal to 12</param>
        /// <returns></returns>
        public static int[] Contour(int density, int contourNumberMod12, int axisNumberMod12)
        {
            Debug.Assert( density > 0 && density <= 7
                && contourNumberMod12 > 0 && contourNumberMod12 <= 12
                && axisNumberMod12 > 0 && axisNumberMod12 <= 12);

            #region contours
            // These arrays are the contours those used for 'beyond the symbolic' - notebook July 1986
            int[, ,] dom2Contours = new int[,,] 
            {
                {{1,2},{2,1}},
                {{2,1},{1,2}}
            };
            int[, ,] dom3Contours = new int[,,] 
            {
                {{1,2,3},{2,1,3},{3,2,1},{3,1,2}},
                {{2,1,3},{1,2,3},{3,1,2},{3,2,1}},
                {{3,2,1},{2,3,1},{1,2,3},{1,3,2}},
                {{2,3,1},{3,2,1},{1,3,2},{1,2,3}}
            };
            int[, ,] dom4Contours = new int[,,] 
            {
                {{1,2,3,4},{2,1,3,4},{4,2,1,3},{4,3,2,1},{4,3,1,2},{3,1,2,4}},
                {{2,1,3,4},{1,2,3,4},{4,1,2,3},{4,3,1,2},{4,3,2,1},{3,2,1,4}},
                {{3,2,4,1},{2,3,4,1},{1,2,3,4},{1,4,2,3},{1,4,3,2},{4,3,2,1}},
                {{4,3,2,1},{3,4,2,1},{1,3,4,2},{1,2,3,4},{1,2,4,3},{2,4,3,1}},
                {{3,4,2,1},{4,3,2,1},{1,4,3,2},{1,2,4,3},{1,2,3,4},{2,3,4,1}},
                {{2,3,1,4},{3,2,1,4},{4,3,2,1},{4,1,3,2},{4,1,2,3},{1,2,3,4}},
            };
            int[, ,] dom5Contours = new int[,,] 
            {
                {{1,2,3,4,5},{2,1,3,4,5},{4,2,1,3,5},{5,4,2,1,3},{5,4,3,2,1},{5,4,3,1,2},{5,3,1,2,4},{3,1,2,4,5}},
                {{2,1,3,4,5},{1,2,3,4,5},{4,1,2,3,5},{5,4,1,2,3},{5,4,3,1,2},{5,4,3,2,1},{5,3,2,1,4},{3,2,1,4,5}},
                {{3,2,4,1,5},{2,3,4,1,5},{1,2,3,4,5},{5,1,2,3,4},{5,1,4,2,3},{5,1,4,3,2},{5,4,3,2,1},{4,3,2,1,5}},
                {{4,3,5,2,1},{3,4,5,2,1},{2,3,4,5,1},{1,2,3,4,5},{1,2,5,3,4},{1,2,5,4,3},{1,5,4,3,2},{5,4,3,2,1}},
                {{5,4,3,2,1},{4,5,3,2,1},{2,4,5,3,1},{1,2,4,5,3},{1,2,3,4,5},{1,2,3,5,4},{1,3,5,4,2},{3,5,4,2,1}},
                {{4,5,3,2,1},{5,4,3,2,1},{2,5,4,3,1},{1,2,3,4,5},{1,2,3,5,4},{1,2,3,4,5},{1,3,4,5,2},{3,4,5,2,1}},
                {{3,4,2,5,1},{4,3,2,5,1},{5,4,3,2,1},{1,5,4,3,2},{1,5,2,4,3},{1,5,2,3,4},{1,2,3,4,5},{2,3,4,5,1}},
                {{2,3,1,4,5},{3,2,1,4,5},{4,3,2,1,5},{5,4,3,2,1},{5,4,1,3,2},{5,4,1,2,3},{5,1,2,3,4},{1,2,3,4,5}}
            };
            int[, ,] dom6Contours = new int[,,] 
            {
                {{1,2,3,4,5,6},{2,1,3,4,5,6},{4,2,1,3,5,6},{6,4,2,1,3,5},{6,5,4,2,1,3},  {6,5,4,3,2,1},{6,5,4,3,1,2},{6,5,3,1,2,4},{5,3,1,2,4,6},{3,1,2,4,5,6}},
                {{2,1,3,4,5,6},{1,2,3,4,5,6},{4,1,2,3,5,6},{6,4,1,2,3,5},{6,5,4,1,2,3},  {6,5,4,3,1,2},{6,5,4,3,2,1},{6,5,3,2,1,4},{5,3,2,1,4,6},{3,2,1,4,5,6}},
                {{3,2,4,1,5,6},{2,3,4,1,5,6},{1,2,3,4,5,6},{6,1,2,3,4,5},{6,5,1,2,3,4},  {6,5,1,4,2,3},{6,5,1,4,3,2},{6,5,4,3,2,1},{5,4,3,2,1,6},{4,3,2,1,5,6}},
                {{4,3,5,2,6,1},{3,4,5,2,6,1},{2,3,4,5,6,1},{1,2,3,4,5,6},{1,6,2,3,4,5},  {1,6,2,5,3,4},{1,6,2,5,4,3},{1,6,5,4,3,2},{6,5,4,3,2,1},{5,4,3,2,6,1}},
                {{5,4,6,3,2,1},{4,5,6,3,2,1},{3,4,5,6,2,1},{1,3,4,5,6,2},{1,2,3,4,5,6},  {1,2,3,6,4,5},{1,2,3,6,5,4},{1,2,6,5,4,3},{2,6,5,4,3,1},{6,5,4,3,2,1}},
                {{6,5,4,3,2,1},{5,6,4,3,2,1},{3,5,6,4,2,1},{1,3,5,6,4,2},{1,2,3,5,6,4},  {1,2,3,4,5,6},{1,2,3,4,6,5},{1,2,4,6,5,3},{2,4,6,5,3,1},{4,6,5,3,2,1}},
                {{5,6,4,3,2,1},{6,5,4,3,2,1},{3,6,5,4,2,1},{1,3,6,5,4,2},{1,2,3,6,5,4},  {1,2,3,4,6,5},{1,2,3,4,5,6},{1,2,4,5,6,3},{2,4,5,6,3,1},{4,5,6,3,2,1}},
                {{4,5,3,6,2,1},{5,4,3,6,2,1},{6,5,4,3,2,1},{1,6,5,4,3,2},{1,2,6,5,4,3},  {1,2,6,3,5,4},{1,2,6,3,4,5},{1,2,3,4,5,6},{2,3,4,5,6,1},{3,4,5,6,2,1}},
                {{3,4,2,5,1,6},{4,3,2,5,1,6},{5,4,3,2,1,6},{6,5,4,3,2,1},{6,1,5,4,3,2},  {6,1,5,2,4,3},{6,1,5,2,3,4},{6,1,2,3,4,5},{1,2,3,4,5,6},{2,3,4,5,1,6}},
                {{2,3,1,4,5,6},{3,2,1,4,5,6},{4,3,2,1,5,6},{6,4,3,2,1,5},{6,5,4,3,2,1},  {6,5,4,1,3,2},{6,5,4,1,2,3},{6,5,1,2,3,4},{5,1,2,3,4,6},{1,2,3,4,5,6}}
            };
            int[, ,] dom7Contours = new int[,,] 
            {
                {{1,2,3,4,5,6,7},{2,1,3,4,5,6,7},{4,2,1,3,5,6,7},{6,4,2,1,3,5,7},{7,6,4,2,1,3,5},{7,6,5,4,2,1,3},  {7,6,5,4,3,2,1},{7,6,5,4,3,1,2},{7,6,5,3,1,2,4},{7,5,3,1,2,4,6},{5,3,1,2,4,6,7},{3,1,2,4,5,6,7}},
                {{2,1,3,4,5,6,7},{1,2,3,4,5,6,7},{4,1,2,3,5,6,7},{6,4,1,2,3,5,7},{7,6,4,1,2,3,5},{7,6,5,4,1,2,3},  {7,6,5,4,3,1,2},{7,6,5,4,3,2,1},{7,6,5,3,2,1,4},{7,5,3,2,1,4,6},{5,3,2,1,4,6,7},{3,2,1,4,5,6,7}},
                {{3,2,4,1,5,6,7},{2,3,4,1,5,6,7},{1,2,3,4,5,6,7},{6,1,2,3,4,5,7},{7,6,1,2,3,4,5},{7,6,5,1,2,3,4},  {7,6,5,1,4,2,3},{7,6,5,1,4,3,2},{7,6,5,4,3,2,1},{7,5,4,3,2,1,6},{5,4,3,2,1,6,7},{4,3,2,1,5,6,7}},
                {{4,3,5,2,6,1,7},{3,4,5,2,6,1,7},{2,3,4,5,6,1,7},{1,2,3,4,5,6,7},{7,1,2,3,4,5,6},{7,1,6,2,3,4,5},  {7,1,6,2,5,3,4},{7,1,6,2,5,4,3},{7,1,6,5,4,3,2},{7,6,5,4,3,2,1},{6,5,4,3,2,1,7},{5,4,3,2,6,1,7}},
                {{5,4,6,3,7,2,1},{4,5,6,3,7,2,1},{3,4,5,6,7,2,1},{2,3,4,5,6,7,1},{1,2,3,4,5,6,7},{1,2,7,3,4,5,6},  {1,2,7,3,6,4,5},{1,2,7,3,6,5,4},{1,2,7,6,5,4,3},{1,7,6,5,4,3,2},{7,6,5,4,3,2,1},{6,5,4,3,7,2,1}},
                {{6,5,7,4,3,2,1},{5,6,7,4,3,2,1},{4,5,6,7,3,2,1},{2,4,5,6,7,3,1},{1,2,4,5,6,7,3},{1,2,3,4,5,6,7},  {1,2,3,4,7,5,6},{1,2,3,4,7,6,5},{1,2,3,7,6,5,4},{1,3,7,6,5,4,2},{3,7,6,5,4,2,1},{7,6,5,4,3,2,1}},
                {{7,6,5,4,3,2,1},{6,7,5,4,3,2,1},{4,6,7,5,3,2,1},{2,4,6,7,5,3,1},{1,2,4,6,7,5,3},{1,2,3,4,6,7,5},  {1,2,3,4,5,6,7},{1,2,3,4,5,7,6},{1,2,3,5,7,6,4},{1,3,5,7,6,4,2},{3,5,7,6,4,2,1},{5,7,6,4,3,2,1}},
                {{6,7,5,4,3,2,1},{7,6,5,4,3,2,1},{4,7,6,5,3,2,1},{2,4,7,6,5,3,1},{1,2,4,7,6,5,3},{1,2,3,4,7,6,5},  {1,2,3,4,5,7,6},{1,2,3,4,5,6,7},{1,2,3,5,6,7,4},{1,3,5,6,7,4,2},{3,5,6,7,4,2,1},{5,6,7,4,3,2,1}},
                {{5,6,4,7,3,2,1},{6,5,4,7,3,2,1},{7,6,5,4,3,2,1},{2,7,6,5,4,3,1},{1,2,7,6,5,4,3},{1,2,3,7,6,5,4},  {1,2,3,7,4,6,5},{1,2,3,7,4,5,6},{1,2,3,4,5,6,7},{1,3,4,5,6,7,2},{3,4,5,6,7,2,1},{4,5,6,7,3,2,1}},
                {{4,5,3,6,2,7,1},{5,4,3,6,2,7,1},{6,5,4,3,2,7,1},{7,6,5,4,3,2,1},{1,7,6,5,4,3,2},{1,7,2,6,5,4,3},  {1,7,2,6,3,5,4},{1,7,2,6,3,4,5},{1,7,2,3,4,5,6},{1,2,3,4,5,6,7},{2,3,4,5,6,7,1},{3,4,5,6,2,7,1}},
                {{3,4,2,5,1,6,7},{4,3,2,5,1,6,7},{5,4,3,2,1,6,7},{6,5,4,3,2,1,7},{7,6,5,4,3,2,1},{7,6,1,5,4,3,2},  {7,6,1,5,2,4,3},{7,6,1,5,2,3,4},{7,6,1,2,3,4,5},{7,1,2,3,4,5,6},{1,2,3,4,5,6,7},{2,3,4,5,1,6,7}},
                {{2,3,1,4,5,6,7},{3,2,1,4,5,6,7},{4,3,2,1,5,6,7},{6,4,3,2,1,5,7},{7,6,4,3,2,1,5},{7,6,5,4,3,2,1},  {7,6,5,4,1,3,2},{7,6,5,4,1,2,3},{7,6,5,1,2,3,4},{7,5,1,2,3,4,6},{5,1,2,3,4,6,7},{1,2,3,4,5,6,7}}
            };
            int[][, ,] contours = new int[6][, ,];
            contours[0] = dom2Contours;
            contours[1] = dom3Contours;
            contours[2] = dom4Contours;
            contours[3] = dom5Contours;
            contours[4] = dom6Contours;
            contours[5] = dom7Contours;

            int[,] rectifiedCoordinates =
            {
                {1,1,1,1,1,1,2,2,2,2,2,2},
                {1,1,1,2,2,2,3,3,3,4,4,4},
                {1,1,2,2,3,3,4,4,5,5,6,6},
                {1,1,2,3,3,4,5,5,6,7,7,8},
                {1,1,2,3,4,5,6,6,7,8,9,10},
                {1,2,3,4,5,6,7,8,9,10,11,12}
            };
            #endregion
            int[] contour = { 0, 0, 0, 0, 0, 0, 0 }; 
            if(density == 1)
            {
                contour[0] = 1;
            }
            else
            {
                int axisNumber = rectifiedCoordinates[density - 2, axisNumberMod12 - 1];
                int contourNumber = rectifiedCoordinates[density - 2, contourNumberMod12 - 1];
                for(int j = 0; j < density; j++)
                {
                    contour[j] = contours[density - 2][contourNumber - 1, axisNumber - 1, j];
                }
            }

            return contour;
        }

        #region Regex Filename verification
        public static bool IsConstantKrystalFilename(string name)
        {
            return Regex.IsMatch(name, @"^[0-9]+[.][0-9_]+[.][0-9]+[.]constant\.krys$");
        }
        public static bool IsLineKrystalFilename(string name)
        {
            return Regex.IsMatch(name, @"^[0-9]+[.][0-9_]+[.][0-9]+[.]line\.krys$");
        }
        public static bool IsExpansionKrystalFilename(string name)
        {
            return Regex.IsMatch(name, @"^[0-9]+[.][0-9_]+[.][0-9]+[.][0-9]+[.]exp\.krys$");
        }
        public static bool IsModulationKrystalFilename(string name)
        {
            return Regex.IsMatch(name, @"^[0-9]+[.][0-9_]+[.][0-9]+[.]mod\.krys$");
        }
        public static bool IsPermutationKrystalFilename(string name)
        {
            return Regex.IsMatch(name, @"^[0-9]+[.][0-9_]+[.][0-9]+[.]perm\.krys$");
        }
        public static bool IsPathKrystalFilename(string name)
        {
            return Regex.IsMatch(name, @"^[0-9]+[.][0-9_]+[.][0-9]+[.]path\.krys$");
        }
        /// <summary>
        /// This is the same definition as in the krystals.xsd schema
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsKrystalFilename(string name)
        {
            return Regex.IsMatch(name, @"^(([0-9]+[.][0-9_]+[.][0-9]+[.](constant|line|path|mod|perm))|([0-9]+[.][0-9_]+[.][0-9]+[.][0-9]+[.](exp)))(\.krys)$");
        }
        public static bool IsModulationOperatorFilename(string name)
        {
            return Regex.IsMatch(name, @"^[m]([0-9])*[x]([0-9])*[(]([0-9])*[)][-]([0-9])*[.][k][m][o][d]$");
        }
        public static bool IsExpansionOperatorFilename(string name)
        {
            return Regex.IsMatch(name, @"^[e][(]([0-9])+[.]([0-9])+[.]([0-9])+[)][.][k][e][x][p]$");
        }
        public static bool IsKrystalOperatorFilename(string name)
        {
            return (IsExpansionOperatorFilename(name) || IsModulationOperatorFilename(name));
        }
        #endregion

        #region krystalName information
        public static int KrystalMaxValue(string krystalName)
        {
            char[] dot = new char[] { '.' };

            var components = krystalName.Split(dot);

            return int.Parse(components[0]);
        }

        public static List<int> KrystalShape(string krystalName)
        {
            char[] dot = new char[] { '.' };
            char[] underline = new char[] { '_' };

            var components = krystalName.Split(dot);
            var shapeComponents = components[1].Split(underline);

            var rval = new List<int>();
            foreach(var value in shapeComponents)
            {
                rval.Add(int.Parse(value));
            }

            return rval;
        }

        public static int KrystalLevel(string krystalName)
        {
            char[] dot = new char[] { '.' };
            char[] underline = new char[] { '_' };

            var components = krystalName.Split(dot);
            var shapeComponents = components[1].Split(underline);
            int level;

            switch(shapeComponents.Length)
            {
                case 1:
                    level = 0;
                    break;
                case 2:
                    level = int.Parse(shapeComponents[0]); // 1 or 2
                    break;
                default:
                    level = shapeComponents.Length;
                    break;
            }
            return level;
        }

        public static int KrystalNameIndex(string krystalName)
        {
            char[] dot = new char[] { '.' };

            var components = krystalName.Split(dot);

            return int.Parse(components[3]);
        }

        public static string KrystalTypeString(string krystalName)
        {
            char[] dot = new char[] { '.' };

            var components = krystalName.Split(dot);

            return components[4];
        }

        public static KrystalType KrystalTypeEnum(string krystalName)
        {
            char[] dot = new char[] { '.' };

            var components = krystalName.Split(dot);

            Enum.TryParse(components[4], out KrystalType krystalType);

            return krystalType;
        }

        #endregion

        public static void RebuildKrystalFamily()
        {
            KrystalFamily kFamily = new KrystalFamily(K.KrystalsFolder);
            kFamily.Rebuild();
            MessageBox.Show("All krystals have been successfully recreated",
                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// If krystalNameList is a list of krystal names, it can be sorted into ascending order by calling
        ///     krystalNameList.Sort(K.CompareKrystalNames);
        ///
        /// Throws an exception if one or both of its arguments is not a krystalName.
        /// </summary
        /// <returns>0 if the names are equal,
        /// -1 if krystalName1 is less than krystalName2,
        /// 1 if krystalName1 is greater than krystalname2.
        /// </returns>
        public static int CompareKrystalNames(string krystalName1, string krystalName2)
		{
            if(!IsKrystalFilename(krystalName1) || !IsKrystalFilename(krystalName2))
            {
                throw new ApplicationException("Argument to K:CompareKrystalNames() was not a krystal file name");
            }

            int rval = String.Compare(krystalName1, krystalName2);

            // The sort criteria are: domain->level->shape->type->index
            if(rval != 0)
            {
                rval = 1; // assume krystalName1 > krystalName2
                GetNameInfo(krystalName1, out int domain1, out string[] shapeComponents1, out int index1, out string type1);
                GetNameInfo(krystalName2, out int domain2, out string[] shapeComponents2, out int index2, out string type2);

                var shapeComparison = CompareKrystalShapes(shapeComponents1, shapeComponents2);
                var typeComparison = CompareKrystalTypes(type1, type2);

                if((domain1 < domain2)
                || (domain1 == domain2 && shapeComparison < 0)
                || (domain1 == domain2 && shapeComparison == 0 && typeComparison < 0)
                || (domain1 == domain2 && shapeComparison == 0 && typeComparison == 0 && index1 < index2))
                {
                    rval = -1;
                }
            }

			return rval;
		}

        /// <summary>
        /// returns 0 if the shape components are equal
        /// -1 if shapeComponents1 less than shapeComponents2
        /// 1 if shapeComponents1 greater than shapeComponents2
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static int CompareKrystalShapes(string[] shapeComponents1, string[] shapeComponents2)
        {
            var rval = 0;
            if(shapeComponents1.Length < shapeComponents2.Length)
                rval = -1;
            else if(shapeComponents1.Length > shapeComponents2.Length)
                rval = 1;
            else // components are the same length
            {
                for(int i = 0; i < shapeComponents1.Length; i++)
                {
                    var int1 = int.Parse(shapeComponents1[i]);
                    var int2 = int.Parse(shapeComponents2[i]);
                    if(int1 == int2)
                    {
                        continue;
                    }
                    else if(int1 < int2)
                    {
                        rval = -1;
                        break;
                    }
                    else if(int1 > int2)
                    {
                        rval = 1;
                        break;
                    }
                }   
            }

            return rval;
        }

        /// <summary>
        /// Krystal type names are sorted in the order they are defined in the enum KrystalType.
        /// </summary>
        /// <returns></returns>
        private static int CompareKrystalTypes(string type1, string type2)
        {
            Enum.TryParse(type1, out KrystalType t1);
            Enum.TryParse(type2, out KrystalType t2);

            int rval = 0; // assume the types are equal
            
            if(t1 < t2) 
            {
                rval = -1;
            }
            else if(t1 > t2)
            {
                rval = 1;
            }
            return rval;
        }        

        private static void GetNameInfo(string krystalName1, out int domain, out string[] shapeComponents, out int index, out string type )
        {
            var dot = new char[] { '.' };
            var underscore = new char[] { '_' };

            var components1 = krystalName1.Split(dot);
            domain = int.Parse(components1[0]);
            shapeComponents = components1[1].Split(underscore);
            index = int.Parse(components1[2]);
            type = components1[3];
        }

        private static int CompareDomains(string domStr1, string domStr2)
        {
            int rval = 0;
            try
            {
                int dom1 = int.Parse(domStr1);
                int dom2 = int.Parse(domStr2);
                rval = K.CompareInts(dom1, dom2);
            }
            catch
            {
                throw new ApplicationException("Error comparing domains in krystal names.");
            }
            return rval;
        }

        /// <summary>
        /// A field string consists of three integers separated by stops. e.g. "7.12.1"
        /// The three numbers are compared individually.
        /// </summary>
        /// <param name="fieldStr1"></param>
        /// <param name="fieldStr2"></param>
        /// <returns></returns>
        private static int CompareFields(string fieldStr1, string fieldStr2)
        {
            int rval = 0;
            int f1Dot1Index = fieldStr1.IndexOf('.');
            int f1Dot2Index = fieldStr1.IndexOf('.', f1Dot1Index + 1);
            int f2Dot1Index = fieldStr2.IndexOf('.');
            int f2Dot2Index = fieldStr2.IndexOf('.', f2Dot1Index + 1);

            string f1Num1Str = fieldStr1.Substring(0, f1Dot1Index);
            string f2Num1Str = fieldStr2.Substring(0, f2Dot1Index);
            try
            {
                int f1Num1 = int.Parse(f1Num1Str);
                int f2Num1 = int.Parse(f2Num1Str);
                rval = K.CompareInts(f1Num1, f2Num1);
                if(rval == 0) // Num1s are the same, now try Num2s
                {
                    string f1Num2Str = fieldStr1.Substring(f1Dot1Index + 1, f1Dot2Index - f1Dot1Index - 1);
                    string f2Num2Str = fieldStr2.Substring(f2Dot1Index + 1, f2Dot2Index - f2Dot1Index - 1);
                    int f1Num2 = int.Parse(f1Num2Str);
                    int f2Num2 = int.Parse(f2Num2Str);
                    rval = K.CompareInts(f1Num2, f2Num2);
                    if(rval == 0) // Num2s are the same, now try Num3s
                    {
                        string f1Num3Str = fieldStr1.Substring(f1Dot2Index + 1);
                        string f2Num3Str = fieldStr2.Substring(f2Dot2Index + 1);
                        int f1Num3 = int.Parse(f1Num3Str);
                        int f2Num3 = int.Parse(f2Num3Str);
                        rval = K.CompareInts(f1Num3, f2Num3);
                    }
                }
            }
            catch
            {
                throw new ApplicationException("Error comparing fields in krystal names.");
            }
            return rval;
        }

        private static int CompareInts(int int1, int int2)
        {
            int rval = 0;
            if(int1 < int2)
                rval = -1;
            else if(int1 > int2)
                rval = 1;
            else if(int1 == int2)
                rval = 0;
            return rval;
        }

        /// <summary>
        /// If moritzName is a known Moritz filename type, the file index is returned, otherwise zero.
        /// </summary>
        public static uint FileIndex(string moritzName)
        {
            uint result = 0;
            string resultStr = "";
            if(K.IsConstantKrystalFilename(moritzName))
                result = 0;
            else if(K.IsLineKrystalFilename(moritzName) || K.IsModulationKrystalFilename(moritzName)
            || K.IsExpansionKrystalFilename(moritzName) || K.IsModulationOperatorFilename(moritzName))
            {
                int startindex = moritzName.IndexOf('-') + 1;
                int endindex = moritzName.Length - 5;
                resultStr = moritzName.Substring(startindex, endindex - startindex);
                result = uint.Parse(resultStr);
            }
            else if(K.IsExpansionOperatorFilename(moritzName))
            {
                int endindex = moritzName.IndexOf(')');
                int startindex = endindex - 1;
                while(Char.IsDigit(moritzName[startindex]))
                    startindex--;
                startindex++;
                resultStr = moritzName.Substring(startindex, endindex - startindex);
                result = uint.Parse(resultStr);
            }
            return result;
        }
        /// <summary>
        /// If expanderName is a well formed expander name, its input domain is returned, otherwise zero.
        /// </summary>
        public static uint ExpansionOperatorInputDomain(string expanderName)
        {
            uint result = 0;
            if(K.IsExpansionOperatorFilename(expanderName))
            {
                int startindex = 2;
                int endindex = 3;
                while(Char.IsDigit(expanderName[endindex]))
                    endindex++;
                string resultStr = expanderName.Substring(startindex, endindex - startindex);
                result = uint.Parse(resultStr);
            }
            return result;
        }

        /// <summary>
        /// If expanderName is a well formed expander name, its output domain is returned, otherwise zero.
        /// </summary>
        public static uint ExpansionOperatorOutputDomain(string expanderName)
        {
            uint result = 0;
            if(K.IsExpansionOperatorFilename(expanderName))
            {
                int startindex = expanderName.IndexOf('.') + 1;
                int endindex = startindex + 1;
                while(Char.IsDigit(expanderName[endindex]))
                    endindex++;
                string resultStr = expanderName.Substring(startindex, endindex - startindex);
                result = uint.Parse(resultStr);
            }
            return result;
        }


        #region Loading and saving gametes and strands from files
        /// <summary>
        /// Reads to the next start or end tag having a name which is in the parameter list.
        /// When this function returns, XmlReader.Name is the name of the start or end tag found.
        /// If none of the names in the parameter list is found, an exception is thrown with a diagnostic message.
        /// </summary>
        /// <param name="r">XmlReader</param>
        /// <param name="possibleElements">Any number of strings, separated by commas</param>
        public static void ReadToXmlElementTag(XmlReader r, params string[] possibleElements)
        {
            List<string> elementNames = new List<string>(possibleElements);
            do
            {
                r.Read();
            } while (!elementNames.Contains(r.Name) && !r.EOF);
            if (r.EOF)
            {
                StringBuilder msg = new StringBuilder("Error reading Xml file:\n"
                    + "None of the following elements could be found:\n");
                foreach(string s in elementNames)
                    msg.Append( s.ToString() + "\n");
                throw new ApplicationException(msg.ToString());
            }
        }
        /// <summary>
        /// Gets a file path from a standard OpenFileDialog, filtering for various types of file.
        /// </summary>
        /// <returns>A path to a krystal, expander, modulator or SVG file - or an empty string if the user cancels the dialog.</returns>
        public static string GetFilepathFromOpenFileDialog(DialogFilterIndex defaultFilterIndex)
        {
            string pathname = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            switch(defaultFilterIndex)
            {
                case DialogFilterIndex.allKrystals:
                case DialogFilterIndex.constant:
                case DialogFilterIndex.line:
                case DialogFilterIndex.expansion:
                case DialogFilterIndex.modulation:
                case DialogFilterIndex.permutation:
                case DialogFilterIndex.path:
                    openFileDialog.InitialDirectory = K.KrystalsFolder;
                    break;
                case DialogFilterIndex.expander:
                    openFileDialog.InitialDirectory = K.ExpansionOperatorsFolder;
                    break;
                case DialogFilterIndex.modulator:
                    openFileDialog.InitialDirectory = K.ModulationOperatorsFolder;
                    break;
                case DialogFilterIndex.svg:
                    openFileDialog.InitialDirectory = K.KrystalsSVGFolder;
                    break;
            }

            openFileDialog.Filter = DialogFilter;
            openFileDialog.FilterIndex = (int)defaultFilterIndex + 1;
            openFileDialog.Title = "Open file";
            openFileDialog.RestoreDirectory = true;

            if(openFileDialog.ShowDialog() == DialogResult.OK)
                pathname = openFileDialog.FileName;
            return pathname;
        }
        #endregion  Loading and saving gametes and strands from files
        /// <summary>
        /// Returns the portion of a string enclosed in single brackets (...), including the brackets.
        /// Used by expansion krystals and expanders, and by modulation krystals and modulators.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string FieldSignature(string filename)
        {
            return filename.Substring(filename.IndexOf('('), filename.IndexOf(')') - filename.IndexOf('(') + 1);
        }
        /// <summary>
        /// returns the filename of the expansion operator associated with the given expansion krystal
        /// </summary>
        public static string ExpansionOperatorFilename(string expansionKrystalFilename)
        {
            string expanderSignature = FieldSignature(expansionKrystalFilename);
            return String.Format("e{0}{1}", expanderSignature, K.ExpanderFilenameSuffix);
        }
        /// <summary>
        /// Converts a List of uints to a string containing the uints separated by spaces
        /// </summary>
        /// <param name="uintList"></param>
        /// <returns>the string</returns>
        public static string GetStringOfUnsignedInts(List<uint> uintList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (uint ui in uintList)
            {
                sb.Append(ui);
                sb.Append(" ");
            }
            if (uintList.Count > 0)
                sb.Remove(sb.Length - 1, 1); // remove the final space
            return sb.ToString();
        }
        /// <summary>
        /// Converts a List of ints to a string containing the ints separated by spaces.
        /// Throws an exception if the intList contains a negative number.
        /// </summary>
        /// <param name="uintList"></param>
        /// <returns>the string</returns>
        public static string GetStringOfUnsignedInts(List<int> intList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (int i in intList)
            {
                if (i < 0)
                    throw new ApplicationException("Attempt to store a negative number in a string of unsigned integers.");
                sb.Append(i);
                sb.Append(" ");
            }
            if (intList.Count > 0)
                sb.Remove(sb.Length - 1, 1); // remove the final space
            return sb.ToString();
        }
        /// <summary>
        /// Converts a List of uints to a string containing the uints separated by spaces
        /// </summary>
        /// <param name="uintList"></param>
        /// <returns>the string</returns>
        public static string GetStringOfUnsignedInts(uint[] uintArray)
        {
            if (uintArray.Length > 0)
            {
                List<uint> uintList = new List<uint>();
                for (int i = 0; i < uintArray.Length; i++)
                    uintList.Add(uintArray[i]);
                return GetStringOfUnsignedInts(uintList);
            }
            else return "";
        }
        /// <summary>
		/// Converts a string of numeric digits and separators (whitespace, newlines, tabs etc.)
		/// to a List of uint. Throws an ApplicationException if an illegal character is found.
		/// </summary>
		/// <param name="str">The string to be converted</param>
		/// <returns>a List of unsigned integers</returns>
		public static List<uint> GetUIntList(string str)
		{
			List<uint> values = new List<uint>(); 
			
			str = str.Trim();	// removes white space from begin and end of the string
			if( str.Length > 0 )
			{
				StringBuilder s = new StringBuilder();
				bool inSeparator = false;

				// convert internal separators (spaces, newlines, tabs etc) to single spaces
				foreach( Char c in str )
				{
					if( Char.IsDigit(c) )
					{
						s.Append(c);
						inSeparator = false;
					}
					else if( Char.IsWhiteSpace(c) || Char.IsControl(c) )
					{
						if( !inSeparator )
						{
							s.Append(' ');
							inSeparator = true;
						}
					}
					else throw new ApplicationException("Illegal character in list of unsigned integer values.");
				}

				str = s.ToString();
				string[] valueStrings = str.Split(' ');

				foreach( string valStr in valueStrings )
					values.Add(uint.Parse(valStr));
			}

			return values;
		}
        /// <summary>
        /// Convert a float to a string using the static en-US number format.
        /// This function is used when writing XML files.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string FloatToAttributeString(float floatValue)
        {
            return floatValue.ToString("0.###", _numberFormat);
        }
        /// <summary>
        /// Convert a string to a float using the static en-US number format.
        /// This function is used when reading XML files.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float StringToFloat(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0.0f;
            else
                return (float)Convert.ToDouble(value, _numberFormat);
        }

        /// <summary>
        /// Returns the krystalName's domainString component.
        /// </summary>
        public static string GetDomainStringFromKrystalName(string krystalName)
        {
            char[] dot = new char[] { '.' };
            var components = krystalName.Split(dot);
            return components[0];
        }

        /// <summary>
        /// Returns the krystalName's domain as an int.
        /// </summary>
        public static int GetDomainFromFirstComponent(string krystalName)
        {
            char[] dot = new char[] { '.' };
            var components = krystalName.Split(dot);
            int.TryParse(components[0], out int domain);
            return domain;
        }

        /// <summary>
        /// Returns the krystalName's shapeString component.
        /// </summary>
        public static string GetShapeStringFromKrystalName(string krystalName)
        {
            char[] dot = new char[] { '.' };
            var components = krystalName.Split(dot);
            return components[1];
        }

        /// <summary>
        /// Returns a normalised list in which the first element is always 0 or 1.
        /// </summary>
        public static List<int> GetShapeFromKrystalName(string krystalName)
        {
            char[] dot = new char[] { '.' };
            var components = krystalName.Split(dot);
            var rval = GetShapeFromShapeString(components[1]);
            return rval;
        }

        /// <summary>
        /// Returns a normalised list in which the first element is always 0 or 1.
        /// </summary>
        public static List<int> GetShapeFromShapeString(string shapeString)
        {
            char[] underline = new char[] { '_' };
            var shapeComponents = shapeString.Split(underline);
            var rval = new List<int>();
            foreach(var subString in shapeComponents)
            {
                int subInt;
                int.TryParse(subString, out subInt);
                rval.Add(subInt);
            }
            if(rval[0] != 0 && rval[0] != 1)
            {
                rval.Insert(0, 1);
            }
            if(rval[0] == 0 && rval.Count > 1)
            {
                throw new ApplicationException("Error in ConstantKrystal shape.");
            }
            return rval;
        }

        public static string Now
        {
            get { return DateTime.Today.ToString("dddd dd.MM.yyyy", _dateTimeFormat) + ", " + DateTime.Now.ToLongTimeString();}
        }
        #region public variables
        //the following three set from Moritz.Preferences in the above constructor
        public static readonly string KrystalsFolder = "";
        public static readonly string ExpansionOperatorsFolder = "";
        public static readonly string ModulationOperatorsFolder = "";
        public static readonly string MoritzXmlSchemasFolder = "";
        public static readonly string KrystalsSVGFolder = "";

        public static readonly string UntitledExpanderName = "Untitled.kexp";
        //public static readonly string UntitledModulatorName = "Untitled.kmod";
        public static readonly string DialogFilter =
            "all krystals (*.krys)|*.krys|"
            + "constants (*.constant.krys)|*.constant.krys|"
            + "lines (*.line.krys)|*.line.krys|"
            + "expansions (*.exp.krys)|*.exp.krys|"
            + "modulations (*.mod.krys)|*.mod.krys|"
            + "permutations (*.perm.krys)|*.perm.krys|"
            + "paths (*.path.krys)|*.path.krys|"
            + "expanders (*.kexp) |*.kexp|"
            + "modulators (*.kmod) |*.kmod|"
            + "SVG files (*.svg) |*.svg";
        public static readonly string KrystalFilenameSuffix = ".krys";
        public static readonly string ExpanderFilenameSuffix = ".kexp";
        public static readonly string ModulatorFilenameSuffix = ".kmod";

        // These values are used as strings in krystal names to describe their type.
        // A krystal's type defines how its heredity information is stored in the file.
        public enum KrystalType
        {
            constant,
            line,
            exp,
            path,
            mod,
            perm
        };
 
        // used to index the Krystal dialog filter (see above)
        public enum DialogFilterIndex { allKrystals, constant, line, expansion, modulation, permutation, path, expander, modulator, svg };
        public enum PointGroupShape { circle, spiral, straightLine };
        public enum DisplayColor { black, red, green, blue, orange, purple, magenta };
        #endregion public variables
        #region private variables
        private static readonly NumberFormatInfo _numberFormat;
        private static readonly DateTimeFormatInfo _dateTimeFormat;
        #endregion private variables
    }
 }
