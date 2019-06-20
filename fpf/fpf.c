#include <fltKernel.h>

PFLT_FILTER gFilterHandle;

UNICODE_STRING gProtectedPath;

PFLT_PORT gPort;
UNICODE_STRING gPortName = RTL_CONSTANT_STRING(L"\\FpPort");

PFLT_PORT gClientPort;

typedef struct _PROTECTED_PATH_ENTRY
{
	PVOID Path;
	LIST_ENTRY ListEntry;
}PROTECTED_PATH_ENTRY, *PPROTECTED_PATH_ENTRY;

LIST_ENTRY gProtectedPaths;
/*
(PPROTECTED_PATH_ENTRY)gProtectedPaths->Flink;

auto entry = CONTAINING_RECORD(flink, PROTECTED_PATH_ENTRY, ListEntry)

*/

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


FLT_PREOP_CALLBACK_STATUS
FpPreOperation(
	_Inout_ PFLT_CALLBACK_DATA Data,
	_In_ PCFLT_RELATED_OBJECTS FltObjects,
	_Outptr_result_maybenull_ PVOID *CompletionContext
)
{
	UNREFERENCED_PARAMETER(FltObjects);
	UNREFERENCED_PARAMETER(CompletionContext);
	PFLT_FILE_NAME_INFORMATION fileInformation = NULL;

	PAGED_CODE();

	NTSTATUS status = FltGetFileNameInformation(Data, FLT_FILE_NAME_NORMALIZED | FLT_FILE_NAME_QUERY_ALWAYS_ALLOW_CACHE_LOOKUP, &fileInformation);
	if (!NT_SUCCESS(status)) {
		return FLT_PREOP_SUCCESS_NO_CALLBACK;
	}
	if (!fileInformation)
	{
		NT_VERIFY(FALSE);
		return FLT_PREOP_SUCCESS_NO_CALLBACK;
	}

	status = FltParseFileNameInformation(fileInformation);
	if (!NT_SUCCESS(status))
	{
		FltReleaseFileNameInformation(fileInformation);
		return FLT_PREOP_SUCCESS_NO_CALLBACK;
	}
	if (gProtectedPath.Buffer) {
		BOOLEAN result = RtlEqualUnicodeString(&gProtectedPath, &fileInformation->Name, TRUE);

		if (!result) {
			return FLT_PREOP_SUCCESS_NO_CALLBACK;
		}
		
		PNOTIFICATION_FILE_NAME_MATCHED notification = NULL;
		ULONG msgSize = sizeof(NOTIFICATION_FILE_NAME_MATCHED) + fileInformation->Name.MaximumLength;
		notification = ExAllocatePoolWithTag(PagedPool, msgSize, '1GT#');
		notification->Pid = FltGetRequestorProcessId(Data);
		notification->Length = fileInformation->Name.MaximumLength;
		RtlCopyMemory(notification->Buffer, fileInformation->Name.Buffer, fileInformation->Name.MaximumLength);

		BOOLEAN shouldBlock = FALSE;
		ULONG shouldBlockSize = sizeof(BOOLEAN);


		status = FltSendMessage(
			gFilterHandle,
			&gClientPort,
			notification,
			msgSize,
			&shouldBlock,
			&shouldBlockSize,
			NULL
		);
		ExFreePoolWithTag(notification, '1GT#');
		FltReleaseFileNameInformation(fileInformation);
		if (!NT_SUCCESS(status)) {
			return FLT_PREOP_SUCCESS_NO_CALLBACK;
		}

		if (shouldBlock) {
			Data->IoStatus.Status = STATUS_ACCESS_DENIED;
			Data->IoStatus.Information = 0;
			return FLT_PREOP_COMPLETE;
		}
		return FLT_PREOP_SUCCESS_NO_CALLBACK;
	}
	FltReleaseFileNameInformation(fileInformation);
	return FLT_PREOP_SUCCESS_WITH_CALLBACK;
}

