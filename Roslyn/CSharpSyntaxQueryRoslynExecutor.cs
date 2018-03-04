using System;
using System.Collections.Generic;
using System.Linq;
using Lastql.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lastql.Roslyn {
    using static CSharpSyntaxQueryTarget;
    using static SyntaxKind;

    public class CSharpSyntaxQueryRoslynExecutor {
        private static readonly IReadOnlyDictionary<CSharpSyntaxQueryTarget, HashSet<SyntaxKind>> SyntaxKindsByTarget = new Dictionary<CSharpSyntaxQueryTarget, HashSet<SyntaxKind>> {
            { Abstract, HashSet(AbstractKeyword) },
            { Add, HashSet(AddAccessorDeclaration, AddKeyword) },
            { Alias, HashSet(AliasKeyword) },
            { As, HashSet(AsExpression) },
            { Ascending, HashSet(AscendingKeyword) },
            { Assembly, HashSet(AssemblyKeyword) },
            { Async, HashSet(AsyncKeyword) },
            { Await, HashSet(AwaitExpression) },
            { Base, HashSet(BaseExpression, BaseConstructorInitializer) },
            { If, HashSet(IfStatement) },
            { Bool, HashSet(BoolKeyword) },
            { Break, HashSet(BreakStatement, BreakKeyword) },
            { By, HashSet(ByKeyword) },
            { Byte, HashSet(ByteKeyword) },
            { Case, HashSet(CaseSwitchLabel, CaseKeyword) },
            { Catch, HashSet(CatchClause, CatchDeclaration) },
            { Char, HashSet(CharKeyword) },
            { Checked, HashSet(CheckedExpression, CheckedStatement, CheckedKeyword) },
            { Class, HashSet(ClassDeclaration, ClassConstraint, ClassKeyword) },
            { Const, HashSet(ConstKeyword) },
            { Continue, HashSet(ContinueStatement, ContinueKeyword) },
            { Decimal, HashSet(DecimalKeyword) },
            { Default, HashSet(DefaultExpression, DefaultLiteralExpression, DefaultKeyword, DefaultSwitchLabel) },
            { Delegate, HashSet(DelegateDeclaration, DelegateKeyword, AnonymousMethodExpression) },
            { Descending, HashSet(DescendingKeyword) },
            { Do, HashSet(DoStatement, DoKeyword) },
            { Double, HashSet(DoubleKeyword) },
            { Else, HashSet(ElseClause, ElseKeyword) },
            { Enum, HashSet(EnumDeclaration, EnumKeyword) },
            { CSharpSyntaxQueryTarget.Equals, HashSet(EqualsKeyword) },
            { Event, HashSet(EventDeclaration, EventFieldDeclaration, EventKeyword) },
            { Explicit, HashSet(ExplicitKeyword) },
            { Extern, HashSet(ExternAliasDirective, ExternKeyword) },
            { False, HashSet(FalseLiteralExpression, FalseKeyword) },
            { Field, HashSet(FieldKeyword) },
            { Finally, HashSet(FinallyClause, FinallyKeyword) },
            { Fixed, HashSet(FixedStatement, FixedKeyword) },
            { Float, HashSet(FloatKeyword) },
            { For, HashSet(ForStatement, ForKeyword) },
            { Foreach, HashSet(ForEachStatement, ForEachKeyword) },
            { From, HashSet(FromClause, FromKeyword) },
            { Get, HashSet(GetAccessorDeclaration, GetKeyword) },
            { Global, HashSet(GlobalKeyword) },
            { Goto, HashSet(GotoCaseStatement, GotoDefaultStatement, GotoStatement, GotoKeyword) },
            { Group, HashSet(GroupClause, GroupKeyword) },
            { Implicit, HashSet(ImplicitKeyword) },
            { In, HashSet(InKeyword) },
            { Int, HashSet(IntKeyword) },
            { Interface, HashSet(InterfaceDeclaration, InterfaceKeyword) },
            { Internal, HashSet(InternalKeyword) },
            { Into, HashSet(IntoKeyword) },
            { Is, HashSet(IsExpression, IsPatternExpression, IsKeyword) },
            { Join, HashSet(JoinClause, JoinIntoClause, JoinKeyword) },
            { Let, HashSet(LetClause, LetKeyword) },
            { Lock, HashSet(LockStatement, LockKeyword) },
            { Long, HashSet(LongKeyword) },
            { Method, HashSet(MethodKeyword) },
            { Module, HashSet(ModuleKeyword) },
            { NameOf, HashSet(NameOfKeyword) },
            { Namespace, HashSet(NamespaceDeclaration, NamespaceKeyword) },
            { New, HashSet(
               AnonymousObjectCreationExpression,
               ArrayCreationExpression,
               ImplicitArrayCreationExpression,
               ObjectCreationExpression
            ) },
            { Null, HashSet(NullLiteralExpression, NullKeyword) },
            { Object, HashSet(ObjectKeyword) },
            { On, HashSet(OnKeyword) },
            { Operator, HashSet(OperatorDeclaration, OperatorKeyword) },
            { OrderBy, HashSet(OrderByClause, OrderByKeyword) },
            { Out, HashSet(OutKeyword) },
            { Override, HashSet(OverrideKeyword) },
            { Param, HashSet(ParamKeyword) },
            { Params, HashSet(ParamsKeyword) },
            { Partial, HashSet(PartialKeyword) },
            { Private, HashSet(PrivateKeyword) },
            { Property, HashSet(PropertyKeyword) },
            { Protected, HashSet(ProtectedKeyword) },
            { Public, HashSet(PublicKeyword) },
            { Readonly, HashSet(ReadOnlyKeyword) },
            { Ref, HashSet(RefKeyword) },
            { Remove, HashSet(RemoveAccessorDeclaration, RemoveKeyword) },
            { Return, HashSet(ReturnStatement, ReturnKeyword) },
            { SByte, HashSet(SByteKeyword) },
            { Sealed, HashSet(SealedKeyword) },
            { Select, HashSet(SelectClause, SelectKeyword) },
            { Set, HashSet(SetAccessorDeclaration, SetKeyword) },
            { Short, HashSet(ShortKeyword) },
            { Sizeof, HashSet(SizeOfExpression, SizeOfKeyword) },
            { Stackalloc, HashSet(StackAllocArrayCreationExpression, StackAllocKeyword) },
            { Static, HashSet(StaticKeyword) },
            { String, HashSet(StringKeyword) },
            { Struct, HashSet(StructDeclaration, StructKeyword) },
            { Switch, HashSet(SwitchStatement, SwitchKeyword) },
            { This, HashSet(ThisConstructorInitializer, ThisExpression, ThisKeyword) },
            { Throw, HashSet(ThrowStatement, ThrowExpression, ThrowKeyword) },
            { True, HashSet(TrueLiteralExpression, TrueKeyword) },
            { Try, HashSet(TryStatement, TryKeyword) },
            { Type, HashSet(TypeKeyword) },
            { TypeOf, HashSet(TypeOfExpression, TypeOfKeyword) },
            { Typevar, HashSet(TypeVarKeyword) },
            { UInt, HashSet(UIntKeyword) },
            { ULong, HashSet(ULongKeyword) },
            { Unchecked, HashSet(UncheckedStatement, UncheckedExpression, UncheckedKeyword) },
            { Unsafe, HashSet(UnsafeStatement, UnsafeKeyword) },
            { UShort, HashSet(UShortKeyword) },
            { Using, HashSet(UsingDirective, UsingStatement, UsingKeyword) },
            { Virtual, HashSet(VirtualKeyword) },
            { Void, HashSet(VoidKeyword) },
            { Volatile, HashSet(VolatileKeyword) },
            { When, HashSet(WhenClause, CatchFilterClause, WhenKeyword) },
            { Where, HashSet(WhereClause, WhereKeyword) },
            { While, HashSet(WhileStatement, WhileKeyword) },
            { Yield, HashSet(YieldReturnStatement, YieldBreakStatement, YieldKeyword) }
        };

        private static HashSet<SyntaxKind> HashSet(params SyntaxKind[] kinds) {
            return new HashSet<SyntaxKind>(kinds);
        }

        public IEnumerable<SyntaxNodeOrToken> QueryAll(CSharpSyntaxNode current, CSharpSyntaxQuery query) {
            if (current is CompilationUnitSyntax)
                return current.ChildNodes().SelectMany(c => QueryAll((CSharpSyntaxNode)c, query));

            switch (query.Axis) {
                case SyntaxQueryAxis.Self:
                    if (MatchesIgnoringAxis(current, query))
                        return Enumerable.Repeat((SyntaxNodeOrToken)current, 1);
                    return Enumerable.Empty<SyntaxNodeOrToken>();
                case SyntaxQueryAxis.Child:
                    return QueryAllChildrenOrDescendants(current, query, descendants: false);
                case SyntaxQueryAxis.Descendant:
                    return QueryAllChildrenOrDescendants(current, query, descendants: true);
                default:
                    throw new ArgumentException($"Unsupported query axis: {query.Axis}.", nameof(query));
            }
        }

        private IEnumerable<SyntaxNodeOrToken> QueryAllChildrenOrDescendants(SyntaxNode node, CSharpSyntaxQuery query, bool descendants) {
            foreach (var child in node.ChildNodesAndTokens()) {
                if (MatchesIgnoringAxis(child, query)) {
                    yield return child;
                    continue;
                }

                if (descendants && child.IsNode) {
                    foreach (var descendant in QueryAllChildrenOrDescendants(child.AsNode(), query, descendants: true)) {
                        yield return descendant;
                    }
                }
            }
        }

        private static bool MatchesIgnoringAxis(SyntaxNodeOrToken nodeOrToken, CSharpSyntaxQuery query) {
            var node = nodeOrToken.AsNode();
            if (node is ExpressionStatementSyntax statement && node.Kind() == ExpressionStatement)
                return MatchesIgnoringAxis(statement.Expression, query);

            if (node is SwitchSectionSyntax switchSection) {
                foreach (var label in switchSection.Labels) {
                    if (MatchesSyntaxKind(label.Kind(), query))
                        return true;
                }
            }

            return MatchesSyntaxKind(nodeOrToken.Kind(), query);
        }

        private static bool MatchesSyntaxKind(SyntaxKind kind, CSharpSyntaxQuery query) {
            if (!SyntaxKindsByTarget.TryGetValue(query.Target, out var kinds))
                throw new NotSupportedException($"Unsupported query target: {query.Target}.");
            return kinds.Contains(kind);
        }
    }
}