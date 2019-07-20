using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsFormats;

namespace FBX2FLVER.Solvers
{
    public class BoundingBoxSolver
    {
        private readonly FBX2FLVERImporter Importer;
        public BoundingBoxSolver(FBX2FLVERImporter Importer)
        {
            this.Importer = Importer;
        }

        private Dictionary<FLVER.Vertex, List<FLVER.Bone>> PrecalculatedBoneLists = new Dictionary<FLVER.Vertex, List<FLVER.Bone>>();

        private List<FLVER.Bone> GetAllBonesReferencedByVertex(FLVER f, FLVER.Mesh m, FLVER.Vertex v)
        {
            if (!PrecalculatedBoneLists.ContainsKey(v))
            {
                List<FLVER.Bone> result = new List<FLVER.Bone>();

                foreach (var vertBoneIndex in v.BoneIndices)
                {
                    if (vertBoneIndex >= 0)
                    {
                        if (Importer.JOBCONFIG.UseDirectBoneIndices)
                        {
                            result.Add(f.Bones[vertBoneIndex]);
                        }
                        else
                        {
                            if (m.BoneIndices[vertBoneIndex] >= 0)
                                result.Add(f.Bones[m.BoneIndices[vertBoneIndex]]);
                        }
                    }
                }

                PrecalculatedBoneLists.Add(v, result);
            }

            return PrecalculatedBoneLists[v];
        }

        private List<FLVER.Vertex> GetVerticesParentedToBone(FLVER f, FLVER.Bone b)
        {
            var result = new List<FLVER.Vertex>();
            foreach (var sm in f.Meshes)
            {
                foreach (var v in sm.Vertices)
                {
                    var bonesReferencedByThisShit = GetAllBonesReferencedByVertex(f, sm, v);
                    if (bonesReferencedByThisShit.Contains(b))
                        result.Add(v);
                }
            }
            return result;
        }

        private BoundingBox GetBoundingBox(List<Vector3> verts)
        {
            if (verts.Count > 0)
                return BoundingBox.CreateFromPoints(verts);
            else
                return new BoundingBox(Vector3.Zero, Vector3.Zero);
        }

        Matrix GetParentBoneMatrix(FLVER f, FLVER.Bone bone)
        {
            FLVER.Bone parent = bone;

            var boneParentMatrix = Matrix.Identity;

            do
            {
                boneParentMatrix *= Matrix.CreateScale(parent.Scale.X, parent.Scale.Y, parent.Scale.Z);
                boneParentMatrix *= Matrix.CreateRotationX(parent.Rotation.X);
                boneParentMatrix *= Matrix.CreateRotationZ(parent.Rotation.Z);
                boneParentMatrix *= Matrix.CreateRotationY(parent.Rotation.Y);

                //boneParentMatrix *= Matrix.CreateRotationY(parent.EulerRadian.Y);
                //boneParentMatrix *= Matrix.CreateRotationZ(parent.EulerRadian.Z);
                //boneParentMatrix *= Matrix.CreateRotationX(parent.EulerRadian.X);
                boneParentMatrix *= Matrix.CreateTranslation(parent.Translation.X, parent.Translation.Y, parent.Translation.Z);
                //boneParentMatrix *= Matrix.CreateScale(parent.Scale);

                if (parent.ParentIndex >= 0)
                {
                    parent = f.Bones[parent.ParentIndex];
                }
                else
                {
                    parent = null;
                }
            }
            while (parent != null);

            return boneParentMatrix;
        }

        private void SetBoneBoundingBox(FLVER f, FLVER.Bone b)
        {
            var bb = GetBoundingBox(GetVerticesParentedToBone(f, b).Select(v => new Vector3(v.Position.X, v.Position.Y, v.Position.Z)).ToList());
            if (bb.Max.LengthSquared() != 0 || bb.Min.LengthSquared() != 0)
            {
                var matrix = GetParentBoneMatrix(f, b);
                b.BoundingBoxMin = Vector3.Transform(bb.Min, Matrix.Invert(matrix)).ToNumerics();
                b.BoundingBoxMax = Vector3.Transform(bb.Max, Matrix.Invert(matrix)).ToNumerics();
            }
            else
            {
                b.BoundingBoxMin = new System.Numerics.Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                b.BoundingBoxMax = new System.Numerics.Vector3(float.MinValue, float.MinValue, float.MinValue);
            }
        }

        public void FixAllBoundingBoxes(FLVER f)
        {
            PrecalculatedBoneLists.Clear();

            foreach (var b in f.Bones)
            {
                SetBoneBoundingBox(f, b);
                //if (b.Name == "Dummy")
                //    b.Name = "dymmy";
                //else if (b.Name == "SFX")
                //    b.Name = "SFX用";
            }


            var submeshBBs = new List<BoundingBox>();

            foreach (var sm in f.Meshes)
            {
                var bb = GetBoundingBox(sm.Vertices.Select(v => new Vector3(v.Position.X, v.Position.Y, v.Position.Z)).ToList());
                if (bb.Max.LengthSquared() != 0 || bb.Min.LengthSquared() != 0)
                {
                    submeshBBs.Add(bb);
                    sm.BoundingBox = new FLVER.Mesh.BoundingBoxes();
                    sm.BoundingBox.Min = bb.Min.ToNumerics();
                    sm.BoundingBox.Max = bb.Max.ToNumerics();
                }
                else
                {
                    sm.BoundingBox = null;
                }
            }

            if (submeshBBs.Count > 0)
            {
                var finalBB = submeshBBs[0];
                for (int i = 1; i < submeshBBs.Count; i++)
                {
                    finalBB = BoundingBox.CreateMerged(finalBB, submeshBBs[i]);
                }

                f.Header.BoundingBoxMin = new System.Numerics.Vector3(finalBB.Min.X, finalBB.Min.Y, finalBB.Min.Z);
                f.Header.BoundingBoxMax = new System.Numerics.Vector3(finalBB.Max.X, finalBB.Max.Y, finalBB.Max.Z);
            }
            else
            {
                f.Header.BoundingBoxMin = new System.Numerics.Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                f.Header.BoundingBoxMax = new System.Numerics.Vector3(float.MinValue, float.MinValue, float.MinValue);
            }



        }
    }
}
