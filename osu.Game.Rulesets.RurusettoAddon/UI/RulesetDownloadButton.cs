using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class RulesetDownloadButton : GrayButton {
		[Resolved]
		public RulesetDownloadManager DownloadManager { get; private set; }

		public readonly Bindable<DownloadState> State = new( DownloadState.NotDownloading );
		public readonly Bindable<Availability> Avail = new( Availability.Unknown );

		LoadingSpinner spinner;
		APIRuleset ruleset;
		Warning warning;
		public bool UseDarkerBackground { get; init; }
		public bool ProvideContextMenu { get; init; } = true;
		public RulesetDownloadButton ( APIRuleset ruleset ) : base( FontAwesome.Solid.Download ) {
			this.ruleset = ruleset;
			Action = onClick;
		}

		[Resolved]
		private OsuColour colours { get; set; }

		[Resolved]
		private OverlayColourProvider overlayColours { get; set; }

		protected override void LoadComplete () {
			base.LoadComplete();

			Icon.Colour = Colour4.White;
			Icon.Scale = new osuTK.Vector2( 1.5f );
			Background.Colour = overlayColours.Background3;

			Add( spinner = new LoadingSpinner {
				Scale = new osuTK.Vector2( 0.45f )
			} );

			AddInternal( warning = new Warning {
				Height = 16,
				Width = 16,
				Alpha = 0,
				X = -7,
				Y = -7
			} );

			if ( ProvideContextMenu )
				AddInternal( new RulesetManagementContextMenu( ruleset ) );

			DownloadManager.BindWith( ruleset, State );
			DownloadManager.BindWith( ruleset, Avail );

			State.ValueChanged += _ => Schedule( updateVisuals );
			Avail.ValueChanged += _ => Schedule( updateVisuals );

			updateVisuals();

			Schedule( () => FinishTransforms( true ) );
		}

		[Resolved]
		private IBindable<RulesetInfo> currentRuleset { get; set; }

		private void updateVisuals () {
			if ( State.Value == DownloadState.Downloading ) {
				Icon.Alpha = 0;
				spinner.Alpha = 1;
				this.FadeTo( 1f, 200 );
				Background.FadeColour( colours.Blue3, 200, Easing.InOutExpo );
				TooltipText = Localisation.Strings.Downloading;
				warning.FadeOut( 200 );
			}
			else if ( State.Value is DownloadState.ToBeImported or DownloadState.ToBeRemoved || Avail.Value.HasFlagFast( Availability.AvailableLocally ) ) {
				spinner.Alpha = 0;
				Icon.Alpha = 1;
				this.FadeTo( 1f, 200 );
				Background.FadeColour( Colour4.FromHex( "#6CB946" ), 200, Easing.InOutExpo );
				if ( Avail.Value.HasFlagFast( Availability.Outdated ) && Avail.Value.HasFlagFast( Availability.AvailableOnline ) && State.Value == DownloadState.NotDownloading ) {
					Icon.Scale = new osuTK.Vector2( 1.5f );
					Icon.Icon = FontAwesome.Solid.Download;

					TooltipText = Localisation.Strings.Update;
				}
				else {
					Icon.Scale = new osuTK.Vector2( 1.7f );
					Icon.Icon = FontAwesome.Regular.CheckCircle;

					TooltipText = Avail.Value.HasFlagFast( Availability.NotAvailableOnline ) ? Localisation.Strings.InstalledUnavailableOnline : Localisation.Strings.Installed;
				}

				if ( State.Value == DownloadState.ToBeImported ) {
					warning.FadeIn( 200 );
					warning.TooltipText = Avail.Value.HasFlagFast( Availability.AvailableLocally ) ? Localisation.Strings.ToBeUpdated : Localisation.Strings.ToBeInstalled;
				}
				else if ( State.Value == DownloadState.ToBeRemoved ) {
					warning.FadeIn( 200 );
					warning.TooltipText = Localisation.Strings.ToBeRemoved;
				}
				else if ( Avail.Value.HasFlagFast( Availability.Outdated ) ) {
					warning.FadeIn( 200 );
					warning.TooltipText = Localisation.Strings.Outdated;
				}
				else {
					warning.FadeOut( 200 );
				}
			}
			else if ( Avail.Value.HasFlagFast( Availability.NotAvailableOnline ) ) {
				spinner.Alpha = 0;
				Icon.Alpha = 1;
				this.FadeTo( 0.6f, 200 );
				Background.FadeColour( UseDarkerBackground ? overlayColours.Background4 : overlayColours.Background3, 200, Easing.InOutExpo );
				TooltipText = Localisation.Strings.UnavailableOnline;
				warning.FadeOut( 200 );
				Icon.Scale = new osuTK.Vector2( 1.5f );
				Icon.Icon = FontAwesome.Solid.Download;
			}
			else if ( Avail.Value.HasFlagFast( Availability.AvailableOnline ) ) {
				spinner.Alpha = 0;
				Icon.Alpha = 1;
				this.FadeTo( 1f, 200 );
				Background.FadeColour( UseDarkerBackground ? overlayColours.Background4 : overlayColours.Background3, 200, Easing.InOutExpo );
				Icon.Scale = new osuTK.Vector2( 1.5f );
				Icon.Icon = FontAwesome.Solid.Download;
				TooltipText = Localisation.Strings.Download;
				warning.FadeOut( 200 );
			}

			if ( Avail.Value == Availability.Unknown ) {
				this.FadeTo( 0.6f, 200 );
				TooltipText = Localisation.Strings.DownloadChecking;
				warning.FadeOut( 200 );
			}
		}
		
		void onClick () {
			if ( Avail.Value.HasFlagFast( Availability.AvailableOnline ) && State.Value == DownloadState.NotDownloading && Avail.Value.HasFlagFast( Availability.Outdated ) ) {
				DownloadManager.UpdateRuleset( ruleset );
			}
			else if ( Avail.Value.HasFlagFast( Availability.AvailableOnline ) && State.Value == DownloadState.NotDownloading && Avail.Value.HasFlagFast( Availability.NotAvailableLocally ) ) {
				DownloadManager.DownloadRuleset( ruleset );
			}
			else if ( Avail.Value.HasFlagFast( Availability.AvailableLocally ) && currentRuleset is Bindable<RulesetInfo> current && ruleset.LocalRulesetInfo is RulesetInfo info ) {
				current.Value = info;
			}
		}

		private class Warning : CircularContainer, IHasTooltip {
			public Warning () {
				Add( new Box {
					RelativeSizeAxes = Axes.Both,
					Colour = Colour4.FromHex( "#FF6060" )
				} );
				Add( new Circle {
					Origin = Anchor.Centre,
					Anchor = Anchor.Centre,
					RelativeSizeAxes = Axes.Both,
					Size = new osuTK.Vector2( 0.55f ),
					Colour = Colour4.White
				} );

				Masking = true;
			}
				
			public LocalisableString TooltipText { get; set; }
		}
	}
}
