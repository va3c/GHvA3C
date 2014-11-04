using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHva3c
{
    public class va3c_MeshColoredVertices : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_MeshColoredVertices class.
        /// </summary>
        public va3c_MeshColoredVertices()
            : base("va3c_MeshColoredVertices", "va3c_MeshColoredVertices",
                "Creates a va3c mesh and a list of materials from a grasshopper mesh with color data.",
                "va3c", "geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "A Grasshopper Mesh", GH_ParamAccess.item);
            pManager.AddTextParameter("Attribute Names", "[aN]", "Attribute Names", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager.AddTextParameter("Attribute Values", "[aV]", "Attribute Values", GH_ParamAccess.list);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Mesh JSON", "Me", "Mesh JSON output to feed into scene compiler component", GH_ParamAccess.item);
            pManager.AddTextParameter("Materials", "Mat", "Geometry Material", GH_ParamAccess.list);
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
            get { return new Guid("{0a8650b4-31ef-4d56-92f8-dcdd2f9bec8f}"); }
        }
    }
}