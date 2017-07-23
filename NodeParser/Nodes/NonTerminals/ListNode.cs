using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Irony.Ast;
using Irony.Parsing;
using PSUtility.Enumerables;

namespace NodeParser.Nodes.NonTerminals
{
    /// <summary>
    /// A list of several nodes.
    /// </summary>
    /// <typeparam name="T">The node type.</typeparam>
    public class ListNode<T> : AGenericAstBase<IEnumerable<T>>, IEnumerable<IAstNode<T>>
    {
        private readonly IList<IAstNode<T>> m_AstChildren = new PSList<IAstNode<T>>();
        private NodeLocation m_Location;

        /// <inheritdoc />
        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            m_Location = NodeLocation.FromIrony(ANodeGrammar.CurrentGrammar.CurrentFile, parseNode.Span.Location);
            //Trace.WriteLine("Processing list children: " + parseNode.Term.Name);
            foreach (ParseTreeNode node in parseNode.ChildNodes) {
                //Trace.Write("   ... Child " + node.Term.Name + " has ast " + (node.AstNode ?? "null") + " ... ");
                var castedAst = node.AstNode as IAstNode<T>;
                if (castedAst == null) {
                    //Trace.WriteLine(" ... failed to cast to IAstNode<" + typeof(T) + "> ...  ... skipping.");
                    continue;
                }
                m_AstChildren.Add(castedAst);
                //Trace.WriteLine(" ... casted! ...  ... adding to list.");
            }
        }

        /// <inheritdoc />
        public override IEnumerable<T> GetValue()
        {
            foreach (IAstNode<T> child in m_AstChildren) {
                yield return child.GetValue();
            }
        }

        /// <inheritdoc />
        public override NodeLocation Location => m_Location;

        /// <summary>
        /// The amount of children.
        /// </summary>
        public int Count => m_AstChildren.Count;

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<IAstNode<T>> GetEnumerator()
        {
            return m_AstChildren.GetEnumerator();
        }
    }
}