using Newtonsoft.Json;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using SixLabors.ImageSharp.PixelFormats;
using System.Net.Http;
using System.Threading.Tasks;

namespace osu.Game.Rulesets.RurusettoAddon.API;

public class RurusettoAPI : Component {
	private HttpClient client = new();
	public const string DefaultAPIAddress = "https://rulesets.info/api/";
	public readonly Bindable<string> Address = new( DefaultAPIAddress );
	public Uri GetEndpoint ( string endpoint ) => new( new Uri( Address.Value ), endpoint );

	[Resolved]
	private IRenderer renderer { get; set; } = null!;

	public RurusettoAPI () {
		Address.ValueChanged += _ => FlushAllCaches();
	}

	private async void queue<T> ( Task<T?> task, Action<T>? success = null, Action<Exception?>? failure = null, Action? cancelled = null ) {
		if ( task.Status == TaskStatus.RanToCompletion ) {
			if ( task.Result is null )
				failure?.Invoke( task.Exception );
			else
				success?.Invoke( task.Result );
		}
		else {
			var address = Address.Value;
			try {
				var value = await task;
				Schedule( () => {
					// if we changed the address, everyone depending on that address will either be disposed or request again
					if ( address != Address.Value )
						cancelled?.Invoke();
					else if ( value is null )
						failure?.Invoke( null );
					else
						success?.Invoke( value );
				} );
			}
			catch ( Exception e ) {
				Schedule( () => {
					if ( address == Address.Value )
						failure?.Invoke( e );
					else
						cancelled?.Invoke();
				} );
			}
		}
	}

	public void LogFailure ( string message, Exception? exception = null ) {
		message = $"Unhandled Rurusetto Addon Error: `{message}` - API Address: `{Address.Value}`";
		if ( exception is null )
			Logger.Log( message, LoggingTarget.Network, LogLevel.Error );
		else
			Logger.Error( exception, message, LoggingTarget.Network );
	}

	private Task<IEnumerable<ListingEntry>?>? listingCache = null;
	public void RequestRulesetListing ( Action<IEnumerable<ListingEntry>>? success = null, Action<Exception?>? failure = null, Action? cancelled = null ) {
		failure ??= e => LogFailure( $"Could not retrieve ruleset listing", e );

		queue( listingCache ??= requestRulesetListing(), success, failure, cancelled );
	}
	private async Task<IEnumerable<ListingEntry>?> requestRulesetListing () {
		var raw = await client.GetStringAsync( GetEndpoint( "/api/rulesets" ) );
		return JsonConvert.DeserializeObject<List<ListingEntry>>( raw );
	}
	public void FlushRulesetListingCache () {
		listingCache = null;
	}

	private Dictionary<string, Task<RulesetDetail?>> rulesetDetailCache = new();
	public void RequestRulesetDetail ( string slug, Action<RulesetDetail>? success = null, Action<Exception?>? failure = null ) {
		failure ??= e => LogFailure( $"Could not retrieve ruleset detail for {slug}", e );

		if ( !rulesetDetailCache.TryGetValue( slug, out var detail ) ) {
			rulesetDetailCache.Add( slug, detail = requestRulesetDetail( slug ) );
		}

		queue( detail, success, failure );
	}
	private async Task<RulesetDetail?> requestRulesetDetail ( string slug ) {
		var raw = await client.GetStringAsync( GetEndpoint( $"/api/rulesets/{slug}" ) );
		return JsonConvert.DeserializeObject<RulesetDetail>( raw );
	}
	public void FlushRulesetDetailCache ( string shortName ) {
		rulesetDetailCache.Remove( shortName );
	}
	public void FlushRulesetDetailCache () {
		rulesetDetailCache.Clear();
	}

	private Dictionary<string, Task<IEnumerable<SubpageListingEntry>?>> subpageListingCache = new();
	public void RequestSubpageListing ( string slug, Action<IEnumerable<SubpageListingEntry>>? success = null, Action<Exception?>? failure = null ) {
		failure ??= e => LogFailure( $"Could not retrieve subpage listing for {slug}", e );

		if ( !subpageListingCache.TryGetValue( slug, out var listing ) ) {
			subpageListingCache.TryAdd( slug, listing = requestSubpageListing( slug ) );
		}

		queue( listing, success, failure );
	}
	private async Task<IEnumerable<SubpageListingEntry>?> requestSubpageListing ( string slug ) {
		var raw = await client.GetStringAsync( GetEndpoint( $"/api/subpage/{slug}" ) );
		return JsonConvert.DeserializeObject<List<SubpageListingEntry>>( raw );
	}
	public void FlushSubpageListingCache ( string shortName ) {
		subpageListingCache.Remove( shortName );
	}
	public void FlushSubpageListingCache () {
		subpageListingCache.Clear();
	}

	private Dictionary<string, Task<Subpage?>> subpageCache = new();
	public void RequestSubpage ( string rulesetSlug, string subpageSlug, Action<Subpage>? success = null, Action<Exception?>? failure = null ) {
		failure ??= e => LogFailure( $"Could not retrieve subpage {subpageSlug} for ruleset {rulesetSlug}", e );

		if ( !subpageCache.TryGetValue( $"{rulesetSlug}/{subpageSlug}", out var listing ) ) {
			subpageCache.TryAdd( $"{rulesetSlug}/{subpageSlug}", listing = requestSubpage( rulesetSlug, subpageSlug ) );
		}

		queue( listing, success, failure );
	}
	private async Task<Subpage?> requestSubpage ( string rulesetSlug, string subpageSlug ) {
		var raw = await client.GetStringAsync( GetEndpoint( $"/api/subpage/{rulesetSlug}/{subpageSlug}" ) );
		return JsonConvert.DeserializeObject<Subpage>( raw );
	}
	public void FlushSubpageCache ( string rulesetSlug, string subpageSlug ) {
		subpageCache.Remove( $"{rulesetSlug}/{subpageSlug}" );
	}
	public void FlushSubpageCache ( string rulesetSlug ) {
		foreach ( var i in subpageCache.Keys.Where( x => x.StartsWith( $"{rulesetSlug}/" ) ).ToArray() ) {
			subpageCache.Remove( i );
		}
	}
	public void FlushSubpageCache () {
		subpageCache.Clear();
	}

