﻿using GL_EditorFramework.GL_Core;
using GL_EditorFramework.Interfaces;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Library.IO;
using static GL_EditorFramework.EditorDrawables.EditorSceneBase;
using FirstPlugin.Turbo.CourseMuuntStructs;
using FirstPlugin.Turbo;
using System.Collections.Generic;

namespace GL_EditorFramework.EditorDrawables
{
    public class RenderablePathPoint : EditableObject
    {
        public bool CanConnect = true;

        public object NodeObject;

        public bool IsNormalTanTransform = false;

        protected Vector3 position = new Vector3(0, 0, 0);
        protected Vector3 scale = new Vector3(1, 1, 1);
        protected Vector3 rotate = new Vector3(0, 0, 0);

        protected Vector3 Normal = new Vector3(0, 0, 0);
        protected Vector3 Tangent = new Vector3(0, 0, 0);

        protected bool Selected = false;

        public override bool IsSelected() => Selected;

        public override bool IsSelected(int partIndex) => Selected;

        public Vector4 Color = new Vector4(0f, 0.25f, 1f, 1f);

        public Vector3 Position;
        public Vector3 Scale;

        public RenderablePathPoint(Vector4 color, Vector3 pos, Vector3 rot, Vector3 sca, object nodeObject)
        {
            NodeObject = nodeObject;

            Color = color;
            UpdateTransform(pos, rot, sca);
        }

        public RenderablePathPoint(Vector3 pos, Vector3 normal, Vector3 tangent, Vector3 sca)
        {
            UpdateTransform(pos, normal, tangent, sca);
        }

        public void UpdateTransform(Vector3 pos, Vector3 rot, Vector3 sca)
        {
            position = pos;
            rotate = rot;
            scale = new Vector3(sca / 2);
        }

        public void UpdateTransform(Vector3 pos, Vector3 normal, Vector3 tangent, Vector3 scale)
        {
            IsNormalTanTransform = true;
            Normal = normal;
            Tangent = tangent;
            scale = new Vector3(scale / 2);
        }

        public override void Draw(GL_ControlModern control, Pass pass, EditorSceneBase editorScene)
        {
            if (pass == Pass.TRANSPARENT)
                return;

            bool hovered = editorScene.Hovered == this;

            if (IsNormalTanTransform)
            {
                control.UpdateModelMatrix(Matrix4.CreateScale(scale) *
                MatrixExenstion.CreateRotation(Normal, Tangent) *
                Matrix4.CreateTranslation(Selected ? editorScene.CurrentAction.NewPos(Position) : Position));
            }
            else
            {
                control.UpdateModelMatrix(Matrix4.CreateScale(scale) *
                (Matrix4.CreateRotationX(rotate.X) *
                Matrix4.CreateRotationY(rotate.Y) *
                Matrix4.CreateRotationZ(rotate.Z)) *
                Matrix4.CreateTranslation(Selected ? editorScene.CurrentAction.NewPos(Position) : Position));
            }

            Vector4 blockColor;
            Vector4 lineColor;

            if (hovered && Selected)
                lineColor = hoverColor;
            else if (hovered || Selected)
                lineColor = selectColor;
            else
                lineColor = Color;

            if (hovered && Selected)
                blockColor = Color * 0.5f + hoverColor * 0.5f;
            else if (hovered || Selected)
                blockColor = Color * 0.5f + selectColor * 0.5f;
            else
                blockColor = Color;

            Renderers.ColorBlockRenderer.Draw(control, pass, blockColor, lineColor, control.NextPickingColor());

        }

        public override void Draw(GL_ControlModern control, Pass pass)
        {
            if (pass == Pass.TRANSPARENT)
                return;

            control.UpdateModelMatrix(Matrix4.CreateScale(0.5f) *
                Matrix4.CreateTranslation(position));

            Renderers.ColorBlockRenderer.Draw(control, pass, Color, Color, control.NextPickingColor());

        }

