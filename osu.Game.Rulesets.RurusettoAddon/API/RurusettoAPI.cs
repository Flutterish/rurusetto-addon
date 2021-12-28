using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace osu.Game.Rulesets.RurusettoAddon.API {
	public class RurusettoAPI {
		private HttpClient client = new();
		public readonly Bindable<string> Address = new( "https://rulesets.info/api/" );
		public Uri GetEndpoint ( string endpoint ) => new( new Uri( Address.Value ), endpoint );

		private Task<List<ListingEntry>> listingCache = null;
		public async Task<List<ListingEntry>> RequestRulesetListing () {
			if ( listingCache is null ) {
				listingCache = requestRulesetListing();
			}

			return await listingCache;
		}
		private async Task<List<ListingEntry>> requestRulesetListing () {
			var raw = await client.GetStringAsync( GetEndpoint( "/api/rulesets" ) );
			return JsonConvert.DeserializeObject<List<ListingEntry>>( raw );
		}
		public void FlushRulesetListingCache () {
			listingCache = null;
		}

		private Dictionary<string, Task<RulesetDetail>> rulesetDetailCache = new();
		public async Task<RulesetDetail> RequestRulesetDetail ( string shortName ) {
			if ( !rulesetDetailCache.TryGetValue( shortName, out var detail ) ) {
				detail = requestRulesetDetail( shortName );
				rulesetDetailCache.Add( shortName, detail );
			}

			return await detail;
		}
		private async Task<RulesetDetail> requestRulesetDetail ( string shortName ) {
			var raw = await client.GetStringAsync( GetEndpoint( $"/api/rulesets/{shortName}" ) );
			return JsonConvert.DeserializeObject<RulesetDetail>( raw );
		}
		public void FlushRulesetDetailCache ( string shortName ) {
			rulesetDetailCache.Remove( shortName );
		}
		public void FlushRulesetDetailCache () {
			rulesetDetailCache.Clear();
		}

		private Dictionary<string, Task<Texture>> madiaTextureCache = new();
		public async Task<Texture> RequestImage ( string uri ) {
			if ( !madiaTextureCache.TryGetValue( uri, out var task ) ) {
				task = requestImage( uri );
				madiaTextureCache.Add( uri, task );
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
			madiaTextureCache.Remove( uri );
		}
		public void FlushMediaImageCache () {
			madiaTextureCache.Clear();
		}

		private Dictionary<StaticAPIResource, Task<Texture>> staticTextureCache = new();
		public async Task<Texture> RequestImage ( StaticAPIResource resource ) {
			if ( !staticTextureCache.TryGetValue( resource, out var task ) ) {
				task = requestImage( resource );
				staticTextureCache.Add( resource, task );
			}

			return await task;
		}
		private async Task<Texture> requestImage ( StaticAPIResource resource ) {
			var imageStream = await client.GetStreamAsync( GetEndpoint( "/static" + resource.GetURI() ) );
			var image = await Image.LoadAsync<Rgba32>( imageStream );
			imageStream.Dispose();

			var texture = new Texture( image.Width, image.Height );
			texture.SetData( new TextureUpload( image ) );
			texture.AssetName = resource.ToString();

			return texture;
		}
		public void FlushImageCache ( StaticAPIResource resource ) {
			staticTextureCache.Remove( resource );
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

		DefaultProfileImage
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

			[StaticAPIResource.DefaultProfileImage] = "/img/default.png"
		};

		public static string GetURI ( this StaticAPIResource res )
			=> uris[ res ];
	}
}
