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
            pManager.AddBooleanParameter("write?", "W?", "Write the va3c JSON file to disk?", GH_ParamAccess.item);
            pManager.AddTextParameter("filePath", "Fp", "Full filepath of the file you'd like to create.  Files will be overwritten automatically.", GH_ParamAccess.item);
            pManager.AddTextParameter("Meshes", "Mj", "va3c mesh JSON objects to add to the scene.", GH_ParamAccess.list);
            pManager.AddTextParameter("Mesh Materials", "Mm", "va3c mesh materials to add to the scene.  Input one material per mesh.  If list lengths do not match we'll apply the last material to all subsequent meshses.", GH_ParamAccess.list);
            pManager.AddTextParameter("Lines", "Lj", "vA3C line JSON objects to add to the scene.", GH_ParamAccess.list);
            pManager.AddTextParameter("Line Materials", "Lm", "vA3C line materials to add to the scene.  Input one material per line.  If list lengths do not match we'll apply the last material to all subsequent lines.", GH_ParamAccess.list);
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
            pManager.AddTextParameter("Message", "Out", "Message", GH_ParamAccess.item);
            pManager.AddTextParameter("Json Presentation of Scene", "J_Scene", "Json Presentation of Scene", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool write = false;
            string myFilePath = null;
            List<GH_String> inMeshGeometry = new List<GH_String>();
            List<GH_String> inMeshMaterials = new List<GH_String>();
            List<GH_String> inLineGeometry = new List<GH_String>();
            List<GH_String> inLineMaterials = new List<GH_String>();

            //get user inputs
            //user should be able to create a scene contianing only lines, or only meshes, or both.  All geo and material inputs will be optional, and we'll run some defense.
            if (!DA.GetData(0, ref write)) return;
            if (!DA.GetData(1, ref myFilePath)) return;
            DA.GetDataList(2, inMeshGeometry);
            DA.GetDataList(3, inMeshMaterials);
            DA.GetDataList(4, inLineGeometry);
            DA.GetDataList(5, inLineMaterials);

            #region Input defense

            //input some geometry
            if (inMeshGeometry.Count < 1 && inLineGeometry.Count < 1)
            {
                string err = "The scene must contain at least one mesh or one line.";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(0, err);
                return;
            }

            //make sure there are materials if there is geometry
            if (inMeshGeometry.Count > 0 && inMeshMaterials.Count < 1)
            {
                string err = "You need at least one mesh material in order to create meshes.";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(0, err);
                return;
            }
            if (inLineGeometry.Count > 0 && inLineMaterials.Count < 1)
            {
                string err = "You need at least one line material in order to create lines.";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, err);
                DA.SetData(0, err);
                return;
            }

            //if we are not told to run, return
            if (!write)
            {
                string err = "Set the 'W?' input to true to write the JSON file to disk.";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, err);
                DA.SetData(0, err);
                return;
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
                string outJSON = sceneJSON(inMeshGeometry, inMeshMaterials, inLineGeometry, inLineMaterials);
                outJSON = outJSON.Replace("OOO", "object");


                //write the file to disk
                File.WriteAllText(myFilePath, outJSON);

                //report success
                DA.SetData(0, "JSON file written successfully!");
                DA.SetData(1, outJSON);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Something went wrong while trying to write the file to disk.  Here's the error:\n\n"
                    + e.ToString());
                return;
            }
        }

        private string sceneJSON(List<GH_String> meshList, List<GH_String> meshMaterialList, List<GH_String> linesList, List<GH_String> linesMaterialList)
        {
            //defense - if the material lists are longer than the geometry lists, trim them to be the same length
            if (meshMaterialList.Count > meshList.Count)
            {
                meshMaterialList.RemoveRange(meshList.Count - 1, meshMaterialList.Count - meshList.Count);
            }
            if (linesMaterialList.Count > linesList.Count)
            {
                linesMaterialList.RemoveRange(linesList.Count - 1, linesMaterialList.Count - linesList.Count);
            }

            //create a dynamic object to populate
            dynamic jason = new ExpandoObject();

            //populate metadata object
            jason.metadata = new ExpandoObject();
            jason.metadata.version = 4.3;
            jason.metadata.type = "Object";
            jason.metadata.generator = "ObjectExporter";

            //populate mesh geometries:
            jason.geometries = new object[meshList.Count + linesList.Count];   //array for geometry - both lines and meshes
            int meshCounter = 0;
            jason.materials = new object[meshMaterialList.Count + linesMaterialList.Count];  //array for materials - both lines and meshes
            int matCounter = 0;
            Dictionary<string, object> MeshDict = new Dictionary<string, object>();
            Dictionary<string, va3cAttributesCatcher> attrDict = new Dictionary<string, va3cAttributesCatcher>();
            foreach (GH_String m in meshList)
            {
                //get the last material if the list lengths don't match
                if (matCounter == meshMaterialList.Count)
                {
                    matCounter = meshMaterialList.Count - 1;
                }

                //deserialize the geometry and attributes, and add them to our object
                va3cGeometryCatcher c = JsonConvert.DeserializeObject<va3cGeometryCatcher>(m.Value);
                va3cAttributesCatcher ac = JsonConvert.DeserializeObject<va3cAttributesCatcher>(m.Value);
                jason.geometries[meshCounter] = c;
                attrDict.Add(c.uuid, ac);

                //now that we have different types of materials, we need to know which catcher to call
                //use the va3cBaseMaterialCatcher class to determine a material's type, then call the appropriate catcher
                //object mc;
                va3cBaseMaterialCatcher baseCatcher = JsonConvert.DeserializeObject<va3cBaseMaterialCatcher>(meshMaterialList[matCounter].Value);
                if (baseCatcher.type == "MeshFaceMaterial")
                {
                    va3cMeshFaceMaterialCatcher mc = JsonConvert.DeserializeObject<va3cMeshFaceMaterialCatcher>(meshMaterialList[matCounter].Value);
                    jason.materials[matCounter] = mc;
                    MeshDict.Add(c.uuid, mc.uuid);
                }
                if (baseCatcher.type == "MeshPhongMaterial")
                {
                    va3cMeshPhongMaterialCatcher mc = JsonConvert.DeserializeObject<va3cMeshPhongMaterialCatcher>(meshMaterialList[matCounter].Value);
                    jason.materials[matCounter] = mc;
                    MeshDict.Add(c.uuid, mc.uuid);
                }
                if (baseCatcher.type == "MeshLambertMaterial")
                {
                    va3cMeshLambertMaterialCatcher mc = JsonConvert.DeserializeObject<va3cMeshLambertMaterialCatcher>(meshMaterialList[matCounter].Value);
                    jason.materials[matCounter] = mc;
                    MeshDict.Add(c.uuid, mc.uuid);
                }
                if (baseCatcher.type == "MeshBasicMaterial")
                {
                    va3cMeshBasicMaterialCatcher mc = JsonConvert.DeserializeObject<va3cMeshBasicMaterialCatcher>(meshMaterialList[matCounter].Value);
                    jason.materials[matCounter] = mc;
                    MeshDict.Add(c.uuid, mc.uuid);
                }
                matCounter++;
                meshCounter++;


            }

            //populate line geometries
            int lineCounter = meshCounter;
            int lineMaterialCounter = 0;
            Dictionary<string, object> LineDict = new Dictionary<string, object>();
            foreach (GH_String l in linesList)
            {
                //get the last material if the list lengths don't match
                if (lineMaterialCounter == linesMaterialList.Count)
                {
                    lineMaterialCounter = linesMaterialList.Count - 1;
                }

                //deserialize the line and the material
                va3cLineCatcher lc = JsonConvert.DeserializeObject<va3cLineCatcher>(l.Value);
                va3cLineBasicMaterialCatcher lmc = 
                    JsonConvert.DeserializeObject<va3cLineBasicMaterialCatcher>(linesMaterialList[lineMaterialCounter].Value);
                //add the deserialized values to the jason object
                jason.geometries[lineCounter] = lc;
                jason.materials[matCounter + lineMaterialCounter] = lmc;

                //populate dict to match up materials and lines
                LineDict.Add(lc.uuid, lmc.uuid);

                //increment counters
                lineCounter++;
                lineMaterialCounter++;
            }


            jason.OOO = new ExpandoObject();
            //create scene:
            jason.OOO.uuid = System.Guid.NewGuid();
            jason.OOO.type = "Scene";
            int[] numbers = new int[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            jason.OOO.matrix = numbers;
            jason.OOO.children = new object[meshList.Count + linesList.Count];

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
                jason.OOO.children[i].userData = attrDict[g].userData;
                i++;
            }
            foreach (var l in LineDict.Keys)
            {
                jason.OOO.children[i] = new ExpandoObject();
                jason.OOO.children[i].uuid = Guid.NewGuid();
                jason.OOO.children[i].name = "line " + i.ToString();
                jason.OOO.children[i].type = "Line";
                jason.OOO.children[i].geometry = l;
                jason.OOO.children[i].material = LineDict[l];
                jason.OOO.children[i].matrix = numbers;
                i++;
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
            get { return new Guid("{065f9b89-44fd-4660-aba8-f4211e5e2ef9}"); }
        }
    }
}