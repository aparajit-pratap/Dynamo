using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DSCore;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;


namespace DSCoreNodesUI
{
    [NodeName("FileWatcher")]
    [NodeDescription("Creates a FileWatcher for watching changes in a file.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FILE_ACTIONS)]
    [IsDesignScriptCompatible]
    class FileWatcher : NodeModel
    {
        public FileWatcher()
        {
            InPortData.Add(new PortData("path", "Path to the file to create a watcher for."));
            OutPortData.Add(new PortData("fw", "Instance of a FileWatcher."));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            //RequiresRecalc = true;

            var functionCall = AstFactory.BuildFunctionCall(new Func<string, Dynamo.Nodes.FileWatch>(DSCore.FileW.FileWatcher), inputAstNodes);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
            
        }
    }

    [NodeName("FileWatcherChanged")]
    [NodeDescription("Checks if the file watched by the given FileWatcher has changed.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FILE_ACTIONS)]
    [IsDesignScriptCompatible]
    class FileWatcherChanged : NodeModel
    {
        public override bool ForceReExecuteOfNode
        {
            get
            {
                return true;
            }
        }

        public FileWatcherChanged()
        {
            InPortData.Add(new PortData("fw", "File Watcher to check for a change."));
            OutPortData.Add(new PortData("changed?", "Whether or not the file has been changed."));

            RegisterAllPorts();

        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            RequiresRecalc = true;

            var functionCall = AstFactory.BuildFunctionCall(new Func<Dynamo.Nodes.FileWatch, bool>(DSCore.FileW.FileWatcherChanged), inputAstNodes);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
            
        }
    }

    [NodeName("FileWatcherWait")]
    [NodeDescription("Waits for the specified watched file to change.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FILE_ACTIONS)]
    [IsDesignScriptCompatible]
    class FileWatcherWait : NodeModel
    {
        public FileWatcherWait()
        {
            InPortData.Add(new PortData("fw", "File Watcher to check for a change."));
            InPortData.Add(new PortData("limit", "Amount of time (in milliseconds) to wait for an update before failing."));
            OutPortData.Add(new PortData("changed?", "True: File was changed. False: Timed out."));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            RequiresRecalc = true;

            var functionCall = AstFactory.BuildFunctionCall(new Func<Dynamo.Nodes.FileWatch, int, bool>(DSCore.FileW.FileWatcherWait), inputAstNodes);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
            
        }
    }

    [NodeName("FileWatcherReset")]
    [NodeDescription("Resets state of FileWatcher so that it watches again.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FILE_ACTIONS)]
    [IsDesignScriptCompatible]
    class FileWatcherReset : NodeModel
    {
        public FileWatcherReset()
        {
            InPortData.Add(new PortData("fw", "File Watcher to check for a change."));
            OutPortData.Add(new PortData("fw", "Updated watcher."));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            RequiresRecalc = true;

            var functionCall = AstFactory.BuildFunctionCall(new Func<Dynamo.Nodes.FileWatch, Dynamo.Nodes.FileWatch>(DSCore.FileW.FileWatcherReset), inputAstNodes);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
            
        }
    }
}
