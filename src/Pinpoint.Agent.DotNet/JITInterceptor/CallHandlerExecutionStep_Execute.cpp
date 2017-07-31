#include "Interceptor.h";

#define Check(hr) if (FAILED(hr)) exit(1);

CallHandlerExecutionStep_Execute::CallHandlerExecutionStep_Execute(ICorProfilerInfo *corProfilerInfo)
	:Interceptor(corProfilerInfo)
{

}

WCHAR *CallHandlerExecutionStep_Execute::GetClassName2()
{
	return L"CallHandlerExecutionStep";
}

WCHAR *CallHandlerExecutionStep_Execute::GetMethodName()
{
	return L"System.Web.HttpApplication.IExecutionStep.Execute";
}

void CallHandlerExecutionStep_Execute::GetInterceptorBeforeILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, BYTE *ilCode, int *ilCodeSize)
{
	GetGeneralInterceptBeforeIL(metaDataEmit, functionInfo->GetClassNameW(), L"Execute", ilCode, ilCodeSize);
}

void CallHandlerExecutionStep_Execute::GetInterceptorAfterILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, BYTE *ilCode, int *ilCodeSize)
{
	GetGeneralInterceptAfterIL(metaDataEmit, functionInfo->GetClassNameW(), L"Execute", ilCode, ilCodeSize);
}