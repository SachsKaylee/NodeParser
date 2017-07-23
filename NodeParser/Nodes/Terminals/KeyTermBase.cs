using Irony.Parsing;
using NodeParser.Nodes.NonTerminals;

namespace NodeParser.Nodes.Terminals
{
    /// <summary>
    ///     Key terms used in the node parser. Their respective ast nodes is <see cref="KeyTermNode{T}" />.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public class KeyTermBase<T> : KeyTerm
    {
        protected const string PLUS_CHAR = "+";
        protected const string STAR_CHAR = "*";

        private NonTerminalBase m_List;
        private NonTerminalBase m_Q;

        /// <inheritdoc />
        public KeyTermBase(string name, string text, T value) : base(text, name)
        {
            Value = value;
        }

        /// <summary>
        ///     The value associated with this key term.
        /// </summary>
        public T Value { get; }

        public new NonTerminalBase Q()
        {
            if (m_Q != null) {
                return m_Q;
            }
            m_Q = new NonTerminalBase((string.IsNullOrEmpty(Name) ? "UnnamedKeyTerm" : Name) + "?");
            m_Q.Flags = TermFlags.IsTransient;
            m_Q.Rule = this | Grammar.CurrentGrammar.Empty;
            return m_Q;
        }

        public NonTerminalBase LIST(BnfTerm delimiter = null, TermListOptions listOptions = TermListOptions.PlusList)
        {
            if (m_List != null) {
                return m_List;
            }
            bool isPlus = !listOptions.IsSet(TermListOptions.AllowEmpty);
            m_List = new NonTerminalBase(Name + (isPlus ? PLUS_CHAR : STAR_CHAR)).SetAst(context => new ListNode<T>());
            ANodeGrammar.CurrentGrammar.MakeListRule(m_List, delimiter, this, listOptions);
            return m_List;
        }

        public static BnfExpression operator |(KeyTermBase<T> term1, BnfTerm term2)
        {
            return NonTerminalBase.Op_Pipe(term1, term2);
        }

        public static BnfExpression operator |(KeyTermBase<T> term1, NonTerminalBase term2)
        {
            return NonTerminalBase.Op_Pipe(term1, term2);
        }

        public static BnfExpression operator |(BnfTerm term1, KeyTermBase<T> term2)
        {
            return NonTerminalBase.Op_Pipe(term1, term2);
        }

        public static BnfExpression operator |(KeyTermBase<T> term1, KeyTermBase<T> term2)
        {
            return NonTerminalBase.Op_Pipe(term1, term2);
        }

        public static BnfExpression operator |(NonTerminalBase term1, KeyTermBase<T> term2)
        {
            return NonTerminalBase.Op_Pipe(term1, term2);
        }
        
        public static BnfExpression operator +(NonTerminalBase term1, KeyTermBase<T> term2)
        {
            return NonTerminalBase.Op_Plus(term1, term2);
        }

        public static BnfExpression operator +(KeyTermBase<T> term1, BnfTerm term2)
        {
            return NonTerminalBase.Op_Plus(term1, term2);
        }

        public static BnfExpression operator +(KeyTermBase<T> term1, NonTerminalBase term2)
        {
            return NonTerminalBase.Op_Plus(term1, term2);
        }

        public static BnfExpression operator +(BnfTerm term1, KeyTermBase<T> term2)
        {
            return NonTerminalBase.Op_Plus(term1, term2);
        }

        public static BnfExpression operator +(KeyTermBase<T> term1, KeyTermBase<T> term2)
        {
            return NonTerminalBase.Op_Plus(term1, term2);
        }
    }
}