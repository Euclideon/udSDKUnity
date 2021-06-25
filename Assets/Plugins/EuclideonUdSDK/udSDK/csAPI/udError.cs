using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace udSDK
{
    public enum udError
    {
        udE_Success, //!< Indicates the operation was successful

        udE_Failure, //!< A catch-all value that is rarely used, internally the below values are favored
        udE_NothingToDo, //!< The operation didn't specifically fail but it also didn't do anything
        udE_InternalError, //!< There was an internal error that could not be handled

        udE_NotInitialized, //!< The request can't be processed because an object hasn't been configured yet
        udE_InvalidConfiguration, //!< Something in the request is not correctly configured or has conflicting settings
        udE_InvalidParameter, //!< One or more parameters is not of the expected format
        udE_OutstandingReferences, //!< The requested operation failed because there are still references to this object

        udE_MemoryAllocationFailure, //!< udSDK wasn't able to allocate enough memory for the requested feature
        udE_CountExceeded, //!< An internal count was exceeded by the request, generally going beyond the end of a buffer or internal limit

        udE_NotFound, //!< The requested item wasn't found or isn't currently available

        udE_BufferTooSmall, //!< Either the provided buffer or an internal one wasn't big enough to fufill the request
        udE_FormatVariationNotSupported, //!< The supplied item is an unsupported variant of a supported format

        udE_ObjectTypeMismatch, //!< There was a mismatch between what was expected and what was found

        udE_CorruptData, //!< The data/file was corrupt

        udE_InputExhausted, //!< The input buffer was exhausted so no more processing can occur
        udE_OutputExhausted, //!< The output buffer was exhausted so no more processing can occur

        udE_CompressionError, //!< There was an error in compression or decompression
        udE_Unsupported, //!< This functionality has not yet been implemented (usually some combination of inputs isn't compatible yet)

        udE_Timeout, //!< The requested operation timed out. Trying again later may be successful

        udE_AlignmentRequired, //!< Memory alignment was required for the operation

        udE_DecryptionKeyRequired, //!< A decryption key is required and wasn't provided
        udE_DecryptionKeyMismatch, //!< The provided decryption key wasn't the required one

        udE_SignatureMismatch, //!< The digital signature did not match the expected signature

        udE_ObjectExpired, //!< The supplied object has expired

        udE_ParseError, //!< A requested resource or input was unable to be parsed

        udE_InternalCryptoError, //!< There was a low level cryptography issue

        udE_OutOfOrder, //!< There were inputs that were provided out of order
        udE_OutOfRange, //!< The inputs were outside the expected range

        udE_CalledMoreThanOnce, //!< This function was already called

        udE_ImageLoadFailure, //!< An image was unable to be parsed. This is usually an indication of either a corrupt or unsupported image format

        udE_StreamerNotInitialised, //!<  The streamer needs to be initialised before this function can be called

        udE_OpenFailure, //!< The requested resource was unable to be opened
        udE_CloseFailure, //!< The resource was unable to be closed
        udE_ReadFailure, //!< A requested resource was unable to be read
        udE_WriteFailure, //!< A requested resource was unable to be written
        udE_SocketError, //!< There was an issue with a socket problem

        udE_DatabaseError, //!< A database error occurred
        udE_ServerError, //!< The server reported an error trying to complete the request
        udE_AuthError, //!< The provided credentials were declined (usually email or password issue)
        udE_NotAllowed, //!< The requested operation is not allowed (usually this is because the operation isn't allowed in the current state)
        udE_InvalidLicense, //!< The required license isn't available or has expired

        udE_Pending, //!< A requested operation is pending.
        udE_Cancelled, //!< The requested operation was cancelled (usually by the user)
        udE_OutOfSync, //!< There is an inconsistency between the internal udSDK state and something external. This is usually because of a time difference between the local machine and a remote server
        udE_SessionExpired, //!< The udServer has terminated your session

        udE_ProxyError, //!< There was some issue with the provided proxy information (either a proxy is in the way or the provided proxy info wasn't correct)
        udE_ProxyAuthRequired, //!< A proxy has requested authentication
        udE_ExceededAllowedLimit, //!< The requested operation failed because it would exceed the allowed limits (generally used for exceeding server limits like number of projects)

        udE_RateLimited, //!< This functionality is currently being rate limited or has exhausted a shared resource. Trying again later may be successful
        udE_PremiumOnly, //!< The requested operation failed because the current session is not for a premium user

        udE_Count //!< Internally used to verify return values
    };
}