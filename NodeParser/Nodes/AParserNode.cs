using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Irony.Ast;
using Irony.Parsing;
using NodeParser.Nodes.NonTerminals;
using NodeParser.Nodes.Terminals;
using PSUtility.Enumerables;

namespace NodeParser.Nodes
{
    /// <summary>
    ///     Generic base class for custom defined nodes.
    /// </summary>
    /// <typeparam name="TData">The value type.</typeparam>
    public abstract class AParserNode<TData> : AParserNode, IAstNode<TData>
    {
        private TData m_Value;

        /// <inheritdoc />
        public override string Name => GetType().FullName;

        /*protected TSearchData ValueOfId<T, TSearchData>(string id, TSearchData alt, int searchDepth = 4) where T : AParserNode<TSearchData>
        {
            T d = OfId<T>(id);
            if (d != null) {
                return d.GetValue();
            }
            return alt;
        }*/

        /// <inheritdoc />
        public new TData GetValue()
        {
            return m_Value;
        }

        /// <inheritdoc />
        public sealed override Type GetDataType()
        {
            return typeof(TData);
        }

        /// <inheritdoc />
        protected override object GetValue_Impl()
        {
            return GetValue();
        }

        /// <inheritdoc />
        protected sealed override void BuildNodeImpl(IAstNode[] astNodes)
        {
            m_Value = BuildAndGetNode(astNodes);
        }

        /// <summary>
        ///     Builds the node by passing all <see cref="IAstNode" />s defined in the Rule of the node.
        /// </summary>
        /// <param name="astNodes">The node array.</param>
        /// <returns>The data.</returns>
        protected abstract TData BuildAndGetNode(IAstNode[] astNodes);
    }

    /// <summary>
    ///     Base class for custom defined parser nodes. Consider using the generic <see cref="AParserNode{T}" /> instead.
    /// </summary>
    public abstract class AParserNode : AParserTerminalBase
    {
        [Obsolete]
        private PSDictionary<string, IAstNode> m_IdMap;
        private NodeLocation m_Location;
        private ParseTreeNode m_Node;

        /// <summary>
        ///     Is this parse node the terminal instance? Every parse node class has one instance used as non terminal(the Rule).
        ///     <br />
        ///     All other instances are the ast nodes obtaining the values from the parse noded generated from that ont terminal
        ///     instance.
        ///     <br />
        ///     You typically will never work woth the terminal instance. If required obtain the instance via
        ///     <see cref="ANodeGrammar.GetTerminalNode{T}" />
        /// </summary>
        public bool IsTerminalInstance { get; private set; }

        /// <summary>
        ///     The name of this node.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     The "irony bnf" rule of this node.
        /// </summary>
        protected abstract BnfExpression Rule_Impl { get; }

        /// <summary>
        ///     All ast nodes used as children of this node.
        /// </summary>
        protected ReadOnlyList<IAstNode> ChildrenAstNodes { get; private set; }

        /*/// <summary>
        ///     Gets a lookup for all registered ids.
        /// </summary>
        [Obsolete]
        protected ReadOnlyDictionary<string, IAstNode> IdMap => m_IdMap.AsReadOnly();*/

        /*/// <summary>
        ///     Is the ID lookup built? Automtiucally gets built when trying to get an ast node by its ID.
        /// </summary>
        protected bool HasIdLookup => m_IdMap != null;*/

        /// <inheritdoc />
        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            m_Node = parseNode;
            m_Location = NodeLocation.FromIrony(ANodeGrammar.CurrentGrammar.CurrentFile, parseNode.Span.Location);
            var asts = new PSList<IAstNode>();
            foreach (ParseTreeNode child in parseNode.ChildNodes) {
                IAstNode nodeAst = child.AstNode as IAstNode;
                if (nodeAst == null) {
                    continue;
                }
                asts.Add(nodeAst);
            }
            ChildrenAstNodes = asts.AsReadOnly();
            BuildNodeImpl(ChildrenAstNodes.ToArray());
        }

        /// <summary>
        ///     Creates an object of itself as ast node. Called from the bnf instance.
        /// </summary>
        /// <returns>The ast node.</returns>
        protected virtual IAstNode CreateSelfAstNode()
        {
            return (IAstNode) Activator.CreateInstance(GetType());
        }

