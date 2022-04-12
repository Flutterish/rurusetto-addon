using Newtonsoft.Json;
using osu.Framework.Localisation;

#nullable disable
namespace osu.Game.Rulesets.RurusettoAddon.API;

public record SubpageListingEntry {
	/// <summary> Title of the subpage </summary>
	[JsonProperty( "title" )]
	public LocalisableString Title { get; init; }

	/// <summary> Slug of the subpage. Use in subpage URL path. </summary>
	[JsonProperty( "slug" )]
	public string Slug { get; init; }
}