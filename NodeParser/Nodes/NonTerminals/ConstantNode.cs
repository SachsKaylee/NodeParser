using System.Collections.Generic;
using Irony.Parsing;
using NodeParser.Nodes.Terminals;

namespace NodeParser.Nodes.NonTerminals
{
    /// <summary>
    ///     Base class for custom constant, where one string is associated to one value.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    public abstract class ConstantNode<T> : AParserNode<T>
    {
        /// <summary>
        ///     Are the constants from this node keywords?
        /// </summary>
        protected abstract bool AreKeywords { get; }
        
        /// <summary>
        ///     Is the constant node itself transient? If it is you must obtain the constants by using
        ///     <see cref="KeyTermNode{T}" /> instead of <see cref="OperatorNode{T}" />.
        ///     <br />
        ///     While this sounds like a downside(which it is), it is required for things such as precendence to work.
        /// </summary>
        protected virtual bool IsTransient => false;

        /// <inheritdoc />
        protected override BnfExpression Rule_Impl {
            get {
                BnfExpression term = null;
                foreach (Constant constant in GetConstants()) {
                    KeyTermBase<T> keyTerm = CreateConstantKeyTerm(constant);
                    if (term == null) {
                        term = keyTerm;
                    } else {
                        term |= keyTerm;
                    }
                }
                return term ?? Grammar.CurrentGrammar.Empty;
            }
        }

        /// <summary>
        ///     Gets a read only dictionary containing a map of all bnf terms to their mapped term.
        /// </summary>
        /// <returns>The dictionary.</returns>
        protected abstract IEnumerable<Constant> GetConstants();

        /// <inheritdoc />
        public override BnfTerm BuildBnfTerm()
        {
            BnfTerm bnf = base.BuildBnfTerm();
            bnf.SetFlag(TermFlags.IsTransient, IsTransient);
            return bnf;
        }

        /// <summary>
        ///     Creates the key term for the given constant. The default implementation should suffice for almost every case. See
        ///     the default operator node implementation for a cace in which it did not suffice.
        /// </summary>
        /// <param name="constant">The constant to generate the key term from.</param>
        /// <returns>The key term.</returns>
        protected virtual KeyTermBase<T> CreateConstantKeyTerm(Constant constant)
        {
            return AreKeywords
                ? KEYWORD(constant.Text, constant.Value, constant.Name)
                : TERM(constant.Text, constant.Value, constant.Name);
        }

        /// <inheritdoc />
        protected override T BuildAndGetNode(IAstNode[] astNodes)
        {
            return (T) astNodes[0].GetValue();
        }

        /// <summary>
        ///     Represents data about a constant value.
        /// </summary>
        protected class Constant
        {
            /// <summary>
            ///     The name of the constant.
            /// </summary>
            public readonly string Name;

            /// <summary>
            ///     The text string this constant is represented by in user code.
            /// </summary>
            public readonly string Text;

            /// <summary>
            ///     The static value associated with the given text.
            /// </summary>
            public readonly T Value;

            /// <summary>
            ///     Creates a new constant data class.
            /// </summary>
            /// <param name="text">The text string this constant is represented by in user code.</param>
            /// <param name="value">The static value associated with the given text.</param>
            /// <param name="name">The name of the constant. Will use <paramref name="text" /> as name if null.</param>
            public Constant(string text, T value, string name = null)
            {
                Text = text;
                Value = value;
                Name = name ?? text;
            }
        }
    }
}