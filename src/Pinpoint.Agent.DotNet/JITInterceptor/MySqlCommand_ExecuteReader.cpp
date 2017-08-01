#include "Interceptor.h";

#define Check(hr) if (FAILED(hr)) exit(1);

typedef struct {
	BYTE ldstr1;
	BYTE stringToken1[4];
	BYTE ldstr2;
	BYTE stringToken2[4];
	BYTE ldOp;
	BYTE call;
	BYTE callToken[4];
} BeforeIL;

MySqlCommand_ExecuteReader::MySqlCommand_ExecuteReader(ICorProfilerInfo *corProfilerInfo)
	:Interceptor(corProfilerInfo)
{

}

WCHAR *MySqlCommand_ExecuteReader::GetClassName2()
{
	return L"MySql.Data.MySqlClient.MySqlCommand";
}

WCHAR *MySqlCommand_ExecuteReader::GetMethodName()
{
	return L"ExecuteReader";
}

void MySqlCommand_ExecuteReader::GetInterceptorBeforeILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, BYTE *ilCode, int *ilCodeSize)
{
	mdTypeRef classToken;
	Check(metaDataEmit->DefineTypeRefByName(GetAssemblyToken(metaDataEmit, L"Pinpoint.Agent"), L"Pinpoint.Profiler.Bootstrap", &classToken));

	//calling convention, argument count, return type, arg type
	const BYTE signature[] = { IMAGE_CEE_CS_CALLCONV_DEFAULT, 3, ELEMENT_TYPE_VOID,
		ELEMENT_TYPE_STRING, ELEMENT_TYPE_STRING, ELEMENT_TYPE_OBJECT };

	mdMemberRef methodToken;
	Check(metaDataEmit->DefineMemberRef(classToken, L"InterceptMethodBegin", signature, sizeof(signature), &methodToken));

	BeforeIL beforeIL;
	mdString textToken;

	Check(metaDataEmit->DefineUserString(GetClassName2(), wcslen(GetClassName2()), &textToken));
	beforeIL.ldstr1 = 0x72;
	memcpy(beforeIL.stringToken1, (void*)&textToken, sizeof(textToken));

	Check(metaDataEmit->DefineUserString(GetMethodName(), wcslen(GetMethodName()), &textToken));
	beforeIL.ldstr2 = 0x72;
	memcpy(beforeIL.stringToken2, (void*)&textToken, sizeof(textToken));

	beforeIL.ldOp = 0x02;

	beforeIL.call = 0x28;
	memcpy(beforeIL.callToken, (void*)&methodToken, sizeof(methodToken));

	*ilCodeSize = sizeof(beforeIL);
	memcpy(ilCode, &beforeIL, *ilCodeSize);
}

void MySqlCommand_ExecuteReader::GetInterceptorAfterILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, BYTE *ilCode, int *ilCodeSize)
{
	GetGeneralInterceptAfterIL(metaDataEmit, functionInfo->GetClassNameW(), functionInfo->GetFunctionName(), ilCode, ilCodeSize);
}