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
using System.IO;
using System.Text.RegularExpressions;
using GHva3c.Properties;


namespace GHva3c
{
    public class va3c_SceneCompiler : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the va3c_SceneCompiler class.
        /// </summary>
        public va3c_SceneCompiler()
            : base("vA3C_SceneCompiler", "vA3C_SceneCompiler",
                "Compiles vA3C objects into a JSON representation of a THREE.js scene, which can be opened using the vA3C viewer.",
                "vA3C", "vA3C")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("write?", "W?", "Write the VA3C JSON file to disk?", GH_ParamAccess.item);
            pManager.AddTextParameter("filePath", "Fp", "Full filepath of the file you'd like to create.  Files will be overwritten automatically.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Elements", "E", "va3c Elements to add to the scene.", GH_ParamAccess.list);
            pManager[2].DataMapping = GH_DataMapping.Flatten;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Message", "Out", "Message", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool write = false;
            string myFilePath = null;
            List<Element> inElements = new List<Element>();



            //get user inputs
            //user should be able to create a scene contianing only lines, or only meshes, or both.  All geo and material inputs will be optional, and we'll run some defense.
            if (!DA.GetData(0, ref write)) return;
            if (!DA.GetData(1, ref myFilePath)) return;
            DA.GetDataList(2, inElements);


            #region Input management

