using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using System.Collections.Generic;
using System.Threading;

namespace osu.Game.Rulesets.RurusettoAddon {
	public class RurusettoAddonBeatmapConverter : BeatmapConverter<HitObject> {
		public RurusettoAddonBeatmapConverter ( IBeatmap beatmap, Ruleset ruleset ) : base( beatmap, ruleset ) { }

		public override bool CanConvert () => false;

		protected override IEnumerable<HitObject> ConvertHitObject ( HitObject original, IBeatmap beatmap, CancellationToken cancellationToken ) {
			yield break;
		}
	}
}
