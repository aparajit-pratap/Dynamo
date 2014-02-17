using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// DesignScript
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using GraphToDSCompiler;

namespace Dynamo.DSEngine
{
    public class DfsTraverseAst : AstCodeBlockTraverse
    {
        public DfsTraverseAst(bool isInstanceMethod)
        {
            this.isInstanceMethod = isInstanceMethod;
        }

        private bool isInstanceMethod;
        private List<AssociativeNode> leafNodes = new List<AssociativeNode>();
        public List<AssociativeNode> LeafNode
        {
            get
            {
                return leafNodes;
            }
        }

        protected override void EmitIdentifierNode(ref ProtoCore.AST.AssociativeAST.AssociativeNode identNode)
        {

            ProtoCore.AST.AssociativeAST.IdentifierNode iNode = identNode as ProtoCore.AST.AssociativeAST.IdentifierNode;
            if (iNode == null)
                throw new ArgumentNullException("identNode");

            //leafNodes.Add(identNode);
        }

        protected override void EmitIntNode(ref ProtoCore.AST.AssociativeAST.IntNode intNode)
        {
            if (intNode == null)
                throw new ArgumentNullException("intNode");

            //leafNodes.Add(intNode);
        }

        protected override void EmitDoubleNode(ref ProtoCore.AST.AssociativeAST.DoubleNode doubleNode)
        {
            if (doubleNode == null)
                throw new ArgumentNullException("doubleNode");
            
            //leafNodes.Add(doubleNode);
        }

        protected override void EmitNullNode(ref ProtoCore.AST.AssociativeAST.NullNode nullNode)
        {
            if (nullNode == null)
                throw new ArgumentNullException("nullNode");
            
            //leafNodes.Add(nullNode);
        }

        protected override void EmitFunctionDotCallNode(ref ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall)
        {
            if (dotCall == null)
                throw new ArgumentNullException("dotCall");

            ProtoCore.AST.AssociativeAST.AssociativeNode identNode = dotCall.DotCall.FormalArguments[0];
            if(isInstanceMethod)
                leafNodes.Add(identNode);

            ProtoCore.AST.AssociativeAST.FunctionCallNode funcCall = dotCall.FunctionCall;
            EmitFunctionCallNode(ref funcCall);
        }

        protected override void EmitFunctionCallNode(ref ProtoCore.AST.AssociativeAST.FunctionCallNode funcCallNode)
        {
            if (funcCallNode == null)
                throw new ArgumentNullException("funcCallNode");

            string functionName;
            if (funcCallNode.Function is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                functionName = (funcCallNode.Function as ProtoCore.AST.AssociativeAST.IdentifierNode).Value;
            }
            else
                throw new Exception("FunctionCallNode.Function must be of type IdentifierNode");

            for (int n = 0; n < funcCallNode.FormalArguments.Count; ++n)
            {
                ProtoCore.AST.AssociativeAST.AssociativeNode argNode = funcCallNode.FormalArguments[n];
                leafNodes.Add(argNode);
            }
        }
    }
}
