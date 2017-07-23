using System;
using Irony.Ast;
using Irony.Parsing;

namespace NodeParser.Nodes.Terminals
{
    /// <summary>
    ///     Represents a literal.
    /// </summary>
    public abstract class LiteralNode<T> : AParserTerminalBase
    {
        private NodeLocation m_Location;

        private T m_Value;

        /// <inheritdoc />
        public override NodeLocation Location => m_Location;

        /// <summary>
        ///     Tries to parse the given literal.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="parsed"></param>
        /// <returns></returns>
        protected abstract bool TryParse(ParseTreeNode input, out T parsed);

        /// <inheritdoc />
        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            m_Location = NodeLocation.FromIrony(ANodeGrammar.CurrentGrammar.CurrentFile, parseNode.Span.Location);
            if (!TryParse(parseNode, out m_Value)) {
                throw new FormatException("Failed to parse value of literal " + GetType() + ": " + parseNode.Token.ValueString);
            }
        }

        /// <summary>
        ///     Gets the literal value associated with this ast node.
        /// </summary>
        /// <returns>The value.</returns>
        public new T GetValue()
        {
            return m_Value;
        }

        /// <summary>
        ///     Creates the terminal without setting up and ast stuff.
        /// </summary>
        /// <returns>The bnf term.</returns>
        protected abstract BnfTerm CreateTerminal();

        /// <inheritdoc />
        public override BnfTerm BuildBnfTerm()
        {
            BnfTerm terminal = CreateTerminal();
            terminal.SetAst(context => (IAstNode) Activator.CreateInstance(GetType()));
            terminal.SetFlag(TermFlags.IsLiteral);
            return terminal;
        }

        /// <inheritdoc />
        protected override object GetValue_Impl()
        {
            return m_Value;
        }

        /// <inheritdoc />
        public override Type GetDataType()
        {
            return typeof(T);
        }
    }
}