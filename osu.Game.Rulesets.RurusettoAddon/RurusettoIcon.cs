using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.Toolbar;
using System.Diagnostics.CodeAnalysis;

namespace osu.Game.Rulesets.RurusettoAddon;

public class RurusettoIcon : Sprite {
    RurusettoAddonRuleset ruleset;

    public RurusettoIcon ( RurusettoAddonRuleset ruleset ) {
        this.ruleset = ruleset;

        RelativeSizeAxes = Axes.Both;
        FillMode = FillMode.Fit;
        Origin = Anchor.Centre;
        Anchor = Anchor.Centre;
    }

    [BackgroundDependencyLoader( permitNulls: true )]
    void load ( OsuGame game, GameHost host, TextureStore textures ) {
        Texture = ruleset.GetTexture( host, textures, "Textures/rurusetto-logo.png" );

        injectOverlay( game, host );
    }

    public static LocalisableString ErrorMessage ( string code )
        => Localisation.Strings.LoadError( code );

    // we are using the icon load code to inject our "mixin" since it is present in both the intro and the toolbar, where the overlay button should be
    void injectOverlay ( OsuGame game, GameHost host ) {
        if ( game is null ) return;
        if ( game.Dependencies.Get<RurusettoOverlay>() != null ) return;

        var osu = (typeof( OsuGame ), game);

        var notifications = osu.GetField<NotificationOverlay>( "Notifications" );
        if ( notifications is null )
            return;

        void error ( string code ) {
            Schedule( () => notifications.Post( new SimpleErrorNotification { Text = ErrorMessage( code ) } ) );
        }
        bool guard ( [NotNullWhen(false)] object? x, string code ) {
            if ( x is null ) {
                error( code );
                return true;
            }
            return false;
        }

        // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/OsuGame.cs#L790
        // contains overlays
        var overlayContent = osu.GetField<Container>( "overlayContent" );
        if ( guard( overlayContent, "#OCNRE" ) )
            return;

        // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/OsuGame.cs#L953
        // caches the overlay globally and allows us to run code when it is loaded
        var loadComponent = osu.GetMethod<RurusettoOverlay>( "loadComponentSingleFile" );
        if ( guard( loadComponent, "#LCNRE" ) )
            return;

        try {
            loadComponent.Invoke( game,
                new object[] { new RurusettoOverlay( ruleset ), (Action<RurusettoOverlay>)addOverlay, true }
            );
        }
        catch ( Exception ) {
            error( "#LCIE" );
        }

        void addOverlay ( RurusettoOverlay overlay ) {
            Action abort = () => { };
            void errDefer ( Action action ) {
                var oldAbort = abort;
                abort = () => { action(); oldAbort(); };
			}

            overlayContent.Add( overlay );
            errDefer( () => overlayContent.Remove( overlay ) );

            // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/Overlays/Toolbar/Toolbar.cs#L89
            // leveraging an "easy" hack to get the container with toolbar buttons
            var userButton = (typeof( Toolbar ), game.Toolbar).GetField<Drawable>( "userButton" );
            if ( userButton is null || userButton.Parent is not FillFlowContainer buttonsContainer ) {
                error( "#UBNRE" );
                abort();
                return;
            }

            var button = new RurusettoToolbarButton();
            buttonsContainer.Insert( -1, button );
            errDefer( () => buttonsContainer.Remove( button ) );

            // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/OsuGame.cs#L855
            // add overlay hiding, since osu does it manually
            var singleDisplayOverlays = new[] { "chatOverlay", "news", "dashboard", "beatmapListing", "changelogOverlay", "wikiOverlay" };
            var overlays = singleDisplayOverlays.Select( name => osu.GetField<OverlayContainer>( name ) ).ToList();

            if ( !game.Dependencies.TryGet<RankingsOverlay>( out var rov ) ) {
                error( "#ROVNRE" );
                abort();
                return;
            }

            overlays.Add( rov );

            if ( overlays.Any( x => x is null ) ) {
                error( "#OVNRE" );
                abort();
                return;
            }

            foreach ( var i in overlays ) {
                i!.State.ValueChanged += v => {
                    if ( v.NewValue != Visibility.Visible ) return;

                    overlay.Hide();
                };
            }

            overlay.State.ValueChanged += v => {
                if ( v.NewValue != Visibility.Visible ) return;

                foreach ( var i in overlays ) {
                    i!.Hide();
                }

                // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/OsuGame.cs#L896
                // show above other overlays
                if ( overlay.IsLoaded )
                    overlayContent.ChangeChildDepth( overlay, (float)-Clock.CurrentTime );
                else
                    overlay.Depth = (float)-Clock.CurrentTime;
            };

            host.Exited += () => {
                overlay.Dependencies?.Get<RulesetDownloadManager>().PerformTasks();
            };
        }
    }
}