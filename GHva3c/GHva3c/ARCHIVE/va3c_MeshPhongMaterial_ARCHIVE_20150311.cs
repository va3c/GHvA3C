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
    public class va3c_MeshPhongMaterial_ARCHIVE_20140311 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_MeshPhongMaterial class.
        /// </summary>
        public va3c_MeshPhongMaterial_ARCHIVE_20140311()
            : base("vA3C_MeshPhongMaterial", "vA3C_MeshPhongMaterial",
                "Create a shiny material for meshes",
                "vA3C", "materials")
        {
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.hidden;
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
            pManager.AddColourParameter("Specular Color", "[sC]", "Specular color of the material, i.e., how shiny the material is and the color of its shine. Setting this the same color as the diffuse value (times some intensity) makes the material more metallic-looking; setting this to some gray makes the material look more plastic. Default is dark gray.", GH_ParamAccess.item, System.Drawing.Color.DarkGray);
            pManager.AddNumberParameter("Shininess", "[S]", "How shiny the specular highlight is; a higher value gives a sharper highlight. Default is 30", GH_ParamAccess.item, 30.0);
            pManager.AddNumberParameter("[Opacity]", "[O]", "Number in the range of 0.0 - 1.0 indicating how transparent the material is. A value of 0.0 indicates fully transparent, 1.0 is fully opaque.", GH_ParamAccess.item, 1.0);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true; 
            pManager[5].Optional = true;
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
            //System.Drawing.Color inColor = System.Drawing.Color.White;
            GH_Colour inColor = null;
            GH_Colour inAmbient = null;
            GH_Colour inEmissive = null;
            GH_Colour inSpecular = null;
            GH_Number inShininess = null;
            GH_Number inOpacity = null;
            String outMaterial = null;

            //if (!DA.GetData(0, ref inColor)) { return; }
            if (!DA.GetData(0, ref inColor)) { return; }
            if (inColor == null) { return; }
            DA.GetData(1, ref inAmbient);
            DA.GetData(2, ref inEmissive);
            DA.GetData(3, ref inSpecular);
            DA.GetData(4, ref inShininess);
            DA.GetData(5, ref inOpacity);
            if (inOpacity.Value > 1 || inOpacity.Value < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The opacity input must be between 0 and 1, and has been defaulted back to 1.  Check your 'O' input.");
                inOpacity.Value = 1.0;
            }

            outMaterial = ConstructPhongMaterial(inColor, inAmbient, inEmissive, inSpecular, inShininess.Value, inOpacity.Value);
            //call json conversion function

            DA.SetData(0, outMaterial);
        }

        private string ConstructPhongMaterial(GH_Colour col, GH_Colour amb, GH_Colour em, GH_Colour spec, Double shin, Double opp)
        {
            dynamic jason = new ExpandoObject();

            jason.uuid = Guid.NewGuid();
            jason.type = "MeshPhongMaterial";
            jason.color = _Utilities.hexColor(col);
            jason.ambient = _Utilities.hexColor(amb);
            jason.emissive = _Utilities.hexColor(em);
            jason.specular = _Utilities.hexColor(spec);
            jason.shininess = shin;
            if (opp < 1)
            {
                jason.transparent = true;
                jason.opacity = opp;
            }
            jason.wireframe = false;
            jason.side = 2;
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
                return Resources.MESH_PHONG_01_OLD;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{9e566ce6-dc3f-4606-b895-48b90e0caf72}"); }
        }
    }
}