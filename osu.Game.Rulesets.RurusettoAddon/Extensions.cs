global using System;
global using System.Linq;
global using System.Collections.Generic;
global using osuTK;
global using osu.Framework.Bindables;
global using osu.Framework.Graphics;
global using osu.Framework.Allocation;
global using osu.Framework.Graphics.Containers;
global using osu.Framework.Graphics.Shapes;
global using osu.Framework.Graphics.Sprites;
global using osu.Game.Graphics;
global using osu.Game.Graphics.Containers;
global using osu.Game.Rulesets.RurusettoAddon.API;
global using osu.Game.Rulesets.RurusettoAddon.UI.Overlay;
global using osu.Framework.Extensions.Color4Extensions;
global using osu.Framework.Graphics.Colour;
global using osu.Framework.Graphics.Primitives;
global using Image = SixLabors.ImageSharp.Image;

namespace osu.Game.Rulesets.RurusettoAddon;

public static class Extensions {
	public static T GetOrAdd<T, Tkey> ( this IDictionary<Tkey, T> self, Tkey key, Func<T> @default ) {
		if ( !self.TryGetValue( key, out var value ) )
			self.Add( key, value = @default() );

		return value;
	}
}
