using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.RurusettoAddon.UI;
using osuTK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace osu.Game.Rulesets.RurusettoAddon.API {
	public class RulesetIdentity {
		public Source Source;

		public RurusettoAPI? API;
		public string? Slug;

		public string? LocalPath;
		public string  Name = "Untitled Ruleset";
		public string? ShortName;
		public bool IsModifiable;
		public bool IsPresentLocally;
		public bool HasImportFailed;

		public IRulesetInfo? LocalRulesetInfo;
		public ListingEntry? ListingEntry;

		// ListingEntry properties

		/// <inheritdoc cref="ListingEntry.Owner"/>
		public UserDetail? Owner => ListingEntry?.Owner;
		/// <inheritdoc cref="ListingEntry.IsVerified"/>
		public bool IsVerified => ListingEntry?.IsVerified ?? false;
		/// <inheritdoc cref="ListingEntry.Description"/>
		public string Description => ListingEntry?.Description ?? "Local ruleset, not listed on the wiki.";
		/// <inheritdoc cref="ListingEntry.CanDownload"/>
		public bool CanDownload => ListingEntry?.CanDownload ?? false;
		/// <inheritdoc cref="ListingEntry.Download"/>
		public string? Download => ListingEntry?.Download;

		/// <summary>
		/// Creates the dark mode variant of the ruleset logo as a drawable with relative size axes
		/// </summary>
		public async Task<Drawable> RequestDarkLogo () {
			if ( LocalRulesetInfo != null ) {
				var icon = LocalRulesetInfo.CreateInstance()?.CreateIcon();

				if ( icon is CompositeDrawable cd && cd.AutoSizeAxes != Axes.None ) {
					var container = new Container {
						RelativeSizeAxes = Axes.Both,
						Child = icon
					};

					container.OnUpdate += c => {
						c.Scale = Vector2.Divide( c.DrawSize, icon.DrawSize );
					};

					return container;
				}
				else if ( icon != null ) {
					icon.RelativeSizeAxes = Axes.Both;
					icon.Size = new Vector2( 1 );

					return icon;
				}
			}
			else if ( !string.IsNullOrWhiteSpace( ListingEntry?.DarkIcon ) && API != null ) {
				try {
					var logo = await API.RequestImage( ListingEntry.DarkIcon );
					return new Sprite {
						RelativeSizeAxes = Axes.Both,
						FillMode = FillMode.Fit,
						Texture = logo
					};
				}
				catch ( Exception ) {
					// TODO report this
				}
			}

			return new SpriteIcon {
				RelativeSizeAxes = Axes.Both,
				FillMode = FillMode.Fit,
				Icon = FontAwesome.Solid.QuestionCircle
			};
		}

		public async Task<RulesetDetail> RequestDetail () {
			if ( Source == Source.Web && !string.IsNullOrWhiteSpace( Slug ) && API != null ) {
				try {
					var detail = await API.RequestRulesetDetail( Slug );

					return detail;
				}
				catch ( Exception ) {
					API.FlushRulesetDetailCache( Slug );

					return new RulesetDetail {
						CanDownload = CanDownload,
						Download = Download,
						Content = $"Failed to fetch the ruleset wiki page. Sorry!\nYou can still try visiting it at [rurusetto]({API.Address.Value.TrimEnd('/')}/rulesets/{Slug}).",
						CreatedAt = DateTime.Now,
						Description = Description,
						LastEditedAt = DateTime.Now,
						Name = Name,
						Slug = Slug
					};
				}
			}
			else {
				return new RulesetDetail {
					CanDownload = CanDownload,
					Download = Download,
					Content = "Local ruleset, not listed on the wiki.",
					CreatedAt = DateTime.Now,
					Description = Description,
					LastEditedAt = DateTime.Now,
					Name = Name,
					Slug = Slug
				};
			}
		}

		public async Task<Texture?> RequestDarkCover ( RulesetDetail detail ) {
			if ( API is null )
				return null;

			try {
				if ( string.IsNullOrWhiteSpace( detail.CoverDark ) ) {
					return await API.RequestImage( StaticAPIResource.DefaultCover );
				}
				else {
					return await API.RequestImage( detail.CoverDark );
				}
			}
			catch ( Exception ) {
				return null; // TODO report failure
			}
		}

		public IEnumerable<DrawableTag> GenerateTags ( RulesetDetail detail, bool large = false ) {
			if ( Source == Source.Local ) {
				yield return DrawableTag.CreateLocal( large );
			}
			if ( LocalRulesetInfo != null && !IsModifiable ) {
				yield return DrawableTag.CreateHardCoded( large );
			}
			if ( HasImportFailed ) {
				yield return DrawableTag.CreateFailledImport( large );
			}

			if ( detail.IsArchived ) {
				yield return DrawableTag.CreateArchived( large );
			}
			if ( ListingEntry != null ) {
				if ( ListingEntry.Status.IsBorked ) {
					yield return DrawableTag.CreateBorked( large );
				}
				if ( ListingEntry.Status.IsPrerelease ) {
					yield return DrawableTag.CreatePrerelease( large );
				}
			}
		}

		public override string ToString ()
			=> $"{Name}";
	}

	public enum Source {
		Web,
		Local
	}
}
