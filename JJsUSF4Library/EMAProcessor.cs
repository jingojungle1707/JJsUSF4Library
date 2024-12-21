using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using JJsUSF4Library.FileClasses;
using JJsUSF4Library.FileClasses.SubfileClasses;

namespace JJsUSF4Library
{
    public class EMAProcessor : IAnimationProcessor
    {
        private float _currentFrame = 0;
        private int _currentAnimationIndex = 0;
        public float CurrentFrame
        {
            get { return _currentFrame; }
            set
            {
                //SetupFrame(value);
                value = value % Ema.Animations[CurrentAnimationIndex].Duration;
                _currentFrame = value;
                AnimateFrame(value);

            }
        }
        public int CurrentAnimationIndex
        {
            get { return _currentAnimationIndex; }
            set
            {
                SetupAnimation(value);
            }
        }
        public int CurrentFaceIndex { get; private set; }
        public EMA Ema { get; private set; }
        public EMA FaceEma { get; private set; }
        public EMO Emo { get; private set; }
        public AnimatedSkeleton AnimatedSkeleton { get; private set; }
        public AnimatedNode[] AnimatedNodes { get; private set; }
        public string CurrentAnimationName
        {
            get { return Ema.Animations[CurrentAnimationIndex].Name; }
        }
        //private List<CMDTrack> _currentTracks = new List<CMDTrack>();


        public EMAProcessor(EMA animatedEma, EMA faceEma, EMO skinnedEmo = default)
        {
            FaceEma = new EMA();

            //Deep copy of faceEma
            FaceEma.ReadFromStream(new System.IO.BinaryReader(new System.IO.MemoryStream(faceEma.GenerateBytes())));
            Ema = animatedEma;
            Emo = skinnedEmo;

            //Retarget face command tracks
            foreach (Animation a in FaceEma.Animations)
            {
                foreach (CMDTrack c in a.CMDTracks)
                {
                    c.BoneID = Ema.Skeleton.NodeNames.IndexOf(FaceEma.Skeleton.NodeNames[c.BoneID]);
                }
            }

            AnimatedSkeleton = new AnimatedSkeleton(Ema.Skeleton);
            AnimatedNodes = AnimatedSkeleton.AnimatedNodes.ToArray();
            SetupRestPose();

            CurrentFaceIndex = 0;
            _currentAnimationIndex = 0;
            _currentFrame = 0;
            //SetupAnimation(0);
        }

        public AnimatedNode[] ReturnAnimatedNodes()
        {
            return AnimatedNodes;
        }

        private void SetupRestPose()
        {
            //Overwrite rest pose with the _emo rest pose
            foreach (Node n in AnimatedSkeleton.AnimatedNodes)
            {
                int index = Emo.Skeleton.NodeNames.IndexOf(n.Name);
                switch (n.Name)
                {
                    case "RArm2":
                        index = Emo.Skeleton.NodeNames.IndexOf("RElbow");
                        break;
                    case "LArm2":
                        index = Emo.Skeleton.NodeNames.IndexOf("LElbow");
                        break;
                    case "RLeg2":
                        index = Emo.Skeleton.NodeNames.IndexOf("RKnee");
                        break;
                    case "LLeg2":
                        index = Emo.Skeleton.NodeNames.IndexOf("LKnee");
                        break;
                    default:
                        break;
                }
                if (index >= 0)
                {
                    n.TransformMatrix = Emo.Skeleton.Nodes[index].LocalMatrix;
                }
                else
                {
                    Debug.Print($"No match for {n.Name}");
                }
            }
        }

        public Matrix4x4 GetAnimatedLocalMatrixByNodeName(string nodeName)
        {
            return AnimatedNodes.Where(n => n.Name == nodeName).FirstOrDefault().AnimatedLocalMatrix;
        }

        private void SetupAnimation(int animationIndex)
        {
            _currentFrame = 0;
            _currentAnimationIndex = animationIndex;
        }

        private void AnimateFrame(float frame)
        {
            //Lock the frame within the duration
            //CurrentFrame = frame % Ema.Animations[CurrentAnimationIndex].Duration;
            SetupFrame(frame);

            for (int i = 0; i < AnimatedNodes.Length; i++)
            {
                UpdateNode(AnimatedNodes[i]);

            }

            for (int i = 0; i < Ema.Skeleton.IKDataBlocks.Count; i++)
            {
                updateIKData(i);
            }

        }

