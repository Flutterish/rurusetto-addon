using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Toolbar;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.RurusettoAddon.Configuration;
using osu.Game.Rulesets.RurusettoAddon.UI;
using osu.Game.Rulesets.RurusettoAddon.UI.Overlay;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace osu.Game.Rulesets.RurusettoAddon {
	public class RurusettoAddonRuleset : Ruleset
    {
        public override string Description => "rūrusetto addon";
        public override string ShortName => "rurusettoaddon";

        public override IRulesetConfigManager CreateConfig ( SettingsStore settings )
            => new RurusettoConfigManager( settings, RulesetInfo );

		public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) =>
            new DrawableRurusettoAddonRuleset(this, beatmap, mods);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) =>
            new RurusettoAddonBeatmapConverter(beatmap, this);

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) =>
            new RurusettoAddonDifficultyCalculator(RulesetInfo, beatmap);

        public override IEnumerable<Mod> GetModsFor ( ModType type )
            => Array.Empty<Mod>();

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings ( int variant = 0 )
            => Array.Empty<KeyBinding>();

        public override Drawable CreateIcon() => new Icon(ShortName[0], this);

        public class Icon : CompositeDrawable
        {
            private RurusettoAddonRuleset ruleset;

            public Icon ( char c, RurusettoAddonRuleset ruleset ) {
                this.ruleset = ruleset;

                InternalChildren = new Drawable[] {
                    new Circle {
                        Size = new Vector2(20),
                        Colour = Color4.White,
                    },
                    new SpriteText {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = c.ToString(),
                        Font = OsuFont.Default.With(size: 18)
                    }
                };
            }
            
            // we are using the icon load code to inject our "mixin" since it is present in both the intro and the toolbar, where the overlay button should be
            [BackgroundDependencyLoader(permitNulls: true)]
            private void load ( OsuGame game ) {
                if ( game is null ) return;
                if ( game.Dependencies.Get<RurusettoOverlay>() != null ) return;

                // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/OsuGame.cs#L790
                // contains overlays
                var overlayContent = typeof( OsuGame ).GetField( "overlayContent", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( game ) as Container;

                // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/OsuGame.cs#L953
                // caches the overlay globally and allows us to run code when it is loaded
                typeof( OsuGame ).GetMethod( "loadComponentSingleFile", BindingFlags.NonPublic | BindingFlags.Instance ).MakeGenericMethod( typeof( RurusettoOverlay ) ).Invoke(
                    game,
                    new object[] { new RurusettoOverlay( ruleset ), (Action<RurusettoOverlay>)((overlay) => {
                        overlayContent.Add( overlay );

                        // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/Overlays/Toolbar/Toolbar.cs#L89
                        // leveraging an "easy" hack to get the container with toolbar buttons
                        var userButton = typeof( Toolbar ).GetField( "userButton", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( game.Toolbar ) as Drawable;
                        ( userButton.Parent as FillFlowContainer ).Insert( -1, new RurusettoToolbarButton() );

                        // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/OsuGame.cs#L855
                        // add overlay hiding, since osu does it manually
                        var singleDisplayOverlays = new string[] { "chatOverlay", "news", "dashboard", "beatmapListing", "changelogOverlay", "wikiOverlay" };
                        var overlays = singleDisplayOverlays.Select( name => typeof( OsuGame ).GetField( name, BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( game ) as OverlayContainer ).Append( game.Dependencies.Get<RankingsOverlay>() ).ToArray();
                        foreach ( var i in overlays ) {
                            i.State.ValueChanged += v => {
                                if ( v.NewValue != Visibility.Visible ) return;

                                overlay.Hide();
                            };
                        }

                        overlay.State.ValueChanged += v => {
                            if ( v.NewValue != Visibility.Visible ) return;

                            foreach ( var i in overlays ) {
                                i.Hide();
                            }

                            // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/OsuGame.cs#L896
                            // show above other overlays
                            if (overlay.IsLoaded)
                                overlayContent.ChangeChildDepth(overlay, (float)-Clock.CurrentTime);
                            else
                                overlay.Depth = (float)-Clock.CurrentTime;
                        };
                    }), true }
                );
            }
        }
    }
}
