using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.RurusettoAddon.API;
using System;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class RulesetDownloadButton : GrayButton, IHasContextMenu {
		[Resolved]
		public RulesetDownloadManager DownloadManager { get; private set; }

		public readonly Bindable<DownloadState> State = new( DownloadState.NotDownloading );
		public readonly Bindable<Availability> Avail = new( Availability.Unknown );

		LoadingSpinner spinner;
		RulesetIdentity ruleset;
		Warning warning;
		public RulesetDownloadButton ( RulesetIdentity ruleset ) : base( FontAwesome.Solid.Download ) {
			this.ruleset = ruleset;
			Action = onClick;
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			Icon.Colour = Colour4.White;
			Icon.Scale = new osuTK.Vector2( 1.5f );
			Background.Colour = Colour4.FromHex( "#394642" );

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
			OsuMenuItem download = new( "Download", MenuItemType.Standard, () => DownloadManager.DownloadRuleset( ruleset ) );
			OsuMenuItem update = new( "Update", MenuItemType.Standard, () => DownloadManager.UpdateRuleset( ruleset ) );
			OsuMenuItem redownload = new( "Re-download", MenuItemType.Standard, () => DownloadManager.UpdateRuleset( ruleset ) );
			OsuMenuItem remove = new( "Remove", MenuItemType.Destructive, () => DownloadManager.RemoveRuleset( ruleset ) );
			OsuMenuItem cancelDownload = new( "Cancel Download", MenuItemType.Standard, () => DownloadManager.CancelRulesetDownload( ruleset ) );
			OsuMenuItem cancelUpdate = new( "Cancel Update", MenuItemType.Standard, () => DownloadManager.CancelRulesetDownload( ruleset ) );
			OsuMenuItem cancelRemoval = new( "Cancel Removal", MenuItemType.Standard, () => DownloadManager.CancelRulesetRemoval( ruleset ) );
			OsuMenuItem refresh = new( "Refresh", MenuItemType.Standard, () => DownloadManager.CheckAvailability( ruleset ) );

			if ( State.Value == DownloadState.Downloading ) {
				Icon.Alpha = 0;
				spinner.Alpha = 1;
				this.FadeTo( 1f, 200 );
				Background.FadeColour( Colour4.FromHex( "#6291D7" ), 200, Easing.InOutExpo );
				TooltipText = "Downloading...";
				warning.FadeOut( 200 );

				ContextMenuItems = new MenuItem[] { Avail.Value.HasFlagFast( Availability.AvailableLocally ) ? cancelUpdate : cancelDownload };
			}
			else if ( State.Value is DownloadState.ToBeImported or DownloadState.ToBeRemoved || Avail.Value.HasFlagFast( Availability.AvailableLocally ) ) {
				spinner.Alpha = 0;
				Icon.Alpha = 1;
				this.FadeTo( 1f, 200 );
				Background.FadeColour( Colour4.FromHex( "#6CB946" ), 200, Easing.InOutExpo );
				if ( Avail.Value.HasFlagFast( Availability.Outdated ) && Avail.Value.HasFlagFast( Availability.AvailableOnline ) && State.Value == DownloadState.NotDownloading ) {
					Icon.Scale = new osuTK.Vector2( 1.5f );
					Icon.Icon = FontAwesome.Solid.Download;

					TooltipText = "Update";
				}
				else {
					Icon.Scale = new osuTK.Vector2( 1.7f );
					Icon.Icon = FontAwesome.Regular.CheckCircle;

					TooltipText = Avail.Value.HasFlagFast( Availability.NotAvailableOnline ) ? "Installed, not available online" : "Installed";
				}

				if ( State.Value == DownloadState.ToBeImported ) {
					warning.FadeIn( 200 );
					warning.TooltipText = Avail.Value.HasFlagFast( Availability.AvailableLocally ) ? "Will be updated on restart!" : "Will be installed on restart!";

					ContextMenuItems = new MenuItem[] { refresh, Avail.Value.HasFlagFast( Availability.AvailableLocally ) ? cancelUpdate : remove };
				}
				else if ( State.Value == DownloadState.ToBeRemoved ) {
					warning.FadeIn( 200 );
					warning.TooltipText = "Will be removed on restart!";

					ContextMenuItems = new MenuItem[] { refresh, cancelRemoval };
				}
				else if ( Avail.Value.HasFlagFast( Availability.Outdated ) ) {
					warning.FadeIn( 200 );
					warning.TooltipText = "Outdated";

					ContextMenuItems = Avail.Value.HasFlagFast( Availability.AvailableOnline )
						? new MenuItem[] { refresh, update, remove }
						: new MenuItem[] { refresh, remove };
				}
				else {
					warning.FadeOut( 200 );

					ContextMenuItems = Avail.Value.HasFlagFast( Availability.AvailableOnline )
						? new MenuItem[] { refresh, redownload, remove }
						: new MenuItem[] { refresh, remove };
				}
			}
			else if ( Avail.Value.HasFlagFast( Availability.NotAvailableOnline ) ) {
				spinner.Alpha = 0;
				Icon.Alpha = 1;
				this.FadeTo( 0.6f, 200 );
				TooltipText = "Unavailable Online";
				warning.FadeOut( 200 );
				Icon.Scale = new osuTK.Vector2( 1.5f );
				Icon.Icon = FontAwesome.Solid.Download;

				ContextMenuItems = new MenuItem[] { refresh };
			}
			else if ( Avail.Value.HasFlagFast( Availability.AvailableOnline ) ) {
				spinner.Alpha = 0;
				Icon.Alpha = 1;
				this.FadeTo( 1f, 200 );
				Background.FadeColour( Colour4.FromHex( "#394642" ), 200, Easing.InOutExpo );
				Icon.Scale = new osuTK.Vector2( 1.5f );
				Icon.Icon = FontAwesome.Solid.Download;
				TooltipText = "Download";
				warning.FadeOut( 200 );

				ContextMenuItems = new MenuItem[] { refresh, download };
			}

			if ( Avail.Value == Availability.Unknown ) {
				this.FadeTo( 0.6f, 200 );
				TooltipText = "Checking...";
				warning.FadeOut( 200 );

				ContextMenuItems = Array.Empty<MenuItem>();
			}

			if ( !ruleset.IsModifiable ) {
				ContextMenuItems = Array.Empty<MenuItem>();
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

		public MenuItem[] ContextMenuItems { get; private set; }

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
