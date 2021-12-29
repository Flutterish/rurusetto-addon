using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace osu.Game.Rulesets.RurusettoAddon.API {
	public class RurusettoAPI {
		private HttpClient client = new();
		public readonly Bindable<string> Address = new( "https://rulesets.info/api/" );
		public Uri GetEndpoint ( string endpoint ) => new( new Uri( Address.Value ), endpoint );

		private ConcurrentDictionary<string, LocalRulesetWikiEntry> localWiki = new();

		private Task<List<ListingEntry>> listingCache = null;
		public async Task<IEnumerable<ListingEntry>> RequestRulesetListing () {
			if ( listingCache is null ) {
				listingCache = requestRulesetListing();
			}

			var value = await listingCache;
			return value.Concat( localWiki.Values.Select( x => x.ListingEntry ) );
		}
		private async Task<List<ListingEntry>> requestRulesetListing () {
			var raw = await client.GetStringAsync( GetEndpoint( "/api/rulesets" ) );
			return JsonConvert.DeserializeObject<List<ListingEntry>>( raw );
		}
		public void FlushRulesetListingCache () {
			listingCache = null;
		}

		private ConcurrentDictionary<string, Task<RulesetDetail>> rulesetDetailCache = new();
		public async Task<RulesetDetail> RequestRulesetDetail ( string slug ) {
			if ( !rulesetDetailCache.TryGetValue( slug, out var detail ) ) {
				if ( localWiki.TryGetValue( slug, out var local ) ) {
					return local.Detail;
				}
				else {
					detail = requestRulesetDetail( slug );
					rulesetDetailCache.TryAdd( slug, detail );
				}
			}

			return await detail;
		}
		private async Task<RulesetDetail> requestRulesetDetail ( string slug ) {
			var raw = await client.GetStringAsync( GetEndpoint( $"/api/rulesets/{slug}" ) );
			return JsonConvert.DeserializeObject<RulesetDetail>( raw );
		}
		public void FlushRulesetDetailCache ( string shortName ) {
			rulesetDetailCache.TryRemove( shortName, out var _ );
		}
		public void FlushRulesetDetailCache () {
			rulesetDetailCache.Clear();
		}

		private ConcurrentDictionary<string, Task<Texture>> mediaTextureCache = new();
		public async Task<Texture> RequestImage ( string uri ) {
			if ( !mediaTextureCache.TryGetValue( uri, out var task ) ) {
				task = requestImage( uri );
				mediaTextureCache.TryAdd( uri, task );
			}

			return await task;
		}
		private async Task<Texture> requestImage ( string uri ) {
			if ( !uri.StartsWith( "/media/" ) && !uri.StartsWith( "media/" ) && !uri.StartsWith( "/static/" ) && !uri.StartsWith( "static/" ) )
				throw new InvalidOperationException( $"Images can only be requested from `/media/` and `/static/` endpoints, but `{uri}` was requested." );

			var imageStream = await client.GetStreamAsync( GetEndpoint( uri ) );
			var image = await Image.LoadAsync<Rgba32>( imageStream );

			var texture = new Texture( image.Width, image.Height );
			texture.SetData( new TextureUpload( image ) );
			texture.AssetName = uri;

			return texture;
		}
		public void FlushImageCache ( string uri ) {
			mediaTextureCache.TryRemove( uri, out var _ );
		}
		public void FlushImageCache () {
			mediaTextureCache.Clear();
		}

		public async Task<Texture> RequestImage ( StaticAPIResource resource ) {
			return await RequestImage( resource.GetURI() );
		}
		public void FlushImageCache ( StaticAPIResource resource ) {
			FlushImageCache( resource.GetURI() );
		}

		public void FlushAllCaches () {
			FlushImageCache();
			FlushRulesetListingCache();
			FlushRulesetDetailCache();
		}

		public void InjectLocalRuleset ( LocalRulesetWikiEntry entry ) {
			localWiki.TryAdd( entry.ListingEntry.Slug, entry );
		}
		public void ClearLocalWiki () {
			localWiki.Clear();
		}
		public LocalRulesetWikiEntry CreateLocalEntry ( string shortname, string name ) {
			return new LocalRulesetWikiEntry {
				ListingEntry = new() {
					Slug = shortname,
					Name = name,
					Description = "Local ruleset, not listed on the wiki.",
					CanDownload = false,
					Owner = new UserDetail()
				},
				Detail = new RulesetDetail {
					CanDownload = false,
					Content = "Local ruleset, not listed on the wiki.",
					CreatedAt = DateTime.Now,
					Creator = new(),
					Description = "Local ruleset, not listed on the wiki.",
					LastEditedAt = DateTime.Now,
					LastEditedBy = new(),
					Name = name,
					Slug = shortname,
					CoverDark = StaticAPIResource.DefaultCover.GetURI(),
					CoverLight = StaticAPIResource.DefaultCover.GetURI(),
					Owner = new()
				}
			};
		}
	}

	public record LocalRulesetWikiEntry {
		public ListingEntry ListingEntry { get; init; }
		public RulesetDetail Detail { get; init; }
	}

	public enum StaticAPIResource {
		LogoBlack,
		LogoWhite,
		LogoWithName,
		LogoRegular,

		HomeCoverDark,
		HomeCoverLight,
		ListingCoverDark,
		ListingCoverLight,
		StatusCoverDark,
		StatusCoverLight,
		InstallCoverDark,
		InstallCoverLight,
		ChangelogCoverDark,
		ChangelogCoverLight,

		DefaultProfileImage,
		DefaultCover
	}

	public static class StaticAPIResourceExtensions {
		private static Dictionary<StaticAPIResource, string> uris = new() {
			[StaticAPIResource.LogoBlack] = "/static/logo/rurusetto-logo-black.svg",
			[StaticAPIResource.LogoWhite] = "/static/logo/rurusetto-logo-white.svg",
			[StaticAPIResource.LogoWithName] = "/static/logo/rurusetto-logo-with-name.svg",
			[StaticAPIResource.LogoRegular] = "/static/logo/rurusetto-logo.svg",

			[StaticAPIResource.HomeCoverDark] = "/static/img/home-cover-night.png",
			[StaticAPIResource.HomeCoverLight] = "/static/img/home-cover-light.jpeg",
			[StaticAPIResource.ListingCoverDark] = "/static/img/listing-cover-night.png",
			[StaticAPIResource.ListingCoverLight] = "/static/img/listing-cover-light.png",
			[StaticAPIResource.StatusCoverDark] = "/static/img/status-cover-night.jpg",
			[StaticAPIResource.StatusCoverLight] = "/static/img/status-cover-light.png",
			[StaticAPIResource.InstallCoverDark] = "/static/img/install-cover-night.png",
			[StaticAPIResource.InstallCoverLight] = "/static/img/install-cover-light.png",
			[StaticAPIResource.ChangelogCoverDark] = "/static/img/changelog-cover-night2.png",
			[StaticAPIResource.ChangelogCoverLight] = "/static/img/changelog-cover-light3.png",

			[StaticAPIResource.DefaultProfileImage] = "/static/img/default.png",
			[StaticAPIResource.DefaultCover] = "/media/default_wiki_cover.jpeg"
		};

		public static string GetURI ( this StaticAPIResource res )
			=> uris[ res ];
	}
}
