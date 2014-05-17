using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Timers;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using Newtonsoft.Json;

namespace GHva3c
{
    public class va3c_geometry : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_geometry class.
        /// </summary>
        public va3c_geometry()
            : base("va3c_geometry", "va3c_geometry",
                "va3c_geometry",
                "va3c", "va3c")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Triangulated Mesh", GH_ParamAccess.item);
            pManager.AddTextParameter("Attribute Names", "[aN]", "Attribute Names", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager.AddNumberParameter("Attribute Values", "[aV]", "Attribute Values", GH_ParamAccess.list);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Geometry JSON", "G", "output to feed into scene compiler component", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //local varaibles
            GH_Mesh mesh = null;


            //catch inputs and populate local variables
            if (!DA.GetData(0, ref mesh))
            {
                return;
            }
            if (mesh == null)
            {
                return;
            }

            //create json from mesh
            string outJSON = geoJSON(mesh.Value);

            DA.SetData(0, outJSON);
            
        }


        private string geoJSON(Mesh mesh)
        {
            //create a dynamic object to populate
            dynamic jason = new ExpandoObject();


            jason.uuid = Guid.NewGuid();
            jason.type = "Geometry";
            jason.data = new ExpandoObject();

            //populate data object properties
            jason.data.vertices = new object[mesh.Vertices.Count * 3];
            jason.data.normals = new object[0];
            jason.data.uvs = new object[0];
            jason.data.faces = new object[mesh.Faces.Count * 4];
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
                jason.data.vertices[counter++] = mesh.Vertices[i].X;
                jason.data.vertices[counter++] = mesh.Vertices[i].Y;
                jason.data.vertices[counter++] = mesh.Vertices[i].Z;
                i++;
            }

            //populate faces
            counter = 0;
            i = 0;
            foreach (var f in mesh.Faces)
            {
                jason.data.faces[counter++] = 0;
                jason.data.faces[counter++] = mesh.Faces[i].A;
                jason.data.faces[counter++] = mesh.Faces[i].B;
                jason.data.faces[counter++] = mesh.Faces[i].C;
                i++;
            }
            return JsonConvert.SerializeObject(jason);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{981ed24e-945b-44b9-988e-4f17381c3072}"); }
        }
    }
}