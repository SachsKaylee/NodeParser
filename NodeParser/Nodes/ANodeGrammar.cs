using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Irony;
using Irony.Ast;
using Irony.Parsing;
using NodeParser.Exceptions;
using NodeParser.Nodes.Terminals;
using PSUtility.Enumerables;
using PSUtility.Strings;
using Resources = NodeParser.Properties.Resources;

namespace NodeParser.Nodes
{
    /// <summary>
    ///     Base class for all grammars based of the <see cref="AParserNode{T}" /> system.
    /// </summary>
    public abstract class ANodeGrammar : Grammar
    {
        private static readonly MethodInfo s_NonTerminalOfMethod = typeof(ANodeGrammar).GetMethod(nameof(BnfTermOf), BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        ///     All key terms registers in this grammar.
        /// </summary>
        private readonly PSDictionary<string, KeyTerm> m_KeyTerms = new PSDictionary<string, KeyTerm>();

        // The dictionary containing all non terminals.
        private readonly PSDictionary<Type, BnfTerm> m_NodeToBnfTerm = new PSDictionary<Type, BnfTerm>();
        private readonly PSDictionary<Type, AParserTerminalBase> m_TerminalNodes = new PSDictionary<Type, AParserTerminalBase>();
        private Grammar m_Grammar;

        private Parser m_Parser;

        /// <summary>
        ///     The active node grammar.
        /// </summary>
        public new static ANodeGrammar CurrentGrammar => (ANodeGrammar) Grammar.CurrentGrammar;

        /// <summary>
        ///     The currently used file name.
        /// </summary>
        public string CurrentFile { get; internal set; }

        /// <summary>
        ///     Invokes a copy of the protected "MakeListRule" method of Irony.
        /// </summary>
        /// <param name="list">The list non terminal.</param>
        /// <param name="delimiter">The BNF delimiter.</param>
        /// <param name="listMember">The list member.</param>
        /// <param name="options">The list options.</param>
        /// <returns>The list.</returns>
        public new BnfExpression MakeListRule(NonTerminal list, BnfTerm delimiter, BnfTerm listMember, TermListOptions options)
        {
            return base.MakeListRule(list, delimiter, listMember, options);
        }

        /// <summary>
        ///     Yields all comment terminals of this grammar.
        /// </summary>
        /// <returns>The comment terminal enumerable.</returns>
        protected abstract IEnumerable<Terminal> CreateComments();

        /// <summary>
        ///     Gets the terminal node of the given node type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The node.</returns>
        public T GetTerminalNode<T>() where T : AParserTerminalBase, new()
        {
            AParserTerminalBase node;
            if (!m_TerminalNodes.TryGetValue(typeof(T), out node)) {
                node = new T();
                m_TerminalNodes[typeof(T)] = node;
            }
            return (T) node;
        }

        /// <summary>
        ///     Gets the terminal node of the given node type.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>The node.</returns>
        public AParserTerminalBase GetTerminalNode(Type t)
        {
            AParserTerminalBase node;
            if (!m_TerminalNodes.TryGetValue(t, out node)) {
                node = (AParserTerminalBase) Activator.CreateInstance(t);
                m_TerminalNodes[t] = node;
            }
            return node;
        }

        /// <summary>
        ///     The root/entry node of this grammar.
        /// </summary>
        /// <returns>
        ///     The type of the root node. The root node and all its child nodes will be recursively created using their
        ///     default constructor.
        /// </returns>
        protected abstract Type RootNode();

        /// <summary>
        ///     Gets a key term for the given text. Only one key term for any given text can exist at the same time.
        /// </summary>
        /// <param name="text">The key term text.</param>
        /// <param name="name">An optional name.</param>
        /// <param name="value">The value associated with this key term.</param>
        /// <param name="flags">The flags on this term.</param>
        /// <returns>The key term.</returns>
        /// <exception cref="ArgumentNullException">A parameter is <see langword="null" /></exception>
        /// <exception cref="ArgumentException">Multiple terms with the same name but different values exist.</exception>
        internal KeyTermBase<T> KeyTermOf<T>(string name, string text, T value, TermFlags flags)
        {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }
            if (text == null) {
                throw new ArgumentNullException(nameof(text));
            }
            KeyTerm term;
            if (m_KeyTerms.TryGetValue(name, out term)) {
                var casted = (KeyTermBase<T>) term;
                if (casted == null || !Equals(casted.Value, value) || term.Flags != flags) {
                    throw new ArgumentException(Resources.Err_KeyTermConflict.FormatWith(
                        name,
                        term.Text, casted != null ? casted.Value : default(T),
                        term.Flags,
                        text,
                        value,
                        flags));
                }
                return (KeyTermBase<T>) term;
            }
            //create new term
            string.Intern(text);
            term = new KeyTermBase<T>(name, text, value) {
                AstConfig = {
                    NodeType = typeof(KeyTermNode<T>)
                },
                Flags = flags
            };
            term.AstConfig.NodeCreator += (context, node) => node.SetAst(new KeyTermNode<T>()).Init(context, node);
            m_KeyTerms[text] = term;
            return (KeyTermBase<T>) term;
        }

