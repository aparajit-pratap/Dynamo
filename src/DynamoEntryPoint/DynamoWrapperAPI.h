#pragma once

#include "../../extern/DesignScript/DesignScriptRunnerAPI.h"

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the DYNAMO_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// DYNAMOWRAPPER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef DYNAMO_EXPORTS
#define DYNAMOWRAPPER_API __declspec(dllexport)
#else
#define DYNAMOWRAPPER_API __declspec(dllimport)
#endif

/// <summary>
/// DynamoWrapperApi class implements the dynamo graph. 
/// </summary>
class __declspec(novtable) DynamoWrapperApi
{
public:
    
    DYNAMOWRAPPER_API static DynamoWrapperApi* create();

    virtual void Initialize() = 0;
    virtual void StartDynamo() = 0;
    virtual void CreateGraph() = 0;
    virtual void CreateGraphFromAst(AstNode* pAstNode, DesignScriptMethod* pMirror) = 0;
    
};


