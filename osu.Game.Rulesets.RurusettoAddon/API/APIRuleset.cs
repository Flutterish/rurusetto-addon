﻿using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Rulesets.RurusettoAddon.UI;
using osuTK;
using System;
using System.Collections.Generic;

#nullable enable

namespace osu.Game.Rulesets.RurusettoAddon.API {
	public class APIRuleset {
		public Source Source;

		public RurusettoAPI? API;
		public string? Slug;

		public LocalisableString Name = Localisation.Strings.UntitledRuleset;
		public string? LocalPath;
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
		public LocalisableString Description => string.IsNullOrWhiteSpace( ListingEntry?.Description ) ? Localisation.Strings.LocalRulesetDescription : ListingEntry.Description;
		/// <inheritdoc cref="ListingEntry.CanDownload"/>
		public bool CanDownload => ListingEntry?.CanDownload ?? false;
		/// <inheritdoc cref="ListingEntry.Download"/>
		public string? Download => ListingEntry?.Download;

		/// <summary>
		/// Merges info from <paramref name="other"/> into itself
		/// </summary>
		public void Merge ( APIRuleset other ) {
			Source = other.Source;
			Slug = other.Slug;
			Name = other.Name;
			LocalPath = other.LocalPath;
			ShortName = other.ShortName;
			IsModifiable = other.IsModifiable;
			IsPresentLocally = other.IsPresentLocally;
			HasImportFailed = other.HasImportFailed;
			LocalRulesetInfo = other.LocalRulesetInfo;
			ListingEntry = other.ListingEntry;
		}

		/// <summary>
		/// Creates the dark mode variant of the ruleset logo as a drawable with relative size axes
		/// </summary>
		public void RequestDarkLogo ( Action<Drawable> success, Action<Drawable>? failure = null ) {
			static Drawable createDefault () {
				return new SpriteIcon {
					RelativeSizeAxes = Axes.Both,
					FillMode = FillMode.Fit,
					Icon = FontAwesome.Solid.QuestionCircle
				};
			}

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

					success( container );
				}
				else if ( icon != null ) {
					icon.RelativeSizeAxes = Axes.Both;
					icon.Size = new Vector2( 1 );

					success( icon );
				}
				else {
					failure?.Invoke( createDefault() );
				}
			}
			else if ( !string.IsNullOrWhiteSpace( ListingEntry?.DarkIcon ) && API != null ) {
				API.RequestImage( ListingEntry.DarkIcon, logo => {
					success( new Sprite {
						RelativeSizeAxes = Axes.Both,
						FillMode = FillMode.Fit,
						Texture = logo
					} );
				}, () => {
					failure?.Invoke( createDefault() );
				} );
			}
			else {
				failure?.Invoke( createDefault() );
			}
		}

		public void RequestDetail ( Action<RulesetDetail> success, Action? failure = null ) {
			if ( Source == Source.Web && !string.IsNullOrWhiteSpace( Slug ) && API != null ) {
				API.RequestRulesetDetail( Slug, success, failure );
				// TODO this on failures
				//API.FlushRulesetDetailCache( Slug );
				//new RulesetDetail {
				//	CanDownload = CanDownload,
				//	Download = Download,
				//	Content = $"Failed to fetch the ruleset wiki page. Sorry!\nYou can still try visiting it at [rurusetto]({API.Address.Value.TrimEnd('/')}/rulesets/{Slug}).",
				//	CreatedAt = DateTime.Now,
				//	Description = Description,
				//	LastEditedAt = DateTime.Now,
				//	Name = Name,
				//	Slug = Slug
				//};
			}
			else {
				success( new RulesetDetail {
					CanDownload = CanDownload,
					Download = Download,
					Content = Localisation.Strings.LocalRulesetDescription,
					CreatedAt = DateTime.Now,
					Description = Description,
					LastEditedAt = DateTime.Now,
					Name = Name,
					Slug = Slug
				} );
			}
		}

		public void RequestSubpages ( Action<IEnumerable<SubpageListingEntry>> success, Action? failure = null ) {
			if ( Source == Source.Web && API != null && !string.IsNullOrWhiteSpace( Slug ) ) {
				API.RequestSubpageListing( Slug, success, failure );
			}
			else {
				success( Array.Empty<SubpageListingEntry>() );
			}
		}

		public void RequestSubpage ( string subpageSlug, Action<Subpage> success, Action? failure = null ) {
			if ( Source == Source.Web && API != null && !string.IsNullOrWhiteSpace( Slug ) && !string.IsNullOrWhiteSpace( subpageSlug ) ) {
				API.RequestSubpage( Slug, subpageSlug, success, failure );
			}
			else {
				failure?.Invoke();
			}
		}

		public void RequestDarkCover ( Action<Texture> success, Action? failure = null ) {
			if ( API is null ) {
				failure?.Invoke();
			}
			else {
				RequestDetail( detail => {
					if ( string.IsNullOrWhiteSpace( detail.CoverDark ) ) {
						API.RequestImage( StaticAPIResource.DefaultCover, success, failure );
					}
					else {
						API.RequestImage( detail.CoverDark, success, failure );
					}
				}, failure );
			}
		}

		public IEnumerable<DrawableTag> GenerateTags ( RulesetDetail detail, bool large = false, bool includePlayability = true ) {
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
				if ( includePlayability && ListingEntry.Status.IsBorked ) {
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
