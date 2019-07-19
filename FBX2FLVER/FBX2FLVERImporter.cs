extern alias PIPE;
using Microsoft.Xna.Framework;
using PIPE::Microsoft.Xna.Framework.Content.Pipeline;
using PIPE::Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FbxPipeline = PIPE::Microsoft.Xna.Framework;

namespace FBX2FLVER
{


    public class FBX2FLVERImporter
    {

        public List<string> OutputtedFiles = new List<string>();

        public FBX2FLVERImportJobConfig JOBCONFIG = new FBX2FLVERImportJobConfig();

        const float FBX_IMPORT_SCALE_BASE = 1.0f / 100.0f;
        public float FinalScaleMultiplier => (float)(FBX_IMPORT_SCALE_BASE * (JOBCONFIG.ScalePercent / 100.0));

        public readonly Solvers.BoneSolver BoneSolver;
        public readonly Solvers.NormalSolver NormalSolver;
        public readonly Solvers.OrientationSolver OrientationSolver;
        public readonly Solvers.TangentSolver TangentSolver;
        public readonly Solvers.BoundingBoxSolver BoundingBoxSolver;

        public FBX2FLVERImporter()
        {
            BoneSolver = new Solvers.BoneSolver(this);
            NormalSolver = new Solvers.NormalSolver(this);
            OrientationSolver = new Solvers.OrientationSolver(this);
            TangentSolver = new Solvers.TangentSolver(this);
            BoundingBoxSolver = new Solvers.BoundingBoxSolver(this);

            CheckResourceLoad();
        }

        internal static byte[] LoadEmbRes(string relResName)
        {
            byte[] result = null;
            using (var embResStream = typeof(FBX2FLVERImporter).Assembly
                .GetManifestResourceStream($"FBX2FLVER.EmbeddedResources.{relResName}"))
            using (var embResReader = new BinaryReader(embResStream))
                result = embResReader.ReadBytes((int)embResStream.Length);
            return result;
        }

        private static void CheckResourceLoad()
        {
            lock (_RESOURCE_LOAD_LOCKER)
            {
                if (FBX2FLVER_PLACEHOLDER_DIFFUSE == null)
                    FBX2FLVER_PLACEHOLDER_DIFFUSE =
                        LoadEmbRes("FBX2FLVER_PLACEHOLDER_DIFFUSE.dds");
                if (FBX2FLVER_PLACEHOLDER_SPECULAR == null)
                    FBX2FLVER_PLACEHOLDER_SPECULAR =
                        LoadEmbRes("FBX2FLVER_PLACEHOLDER_SPECULAR.dds");
                if (FBX2FLVER_PLACEHOLDER_BUMPMAP == null)
                    FBX2FLVER_PLACEHOLDER_BUMPMAP =
                        LoadEmbRes("FBX2FLVER_PLACEHOLDER_BUMPMAP.dds");
            }
        }


        public event EventHandler<FBX2FLVERGenericEventArgs<string>> InfoTextOutputted;
        public event EventHandler<FBX2FLVERGenericEventArgs<string>> WarningTextOutputted;
        public event EventHandler<FBX2FLVERGenericEventArgs<string>> ErrorTextOutputted;

        public event EventHandler ImportStarted;
        public event EventHandler ImportEnding;

        public event EventHandler<FBX2FLVERGenericEventArgs<NodeContent>> FbxLoaded;
        //public event EventHandler<FBX2FLVERGenericEventArgs<FLVER>> FlverGenerated;

        internal void Print(string text)
        {
            OnInfoTextOutputted(text);
        }

        internal void PrintWarning(string text)
        {
            OnWarningTextOutputted(text);
        }

        internal void PrintError(string text)
        {
            OnErrorTextOutputted(text);
        }

        protected virtual void OnInfoTextOutputted(string text)
        {
            var handler = InfoTextOutputted;
            handler?.Invoke(this, new FBX2FLVERGenericEventArgs<string>(text));
        }

        protected virtual void OnWarningTextOutputted(string text)
        {
            var handler = WarningTextOutputted;
            handler?.Invoke(this, new FBX2FLVERGenericEventArgs<string>(text));
        }

        protected virtual void OnErrorTextOutputted(string text)
        {
            var handler = ErrorTextOutputted;
            handler?.Invoke(this, new FBX2FLVERGenericEventArgs<string>(text));
        }

