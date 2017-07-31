#include "Interceptor.h";

#define Check(hr) if (FAILED(hr)) exit(1);

typedef struct {
	BYTE ldstr1;
	BYTE stringToken1[4];
	BYTE ldstr2;
	BYTE stringToken2[4];
	BYTE call;
	BYTE callToken[4];
} BeforeIL;

ModelBase_ModelSend::ModelBase_ModelSend(ICorProfilerInfo *corProfilerInfo)
	:Interceptor(corProfilerInfo)
{

}

WCHAR *ModelBase_ModelSend::GetClassName2()
{
	return L"RabbitMQ.Client.Impl.ModelBase";
}

WCHAR *ModelBase_ModelSend::GetMethodName()
{
	return L"BasicPublish";
}

void ModelBase_ModelSend::GetInterceptorBeforeILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, BYTE *ilCode, int *ilCodeSize)
{
	GetGeneralInterceptBeforeIL(metaDataEmit, functionInfo->GetClassNameW(), functionInfo->GetFunctionName(), ilCode, ilCodeSize);
}

void ModelBase_ModelSend::GetInterceptorAfterILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, BYTE *ilCode, int *ilCodeSize)
{
	GetGeneralInterceptAfterIL(metaDataEmit, functionInfo->GetClassNameW(), functionInfo->GetFunctionName(), ilCode, ilCodeSize);
}