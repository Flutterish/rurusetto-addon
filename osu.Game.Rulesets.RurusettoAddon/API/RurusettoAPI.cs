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

		private Dictionary<string, LocalRulesetWikiEntry> localWiki = new();

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
		public async Task<RulesetDetail> RequestRulesetDetail ( string shortName ) {
			if ( !rulesetDetailCache.TryGetValue( shortName, out var detail ) ) {
				if ( localWiki.TryGetValue( shortName, out var local ) ) {
					return local.Detail;
				}
				else {
					detail = requestRulesetDetail( shortName );
					rulesetDetailCache.TryAdd( shortName, detail );
				}
			}

			return await detail;
		}
		private async Task<RulesetDetail> requestRulesetDetail ( string shortName ) {
			var raw = await client.GetStringAsync( GetEndpoint( $"/api/rulesets/{shortName}" ) );
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
			if ( !uri.StartsWith( "/media/" ) && !uri.StartsWith( "media/" ) )
				throw new InvalidOperationException( $"Images can only be requested from `/media/` endpoints, but `{uri}` was requested." );

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
		public void FlushMediaImageCache () {
			mediaTextureCache.Clear();
		}

		private ConcurrentDictionary<StaticAPIResource, Task<Texture>> staticTextureCache = new();
		public async Task<Texture> RequestImage ( StaticAPIResource resource ) {
			if ( !staticTextureCache.TryGetValue( resource, out var task ) ) {
				task = requestImage( resource );
				staticTextureCache.TryAdd( resource, task );
			}

			return await task;
		}
		private async Task<Texture> requestImage ( StaticAPIResource resource ) {
			var imageStream = await client.GetStreamAsync( GetEndpoint( (resource.GetURI().StartsWith( "/media/" ) ? "" : "/static") + resource.GetURI() ) );
			var image = await Image.LoadAsync<Rgba32>( imageStream );
			imageStream.Dispose();

			var texture = new Texture( image.Width, image.Height );
			texture.SetData( new TextureUpload( image ) );
			texture.AssetName = resource.ToString();

			return texture;
		}
		public void FlushImageCache ( StaticAPIResource resource ) {
			staticTextureCache.Remove( resource, out var _ );
		}
		public void FlushStaticImageCache () {
			staticTextureCache.Clear();
		}

		public void FlushAllImageCaches () {
			FlushMediaImageCache();
			FlushStaticImageCache();
		}

		public void FlushAllCaches () {
			FlushAllImageCaches();
			FlushRulesetListingCache();
			FlushRulesetDetailCache();
		}

		public void InjectLocalRuleset ( LocalRulesetWikiEntry entry ) {
			localWiki.Add( entry.ListingEntry.ShortName, entry );
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
			[StaticAPIResource.LogoBlack] = "/logo/rurusetto-logo-black.svg",
			[StaticAPIResource.LogoWhite] = "/logo/rurusetto-logo-white.svg",
			[StaticAPIResource.LogoWithName] = "/logo/rurusetto-logo-with-name.svg",
			[StaticAPIResource.LogoRegular] = "/logo/rurusetto-logo.svg",

			[StaticAPIResource.HomeCoverDark] = "/img/home-cover-night.png",
			[StaticAPIResource.HomeCoverLight] = "/img/home-cover-light.jpeg",
			[StaticAPIResource.ListingCoverDark] = "/img/listing-cover-night.png",
			[StaticAPIResource.ListingCoverLight] = "/img/listing-cover-light.png",
			[StaticAPIResource.StatusCoverDark] = "/img/status-cover-night.jpg",
			[StaticAPIResource.StatusCoverLight] = "/img/status-cover-light.png",
			[StaticAPIResource.InstallCoverDark] = "/img/install-cover-night.png",
			[StaticAPIResource.InstallCoverLight] = "/img/install-cover-light.png",
			[StaticAPIResource.ChangelogCoverDark] = "/img/changelog-cover-night2.png",
			[StaticAPIResource.ChangelogCoverLight] = "/img/changelog-cover-light3.png",

			[StaticAPIResource.DefaultProfileImage] = "/img/default.png",
			[StaticAPIResource.DefaultCover] = "/media/default_wiki_cover.jpeg"
		};

		public static string GetURI ( this StaticAPIResource res )
			=> uris[ res ];
	}
}
