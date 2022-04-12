using Newtonsoft.Json;

namespace osu.Game.Rulesets.RurusettoAddon.API;

public record Status {
	/// <summary> The latest version name of the ruleset. </summary>
	[JsonProperty( "latest_version" )]
	public string LatestVersion { get; init; }
	/// <summary> The time on ruleset's latest update in JSON time format. </summary>
	[JsonProperty( "latest_update" )]
	public DateTime? LatestUpdate { get; init; }

	/// <summary> True if the ruleset is marked as pre-release in GitHub Release. </summary>
	[JsonProperty( "pre_realase" )]
	public bool IsPrerelease { get; init; }
	/// <summary> The latest changelog of the ruleset in markdown format. </summary>
	[JsonProperty( "changelog" )]
	public string Changelog { get; init; }
	/// <summary> The size of the latest release file in bytes. </summary>
	[JsonProperty( "file_size" )]
	public int FileSize { get; init; }
	/// <summary> The status about the playable of the ruleset. Has 3 choices (yes, no, unknown) </summary>
	[JsonProperty( "playable" )]
	public string PlayableStatus { get; init; }

	public bool IsPlayable => PlayableStatus == "yes";
	public bool IsBorked => PlayableStatus == "no";
}