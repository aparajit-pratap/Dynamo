// DynamoEntryPoint.h

#pragma once

#include "DynamoWrapperAPI.h"

using namespace System;


public class DynamoManagedWrapper : public DynamoWrapperApi
{
		
public:
    virtual ~DynamoManagedWrapper() {};

    virtual void StartDynamo();
};

