using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHva3c
{
    public class va3c_MeshColoredFaces : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_coloredMesh class.
        /// </summary>
        public va3c_MeshColoredFaces()
            : base("va3c_coloredMesh", "va3c_coloredMesh",
                "Creates a va3c mesh and a set of materials from a grasshopper mesh and a list of colors - one color per face.  If the colors list isn't the same length as the list of faces, we'll do standard grasshopper longest list iteration, using the mesh faces as the driving list.",
                "va3c", "geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "A Grasshopper Mesh", GH_ParamAccess.item);
            pManager.AddColourParameter("colors", "C", "A list of colors - one per face.", GH_ParamAccess.list);
            pManager.AddTextParameter("Attribute Names", "[aN]", "Attribute Names", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddTextParameter("Attribute Values", "[aV]", "Attribute Values", GH_ParamAccess.list);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Mesh JSON", "Me", "Mesh JSON output to feed into scene compiler component", GH_ParamAccess.item);
            pManager.AddTextParameter("Material", "Mat", "Geometry Material", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetData(0, "BUILD ME!");
            DA.SetData(1, "BUILD ME!");
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
            get { return new Guid("{815a9851-bf5d-44d8-87a7-0915bc58a628}"); }
        }
    }
}