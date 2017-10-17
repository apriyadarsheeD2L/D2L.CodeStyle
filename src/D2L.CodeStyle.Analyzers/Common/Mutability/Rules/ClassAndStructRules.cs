﻿using System;
using System.Collections.Generic;
using D2L.CodeStyle.Analyzers.Common.Mutability.Goals;
using Microsoft.CodeAnalysis;

namespace D2L.CodeStyle.Analyzers.Common.Mutability.Rules {
	internal static class ClassAndStructRules {
		public static IEnumerable<Goal> Apply(
			ISemanticModel model,
			ClassGoal goal
		) {
			yield return new ConcreteTypeGoal(
				goal.Type.BaseType
			);

			foreach( var subgoal in ApplyToMembers( model, goal.Type ) ) {
				yield return subgoal;
			}
		}

		public static IEnumerable<Goal> Apply(
			ISemanticModel model,
			StructGoal goal
		) {
			// Structs have no base type

			foreach( var subgoal in ApplyToMembers( model, goal.Type ) ) {
				yield return subgoal;
			}
		}

		private static IEnumerable<Goal> ApplyToMembers(
			ISemanticModel model,
			ITypeSymbol type
		) {
			// ClassGoal and StructGoal will only be generated by TypeGoal and
			// ConcreteTypeGoal rules, and those rules won't generate this goal
			// for types in other assemblies. The foreach below wouldn't
			// function correctly for those types (e.g. private members would
			// be omitted.)
			if( model.Assembly() != type.ContainingAssembly ) {
				throw new InvalidOperationException( "pre-condition not met in ClassOrStructRule" );
			}

			var members = type.GetExplicitNonStaticMembers();

			foreach( ISymbol member in members ) {
				Goal subgoal;
				if( MemberToGoal( member, out subgoal ) ) {
					yield return subgoal;
				}
			}
		}

		private static bool MemberToGoal(
			ISymbol member,
			out Goal res
		) {
			switch( member.Kind ) {
				case SymbolKind.Field:
					res = new FieldGoal( member as IFieldSymbol );
					return true;

				case SymbolKind.Property:
					res = new PropertyGoal( member as IPropertySymbol );
					return true;

				// Methods can't hold state (aren't the source of mutability,
				// even if they mutate others state.)
				case SymbolKind.Method:
					res = null;
					return false;

				// Embedded class/struct definitions, etc.
				// These will be scanned if necessary (if some held state uses
				// that type) otherwise these aren't relevant (putting decls
				// inside the class is just a syntactic thing.)
				case SymbolKind.NamedType:
					res = null;
					return false;

				case SymbolKind.Event:
					res = new EventGoal( member as IEventSymbol );
					return true;

				default:
					// I'm not aware of any missed case but the SymbolKind enum
					// is a superset of the kinds of members.
					// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/members
					throw new NotImplementedException();
			}
		}
	}
}
