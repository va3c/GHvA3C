using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

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
            pManager.AddMeshParameter("Meshes", "M", "Triangulated Meshes", GH_ParamAccess.item);
            pManager.AddTextParameter("Materials", "[Mat]", "Materials", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddTextParameter("Attribute Names", "aN", "Attribute Names", GH_ParamAccess.list);
            pManager.AddNumberParameter("Attribute Values", "aV", "Attribute Values", GH_ParamAccess.list);
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