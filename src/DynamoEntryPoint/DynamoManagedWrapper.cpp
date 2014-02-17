// This is the main DLL file.

#include "stdafx.h"

#include "DynamoManagedWrapper.h"
#include "../../extern/DesignScript/MirrorObjectWrapper.h"
#include "../../extern/DesignScript/LiveRunnerWrapper.h"


using namespace System;
using namespace System::IO;
using namespace System::Threading;


using namespace DynamoWrapper;

DynamoWrapperApi* DynamoWrapperApi::create()
{
    return new DynamoManagedWrapper();
}

void DynamoManagedWrapper::Initialize()
{
    DynamoCSharpWrapper::Initialize();
}

void DynamoManagedWrapper::StartDynamo()
{
    DynamoCSharpWrapper::StartUp();
}

void DynamoManagedWrapper::CreateGraph()
{
    DynamoCSharpWrapper::CreateGraph();
}

void DynamoManagedWrapper::CreateGraphFromAst(AstNode* pAstNode, DesignScriptMethod* pMethod)
{
    AssociativeNodeWrapper* aNode = dynamic_cast<AssociativeNodeWrapper*>(pAstNode);
    if(aNode != NULL)
    {
        AssociativeNode^ astNode = aNode->wrapper();

        MethodMirrorWrapper* pMirror = dynamic_cast<MethodMirrorWrapper*>(pMethod);
        if(pMirror != NULL)
              DynamoCSharpWrapper::CreateGraphFromAst(astNode, pMirror->wrapper());
    }
}

