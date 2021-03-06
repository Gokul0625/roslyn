﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using System.Diagnostics;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp
{
    internal sealed class WhileBinder : LoopBinder
    {
        private readonly StatementSyntax _syntax;

        public WhileBinder(Binder enclosing, StatementSyntax syntax)
            : base(enclosing)
        {
            Debug.Assert(syntax != null && (syntax.IsKind(SyntaxKind.WhileStatement) || syntax.IsKind(SyntaxKind.DoStatement)));
            _syntax = syntax;
        }

        internal override BoundWhileStatement BindWhileParts(DiagnosticBag diagnostics, Binder originalBinder)
        {
            var node = (WhileStatementSyntax)_syntax;

            var condition = originalBinder.BindBooleanExpression(node.Condition, diagnostics);
            var body = originalBinder.BindPossibleEmbeddedStatement(node.Statement, diagnostics);
            Debug.Assert(this.Locals == this.GetDeclaredLocalsForScope(node));
            return new BoundWhileStatement(node, this.Locals, condition, body, this.BreakLabel, this.ContinueLabel);
        }

        internal override BoundDoStatement BindDoParts(DiagnosticBag diagnostics, Binder originalBinder)
        {
            var node = (DoStatementSyntax)_syntax;

            var condition = originalBinder.BindBooleanExpression(node.Condition, diagnostics);
            var body = originalBinder.BindPossibleEmbeddedStatement(node.Statement, diagnostics);
            Debug.Assert(this.Locals == this.GetDeclaredLocalsForScope(node));
            return new BoundDoStatement(node, this.Locals, condition, body, this.BreakLabel, this.ContinueLabel);
        }

        override protected ImmutableArray<LocalSymbol> BuildLocals()
        {
            var locals = ArrayBuilder<LocalSymbol>.GetInstance();
            ExpressionSyntax condition;

            switch (_syntax.Kind())
            {
                case SyntaxKind.WhileStatement:
                    condition = ((WhileStatementSyntax)_syntax).Condition;
                    break;
                case SyntaxKind.DoStatement:
                    condition = ((DoStatementSyntax)_syntax).Condition;
                    break;
                default:
                    throw ExceptionUtilities.UnexpectedValue(_syntax.Kind());
            }

            ExpressionVariableFinder.FindExpressionVariables(this, locals, node: condition);
            return locals.ToImmutableAndFree();
        }

        internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
        {
            if (_syntax == scopeDesignator)
            {
                return this.Locals;
            }

            throw ExceptionUtilities.Unreachable;
        }

        internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
        {
            throw ExceptionUtilities.Unreachable;
        }

        internal override SyntaxNode ScopeDesignator
        {
            get
            {
                return _syntax;
            }
        }
    }
}