        /// <summary>
        ///     Gets the <see cref="NonTerminal" /> of the node of the given type. If you are creating nodes prefer using the
        ///     wrapper method <see cref="AParserNode.NODE{T}" /> instead.
        /// </summary>
        /// <returns>The non terminal.</returns>
        /// <exception cref="ArgumentException">The given type is not a subclass of AParserNode</exception>
        public BnfTerm BnfTermOf<T>() where T : AParserTerminalBase, new()
        {
            BnfTerm bnf;
            if (!m_NodeToBnfTerm.TryGetValue(typeof(T), out bnf)) {
                T node = GetTerminalNode<T>();
                bnf = node.BuildBnfTerm();
                m_NodeToBnfTerm.Add(typeof(T), bnf);
                node.PostProcessBnfTerm(bnf);
            }
            return bnf;
        }

        /// <summary>
        ///     Instructs the grammar to parse the given tree.
        /// </summary>
        /// <param name="text">The code to parse.</param>
        /// <param name="name">The name used as file name in locations related to this parse process.</param>
        /// <param name="errorLevel">When does a message count as error?</param>
        /// <returns>The root ast node.</returns>
        /// <exception cref="NodeParserParseErrorException">Failed to parse the given input.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="text" /> is <see langword="null" /></exception>
        /// <exception cref="InvalidOperationException">Cannot parse without the grammar being generated.</exception>
        /// <seealso cref="BuildGrammar" />
        public IAstNode Parse(string text, string name, ErrorLevel errorLevel = ErrorLevel.Error)
        {
            if (text == null) {
                throw new ArgumentNullException(nameof(text));
            }
            if (m_Grammar == null || m_Parser == null) {
                throw new InvalidOperationException(Resources.Err_CannotParseWithoutGrammar);
            }
            CurrentFile = name;
            try {
                ParseTree node = m_Parser.Parse(text, CurrentFile);
                if (node.ParserMessages.Count != 0) {
                    int errId = (int) errorLevel;
                    StringBuilder errorBuilder = new StringBuilder();
                    bool error = false;
                    SourceLocation location = SourceLocation.Empty;
                    foreach (LogMessage msg in node.ParserMessages) {
                        errorBuilder.Append("[");
                        errorBuilder.Append(msg.Level);
                        errorBuilder.Append("@");
                        errorBuilder.Append(msg.Location.Line + 1);
                        errorBuilder.Append(":");
                        errorBuilder.Append(msg.Location.Column + 1);
                        errorBuilder.Append("] ");
                        errorBuilder.AppendLine(msg.Message);
                        int levelid = (int) msg.Level;
                        if (levelid >= errId) {
                            location = msg.Location;
                            error = true;
                        }
                    }
                    if (error) {
                        throw new NodeParserParseErrorException(NodeLocation.FromIrony(CurrentFile, location), Resources.Err_FailedToParse.FormatWith(errorBuilder));
                    }
                }
                return (IAstNode) node.Root.AstNode;
            } finally {
                CurrentFile = null;
            }
        }

