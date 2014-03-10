using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// DesignScript
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using GraphToDSCompiler;

// Dynamo
using Dynamo.Utilities;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Nodes;

namespace Dynamo.DSEngine
{
    public class DfsTraverseAst : AstCodeBlockTraverse
    {
        public DfsTraverseAst(BinaryExpressionNode bNode, MethodMirror methodMirror)
        {
            rootNode = bNode;
            this.methodMirror = methodMirror;
        }

        private MethodMirror methodMirror;
        private BinaryExpressionNode rootNode;

        private Guid dynamoNodeID;
        public Guid DynamoNodeID
        {
            get
            {
                return dynamoNodeID;
            }
        }

        private List<AssociativeNode> leafNodes = new List<AssociativeNode>();
        public List<AssociativeNode> LeafNode
        {
            get
            {
                return leafNodes;
            }
        }

        public void DFSTraverse()
        {
            AssociativeNode astNode = rootNode.RightNode;
            base.DFSTraverse(ref astNode);            
        }

        #region Overrides

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

            if (methodMirror == null)
                throw new Exception("Method mirror cannot be null for function call");

            bool isInstanceMethod = true;
            if (methodMirror.IsConstructor || methodMirror.IsStatic)
                isInstanceMethod = false;

            IdentifierNode iNode = rootNode.LeftNode as IdentifierNode;
            if (iNode == null)
                throw new Exception("Left node must be an IdentifierNode.");

            dynamoNodeID = CreateFunctionNode(iNode, methodMirror);

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

        protected override void EmitExprListNode(ref ProtoCore.AST.AssociativeAST.ExprListNode exprListNode)
        {
            if (exprListNode == null)
                throw new ArgumentNullException("exprListNode");

            IdentifierNode iNode = rootNode.LeftNode as IdentifierNode;
            if (iNode == null)
                throw new Exception("Left node must be an IdentifierNode.");

            dynamoNodeID = CreateListNode();

            for (int i = 0; i < exprListNode.list.Count; i++)
            {
                ProtoCore.AST.AssociativeAST.AssociativeNode node = exprListNode.list[i];
                // node must be either an Identifier or primitive
                leafNodes.Add(node);
            }
        }

        #endregion

        private string GetMangledName(MethodMirror methodMirror)
        {
            string mangledName = string.Empty;
            ClassMirror type = methodMirror.GetClass();
            if (type == null)
                throw new NotImplementedException("Global functions are not supported yet.");

            string funcName = methodMirror.MethodName;
            string argList = string.Empty;
            List<ProtoCore.Type> argTypes = methodMirror.GetArgumentTypes();
            if (argTypes.Count > 0)
            {
                int i = 0;
                for (i = 0; i < argTypes.Count - 1; ++i)
                {
                    argList += argTypes[i].ToString() + ",";
                }
                argList += argTypes[i].ToString();
                mangledName = string.Format("{0}.{1}@{2}", type.ClassName, funcName, argList);
            }
            else
                mangledName = string.Format("{0}.{1}", type.ClassName, funcName);

            return mangledName;
        }

        
        private Guid CreateFunctionNode(IdentifierNode iNode, MethodMirror methodMirror)
        {
            System.Guid funcNodeId = Guid.NewGuid();

            string mangledName = GetMangledName(methodMirror);            

            dynSettings.Controller.DynamoViewModel.ExecuteCommand(
                new DynamoViewModel.CreateNodeCommand(funcNodeId, mangledName, 0, 0, true, true));

            NodeModel fNode = dynSettings.Controller.DynamoModel.Nodes.Find((x) => (x.GUID == funcNodeId));
            string nodeName = iNode.Value;
            fNode.SetAstIdentifier(nodeName);
            return funcNodeId;
        }

        private Guid CreateListNode()
        {
            System.Guid exprNodeId = AstToNode.CreatePrimitiveNode("Code Block", rootNode); 
            return exprNodeId;
        }
    }
}
