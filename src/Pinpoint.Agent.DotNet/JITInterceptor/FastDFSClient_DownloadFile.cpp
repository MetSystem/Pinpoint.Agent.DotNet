#include "Interceptor.h";

#define Check(hr) if (FAILED(hr)) exit(1);

FastDFSClient_DownloadFile::FastDFSClient_DownloadFile(ICorProfilerInfo *corProfilerInfo)
	:Interceptor(corProfilerInfo)
{

}

WCHAR *FastDFSClient_DownloadFile::GetClassName2()
{
	return L"FastDFS.Client.FastDFSClient";
}

WCHAR *FastDFSClient_DownloadFile::GetMethodName()
{
	return L"DownloadFile";
}

void FastDFSClient_DownloadFile::GetInterceptorBeforeILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, BYTE *ilCode, int *ilCodeSize)
{
	GetGeneralInterceptBeforeIL(metaDataEmit, functionInfo->GetClassNameW(), functionInfo->GetFunctionName(), ilCode, ilCodeSize);
}

void FastDFSClient_DownloadFile::GetInterceptorAfterILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, BYTE *ilCode, int *ilCodeSize)
{
	GetGeneralInterceptAfterIL(metaDataEmit, functionInfo->GetClassNameW(), functionInfo->GetFunctionName(), ilCode, ilCodeSize);
}