        private void SetupFrame(float frame)
        {
            //Reset all the nodes, and calculate their transforms
            for (int i = 0; i < AnimatedNodes.Length; i++)
            {
                //Reset flags
                AnimatedNodes[i].AnimationProcessingDone = false;
                AnimatedNodes[i].AnimatedAbsoluteRotationFlag = false;
                AnimatedNodes[i].AnimatedAbsoluteScaleFlag = false;
                AnimatedNodes[i].AnimatedAbsoluteTranslationFlag = false;
                //Reset transformation
                AnimatedNodes[i].AnimatedRotation = AnimatedNodes[i].Rotation;
                AnimatedNodes[i].AnimatedScale = AnimatedNodes[i].Scale;
                AnimatedNodes[i].AnimatedTranslation = AnimatedNodes[i].Translation;
                AnimatedNodes[i].AnimatedRotationQuaternion = AnimatedNodes[i].RotationQuaternion;
                AnimatedNodes[i].AnimatedMatrix = AnimatedNodes[i].TransformMatrix;

                //Calculate interpolated values, if the node is animated
                if (getTransform(_currentAnimationIndex, _currentFrame, AnimatedNodes[i].ID, 0, out Vector3 translation, out bool absoluteTranslation))
                {
                    AnimatedNodes[i].AnimatedAbsoluteTranslationFlag = absoluteTranslation;
                    AnimatedNodes[i].AnimatedTranslation = translation;

                }
                if (getTransform(_currentAnimationIndex, _currentFrame, AnimatedNodes[i].ID, 1, out Vector3 rotation_d, out bool absoluteRotation))
                {
                    //aNodes_array[i].animatedRotation = new Vector3((float)(rotation_d.X * Math.PI / 180d), (float)(rotation_d.Y * Math.PI / 180d), (float)(rotation_d.Z * Math.PI / 180d));
                    AnimatedNodes[i].AnimatedAbsoluteRotationFlag = absoluteRotation;
                    AnimatedNodes[i].AnimatedRotation = new Vector3(rotation_d.X, rotation_d.Y, rotation_d.Z);
                    EMAProcessorUtils.EulerToQuaternionXYZ((float)(rotation_d.Y * Math.PI / 180d), (float)(rotation_d.Z * Math.PI / 180d), (float)(rotation_d.X * Math.PI / 180d), out Quaternion quaternion);
                    AnimatedNodes[i].AnimatedRotationQuaternion = quaternion;
                }
                if (getTransform(_currentAnimationIndex, _currentFrame, AnimatedNodes[i].ID, 2, out Vector3 scale, out bool absoluteScale))
                {
                    AnimatedNodes[i].AnimatedAbsoluteScaleFlag = absoluteScale;
                    AnimatedNodes[i].AnimatedScale = scale;
                }

                AnimatedNodes[i].AnimatedMatrix = Matrix4x4.CreateScale(AnimatedNodes[i].AnimatedScale) * Matrix4x4.CreateFromQuaternion(AnimatedNodes[i].AnimatedRotationQuaternion) * Matrix4x4.CreateTranslation(AnimatedNodes[i].AnimatedTranslation);
            }
        }

        private bool getTransform(int anim_index, float frame, int bone_index, int transform_type, out Vector3 values, out bool absolute)
        {
            bool animated = false;
            absolute = false;

            values = new Vector3(0, 0, 0);

            int coreTracks = Ema.Animations[CurrentAnimationIndex].CMDTracks.Count;

            List<CMDTrack> tracks = Ema.Animations[CurrentAnimationIndex].CMDTracks.Select(x => x).ToList();
            tracks.AddRange(FaceEma.Animations[CurrentFaceIndex].CMDTracks);
            //_currentTracks = tracks;

            foreach (CMDTrack c in tracks.Where(x => x.BoneID == bone_index && x.TransformType == transform_type))
            {
                int axis = c.BitFlag & 0x03;

                animated = true;
                if ((c.BitFlag & 0x10) == 0x10) absolute = true;
                else absolute = false;

                float val = EMAProcessorUtils.InterpolateRelativeKeyFrames(c, frame);
                switch (axis)
                {
                    case 0:
                        values.X = val;
                        break;
                    case 1:
                        values.Y = val;
                        break;
                    case 2:
                        values.Z = val;
                        break;
                }


            }

            return animated;
        }