        /// <inheritdoc />
        public override BnfTerm BuildBnfTerm()
        {
            IsTerminalInstance = true;
            //m_BnfMapTO = new PSDictionary<BnfTerm, string>();
            //m_IdsTO = new ConditionalWeakTable<IAstNode, string>();
            //m_BufferNonTerminalOnly = new PSDictionary<string, IAstNode>();
            NonTerminalBase nt = new NonTerminalBase(Name) {
                AstConfig = {
                    NodeType = GetType()
                }
            };
            nt.AstConfig.NodeCreator += (context, parseNode) => {
                //Trace.WriteLine(">>> CREATING AST NODE FOR " + GetType());
                AParserNode ast = (AParserNode) parseNode.SetAst(CreateSelfAstNode());
                //Trace.WriteLine("  > " + ast);
                /*foreach (KeyValuePair<string, IAstNode> buffer in m_BufferNonTerminalOnly) {
                    Trace.WriteLine("  > inject: " + buffer);
                    ast.m_IdMap.Add(buffer);
                }
               // m_BufferNonTerminalOnly.Clear();*/
                //Trace.WriteLine("  > OK! init...");
                ast.Init(context, parseNode);
                //Trace.WriteLine("  > OK! done!");
            };
            return nt;
        }

        /// <inheritdoc />
        public override void PostProcessBnfTerm(BnfTerm term)
        {
            //m_IdMapOnlyOnTerminal = new PSDictionary<string, BnfTerm>();
            base.PostProcessBnfTerm(term);
            NonTerminalBase nt = (NonTerminalBase) term;
            nt.Rule = Rule_Impl;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (m_Node != null) {
                return m_Node.ToString();
            }
            return base.ToString();
        }

        /*/// <summary>
        ///     Builds the ID lookup required to access ast nodes by their index.
        /// </summary>
        /// <exception cref="InvalidOperationException">Cannot build the ID lookup; it has already been built.</exception>
        /// <seealso cref="HasIdLookup" />
        [Obsolete]
        protected void BuildIdLookup()
        {
            if (HasIdLookup) {
                throw new InvalidOperationException("Cannot build the ID lookup; it has already been built.");
            }
            m_IdMap = new PSDictionary<string, IAstNode>();
            using (IdContext ctx = new IdContext(m_Node)) {
                BuildIdLookup(ctx, true);
#if DEBUG
                Debug.Assert(ctx.Nodes.Pop() == m_Node);
#endif
            }
        }*/

        /*/// <summary>
        ///     Builds the ID lookup starting at the current node.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="isFirst">
        ///     Is this the first node? If this is true the builder will step into child nodes if the current
        ///     node is a parse node. Which is always the case for first nodes, but this behaviour is what prevents us from endelss
        ///     iteration otherwise.
        /// </param>
        /// <remarks>Warning: Does not validate state.</remarks>
        [Obsolete]
        private void BuildIdLookup(IdContext ctx, bool isFirst)
        {
            NodeParserBnfTag tag;
            NodeParserBnfTag.IdSet ids;
            if (NodeParserBnfTag.TryGetOf(ctx.ActiveNode.Term, out tag) && tag.TryGetIds(GetType(), out ids)) {
                if (ids.Count != 0) {
                    int index = ctx.Get(ctx.ActiveNode.Term);
                    ctx.Set(ctx.ActiveNode.Term, index + 1);
                    string id = ids[index];
                    m_IdMap.Add(id, (IAstNode) ctx.ActiveNode.AstNode);
                }
            }
            // Do not pick up the IDs from other parse nodes.
            if (isFirst || !(ctx.ActiveNode.AstNode is AParserNode)) {
                foreach (ParseTreeNode child in ctx.ActiveNode.ChildNodes) {
                    ctx.Nodes.Push(child);
                    BuildIdLookup(ctx, false);
#if DEBUG
                    Debug.Assert(ctx.Nodes.Pop() == child);
#else
                    ctx.Nodes.Pop();
#endif
                }
            }
        }*/

        /*protected static void BuildIdLookup(ParseTreeNode parse)
        {
            BuildIdLookup(parse, parse.AstNode as AParserNode);
        }

        /// <summary>
        ///     Builds the id lookup for the given parse node and all children.
        /// </summary>
        /// <param name="parse">The active parse node.</param>
        /// <param name="ast">[CanBeNull] The ast node to insert found ids into. Updated automatically.</param>
        protected static void BuildIdLookup(ParseTreeNode parse, AParserNode ast)
        {
            NodeParserTag tag = NodeParserTag.Of(parse);
            if (tag.HasIdMap) {
                return;
            }
            IAstNode astCurrent = parse.AstNode as IAstNode;
            if (ast != null) {
                string id;
                AParserNode tn = (AParserNode) ANodeGrammar.Active.GetTerminalNode(ast.GetType());
                if (astCurrent != null && tn.m_IdsTO.TryGetValue(astCurrent, out id)) {
                    ast.m_IdMap.Add(id, astCurrent);
                }
            }
            AParserNode astNewCastedSafe = astCurrent as AParserNode;
            if (astNewCastedSafe != null) {
                ast = astNewCastedSafe;
            }
            foreach (ParseTreeNode parseTreeNode in parse.ChildNodes) {
                BuildIdLookup(parseTreeNode, ast);
            }
            tag.HasIdMap = true;
        }*/

