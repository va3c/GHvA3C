using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Timers;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using GHva3c.Properties;

using Newtonsoft.Json;

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

        /// <summary>
        /// Returns a JSON string representing a rhino mesh, and containing any attributes as user data
        /// </summary>
        /// <param name="mesh">The rhino mesh to serialize.  Can contain quads and tris.</param>
        /// <param name="attDict">The attribute dictionary to serialize.  Objects should all be reference types.</param>
        /// <returns>a JSON string representing a rhino mes</returns>
        public static string geoJSON(Mesh mesh, Dictionary<string, object> attDict)
        {
            //create a dynamic object to populate
            dynamic jason = new ExpandoObject();


            jason.uuid = Guid.NewGuid();
            jason.type = "Geometry";
            jason.data = new ExpandoObject();
            jason.userData = new ExpandoObject();

            //populate data object properties

            //fisrt, figure out how many faces we need based on the tri/quad count
            var quads = from q in mesh.Faces
                        where q.IsQuad
                        select q;

            jason.data.vertices = new object[mesh.Vertices.Count * 3];
            jason.data.faces = new object[(mesh.Faces.Count + quads.Count()) * 4];
            jason.data.normals = new object[0];
            jason.data.uvs = new object[0];
            jason.data.scale = 1;
            jason.data.visible = true;
            jason.data.castShadow = true;
            jason.data.receiveShadow = false;
            jason.data.doubleSided = true;

            //populate vertices
            int counter = 0;
            int i = 0;
            foreach (var v in mesh.Vertices)
            {
                jason.data.vertices[counter++] = Math.Round(mesh.Vertices[i].X * -1.0, 5);
                jason.data.vertices[counter++] = Math.Round(mesh.Vertices[i].Z, 5);
                jason.data.vertices[counter++] = Math.Round(mesh.Vertices[i].Y, 5);
                i++;
            }

            //populate faces
            counter = 0;
            i = 0;
            foreach (var f in mesh.Faces)
            {
                if (f.IsTriangle)
                {
                    jason.data.faces[counter++] = 0;
                    jason.data.faces[counter++] = mesh.Faces[i].A;
                    jason.data.faces[counter++] = mesh.Faces[i].B;
                    jason.data.faces[counter++] = mesh.Faces[i].C;
                    i++;
                }
                if (f.IsQuad)
                {
                    jason.data.faces[counter++] = 0;
                    jason.data.faces[counter++] = mesh.Faces[i].A;
                    jason.data.faces[counter++] = mesh.Faces[i].B;
                    jason.data.faces[counter++] = mesh.Faces[i].C;
                    jason.data.faces[counter++] = 0;
                    jason.data.faces[counter++] = mesh.Faces[i].C;
                    jason.data.faces[counter++] = mesh.Faces[i].D;
                    jason.data.faces[counter++] = mesh.Faces[i].A;
                    i++;
                }
            }

            //populate vertex colors
            if (mesh.VertexColors.Count != 0)
            {
                jason.data.colors = new object[mesh.Vertices.Count];
                i = 0;
                foreach (var c in mesh.VertexColors)
                {
                    jason.data.colors[i] = _Utilities.hexColor(new GH_Colour(c));
                    i++;
                }
            }


            //populate userData objects
            var attributeCollection = (ICollection<KeyValuePair<string, object>>)jason.userData;
            foreach (var kvp in attDict)
            {
                attributeCollection.Add(kvp);
            }


            return JsonConvert.SerializeObject(jason);
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

    public class va3cBaseMaterialCatcher
    {
        public string type;
    }

    //mesh phong materials
    public class va3cMeshPhongMaterialCatcher
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

    //mesh basic materials
    public class va3cMeshBasicMaterialCatcher
    {
        public string uuid;
        public string type;
        public string color;
        public int side;
        public double opacity;
        public bool transparent;
    }

    //mesh basic materials with face colors
    public class va3cMeshFaceMaterialCatcher
    {
        public string uuid;
        public string type;
        //public string color;
        //public bool transparent;
        //public bool wireframe;
        //public int side;
        public object[] materials;
    }

    //mesh lambert materials - for use with vertex colors
    public class va3cMeshLambertMaterialCatcher
    {
        public string uuid;
        public string type;
        public string color;
        public string ambient;
        public string emissive;
        public int side;
        public double opacity;
        public bool transparent;
        public int shading;

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
