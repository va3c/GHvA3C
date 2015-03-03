using System;
using System.Collections.Generic;
using System.Dynamic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using Newtonsoft.Json;
using GHva3c.Properties;

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
            pManager.AddColourParameter("Ambient Color", "[aC]", "Ambient color of the material, multiplied by the color of the ambient light in the scene.  Default is black", GH_ParamAccess.item, System.Drawing.Color.Black);
            pManager.AddColourParameter("Emissive Color", "[eC]", "Emissive (light) color of the material, essentially a solid color unaffected by other lighting. Default is black.", GH_ParamAccess.item, System.Drawing.Color.Black);
            pManager.AddNumberParameter("[Opacity]", "[O]", "Number in the range of 0.0 - 1.0 indicating how transparent the material is. A value of 0.0 indicates fully transparent, 1.0 is fully opaque.", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Smooth edges?", "[s]", "Smooth edges between faces?  If false, mesh will appear faceted.", GH_ParamAccess.item, true);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh Material", "Mm", "Mesh Material.  Feed this into the va3C Mesh component.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Colour inColor = null;
            GH_Colour inAmbient = null;
            GH_Colour inEmissive = null;
            GH_Number inOpacity = null;
            GH_Boolean inSmooth = null;
            String outMaterial = null;

            if (!DA.GetData(0, ref inColor)) { return; }
            if (inColor == null) { return; }
            DA.GetData(1, ref inAmbient);
            DA.GetData(2, ref inEmissive);
            DA.GetData(3, ref inOpacity);
            if (inOpacity.Value > 1 || inOpacity.Value < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The opacity input must be between 0 and 1, and has been defaulted back to 1.  Check your 'O' input.");
                inOpacity.Value = 1.0;
            }
            DA.GetData(4, ref inSmooth);

            outMaterial = ConstructLambertMaterial(inColor, inAmbient, inEmissive, inOpacity.Value, inSmooth.Value);
            
            Material material = new Material(outMaterial, mType.Mesh);

            DA.SetData(0, material);
        }

        private string ConstructLambertMaterial(GH_Colour col, GH_Colour amb, GH_Colour em,  Double opp, bool smooth)
        {
            dynamic jason = new ExpandoObject();

            jason.uuid = Guid.NewGuid();
            jason.type = "MeshLambertMaterial";
            jason.color = _Utilities.hexColor(col);
            jason.ambient = _Utilities.hexColor(amb);
            jason.emissive = _Utilities.hexColor(em);
            if (opp < 1)
            {
                jason.transparent = true;
                jason.opacity = opp;
            }
            jason.wireframe = false;
            jason.side = 2;
            if (smooth)
            {
                jason.shading = 2; 
            }
            else
            {
                jason.shading = 1;
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
                return Resources.MESH_LAMBERT;
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