        protected virtual void OnImportStarted()
        {
            var handler = ImportStarted;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnImportEnding()
        {
            var handler = ImportEnding;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnFbxLoaded(NodeContent fbx)
        {
            var handler = FbxLoaded;
            handler?.Invoke(this, new FBX2FLVERGenericEventArgs<NodeContent>(fbx));
        }

        //protected virtual void OnFlverGenerated(FLVER flver)
        //{
        //    var handler = FlverGenerated;
        //    handler?.Invoke(this, new FBX2FLVERGenericEventArgs<FLVER>(flver));
        //}


        static object _RESOURCE_LOAD_LOCKER = new object();
        
        static byte[] FBX2FLVER_PLACEHOLDER_DIFFUSE;
        static byte[] FBX2FLVER_PLACEHOLDER_SPECULAR;
        static byte[] FBX2FLVER_PLACEHOLDER_BUMPMAP;
        const int FACESET_MAX_TRIANGLES = 65535;
        static readonly char[] CHAR_NUMS = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };


        //static readonly Matrix FBX_IMPORT_MATRIX = Matrix.CreateScale(FBX_IMPORT_SCALE);

        //static void AddAllBoneChildrenAsHitboxes(FLVER flver, NodeContent boneContent, int parentBoneIndex)
        //{
        //    foreach (var dummyNode in boneContent.Children)
        //    {
        //        if (dummyNode is NodeContent dummyBone)
        //        {
        //            var dmy = new FlverDummy(flver);

        //            var dummyNumber = int.Parse(Util.GetAngleBracketContents(dummyBone.Name));

        //            dmy.Position = /*Vector3.Transform(*/new Vector3(dummyBone.AbsoluteTransform.Translation.X,
        //                dummyBone.AbsoluteTransform.Translation.Y,
        //                dummyBone.AbsoluteTransform.Translation.Z) * FBX_IMPORT_SCALE/*, Matrix.CreateRotationX(-MathHelper.PiOver2))*/;

        //            dmy.Row2 = new Vector3(0, -0.180182f, 0);

        //            dmy.Row3 = new Vector3(0, 0, -0.077194f);

        //            dmy.ParentBoneIndex = (short)parentBoneIndex;

        //            dmy.TypeID = (short)dummyNumber;

        //            flver.Dummies.Add(dmy);
        //        }
        //    }
        //}

        //static void HandleDummyBone(FLVER flver, NodeContent boneContent, FlverBone flverBone)
        //{
        //    flverBone.Name = "dymmy";

        //    flverBone.Scale = Vector3.One;
        //    flverBone.Translation = Vector3.Zero;
        //    flverBone.EulerRadian = Vector3.Zero;

        //    int myIndex = flver.Bones.IndexOf(flverBone);

        //    AddAllBoneChildrenAsHitboxes(flver, boneContent, myIndex);
        //}

        //static void HandleSfxBone(FLVER flver, NodeContent boneContent, FlverBone flverBone)
        //{
        //    flverBone.Name = "SFX用";

        //    flverBone.Scale = Vector3.One;
        //    flverBone.Translation = Vector3.Zero;
        //    flverBone.EulerRadian = Vector3.Zero;

        //    int myIndex = flver.Bones.IndexOf(flverBone);

        //    AddAllBoneChildrenAsHitboxes(flver, boneContent, myIndex);
        //}

        private static readonly char[] _dirSep = new char[] { '\\', '/' };

        public static string GetFileNameWithoutDirectoryOrExtension(string fileName)
        {
            if (fileName.EndsWith("\\") || fileName.EndsWith("/"))
                fileName = fileName.TrimEnd(_dirSep);

            if (fileName.Contains("\\") || fileName.Contains("/"))
                fileName = fileName.Substring(fileName.LastIndexOfAny(_dirSep) + 1);

            if (fileName.Contains("."))
                fileName = fileName.Substring(0, fileName.LastIndexOf('.'));

            return fileName;
        }

        public bool Import()
        {
            var flver = new SoulsFormats.FLVER();
            flver.Header.Version = JOBCONFIG.FlverVersion;

            var tpf = new SoulsFormats.TPF();
            tpf.Encoding = JOBCONFIG.TpfEncoding;
            tpf.Flag2 = JOBCONFIG.TpfFlag2;

            bool result = ImportToFlver(flver, tpf);

            if (result)
            {
                if (JOBCONFIG.BeforeSaveAction != null)
                {
                    JOBCONFIG.BeforeSaveAction.Invoke(flver, tpf);
                }

                SFHelper.WriteFile(flver, JOBCONFIG.OutputFlverPath);
                SFHelper.WriteFile(tpf, JOBCONFIG.OutputTpfPath);
            }

            return result;
        }

        

        private bool ImportToFlver(SoulsFormats.FLVER flver, SoulsFormats.TPF tpf)
        {
            OnImportStarted();

            Print("Importing...");

            var flverMeshNameMap = new Dictionary<SoulsFormats.FLVER.Mesh, string>();

            var fbxImporter = new FbxImporter();
            var context = new FBX2FLVERContentImporterContext();
            var fbx = fbxImporter.Import(JOBCONFIG.FBXPath, context);

            JOBCONFIG.LoadMTDBND(JOBCONFIG.MTDBNDPath, !SoulsFormats.BND3.Is(JOBCONFIG.MTDBNDPath));
            JOBCONFIG.ChooseGamePreset(JOBCONFIG.Preset);

            if (!LoadFbxIntoFlver(fbx, flver, flverMeshNameMap, tpf))
            {
                PrintError("Import failed.");
                return false;
            }
            else
            {
                Print("Import complete.");
            }

            OnImportEnding();

            return true;
        }

        bool LoadFbxIntoFlver(NodeContent fbx, SoulsFormats.FLVER flver, Dictionary<SoulsFormats.FLVER.Mesh, string> flverSubmeshNameMap, SoulsFormats.TPF tpf)
        {
            var FBX_Bones = new List<NodeContent>();
            var FBX_RootBones = new List<NodeContent>();
            var FBX_Meshes = new Dictionary<SoulsFormats.FLVER.Mesh, MeshContent>();

            //var FLVER_VertexStructLayoutChecks = new List<FlverVertexStructLayoutCheck>();

            bool doesFbxHaveAnyBones = false;

            foreach (var fbxComponent in fbx.Children)
            {
                if (fbxComponent is MeshContent meshContent)
                {
                    FBX_Meshes.Add(new SoulsFormats.FLVER.Mesh(), meshContent);
                }
                else if (fbxComponent is NodeContent boneContent)
                {
                    if (boneContent.Name.Trim().ToUpper() == "SKELETON")
                    {
                        foreach (var childBone in boneContent.Children)
                        {
                            if (childBone.Name.ToUpper().Trim() == "ROOT")
                                FBX_RootBones.Add(childBone);
                            else
                                FBX_Bones.Add(childBone);
                        }
                    }
                    else
                    {
                        if (boneContent.Name.ToUpper().Trim() == "ROOT")
                            FBX_RootBones.Add(boneContent);
                        else
                            FBX_Bones.Add(boneContent);
                    }

                    doesFbxHaveAnyBones = true;
                }

            }

            if (!doesFbxHaveAnyBones)
                PrintWarning("FBX file contains no skeleton data.");

            if (fbx is MeshContent topLevelMeshContent)
            {
                FBX_Meshes.Add(new SoulsFormats.FLVER.Mesh(), topLevelMeshContent);
            }

            var topLevelBoneIndices = new List<int>();

            var flverRootBoneNameMap = new Dictionary<SoulsFormats.FLVER.Bone, string>();

            var dummyFollowBoneMap = new Dictionary<(string nodeName, SoulsFormats.FLVER.Dummy dmy), string>();

            foreach (var boneContent in FBX_RootBones)
            {
                var nextRootBoneIndex = BoneSolver.SolveBone(flver, fbx, boneContent, -1, dummyFollowBoneMap);
                if (nextRootBoneIndex >= 0 && nextRootBoneIndex < flver.Bones.Count)
                {
                    topLevelBoneIndices.Add(nextRootBoneIndex);
                }
                    
                //flver.Bones[nextRootBoneIndex].Name = shortModelName;
                flverRootBoneNameMap.Add(flver.Bones[nextRootBoneIndex], boneContent.Name);
            }

            foreach (var boneContent in FBX_Bones)
            {
                int nextBoneIndex = BoneSolver.SolveBone(flver, fbx, boneContent, -1, dummyFollowBoneMap);
                if (nextBoneIndex >= 0 && nextBoneIndex < flver.Bones.Count)
                {
                    topLevelBoneIndices.Add(nextBoneIndex);
                }
            }

            for (int i = 0; i < topLevelBoneIndices.Count; i++)
            {
                if (i > 0)
                {
                    flver.Bones[topLevelBoneIndices[i]].PreviousSiblingIndex = (short)topLevelBoneIndices[i - 1];
                }
                else
                {
                    flver.Bones[topLevelBoneIndices[i]].PreviousSiblingIndex = (short)-1;
                }

                if (i < topLevelBoneIndices.Count - 1)
                {
                    flver.Bones[topLevelBoneIndices[i]].NextSiblingIndex = (short)topLevelBoneIndices[i + 1];
                }
                else
                {
                    flver.Bones[topLevelBoneIndices[i]].NextSiblingIndex = (short)-1;
                }
            }

            //if (flver.Bones.Count == 0)
            //{
            //    flver.Bones.Add(new FlverBone(flver)
            //    {
            //        Name = shortModelName,
            //    });
            //}

            var bonesByName = new Dictionary<string, SoulsFormats.FLVER.Bone>();

            //if (ImportSkeletonPath != null)
            //{
            //    EntityBND skeletonSourceEntityBnd = null;

            //    if (ImportSkeletonPath.ToUpper().EndsWith(".DCX"))
            //    {
            //        skeletonSourceEntityBnd = DataFile.LoadFromDcxFile<EntityBND>(ImportSkeletonPath);
            //    }
            //    else
            //    {
            //        skeletonSourceEntityBnd = DataFile.LoadFromFile<EntityBND>(ImportSkeletonPath);
            //    }

            //    flver.Bones = skeletonSourceEntityBnd.Models[0].Mesh.Bones;
            //    flver.Dummies = skeletonSourceEntityBnd.Models[0].Mesh.Dummies;
            //}

            if (!string.IsNullOrEmpty(JOBCONFIG.ImportSkeletonFromFLVER))
            {
                var skeleFlver = (SoulsFormats.FLVER)(SFHelper.ReadFile<SoulsFormats.FLVER>(null, JOBCONFIG.ImportSkeletonFromFLVER).File);

                flver.Bones = skeleFlver.Bones;
                flver.Dummies = skeleFlver.Dummies;
            }

            for (int i = 0; i < flver.Bones.Count; i++)
            {
                //if (!flver.Bones[i].IsNub)
                //{
                //    flver.Bones[i].BoundingBoxMin = new Vector3(-1, -0.25f, -0.25f) * 0.0015f;
                //    flver.Bones[i].BoundingBoxMax = new Vector3(1, 0.25f, 0.25f) * 0.0015f;
                //}

                if (flverRootBoneNameMap.ContainsKey(flver.Bones[i]))
                {
                    bonesByName.Add(flverRootBoneNameMap[flver.Bones[i]], flver.Bones[i]);
                }
                else
                {
                    string boneName = flver.Bones[i].Name;

                    if (flver.Bones[i].Unk3C == 1)
                        boneName = $"[{boneName}]";

                    if (!bonesByName.ContainsKey(boneName))
                        bonesByName.Add(boneName, flver.Bones[i]);
                }

                
            }

            if (FBX_Meshes.Count == 0)
                PrintWarning("FBX file contains no mesh data.");

            foreach (var kvp in FBX_Meshes)
            {
                var flverMesh = kvp.Key;
                var fbxMesh = kvp.Value;

                var bonesReferencedByThisMesh = new List<SoulsFormats.FLVER.Bone>();

                var submeshHighQualityNormals = new List<Vector3>();
                var submeshHighQualityTangents = new List<Vector4>();
                var submeshVertexHighQualityBasePositions = new List<Vector3>();
                var submeshVertexHighQualityBaseUVs = new List<Vector2>();

                foreach (var geometryNode in fbxMesh.Geometry)
                {

                    if (geometryNode is GeometryContent geometryContent)
                    {
                        int numTriangles = geometryContent.Indices.Count / 3;

                        int numFacesets = numTriangles / FACESET_MAX_TRIANGLES;

                        /*
                            FACE SET ADDING/SPLITTING
                        */
                        {
                            var faceSet = new SoulsFormats.FLVER.FaceSet();

                            faceSet.CullBackfaces = !JOBCONFIG.IsDoubleSided;

                            //for (int i = geometryContent.Indices.Count - 1; i >= 0; i--)
                            for (int i = 0; i < geometryContent.Indices.Count; i += 3)
                            {
                                if (faceSet.Indices.Count >= FACESET_MAX_TRIANGLES * 3)
                                {
                                    flverMesh.FaceSets.Add(faceSet);
                                    faceSet = new SoulsFormats.FLVER.FaceSet();
                                }
                                else
                                {
                                    faceSet.Indices.Add((ushort)geometryContent.Indices[i + 2]);
                                    faceSet.Indices.Add((ushort)geometryContent.Indices[i + 1]);
                                    faceSet.Indices.Add((ushort)geometryContent.Indices[i + 0]);
                                }
                            }

                            if (faceSet.Indices.Count > 0)
                            {
                                flverMesh.FaceSets.Add(faceSet);
                            }

                        }

                        if (flverMesh.FaceSets.Count > 1)
                        {
                            PrintWarning($"Mesh '{fbxMesh.Name}' has {flverMesh.Vertices.Count} " +
                                $"vertices. \nEach individual triangle list can only support up to " +
                                $"65535 vertices due to file format limitations. Because of this, " +
                                $"the mesh had to be automatically be split into multiple " +
                                $"triangle lists. \n\nThis can be problematic for weapons in " +
                                $"particular because the game ignores ALL additional triangle " +
                                $"lists after the first one, on weapons specifically; " +
                                $"Only the triangles connected with the " +
                                $"first 65535 vertices will be shown ingame. \nIf you are " +
                                $"experiencing this issue, split the mesh into multiple " +
                                $"separate mesh objects, each with no more than 65535 vertices.");
                        }


                        //var materialOverrides = fbxMesh.Children.Where(x => x.Name.StartsWith("MaterialOverride"));

                        string matName = null;
                        string mtdName = null;

                        Dictionary<string, string> matTextures = new Dictionary<string, string>();

                        //if (materialOverrides.Any())
                        //{
                        //    if (materialOverrides.Count() > 1)
                        //    {
                        //        PrintWarning($"Mesh '{fbxMesh.Name}' has 2 " +
                        //            $"material override nodes parented to it. Using the first one " +
                        //            $"as the mesh's material and ignoring the material stored in " +
                        //            $"the FBX's geometry data as well as any other material " +
                        //            $"override nodes parented to this mesh.");
                        //    }
                        //    else
                        //    {
                        //        Print($"Material override node was found parented to mesh '{fbxMesh.Name}'. " +
                        //        "Using this override node as the mesh's material and ignoring the material stored in the FBX's geometry data.");
                        //    }

                        //    var matOverride = materialOverrides.First();

                        //    string matName = Util.GetAngleBracketContents(matOverride.Name);
                        //    if (!string.IsNullOrWhiteSpace(matName))
                        //    {
                        //        mtdName = matName;
                        //    }

                            
                        //}

                        if (geometryContent.Material != null)
                        {
                            string fbxMaterialName = geometryContent.Material.Name;

                            if (!fbxMaterialName.Contains("|"))
                            {
                                PrintWarning("FBX material name " +
                                    $"for mesh '{fbxMesh.Name}' is using the old format. Material name will be \"{JOBCONFIG.PlaceholderMaterialName}\"." +
                                    "To specify your own material name, use this FBX material naming format: " +
                                    "\"MaterialName | ShaderName\" e.g. " +
                                    "\"Shiny Metal Chestpiece | P_Metal[DSB]\"");

                                string desiredShaderName = fbxMaterialName.Trim();
                                if (desiredShaderName.Contains("#"))
                                {
                                    desiredShaderName = desiredShaderName.Substring(0, desiredShaderName.IndexOf("#")).Trim();
                                }

                                mtdName = desiredShaderName.Trim();
                                matName = JOBCONFIG.PlaceholderMaterialName;
                            }
                            else
                            {
                                string[] matPart = fbxMaterialName.Split('|')
                                    .Select(x => x.Trim())
                                    .ToArray();

                                if (matPart.Length != 2)
                                {
                                    PrintWarning("Invalid FBX material name " +
                                    $"defined for mesh '{fbxMesh.Name}'. \n" +
                                    "Material names must be in this format: " +
                                    "\"MaterialName | ShaderName\" e.g. " +
                                    "\"Shiny Metal Chestpiece | P_Metal[DSB]\"\n\n" +
                                    "Defaulting to placeholder material " +
                                    $"with '{JOBCONFIG.PlaceholderMaterialShaderName}' shader and placeholder textures.");
                                    mtdName = JOBCONFIG.PlaceholderMaterialShaderName;
                                    matName = fbxMaterialName;
                                }
                                else
                                {
                                    matName = matPart[0];
                                    mtdName = matPart[1];
                                }
                            }

                            foreach (var texKvp in geometryContent.Material.Textures)
                            {
                                var shortTexName = GetFileNameWithoutDirectoryOrExtension(texKvp.Value.Filename);

                                if (JOBCONFIG.FBXMaterialChannelMap.ContainsKey(texKvp.Key))
                                {
                                    matTextures.Add(JOBCONFIG.FBXMaterialChannelMap[texKvp.Key], shortTexName);
                                }
                                else
                                {
                                    PrintWarning($"FBX material texture channel \"{texKvp.Key}\" was not mapped to an ingame texture channel");
                                }

                                if (!tpf.Textures.Any(t => t.Name == shortTexName))
                                {
                                    if (File.Exists(texKvp.Value.Filename))
                                    {
                                        var texBytes = File.ReadAllBytes(texKvp.Value.Filename);
                                        var texFormat = DDSHelper.GetTpfFormatFromDdsBytes(this, shortTexName, texBytes);
                                        tpf.Textures.Add(new SoulsFormats.TPF.Texture(shortTexName, (byte)texFormat, 0, 0, texBytes));
                                    }
                                    else
                                    {
                                        PrintWarning($"Texture file \"{texKvp.Value.Filename}\" did not exist. Using placeholder");

                                        if (texKvp.Key == "NormalMap")
                                        {
                                            var texFormat = DDSHelper.GetTpfFormatFromDdsBytes(this, shortTexName, FBX2FLVER_PLACEHOLDER_BUMPMAP);
                                            tpf.Textures.Add(new SoulsFormats.TPF.Texture(shortTexName, (byte)texFormat, 0, 0, FBX2FLVER_PLACEHOLDER_BUMPMAP));
                                        }
                                        else if (texKvp.Key == "Specular")
                                        {
                                            var texFormat = DDSHelper.GetTpfFormatFromDdsBytes(this, shortTexName, FBX2FLVER_PLACEHOLDER_SPECULAR);
                                            tpf.Textures.Add(new SoulsFormats.TPF.Texture(shortTexName, (byte)texFormat, 0, 0, FBX2FLVER_PLACEHOLDER_SPECULAR));
                                        }
                                        else
                                        {
                                            var texFormat = DDSHelper.GetTpfFormatFromDdsBytes(this, shortTexName, FBX2FLVER_PLACEHOLDER_DIFFUSE);
                                            tpf.Textures.Add(new SoulsFormats.TPF.Texture(shortTexName, (byte)texFormat, 0, 0, FBX2FLVER_PLACEHOLDER_DIFFUSE));
                                        }
                                    }
                                }

                                //TODO: OTHER TEXTURE TYPES
                            }

                            foreach (var hardcodedTexturePath in JOBCONFIG.MTDHardcodedTextureChannels)
                            {
                                matTextures.Add(hardcodedTexturePath.Key, hardcodedTexturePath.Value);
                            }
                        }
                        else
                        {
                            matName = null;
                            mtdName = JOBCONFIG.PlaceholderMaterialShaderName;
                            PrintWarning("No FBX material " +
                                $"defined for mesh '{fbxMesh.Name}'. \n" +
                                "Defaulting to " +
                                $"'{JOBCONFIG.PlaceholderMaterialShaderName}', with placeholder textures.");
                        }

                        if (!JOBCONFIG.MaterialLibrary.DoesMTDExist(mtdName))
                        {
                            PrintWarning($"MTD specified on mesh \"{fbxMesh.Name}\" does not exist: '{mtdName}'. \n" +
                                $"Defaulting to placeholder material '{JOBCONFIG.PlaceholderMaterialShaderName}'.");
                            mtdName = JOBCONFIG.PlaceholderMaterialShaderName;
                        }

                        var mtdRequiredTextures = JOBCONFIG.MaterialLibrary.GetRequiredTextures(mtdName);

                        var mtdMissingTextures = new List<string>();

                        foreach (var tex in mtdRequiredTextures)
                        {
                            if (!matTextures.ContainsKey(tex))
                            {
                                mtdMissingTextures.Add(tex);
                            }
                        }

                        if (mtdMissingTextures.Count > 0)
                        {
                            var sb = new System.Text.StringBuilder();
                            sb.AppendLine($"Material for mesh '{fbxMesh.Name}' uses shader '{mtdName}' but doesn't implement the following required textures:");
                            foreach (var miss in mtdMissingTextures)
                            {
                                sb.AppendLine($"    {miss}");
                            }
                            PrintWarning(sb.ToString());
                        }

                        //FBX2FLVERTODO: MATERIALS

                        flverMesh.MaterialIndex = flver.Materials.Count;

                        var placeholderGhettoMaterial = new SoulsFormats.FLVER.Material(matName, mtdName + ".mtd", 0);
                        //placeholderGhettoMaterial.Textures.Add(new SoulsFormats.FLVER.Texture("g_Diffuse", "BD_M_body.dds", 1.0f, 1.0f, 1, true, 0, 0, 0));
                        //placeholderGhettoMaterial.Textures.Add(new SoulsFormats.FLVER.Texture("g_Specular", "BD_M_body_s.dds", 1.0f, 1.0f, 1, true, 0, 0, 0));
                        //placeholderGhettoMaterial.Textures.Add(new SoulsFormats.FLVER.Texture("g_Bumpmap", "BD_M_body_n.dds", 1.0f, 1.0f, 1, true, 0, 0, 0));

                        foreach (var thing in matTextures)
                        {
                            placeholderGhettoMaterial.Textures.Add(new SoulsFormats.FLVER.Texture(thing.Key, thing.Value, System.Numerics.Vector2.One, 1, true, 0, 0, 0));
                            //placeholderGhettoMaterial.GXIndex = flverMesh.MaterialIndex;
                            //flver.GXLists.Add(new List<SoulsFormats.FLVER.GXItem>() { new SoulsFormats.FLVER.GXItem(0, 0, new byte[] { 1, 0, 0, 0, 102, 0, 0, 0, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 100, 0, 0, 0, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 0, 0, 0, 101, 0, 0, 0, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 102, 0, 0, 0, 101, 0, 0, 0, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 104, 0, 0, 0, 101, 0, 0, 0, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 127, 100, 0, 0, 0, 28, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }) });
                        }

                        flver.Materials.Add(placeholderGhettoMaterial);

                        //if (mtdName == null)
                        //{
                        //    flverMesh.Material = new FlverMaterial()
                        //    {
                        //        Name = "FBX2FLVER_Placeholder",
                        //        MTDName = "FBX2FLVER_Placeholder"
                        //    };
                        //}
                        //else if (MTDs.ContainsKey(mtdName))
                        //{
                        //    flverMesh.Material = new FlverMaterial()
                        //    {
                        //        Name = matName ?? "FBX2FLVER_Material_",
                        //        MTDName = mtdName
                        //    };

                        //    var missingTextures = new List<string>();

                        //    foreach (var extParam in MTDs[flverMesh.Material.MTDName].ExternalParams)
                        //    {
                        //        var newMatParam = new FlverMaterialParameter();
                        //        newMatParam.Name = extParam.Name;

                        //        if (matTextures.ContainsKey(extParam.Name))
                        //        {
                        //            newMatParam.Value = matTextures[extParam.Name];
                        //        }
                        //        else
                        //        {
                        //            newMatParam.Value = "";
                        //            //TODO: Check if this causes issues for mats that se g_DetailBump in vanilla
                        //            if (extParam.Name != "g_DetailBumpmap")
                        //                missingTextures.Add(extParam.Name);
                        //        }
                        //    }

                        //    if (missingTextures.Count > 0)
                        //    {
                        //        var sb = new StringBuilder();
                        //        sb.AppendLine($"Mesh '{fbxMesh.Name}' has no textures for the " +
                        //            $"following inputs in its assigned ingame material ('{mtdName}'):");

                        //        foreach (var mt in missingTextures)
                        //        {
                        //            sb.AppendLine("    " + mt);
                        //        }

                        //        PrintWarning(sb.ToString());
                        //    }
                        //}
                        //else
                        //{
                        //    PrintWarning($"The material assigned to " +
                        //        $"mesh '{fbxMesh.Name}' has " +
                        //        $"'{mtdName.Substring(0, mtdName.Length - 4)/*Remove .mtd*/}'" +
                        //        $"as the shader name, which is not a valid " +
                        //        $"ingame shader name.\nDefaulting to " +
                        //        $"'{PlaceholderMaterialName}'.");

                        //    flverMesh.Material = new FlverMaterial()
                        //    {
                        //        Name = matName ?? "FBX2FLVER_Placeholder",
                        //        MTDName = "FBX2FLVER_Placeholder"
                        //    };
                        //}

                        //if (flverMesh.Material != null && flverMesh.Material.MTDName != null && flverMesh.Material.MTDName != "FBX2FLVER_Placeholder")
                        //{
                        //    foreach (var extParam in MTDs[flverMesh.Material.MTDName].ExternalParams)
                        //    {
                        //        flverMesh.Material.Parameters.Add(new FlverMaterialParameter()
                        //        {
                        //            Name = extParam.Name,
                        //            Value = matTextures.ContainsKey(extParam.Name) ? matTextures[extParam.Name] : ""
                        //        });
                        //    }
                        //}

                        for (int i = 0; i < geometryContent.Vertices.Positions.Count; i++)
                        {
                            var nextPosition = geometryContent.Vertices.Positions[i];
                            var posVec3 = FbxPipeline.Vector3.Transform(
                                new FbxPipeline.Vector3(-nextPosition.X, nextPosition.Y, nextPosition.Z)
                                , (JOBCONFIG.UseAbsoluteVertPositions ? fbxMesh.AbsoluteTransform : fbx.Transform) * FbxPipeline.Matrix.CreateScale(FinalScaleMultiplier)
                                //* FbxPipeline.Matrix.CreateScale(FinalScaleMultiplier)
                                //* FbxPipeline.Matrix.CreateRotationZ(MathHelper.Pi)

                                );

                            var newVert = new SoulsFormats.FLVER.Vertex()
                            {
                                //Position = scaledPosition
                                Position = new System.Numerics.Vector3(posVec3.X, posVec3.Y, posVec3.Z),
                                BoneIndices = new int[] { 0, 0, 0, 0 },
                                BoneWeights = new float[] { 0, 0, 0, 0 },
                            };

                            foreach (var memb in JOBCONFIG.BufferLayout)
                            {
                                switch (memb.Semantic)
                                {
                                    case SoulsFormats.FLVER.BufferLayout.MemberSemantic.Position: break;
                                    case SoulsFormats.FLVER.BufferLayout.MemberSemantic.Normal: newVert.Normal = new System.Numerics.Vector4(0, 0, 0, JOBCONFIG.NormalWValue); break;
                                    case SoulsFormats.FLVER.BufferLayout.MemberSemantic.Tangent: newVert.Tangents.Add(new System.Numerics.Vector4()); break;
                                    case SoulsFormats.FLVER.BufferLayout.MemberSemantic.BoneIndices: newVert.BoneIndices = new int[] { 0, 0, 0, 0 }; break;
                                    case SoulsFormats.FLVER.BufferLayout.MemberSemantic.BoneWeights: newVert.BoneWeights = new float[] { 0, 0, 0, 0 }; break;
                                    case SoulsFormats.FLVER.BufferLayout.MemberSemantic.UV:
                                        if (memb.Type == SoulsFormats.FLVER.BufferLayout.MemberType.UVPair)
                                            newVert.UVs.Add(new System.Numerics.Vector3());
                                        newVert.UVs.Add(new System.Numerics.Vector3());
                                        break;
                                    case SoulsFormats.FLVER.BufferLayout.MemberSemantic.VertexColor: newVert.Colors.Add(new SoulsFormats.FLVER.Vertex.Color(255, 255, 255, 255)); break;
                                    case SoulsFormats.FLVER.BufferLayout.MemberSemantic.UnknownVector4A: newVert.UnknownVector4 = new byte[] { 0, 0, 0, 0 }; break;
                                }
                            }

                            //TODO: MAYBE TRY VERTEX COLOR FROM FBX?

                            flverMesh.Vertices.Add(newVert);

                            //var euler = Util.GetEuler(fbxMesh.AbsoluteTransform);

                            //submeshVertexHighQualityBasePositions.Add(new Vector3(nextPosition.X, nextPosition.Y, nextPosition.Z));
                            //submeshVertexHighQualityBasePositions.Add(Vector3.Transform(new Vector3(nextPosition.X, nextPosition.Y, nextPosition.Z),
                            //    Matrix.CreateRotationY(euler.Y) * Matrix.CreateRotationZ(euler.Z) * Matrix.CreateRotationX(euler.X)
                            //    ));

                            submeshVertexHighQualityBasePositions.Add(new Vector3(posVec3.X, posVec3.Y, posVec3.Z));

                        }

                        ////TEST
                        //foreach (var pos in geometryContent.Vertices.Positions)
                        //{
                        //    flverMesh.Vertices.Add(new FlverVertex()
                        //    {
                        //        //Position = scaledPosition
                        //        Position = new Vector3(pos.X, pos.Y, pos.Z) * FBX_IMPORT_SCALE
                        //    });
                        //}
                        ////TEST

                        bool hasWeights = false;

                        foreach (var channel in geometryContent.Vertices.Channels)
                        {
                            if (channel.Name == "Normal0")
                            {
                                for (int i = 0; i < flverMesh.Vertices.Count; i++)
                                {
                                    var channelValue = (FbxPipeline.Vector3)(channel[i]);
                                    var normalRotMatrix = FbxPipeline.Matrix.CreateRotationX(-MathHelper.PiOver2);
                                    var normalInputVector = new FbxPipeline.Vector3(-channelValue.X, channelValue.Y, channelValue.Z);

                                    //if (RotateNormalsBackward)
                                    //{
                                    //    normalRotMatrix *= FbxPipeline.Matrix.CreateRotationY(MathHelper.Pi);
                                    //}

                                    FbxPipeline.Vector3 rotatedNormal = FbxPipeline.Vector3.Normalize(
                                        FbxPipeline.Vector3.TransformNormal(normalInputVector, normalRotMatrix)
                                        );

                                    flverMesh.Vertices[i].Normal = new System.Numerics.Vector4()
                                    {
                                        X = rotatedNormal.X,
                                        Y = rotatedNormal.Y,
                                        Z = rotatedNormal.Z,
                                        W = flverMesh.Vertices[i].Normal.W,
                                    };



                                    submeshHighQualityNormals.Add(new Vector3(rotatedNormal.X, rotatedNormal.Y, rotatedNormal.Z));
                                }
                            }
                            else if (channel.Name.StartsWith("TextureCoordinate"))
                            {
                                var uvIndex = int.Parse(channel.Name.Substring(channel.Name.IndexOfAny(CHAR_NUMS)));

                                if (uvIndex > 2)
                                {
                                    PrintWarning($"Found a UV vertex data channel with an abnormally " +
                                        $"high index ({uvIndex}) in FBX mesh '{fbxMesh.Name}'. This UV map " +
                                        $"will be ignored, as the game only ever reads UV channels 0 or 1 " +
                                        $"(and 2 in a few map meshes).");
                                }

                                bool isBaseUv = submeshVertexHighQualityBaseUVs.Count == 0;

                                for (int i = 0; i < flverMesh.Vertices.Count; i++)
                                {
                                    var channelValue = (FbxPipeline.Vector2)channel[i];

                                    var uv = new System.Numerics.Vector3(channelValue.X, channelValue.Y, 0);

                                    if (flverMesh.Vertices[i].UVs.Count > uvIndex)
                                    {
                                        flverMesh.Vertices[i].UVs[uvIndex] = uv;
                                    }
                                    else if (flverMesh.Vertices[i].UVs.Count == uvIndex)
                                    {
                                        flverMesh.Vertices[i].UVs.Add(uv);
                                    }
                                    else if (uvIndex <= 2)
                                    {
                                        while (flverMesh.Vertices[i].UVs.Count <= uvIndex)
                                        {
                                            flverMesh.Vertices[i].UVs.Add(System.Numerics.Vector3.Zero);
                                        }
                                    }

                                    if (isBaseUv)
                                    {
                                        submeshVertexHighQualityBaseUVs.Add(
                                            new Vector2(channelValue.X, channelValue.Y));
                                    }
                                }
                            }
                            else if (channel.Name == "Weights0")
                            {
                                hasWeights = true;
                                for (int i = 0; i < flverMesh.Vertices.Count; i++)
                                {
                                    var channelValue = (BoneWeightCollection)channel[i];

                                    if (channelValue.Count >= 1)
                                    {
                                        if (bonesByName.ContainsKey(channelValue[0].BoneName))
                                        {
                                            var bone = bonesByName[channelValue[0].BoneName];
                                            float weight = channelValue[0].Weight;
                                            int index = 0;

                                            if (JOBCONFIG.UseDirectBoneIndices)
                                            {
                                                index = flver.Bones.IndexOf(bone);
                                            }
                                            else
                                            {
                                                if (bonesReferencedByThisMesh.Contains(bone))
                                                {
                                                    index = (sbyte)bonesReferencedByThisMesh.IndexOf(bone);
                                                }
                                                else
                                                {
                                                    bonesReferencedByThisMesh.Add(bone);
                                                    index = (sbyte)bonesReferencedByThisMesh.IndexOf(bone);
                                                }
                                            }

                                            flverMesh.Vertices[i].BoneIndices[0] = index;
                                            flverMesh.Vertices[i].BoneWeights[0] = weight;
                                        }
                                        //else
                                        //{
                                        //    PrintWarning($"Warning: Bone '{channelValue[0].BoneName}' does not exist.");
                                        //}
                                    }

                                    if (channelValue.Count >= 2)
                                    {
                                        if (bonesByName.ContainsKey(channelValue[1].BoneName))
                                        {
                                            var bone = bonesByName[channelValue[1].BoneName];
                                            float weight = channelValue[1].Weight;
                                            int index = 0;

                                            if (JOBCONFIG.UseDirectBoneIndices)
                                            {
                                                index = flver.Bones.IndexOf(bone);
                                            }
                                            else
                                            {
                                                if (bonesReferencedByThisMesh.Contains(bone))
                                                {
                                                    index = (sbyte)bonesReferencedByThisMesh.IndexOf(bone);
                                                }
                                                else
                                                {
                                                    bonesReferencedByThisMesh.Add(bone);
                                                    index = (sbyte)bonesReferencedByThisMesh.IndexOf(bone);
                                                }
                                            }

                                            flverMesh.Vertices[i].BoneIndices[1] = index;
                                            flverMesh.Vertices[i].BoneWeights[1] = weight;
                                        }
                                        //else
                                        //{
                                        //    PrintWarning($"Warning: Bone '{channelValue[1].BoneName}' does not exist.");
                                        //}
                                    }

                                    if (channelValue.Count >= 3)
                                    {
                                        if (bonesByName.ContainsKey(channelValue[2].BoneName))
                                        {
                                            var bone = bonesByName[channelValue[2].BoneName];
                                            float weight = channelValue[2].Weight;
                                            int index = 0;

                                            if (JOBCONFIG.UseDirectBoneIndices)
                                            {
                                                index = flver.Bones.IndexOf(bone);
                                            }
                                            else
                                            {
                                                if (bonesReferencedByThisMesh.Contains(bone))
                                                {
                                                    index = (sbyte)bonesReferencedByThisMesh.IndexOf(bone);
                                                }
                                                else
                                                {
                                                    bonesReferencedByThisMesh.Add(bone);
                                                    index = (sbyte)bonesReferencedByThisMesh.IndexOf(bone);
                                                }
                                            }

                                            flverMesh.Vertices[i].BoneIndices[2] = index;
                                            flverMesh.Vertices[i].BoneWeights[2] = weight;
                                        }
                                        //else
                                        //{
                                        //    PrintWarning($"Warning: Bone '{channelValue[2].BoneName}' does not exist.");
                                        //}
                                    }

                                    if (channelValue.Count >= 4)
                                    {
                                        if (bonesByName.ContainsKey(channelValue[3].BoneName))
                                        {
                                            var bone = bonesByName[channelValue[3].BoneName];
                                            float weight = channelValue[3].Weight;
                                            int index = 0;

                                            if (JOBCONFIG.UseDirectBoneIndices)
                                            {
                                                index = flver.Bones.IndexOf(bone);
                                            }
                                            else
                                            {
                                                if (bonesReferencedByThisMesh.Contains(bone))
                                                {
                                                    index = (sbyte)bonesReferencedByThisMesh.IndexOf(bone);
                                                }
                                                else
                                                {
                                                    bonesReferencedByThisMesh.Add(bone);
                                                    index = (sbyte)bonesReferencedByThisMesh.IndexOf(bone);
                                                }
                                            }

                                            flverMesh.Vertices[i].BoneIndices[3] = index;
                                            flverMesh.Vertices[i].BoneWeights[3] = weight;
                                        }
                                        //else
                                        //{
                                        //    PrintWarning($"Warning: Bone '{channelValue[3].BoneName}' does not exist.");
                                        //}
                                    }
                                }
                            }
                            else
                            {
                                PrintWarning($"Found an unfamiliar vertex data " +
                                    $"channel ('{channel.Name}') in FBX mesh '{fbxMesh.Name}'.");
                            }
                        }

                        if (!hasWeights)
                        {
                            foreach (var vert in flverMesh.Vertices)
                            {
                                vert.BoneIndices = new int[] { 0, 0, 0, 0 };
                                vert.BoneWeights = new float[] { 0,0,0,0 };
                                //vert.BoneWeights = new FlverBoneWeights(0, 0, 0, 0);

                                //vert.BoneWeights = new FlverBoneWeights(65535, 65535, 65535, 65535);
                                //TODO: FIND OUT WHY AUTO MAX WEIGHT WAS TURNING OBJECT THE COLOR OF ITS NORMALS INGAME????????!!!!!!!!!!!!!!!
                            }
                            flverMesh.Dynamic = false;
                        }
                        else
                        {
                            flverMesh.Dynamic = true;
                        }



                    }
                }

                if (bonesReferencedByThisMesh.Count == 0 && flver.Bones.Count > 0)
                {
                    bonesReferencedByThisMesh.Add(flver.Bones[0]);
                }

                

                foreach (var refBone in bonesReferencedByThisMesh)
                {
                    flverMesh.BoneIndices.Add(flver.Bones.IndexOf(refBone));
                }

                //foreach (var faceset in flverMesh.FaceSets)
                //{
                //    for (int i = 0; i < faceset.VertexIndices.Count; i += 3)
                //    {
                //        var a = faceset.VertexIndices[i];
                //        //var b = faceset.VertexIndices[i + 1];
                //        var c = faceset.VertexIndices[i + 2];

                //        faceset.VertexIndices[i] = c;
                //        //faceset.VertexIndices[i + 1] = b;
                //        faceset.VertexIndices[i + 2] = a;
                //    }
                //}

                var submeshVertexIndices = new List<int>();

                foreach (var faceSet in flverMesh.FaceSets)
                {
                    submeshVertexIndices.AddRange(faceSet.Indices);
                }

                //submeshVertexIndices.Reverse();
                //submeshVertexIndices = submeshVertexIndices.Reverse<ushort>().ToList();

                //submeshHighQualityNormals =
                //    NormalSolver.SolveNormals(submeshVertexIndices, flverMesh.Vertices,
                //    submeshVertexHighQualityBasePositions, onOutput, onError);

                if (submeshVertexHighQualityBaseUVs.Count == 0)
                {
                    PrintError($"Mesh '{fbxMesh.Name}' has no UVs. " +
                        $"UVs are needed to calculate the mesh tangents properly.");
                    return false;
                }

                submeshHighQualityTangents = TangentSolver.SolveTangents(flverMesh, submeshVertexIndices, 
                    submeshHighQualityNormals,
                    submeshVertexHighQualityBasePositions,
                    submeshVertexHighQualityBaseUVs);

                for (int i = 0; i < flverMesh.Vertices.Count; i++)
                {
                    Vector3 thingy = Vector3.Normalize(Vector3.Cross(submeshHighQualityNormals[i],
                       new Vector3(submeshHighQualityTangents[i].X,
                       submeshHighQualityTangents[i].Y,
                       submeshHighQualityTangents[i].Z) * submeshHighQualityTangents[i].W));

                    flverMesh.Vertices[i].Tangents[0] = new System.Numerics.Vector4(thingy.X, thingy.Y, thingy.Z, submeshHighQualityTangents[i].W);
                }

                


                //FBX2FLVERTODO BUFFER LAYOUTS

                //FBXFLVERTODO Bone Shit

                foreach (var mesh in flver.Meshes)
                {
                    if (mesh.BoneIndices.Count == 0)
                        mesh.BoneIndices.Add(0);

                    mesh.DefaultBoneIndex = 0;
                }
                

                //foreach (var kvpB in flverMeshNameMap)
                //{
                //    var matchingBones = flver.Bones.Where(x => x.Name == kvp.Value);
                //    int boneIndex = -1;
                //    if (matchingBones.Any())
                //    {
                //        boneIndex = flver.Bones.IndexOf(matchingBones.First());
                //    }
                //    else
                //    {
                //        var newMeshBone = new FlverBone(flver)
                //        {
                //            Name = kvp.Value
                //        };
                //        flver.Bones.Add(newMeshBone);
                //        boneIndex = flver.Bones.Count - 1;
                //    }
                //    kvp.Key.NameBoneIndex = boneIndex;
                //}

                var topLevelParentBones = flver.Bones.Where(x => x.ParentIndex == -1).ToArray();

                if (topLevelParentBones.Length > 0)
                {
                    for (int i = 0; i < topLevelParentBones.Length; i++)
                    {
                        if (i == 0)
                            topLevelParentBones[i].PreviousSiblingIndex = -1;
                        else
                            topLevelParentBones[i].PreviousSiblingIndex = (short)flver.Bones.IndexOf(topLevelParentBones[i - 1]);

                        if (i == topLevelParentBones.Length - 1)
                            topLevelParentBones[i].NextSiblingIndex = -1;
                        else
                            topLevelParentBones[i].NextSiblingIndex = (short)flver.Bones.IndexOf(topLevelParentBones[i + 1]);
                    }
                }


                flverMesh.VertexBuffers.Add(new SoulsFormats.FLVER.VertexBuffer(layoutIndex: 0));

                //if (flverMesh.NameBoneIndex < 0)
                //{
                //    var defaultBone = flver.FindBone(fbxMesh.Name, ignoreErrors: true);
                //    if (defaultBone != null)
                //    {
                //        flverMesh.NameBoneIndex = flver.Bones.IndexOf(defaultBone);
                //    }
                //    else
                //    {
                //        flverMesh.NameBoneIndex = -1;
                //    }
                //}

                flver.Meshes.Add(flverMesh);
            }

            OrientationSolver.SolveOrientation(flver, solveBones: true);

            if (JOBCONFIG.GenerateLodAndMotionBlur)
                GeneratePlaceholderLODs(flver);

            //FBX2FLVERTODO: Fix Bounding Boxes
            //BoundingBoxSolver.FixAllBoundingBoxes(flver);

            flver.BufferLayouts = new List<SoulsFormats.FLVER.BufferLayout> { JOBCONFIG.BufferLayout };

            flver.Header.BoundingBoxMax = new System.Numerics.Vector3(1,1,1);
            flver.Header.BoundingBoxMin = new System.Numerics.Vector3(-1,-1,-1);

            flver.Header.Unk5C = JOBCONFIG.Unk0x5CValue;
            flver.Header.Unk68 = JOBCONFIG.Unk0x68Value;

            foreach (var dmyKvp in dummyFollowBoneMap)
            {
                var followBone = flver.Bones.FirstOrDefault(b => b.Name == dmyKvp.Value);
                if (followBone != null)
                {
                    dmyKvp.Key.dmy.AttachBoneIndex = (short)(flver.Bones.IndexOf(followBone));
                }
                else
                {
                    PrintWarning($"Dummy node '{dmyKvp.Key.nodeName}' is attempting to follow a bone which does not exist ('{dmyKvp.Value}') and will instead be set to follow nothing.");
                }
            }

            if (JOBCONFIG.UseDirectBoneIndices)
            {
                foreach (var m in flver.Meshes)
                {
                    m.BoneIndices.Clear();
                }
            }
            

            foreach (var kvp in FBX_Meshes)
            {
                flverSubmeshNameMap.Add(kvp.Key, kvp.Value.Name);
            }

            return true;
        }

        private void GeneratePlaceholderLODs(SoulsFormats.FLVER flver)
        {
            foreach (var submesh in flver.Meshes)
            {
                var newFacesetsToAdd = new List<SoulsFormats.FLVER.FaceSet>();
                foreach (var faceset in submesh.FaceSets)
                {
                    var lod1 = new SoulsFormats.FLVER.FaceSet()
                    {
                        CullBackfaces = faceset.CullBackfaces,
                        Flags = SoulsFormats.FLVER.FaceSet.FSFlags.LodLevel1,
                        TriangleStrip = faceset.TriangleStrip,
                        Indices = faceset.Indices
                    };

                    var lod2 = new SoulsFormats.FLVER.FaceSet()
                    {
                        CullBackfaces = faceset.CullBackfaces,
                        Flags = SoulsFormats.FLVER.FaceSet.FSFlags.LodLevel2,
                        TriangleStrip = faceset.TriangleStrip,
                        Indices = faceset.Indices
                    };

                    var mblur = new SoulsFormats.FLVER.FaceSet()
                    {
                        CullBackfaces = faceset.CullBackfaces,
                        Flags = SoulsFormats.FLVER.FaceSet.FSFlags.MotionBlur,
                        TriangleStrip = faceset.TriangleStrip,
                        Indices = faceset.Indices
                    };

                    var mblurlod1 = new SoulsFormats.FLVER.FaceSet()
                    {
                        CullBackfaces = faceset.CullBackfaces,
                        Flags = SoulsFormats.FLVER.FaceSet.FSFlags.LodLevel1 | SoulsFormats.FLVER.FaceSet.FSFlags.MotionBlur,
                        TriangleStrip = faceset.TriangleStrip,
                        Indices = faceset.Indices
                    };

                    var mblurlod2 = new SoulsFormats.FLVER.FaceSet()
                    {
                        CullBackfaces = faceset.CullBackfaces,
                        Flags = SoulsFormats.FLVER.FaceSet.FSFlags.LodLevel2 | SoulsFormats.FLVER.FaceSet.FSFlags.MotionBlur,
                        TriangleStrip = faceset.TriangleStrip,
                        Indices = faceset.Indices
                    };

                    newFacesetsToAdd.Add(lod1);
                    newFacesetsToAdd.Add(lod2);
                    newFacesetsToAdd.Add(mblur);
                    newFacesetsToAdd.Add(mblurlod1);
                    newFacesetsToAdd.Add(mblurlod2);
                }

                foreach (var lod in newFacesetsToAdd)
                {
                    submesh.FaceSets.Add(lod);
                }
            }
        }

        //private void FixBodyNormals(FLVER flver)
        //{
        //    foreach (var sm in flver.Submeshes)
        //    {
        //        var submeshName = sm.GetName();
        //        Print($"Checking if submesh '{submeshName}' is part of human body base mesh (MTD '{sm.Material.MTDName}')...");
        //        var bodyPartType = submeshName.Split('|')[0].Trim();
        //        if (sm.Material.MTDName.ToUpper().Trim() == "PS_BODY[DSB].MTD")
        //        {
        //            Print($"Attempting to fix human body submesh '{submeshName}', targeting body part type '{bodyPartType}'...");
        //            var fixResults = HumanBodyFixer.FixBodyPiece(sm, bodyPartType, out string possibleError);
        //            if (possibleError != null)
        //            {
        //                PrintError(possibleError);
        //            }
        //            else
        //            {
        //                if (fixResults.VertsFixed > 0)
        //                    Print($"Matched {fixResults.VertsFixed} / {sm.Vertices.Count} vertices in submesh '{submeshName}' to the original '{bodyPartType}' mesh (which has {fixResults.TotalSourceVerts} verts total) and fixed normals, tangents, and bone weights...");
        //                else
        //                    PrintWarning($"Unable to match any vertices in submesh '{submeshName}' with the originals in '{sm.Material.Name}'.");
        //            }
        //        }
        //    }
        //}
    }
}
