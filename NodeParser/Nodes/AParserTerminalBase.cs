using System;
using Irony.Ast;
using Irony.Parsing;

namespace NodeParser.Nodes
{
    /// <summary>
    ///     Base class for custom (non)terminals in the node parser.
    ///     <br />
    ///     You typically wish to use <see cref="AParserNode{TData}" /> or <see cref="AParserNode" /> instead for non
    ///     terminals as they are more derived and easier to use.
    ///     <br />
    ///     If you wish to create bog-basic terminals, this is your place.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public abstract class AParserTerminalBase<T> : AParserTerminalBase
    {
        /// <summary>
        ///     Gets the value of the ast node. The value should be create din the Init method.
        /// </summary>
        /// <returns>The value.</returns>
        protected abstract T GetValue_GenericImpl();

        /// <inheritdoc />
        protected override object GetValue_Impl()
        {
            return GetValue_GenericImpl();
        }

        /// <summary>
        ///     Gets the value of the ast node. The value should be create din the Init method.
        /// </summary>
        /// <returns>The value.</returns>
        public new T GetValue()
        {
            return GetValue_GenericImpl();
        }

        /// <inheritdoc />
        public override Type GetDataType()
        {
            return typeof(T);
        }
    }

    /// <summary>
    ///     Base class for custom (non)terminals in the node parser.
    ///     <br />
    ///     You typically wish to use <see cref="AParserNode{TData}" /> or <see cref="AParserNode" /> instead for non
    ///     terminals as they are more derived and easier to use.
    ///     <br />
    ///     If you wish to create bog-basic terminals, this is your place.
    /// </summary>
    public abstract class AParserTerminalBase : IAstNode
    {
        /// <inheritdoc />
        public abstract void Init(AstContext context, ParseTreeNode parseNode);

        /// <inheritdoc />
        public abstract NodeLocation Location { get; }

        /// <inheritdoc />
        public object GetValue()
        {
            return GetValue_Impl();
        }

        /// <inheritdoc />
        public abstract Type GetDataType();

        /// <summary>
        ///     Gets the value of the ast node. The value should be create din the <see cref="Init" /> method.
        /// </summary>
        /// <returns>The value.</returns>
        protected abstract object GetValue_Impl();

        /// <summary>
        ///     Creates the term for this terminal.
        /// </summary>
        /// <returns>The term.</returns>
        public abstract BnfTerm BuildBnfTerm();

        /// <summary>
        ///     Called after the bnf term has been created by <see cref="BuildBnfTerm" /> and after the bnf term has been
        ///     registered in the cache. Use this if you experience stack overflow problems while creating the bnf terms.
        /// </summary>
        public virtual void PostProcessBnfTerm(BnfTerm term) {}
    }
}