            //if we are not told to run, return
            if (!write)
            {
                string err = "Set the 'W?' input to true to write the JSON file to disk";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, err);
                DA.SetData(0, err);
                return;
            }


            List<GH_String> inMeshGeometry = new List<GH_String>();
            List<GH_String> inLineGeometry = new List<GH_String>();
            List<GH_String> inViews = new List<GH_String>();

            List<GH_String> inMeshMaterial = new List<GH_String>();
            List<GH_String> inLineMaterial = new List<GH_String>();

            List<GH_String> inMeshLayer = new List<GH_String>();
            List<GH_String> inLineLayer = new List<GH_String>();

            Dictionary<string, List<Element>> definitionLayers = new Dictionary<string, List<Element>>();

            foreach (Element e in inElements)
            {
                GH_String g = new GH_String();
                g.Value = e.GeometryJson;

                if (e.Type != va3cElementType.Camera)
                {
                    GH_String m = new GH_String();
                    m.Value = e.Material.MaterialJson;

                    string layerName = "";
                    if (e.Layer == null) layerName = "Default";
                    else layerName = e.Layer.Name;

                    GH_String l = new GH_String();
                    l.Value = layerName;
                    

                    if (e.Type == va3cElementType.Mesh)
                    {
                        inMeshGeometry.Add(g);
                        inMeshMaterial.Add(m);
                        inMeshLayer.Add(l);
                    }

                    if (e.Type == va3cElementType.Line)
                    {
                        inLineGeometry.Add(g);
                        inLineMaterial.Add(m);
                        inLineLayer.Add(l);
                    }
                    
                    

                    if (!definitionLayers.Keys.Contains(layerName))
                    {
                        List<Element> layerElements = new List<Element>();
                        definitionLayers.Add(layerName, layerElements);
                    }

                    definitionLayers[layerName].Add(e);


                }
                else
                {
                    inViews.Add(g);
                }
            }

            #endregion


            #region file path defense

            //check to see if the file path has any invalid characters
            try
            {
                //FIRST check to see if there is more than one semicolon in the path
                //or if there is a semiColon anywhere in there
                string[] colonFrags = myFilePath.Split(':');
                if (colonFrags.Length > 2 || myFilePath.Contains(";"))
                {
                    throw new Exception();
                }

                //SECOND test the file name for invalid characters using regular expressions
                //this method comes from the C# 4.0 in a nutshell book, p991
                string inputFileName = Path.GetFileName(myFilePath);
                char[] inValidChars = Path.GetInvalidFileNameChars();
                string inValidString = Regex.Escape(new string(inValidChars));
                string myNewValidFileName = Regex.Replace(inputFileName, "[" + inValidString + "]", "");

                //if the replace worked, throw an error at the user.
                if (inputFileName != myNewValidFileName)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                //warn the user
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Your file name is invalid - check your input and try again.");
                return;
            }


            //if neither the file or directory exist, throw a warning
            if (!File.Exists(myFilePath) && !Directory.Exists(Path.GetDirectoryName(myFilePath)))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "The directory you specified does not exist. Please double check your input. No file path will be set.");
                return;
            }

            //if the directory exists but the file type is not .xlsx, throw a warning and set pathString = noFIle
            if (Directory.Exists(Path.GetDirectoryName(myFilePath)) && !isJSONfile(Path.GetExtension(myFilePath)))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Please provide a file of type .js or .json.  Something like: 'myExampleFile.json'.");
                return;
            }
            #endregion



            //compile geometry + materials into one JSON object with metadata etc.
            //https://raw.githubusercontent.com/mrdoob/three.js/master/examples/obj/blenderscene/scene.js

            try
            {
                //create json from lists of json:
                string outJSON = sceneJSON(inMeshGeometry, inMeshMaterial, inMeshLayer, inLineGeometry, inLineMaterial,inLineLayer, inViews, definitionLayers);
                outJSON = outJSON.Replace("OOO", "object");

                //write the file to disk
                File.WriteAllText(myFilePath, outJSON);

                //report success
                DA.SetData(0, "JSON file written successfully!");
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Something went wrong while trying to write the file to disk.  Here's the error:\n\n"
                    + e.ToString());
                return;
            }
        }

        private string sceneJSON(List<GH_String> meshList, List<GH_String> meshMaterialList, List<GH_String> meshLayerList, List<GH_String> linesList, List<GH_String> linesMaterialList, List<GH_String> lineLayerList, List<GH_String> viewList, Dictionary<string, List<Element>> defLayers)
        {

            //create a dynamic object to populate
            dynamic jason = new ExpandoObject();

            //populate metadata object
            jason.metadata = new ExpandoObject();
            jason.metadata.version = 4.3;
            jason.metadata.type = "Object";
            jason.metadata.generator = "vA3C_Grasshopper_Exporter";

            int size = meshList.Count + linesList.Count;

            //populate mesh geometries:
            jason.geometries = new object[size];   //array for geometry - both lines and meshes
            jason.materials = new object[size];  //array for materials - both lines and meshes


            #region Mesh management
            int meshCounter = 0;
            Dictionary<string, object> MeshDict = new Dictionary<string, object>();
            Dictionary<string, va3cAttributesCatcher> attrDict = new Dictionary<string, va3cAttributesCatcher>();


            foreach (GH_String m in meshList)
            {
                //deserialize the geometry and attributes, and add them to our object
                va3cGeometryCatcher c = JsonConvert.DeserializeObject<va3cGeometryCatcher>(m.Value);
                va3cAttributesCatcher ac = JsonConvert.DeserializeObject<va3cAttributesCatcher>(m.Value);
                jason.geometries[meshCounter] = c;
                attrDict.Add(c.uuid, ac);

                //now that we have different types of materials, we need to know which catcher to call
                //use the va3cBaseMaterialCatcher class to determine a material's type, then call the appropriate catcher
                //object mc;
                va3cBaseMaterialCatcher baseCatcher = JsonConvert.DeserializeObject<va3cBaseMaterialCatcher>(meshMaterialList[meshCounter].Value);
                if (baseCatcher.type == "MeshFaceMaterial")
                {
                    va3cMeshFaceMaterialCatcher mc = JsonConvert.DeserializeObject<va3cMeshFaceMaterialCatcher>(meshMaterialList[meshCounter].Value);
                    jason.materials[meshCounter] = mc;
                    MeshDict.Add(c.uuid, mc.uuid);
                }
                if (baseCatcher.type == "MeshPhongMaterial")
                {
                    va3cMeshPhongMaterialCatcher mc = JsonConvert.DeserializeObject<va3cMeshPhongMaterialCatcher>(meshMaterialList[meshCounter].Value);
                    jason.materials[meshCounter] = mc;
                    MeshDict.Add(c.uuid, mc.uuid);
                }
                if (baseCatcher.type == "MeshLambertMaterial")
                {
                    va3cMeshLambertMaterialCatcher mc = JsonConvert.DeserializeObject<va3cMeshLambertMaterialCatcher>(meshMaterialList[meshCounter].Value);
                    jason.materials[meshCounter] = mc;
                    MeshDict.Add(c.uuid, mc.uuid);
                }
                if (baseCatcher.type == "MeshBasicMaterial")
                {
                    va3cMeshBasicMaterialCatcher mc = JsonConvert.DeserializeObject<va3cMeshBasicMaterialCatcher>(meshMaterialList[meshCounter].Value);
                    jason.materials[meshCounter] = mc;
                    MeshDict.Add(c.uuid, mc.uuid);
                }
                meshCounter++;

            }
            #endregion

            #region Line management
            //populate line geometries
            int lineCounter = meshCounter;
            int lineMaterialCounter = 0;
            Dictionary<string, object> LineDict = new Dictionary<string, object>();
            foreach (GH_String l in linesList)
            {
                //deserialize the line and the material
                va3cLineCatcher lc = JsonConvert.DeserializeObject<va3cLineCatcher>(l.Value);
                va3cLineBasicMaterialCatcher lmc =
                    JsonConvert.DeserializeObject<va3cLineBasicMaterialCatcher>(linesMaterialList[lineMaterialCounter].Value);
                //add the deserialized values to the jason object
                jason.geometries[lineCounter] = lc;
                jason.materials[meshCounter + lineMaterialCounter] = lmc;

                //populate dict to match up materials and lines
                LineDict.Add(lc.uuid, lmc.uuid);

                //increment counters
                lineCounter++;
                lineMaterialCounter++;
            }
            #endregion


            #region Camera management
            //populate line geometries
            int viewCounter = 0;

            Dictionary<string, List<object>> viewDict = new Dictionary<string, List<object>>();
            foreach (GH_String l in viewList)
            {
                //deserialize the line and the material
                va3cCameraCatcher lc = JsonConvert.DeserializeObject<va3cCameraCatcher>(l.Value);

                List<object> viewSettings = new List<object>();
                viewSettings.Add(lc.eye);
                viewSettings.Add(lc.target);

                viewDict.Add(lc.name, viewSettings);

                //increment counters
                viewCounter++;

            }
            #endregion

            jason.OOO = new ExpandoObject();
            //create scene:
            jason.OOO.uuid = System.Guid.NewGuid();
            jason.OOO.type = "Scene";
            int[] numbers = new int[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            jason.OOO.matrix = numbers;
            jason.OOO.children = new object[meshList.Count + linesList.Count];
            jason.OOO.userData = new ExpandoObject();

            

            //create childern
            //loop over meshes and lines
            int i = 0;
            foreach (var g in MeshDict.Keys) //meshes
            {
                jason.OOO.children[i] = new ExpandoObject();
                jason.OOO.children[i].uuid = Guid.NewGuid();
                jason.OOO.children[i].name = "mesh" + i.ToString();
                jason.OOO.children[i].type = "Mesh";
                jason.OOO.children[i].geometry = g;
                jason.OOO.children[i].material = MeshDict[g];
                jason.OOO.children[i].matrix = numbers;
                //jason.OOO.children[i].userData = attrDict[g].userData;

                jason.OOO.children[i].userData = new ExpandoObject();
                jason.OOO.children[i].userData.attributes = new object[1];
                jason.OOO.children[i].userData.attributes[0]=attrDict[g].userData;
                jason.OOO.children[i].userData.layer = new object[1];
                jason.OOO.children[i].userData.layer[0] = new ExpandoObject();
                jason.OOO.children[i].userData.layer[0].name = meshLayerList[i].Value;
                i++;
            }
            int lineCount = 0;
            foreach (var l in LineDict.Keys)
            {
                jason.OOO.children[i] = new ExpandoObject();
                jason.OOO.children[i].uuid = Guid.NewGuid();
                jason.OOO.children[i].name = "line " + i.ToString();
                jason.OOO.children[i].type = "Line";
                jason.OOO.children[i].geometry = l;
                jason.OOO.children[i].material = LineDict[l];
                jason.OOO.children[i].matrix = numbers;
                jason.OOO.children[i].userData = new ExpandoObject();
                jason.OOO.children[i].userData.layer = new object[1];
                jason.OOO.children[i].userData.layer[0] = new ExpandoObject();
                jason.OOO.children[i].userData.layer[0].name = lineLayerList[lineCount].Value;
                i++;
                lineCount++;
            }

            jason.OOO.userData.views = new object[viewList.Count];
            int j = 0;
            foreach (var n in viewDict.Keys)
            {
                jason.OOO.userData.views[j] = new ExpandoObject();
                jason.OOO.userData.views[j].name = n;
                jason.OOO.userData.views[j].eye = viewDict[n][0];
                jason.OOO.userData.views[j].target = viewDict[n][1];

                j++;
            }

            jason.OOO.userData.layers = new object[defLayers.Keys.Count];
            int li = 0;
            foreach (var n in defLayers.Keys)
            {
                jason.OOO.userData.layers[li] = new ExpandoObject();
                jason.OOO.userData.layers[li].name = n;
                //jason.OOO.userData.layers[li].elements = defLayers[n];
                //maybe add here children and parent layers if any...

                li++;
            }


            return JsonConvert.SerializeObject(jason);
        }

        private bool isJSONfile(string fileExtension)
        {
            if (fileExtension.ToLower() == ".js" ||
                fileExtension.ToLower() == ".json")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Resources._3;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{9e0fb676-f7a2-41b2-bf39-1dbdb9e8478d}"); }
        }
    }
}