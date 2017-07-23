using System;
using Irony.Ast;
using Irony.Parsing;
using NodeParser.Nodes.Terminals;

namespace NodeParser.Nodes
{
    /// <summary>
    ///     A node representing the result of the BRACES operation.
    /// </summary>
    public class BraceNode : IAstNode
    {
        /// <summary>
        ///     The nested ast node.
        /// </summary>
        private IAstNode m_Nested;

        /// <summary>
        ///     The symbol used to open the braces. Can be null if the braces were optional.
        /// </summary>
        public string OpenSymbol { get; private set; }

        /// <summary>
        ///     The symbol used to close the braces. Can be null if the braces were optional.
        /// </summary>
        public string CloseSymbol { get; private set; }

        /// <inheritdoc />
        public NodeLocation Location { get; private set; }

        /// <inheritdoc />
        public void Init(AstContext context, ParseTreeNode parseNode)
        {
            Location = NodeLocation.FromIrony(ANodeGrammar.CurrentGrammar.CurrentFile, parseNode.Span.Location);
            ParseTreeNodeList children = parseNode.ChildNodes;
            DefaultAst defaultAst;
            // We could still have three children if we are nesting a node whith three elements.
            if (children.Count == 3 && (children[0].Term.Flags & TermFlags.IsOpenBrace) == TermFlags.IsOpenBrace
                && (children[2].Term.Flags & TermFlags.IsCloseBrace) == TermFlags.IsCloseBrace) {
                OpenSymbol = ((KeyTermNode<string>) children[0].AstNode).GetValue();
                CloseSymbol = ((KeyTermNode<string>) children[2].AstNode).GetValue();
                m_Nested = (IAstNode) children[1].AstNode;
                defaultAst = m_Nested as DefaultAst;
            } else {
                m_Nested = defaultAst = new DefaultAst();
                defaultAst.Init(context, parseNode);
                /*DefaultAst fakeAst = new DefaultAst {
                    m_Location = Location,
                    m_Children = new PSList<IAstNode>(children.Where(c => c.AstNode is IAstNode).Select(c => (IAstNode) c.AstNode))
                };*/
            }
            if (defaultAst != null && defaultAst.m_Children.Count == 1) {
                m_Nested = defaultAst.m_Children[0];
            }
        }

        /// <inheritdoc />
        public object GetValue()
        {
            return m_Nested.GetValue();
        }

        /// <inheritdoc />
        public Type GetDataType()
        {
            return m_Nested.GetDataType();
        }
    }
}