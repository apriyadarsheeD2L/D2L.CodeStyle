﻿namespace D2L.CodeStyle.Analyzers.Common.Exemptions {
	internal struct Exemption {
		public Exemption(
			ExemptionKind kind,
			string identifier
		) {
			Kind = kind;
			Identifier = identifier;
		}

		public ExemptionKind Kind { get; }
		public string Identifier { get; }

		public override bool Equals(object obj) {
			if ( !(obj is Exemption) ) {
				return false;
			}

			var other = (Exemption)obj;

			return Kind == other.Kind && Identifier == other.Identifier;
		}

		public override int GetHashCode() {
			int result = 17;
			result = ( result * 23 ) + Kind.GetHashCode();
			result = ( result * 23 ) + Identifier.GetHashCode();
			return result;
		}
	}
}
