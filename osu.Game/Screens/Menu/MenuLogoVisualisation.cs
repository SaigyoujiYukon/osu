﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Game.Skinning;
using osu.Game.Online.API;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Screens.Menu
{
    public class MenuLogoVisualisation : LogoVisualisation
    {
        private IBindable<APIUser> user;
        private Bindable<Skin> skin;

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, SkinManager skinManager)
        {
            user = api.LocalUser.GetBoundCopy();
            skin = skinManager.CurrentSkin.GetBoundCopy();

            user.ValueChanged += _ => updateColour();
            skin.BindValueChanged(_ => updateColour(), true);
        }

        private void updateColour()
        {
            if (user.Value?.IsSupporter ?? false)
                Colour = skin.Value.GetConfig<GlobalSkinColours, Color4>(GlobalSkinColours.MenuGlow)?.Value ?? Color4.White;
            else
                Colour = Color4.White;
        }
    }
}
