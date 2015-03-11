﻿/*  HEADER
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

        public va3cElementType Type { get; set; }

        public string GeometryJson { get; set; }

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
        public Element(string json, va3cElementType type)
        {
            GeometryJson = json;
            Type = type;
        }

        public Element(string json, va3cElementType type, Material material)
        {
            GeometryJson = json;
            Type = type;
            Material = material;
        }
    }

    public enum va3cElementType
    {
        Mesh,
        Line,
        Camera
    }
}