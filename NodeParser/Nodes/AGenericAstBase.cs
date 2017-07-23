using System;
using Irony.Ast;
using Irony.Parsing;

namespace NodeParser.Nodes
{
    /// <summary>
    /// Base class for implementing your own ast nodes. Using the class instead of the <see cref="IAstNode"/> interface only adds a little by of convenience but is not required.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    public abstract class AGenericAstBase<T> : IAstNode<T>
    {
        /// <inheritdoc />
        public abstract void Init(AstContext context, ParseTreeNode parseNode);

        /// <inheritdoc />
        object IAstNode.GetValue()
        {
            return GetValue();
        }

        /// <inheritdoc />
        public abstract T GetValue();

        /// <inheritdoc />
        public Type GetDataType()
        {
            return typeof(T);
        }

        /// <inheritdoc />
        public abstract NodeLocation Location { get; }
    }
}
