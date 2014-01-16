// This is the main DLL file.

#include "stdafx.h"

#include "DynamoManagedWrapper.h"

using namespace System;
using namespace System::IO;
using namespace System::Threading;
using namespace System::Windows;

using namespace DynamoWrapper;

DynamoWrapperApi* DynamoWrapperApi::create()
{
    return new DynamoManagedWrapper();
}

void DynamoManagedWrapper::StartDynamo()
{
    DynamoCSharpWrapper::StartUp();
}