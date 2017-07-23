using System;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using NodeParser.Nodes;
using NodeParser.Nodes.NonTerminals;
using NodeParser.Properties;
using PSUtility.Enumerables;

namespace NodeParser
{
    /// <summary>
    ///     Extension methods for the node parser.
    /// </summary>
    public static class NodeParserExtensions
    {
        /// <summary>
        ///     This delegate creates a node without assigning it.
        /// </summary>
        /// <typeparam name="T">The node type.</typeparam>
        /// <param name="context">The active ast context.</param>
        /// <returns>The node.</returns>
        public delegate T NodeParserAstCreator<out T>(AstContext context) where T : IAstNode;

        public delegate bool NodeSelectorPredicate(IAstNode node, int index);

        // ReSharper disable once ExceptionNotThrown
        /// <summary>
        ///     Tries to find a node matching the given prediate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="selector"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="array" /> is <see langword="null" /></exception>
        /// <exception cref="ArgumentNullException"><paramref name="selector" /> is <see langword="null" /></exception>
        /// <exception cref="InvalidCastException">Cannot cast the element to <typeparamref name="T" />.</exception>
        public static T NodeValue<T>(this IAstNode[] array, NodeSelectorPredicate selector, T defValue)
        {
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }
            if (selector == null) {
                throw new ArgumentNullException(nameof(selector));
            }
            for (int i = 0; i < array.Length; i++) {
                IAstNode astNode = array[i];
                if (selector(astNode, i)) {
                    return (T) astNode.GetValue();
                }
            }
            return defValue;
        }

        /// <summary>
        ///     Gets the value at the given list index; or; if the list is not long enough a replacement value.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="index">The index.</param>
        /// <param name="otherwise">Tbe replacement value.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The index is smaller than 0.</exception>
        public static T At<T>(this ReadOnlyList<IAstNode> list, int index, T otherwise)
        {
            if (index < 0) {
                throw new ArgumentOutOfRangeException(nameof(index), index, Resources.Err_IndexSmallerThanZero);
            }
            if (list.Count <= index) {
                return otherwise;
            }
            return list[index].GetValue<T>();
        }

        /// <summary>
        ///     Casts the ast node. This syntax is supported to avoid massivley nested casts for ast nodes.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="ast">The ast node to case.</param>
        /// <returns>The casted node.</returns>
        public static T As<T>(this IAstNode ast) where T : IAstNode
        {
            return (T) ast;
        }

        /// <summary>
        ///     Makes the non terminal optional.
        /// </summary>
        /// <returns>The optional non terminal (transient: If you need an ID wrap the Q'ed non terminal in the id call.)</returns>
        public static NonTerminalBase OPT(this BnfTerm bnf)
        {
            NodeParserBnfTag tag = NodeParserBnfTag.Of(bnf);
            return tag.GetOptional();
        }

        /// <summary>
        ///     Gets a value of an ast node and casts it. Helper syntax provided to prevent stacked casts in the scanning part.
        /// </summary>
        /// <typeparam name="T">The ast node type.</typeparam>
        /// <param name="node">The node.</param>
        /// <returns>The casted node.</returns>
        public static T GetValue<T>(this IAstNode node)
        {
            return (T) node.GetValue();
        }

        /// <summary>
        ///     Marks a bnf term as transient.
        /// </summary>
        /// <typeparam name="T">The term type.</typeparam>
        /// <param name="this">The bnf term to make transient.</param>
        /// <param name="isTransient">If true the transient flag will be set, if false unset.</param>
        /// <returns>The bnf term.</returns>
        public static T TRANS<T>(this T @this, bool isTransient = true) where T : BnfTerm
        {
            @this.SetFlag(TermFlags.IsTransient, isTransient);
            return @this;
        }

        public static NonTerminalBase LIST<TData>(this NonTerminalBase @this, BnfTerm delimiter = null, TermListOptions listOptions = TermListOptions.PlusList)
        {
            bool isPlus = !listOptions.IsSet(TermListOptions.AllowEmpty);
            NonTerminalBase m_List = new NonTerminalBase(@this.Name + (isPlus ? "+" : "*")).SetAst(context => new ListNode<TData>());
            ANodeGrammar.CurrentGrammar.MakeListRule(m_List, delimiter, @this, listOptions);
            return m_List;
        }

