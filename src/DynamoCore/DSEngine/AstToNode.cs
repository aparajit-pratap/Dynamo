using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// DesignScript
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;

// Dynamo
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.Nodes;
using Dynamo.ViewModels;

namespace Dynamo.DSEngine
{
    public class AstToNode
    {
        public void CreateNodeFromAST(AssociativeNode astNode, MethodMirror methodMirror)
        {
            BinaryExpressionNode bNode = null;
            if(astNode is CodeBlockNode)
	        {
                List<AssociativeNode> astNodeList = (astNode as CodeBlockNode).Body;
                bNode = astNodeList[0] as BinaryExpressionNode;
	        }
            else if (astNode is BinaryExpressionNode)
            {
                bNode = astNode as BinaryExpressionNode;
            }

            if (bNode != null)
            {
                // Create new or Modify existing identifier node
                IdentifierNode iNode = bNode.LeftNode as IdentifierNode;
                if (iNode == null)
                    throw new Exception("Left node must be an IdentifierNode.");
                
                //CreateOrModifyIdentifierNode(iNode);
                string nodeName = iNode.Value;
                // Search for nodeName in graph
                NodeModel nModel = null;
                Guid nodeID = SearchNodeByName(nodeName, out nModel);
                if (nodeID == Guid.Empty)
                {
                    // Create Dynamo node for astNode.RightNode
                    CreateNodeFromRightNode(bNode, methodMirror);
                }
                else
                {
                    // ModifyNode
                    throw new NotImplementedException("Modify node is not implemented yet");
                }
                
            }
            else
            {
                throw new Exception("AST node other than BinaryExpressionNode is not supported");
            }

           
        }

        private void CreateOrModifyIdentifierNode(IdentifierNode iNode)
        {
            // Determine identifier from astNode.LeftNode
            // Search for identifier in Dynamo graph
            // If does not exist, create new Dynamo node with same name
            // If it exists, modify it
            DynamoModel model = dynSettings.Controller.DynamoModel;
            string nodeName = iNode.Value;
            // Search for nodeName in graph
            NodeModel nModel = null;
            Guid nodeID = SearchNodeByName(nodeName, out nModel);
            if (nodeID == Guid.Empty)
            {

                // Create new node
                //System.Guid guid = Guid.NewGuid();
                /*DynamoViewModel.RecordableCommand command = new DynamoViewModel.CreateNodeCommand(guid, "Code Block",
                    0, 0, true, false);
                dynSettings.Controller.DynamoViewModel.ExecuteCommand(command);*/

                //NodeModel nModel = model.CreateNode(guid, "Code Block", 0, 0, true, false);
                
            }
            else
            {
                // ModifyNode
            }
        }

        private Guid ConnectOrCreateIdentifierNode(IdentifierNode iNode, Guid funcNodeId, int endPortIndex)
        {
            if (iNode == null)
                throw new ArgumentNullException("iNode");

            // Search for identifier in Dynamo graph
            // If does not exist, create new Dynamo node with same name
            // If it exists, create a connection to it
            DynamoModel model = dynSettings.Controller.DynamoModel;
            string nodeName = iNode.Value;
            // Search for nodeName in graph
            NodeModel nModel = null;
            Guid nodeID = SearchNodeByName(nodeName, out nModel);
            if (nodeID == Guid.Empty)
            {
                // Create new node
                nodeID = CreatePrimitiveNode("Code Block", iNode);
                CreateConnection(nodeID, 0, funcNodeId, endPortIndex);  
            }
            else
            {
                // Make connection
                if (nModel is CodeBlockNodeModel)
                {
                    int startPortIndex = CodeBlockNodeModel.GetInportIndex(nModel as CodeBlockNodeModel, nodeName);
                    CreateConnection(nodeID, startPortIndex, funcNodeId, endPortIndex);
                }
                else
                {
                    CreateConnection(nodeID, 0, funcNodeId, endPortIndex);  
                }
            }
            return nodeID;
        }


