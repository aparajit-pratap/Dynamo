// DynamoEntryPoint.h

#pragma once
#pragma comment(lib, "E:\\Github\\Dynamo\\Dynamo\\extern\\DesignScript\\DesignScriptRunner.lib")

#include "DynamoWrapperAPI.h"

using namespace System;
using namespace DynamoWrapper;

public class DynamoManagedWrapper : public DynamoWrapperApi
{
		
public:
    virtual ~DynamoManagedWrapper() {};

    virtual void Initialize();
    virtual void StartDynamo();
    virtual void CreateGraph();
    virtual void CreateGraphFromAst(AstNode* pAstNode, DesignScriptMethod* pMirror);    

};

