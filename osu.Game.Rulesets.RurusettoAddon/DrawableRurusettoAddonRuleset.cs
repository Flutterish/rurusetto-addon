using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class DrawableRurusettoAddonRuleset : DrawableRuleset<HitObject> {
        public DrawableRurusettoAddonRuleset( RurusettoAddonRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null ) : base( ruleset, beatmap, mods ) { }

        protected override Playfield CreatePlayfield() 
            => new RurusettoAddonPlayfield();

        public override DrawableHitObject<HitObject> CreateDrawableRepresentation ( HitObject h ) 
            => null;

        protected override PassThroughInputManager CreateInputManager() 
            => new RurusettoAddonInputManager( Ruleset?.RulesetInfo );
    }
}
