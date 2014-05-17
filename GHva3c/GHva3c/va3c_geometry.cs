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
            pManager.AddTextParameter("Material names", "[Mn]", "Material Nams", GH_ParamAccess.item);
            pManager.AddTextParameter("Attribute Names", "[aN]", "Attribute Names", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddNumberParameter("Attribute Values", "[aV]", "Attribute Values", GH_ParamAccess.list);
            pManager[3].Optional = true;
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
            GH_String matName = null;


            //catch inputs and populate local variables
            if (!DA.GetData(0, ref mesh))
            {
                return;
            }
            if (!DA.GetData(1, ref matName))
            {
                return;
            }
            if (mesh == null || matName == null)
            {
                return;
            }

            //create json from mesh
            string outJSON = geoJSON(mesh.Value, matName.Value);

            DA.SetData(0, outJSON);
            
        }

        private int meshCounter = 0;

        private string objectName()
        {
            return meshCounter++.ToString();
        }

        private string geoJSON(Mesh mesh, string matName)
        {
            //create a dynamic object to populate
            dynamic jason = new ExpandoObject();

            //populate object properties
            jason.geometry = new ExpandoObject();
            jason.groups = new object[0];
            jason.material = matName;
            jason.position = new object[3];
            jason.position[0] = 0; jason.position[1] = 0; jason.position[2] = 0;
            jason.rotation = new object[3];
            jason.rotation[0] = 0; jason.rotation[1] = 0; jason.rotation[2] = 0;
            jason.quaternion = new object[4];
            jason.quaternion[0] = 0; jason.quaternion[1] = 0; jason.quaternion[2] = 0; jason.quaternion[3] = 0;
            jason.scale = new object[3];
            jason.scale[0] = 1; jason.scale[1] = 1; jason.scale[2] = 1;
            jason.visible = true;
            jason.castShadow = true;
            jason.receiveShadow = false;
            jason.doubleSided = true;


            //populate geometry object
            jason.geometry.metadata = new ExpandoObject();
            jason.geometry.metadata.formatVersion = 3.1;
            jason.geometry.metadata.generatedBy = "GHva3c 0.01 Exporter";
            jason.geometry.metadata.vertices = mesh.Vertices.Count;
            jason.geometry.metadata.faces = mesh.Faces.Count;
            jason.geometry.metadata.normals = 0;
            jason.geometry.metadata.colors = 0;
            jason.geometry.metadata.uvs = 0;
            jason.geometry.metadata.materials = 0;
            jason.geometry.metadata.morphTargets = 0;
            jason.geometry.metadata.bones = 0;

            jason.geometry.scale = 1.000;
            jason.geometry.materials = new object[0];
            jason.geometry.vertices = new object[mesh.Vertices.Count*3];
            jason.geometry.morphTargets = new object[0];
            jason.geometry.normals = new object[0];
            jason.geometry.colors = new object[0];
            jason.geometry.uvs = new object[0];
            jason.geometry.faces = new object[mesh.Faces.Count*3];
            jason.geometry.bones = new object[0];
            jason.geometry.skinIndices = new object[0];
            jason.geometry.skinWeights = new object[0];
            jason.geometry.animation = new ExpandoObject();

            //populate vertices
            int counter = 0;
            int i = 0;
            foreach (var v in mesh.Vertices)
            {
                jason.geometry.vertices[counter++] = mesh.Vertices[i].X;
                jason.geometry.vertices[counter++] = mesh.Vertices[i].Y;
                jason.geometry.vertices[counter++] = mesh.Vertices[i].Z;
                i++;
            }

            //populate faces
            counter = 0;
            i = 0;
            foreach (var f in mesh.Faces)
            {
                jason.geometry.faces[counter++] = mesh.Faces[i].A;
                jason.geometry.faces[counter++] = mesh.Faces[i].B;
                jason.geometry.faces[counter++] = mesh.Faces[i].C;
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