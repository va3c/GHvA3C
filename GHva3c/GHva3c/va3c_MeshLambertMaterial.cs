using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHva3c
{
    public class va3c_MeshLambertMaterial : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_MeshLambertMaterial class.
        /// </summary>
        public va3c_MeshLambertMaterial()
            : base("vA3C_MeshLambertMaterial", "vA3C_MeshLambertMaterial",
                "Creates a non-shiny mesh material.",
                "vA3C", "materials")
        {
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.secondary;
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Color", "C", "Diffuse color of the material", GH_ParamAccess.item);
            pManager.AddColourParameter("Ambient Color", "[aC]", "Ambient color of the material, multiplied by the color of the ambient light in the scene.  Default is white", GH_ParamAccess.item, System.Drawing.Color.White);
            pManager.AddColourParameter("Emissive Color", "[eC]", "Emissive (light) color of the material, essentially a solid color unaffected by other lighting. Default is black.", GH_ParamAccess.item, System.Drawing.Color.Black);
            pManager.AddBooleanParameter("Smooth edges?", "[s]", "Smooth edges between faces?  If false, mesh will appear faceted.", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Mesh Material", "Mm", "Mesh Material JSON representation.  Feed this into the Scene Compiler component.", GH_ParamAccess.item);
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
            get { return new Guid("{71ca4ca9-fd21-4d13-8ee0-6a42382cec5d}"); }
        }
    }
}