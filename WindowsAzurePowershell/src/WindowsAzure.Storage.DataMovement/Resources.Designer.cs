using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.DataMovement
{
	[CompilerGenerated]
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	internal class Resources
	{
		private static System.Resources.ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		internal static string BlobFileSizeInvalidException
		{
			get
			{
				return Resources.ResourceManager.GetString("BlobFileSizeInvalidException", Resources.resourceCulture);
			}
		}

		internal static string BlobFileSizeTooLargeException
		{
			get
			{
				return Resources.ResourceManager.GetString("BlobFileSizeTooLargeException", Resources.resourceCulture);
			}
		}

		internal static string BlobTransferCancelledException
		{
			get
			{
				return Resources.ResourceManager.GetString("BlobTransferCancelledException", Resources.resourceCulture);
			}
		}

		internal static string BlockBlob
		{
			get
			{
				return Resources.ResourceManager.GetString("BlockBlob", Resources.resourceCulture);
			}
		}

		internal static string BlockSizeOutOfRangeException
		{
			get
			{
				return Resources.ResourceManager.GetString("BlockSizeOutOfRangeException", Resources.resourceCulture);
			}
		}

		internal static string BufferNotAllocatedThroughMemoryManagerException
		{
			get
			{
				return Resources.ResourceManager.GetString("BufferNotAllocatedThroughMemoryManagerException", Resources.resourceCulture);
			}
		}

		internal static string CannotMoveSourceIfMonitoringIsTurnedOffException
		{
			get
			{
				return Resources.ResourceManager.GetString("CannotMoveSourceIfMonitoringIsTurnedOffException", Resources.resourceCulture);
			}
		}

		internal static string CannotMoveSourceIfSourceBlobIsNullException
		{
			get
			{
				return Resources.ResourceManager.GetString("CannotMoveSourceIfSourceBlobIsNullException", Resources.resourceCulture);
			}
		}

		internal static string CannotOverwriteBlockBlobWithPageBlobException
		{
			get
			{
				return Resources.ResourceManager.GetString("CannotOverwriteBlockBlobWithPageBlobException", Resources.resourceCulture);
			}
		}

		internal static string CannotOverwritePageBlobWithBlockBlobException
		{
			get
			{
				return Resources.ResourceManager.GetString("CannotOverwritePageBlobWithBlockBlobException", Resources.resourceCulture);
			}
		}

		internal static string CannotParseAccountFromUriException
		{
			get
			{
				return Resources.ResourceManager.GetString("CannotParseAccountFromUriException", Resources.resourceCulture);
			}
		}

		internal static string CannotRemoveSourceWithoutSourceFileException
		{
			get
			{
				return Resources.ResourceManager.GetString("CannotRemoveSourceWithoutSourceFileException", Resources.resourceCulture);
			}
		}

		internal static string CanOnlySetOneCredentialException
		{
			get
			{
				return Resources.ResourceManager.GetString("CanOnlySetOneCredentialException", Resources.resourceCulture);
			}
		}

		internal static string ConcurrentCountNotPositiveException
		{
			get
			{
				return Resources.ResourceManager.GetString("ConcurrentCountNotPositiveException", Resources.resourceCulture);
			}
		}

		internal static string ContainerOnlyValidForSourceException
		{
			get
			{
				return Resources.ResourceManager.GetString("ContainerOnlyValidForSourceException", Resources.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		internal static string DataMovement_ExceptionFromCallback
		{
			get
			{
				return Resources.ResourceManager.GetString("DataMovement_ExceptionFromCallback", Resources.resourceCulture);
			}
		}

		internal static string DownloadedMd5MismatchException
		{
			get
			{
				return Resources.ResourceManager.GetString("DownloadedMd5MismatchException", Resources.resourceCulture);
			}
		}

		internal static string FailedToAllocateMemoryException
		{
			get
			{
				return Resources.ResourceManager.GetString("FailedToAllocateMemoryException", Resources.resourceCulture);
			}
		}

		internal static string FailedToCopyBlobException
		{
			get
			{
				return Resources.ResourceManager.GetString("FailedToCopyBlobException", Resources.resourceCulture);
			}
		}

		internal static string FailedToGetBlobTypeException
		{
			get
			{
				return Resources.ResourceManager.GetString("FailedToGetBlobTypeException", Resources.resourceCulture);
			}
		}

		internal static string FailedToGetSourceLastWriteTime
		{
			get
			{
				return Resources.ResourceManager.GetString("FailedToGetSourceLastWriteTime", Resources.resourceCulture);
			}
		}

		internal static string FailedToOpenFileException
		{
			get
			{
				return Resources.ResourceManager.GetString("FailedToOpenFileException", Resources.resourceCulture);
			}
		}

		internal static string FailedToRetrieveCopyStateForBlobToMonitorException
		{
			get
			{
				return Resources.ResourceManager.GetString("FailedToRetrieveCopyStateForBlobToMonitorException", Resources.resourceCulture);
			}
		}

		internal static string InvalidInitialEntryStatusForControllerException
		{
			get
			{
				return Resources.ResourceManager.GetString("InvalidInitialEntryStatusForControllerException", Resources.resourceCulture);
			}
		}

		internal static string InvalidInitialEntryStatusWhenMoveSourceIsOffException
		{
			get
			{
				return Resources.ResourceManager.GetString("InvalidInitialEntryStatusWhenMoveSourceIsOffException", Resources.resourceCulture);
			}
		}

		internal static string LocalToLocalTransferUnsupportedException
		{
			get
			{
				return Resources.ResourceManager.GetString("LocalToLocalTransferUnsupportedException", Resources.resourceCulture);
			}
		}

		internal static string MismatchFoundBetweenLocalAndServerCopyIdsException
		{
			get
			{
				return Resources.ResourceManager.GetString("MismatchFoundBetweenLocalAndServerCopyIdsException", Resources.resourceCulture);
			}
		}

		internal static string OnlySupportTwoBlobTypesException
		{
			get
			{
				return Resources.ResourceManager.GetString("OnlySupportTwoBlobTypesException", Resources.resourceCulture);
			}
		}

		internal static string OverwriteCallbackCancelTransferException
		{
			get
			{
				return Resources.ResourceManager.GetString("OverwriteCallbackCancelTransferException", Resources.resourceCulture);
			}
		}

		internal static string PageBlob
		{
			get
			{
				return Resources.ResourceManager.GetString("PageBlob", Resources.resourceCulture);
			}
		}

		internal static string ProvideAtMostOneParameterBothProvidedException
		{
			get
			{
				return Resources.ResourceManager.GetString("ProvideAtMostOneParameterBothProvidedException", Resources.resourceCulture);
			}
		}

		internal static string ProvideExactlyOneParameterBothNullException
		{
			get
			{
				return Resources.ResourceManager.GetString("ProvideExactlyOneParameterBothNullException", Resources.resourceCulture);
			}
		}

		internal static string ProvideExactlyOneParameterBothProvidedException
		{
			get
			{
				return Resources.ResourceManager.GetString("ProvideExactlyOneParameterBothProvidedException", Resources.resourceCulture);
			}
		}

		internal static string ReadableSizeFormatBytes
		{
			get
			{
				return Resources.ResourceManager.GetString("ReadableSizeFormatBytes", Resources.resourceCulture);
			}
		}

		internal static string ReadableSizeFormatExaBytes
		{
			get
			{
				return Resources.ResourceManager.GetString("ReadableSizeFormatExaBytes", Resources.resourceCulture);
			}
		}

		internal static string ReadableSizeFormatGigaBytes
		{
			get
			{
				return Resources.ResourceManager.GetString("ReadableSizeFormatGigaBytes", Resources.resourceCulture);
			}
		}

		internal static string ReadableSizeFormatKiloBytes
		{
			get
			{
				return Resources.ResourceManager.GetString("ReadableSizeFormatKiloBytes", Resources.resourceCulture);
			}
		}

		internal static string ReadableSizeFormatMegaBytes
		{
			get
			{
				return Resources.ResourceManager.GetString("ReadableSizeFormatMegaBytes", Resources.resourceCulture);
			}
		}

		internal static string ReadableSizeFormatPetaBytes
		{
			get
			{
				return Resources.ResourceManager.GetString("ReadableSizeFormatPetaBytes", Resources.resourceCulture);
			}
		}

		internal static string ReadableSizeFormatTeraBytes
		{
			get
			{
				return Resources.ResourceManager.GetString("ReadableSizeFormatTeraBytes", Resources.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static System.Resources.ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(Resources.resourceMan, null))
				{
					Resources.resourceMan = new System.Resources.ResourceManager("Microsoft.WindowsAzure.Storage.DataMovement.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		internal static string RestartableInfoCorruptedException
		{
			get
			{
				return Resources.ResourceManager.GetString("RestartableInfoCorruptedException", Resources.resourceCulture);
			}
		}

		internal static string SmallMemoryCacheSizeLimitationException
		{
			get
			{
				return Resources.ResourceManager.GetString("SmallMemoryCacheSizeLimitationException", Resources.resourceCulture);
			}
		}

		internal static string SourceAndDestinationLocationCannotBeEqualException
		{
			get
			{
				return Resources.ResourceManager.GetString("SourceAndDestinationLocationCannotBeEqualException", Resources.resourceCulture);
			}
		}

		internal static string SourceBlobDoesNotExistException
		{
			get
			{
				return Resources.ResourceManager.GetString("SourceBlobDoesNotExistException", Resources.resourceCulture);
			}
		}

		internal static string SourceFileHasBeenChangedException
		{
			get
			{
				return Resources.ResourceManager.GetString("SourceFileHasBeenChangedException", Resources.resourceCulture);
			}
		}

		internal static string StorageKeyInvalidFormatException
		{
			get
			{
				return Resources.ResourceManager.GetString("StorageKeyInvalidFormatException", Resources.resourceCulture);
			}
		}

		internal static string StreamMustSupportReadException
		{
			get
			{
				return Resources.ResourceManager.GetString("StreamMustSupportReadException", Resources.resourceCulture);
			}
		}

		internal static string StreamMustSupportSeekException
		{
			get
			{
				return Resources.ResourceManager.GetString("StreamMustSupportSeekException", Resources.resourceCulture);
			}
		}

		internal static string StreamMustSupportWriteException
		{
			get
			{
				return Resources.ResourceManager.GetString("StreamMustSupportWriteException", Resources.resourceCulture);
			}
		}

		internal static string SubfoldersNotAllowedUnderRootContainerException
		{
			get
			{
				return Resources.ResourceManager.GetString("SubfoldersNotAllowedUnderRootContainerException", Resources.resourceCulture);
			}
		}

		internal static string TransferEntryCopyIdCannotBeNullOrEmptyException
		{
			get
			{
				return Resources.ResourceManager.GetString("TransferEntryCopyIdCannotBeNullOrEmptyException", Resources.resourceCulture);
			}
		}

		internal static string TransferEntryPropertyCanBeSetOnlyOnceException
		{
			get
			{
				return Resources.ResourceManager.GetString("TransferEntryPropertyCanBeSetOnlyOnceException", Resources.resourceCulture);
			}
		}

		internal static string UndefinedBlobTypeException
		{
			get
			{
				return Resources.ResourceManager.GetString("UndefinedBlobTypeException", Resources.resourceCulture);
			}
		}

		internal static string UndefinedTransferEntryStatusException
		{
			get
			{
				return Resources.ResourceManager.GetString("UndefinedTransferEntryStatusException", Resources.resourceCulture);
			}
		}

		internal Resources()
		{
		}
	}
}