FLT_POSTOP_CALLBACK_STATUS
FpPostOperation(
	_Inout_ PFLT_CALLBACK_DATA Data,
	_In_ PCFLT_RELATED_OBJECTS FltObjects,
	_In_opt_ PVOID CompletionContext,
	_In_ FLT_POST_OPERATION_FLAGS Flags
)
{

	UNREFERENCED_PARAMETER(Data);
	UNREFERENCED_PARAMETER(FltObjects);
	UNREFERENCED_PARAMETER(CompletionContext);
	UNREFERENCED_PARAMETER(Flags);

	return FLT_POSTOP_FINISHED_PROCESSING;
}

CONST FLT_OPERATION_REGISTRATION Callbacks[] =
{
	{IRP_MJ_CREATE, 0, FpPreOperation, FpPostOperation},
	{IRP_MJ_OPERATION_END}
};

NTSTATUS
FpUnloadFunction(
	_In_ FLT_FILTER_UNLOAD_FLAGS Flags
)
{
	UNREFERENCED_PARAMETER(Flags);

	FltCloseClientPort(gFilterHandle, &gClientPort);
	FltCloseCommunicationPort(gPort);
	FltUnregisterFilter(gFilterHandle);
	if (gProtectedPath.Buffer)
	{
		ExFreePoolWithTag(gProtectedPath.Buffer, 'GAT#');
	}
	return STATUS_SUCCESS;
}

NTSTATUS
FpInstanceSetup(
	_In_ PCFLT_RELATED_OBJECTS FltObjects,
	_In_ FLT_INSTANCE_SETUP_FLAGS Flags,
	_In_ DEVICE_TYPE VolumeDeviceType,
	_In_ FLT_FILESYSTEM_TYPE VolumeFilesystemType
)
{
	UNREFERENCED_PARAMETER(FltObjects);
	UNREFERENCED_PARAMETER(Flags);
	UNREFERENCED_PARAMETER(VolumeDeviceType);
	UNREFERENCED_PARAMETER(VolumeFilesystemType);

	return STATUS_SUCCESS;
}

NTSTATUS
FpQueryTearDown(
	_In_ PCFLT_RELATED_OBJECTS FltObjects,
	_In_ FLT_INSTANCE_QUERY_TEARDOWN_FLAGS Flags
)
{
	UNREFERENCED_PARAMETER(FltObjects);
	UNREFERENCED_PARAMETER(Flags);

	return STATUS_SUCCESS;
}


CONST FLT_REGISTRATION FltRegistration = {
	sizeof(FLT_REGISTRATION),					// Size
	FLT_REGISTRATION_VERSION,					// Version
	0,											// Flags

	NULL,										// Context
	Callbacks,									// Operation Callbacks

	FpUnloadFunction,							// MiniFilterUnload

	FpInstanceSetup,							// InstanceSetup
	FpQueryTearDown,					// InstanceQueryTearDown
	NULL,										// InstanceTearDownStart
	NULL,										// InstanceTearDownComplete

	NULL,										// GenerateFileName
	NULL,										// GenerateDestinationFileName
	NULL										// NormalizeNameComponent
};

NTSTATUS
FLTAPI
FpOnClientConnect(
	_In_ PFLT_PORT ClientPort,
	_In_opt_ PVOID ServerPortCookie,
	_In_reads_bytes_opt_(SizeOfContext) PVOID ConnectionContext,
	_In_ ULONG SizeOfContext,
	_Outptr_result_maybenull_ PVOID *ConnectionPortCookie
) 
{
	UNREFERENCED_PARAMETER(ServerPortCookie);
	UNREFERENCED_PARAMETER(ConnectionContext);
	UNREFERENCED_PARAMETER(SizeOfContext);
	UNREFERENCED_PARAMETER(ConnectionPortCookie);

	gClientPort = ClientPort;

	return STATUS_SUCCESS;
}

VOID
FLTAPI 
FpOnClientDisconnect(
	_In_opt_ PVOID ConnectionCookie
) 
{
	UNREFERENCED_PARAMETER(ConnectionCookie);
}

