#include <fltKernel.h>

PFLT_FILTER gFilterHandle;

UNICODE_STRING gDummyBlock = RTL_CONSTANT_STRING(L"dummy_block_me_laura");


FLT_PREOP_CALLBACK_STATUS
tfPreOperation(
	_Inout_ PFLT_CALLBACK_DATA Data,
	_In_ PCFLT_RELATED_OBJECTS FltObjects,
	_Outptr_result_maybenull_ PVOID *CompletionContext
)
{
	UNREFERENCED_PARAMETER(Data);
	UNREFERENCED_PARAMETER(FltObjects);
	UNREFERENCED_PARAMETER(CompletionContext);
	PFLT_FILE_NAME_INFORMATION fileInformation = NULL;

	PAGED_CODE();

	__debugbreak();
	NTSTATUS status = FltGetFileNameInformation(Data, FLT_FILE_NAME_OPENED | FLT_FILE_NAME_QUERY_ALWAYS_ALLOW_CACHE_LOOKUP, &fileInformation);
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

	BOOLEAN result = RtlEqualUnicodeString(&gDummyBlock, &fileInformation->FinalComponent, TRUE);
	if (result) {
		Data->IoStatus.Status = STATUS_ACCESS_DENIED;
		Data->IoStatus.Information = 0;
		FltReleaseFileNameInformation(fileInformation);
		return FLT_PREOP_COMPLETE;
	}

	FltReleaseFileNameInformation(fileInformation);
	return FLT_PREOP_SUCCESS_WITH_CALLBACK;
}

FLT_POSTOP_CALLBACK_STATUS
tfPostOperation(
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
	{IRP_MJ_CREATE, 0, tfPreOperation, tfPostOperation},
	{IRP_MJ_OPERATION_END}
};


NTSTATUS
myUnloadFunction(
	_In_ FLT_FILTER_UNLOAD_FLAGS Flags
)
{
	UNREFERENCED_PARAMETER(Flags);
	FltUnregisterFilter(gFilterHandle);
	return STATUS_SUCCESS;
}

NTSTATUS
myInstanceSetup(
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
myInstanceQueryTearDown(
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

	myUnloadFunction,							// MiniFilterUnload

	myInstanceSetup,							// InstanceSetup
	myInstanceQueryTearDown,					// InstanceQueryTearDown
	NULL,										// InstanceTearDownStart
	NULL,										// InstanceTearDownComplete

	NULL,										// GenerateFileName
	NULL,										// GenerateDestinationFileName
	NULL										// NormalizeNameComponent
};

NTSTATUS
DriverEntry(
	PDRIVER_OBJECT DriverObject,
	PUNICODE_STRING RegistryPath
)
{
	NTSTATUS status;

	UNREFERENCED_PARAMETER(DriverObject);
	UNREFERENCED_PARAMETER(RegistryPath);

	status = FltRegisterFilter(DriverObject,
		&FltRegistration,
		&gFilterHandle);

	FLT_ASSERT(NT_SUCCESS(status));

	if (NT_SUCCESS(status)) {

		//
		//  Start filtering i/o
		//

		status = FltStartFiltering(gFilterHandle);

		if (!NT_SUCCESS(status)) {

			FltUnregisterFilter(gFilterHandle);
		}
	}

	return status;
}