        private bool UpdateNode(AnimatedNode node, bool bForce = false, bool bUpdateChildren = false, bool bUpdateSiblings = false)
        {
            //TODO check for IK-animated nodes and skip 'em
            if (bForce && !node.IKanimatedNode)
            {
                node.AnimationProcessingDone = false;

                //Reset flags
                node.AnimationProcessingDone = false;
                node.AnimatedAbsoluteRotationFlag = false;
                node.AnimatedAbsoluteScaleFlag = false;
                node.AnimatedAbsoluteTranslationFlag = false;
                //Reset transformation
                node.AnimatedRotation = node.Rotation;
                node.AnimatedScale = node.Scale;
                node.AnimatedTranslation = node.Translation;

                Quaternion quaternion = node.RotationQuaternion;
                Vector3 trans = node.Translation;
                node.AnimatedRotationQuaternion = quaternion;
                node.AnimatedMatrix = node.TransformMatrix;
                //Calculate interpolated values, if the node is animated
                if (getTransform(_currentAnimationIndex, _currentFrame, node.ID, 0, out trans, out bool absoluteTranslation))
                {
                    node.AnimatedAbsoluteTranslationFlag = absoluteTranslation;
                    node.AnimatedTranslation = trans;

                }
                if (getTransform(_currentAnimationIndex, _currentFrame, node.ID, 1, out Vector3 rotation_d, out bool absoluteRotation))
                {
                    //aNodes_array[i].animatedRotation = new Vector3((float)(rotation_d.X * Math.PI / 180d), (float)(rotation_d.Y * Math.PI / 180d), (float)(rotation_d.Z * Math.PI / 180d));
                    node.AnimatedAbsoluteRotationFlag = absoluteRotation;
                    node.AnimatedRotation = new Vector3(rotation_d.X, rotation_d.Y, rotation_d.Z);
                    EMAProcessorUtils.EulerToQuaternionXYZ((float)(rotation_d.Y * Math.PI / 180d), (float)(rotation_d.Z * Math.PI / 180d), (float)(rotation_d.X * Math.PI / 180d), out quaternion);
                    node.AnimatedRotationQuaternion = quaternion;
                }
                if (getTransform(_currentAnimationIndex, _currentFrame, node.ID, 2, out Vector3 scl, out bool absoluteScale))
                {
                    node.AnimatedAbsoluteScaleFlag = absoluteScale;
                    node.AnimatedScale = scl;
                }

                node.AnimatedMatrix = Matrix4x4.CreateScale(scl) * Matrix4x4.CreateFromQuaternion(quaternion) * Matrix4x4.CreateTranslation(trans);

            }

            if (node.AnimationProcessingDone)
            {
                if (bUpdateSiblings && node.Sibling != string.Empty)// && aNodes_array[nodeNumber].Sibling != -1)
                {

                    UpdateNode(AnimatedNodes[AnimatedSkeleton.NodeNames.IndexOf(node.Sibling)], true, bUpdateChildren, true);
                }

                if (bUpdateChildren && node.Child1 != string.Empty)// && aNodes_array[nodeNumber].Child1 != -1)
                {
                    UpdateNode(AnimatedNodes[AnimatedSkeleton.NodeNames.IndexOf(node.Child1)], true, true, true);
                }
                return true;
            }


            Matrix4x4 matrix = node.AnimatedMatrix;
            Vector3 translation = node.AnimatedTranslation;
            Vector3 scale = node.AnimatedScale;
            Quaternion rotation = node.AnimatedRotationQuaternion;

            if (node.Parent != string.Empty)
            {
                AnimatedNode parent = AnimatedNodes[AnimatedSkeleton.NodeNames.IndexOf(node.Parent)];

                if (!parent.AnimationProcessingDone)
                {
                    if (!UpdateNode(parent))
                        return false;
                }

                Matrix4x4 parentMatrix = parent.AnimatedMatrix;

                if (!node.AnimatedAbsoluteTranslationFlag)
                {
                    translation = Vector3.Transform(translation, parent.AnimatedMatrix);
                }

                if (!node.AnimatedAbsoluteRotationFlag)
                {
                    rotation = parent.AnimatedRotationQuaternion * rotation;
                }

                if (!node.AnimatedAbsoluteScaleFlag)
                {
                    scale.X *= parent.AnimatedScale.X;
                    scale.Y *= parent.AnimatedScale.Y;
                    scale.Z *= parent.AnimatedScale.Z;
                }

                EMAProcessorUtils.ComposeMatrixQuat(rotation, scale, translation, out matrix);

                node.AnimatedScale = scale;
                node.AnimatedTranslation = translation;
                node.AnimatedRotationQuaternion = rotation;
                node.AnimatedMatrix = matrix;

                if (node.Parent != string.Empty)
                {
                    Matrix4x4.Invert(parent.AnimatedMatrix, out Matrix4x4 parentInverseMatrix);
                    node.AnimatedLocalMatrix = matrix * parentInverseMatrix;
                }

                node.AnimationProcessingDone = true;
            }

            if (bUpdateSiblings && !string.IsNullOrEmpty(node.Sibling))
            {
                UpdateNode(AnimatedNodes[AnimatedSkeleton.NodeNames.IndexOf(node.Sibling)], true, bUpdateChildren, true);
            }

            if (bUpdateChildren && !string.IsNullOrEmpty(node.Child1))
            {
                UpdateNode(AnimatedNodes[AnimatedSkeleton.NodeNames.IndexOf(node.Child1)], true, true, true);
            }

            return true;
        }

