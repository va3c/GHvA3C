using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GHva3c
{
    /// <summary>
    /// Contains utility functions to be called from GH component classes
    /// </summary>
    public class _Utilities
    {
        /// <summary>
        /// Returns a string representation of a hex color given a GH_Colour object
        /// </summary>
        /// <param name="ghColor">the grasshopper color to convert</param>
        /// <returns>a hex color string</returns>
        public static string hexColor(GH_Colour ghColor)
        {
            string hexStr = "0x" + ghColor.Value.R.ToString("X2") +
                ghColor.Value.G.ToString("X2") +
                ghColor.Value.B.ToString("X2");

            return hexStr;
        }
    }


    //below are a number of Catcher classes which are used to deserialize JSON objects
    //mostly called from the va3c_CompileScene component


    public class va3cGeometryCatcher
    {
        public string uuid;
        public string type;
        public object data;
    }

    public class va3cMaterialCatcher
    {
        public string uuid;
        public string type;
        public string color;
        public string ambient;
        public string emissive;
        public string specular;
        public double shininess;
        public double opacity;
        public bool transparent;
        public bool wireframe;
        public int side;
    }

    public class va3cAttributesCatcher
    {
        public object userData;
    }

    public class va3cLineCatcher
    {
        public string uuid;
        public string type;
        public object data;
    }

    public class va3cLineBasicMaterialCatcher
    {
        public string uuid;
        public string type;
        public string color;
        public double linewidth;
        public double opacity;
    }
}
