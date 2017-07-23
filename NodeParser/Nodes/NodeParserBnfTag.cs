using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Irony.Parsing;
using NodeParser.Nodes.NonTerminals;
using PSUtility.Enumerables;

namespace NodeParser.Nodes
{
    /// <summary>
    ///     A tag attached to the bnf terms.
    /// </summary>
    public class NodeParserBnfTag
    {
        private NonTerminalBase m_Optional;
        private static readonly ConditionalWeakTable<BnfTerm, NodeParserBnfTag> s_Table = new ConditionalWeakTable<BnfTerm, NodeParserBnfTag>();

        /*/// <summary>
        ///     The dictionary of the id lists. Created lazily.
        /// </summary>
        private PSDictionary<Type, IdSet> m_IdDictionary;*/

        private NodeParserBnfTag(BnfTerm term)
        {
            Term = term;
        }

        /// <summary>
        /// Gets the optional variant of this bnf term.
        /// </summary>
        /// <returns>The non terminal.</returns>
        public NonTerminalBase GetOptional()
        {
            if (m_Optional == null)
            {
                m_Optional = new NonTerminalBase(Term.Name + "?", Term | Grammar.CurrentGrammar.Empty);
                m_Optional.SetAst(context => new OptionalNode());
            }
            return m_Optional;
        }

        /// <summary>
        ///     The linked <see cref="BnfTerm" />.
        /// </summary>
        public BnfTerm Term { get; }

        /*/// <summary>
        ///     Gets all IDs assigned to this term in the given node.
        /// </summary>
        /// <typeparam name="T">The node type.</typeparam>
        /// <returns>The ID list.</returns>
        /// <remarks>
        ///     Why can a single BnfTerm have multiple IDs? Rather simple.
        ///     <code>
        /// NODE(Expr, "expr1") + KEYWORD(=) + NODE(Expr, "expr2")
        /// </code>
        ///     In this case we have two nodes of the "Expr" type, thus resulting in two IDENTICAL BNF TERM instances.
        ///     <br />
        ///     Thus we need to assign two different IDs to a single BnfTerm. Keep in mind that the order of the IDs matter. Should
        ///     we assign "expr2"
        ///     first the IDs will be in the wrong order.
        /// </remarks>
        public IdSet GetIds<T>() where T : AParserNode
        {
            return GetIds(typeof(T));
        }

        /// <summary>
        ///     Gets all IDs assigned to this term in the given node.
        /// </summary>
        /// <param name="nodeType">The node type.</param>
        /// <returns>The ID list.</returns>
        /// <remarks>
        ///     Why can a single BnfTerm have multiple IDs? Rather simple.
        ///     <code>
        /// NODE(Expr, "expr1") + KEYWORD(=) + NODE(Expr, "expr2")
        /// </code>
        ///     In this case we have two nodes of the "Expr" type, thus resultin in two IDENTIFICAL BNF TERM instances.
        ///     <br />
        ///     Thus we need to assign two different IDs. Keep in mind that the order of the IDs matter. Should we assign "expr2"
        ///     first the IDs will be in the wrong order.
        /// </remarks>
        public IdSet GetIds(Type nodeType)
        {
            if (m_IdDictionary == null) {
                m_IdDictionary = new PSDictionary<Type, IdSet>();
            }
            IdSet set;
            if (!m_IdDictionary.TryGetValue(nodeType, out set)) {
                m_IdDictionary[nodeType] = set = new IdSet(nodeType);
            }
            return set;
        }

        /// <summary>
        ///     Tries to get the IDs of the given node type. Please read the documentation of the "normal" non-try methods.
        /// </summary>
        /// <param name="nodeType">The node type.</param>
        /// <param name="ids">The ID set. Can be null if none exists.</param>
        /// <returns>true if the ID set was found, false if not.</returns>
        /// <seealso cref="GetIds{T}" />
        public bool TryGetIds(Type nodeType, out IdSet ids)
        {
            if (m_IdDictionary == null) {
                ids = null;
                return false;
            }
            return m_IdDictionary.TryGetValue(nodeType, out ids);
        }*/

        /// <summary>
        ///     Tries to get the tag of the given bnf term.
        /// </summary>
        /// <param name="bnf">The bnf term.</param>
        /// <param name="tag">The tag, null if none exists.</param>
        /// <returns>true if a tag exists, false if not.</returns>
        public static bool TryGetOf(BnfTerm bnf, out NodeParserBnfTag tag)
        {
            return s_Table.TryGetValue(bnf, out tag);
        }

        /// <summary>
        ///     Gets the tag of the given bnf tag. Will created a tag if not tag exists.
        /// </summary>
        /// <param name="bnf">The bnf term.</param>
        /// <returns>The tag.</returns>
        public static NodeParserBnfTag Of(BnfTerm bnf)
        {
            return s_Table.GetValue(bnf, CreateValueCallback);
        }

        private static NodeParserBnfTag CreateValueCallback(BnfTerm key)
        {
            return new NodeParserBnfTag(key);
        }

        /*/// <summary>
        ///     A set containing IDs for a single node type.
        /// </summary>
        public class IdSet : PSList<string>
        {
            /// <summary>
            ///     Creates the ID set.
            /// </summary>
            /// <param name="nodeType">The node type.</param>
            internal IdSet(Type nodeType)
            {
                NodeType = nodeType;
            }

            /// <summary>
            ///     The node type the ID set is for.
            /// </summary>
            public Type NodeType { get; }
        }*/
    }
}