        static Vector3 lerpVector3(Vector3 in1, Vector3 in2, float f_scale)
        {
            return in1 + (in2 - in1) * f_scale;
        }
        static Quaternion lerpQuaternion(Quaternion in1, Quaternion in2, float f_scale)
        {
            return in1 + (in2 - in1) * f_scale;
        }

        static Vector4 var_xmm0;
        static Vector4 var_xmm1;
        static Vector4 var_xmm2;
        static Vector4 var_xmm3;
        static Vector4 var_xmm4;

        static Matrix4x4 MatrixCreate(Vector4 pV1, Vector4 pV2, Vector4 pV3, Vector4 pV4)
        {
            Matrix4x4 pM = Matrix4x4.Identity;
            //
            pM.M11 = pV1.X;
            pM.M12 = pV1.Y;
            pM.M13 = pV1.Z;
            pM.M14 = pV1.W;

            //
            pM.M21 = pV2.X;
            pM.M22 = pV2.Y;
            pM.M23 = pV2.Z;
            pM.M24 = pV2.W;

            //
            pM.M31 = pV3.X;
            pM.M32 = pV3.Y;
            pM.M33 = pV3.Z;
            pM.M34 = pV3.W;

            //
            pM.M41 = pV4.X;
            pM.M42 = pV4.Y;
            pM.M43 = pV4.Z;
            pM.M44 = pV4.W;

            return pM;
        }

        static Matrix4x4 sub_504330(Matrix4x4 pM_ECX, Vector4 pV1_ESI, Vector4 pV2)
        {
            Vector4 var_60;
            Vector4 var_50 = new(0, 0, 0, 0);
            Vector4 var_40;
            Vector4 var_30;
            Vector4 var_20;
            Vector4 var_10;

            var_60 = new Vector4(pV1_ESI.Y, pV1_ESI.Z, pV1_ESI.X, 0);

            var_30 = new Vector4(pV1_ESI.Z, pV1_ESI.X, pV1_ESI.Y, 0);

            var_50.Z = 1f - pV2.Y;

            var_10 = new Vector4(pV2.X, pV2.X, pV2.X, pV2.X);

            var_20 = new Vector4(pV2.Y, pV2.Y, pV2.Y, pV2.Y);

            var_50 = new Vector4(var_50.Z, var_50.Z, var_50.Z, var_50.Z);

            var_40 = new Vector4(pV2.X, pV2.X, pV2.X, pV2.X);

            var_xmm0 = Vector4.Multiply(var_50, var_60);

            var_xmm2 = Vector4.Multiply(var_50, pV1_ESI);

            var_xmm0 = Vector4.Multiply(var_xmm0, var_30);

            var_xmm1 = Vector4.Multiply(pV1_ESI, var_xmm2);

            var_xmm1 = Vector4.Add(var_xmm1, var_20);

            var_50 = var_xmm1;
            var_30 = var_xmm0;

            var_xmm1 = Vector4.Multiply(pV1_ESI, var_40);
            var_xmm1 = Vector4.Add(var_xmm1, var_xmm0);

            var_40 = var_xmm1;

            var_xmm0 = Vector4.Multiply(pV1_ESI, var_10);

            var_60 = Vector4.Subtract(var_xmm0, var_30);

            var_60 = -var_60;

            pM_ECX.M11 = var_50.X;
            pM_ECX.M12 = var_40.Z;
            pM_ECX.M13 = var_60.Y;
            pM_ECX.M13 = 0f;

            pM_ECX.M21 = var_60.Z;
            pM_ECX.M22 = var_50.Y;
            pM_ECX.M23 = var_40.X;
            pM_ECX.M24 = 0f;

            pM_ECX.M31 = var_40.Y;
            pM_ECX.M32 = var_60.X;
            pM_ECX.M33 = var_50.Z;
            pM_ECX.M34 = 0f;

            pM_ECX.M41 = 0f;
            pM_ECX.M42 = 0f;
            pM_ECX.M43 = 0f;
            pM_ECX.M44 = 1f;

            return pM_ECX;
        }

