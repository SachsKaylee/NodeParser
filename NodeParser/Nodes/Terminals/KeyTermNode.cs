using System;
using Irony.Ast;
using Irony.Parsing;

namespace NodeParser.Nodes.Terminals
{
    /// <summary>
    ///     Ast nodes used for key terms created by TERM, KEYWORD and PUNCTUATION calls.
    /// </summary>
    /// <typeparam name="T">The term value type.</typeparam>
    public class KeyTermNode<T> : AGenericAstBase<T>
    {
        private NodeLocation m_Location;
        private T m_Value;

        /// <inheritdoc />
        public override NodeLocation Location => m_Location;

        /// <inheritdoc />
        /// <exception cref="ArgumentException">Failed to extract value.</exception>
        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            m_Location = NodeLocation.FromIrony(ANodeGrammar.CurrentGrammar.CurrentFile, parseNode.Span.Location);
            var node = parseNode.Term as KeyTermBase<T>;
            if (node != null) {
                m_Value = node.Value;
            } else if (typeof(T) == typeof(string)) {
                m_Value = (T) (object) parseNode.Token.ValueString;
            } else {
                throw new ArgumentException("Cannot create a key term node of type " + typeof(T) + " from a term of type " + parseNode.Term.GetType() + ".");
            }
        }

        /// <inheritdoc />
        public override T GetValue()
        {
            return m_Value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (typeof(T) == typeof(string) && m_Value != null) {
                return (string)(object)m_Value;
            }
            return base.ToString();
        }
    }
}