        /*/// <summary>
        ///     Gets the ast node linked to the given id.
        /// </summary>
        /// <typeparam name="T">The node type.</typeparam>
        /// <param name="id">The node id.</param>
        /// <param name="optional">
        ///     If this is false and no node if found an exception is thrown. If true null returned. Will still
        ///     throw on type mismatch.
        /// </param>
        /// <returns>The ast node or null.</returns>
        /// <exception cref="ArgumentException">No node with the given <paramref name="id" /> exists.</exception>
        [Obsolete("Buggy with | operators. Use manual indexing instead.")]
        protected T OfId<T>(string id, bool optional = false) where T : class, IAstNode
        {
            if (m_Node == null) {
                return null;
            }
            if (!HasIdLookup) {
                //BuildIdLookup(m_Node);
                BuildIdLookup();
            }
            IAstNode node;
            if (!m_IdMap.TryGetValue(id, out node)) {
                if (optional) {
                    return null;
                }
                throw new ArgumentException("No node with the id \"" + id + "\" exists in node \"" + GetType() + "\".");
            }
            return (T) node;
        }*/

        /*/// <summary>
        ///     Context used to build the ID lookup. Required to keep track of how many times a single BnfTerm instance has been
        ///     used(so that we can determine which ID index to use). If this sounds horribly confusing read the
        ///     <see cref="NodeParserBnfTag.GetIds{T}" /> documentation. Or just accept it. I tried to do it another way for
        ///     months. It did not work.
        /// </summary>
        private class IdContext : IDisposable
        {
            /// <summary>
            ///     The counter dictionary keepind track of how many times a single BNF term was used.
            /// </summary>
            private readonly PSDictionary<BnfTerm, int> m_Counter = new PSDictionary<BnfTerm, int>();

            /// <summary>
            ///     Creates a new ID context.
            /// </summary>
            /// <param name="start">The start node.</param>
            public IdContext(ParseTreeNode start)
            {
                Nodes.Push(start);
            }

            /// <summary>
            ///     The currently active parse node.
            /// </summary>
            public Stack<ParseTreeNode> Nodes { get; } = new Stack<ParseTreeNode>();

            /// <summary>
            ///     The currently active node. (Peeks at the top of the stack)
            /// </summary>
            public ParseTreeNode ActiveNode => Nodes.Peek();

            /// <inheritdoc />
            public void Dispose()
            {
                GC.SuppressFinalize(this);
                m_Counter.Clear();
                Nodes.Clear();
            }

            /// <summary>
            ///     Gets how many times the given BnfTerm has been used.
            /// </summary>
            /// <param name="term">The term.</param>
            /// <returns>The amount.</returns>
            public int Get(BnfTerm term)
            {
                int v;
                if (!m_Counter.TryGetValue(term, out v)) {
                    return 0;
                }
                return v;
            }

            /// <summary>
            ///     Sets how many times the given BnfTerm has been used.
            /// </summary>
            /// <param name="term">The term.</param>
            /// <param name="v">The amount.</param>
            public void Set(BnfTerm term, int v)
            {
                m_Counter[term] = v;
            }
        }*/

        //private PSDictionary<BnfTerm, string> m_BnfMapTO;

        /*private ParseTreeNode SearchForID(ParseTreeNode current, string id, int depth)
        {
            if (depth <= 0) {
                return null;
            }
            BnfTerm bnf = current.Term as BnfTerm;
            if (bnf != null && bnf.Id == id) {
                return current;
            }
            foreach (ParseTreeNode currentChildNode in current.ChildNodes) {
                ParseTreeNode found = SearchForID(currentChildNode, id, depth - 1);
                if (found != null) {
                    return found;
                }
            }
            return null;
        }*/

        /*private ParseTreeNode SearchForBnfTermRecursiveOnlyOnTerminal(BnfTerm nt, ParseTreeNode current, int depth)
        {
            if (depth <= 0) {
                return null;
            }
            foreach (ParseTreeNode currentChildNode in current.ChildNodes) {
                if (currentChildNode.Term == nt) {
                    return currentChildNode;
                }
                ParseTreeNode found = SearchForBnfTermRecursiveOnlyOnTerminal(nt, currentChildNode, depth - 1);
                if (found != null) {
                    return found;
                }
            }
            return null;
        }*/