        private void ProcessIKData0x00_00(IKDataBlock ik)
        {

        }

        private void ProcessIKData0x00_02(IKDataBlock ik)
        {

            AnimatedNode node0 = AnimatedNodes[ik.BoneIDs[0]];
            AnimatedNode node1 = AnimatedNodes[ik.BoneIDs[1]];
            AnimatedNode node2 = AnimatedNodes[ik.BoneIDs[2]];
            AnimatedNode node3 = AnimatedNodes[ik.BoneIDs[3]];
            AnimatedNode node4 = AnimatedNodes[ik.BoneIDs[4]];
            AnimatedNode nodeP = AnimatedNodes[Ema.Skeleton.NodeNames.IndexOf(node0.Parent)];

            node1.IKanimatedNode = true;
            node2.IKanimatedNode = true;
            node3.IKanimatedNode = true;

            // sub_524390
            if (1 == 1)
            {
                Vector4 var_F0 = new Vector4(0, 0, 0, 0);
                Vector4 var_D0 = new Vector4(0, 0, 0, 0);
                Vector4 var_C0 = new Vector4(0, 0, 0, 0);
                Vector4 var_B0 = new Vector4(0, 0, 0, 0);
                Vector4 var_A0 = new Vector4(0, 0, 0, 1);

                Vector4 var_140 = new Vector4(0, 0, 0, 0);
                Vector4 var_120 = new Vector4(0, 0, 0, 0);
                Vector4 var_100 = new Vector4(0, 0, 0, 0);
                Vector4 var_90 = new Vector4(0, 0, 0, 0);

                // update bone index unknown0x08
                UpdateNode(node1);
                // update bone index unknown0x0A
                UpdateNode(node2);

                var_100 = new Vector4(node0.AnimatedTranslation, 1); // position unknown0x06
                var_120 = new Vector4(node3.AnimatedTranslation, 1); // position unknown0x0C
                var_140 = new Vector4(node4.AnimatedTranslation, 1); // position unknown0x0E

                var_120 = Vector4.Subtract(var_120, var_100);
                var_140 = Vector4.Subtract(var_140, var_100);

                Vector3 v3_140 = new Vector3(var_140.X, var_140.Y, var_140.Z);
                Vector3 v3_120 = new Vector3(var_120.X, var_120.Y, var_120.Z);

                Vector3 v3_F0 = Vector3.Cross(v3_140, v3_120);
                v3_140 = Vector3.Cross(v3_F0, v3_120);

                var_F0.X = v3_F0.X;
                var_F0.Y = v3_F0.Y;
                var_F0.Z = v3_F0.Z;

                var_140.X = v3_140.X;
                var_140.Y = v3_140.Y;
                var_140.Z = v3_140.Z;

                if ((ik.Flag0x01 & 0x01) == 0x01)
                {
                    var_F0 = -var_F0;
                    var_140 = -var_140;
                }

                var_D0 = Vector4.Normalize(var_120);
                var_C0 = Vector4.Normalize(var_140);
                var_B0 = Vector4.Normalize(var_F0);

                var_A0 = var_100;

                var_xmm4 = new Vector4(var_120.Length(), 0, 0, 0);

                Vector4 scale0x08 = new Vector4(node1.AnimatedScale, 1);
                Vector4 scale0x0A = new Vector4(node2.AnimatedScale, 1);

                float xScale0x08 = scale0x08.X;
                float unkScale0x08 = node1.BoneLength;
                float xScale0x0A = scale0x0A.X;
                float unkScale0x0A = node2.BoneLength;

                var_xmm1 = new Vector4(xScale0x08 * unkScale0x08, 0, 0, 0);

                var_xmm3 = new Vector4(var_xmm4.X, var_xmm1.X, 0, 0);

                var_xmm2 = new Vector4(xScale0x0A * unkScale0x0A, 0, 0, 0);

                float var_104 = var_xmm1.X;

                var_xmm1 = new Vector4(var_xmm1.X, var_xmm2.X, 0, 0);

                var_xmm2 = new Vector4(var_xmm2.X, var_xmm4.X, 0, 0);

                var_100 = var_xmm1;

                var_xmm4 = var_xmm3;

                var_xmm1 = Vector4.Multiply(var_xmm1, var_xmm1);

                var_xmm4 = Vector4.Multiply(var_xmm4, var_xmm3);

                var_90 = var_xmm3;

                var_xmm2 = Vector4.Multiply(var_xmm2, var_xmm2);

                var_xmm4 = Vector4.Add(var_xmm4, var_xmm1);

                var_xmm1 = new Vector4(0.5f, 0, 0, 0);

                var_xmm3 = new Vector4(var_100.Y, 0, 0, 0);
                var_xmm3.X *= var_90.Y;

                var_xmm4 = Vector4.Subtract(var_xmm4, var_xmm2);

                var_xmm2 = new Vector4(var_100.X, 0, 0, 0);
                var_xmm2.X *= var_90.X;

                var_xmm1 = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);

                var_xmm4 = Vector4.Multiply(var_xmm4, var_xmm1);

                var_120 = var_xmm4;

                var_xmm1 = new Vector4(var_120.X, 0, 0, 0);

                var_xmm4 = new Vector4(1f, 0, 0, 0);

                var_xmm1.X /= var_xmm2.X;

                var_xmm2 = new Vector4(var_120.Y, 0, 0, 0);

                var_xmm2.X /= var_xmm3.X;

                var_xmm3 = new Vector4(-1f, 0, 0, 0);

                var_120.X = var_xmm1.X;

                var_120.Y = var_xmm2.X;

                if (var_xmm3.X > var_xmm1.X)
                {
                    var_120.X = var_xmm3.X;
                }
                else if (var_xmm4.X < var_xmm1.X)
                {
                    var_120.X = var_xmm4.X;
                }

                if (var_xmm3.X > var_xmm2.X)
                {
                    var_120.Y = var_xmm3.X;
                }
                else if (var_xmm4.X < var_xmm2.X)
                {
                    var_120.Y = var_xmm4.X;
                }

                var_xmm1 = var_120;

                var_xmm0 = new Vector4(1f, 1f, 0, 0); // D3DXVECTOR4(var_xmm4.x, var_xmm4.x);

                var_xmm1 = Vector4.Multiply(var_xmm1, var_xmm1);

                var_xmm0 = Vector4.Subtract(var_xmm0, var_xmm1);

                var_140 = new Vector4((float)Math.Sqrt(var_xmm0.X), (float)Math.Sqrt(var_xmm0.Y), 0, 0);

                var_xmm0 = new Vector4(0, 0, 0, 0);

                var_F0 = new Vector4(0, 0, -1f, 0);

                Matrix4x4 var_80 = Matrix4x4.Identity, var_40 = Matrix4x4.Identity;

                //LINE 690
                var_80 = sub_504330(var_80, var_F0, new Vector4(var_140.X, var_120.X, 0f, 0f));
                var_40 = sub_504330(var_40, var_F0, new Vector4(-var_140.Y, -var_120.Y, 0f, 0f));

                Matrix4x4 mat_D0 = MatrixCreate(var_D0, var_C0, var_B0, var_A0);

                var_80 = Matrix4x4.Multiply(var_80, mat_D0);

                // bone matrix = bone scale matrix * mat_80
                Matrix4x4 mat_bone0x08 = Matrix4x4.CreateScale(scale0x08.X, scale0x08.Y, scale0x08.Z);
                mat_bone0x08 = Matrix4x4.Multiply(mat_bone0x08, var_80);

                mat_D0 = var_80;

                var_100 = new Vector4(var_104, 0, 0, 1f);

                // mat_D0 position = bone matrix position
                mat_D0.M41 = mat_bone0x08.M41;
                mat_D0.M42 = mat_bone0x08.M42;
                mat_D0.M43 = mat_bone0x08.M43;
                mat_D0.M44 = mat_bone0x08.M44;

                var_100 = Vector4.Transform(var_100, mat_D0);

                mat_D0.M41 = var_100.X;
                mat_D0.M42 = var_100.Y;
                mat_D0.M43 = var_100.Z;
                mat_D0.M44 = var_100.W;

                var_40 = Matrix4x4.Multiply(var_40, mat_D0);

                // bone matrix = bone scale matrix * mat_80
                Matrix4x4 mat_bone0x0A = Matrix4x4.CreateScale(scale0x0A.X, scale0x0A.Y, scale0x0A.Z);
                mat_bone0x0A = Matrix4x4.Multiply(mat_bone0x0A, var_40);

                var_100 = new Vector4(unkScale0x0A, 0, 0, 1f);

                var_100 = Vector4.Transform(var_100, mat_bone0x0A);

                // update position and quaternion information for bone index 0x08
                Quaternion rotation = Quaternion.CreateFromRotationMatrix(var_80);
                EMAProcessorUtils.LeftHandToEulerAnglesXYZ(var_80, out Vector3 eulerRotation1);
                node1.AnimatedRotation = eulerRotation1;

                node1.AnimatedTranslation = mat_bone0x08.Translation;
                node1.AnimatedRotationQuaternion = rotation;
                node1.AnimatedMatrix = mat_bone0x08;

                // update position and quaternion information for bone index 0x0A
                rotation = Quaternion.CreateFromRotationMatrix(var_40);
                EMAProcessorUtils.LeftHandToEulerAnglesXYZ(var_40, out Vector3 eulerRotation2);
                node2.AnimatedRotation = eulerRotation2;

                node2.AnimatedTranslation = mat_bone0x0A.Translation;
                node2.AnimatedRotationQuaternion = rotation;
                node2.AnimatedMatrix = mat_bone0x0A;

                // set flags for absolute translation and rotation
                node2.AnimationProcessingDone = true;
                node2.AnimatedAbsoluteRotationFlag = true;
                node2.AnimatedAbsoluteTranslationFlag = true;

                // update position for bone index 0x0C
                node3.AnimatedTranslation = new Vector3(var_100.X, var_100.Y, var_100.Z);
                Matrix4x4 m = node3.AnimatedMatrix;
                m.Translation = new Vector3(var_100.X, var_100.Y, var_100.Z);
                node3.AnimatedMatrix = m;

                // set flags for absolute translation
                node2.AnimatedAbsoluteTranslationFlag = true;

                //UpdateNode(aNodes_array[ema.Skeleton.NodeNames.IndexOf(node1.Child1)], true, true, false);
                UpdateNode(AnimatedNodes[Ema.Skeleton.NodeNames.IndexOf(node2.Child1)], true, true, true);

            }
        }