        private void CreateNodeFromRightNode(BinaryExpressionNode bNode, MethodMirror methodMirror)
        {
            // DFS traverse rightnode to find its leaf nodes
            // Leaf nodes can be either primitives or identifiers
            // For primitive leaf nodes, create primitive nodes
            // If identifier, search for it in Dynamo graph
            IdentifierNode iNode = bNode.LeftNode as IdentifierNode;
            if (iNode == null)
                throw new Exception("Left node must be an IdentifierNode.");

            if (!(bNode.RightNode is FunctionDotCallNode || bNode.RightNode is IdentifierListNode))
                throw new NotImplementedException("AST nodes other than Function calls are not supported yet");

            if(methodMirror == null)
                throw new NotImplementedException("AST nodes other than Function calls are not supported yet");

            Guid funcNodeId = CreateFunctionNode(iNode, methodMirror);

            // TODO: this assumes that these are single function calls and not chained calls
            // therefore return the argument nodes which for the time-being will be either primitives or identifiers only
            bool isInstanceMethod = true;
            if (methodMirror.IsConstructor || methodMirror.IsStatic)
                isInstanceMethod = false;

            DfsTraverseAst traversal = new DfsTraverseAst(isInstanceMethod);
            AssociativeNode astNode = bNode.RightNode;
            traversal.DFSTraverse(ref astNode);
            List<AssociativeNode> leafNodes = traversal.LeafNode;

            // Create primitive nodes
            int portCount = 0;
            foreach (var node in leafNodes)
            {
                if (node is IdentifierNode)
                {
                    // search for identifier in graph
                    Guid inputId = ConnectOrCreateIdentifierNode(node as IdentifierNode, funcNodeId, portCount++);                    
                }
                else if(node is DoubleNode || node is IntNode)
                {
                    // Create Number node
                    System.Guid inputId = CreatePrimitiveNode("Number", node);                    
                    CreateConnection(inputId, 0, funcNodeId, portCount++);                                        
                }
                else
                    throw new NotImplementedException("AST input node other than double and integer are not supported yet");
            }

        }

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
            string mangledName = GetMangledName(methodMirror);
            System.Guid funcNodeId = Guid.NewGuid();

            dynSettings.Controller.DynamoViewModel.ExecuteCommand(
                new DynamoViewModel.CreateNodeCommand(funcNodeId, mangledName, 0, 0, true, true));
            
            NodeModel fNode = dynSettings.Controller.DynamoModel.Nodes.Find((x) => (x.GUID == funcNodeId));
            string nodeName = iNode.Value;
            fNode.SetAstIdentifier(nodeName);
            return funcNodeId;
        }

        private Guid CreatePrimitiveNode(string nodeType, AssociativeNode node)
        {
            System.Guid inputId = Guid.NewGuid();
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(
                new DynamoViewModel.CreateNodeCommand(inputId, nodeType, 0, 0, true, true));

            NodeModel input = dynSettings.Controller.DynamoModel.Nodes.Find((x) => (x.GUID == inputId));
            if (input is DoubleInput)
                (input as DoubleInput).Value = node.ToString();
            else if (input is CodeBlockNodeModel)
            {
                (input as CodeBlockNodeModel).Code = node.ToString();
            }
            else
                throw new Exception("Primitives can only be identifiers or number nodes");

            return inputId;
        }

        private void CreateConnection(Guid startNodeId, int startPortIndex, Guid endNodeId, int endPortIndex)
        {
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(new DynamoViewModel.MakeConnectionCommand(
                            startNodeId, startPortIndex, PortType.OUTPUT, DynamoViewModel.MakeConnectionCommand.Mode.Begin));

            dynSettings.Controller.DynamoViewModel.ExecuteCommand(new DynamoViewModel.MakeConnectionCommand(
                endNodeId, endPortIndex, PortType.INPUT, DynamoViewModel.MakeConnectionCommand.Mode.End));
        }

        private Guid SearchNodeByName(string name, out NodeModel nModel)
        {
            nModel = null;
            Guid nodeId = Guid.Empty;
            DynamoModel model = dynSettings.Controller.DynamoModel;
            List<NodeModel> dynamoNodes = model.Nodes;
            foreach (NodeModel node in dynamoNodes)
            {
                if (node is CodeBlockNodeModel)
                {
                    var tempVars = (node as CodeBlockNodeModel).TempVariables;
                    foreach (var tempVar in tempVars)
                    {
                        if (tempVar.Equals(name))
                        {
                            nodeId = node.GUID;
                            nModel = node;
                            break;
                        }
                    }
                }
                else
                {
                    if (node.AstIdentifierForPreview.Value.Equals(name))
                    {
                        nodeId = node.GUID;
                        nModel = node;
                        break;
                    }
                }
                if (nodeId != Guid.Empty)
                    break;
            }
            return nodeId;
        }
    }
}
