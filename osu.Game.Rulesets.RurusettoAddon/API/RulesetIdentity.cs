using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace osu.Game.Rulesets.RurusettoAddon.API {
	public class RulesetIdentity {
		public Source Source;

		public RurusettoAPI? API;
		public string? Slug;

		public string? LocalPath;
		public string? Name;
		public string? ShortName;
		public bool IsModifiable;
		public bool IsPresentLocally;
		public bool HasImportFailed;
		public IRulesetInfo? LocalRulesetInfo;
	}

	public enum Source {
		Web,
		Local
	}
}
