using System;
using Irony.Ast;
using Irony.Parsing;
using PSUtility.Enumerables;

namespace NodeParser.Nodes
{
    /// <summary>
    ///     A node containing an optional value.
    /// </summary>
    public class OptionalNode : IAstNode
    {
        private IAstNode m_Value;

        /// <summary>
        ///     Is the optional value present?
        /// </summary>
        public bool HasValue => m_Value != null;

        /// <inheritdoc />
        public void Init(AstContext context, ParseTreeNode parseNode)
        {
            Location = NodeLocation.FromIrony(ANodeGrammar.CurrentGrammar.CurrentFile, parseNode.Span.Location);
            if (parseNode.ChildNodes.Count == 0) {
                m_Value = null;
            } else {
                DefaultAst defaultAst;
                if (parseNode.ChildNodes.Count == 1) {
                    m_Value = (IAstNode) parseNode.ChildNodes[0].AstNode;
                    defaultAst = m_Value as DefaultAst;
                } else {
                    m_Value = defaultAst = new DefaultAst();
                    defaultAst.Init(context, parseNode);
                }
                // Try to flatten the ast hierarchy. The flatter everythign is the easier to use by indexing.
                if (defaultAst != null && defaultAst.m_Children.Count == 1) {
                    m_Value = defaultAst.m_Children[0];
                }
            }
        }

        /// <inheritdoc />
        public NodeLocation Location { get; private set; }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Cannot get the value if no value exists.</exception>
        /// <seealso cref="HasValue" />
        public object GetValue()
        {
            if (!HasValue) {
                throw new InvalidOperationException("Cannot get the value if no value exists.");
            }
            return m_Value.GetValue();
        }

        /// <inheritdoc />
        public Type GetDataType()
        {
            if (HasValue) {
                return m_Value.GetDataType();
            }
            return typeof(object);
        }

        /// <summary>
        ///     Gets the value and post processes it. If no value exists a simple replacer value is returned. Shorter method
        ///     signature if the childnode is a <see cref="DefaultAst" /> node.
        /// </summary>
        /// <param name="otherwise">The value to return if no value exists.</param>
        /// <param name="converter">The delegate used to convert the value if one exists.</param>
        /// <returns>The value.</returns>
        /// <typeparam name="TData">The data type after conversion.</typeparam>
        public TData GetValue<TData>(TData otherwise, AstFunc<TData> converter)
        {
            if (HasValue) {
                return converter((ReadOnlyList<IAstNode>) m_Value.GetValue());
            }
            return otherwise;
        }

        /// <summary>
        ///     Gets the value and post processes it. If no value exists a simple replacer value is returned.
        /// </summary>
        /// <param name="otherwise">The value to return if no value exists.</param>
        /// <param name="converter">The delegate used to convert the value if one exists.</param>
        /// <returns>The value.</returns>
        /// <typeparam name="T">The data type. The value of the optional node will be casted without further checks.</typeparam>
        /// <typeparam name="TData">The data type after conversion.</typeparam>
        public TData GetValue<TData, T>(TData otherwise, Func<T, TData> converter)
        {
            if (HasValue) {
                return converter((T) m_Value.GetValue());
            }
            return otherwise;
        }

        /// <summary>
        ///     Gets the value - or a replacer value - of this node.
        /// </summary>
        /// <param name="otherwise">The value to return if no value exists.</param>
        /// <returns>The value.</returns>
        /// <typeparam name="TData">The data type. The value of the optional node will be casted without further checks.</typeparam>
        public TData GetValue<TData>(TData otherwise)
        {
            if (HasValue) {
                return (TData) m_Value.GetValue();
            }
            return otherwise;
        }

        /// <summary>
        ///     Gets the value - or a replacer value - of this node.
        /// </summary>
        /// <param name="otherwise">The value to return if no value exists.</param>
        /// <returns>The value.</returns>
        public object GetValue(object otherwise)
        {
            if (HasValue) {
                return m_Value.GetValue();
            }
            return otherwise;
        }
    }
}