	public enum RecommendationSource {
		All,
		Author,
		Users
	}
	private Dictionary<(string slug, RecommendationSource source), Task<List<BeatmapRecommendation>?>> recommmendationCache = new();
	public void RequestBeatmapRecommendations ( string rulesetSlug, RecommendationSource source, Action<List<BeatmapRecommendation>>? success = null, Action<Exception?>? failure = null ) {
		failure ??= e => LogFailure( $"Could not retrieve beatmap recommendations from {source} for ruleset {rulesetSlug}", e );

		if ( !recommmendationCache.TryGetValue( (rulesetSlug, source), out var list ) ) {
			recommmendationCache.TryAdd( (rulesetSlug, source), list = requestBeatmapRecommendations( rulesetSlug, source ) );
		}

		queue( list, success, failure );
	}
	private async Task<List<BeatmapRecommendation>?> requestBeatmapRecommendations ( string rulesetSlug, RecommendationSource source ) {
		var url = $"/api/rulesets/{rulesetSlug}/beatmaps";
		if ( source is RecommendationSource.Author )
			url += "/creator";
		else if ( source is RecommendationSource.Users )
			url += "/players";

		var raw = await client.GetStringAsync( GetEndpoint( url ) );
		return JsonConvert.DeserializeObject<List<BeatmapRecommendation>>( raw );
	}
	public void FlushBeatmapRecommendationsCache ( string rulesetSlug, RecommendationSource source ) {
		recommmendationCache.Remove( (rulesetSlug, source) );
	}
	public void FlushBeatmapRecommendationsCache ( string rulesetSlug ) {
		foreach ( var i in recommmendationCache.Keys.Where( x => x.slug == rulesetSlug ).ToArray() ) {
			recommmendationCache.Remove( i );
		}
	}
	public void FlushBeatmapRecommendationsCache () {
		recommmendationCache.Clear();
	}

	private Dictionary<int, Task<UserProfile?>> userCache = new();
	public void RequestUserProfile ( int id, Action<UserProfile>? success = null, Action<Exception?>? failure = null ) {
		failure ??= e => LogFailure( $"Could not retrieve user profile with ID = {id}", e );

		if ( !userCache.TryGetValue( id, out var user ) ) {
			userCache.TryAdd( id, user = requestUserProfile( id ) );
		}

		queue( user, success, failure );
	}
	private async Task<UserProfile?> requestUserProfile ( int id ) {
		var raw = await client.GetStringAsync( GetEndpoint( $"/api/profile/{id}" ) );
		return JsonConvert.DeserializeObject<UserProfile>( raw );
	}
	public void FlushUserProfileCache ( int id ) {
		userCache.Remove( id );
	}
	public void FlushUserProfileCache () {
		userCache.Clear();
	}

	private Dictionary<string, Task<Texture?>> mediaTextureCache = new();
	public void RequestImage ( string uri, Action<Texture>? success = null, Action<Exception?>? failure = null ) {
		failure ??= e => LogFailure( $"Could not retrieve image at `{uri}`", e );

		if ( !mediaTextureCache.TryGetValue( uri, out var task ) ) {
			mediaTextureCache.TryAdd( uri, task = requestImage( uri ) );
		}

		queue( task, success, failure );
	}
	private async Task<Texture?> requestImage ( string uri ) {
		if ( !uri.StartsWith( "/media/" ) && !uri.StartsWith( "media/" ) && !uri.StartsWith( "/static/" ) && !uri.StartsWith( "static/" ) )
			throw new InvalidOperationException( $"Images can only be requested from `/media/` and `/static/` endpoints, but `{uri}` was requested." );

		var imageStream = await client.GetStreamAsync( GetEndpoint( uri ) );
		var image = await Image.LoadAsync<Rgba32>( imageStream );

		var texture = renderer.CreateTexture( image.Width, image.Height );
		texture.SetData( new TextureUpload( image ) );
		texture.AssetName = uri;

		return texture;
	}
	public void FlushImageCache ( string uri ) {
		mediaTextureCache.Remove( uri );
	}
	public void FlushImageCache () {
		mediaTextureCache.Clear();
	}

	public void RequestImage ( StaticAPIResource resource, Action<Texture>? success = null, Action<Exception?>? failure = null ) {
		failure ??= e => LogFailure( $"Could not retrieve static resource {resource} at {resource.GetURI()}", e );

		RequestImage( resource.GetURI(), success, failure );
	}
	public void FlushImageCache ( StaticAPIResource resource ) {
		FlushImageCache( resource.GetURI() );
	}

	public void FlushAllCaches () {
		FlushImageCache();
		FlushRulesetListingCache();
		FlushRulesetDetailCache();
		FlushSubpageListingCache();
		FlushSubpageCache();
		FlushUserProfileCache();
		FlushBeatmapRecommendationsCache();
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
		=> uris[res];
}