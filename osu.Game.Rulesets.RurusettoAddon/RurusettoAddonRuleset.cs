using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.RurusettoAddon.Configuration;
using osu.Game.Rulesets.RurusettoAddon.UI;
using osu.Game.Rulesets.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace osu.Game.Rulesets.RurusettoAddon;

public partial class RurusettoAddonRuleset : Ruleset {
    public override string Description => "rūrusetto addon";
    public override string ShortName => "rurusettoaddon";

    public override IRulesetConfigManager CreateConfig ( SettingsStore settings )
        => new RurusettoConfigManager( settings, RulesetInfo );
    public override RulesetSettingsSubsection CreateSettings ()
        => new RurusettoAddonConfigSubsection( this );

    public override DrawableRuleset CreateDrawableRulesetWith ( IBeatmap beatmap, IReadOnlyList<Mod> mods = null )
        => new DrawableRurusettoAddonRuleset( this, beatmap, mods );

    public override IBeatmapConverter CreateBeatmapConverter ( IBeatmap beatmap ) 
        => new RurusettoAddonBeatmapConverter( beatmap, this );

    public override DifficultyCalculator CreateDifficultyCalculator ( IWorkingBeatmap beatmap ) 
        => new RurusettoAddonDifficultyCalculator( RulesetInfo, beatmap );

    public override IEnumerable<Mod> GetModsFor ( ModType type )
        => Array.Empty<Mod>();

    public override IEnumerable<KeyBinding> GetDefaultKeyBindings ( int variant = 0 )
        => Array.Empty<KeyBinding>();

    public static LocalisableString ErrorMessage ( string code )
        => Localisation.Strings.LoadError( code );

    public Texture GetTexture ( GameHost host, TextureStore textures, string path ) {
        if ( !textures.GetAvailableResources().Contains( path ) )
            textures.AddStore( host.CreateTextureLoaderStore( CreateResourceStore() ) );

        return textures.Get( path );
    }

    public override Drawable CreateIcon () => new RurusettoIcon( this );
}

#region Vestigial organs

public class RurusettoAddonPlayfield : Playfield { }
public enum RurusettoAddonAction { }
public class RurusettoAddonInputManager : RulesetInputManager<RurusettoAddonAction> {
    public RurusettoAddonInputManager ( RulesetInfo ruleset ) : base( ruleset, 0, SimultaneousBindingMode.Unique ) { }
}
public class RurusettoAddonDifficultyCalculator : DifficultyCalculator {
    public RurusettoAddonDifficultyCalculator ( IRulesetInfo ruleset, IWorkingBeatmap beatmap ) : base( ruleset, beatmap ) { }

    protected override DifficultyAttributes CreateDifficultyAttributes ( IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate )
        => new( mods, 0 );

    protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects ( IBeatmap beatmap, double clockRate )
        => Array.Empty<DifficultyHitObject>();

    protected override Skill[] CreateSkills ( IBeatmap beatmap, Mod[] mods, double clockRate )
        => Array.Empty<Skill>();
}
public class RurusettoAddonBeatmapConverter : BeatmapConverter<HitObject> {
    public RurusettoAddonBeatmapConverter ( IBeatmap beatmap, Ruleset ruleset ) : base( beatmap, ruleset ) { }

    public override bool CanConvert () => false;

    protected override IEnumerable<HitObject> ConvertHitObject ( HitObject original, IBeatmap beatmap, CancellationToken cancellationToken ) {
        yield break;
    }
}
public class DrawableRurusettoAddonRuleset : DrawableRuleset<HitObject> {
    public DrawableRurusettoAddonRuleset ( RurusettoAddonRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null ) : base( ruleset, beatmap, mods ) { }

    protected override Playfield CreatePlayfield ()
        => new RurusettoAddonPlayfield();

    public override DrawableHitObject<HitObject> CreateDrawableRepresentation ( HitObject h )
        => null;

    protected override PassThroughInputManager CreateInputManager ()
        => new RurusettoAddonInputManager( Ruleset?.RulesetInfo );
}

#endregion