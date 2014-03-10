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
                    // 1. Search for existing node with same symbol name
                    // 2. Delete the node (inputs and outputs are disconnected)
                    // 3. Create new node of RHS type and having same name
                    // 4. Reconnect connectors
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

        protected Guid ConnectOrCreateIdentifierNode(IdentifierNode iNode, Guid funcNodeId, int endPortIndex)
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
                    int startPortIndex = (nModel as CodeBlockNodeModel).GetOutportIndex(nodeName);
                    CreateConnection(nodeID, startPortIndex, funcNodeId, endPortIndex);
                }
                else
                {
                    CreateConnection(nodeID, 0, funcNodeId, endPortIndex);  
                }
            }
            return nodeID;
        }


        virtual protected void CreateNodeFromRightNode(BinaryExpressionNode bNode, MethodMirror methodMirror)
        {
            // DFS traverse rightnode to find its leaf nodes
            // Leaf nodes can be either primitives or identifiers
            // For primitive leaf nodes, create primitive nodes
            // If identifier, search for it in Dynamo graph
            IdentifierNode iNode = bNode.LeftNode as IdentifierNode;
            if (iNode == null)
                throw new Exception("Left node must be an IdentifierNode.");

            // TODO: this assumes that these are single function calls (and not nested or chained calls) 
            // or primitive/identifier array nodes
            // therefore return the argument nodes which for the time-being will be either primitives or identifiers only            
            DfsTraverseAst traversal = new DfsTraverseAst(bNode, methodMirror);
            traversal.DFSTraverse();
            List<AssociativeNode> leafNodes = traversal.LeafNode;

            // Create primitive nodes
            int portCount = 0;
            foreach (var node in leafNodes)
            {
                if (node is IdentifierNode)
                {
                    // search for identifier in graph
                    Guid inputId = ConnectOrCreateIdentifierNode(node as IdentifierNode, traversal.DynamoNodeID, portCount++);                    
                }
                else if(node is DoubleNode || node is IntNode)
                {
                    // Create Number node
                    System.Guid inputId = CreatePrimitiveNode("Number", node);
                    CreateConnection(inputId, 0, traversal.DynamoNodeID, portCount++);                                        
                }
                else
                    throw new NotImplementedException("AST input node other than double and integer are not supported yet");
            }

        }

        public static Guid CreatePrimitiveNode(string nodeType, AssociativeNode node)
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

        protected void CreateConnection(Guid startNodeId, int startPortIndex, Guid endNodeId, int endPortIndex)
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
                    var tempVars = (node as CodeBlockNodeModel).GetDefinedVariableNames();
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
