using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.Toolbar;
using System.Reflection;

namespace osu.Game.Rulesets.RurusettoAddon;

public class RurusettoIcon : Sprite {
    private RurusettoAddonRuleset ruleset;

    public RurusettoIcon ( RurusettoAddonRuleset ruleset ) {
        this.ruleset = ruleset;

        RelativeSizeAxes = Axes.Both;
        FillMode = FillMode.Fit;
        Origin = Anchor.Centre;
        Anchor = Anchor.Centre;
    }

    [BackgroundDependencyLoader( permitNulls: true )]
    void load ( OsuGame game, GameHost host, TextureStore textures ) {
        if ( !textures.GetAvailableResources().Contains( "Textures/rurusetto-logo.png" ) )
            textures.AddStore( host.CreateTextureLoaderStore( ruleset.CreateResourceStore() ) );

        Texture = textures.Get( "Textures/rurusetto-logo.png" );

        injectOverlay( game, host );
    }

    public static LocalisableString ErrorMessage ( string code )
        => Localisation.Strings.LoadError( code );

    // we are using the icon load code to inject our "mixin" since it is present in both the intro and the toolbar, where the overlay button should be
    void injectOverlay ( OsuGame game, GameHost host ) {
        if ( game is null ) return;
        if ( game.Dependencies.Get<RurusettoOverlay>() != null ) return;

        var notifications = typeof( OsuGame ).GetField( "Notifications", BindingFlags.NonPublic | BindingFlags.Instance )?.GetValue( game ) as NotificationOverlay;
        if ( notifications is null ) {
            return;
        }

        // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/OsuGame.cs#L790
        // contains overlays
        var overlayContent = typeof( OsuGame ).GetField( "overlayContent", BindingFlags.NonPublic | BindingFlags.Instance )?.GetValue( game ) as Container;

        if ( overlayContent is null ) {
            Schedule( () => notifications.Post( new SimpleErrorNotification { Text = ErrorMessage( "#OCNRE" ) } ) );
            return;
        }

        // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/OsuGame.cs#L953
        // caches the overlay globally and allows us to run code when it is loaded
        var loadComponent = typeof( OsuGame ).GetMethod( "loadComponentSingleFile", BindingFlags.NonPublic | BindingFlags.Instance )?.MakeGenericMethod( typeof( RurusettoOverlay ) );

        if ( loadComponent is null ) {
            Schedule( () => notifications.Post( new SimpleErrorNotification { Text = ErrorMessage( "#LCNRE" ) } ) );
            return;
        }

        try {
            loadComponent.Invoke( game,
                new object[] { new RurusettoOverlay( ruleset ), (Action<RurusettoOverlay>)addOverlay, true }
            );
        }
        catch ( Exception ) {
            Schedule( () => notifications.Post( new SimpleErrorNotification { Text = ErrorMessage( "#LCIE" ) } ) );
            return;
        }

        void addOverlay ( RurusettoOverlay overlay ) {
            overlayContent.Add( overlay );

            // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/Overlays/Toolbar/Toolbar.cs#L89
            // leveraging an "easy" hack to get the container with toolbar buttons
            var userButton = typeof( Toolbar ).GetField( "userButton", BindingFlags.NonPublic | BindingFlags.Instance )?.GetValue( game.Toolbar ) as Drawable;
            if ( userButton is null || userButton.Parent is not FillFlowContainer buttonsContainer ) {
                Schedule( () => notifications.Post( new SimpleErrorNotification { Text = ErrorMessage( "#UBNRE" ) } ) );
                overlayContent.Remove( overlay );
                return;
            }

            var button = new RurusettoToolbarButton();
            buttonsContainer.Insert( -1, button );

            // https://github.com/ppy/osu/blob/edf5e558aca6cd75e70b510a5f0dd233d6cfcb90/osu.Game/OsuGame.cs#L855
            // add overlay hiding, since osu does it manually
            var singleDisplayOverlays = new string[] { "chatOverlay", "news", "dashboard", "beatmapListing", "changelogOverlay", "wikiOverlay" };
            var overlays = singleDisplayOverlays.Select( name =>
                typeof( OsuGame ).GetField( name, BindingFlags.NonPublic | BindingFlags.Instance )?.GetValue( game ) as OverlayContainer
            ).ToList();
            if ( game.Dependencies.TryGet<RankingsOverlay>( out var rov ) ) {
                overlays.Add( rov );
            }
            else {
                Schedule( () => notifications.Post( new SimpleErrorNotification { Text = ErrorMessage( "#ROVNRE" ) } ) );
                overlayContent.Remove( overlay );
                buttonsContainer.Remove( button );
                return;
            }

            if ( overlays.Any( x => x is null ) ) {
                Schedule( () => notifications.Post( new SimpleErrorNotification { Text = ErrorMessage( "#OVNRE" ) } ) );
                overlayContent.Remove( overlay );
                buttonsContainer.Remove( button );
                return;
            }

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