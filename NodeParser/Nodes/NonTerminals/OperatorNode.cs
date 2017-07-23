using System.Collections.Generic;
using Irony.Parsing;
using NodeParser.Nodes.Terminals;

namespace NodeParser.Nodes.NonTerminals
{
    /// <summary>
    ///     Base class for creating custom operators in the node parser.
    ///     <br />
    ///     All operators are transient by default. This is required for precendence to work.
    /// </summary>
    /// <typeparam name="T">The type linked to each operator.</typeparam>
    public abstract class OperatorNode<T> : ConstantNode<T>
    {
        /// <summary>
        ///     This property is meaningless in operators as it overrides the keyterm generation.
        /// </summary>
        protected sealed override bool AreKeywords => false;

        /// <summary>
        ///     All operators are transient by default. This is required for precendence to work.
        /// </summary>
        protected override bool IsTransient => true;

        /// <summary>
        ///     Gets all operators for this node type.
        /// </summary>
        /// <returns>The operators.</returns>
        protected abstract IEnumerable<Operator> GetOperators();

        /// <inheritdoc />
        protected override IEnumerable<Constant> GetConstants()
        {
            foreach (Operator op in GetOperators()) {
                yield return op;
            }
        }

        /// <inheritdoc />
        protected override KeyTermBase<T> CreateConstantKeyTerm(Constant constant)
        {
            Operator op = (Operator) constant;
            KeyTermBase<T> opSymbol = OPERATOR(op.Text, op.Value, op.Name);
            opSymbol.Precedence = op.Precendence;
            opSymbol.Associativity = op.Associativity;
            return opSymbol;
        }

        /*/// <inheritdoc />
        protected override KeyTerm CreateConstantKeyTerm(string text)
        {
            Operator op = ConstantOf(text) as Operator;
            if (op == null) {
                throw new NotSupportedException("Each operator node element must be a non-null class of the Operator class.");
            }
            KeyTermBase<T> opSymbol = TERM(op.Text, op.Value);
            // Is this key term already an operator? (Irony overrides, we validate and then potentially throw)
            if ((opSymbol.Flags & TermFlags.IsOperator) == TermFlags.IsOperator) {
                if (opSymbol.Precedence != op.Precendence || opSymbol.Associativity != op.Associativity) {
                    throw new InvalidOperationException("The operator \"" + text + "\" was registered multiple times with different attributes: Registry 1: prec: " + opSymbol.Precedence + ", assoc: " +
                                                        opSymbol.Associativity + " // Registry 2: prec: " + op.Precendence + ", assoc: " + op.Associativity +
                                                        " - Operator attributes must be consitent if reused.");
                }
            } else {
                // Copy Pasta from Irony/Grammar.cs/129
                opSymbol.SetFlag(TermFlags.IsOperator);
                opSymbol.Precedence = op.Precendence;
                opSymbol.Associativity = op.Associativity;
            }
            return opSymbol;
        }*/

        /// <summary>
        ///     Represents data about an operator.
        /// </summary>
        protected class Operator : Constant
        {
            /// <summary>
            ///     Should the operator be left, right or neutrally associated?
            /// </summary>
            public readonly Associativity Associativity;

            /// <summary>
            ///     The predencene of this operator. Operators with a higher value take priority over the ones with a lower one.
            /// </summary>
            public readonly int Precendence;

            /// <inheritdoc />
            public Operator(string text, T value, int precendence, Associativity associativity, string name = null) : base(text, value, name)
            {
                Precendence = precendence;
                Associativity = associativity;
            }
        }
    }
}