#include <Windows.h>


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
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

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
}