        #region Rule Helper

        // Naming will be CAPITALIZED to mark their constant nature.
        // ReSharper disable InconsistentNaming

        /// <summary>
        ///     Allow an empty node.
        /// </summary>
        protected Terminal EMPTY() => Grammar.CurrentGrammar.Empty;

        /*private static ParseTreeNode FindParentWithNodeType(ParseTreeNode node, Type nodeType, int depth)
        {
            while (depth > 0) {
                if (node.Term.AstConfig.NodeType == nodeType) {
                    return node;
                }
                node = node.Term.;
                depth--;
            }
            return null;
        }*/

        /*/// <summary>
        ///     Sets an id to the given non terminal base, allowing us to easily recover the linked ast node later on.
        /// </summary>
        /// <param name="bnf">The non terminal.</param>
        /// <param name="id">The id.</param>
        /// <returns>The non terminal itself.</returns>
        /// <exception cref="ArgumentException">A BnfTerm with the same id exists already.</exception>
        [Obsolete("Buggy with | operators. Use manual indexing instead.")]
        protected T ID<T>(T bnf, string id) where T : BnfTerm
        {
            NodeParserBnfTag.IdSet ids = NodeParserBnfTag.Of(bnf).GetIds(GetType());
            if (ids.Contains(id)) {
                throw new ArgumentException("A BnfTerm with the same id \"" + id + "\" exists already.");
            }
            ids.Add(id);
            return bnf;
        }*/

        /// <summary>
        ///     Creates braces around a given term.
        /// </summary>
        /// <param name="open">The opening brace.</param>
        /// <param name="inside">The bnf term inside the braces.</param>
        /// <param name="close">The closing brace.</param>
        /// <param name="optional">Are the braces optional?</param>
        /// <returns>The expression.</returns>
        protected NonTerminalBase BRACES(string open, BnfTerm inside, string close, bool optional = false)
        {
            KeyTermBase<string> openS = TERM(open, open, open, TermFlags.IsOpenBrace);
            KeyTermBase<string> closeS = TERM(close, close, close, TermFlags.IsCloseBrace);
            openS.IsPairFor = closeS;
            closeS.IsPairFor = openS;
            BnfExpression expr;
            if (optional) {
                expr = (openS + inside + closeS).TRANS() | inside;
            } else {
                expr = openS + inside + closeS;
            }
            NonTerminalBase ntb = new NonTerminalBase(open + inside.Name + close, expr);
            ntb.SetAst(context => new BraceNode());
            return ntb;
        }

        /// <summary>
        ///     Gets the non terminal for the given node.
        /// </summary>
        /// <param name="id">An optional id. If not null identical to wrapping the call in <see cref="ID{T}" />.</param>
        /// <typeparam name="T">The node type.</typeparam>
        /// <returns>The node non terminal.</returns>
        protected BnfTerm TERMINAL<T>(/*string id = null*/) where T : AParserTerminalBase, new()
        {
            BnfTerm bnf = ANodeGrammar.CurrentGrammar.BnfTermOf<T>();
            /*if (id != null) {
                ID(bnf, id);
            }*/
            return bnf;
        }

        /// <summary>
        ///     Gets the non terminal for the given node.
        /// </summary>
        /// <param name="id">An optional id. If not null identical to wrapping the call in <see cref="ID{T}" />.</param>
        /// <typeparam name="T">The node type.</typeparam>
        /// <returns>The node non terminal.</returns>
        protected NonTerminalBase NODE<T>(/*string id = null*/) where T : AParserNode, new()
        {
            NonTerminalBase nt = (NonTerminalBase) ANodeGrammar.CurrentGrammar.BnfTermOf<T>();
            /*if (id != null) {
                ID(nt, id);
            }*/
            return nt;
        }

        protected KeyTermBase<string> OPERATOR(string text)
        {
            return TERM(text, text, text, TermFlags.IsOperator);
        }

        protected KeyTermBase<T> OPERATOR<T>(string text, T value, string name = null)
        {
            return TERM(text, value, name ?? text, TermFlags.IsOperator);
        }

        /// <summary>
        ///     Creates a reserved keyword for the given text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The key term.</returns>
        protected KeyTermBase<string> KEYWORD(string text)
        {
            return TERM(text, text, text, TermFlags.IsKeyword | TermFlags.IsReservedWord);
        }

