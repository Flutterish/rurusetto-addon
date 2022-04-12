using Newtonsoft.Json;

#nullable disable
namespace osu.Game.Rulesets.RurusettoAddon.API;

public record BeatmapRecommendation {
	/// <summary> user_detail of user who recommend this beatmap. </summary>
	[JsonProperty( "user_detail" )]
	public UserDetail Recommender { get; init; }

	/// <summary> ID of this beatmap in osu!. </summary>
	[JsonProperty( "beatmap_id" )]
	public int BeatmapID { get; init; }

	/// <summary> ID of set of this beatmap in osu!. </summary>
	[JsonProperty( "beatmapset_id" )]
	public int BeatmapSetID { get; init; }

	/// <summary> Beatmap's song name. </summary>
	[JsonProperty( "title" )]
	public string Title { get; init; }

	/// <summary> Song's artist of this beatmap. </summary>
	[JsonProperty( "artist" )]
	public string Artist { get; init; }

	/// <summary> Song's source of this beatmap. </summary>
	[JsonProperty( "source" )]
	public string Source { get; init; }

	/// <summary> Name of user in osu! who create this beatmap (mapper). </summary>
	[JsonProperty( "creator" )]
	public string Creator { get; init; }

	/// <summary> Approval state of this beatmap (4 = loved, 3 = qualified, 2 = approved, 1 = ranked, 0 = pending, -1 = WIP, -2 = graveyard) </summary>
	[JsonProperty( "approved" )]
	public BeatmapStatus Status { get; init; }

	/// <summary> Star rating of this beatmap in osu! mode. </summary>
	[JsonProperty( "difficultyrating" )]
	public float StarDifficulty { get; init; }

	/// <summary> BPM of the song in this beatmap. </summary>
	[JsonProperty( "bpm" )]
	public float BPM { get; init; }

	/// <summary> Difficulty name of this beatmap in beatmap's beatmapset. </summary>
	[JsonProperty( "version" )]
	public string Version { get; init; }

	/// <summary> URL to go to this beatmap in osu! website. </summary>
	[JsonProperty( "url" )]
	public string Url { get; init; }

	/// <summary> URL of beatmap's cover image that use as the background in beatmap page. </summary>
	[JsonProperty( "beatmap_cover" )]
	public string CoverImage { get; init; }

	/// <summary> URL of beatmap's thumbnail image that use in old osu! site and in osu! stable. </summary>
	[JsonProperty( "beatmap_thumbnail" )]
	public string ThumbnailImage { get; init; }

	/// <summary> URL of beatmap's card image that use in new osu! new beatmap card design. </summary>
	[JsonProperty( "beatmap_card" )]
	public string CardImage { get; init; }

	/// <summary> URL of beatmap's list image that use in new osu! new beatmap card design. </summary>
	[JsonProperty( "beatmap_list" )]
	public string ListImage { get; init; }

	/// <summary> Comment from user who recommend this beatmap. </summary>
	[JsonProperty( "comment" )]
	public string Comment { get; init; }

	/// <summary> The time on this recommend beatmap added to the site in JSON time format. </summary>
	[JsonProperty( "created_at" )]
	public DateTime? CreatedAt { get; init; }
}

public enum BeatmapStatus {
	Pending = 0,
	Ranked = 1,
	Approved = 2,
	Qualified = 3,
	Loved = 4,
	WIP = -1,
	Graveyard = -2
}