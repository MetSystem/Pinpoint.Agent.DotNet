#include "Interceptor.h";

#define Check(hr) if (FAILED(hr)) exit(1);

typedef struct {
	BYTE ldstr1;
	BYTE stringToken1[4];
	BYTE ldstr2;
	BYTE stringToken2[4];
	BYTE ldOp1;
	BYTE ldOp2;
	BYTE fieldToken3[4];
	BYTE ldOp3;
	BYTE ldOp4;
	BYTE fieldToken4[4];
	BYTE call;
	BYTE callToken[4];
} BeforeIL;

HttpWebRequest_GetResponse::HttpWebRequest_GetResponse(ICorProfilerInfo *corProfilerInfo)
	:Interceptor(corProfilerInfo)
{

}

WCHAR *HttpWebRequest_GetResponse::GetClassName2()
{
	return L"System.Net.HttpWebRequest";
}

WCHAR *HttpWebRequest_GetResponse::GetMethodName()
{
	return L"GetResponse";
}

void HttpWebRequest_GetResponse::GetInterceptorBeforeILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, BYTE *ilCode, int *ilCodeSize)
{
	mdTypeRef classToken;
	Check(metaDataEmit->DefineTypeRefByName(GetAssemblyToken(metaDataEmit, L"Pinpoint.Agent"), L"Pinpoint.Profiler.Bootstrap", &classToken));

	//calling convention, argument count, return type, arg type
	const BYTE signature[] = { IMAGE_CEE_CS_CALLCONV_DEFAULT, 4, ELEMENT_TYPE_VOID,
		ELEMENT_TYPE_STRING, ELEMENT_TYPE_STRING, ELEMENT_TYPE_OBJECT, ELEMENT_TYPE_OBJECT };
	
	mdMemberRef methodToken;
	Check(metaDataEmit->DefineMemberRef(classToken, L"InterceptMethodBegin", signature, sizeof(signature), &methodToken));

	BeforeIL beforeIL;
	mdString textToken;

	Check(metaDataEmit->DefineUserString(L"System.Net.HttpWebRequest", wcslen(L"System.Net.HttpWebRequest"), &textToken));
	beforeIL.ldstr1 = 0x72;
	memcpy(beforeIL.stringToken1, (void*)&textToken, sizeof(textToken));

	Check(metaDataEmit->DefineUserString(L"GetResponse", wcslen(L"GetResponse"), &textToken));
	beforeIL.ldstr2 = 0x72;
	memcpy(beforeIL.stringToken2, (void*)&textToken, sizeof(textToken));

	beforeIL.ldOp1 = 0x02;
	beforeIL.ldOp2 = 0x7b;
	mdFieldDef fieldToken = GetFieldToken(functionInfo, L"_OriginUri");
	memcpy(beforeIL.fieldToken3, (void*)&fieldToken, sizeof(mdFieldDef));

	beforeIL.ldOp3 = 0x02;
	beforeIL.ldOp4 = 0x7b;
	mdFieldDef fieldToken2 = GetFieldToken(functionInfo, L"_HttpRequestHeaders");
	memcpy(beforeIL.fieldToken4, (void*)&fieldToken2, sizeof(mdFieldDef));

	beforeIL.call = 0x28;
	memcpy(beforeIL.callToken, (void*)&methodToken, sizeof(methodToken));

	*ilCodeSize = sizeof(beforeIL);
	memcpy(ilCode, &beforeIL, *ilCodeSize);
}

void HttpWebRequest_GetResponse::GetInterceptorAfterILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, BYTE *ilCode, int *ilCodeSize)
{
	GetGeneralInterceptAfterIL(metaDataEmit, functionInfo->GetClassNameW(), functionInfo->GetFunctionName(), ilCode, ilCodeSize);
}