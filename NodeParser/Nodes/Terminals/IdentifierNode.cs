using System;
using Irony.Ast;
using Irony.Parsing;

namespace NodeParser.Nodes.Terminals
{
    /// <summary>
    ///     Premade identifier node.
    /// </summary>
    public class IdentifierNode : AParserTerminalBase
    {
        private NodeLocation m_Location;
        private string m_Symbol;

        /// <summary>
        ///     The case restriction of the identifier.
        /// </summary>
        protected virtual CaseRestriction CaseRestriction => CaseRestriction.None;

        /// <inheritdoc />
        public override NodeLocation Location => m_Location;

        /// <inheritdoc />
        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            m_Location = NodeLocation.FromIrony(ANodeGrammar.CurrentGrammar.CurrentFile, parseNode.Span.Location);
            m_Symbol = parseNode.Token.ValueString;
        }

        /// <summary>
        ///     Gets the string representing this identifier.
        /// </summary>
        /// <returns>The string.</returns>
        public new string GetValue()
        {
            return m_Symbol;
        }

        /// <inheritdoc />
        protected override object GetValue_Impl()
        {
            return m_Symbol;
        }

        /// <inheritdoc />
        public override Type GetDataType()
        {
            return typeof(string);
        }

        /// <inheritdoc />
        public override BnfTerm BuildBnfTerm()
        {
            IdentifierTerminal id = new IdentifierTerminal("_identifier") {
                AstConfig = {
                    NodeType = typeof(IdentifierNode),
                    NodeCreator = delegate(AstContext context, ParseTreeNode node) {
                        IdentifierNode ast = new IdentifierNode();
                        node.AstNode = ast;
                        ast.Init(context, node);
                    }
                },
                CaseRestriction = CaseRestriction
            };
            return id;
        }
    }
}