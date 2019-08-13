using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBX2FLVER.Solvers
{
    public class OrientationSolver
    {
        private readonly FBX2FLVERImporter Importer;
        public OrientationSolver(FBX2FLVERImporter Importer)
        {
            this.Importer = Importer;
        }

        public void SolveOrientation(SoulsFormats.FLVER2 flver, bool solveBones)
        {
            foreach (var flverMesh in flver.Meshes)
            {
                for (int i = 0; i < flverMesh.Vertices.Count; i++)
                {

                    var m = Matrix.Identity
                    * Matrix.CreateRotationY(Importer.JOBCONFIG.SceneRotation.Y)
                    * Matrix.CreateRotationZ(Importer.JOBCONFIG.SceneRotation.Z)
                    * Matrix.CreateRotationX(Importer.JOBCONFIG.SceneRotation.X)
                    ;

                    flverMesh.Vertices[i].Position = Vector3.Transform(new Vector3(flverMesh.Vertices[i].Position.X, flverMesh.Vertices[i].Position.Y, flverMesh.Vertices[i].Position.Z), m).ToNumerics();
                    Vector3 normVec = Vector3.Normalize(Vector3.Transform(new Vector3(flverMesh.Vertices[i].Normal.X, flverMesh.Vertices[i].Normal.Y, flverMesh.Vertices[i].Normal.Z), m));
                    flverMesh.Vertices[i].Normal = new System.Numerics.Vector4(normVec.X, normVec.Y, normVec.Z, flverMesh.Vertices[i].Normal.W);
                    var rotBitangentVec3 = Vector3.Transform(new Vector3(flverMesh.Vertices[i].Tangents[0].X, flverMesh.Vertices[i].Tangents[0].Y, flverMesh.Vertices[i].Tangents[0].Z), m);
                    flverMesh.Vertices[i].Tangents[0] = new System.Numerics.Vector4(rotBitangentVec3.X, rotBitangentVec3.Y, rotBitangentVec3.Z, flverMesh.Vertices[i].Tangents[0].W);
                }
            }

            //Matrix GetBoneMatrix(FlverBone b)
            //{
            //    return Matrix.CreateScale(b.Scale)
            //    * Matrix.CreateRotationX(b.EulerRadian.X)
            //    * Matrix.CreateRotationZ(b.EulerRadian.Z)
            //    * Matrix.CreateRotationY(b.EulerRadian.Y)
            //    * Matrix.CreateTranslation(b.Translation)
            //    ;
            //}

            //void ApplyBoneMatrix(FlverBone b, Matrix m)
            //{
            //    Matrix orig = GetBoneMatrix(b);
            //    orig *= m;

            //    b.Translation = orig.Translation;
            //    b.Scale = orig.Scale;
            //    b.EulerRadian = Util.GetFlverEulerFromQuaternion(orig.Rotation);
            //}
            //bool anyAdjusted = false;
            //do
            //{
            //anyAdjusted = false;

            //foreach (var b in flver.Bones)
            //{
            //    b.EulerRadian = Vector3.Zero;
            //    //if (b.ParentIndex == -1)
            //    //{
            //    //    b.EulerRadian = Vector3.Zero;
            //    //    //b.Scale = Vector3.One;
            //    //    //b.Translation = Vector3.Zero;
            //    //}
            //}


            //ACTUALY WORKS:

            if (solveBones)
            {
                for (int b = 0; b < flver.Bones.Count; b++)
                {
                    if (flver.Bones[b].ParentIndex == -1)
                    {

                        if (Importer.JOBCONFIG.SkeletonRotation != Vector3.Zero)
                        {
                            Matrix boneTrans_Xna = Matrix.CreateRotationY(Importer.JOBCONFIG.SkeletonRotation.Y)
                            *  Matrix.CreateRotationZ(Importer.JOBCONFIG.SkeletonRotation.Z)
                            * Matrix.CreateRotationX(Importer.JOBCONFIG.SkeletonRotation.X);

                            flver.Bones[b].Rotation += new System.Numerics.Vector3(
                                Importer.JOBCONFIG.SkeletonRotation.X, 
                                Importer.JOBCONFIG.SkeletonRotation.Y, 
                                Importer.JOBCONFIG.SkeletonRotation.Z);

                            flver.Bones[b].Translation = Vector3.Transform(new Vector3(flver.Bones[b].Translation.X, flver.Bones[b].Translation.Y, flver.Bones[b].Translation.Z), boneTrans_Xna).ToNumerics();


                        }
                    }
                    //flver.Bones[b].Scale = Vector3.One;

                    //bool hasChildren = false;
                    foreach (var ch in flver.Bones.Where(bone => bone.ParentIndex == b))
                    {
                        ch.Translation *= flver.Bones[b].Scale;
                        ch.Scale *= flver.Bones[b].Scale;
                        //hasChildren = true;
                        
                    }

                    foreach (var dmy in flver.Dummies.Where(d => d.DummyBoneIndex == b))
                    {
                        dmy.Position *= flver.Bones[b].Scale;
                    }

                    flver.Bones[b].Scale = System.Numerics.Vector3.One;

                    //if (flver.Bones[b].Scale.X < 0)
                    //{
                    //    flver.Bones[b].Scale.X *= -1;
                    //    flver.Bones[b].EulerRadian.Y += MathHelper.Pi;

                    //    foreach (var dmy in flver.Dummies.Where(dm => dm.ParentBoneIndex == b))
                    //    {
                    //        dmy.Position *= new Vector3(-1, 1, 1);
                    //    }
                    //}

                    //if (flver.Bones[b].Scale.Y < 0)
                    //{
                    //    flver.Bones[b].Scale.Y *= -1;
                    //    flver.Bones[b].EulerRadian.X += MathHelper.Pi;

                    //    foreach (var dmy in flver.Dummies.Where(dm => dm.ParentBoneIndex == b))
                    //    {
                    //        dmy.Position *= new Vector3(1, -1, 1);
                    //    }
                    //}

                    //Do this only for parent bones, cuz child bones could be real bones like on a whip...?
                    //if (Importer.OutputType == FBX2FLVEROutputType.Weapon && flver.Bones[b].ParentIndex == -1)
                    //{
                    //    var oldBoneRotMatrix = Matrix.CreateRotationY(flver.Bones[b].EulerRadian.Y)
                    //        * Matrix.CreateRotationZ(flver.Bones[b].EulerRadian.Z)
                    //        * Matrix.CreateRotationX(flver.Bones[b].EulerRadian.X);
                    //    foreach (var dmy in flver.Dummies.Where(d => d.ParentBoneIndex == b))
                    //    {
                    //        dmy.Position = Vector3.Transform(dmy.Position, Matrix.Invert(oldBoneRotMatrix) * Matrix.CreateRotationX(MathHelper.Pi));
                    //    }
                    //    flver.Bones[b].EulerRadian = Vector3.Zero;
                    //    flver.Bones[b].Scale = Vector3.One;
                    //}
                }
            }


            if (Importer.JOBCONFIG.RotateNormalsBackward || Importer.JOBCONFIG.ConvertNormalsAxis)
            {
                foreach (var sm in flver.Meshes)
                {
                    foreach (var vert in sm.Vertices)
                    {
                        if (vert.Normal != null)
                        {
                            if (Importer.JOBCONFIG.ConvertNormalsAxis)
                            {
                                var x = vert.Normal.X;
                                var y = vert.Normal.Y;
                                var z = vert.Normal.Z;
                                var w = vert.Normal.W;

                                vert.Normal = new System.Numerics.Vector4(x, -z, y, w);
                            }

                            if (Importer.JOBCONFIG.RotateNormalsBackward)
                            {
                                var normVec3 = Vector3.Transform(new Vector3(vert.Normal.X, vert.Normal.Y, vert.Normal.Z), Matrix.CreateRotationY(MathHelper.Pi));
                                vert.Normal = new System.Numerics.Vector4(normVec3.X, normVec3.Y, normVec3.Z, vert.Normal.W);
                            }
                                

                            
                        }
                    }
                }
            }



            //foreach (var m in flver.Submeshes)
            //{
            //    foreach (var v in m.Vertices)
            //    {
            //        var norm = (Vector3)v.Normal;
            //        var tan = (Vector3)v.BiTangent;

            //        v.BiTangent = new Vector4(Vector3.Cross(norm, tan) * v.BiTangent.W, v.BiTangent.W);
            //    }
            //}


            //}
            //while (anyAdjusted);




            //if (solveBones)
            //{
            //    foreach (var bone in flver.Bones.Where(b => b.ParentIndex == -1))
            //    {
            //        var origMatrix = Matrix.CreateTranslation(bone.Translation)
            //            * Matrix.CreateScale(bone.Scale);

            //        var m = Matrix.CreateRotationZ(-MathHelper.PiOver2)
            //            //* Matrix.CreateRotationX(MathHelper.Pi)
            //            ;

            //        if ((origMatrix * m).Decompose(out var scale, out _, out var trans))
            //        {
            //            bone.Translation = trans;
            //            bone.Scale = scale;
            //        }

            //        bone.EulerRadian.Z += -MathHelper.PiOver2;
            //        //bone.EulerRadian.X += MathHelper.Pi;

            //    }
            //}

            foreach (var dmy in flver.Dummies)
            {
                var m = Matrix.Identity;
                bool wasAnyRotationAppliedFatcat = false;
                if (Importer.JOBCONFIG.SceneRotation.Y != 0)
                {
                    m *= Matrix.CreateRotationY(Importer.JOBCONFIG.SceneRotation.Y);
                    wasAnyRotationAppliedFatcat = true;
                }
                if (Importer.JOBCONFIG.SceneRotation.Z != 0)
                {
                    m *= Matrix.CreateRotationZ(Importer.JOBCONFIG.SceneRotation.Z);
                    wasAnyRotationAppliedFatcat = true;
                }
                if (Importer.JOBCONFIG.SceneRotation.X != 0)
                {
                    m *= Matrix.CreateRotationX(Importer.JOBCONFIG.SceneRotation.X);
                    wasAnyRotationAppliedFatcat = true;
                }
                    
                if (wasAnyRotationAppliedFatcat)
                    dmy.Position = Vector3.Transform(new Vector3(dmy.Position.X, dmy.Position.Y, dmy.Position.Z), m).ToNumerics();
            }
        }
    }
}
