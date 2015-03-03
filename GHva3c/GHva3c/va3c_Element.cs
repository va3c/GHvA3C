/*  HEADER
 * 
 * ELEMENT CLASS  
 *  A high level class to inherit from.  Provides fields that all model elements must have.
 *  
 *  3/3/15
 *  
 * Ana Garcia Puyol
 *  
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;


using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using GHva3c.Properties;


using Newtonsoft.Json;

namespace GHva3c
{

    public class Element
    {
        //ATTRIBUTES

        private string myID;

        public eType Type { get; set; }

        public string Json { get; set; }

        public Material Material { get; set; }


        //PROPERTIES
        //[DataMember]
        public string ID
        {
            get { return myID; }
            set
            {
                try
                {
                    //test for the empty string
                    if (value == "")
                    {
                        throw new ArgumentException("The input string cannot be empty");
                    }

                    myID = value;
                }

                catch (Exception e) //should catch the null case
                {
                    throw e;
                }
            }
        }

        public Element() { }
        public Element(string json, eType type)
        {
            Json = json;
            Type = type;
        }
    }

    public enum eType
    {
        Mesh,
        Line,
        Camera,
        Scene
    }
}
