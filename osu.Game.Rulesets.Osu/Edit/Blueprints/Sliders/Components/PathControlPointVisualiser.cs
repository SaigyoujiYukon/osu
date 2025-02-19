// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class PathControlPointVisualiser : CompositeDrawable, IKeyBindingHandler<PlatformAction>, IHasContextMenu
    {
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true; // allow context menu to appear outside of the playfield.

        internal readonly Container<PathControlPointPiece> Pieces;
        internal readonly Container<PathControlPointConnectionPiece> Connections;

        private readonly IBindableList<PathControlPoint> controlPoints = new BindableList<PathControlPoint>();
        private readonly Slider slider;
        private readonly bool allowSelection;

        private InputManager inputManager;

        public Action<List<PathControlPoint>> RemoveControlPointsRequested;

        public PathControlPointVisualiser(Slider slider, bool allowSelection)
        {
            this.slider = slider;
            this.allowSelection = allowSelection;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                Connections = new Container<PathControlPointConnectionPiece> { RelativeSizeAxes = Axes.Both },
                Pieces = new Container<PathControlPointPiece> { RelativeSizeAxes = Axes.Both }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();

            controlPoints.CollectionChanged += onControlPointsChanged;
            controlPoints.BindTo(slider.Path.ControlPoints);
        }

        private void onControlPointsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // If inserting in the path (not appending),
                    // update indices of existing connections after insert location
                    if (e.NewStartingIndex < Pieces.Count)
                    {
                        foreach (var connection in Connections)
                        {
                            if (connection.ControlPointIndex >= e.NewStartingIndex)
                                connection.ControlPointIndex += e.NewItems.Count;
                        }
                    }

                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var point = (PathControlPoint)e.NewItems[i];

                        Pieces.Add(new PathControlPointPiece(slider, point).With(d =>
                        {
                            if (allowSelection)
                                d.RequestSelection = selectPiece;
                        }));

                        Connections.Add(new PathControlPointConnectionPiece(slider, e.NewStartingIndex + i));
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var point in e.OldItems.Cast<PathControlPoint>())
                    {
                        Pieces.RemoveAll(p => p.ControlPoint == point);
                        Connections.RemoveAll(c => c.ControlPoint == point);
                    }

                    // If removing before the end of the path,
                    // update indices of connections after remove location
                    if (e.OldStartingIndex < Pieces.Count)
                    {
                        foreach (var connection in Connections)
                        {
                            if (connection.ControlPointIndex >= e.OldStartingIndex)
                                connection.ControlPointIndex -= e.OldItems.Count;
                        }
                    }

                    break;
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            foreach (var piece in Pieces)
            {
                piece.IsSelected.Value = false;
            }

            return false;
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            switch (e.Action)
            {
                case PlatformAction.Delete:
                    return DeleteSelected();
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }

        private void selectPiece(PathControlPointPiece piece, MouseButtonEvent e)
        {
            if (e.Button == MouseButton.Left && inputManager.CurrentState.Keyboard.ControlPressed)
                piece.IsSelected.Toggle();
            else
            {
                foreach (var p in Pieces)
                    p.IsSelected.Value = p == piece;
            }
        }

        /// <summary>
        /// Attempts to set the given control point piece to the given path type.
        /// If that would fail, try to change the path such that it instead succeeds
        /// in a UX-friendly way.
        /// </summary>
        /// <param name="piece">The control point piece that we want to change the path type of.</param>
        /// <param name="type">The path type we want to assign to the given control point piece.</param>
        private void updatePathType(PathControlPointPiece piece, PathType? type)
        {
            int indexInSegment = piece.PointsInSegment.IndexOf(piece.ControlPoint);

            switch (type)
            {
                case PathType.PerfectCurve:
                    // Can't always create a circular arc out of 4 or more points,
                    // so we split the segment into one 3-point circular arc segment
                    // and one segment of the previous type.
                    int thirdPointIndex = indexInSegment + 2;

                    if (piece.PointsInSegment.Count > thirdPointIndex + 1)
                        piece.PointsInSegment[thirdPointIndex].Type = piece.PointsInSegment[0].Type;

                    break;
            }

            piece.ControlPoint.Type = type;
        }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

        public bool DeleteSelected()
        {
            List<PathControlPoint> toRemove = Pieces.Where(p => p.IsSelected.Value).Select(p => p.ControlPoint).ToList();

            // Ensure that there are any points to be deleted
            if (toRemove.Count == 0)
                return false;

            changeHandler?.BeginChange();
            RemoveControlPointsRequested?.Invoke(toRemove);
            changeHandler?.EndChange();

            // Since pieces are re-used, they will not point to the deleted control points while remaining selected
            foreach (var piece in Pieces)
                piece.IsSelected.Value = false;

            return true;
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (!Pieces.Any(p => p.IsHovered))
                    return null;

                var selectedPieces = Pieces.Where(p => p.IsSelected.Value).ToList();
                int count = selectedPieces.Count;

                if (count == 0)
                    return null;

                List<MenuItem> items = new List<MenuItem>();

                if (!selectedPieces.Contains(Pieces[0]))
                    items.Add(createMenuItemForPathType(null));

                // todo: hide/disable items which aren't valid for selected points
                items.Add(createMenuItemForPathType(PathType.Linear));
                items.Add(createMenuItemForPathType(PathType.PerfectCurve));
                items.Add(createMenuItemForPathType(PathType.Bezier));
                items.Add(createMenuItemForPathType(PathType.Catmull));

                string controlPointDeleteText = count > 1 ? $"{count}个滑条点" : "滑条点";
                return new MenuItem[]
                {
                    new OsuMenuItem($"删除{controlPointDeleteText}", MenuItemType.Destructive, () => DeleteSelected()),
                    new OsuMenuItem("曲线类型")
                    {
                        Items = items
                    }
                };
            }
        }

        private MenuItem createMenuItemForPathType(PathType? type)
        {
            int totalCount = Pieces.Count(p => p.IsSelected.Value);
            int countOfState = Pieces.Where(p => p.IsSelected.Value).Count(p => p.ControlPoint.Type == type);

            var item = new TernaryStateRadioMenuItem(type == null ? "继承" : type.ToString().Humanize(), MenuItemType.Standard, _ =>
            {
                foreach (var p in Pieces.Where(p => p.IsSelected.Value))
                    updatePathType(p, type);
            });

            if (countOfState == totalCount)
                item.State.Value = TernaryState.True;
            else if (countOfState > 0)
                item.State.Value = TernaryState.Indeterminate;
            else
                item.State.Value = TernaryState.False;

            return item;
        }
    }
}
