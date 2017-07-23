using System;
using Irony.Ast;

namespace NodeParser.Nodes
{
    /// <summary>
    ///     Base class for all ast nodes in the node parser. The parse node recceives an <see cref="Init" /> callback form
    ///     irony.
    /// </summary>
    public interface IAstNode : IAstNodeInit
    {
        /// <summary>
        ///     Gets the source location of this node.
        /// </summary>
        NodeLocation Location { get; }

        /// <summary>
        ///     Gets the value from this parse node. The value should be constructed in the <see cref="Init" /> method.
        /// </summary>
        /// <returns>The value.</returns>
        object GetValue();

        /// <summary>
        ///     The data type of the node.
        /// </summary>
        /// <returns>the type.</returns>
        Type GetDataType();
    }

    /// <summary>
    ///     Base class for all ast nodes in the node parser. The parse node recceives an <see cref="Init" /> callback form
    ///     irony.
    /// </summary>
    public interface IAstNode<out T> : IAstNode
    {
        /// <summary>
        ///     Gets the value from this parse node. The value should be constructed in the <see cref="Init" /> method.
        /// </summary>
        /// <returns>The value.</returns>
        new T GetValue();
    }
}