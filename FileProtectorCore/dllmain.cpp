#include <Windows.h>
#include <fltuser.h>

HANDLE gFilterPort;

void Init()
{
	HRESULT status = E_FAIL;

	status = FilterConnectCommunicationPort(
		L"\\FpPort",
		0,
		NULL,
		0,
		NULL,
		&gFilterPort
	);

	if (!SUCCEEDED(status))
	{
		;
	}
}

void Uninit() 
{
	FilterClose(gFilterPort);
}

BOOL
APIENTRY
DllMain(
	HMODULE Module,
	DWORD  Reason,
	LPVOID Reserved
)
{
	UNREFERENCED_PARAMETER(Reserved);
    switch (Reason)
    {
    case DLL_PROCESS_ATTACH:
		DisableThreadLibraryCalls(Module);
		//Init();
		break;
    case DLL_THREAD_ATTACH:
		break;
    case DLL_THREAD_DETACH:
		break;
    case DLL_PROCESS_DETACH:
		//Uninit();
        break;
    }
    return TRUE;
}

#pragma warning(push)
#pragma warning(disable: 4200)

#pragma pack(push)
#pragma pack(1)

typedef struct _NOTIFICATION_FILE_NAME_MATCHED
{
	ULONG Pid;
	USHORT Length;
	wchar_t Buffer[0];
} NOTIFICATION_FILE_NAME_MATCHED, *PNOTIFICATION_FILE_NAME_MATCHED;

typedef struct _CMD_PROTECT_FILE
{
	USHORT Length;
	wchar_t Buffer[0];
}CMD_PROTECT_FILE, *PCMD_PROTECT_FILE;

#pragma pack(pop)

#pragma warning(pop)

/*
struct = malloc(sizeof(CmdProtectFile) + Length * sizeof(wchar_t))
commandid = 1
length = l
CopyMemory(buffer, path, length * sizeof(wchar_t));
*/
extern "C"
{
	

	__declspec(dllexport)
	int
	function()
	{
		return 1;
	}
	
	__declspec(dllexport)
	char*
	function2()
	{
		char *str = (char *)malloc(sizeof(char) * 3);
		str[0] = 'm';
		str[1] = 'y';
		str[2] = '\0';

		return str;
	}

	__declspec(dllexport)
	void
	ProtectFile(
		wchar_t* Path,
		USHORT Length
	)
	{
		UNREFERENCED_PARAMETER(Path);
		UNREFERENCED_PARAMETER(Length);
		/*HRESULT status = E_FAIL;
		PCMD_PROTECT_FILE cmdProtect = NULL;
		DWORD ret = 0;
		DWORD size = sizeof(CMD_PROTECT_FILE) + Length * (sizeof(wchar_t));

		cmdProtect = (PCMD_PROTECT_FILE)malloc(size);
		cmdProtect->Length = Length * 2;
		CopyMemory(cmdProtect->Buffer, Path, Length * 2);

		status = FilterSendMessage(
			gFilterPort,
			(PVOID)cmdProtect,
			size,
			NULL,
			0,
			&ret
		);
		if (!SUCCEEDED(status))
		{
			;
		}

		free(cmdProtect);*/
	}

	__declspec(dllexport)
	HRESULT
	GetNextNotification(
		wchar_t** Path,
		PULONG Pid
	)
	{
		*Pid = 3;
		wchar_t* path = (wchar_t*)malloc(6);
		path[0] = L'a';
		path[1] = L'a';
		path[2] = L'\0';
		*Path = path;

		return S_OK;
	}

	__declspec(dllexport)
	HRESULT
	FreeNotification(
		wchar_t* Path
	)
	{
		free(Path);
		return S_OK;
	}
}