        private void ProcessIKData0x01_00(IKDataBlock ik)
        {
            // sub_525270
            {
                AnimatedNode node0 = AnimatedNodes[ik.BoneIDs[0]];
                AnimatedNode node1 = AnimatedNodes[ik.BoneIDs[1]];
                AnimatedNode node2 = AnimatedNodes[ik.BoneIDs[2]];

                //node0.IKanimatedNode = true;

                AnimatedNode nodeP = new AnimatedNode();
                if (node0.Parent != string.Empty)
                {
                    nodeP = AnimatedNodes[AnimatedSkeleton.NodeNames.IndexOf(node0.Parent)];
                }

                //if (node0.NodeMatrix == null || node1.NodeMatrix == null || node2.NodeMatrix == null || nodeP.NodeMatrix == null)
                //{
                //    return;
                //}

                UpdateNode(node1);
                UpdateNode(node2);
                UpdateNode(nodeP);

                Vector3 translation;
                // if flags & 0x01
                if ((ik.Flag0x01 & 0x01) == 0x01)
                {
                    // - boneTransform0x06.position = lerp(boneTransform0x08.position, boneTransform0x0A.position, float0x0C);
                    translation = lerpVector3(node1.AnimatedTranslation, node2.AnimatedTranslation, ik.IKFloats[0]);
                }
                else
                {
                    //translation = Vector3.Transform(node0.Translation, nodeP.animatedMatrix);
                    translation = node0.AnimatedTranslation;
                }
                Quaternion rotation;
                // if flags & 0x02
                if ((ik.Flag0x01 & 0x02) == 0x02)
                {
                    rotation = lerpQuaternion(node1.AnimatedRotationQuaternion, node2.AnimatedRotationQuaternion, ik.IKFloats[1]);
                }
                else
                {
                    //rotation = node0.animatedRotationQuaternion * nodeP.animatedRotationQuaternion;
                    rotation = node0.AnimatedRotationQuaternion;
                }
                Vector3 scale;
                //if flags & 0x04
                if ((ik.Flag0x01 & 0x04) == 0x04)
                {
                    scale = lerpVector3(node1.AnimatedScale, node2.AnimatedScale, ik.IKFloats[2]);
                }
                else
                {
                    scale = node0.AnimatedScale;
                }

                EMAProcessorUtils.ComposeMatrixQuat(rotation, scale, translation, out Matrix4x4 matrix);

                EMAProcessorUtils.LeftHandToEulerAnglesXYZ(matrix, out Vector3 eulerAngles);

                AnimatedNodes[node0.ID].AnimatedScale = scale;
                AnimatedNodes[node0.ID].AnimatedTranslation = translation;
                AnimatedNodes[node0.ID].AnimatedRotationQuaternion = rotation;
                AnimatedNodes[node0.ID].AnimatedMatrix = matrix;

                AnimatedNodes[node0.ID].AnimationProcessingDone = true;
                AnimatedNodes[node0.ID].AnimatedAbsoluteRotationFlag = true;
                AnimatedNodes[node0.ID].AnimatedAbsoluteTranslationFlag = true;
                AnimatedNodes[node0.ID].AnimatedAbsoluteScaleFlag = true;

                if (node0.Parent != string.Empty)
                {
                    Matrix4x4.Invert(AnimatedNodes[AnimatedSkeleton.NodeNames.IndexOf(node0.Parent)].AnimatedMatrix, out Matrix4x4 parentInverseMatrix);
                    node0.AnimatedLocalMatrix = matrix * parentInverseMatrix;
                }

                int node0Child = Ema.Skeleton.NodeNames.IndexOf(node0.Child1);
                if (node0Child >= 0 && node0Child < AnimatedNodes.Count())
                {
                    UpdateNode(AnimatedNodes[node0Child], true, true, true);
                }
                int node1Child = Ema.Skeleton.NodeNames.IndexOf(node1.Child1);
                if (node1Child >= 0 && node1Child < AnimatedNodes.Count())
                {
                    UpdateNode(AnimatedNodes[Ema.Skeleton.NodeNames.IndexOf(node1.Child1)], true, true, true);
                }
                int node2Child = Ema.Skeleton.NodeNames.IndexOf(node2.Child1);
                if (node2Child >= 0 && node2Child < AnimatedNodes.Count())
                {
                    UpdateNode(AnimatedNodes[Ema.Skeleton.NodeNames.IndexOf(node2.Child1)], true, true, true);
                }
            }
        }

        private bool updateIKData(int ikNumber)
        {
            bool bResult = false;

            AnimatedSkeleton s = AnimatedSkeleton;

            IKDataBlock ik = s.IKDataBlocks[ikNumber];

            switch (ik.Method)
            {
                case 0x00:
                    {
                        switch (ik.Flag0x00)
                        {
                            case 0x00:
                                {
                                    ProcessIKData0x00_00(ik);
                                    Debug.Print("Attempted to process 0x00_00 data");
                                }
                                break;
                            case 0x02:
                                {
                                    ProcessIKData0x00_02(ik);
                                    bResult = true;
                                }
                                break;
                        }
                    }
                    break;
                case 0x01:
                    {
                        ProcessIKData0x01_00(ik);
                        bResult = true;
                    }
                    break;
            }

            return bResult;
        }
    }
}