        public override void Draw(GL_ControlLegacy control, Pass pass, EditorSceneBase editorScene)
        {
            if (pass == Pass.TRANSPARENT)
                return;

            bool hovered = editorScene.Hovered == this;

            control.UpdateModelMatrix(Matrix4.CreateScale(0.5f) *
                Matrix4.CreateTranslation(Selected ? editorScene.CurrentAction.NewPos(position) : position));

            Vector4 blockColor;
            Vector4 lineColor;

            if (hovered && Selected)
                lineColor = hoverColor;
            else if (hovered || Selected)
                lineColor = selectColor;
            else
                lineColor = Color;

            if (hovered && Selected)
                blockColor = Color * 0.5f + hoverColor * 0.5f;
            else if (hovered || Selected)
                blockColor = Color * 0.5f + selectColor * 0.5f;
            else
                blockColor = Color;

            Renderers.ColorBlockRenderer.Draw(control, pass, blockColor, lineColor, control.NextPickingColor());
        }

        public override void Draw(GL_ControlLegacy control, Pass pass)
        {
            if (pass == Pass.TRANSPARENT)
                return;

            control.UpdateModelMatrix(Matrix4.CreateScale(0.5f) *
                Matrix4.CreateTranslation(position));

            Renderers.ColorBlockRenderer.Draw(control, pass, Color, Color, control.NextPickingColor());

        }

        public override void Prepare(GL_ControlModern control)
        {
            Renderers.ColorBlockRenderer.Initialize(control);
        }

        public override void Prepare(GL_ControlLegacy control)
        {

        }

        public virtual void Translate(Vector3 lastPos, Vector3 translate, int subObj)
        {
            position = lastPos + translate;
        }

        public virtual void UpdatePosition(int subObj)
        {
        }

        public override void GetSelectionBox(ref BoundingBox boundingBox)
        {

        }


        public override uint SelectAll(GL_ControlBase control, ISet<object> selectedObjects)
        {
            Selected = true;
            return REDRAW;
        }

        public override uint SelectDefault(GL_ControlBase control, ISet<object> selectedObjects)
        {
            Selected = true;
            return REDRAW;
        }

        public override uint Select(int partIndex, GL_ControlBase control, ISet<object> selectedObjects)
        {
            Selected = true;
            return REDRAW;
        }

        public override uint Deselect(int partIndex, GL_ControlBase control, ISet<object> selectedObjects)
        {
            Selected = false;
            return REDRAW;
        }

        public override uint DeselectAll(GL_ControlBase control, ISet<object> selectedObjects)
        {
            Selected = false;
            return REDRAW;
        }

        public override LocalOrientation GetLocalOrientation(int partIndex)
        {
            return new LocalOrientation(position);
        }

        public override bool TryStartDragging(DragActionType actionType, int hoveredPart, out LocalOrientation localOrientation, out bool dragExclusively)
        {
            localOrientation = new LocalOrientation(position);
            dragExclusively = false;
            return Selected;
        }

        public override bool IsInRange(float range, float rangeSquared, Vector3 pos)
        {
            return true;
        }

        private void UpdateNodePosition()
        {
            if (NodeObject is BasePathPoint)
            {
                ((BasePathPoint)NodeObject).Translate = position;
                ((BasePathPoint)NodeObject).Scale = scale;
                ((BasePathPoint)NodeObject).Rotate = rotate;

                if (((BasePathPoint)NodeObject).OnPathMoved != null)
                    ((BasePathPoint)NodeObject).OnPathMoved();
            }
            if (NodeObject is Course_MapCamera_bin)
            {
                if (Color == new Vector4(1, 0, 0, 1))
                {
                    ((Course_MapCamera_bin)NodeObject).cameraData.PositionX = position.X;
                    ((Course_MapCamera_bin)NodeObject).cameraData.PositionY = position.Y;
                    ((Course_MapCamera_bin)NodeObject).cameraData.PositionZ = position.Z;
                }
                else
                {
                    ((Course_MapCamera_bin)NodeObject).cameraData.TargetX = position.X;
                    ((Course_MapCamera_bin)NodeObject).cameraData.TargetY = position.Y;
                    ((Course_MapCamera_bin)NodeObject).cameraData.TargetZ = position.Z;
                }
            }
        }
    }
}
