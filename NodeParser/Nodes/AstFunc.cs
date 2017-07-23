using PSUtility.Enumerables;

namespace NodeParser.Nodes
{
    /// <summary>
    /// A delegate produsing a result from an input list of ast nodes.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="list">The list.</param>
    /// <returns>The result.</returns>
    public delegate T AstFunc<out T>(ReadOnlyList<IAstNode> list);
}
