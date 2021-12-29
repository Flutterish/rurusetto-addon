using System;
using System.Threading.Tasks;

namespace osu.Game.Rulesets.RurusettoAddon {
	public class AsyncLazy<T> : Lazy<Task<T>> {
		public AsyncLazy ( Func<Task<T>> valueFactory ) : base( valueFactory ) { }
	}
}
