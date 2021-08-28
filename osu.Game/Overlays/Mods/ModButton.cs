﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Utils;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// Represents a clickable button which can cycle through one of more mods.
    /// </summary>
    public class ModButton : ModButtonEmpty, IHasCustomTooltip
    {
        private ModIcon foregroundIcon;
        private ModIcon backgroundIcon;
        private readonly SpriteText text;
        private readonly Container<ModIcon> iconsContainer;
        private readonly CompositeDrawable incompatibleIcon;

        /// <summary>
        /// Fired when the selection changes.
        /// </summary>
        public Action<Mod> SelectionChanged;

        public LocalisableString TooltipText => (SelectedMod?.Description ?? Mods.FirstOrDefault()?.Description) ?? string.Empty;

        private const Easing mod_switch_easing = Easing.InOutSine;
        private const double mod_switch_duration = 120;

        // A selected index of -1 means not selected.
        private int selectedIndex = -1;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> selectedMods { get; set; }

        /// <summary>
        /// Change the selected mod index of this button.
        /// </summary>
        /// <param name="newIndex">The new index.</param>
        /// <param name="resetSettings">Whether any settings applied to the mod should be reset on selection.</param>
        /// <returns>Whether the selection changed.</returns>
        private bool changeSelectedIndex(int newIndex, bool resetSettings = true)
        {
            if (newIndex == selectedIndex) return false;

            int direction = newIndex < selectedIndex ? -1 : 1;

            bool beforeSelected = Selected;

            Mod previousSelection = SelectedMod ?? Mods[0];

            if (newIndex >= Mods.Length)
                newIndex = -1;
            else if (newIndex < -1)
                newIndex = Mods.Length - 1;

            if (newIndex >= 0 && !Mods[newIndex].HasImplementation)
                return false;

            selectedIndex = newIndex;

            Mod newSelection = SelectedMod ?? Mods[0];

            if (resetSettings)
                newSelection.ResetSettingsToDefaults();

            Schedule(() =>
            {
                if (beforeSelected != Selected)
                {
                    iconsContainer.RotateTo(Selected ? 5f : 0f, 300, Easing.OutElastic);
                    iconsContainer.ScaleTo(Selected ? 1.1f : 1f, 300, Easing.OutElastic);
                }

                if (previousSelection != newSelection)
                {
                    const float rotate_angle = 16;

                    foregroundIcon.RotateTo(rotate_angle * direction, mod_switch_duration, mod_switch_easing);
                    backgroundIcon.RotateTo(-rotate_angle * direction, mod_switch_duration, mod_switch_easing);

                    backgroundIcon.Mod = newSelection;

                    using (BeginDelayedSequence(mod_switch_duration))
                    {
                        foregroundIcon
                            .RotateTo(-rotate_angle * direction)
                            .RotateTo(0f, mod_switch_duration, mod_switch_easing);

                        backgroundIcon
                            .RotateTo(rotate_angle * direction)
                            .RotateTo(0f, mod_switch_duration, mod_switch_easing);

                        Schedule(() => displayMod(newSelection));
                    }
                }

                foregroundIcon.Selected.Value = Selected;
            });

            SelectionChanged?.Invoke(SelectedMod);

            return true;
        }

        public bool Selected => selectedIndex != -1;

        private Color4 selectedColour;

        public Color4 SelectedColour
        {
            get => selectedColour;
            set
            {
                if (value == selectedColour) return;

                selectedColour = value;
                if (Selected) foregroundIcon.Colour = value;
            }
        }

        private Mod mod;
        private readonly Container scaleContainer;

        public Mod Mod
        {
            get => mod;
            set
            {
                mod = value;

                if (mod == null)
                {
                    Mods = Array.Empty<Mod>();
                    Alpha = 0;
                }
                else
                {
                    Mods = (mod as MultiMod)?.Mods ?? new[] { mod };
                    Alpha = 1;
                }

                createIcons();

                if (Mods.Length > 0)
                {
                    displayMod(Mods[0]);
                }
            }
        }

        public Mod[] Mods { get; private set; }

        public virtual Mod SelectedMod => Mods.ElementAtOrDefault(selectedIndex);

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            scaleContainer.ScaleTo(0.9f, 800, Easing.Out);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            scaleContainer.ScaleTo(1, 500, Easing.OutElastic);

            // only trigger the event if we are inside the area of the button
            if (Contains(e.ScreenSpaceMousePosition))
            {
                switch (e.Button)
                {
                    case MouseButton.Right:
                        SelectNext(-1);
                        break;
                }
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            SelectNext(1);

            return true;
        }

        /// <summary>
        /// Select the next available mod in a specified direction.
        /// </summary>
        /// <param name="direction">1 for forwards, -1 for backwards.</param>
        public void SelectNext(int direction)
        {
            int start = selectedIndex + direction;
            // wrap around if we are at an extremity.
            if (start >= Mods.Length)
                start = -1;
            else if (start < -1)
                start = Mods.Length - 1;

            for (int i = start; i < Mods.Length && i >= 0; i += direction)
            {
                if (SelectAt(i))
                    return;
            }

            Deselect();
        }

        /// <summary>
        /// Select the mod at the provided index.
        /// </summary>
        /// <param name="index">The index to select.</param>
        /// <param name="resetSettings">Whether any settings applied to the mod should be reset on selection.</param>
        /// <returns>Whether the selection changed.</returns>
        public bool SelectAt(int index, bool resetSettings = true)
        {
            if (!Mods[index].HasImplementation) return false;

            changeSelectedIndex(index, resetSettings);
            return true;
        }

        public void Deselect() => changeSelectedIndex(-1);

        private void displayMod(Mod mod)
        {
            if (backgroundIcon != null)
                backgroundIcon.Mod = foregroundIcon.Mod;
            foregroundIcon.Mod = mod;
            text.Text = mod.Name;
            Colour = mod.HasImplementation ? Color4.White : Color4.Gray;

            Scheduler.AddOnce(updateCompatibility);
        }

        private void updateCompatibility()
        {
            var m = SelectedMod ?? Mods.First();

            bool isIncompatible = false;

            if (selectedMods.Value.Count > 0 && !selectedMods.Value.Contains(m))
                isIncompatible = !ModUtils.CheckCompatibleSet(selectedMods.Value.Append(m));

            if (isIncompatible)
                incompatibleIcon.Show();
            else
                incompatibleIcon.Hide();
        }

        private void createIcons()
        {
            iconsContainer.Clear();

            if (Mods.Length > 1)
            {
                iconsContainer.AddRange(new[]
                {
                    backgroundIcon = new ModIcon(Mods[1], false)
                    {
                        Origin = Anchor.BottomRight,
                        Anchor = Anchor.BottomRight,
                        Position = new Vector2(1.5f),
                    },
                    foregroundIcon = new ModIcon(Mods[0], false)
                    {
                        Origin = Anchor.BottomRight,
                        Anchor = Anchor.BottomRight,
                        Position = new Vector2(-1.5f),
                    },
                });
            }
            else
            {
                iconsContainer.Add(foregroundIcon = new ModIcon(Mod, false)
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                });
            }
        }

        public ModButton(Mod mod)
        {
            Children = new Drawable[]
            {
                new Container
                {
                    Size = new Vector2(77f, 80f),
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        scaleContainer = new Container
                        {
                            Children = new Drawable[]
                            {
                                iconsContainer = new Container<ModIcon>
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                },
                                incompatibleIcon = new IncompatibleIcon
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.BottomRight,
                                    Position = new Vector2(-13),
                                }
                            },
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        }
                    }
                },
                text = new OsuSpriteText
                {
                    Y = 75,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Font = OsuFont.GetFont(size: 22)
                },
                new HoverSounds()
            };
            Mod = mod;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedMods.BindValueChanged(_ => Scheduler.AddOnce(updateCompatibility), true);
        }

        public ITooltip GetCustomTooltip() => new ModButtonTooltip();

        public object TooltipContent => SelectedMod ?? Mods.FirstOrDefault();
    }
}