        /// <summary>
        ///     Creates a reserved keyword for the given text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value associated with this keyword.</param>
        /// <param name="name">The name of the keyword. Will use <paramref name="text" /> as name if null.</param>
        /// <returns>The key term.</returns>
        protected KeyTermBase<T> KEYWORD<T>(string text, T value, string name = null)
        {
            return TERM(text, value, name ?? text, TermFlags.IsKeyword | TermFlags.IsReservedWord);
        }

        /// <summary>
        ///     Creates punctuation for the given text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The key term.</returns>
        protected KeyTermBase<string> PUNCTUATION(string text)
        {
            return TERM(text, text, text, TermFlags.IsPunctuation);
        }

        /// <summary>
        ///     Creates punctuation for the given text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value associated with this keyword.</param>
        /// <param name="name">The name of the punctuation. Will use <paramref name="text" /> as name if null.</param>
        /// <returns>The key term.</returns>
        protected KeyTermBase<T> PUNCTUATION<T>(string text, T value, string name = null)
        {
            return TERM(text, value, name ?? text, TermFlags.IsPunctuation);
        }

        /// <summary>
        ///     Creates a basic key term for the given text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The key term.</returns>
        protected KeyTermBase<string> TERM(string text)
        {
            return TERM(text, text, text);
        }

        /// <summary>
        ///     Gets a key term for the given text. Only one key term for any given text can exist at the same time.
        /// </summary>
        /// <param name="text">The key term text.</param>
        /// <param name="value">The value associated with this key term.</param>
        /// <param name="name">An optional name.</param>
        /// <param name="flags">The flags on this key term.</param>
        /// <returns>The key term.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text" /> is <see langword="null" /></exception>
        protected KeyTermBase<T> TERM<T>(string text, T value, string name = null, TermFlags flags = TermFlags.None)
        {
            return ANodeGrammar.CurrentGrammar.KeyTermOf(name ?? text, text, value, flags);
        }

        /// <summary>
        ///     Instructs the parser to shift here.
        /// </summary>
        /// <returns>The grammar hint.</returns>
        protected GrammarHint SHIFT()
        {
            return new PreferredActionHint(PreferredActionType.Shift);
        }

        /// <summary>
        ///     Instructs the parser to reduce here.
        /// </summary>
        /// <returns>The grammar hint.</returns>
        protected GrammarHint REDUCE()
        {
            return new PreferredActionHint(PreferredActionType.Reduce);
        }

        protected TokenPreviewHint REDUCE_IF(string thisSymbol, params string[] comesBefore)
        {
            return new TokenPreviewHint(PreferredActionType.Reduce, thisSymbol, comesBefore);
        }

        protected TokenPreviewHint REDUCE_IF(Terminal thisSymbol, params Terminal[] comesBefore)
        {
            return new TokenPreviewHint(PreferredActionType.Reduce, thisSymbol, comesBefore);
        }

        protected TokenPreviewHint SHIFT_IF(string thisSymbol, params string[] comesBefore)
        {
            return new TokenPreviewHint(PreferredActionType.Shift, thisSymbol, comesBefore);
        }

        protected TokenPreviewHint SHIFT_IF(Terminal thisSymbol, params Terminal[] comesBefore)
        {
            return new TokenPreviewHint(PreferredActionType.Shift, thisSymbol, comesBefore);
        }

        protected GrammarHint PRECENDENCE(int precedence, Associativity associativity = Associativity.Left)
        {
            return new ImpliedPrecedenceHint(precedence, associativity);
        }

        protected CustomActionHint CUSTOM_HINT(ExecuteActionMethod executeMethod, PreviewActionMethod previewMethod = null)
        {
            return new CustomActionHint(executeMethod, previewMethod);
        }

        // ReSharper enable InconsistentNaming

        #endregion

        /// <summary>
        ///     Converts this node to a parse tree string, allowing you to visualize the nesting of each node.
        /// </summary>
        /// <returns>The string.</returns>
        public string ToTreeString()
        {
            return NodeParserExtensions.GetTreeString(m_Node);
        }

        /// <summary>
        ///     Callback to process the children of this node.
        /// </summary>
        /// <param name="astNodes">The children. Array alias for <see cref="ChildrenAstNodes" />.</param>
        protected abstract void BuildNodeImpl(IAstNode[] astNodes);

        /// <inheritdoc />
        public override NodeLocation Location => m_Location;

        //private static readonly ConditionalWeakTable<IAstNode, string> s_Ids = new ConditionalWeakTable<IAstNode, string>();
        //private readonly PSDictionary<string, IAstNode> m_IdMap = new PSDictionary<string, IAstNode>();
        //private ConditionalWeakTable<IAstNode, string> m_IdsTO;
        //private PSDictionary<string, IAstNode> m_BufferNonTerminalOnly;
    }
}