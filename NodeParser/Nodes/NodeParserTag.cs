using Irony.Parsing;

namespace NodeParser.Nodes
{
    /// <summary>
    ///     The tag assigned to parse tree nodes by the node parser.
    /// </summary>
    public class NodeParserTag
    {
        /// <summary>
        ///     Does this parse tree node have an Id map?
        /// </summary>
        public bool HasIdMap;
        
        private NodeParserTag() {}

        /// <summary>
        ///     Gets; or created if it does not exist; the tag of the given parse node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The tag.</returns>
        public static NodeParserTag Of(ParseTreeNode node)
        {
            NodeParserTag tag = node.Tag as NodeParserTag;
            if (tag == null) {
                node.Tag = tag = new NodeParserTag();
            }
            return tag;
        }
    }
}