        /// <summary>
        ///     Builds the grammar. <see cref="RootNode" />
        /// </summary>
        /// <returns>The irony grammar.</returns>
        /// <exception cref="NodeParserGrammarException">Failed to build the grammar.</exception>
        public bool BuildGrammar(GrammarErrorLevel errorLevel = GrammarErrorLevel.Error)
        {
            if (m_Grammar != null && m_Parser != null) {
                return false;
            }
            m_Grammar = this;
            m_Grammar.LanguageFlags |= LanguageFlags.CreateAst;
            // todo: I don't want to provide a non-generic base class for non terminals so we have to do some reflection here.
            m_Grammar.Root = (NonTerminal) s_NonTerminalOfMethod.MakeGenericMethod(RootNode()).Invoke(this, ArrayUtility.Empty<object>());
            foreach (Terminal terminal in CreateComments()) {
                m_Grammar.NonGrammarTerminals.Add(terminal);
            }
            m_Parser = new Parser(m_Grammar);
            if (m_Parser.Language.Errors.Count != 0) {
                int errId = (int) errorLevel;
                StringBuilder errorBuilder = new StringBuilder();
                bool error = false;
                foreach (GrammarError msg in m_Parser.Language.Errors) {
                    errorBuilder.Append("[");
                    errorBuilder.Append(msg.Level);
                    errorBuilder.Append("] ");
                    errorBuilder.AppendLine(msg.Message);
                    int levelid = (int) msg.Level;
                    if (levelid >= errId) {
                        error = true;
                    }
                }
                if (error) {
                    string stateList = ParserDataPrinter.PrintStateList(m_Parser.Language);
                    m_Grammar = null;
                    m_Parser = null;
                    throw new NodeParserGrammarException(Resources.Err_FailedToBuildGrammar.FormatWith(errorBuilder, stateList));
                }
            }
            return true;
        }

        /// <inheritdoc />
        public override void BuildAst(LanguageData language, ParseTree parseTree)
        {
            if (!LanguageFlags.IsSet(LanguageFlags.CreateAst)) {
                return;
            }
            AstContext astContext = new AstContext(language);
            astContext.DefaultNodeType = GetDefaultAstType();
            //astContext.DefaultIdentifierNodeType = m_Node.GetDefaultIdentifierAstType();
            //astContext.DefaultLiteralNodeType = m_Node.GetDefaultLiteralAstType();
            AstBuilder astBuilder = new AstBuilder(astContext);
            astBuilder.BuildAst(parseTree);
        }

        protected virtual Type GetDefaultAstType()
        {
            return typeof(DefaultAst);
        }

        /*public class DefaultLiteral : LiteralNode<string>
        {
            /// <inheritdoc />
            protected override bool TryParse(ParseTreeNode input, out string parsed)
            {
                parsed = (string) input.Token.Value;
                return true;
            }

            /// <inheritdoc />
            protected override BnfTerm CreateTerminal()
            {
                return new StringLiteral("_literal");
            }
        }
        
        /// <inheritdoc />
        public override void BuildAst(LanguageData language, ParseTree parseTree)
        {
            if (!LanguageFlags.IsSet(LanguageFlags.CreateAst))
            {
                return;
            }
            AstContext astContext = new AstContext(language);
            astContext.DefaultNodeType = GetDefaultAstType();
            //astContext.DefaultIdentifierNodeType = m_Node.GetDefaultIdentifierAstType();
            //astContext.DefaultLiteralNodeType = m_Node.GetDefaultLiteralAstType();
            AstBuilder astBuilder = new AstBuilder(astContext);
            astBuilder.BuildAst(parseTree);
        }

        /*#region Nested type: GrammarImpl

        private class GrammarImpl : Grammar
        {
            private readonly ANodeGrammar m_Node;

            public GrammarImpl(ANodeGrammar node)
            {
                m_Node = node;
            }

            /// <inheritdoc />
            public override void BuildAst(LanguageData language, ParseTree parseTree)
            {
                if (!LanguageFlags.IsSet(LanguageFlags.CreateAst)) {
                    return;
                }
                AstContext astContext = new AstContext(language);
                astContext.DefaultNodeType = m_Node.GetDefaultAstType();
                //astContext.DefaultIdentifierNodeType = m_Node.GetDefaultIdentifierAstType();
                //astContext.DefaultLiteralNodeType = m_Node.GetDefaultLiteralAstType();
                AstBuilder astBuilder = new AstBuilder(astContext);
                astBuilder.BuildAst(parseTree);
            }
        }

        #endregion*/
    }
}