        public static NonTerminalBase LIST<TData>(this BnfExpression @this, BnfTerm delimiter = null, TermListOptions listOptions = TermListOptions.PlusList)
        {
            bool isPlus = !listOptions.IsSet(TermListOptions.AllowEmpty);
            NonTerminalBase m_List = new NonTerminalBase(@this.Name + (isPlus ? "+" : "*")).SetAst(context => new ListNode<TData>());
            ANodeGrammar.CurrentGrammar.MakeListRule(m_List, delimiter, @this, listOptions);
            return m_List;
        }

        /// <summary>
        ///     Gets the node at the the given index.
        /// </summary>
        /// <typeparam name="T">The node type.</typeparam>
        /// <param name="array">The ast node array.</param>
        /// <param name="index">The index.</param>
        /// <returns>The node.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="array" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index" /> is out of range.</exception>
        // ReSharper disable once ExceptionNotThrown
        /// <exception cref="InvalidCastException">Cannot cast the element to <typeparamref name="T" />.</exception>
        public static T NodeAt<T>(this IAstNode[] array, int index) where T : IAstNode
        {
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }
            if (array.Length < index || index < 0) {
                throw new ArgumentOutOfRangeException(nameof(index), index, string.Format(Resources.Err_InvalidIndex, array.Length, index));
            }
            IAstNode node = array[index];
            return (T) node;
        }

        /// <summary>
        ///     Sets the ast node of the given parse tree node.
        /// </summary>
        /// <typeparam name="T">The node type.</typeparam>
        /// <param name="parseNode">The parse node.</param>
        /// <param name="astNode">The ast node.</param>
        /// <returns>The ast node.</returns>
        public static T SetAst<T>(this ParseTreeNode parseNode, T astNode) where T : IAstNode
        {
            // This extension method exists as a "one-liner" to support in line delegates where 
            // setting the ast is required. Also gives a little bit of additional type-safety.
            parseNode.AstNode = astNode;
            return astNode;
        }

        /// <summary>
        ///     Sets the ast node creator for the given bnf term.
        /// </summary>
        /// <typeparam name="T">The bnf term type.</typeparam>
        /// <typeparam name="TNode">The node type.</typeparam>
        /// <param name="bnfTerm">The bnf term.</param>
        /// <param name="creator">The node creator delegate.</param>
        /// <returns>The bnf term.</returns>
        public static T SetAst<T, TNode>(this T bnfTerm, NodeParserAstCreator<TNode> creator) where T : BnfTerm where TNode : IAstNode
        {
            bnfTerm.AstConfig.NodeType = typeof(TNode);
            bnfTerm.AstConfig.NodeCreator += delegate(AstContext context, ParseTreeNode node) { node.SetAst(creator(context)).Init(context, node); };
            return bnfTerm;
        }

        /// <summary>
        ///     Specifies that no ast node should be generated for the given bnf term.
        /// </summary>
        /// <param name="bnfTerm">The bnf term.</param>
        /// <returns>The non terminal.</returns>
        // ReSharper disable once InconsistentNaming
        public static NonTerminal NOAST(this BnfTerm bnfTerm)
        {
            NonTerminal noAst = new NonTerminal(bnfTerm.Name + "(No AST)") {
                Flags = TermFlags.NoAstNode,
                Rule = new BnfExpression(bnfTerm)
            };
            return noAst;
        }

        public static string GetTreeString(ParseTreeNode node)
        {
            StringBuilder builder = new StringBuilder();
            GetTreeString(node, builder, 0);
            return builder.ToString();
        }

        private static void GetTreeString(ParseTreeNode node, StringBuilder target, int level)
        {
            string indent = new string(' ', level);
            target.Append(indent).Append("\"").Append(node.Term.Name).Append("\" = ");
            if (node.ChildNodes.Count == 0) {
                target.Append("\"");
                target.Append(node.Token?.ValueString ?? "<null token>");
                target.AppendLine("\",");
            } else {
                target.AppendLine("{");
                foreach (ParseTreeNode childNode in node.ChildNodes) {
                    GetTreeString(childNode, target, level + 1);
                }
                target.Append(indent).AppendLine("},");
            }
        }
    }
}