NTSTATUS
FLTAPI FpOnClientNotify(
	_In_opt_ PVOID PortCookie,
	_In_reads_bytes_opt_(InputBufferLength) PVOID InputBuffer,
	_In_ ULONG InputBufferLength,
	_Out_writes_bytes_to_opt_(OutputBufferLength, *ReturnOutputBufferLength) PVOID OutputBuffer,
	_In_ ULONG OutputBufferLength,
	_Out_ PULONG ReturnOutputBufferLength
)
{
	UNREFERENCED_PARAMETER(InputBufferLength);
	UNREFERENCED_PARAMETER(PortCookie);
	UNREFERENCED_PARAMETER(OutputBuffer);
	UNREFERENCED_PARAMETER(OutputBufferLength);

	NTSTATUS status = STATUS_UNSUCCESSFUL;

	PCMD_PROTECT_FILE input = (PCMD_PROTECT_FILE)InputBuffer;
	UNICODE_STRING path = { 0 };

	UNICODE_STRING ustr = { 0 };
	ULONG actualLenght = 0;

	__debugbreak();

	path.Buffer = (wchar_t*)input->Buffer;
	path.Length = path.MaximumLength = input->Length;

	path.Length = 12;

	*ReturnOutputBufferLength = 0;

	OBJECT_ATTRIBUTES oa = { 0 };

	InitializeObjectAttributes(
		&oa,
		&path,
		OBJ_KERNEL_HANDLE,
		NULL,
		NULL
	);

	HANDLE out = NULL;

	status = ZwOpenSymbolicLinkObject(
		&out,
		GENERIC_ALL,
		&oa
	);
	if (!NT_SUCCESS(status)) {
		goto Exit;
	}

	status = ZwQuerySymbolicLinkObject(
		out,
		&ustr,
		&actualLenght
	);
	if (STATUS_BUFFER_TOO_SMALL != status) {
		status = STATUS_INVALID_BUFFER_SIZE;
		goto Exit;
	}

	ustr.MaximumLength = (USHORT)actualLenght + input->Length - 12;
	ustr.Buffer = ExAllocatePoolWithTag(PagedPool, ustr.MaximumLength, 'GAT#');
	ustr.Length = 0;

	status = ZwQuerySymbolicLinkObject(
		out,
		&ustr,
		&actualLenght
	);

	if (!NT_SUCCESS(status)) {
		goto Exit;
	}

	path.Length = input->Length;
	path.Length -= 12;
	path.MaximumLength -= 12;
	path.Buffer += 6;

	status = RtlAppendUnicodeStringToString(&ustr, &path);
	if (!NT_SUCCESS(status)) {
		goto Exit;
	}
	
	if (gProtectedPath.Buffer)
	{
		ExFreePoolWithTag(gProtectedPath.Buffer, 'GAT#');
	}
	gProtectedPath = ustr;

Exit:
	if (out) {
		ZwClose(out);
	}
	return status;
}

NTSTATUS
DriverEntry(
	PDRIVER_OBJECT DriverObject,
	PUNICODE_STRING RegistryPath
)
{
	NTSTATUS status;

	UNREFERENCED_PARAMETER(DriverObject);
	UNREFERENCED_PARAMETER(RegistryPath);

	status = FltRegisterFilter(
		DriverObject,
		&FltRegistration,
		&gFilterHandle
	);
	if (!NT_VERIFY(NT_SUCCESS(status)))
	{
		return status;
	}


	PSECURITY_DESCRIPTOR sd = NULL;

	status = FltBuildDefaultSecurityDescriptor(
		&sd,
		FLT_PORT_ALL_ACCESS
	);
	if (!NT_VERIFY(NT_SUCCESS(status))) 
	{
		FltUnregisterFilter(gFilterHandle);
		return status;
	}
	
	OBJECT_ATTRIBUTES oa = { 0 };

	InitializeObjectAttributes(
		&oa,
		&gPortName,
		OBJ_CASE_INSENSITIVE | OBJ_KERNEL_HANDLE,
		NULL,
		sd
	);


	status = FltCreateCommunicationPort(
		gFilterHandle,
		&gPort,
		&oa,
		NULL,
		FpOnClientConnect,
		FpOnClientDisconnect,
		FpOnClientNotify,
		1
	);
	FltFreeSecurityDescriptor(sd);
	if (!NT_VERIFY(NT_SUCCESS(status)))
	{
		FltUnregisterFilter(gFilterHandle);
		return status;
	}

	status = FltStartFiltering(gFilterHandle);
	if (!NT_SUCCESS(status))
	{
		FltUnregisterFilter(gFilterHandle);
	}

	return status;
}
