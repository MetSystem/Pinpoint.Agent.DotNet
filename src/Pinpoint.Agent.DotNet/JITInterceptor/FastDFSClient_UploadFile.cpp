#include "Interceptor.h";

#define Check(hr) if (FAILED(hr)) exit(1);

FastDFSClient_UploadFile::FastDFSClient_UploadFile(ICorProfilerInfo *corProfilerInfo)
	:Interceptor(corProfilerInfo)
{

}

WCHAR *FastDFSClient_UploadFile::GetClassName2()
{
	return L"FastDFS.Client.FastDFSClient";
}

WCHAR *FastDFSClient_UploadFile::GetMethodName()
{
	return L"UploadFile";
}

void FastDFSClient_UploadFile::GetInterceptorBeforeILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, BYTE *ilCode, int *ilCodeSize)
{
	GetGeneralInterceptBeforeIL(metaDataEmit, functionInfo->GetClassNameW(), functionInfo->GetFunctionName(), ilCode, ilCodeSize);
}

void FastDFSClient_UploadFile::GetInterceptorAfterILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, BYTE *ilCode, int *ilCodeSize)
{
	GetGeneralInterceptAfterIL(metaDataEmit, functionInfo->GetClassNameW(), functionInfo->GetFunctionName(), ilCode, ilCodeSize);
}