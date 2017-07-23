using System.Diagnostics;
using Irony.Ast;
using Irony.Parsing;
using PSUtility.Enumerables;

namespace NodeParser.Nodes
{
    /// <summary>
    ///     The default ast tree node for expressions which are not directly represented by a node.
    /// </summary>
    public class DefaultAst : AGenericAstBase<ReadOnlyList<IAstNode>>
    {
        internal PSList<IAstNode> m_Children;
        internal NodeLocation m_Location;

        /// <inheritdoc />
        public override NodeLocation Location => m_Location;

        /// <summary>
        ///     Gets the child node at the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The child node.</returns>
        public IAstNode this[int index] => m_Children[index];

        /// <inheritdoc />
        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            m_Location = NodeLocation.FromIrony(ANodeGrammar.CurrentGrammar.CurrentFile, parseNode.Span.Location);
            m_Children = new PSList<IAstNode>();
            ParseTreeNodeList cnodes = parseNode.ChildNodes;
            for (int i = 0; i < cnodes.Count; i++) {
                IAstNode ast = cnodes[i].AstNode as IAstNode;
                if (ast != null) {
                    m_Children.Add(ast);
                } else {
                    Trace.WriteLine("[NODE PARSER] Child node#" + i + " \"" + cnodes[i] + "\" of \"" + parseNode + "\" is not as ast node. Skipping ...");
                }
            }
            // Avoid nested DefaultAst
            if (m_Children.Count == 1 && m_Children[0] is DefaultAst) {
                DefaultAst nestedAst = (DefaultAst) m_Children[0];
                m_Children = nestedAst.m_Children;
            }
        }

        /// <inheritdoc />
        public override ReadOnlyList<IAstNode> GetValue()
        {
            return m_Children.AsReadOnly();
        }
    }
}