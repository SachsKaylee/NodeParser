using System;
using System.Collections.Generic;
using System.Reflection;
using Irony.Ast;
using Irony.Parsing;

namespace NodeParser.Nodes.NonTerminals
{
    /// <summary>
    /// Base class for non terminals in the node parser.
    /// </summary>
    public class NonTerminalBase : NonTerminal
    {
        protected const string PLUS_CHAR = "+";
        protected const string STAR_CHAR = "*";

        private NonTerminalBase m_Q;
        private NonTerminal m_Star;

        /// <inheritdoc />
        internal NonTerminalBase(string name) : base(name) {}

        /// <inheritdoc />
        internal NonTerminalBase(string name, string errorAlias) : base(name, errorAlias) {}

        /// <inheritdoc />
        internal NonTerminalBase(string name, string errorAlias, Type nodeType) : base(name, errorAlias, nodeType) {}

        /// <inheritdoc />
        internal NonTerminalBase(string name, string errorAlias, AstNodeCreator nodeCreator) : base(name, errorAlias, nodeCreator) {}

        /// <inheritdoc />
        internal NonTerminalBase(string name, Type nodeType) : base(name, nodeType) {}

        /// <inheritdoc />
        internal NonTerminalBase(string name, AstNodeCreator nodeCreator) : base(name, nodeCreator) {}

        /// <inheritdoc />
        internal NonTerminalBase(string name, BnfExpression expression) : base(name, expression) {}

        /// <summary>
        /// Makes the non terminal optional.
        /// </summary>
        /// <returns>The optional non terminal (transient: If you need an ID wrap the Q'ed non terminal in the id call.)</returns>
        public new NonTerminalBase Q()
        {
            if (m_Q != null) {
                return m_Q;
            }
            m_Q = new NonTerminalBase((string.IsNullOrEmpty(Name) ? "UnnamedNonTerminal" : Name) + "?");
            m_Q.Flags = TermFlags.IsTransient;
            m_Q.Rule = this | Grammar.CurrentGrammar.Empty;
            return m_Q;
        }

        public NonTerminalBase LIST<TData>(BnfTerm delimiter, TermListOptions listOptions = TermListOptions.PlusList)
        {
            if (m_Star == null) {
                bool isPlus = !listOptions.IsSet(TermListOptions.AllowEmpty);
                m_Star = new NonTerminalBase(Name + (isPlus ? PLUS_CHAR : STAR_CHAR)) {
                    AstConfig = {
                        NodeType = typeof(ListNode<TData>)
                    }
                };
                ANodeGrammar.CurrentGrammar.MakeListRule(m_Star, delimiter, this, listOptions);
            }
            return (NonTerminalBase)m_Star;
        }

        /*public static BnfExpression operator |(NonTerminalBase term1, BnfTerm term2)
        {
            return Op_Pipe(term1, term2);
        }
        
        public static BnfExpression operator |(BnfTerm term1, NonTerminalBase term2)
        {
            return Op_Pipe(term1, term2);
        }*/

        /*public static BnfExpression operator |(NonTerminalBase term1, NonTerminalBase term2)
        {
            return Op_Pipe(term1, term2);
        }*/

        /*public static BnfExpression operator |(NonTerminalBase term1, TerminalBase term2)
        {
            return Op_Pipe(term1, term2);
        }*/

        /*public static BnfExpression operator +(NonTerminalBase term1, BnfTerm term2)
        {
            return Op_Plus(term1, term2);
        }

        public static BnfExpression operator +(BnfTerm term1, NonTerminalBase term2)
        {
            return Op_Plus(term1, term2);
        }*/

        /*public static BnfExpression operator +(NonTerminalBase term1, NonTerminalBase term2)
        {
            return Op_Plus(term1, term2);
        }*/

        /*public static BnfExpression operator +(NonTerminalBase term1, TerminalBase term2)
        {
            return Op_Plus(term1, term2);
        }*/

        private static readonly FieldInfo m_DataFieldInfo = typeof(BnfExpression).GetField("Data", BindingFlags.Instance | BindingFlags.NonPublic);
        public static List<BnfTermList> DataOf(BnfExpression expr)
        {
            return (List<BnfTermList>) m_DataFieldInfo.GetValue(expr);
        }

        //BNF operations implementation -----------------------
        // Plus/sequence
        internal static BnfExpression Op_Plus(BnfTerm term1, BnfTerm term2)
        {
            //Check term1 and see if we can use it as result, simply adding term2 as operand
            BnfExpression expr1 = term1 as BnfExpression;
            if (expr1 == null || DataOf(expr1).Count > 1) //either not expression at all, or Pipe-type expression (count > 1)
            {
                expr1 = new BnfExpression(term1);
            }
            DataOf(expr1)[DataOf(expr1).Count - 1].Add(term2);
            return expr1;
        }

        //Pipe/Alternative
        //New version proposed by the codeplex user bdaugherty
        internal static BnfExpression Op_Pipe(BnfTerm term1, BnfTerm term2)
        {
            BnfExpression expr1 = term1 as BnfExpression ?? new BnfExpression(term1);
            BnfExpression expr2 = term2 as BnfExpression ?? new BnfExpression(term2);
            DataOf(expr1).AddRange(DataOf(expr2));
            expr1.SetFlag(TermFlags.IsTransient);
            expr1.SetFlag(TermFlags.NoAstNode);
            return expr1;
        }
    }

/*
    public class NonTerminalBase : NonTerminalBase
    {
        /// <inheritdoc />
        public NonTerminalBase(string name) : base(name) {}

        /// <inheritdoc />
        public NonTerminalBase(string name, string errorAlias) : base(name, errorAlias) {}

        /// <inheritdoc />
        public NonTerminalBase(string name, string errorAlias, Type nodeType) : base(name, errorAlias, nodeType) {}

        /// <inheritdoc />
        public NonTerminalBase(string name, string errorAlias, AstNodeCreator nodeCreator) : base(name, errorAlias, nodeCreator) {}

        /// <inheritdoc />
        public NonTerminalBase(string name, Type nodeType) : base(name, nodeType) {}

        /// <inheritdoc />
        public NonTerminalBase(string name, AstNodeCreator nodeCreator) : base(name, nodeCreator) {}

        /// <inheritdoc />
        public NonTerminalBase(string name, BnfExpression expression) : base(name, expression) {}

        private NonTerminalBase m_Q;

        public new NonTerminalBase Q()
        {
            if (m_Q != null)
                return (NonTerminalBase)m_Q;
            m_Q = new NonTerminalBase(this.Name + "?");
            m_Q.Flags = TermFlags.IsTransient;
            m_Q.Rule = this | Grammar.CurrentGrammar.Empty;
            return (NonTerminalBase)m_Q;
        }

        protected const string PLUS_CHAR = "+";
        protected const string STAR_CHAR = "*";

        private NonTerminalBase m_Star;
        public NonTerminalBase LIST(BnfTerm delimiter, TermListOptions listOptions = TermListOptions.PlusList)
        {
            if (m_Star != null)
                return m_Star;
            bool isPlus = !listOptions.IsSet(TermListOptions.AllowEmpty);
            NonTerminalBase list = new NonTerminalBase(Name + (isPlus ? PLUS_CHAR : STAR_CHAR));
            list.AstConfig.NodeType = typeof(ListNode<T>);
            Grammar.MakeListRule(list, delimiter, this, listOptions);
            return list;
        }
    }
*/
}