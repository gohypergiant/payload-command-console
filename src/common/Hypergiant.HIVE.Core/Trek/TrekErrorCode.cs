﻿namespace Hypergiant.HIVE
{
    public enum TrekErrorCode
    {
        SUCCESS = 0,

        DS_CODE_BASE = 50000,
        DS_THREAD_START_ERROR = 50001,  //!< Error starting a thread.
        DS_THREAD_TIMEOUT = 50002,  //!< A timeout occurred while starting a thread. 
        DS_THREAD_STOP_ERROR = 50003,   //!< Error stopping a thread.
        DS_LIBRARY_LOAD_ERROR = 50004,  //!< Error loading a device library using pathname.
        DS_LIBRARY_ADDRESS_ERROR = 50005,   //!< Error addressing a device library.
        DS_FILE_READ_SIZE_ERROR = 50006,    //!< Error identifying the bytes to read from a file.
        DS_DEVICE_NOT_FOUND = 50007,    //!< The device name or key does not match any existing device.
        DS_INVALID_FILE_DEVICE_TYPE = 50008,    //!< The file device type is not invlaid.
        DS_INVALID_SEND_TO_DEVICE = 50009,  //!< Cannot send packets to this device.
        DS_INVALID_DEVICE_TYPE = 50010, //!< The device type does not support request.
        DS_DEVICE_HAS_EXISTING_PHP = 50011, //!< The device has an existing packet header processor. 
        DS_PACKET_KEY_NOT_FOUND = 50012,    //!< The packet key was not found in the associated map.
        DS_PHP_PACKET_TYPE_MISMATCH = 50013,    //!< The packet header processors do not describe the same headers.
        DS_PHP_VECTOR_EMPTY = 50014,    //!< The vector of packet header processors is empty.
        DS_DEVICE_HAS_EXISTING_PACKET_TRANSFORMER = 50015,  //!< The device has an existing packet transformer.
        DS_INVALID_PACKET_TRANSFORMER_TYPE = 50016, //!< The packet transformer type does not match an existing transformer type. 
        DS_PACKET_TRANSFORMER_INITIALIZATION_ERROR = 50017, //!< Error initializing packet transformer.
        DS_PACKET_TRANSFORMER_PHP_MISMATCH = 50018, //!< The packet transformers's packet header processor does not match it's neighbors. 
        DS_SOCKET_CREATION_ERROR = 50019,   //!< Error creating socket.
        DS_SOCKET_BIND_ERROR = 50020,   //!< Error binding socket to ip address and port.
        DS_SOCKET_LISTEN_STATE_ERROR = 50021,   //!< Error configuring socket to accept connections.
        DS_SOCKET_INVALID = 50022,  //!< The socket is not connected and cannot support the request. 
        DS_SOCKET_RECEIVE_FROM_ERROR = 50023,   //!< Error receiving packet from the network.
        DS_SOCKET_CONNECTION_ERROR = 50024, //!< Error connecting socket.
        DS_SOCKET_CONNECTION_TIMEOUT_ERROR = 50025, //!< Error caused by timeout waiting for socket to complete connection.
        DS_SOCKET_JOIN_MULTICAST_GROUP_ERROR = 50026,   //!< Error joining multicast group.
        DS_SOCKET_IP_ADDRESS_CONVERSION_ERROR = 50027,  //!< Error converting IP address.
        DS_SOCKET_OPTION_MULTICAST_TTL_ERROR = 50028,   //!< Error setting the multicast time to live parameter.
        DS_SOCKET_OPTION_KEEPALIVE_ERROR = 50029,   //!< Error setting the keepalive parameter.
        DS_ARRAY_SIZE_ERROR = 50030,    //!< The size of the array is not large enough to hold the requested values.
        DS_TIMEOUT = 50031, //!< The timer associated with the request expired.
        DS_DEVICE_KEY_BUFFER_SIZE_ERROR = 50032,    //!< The device key buffer is too small to host the device key.
        DS_PACKET_BUFFER_SIZE_ERROR = 50033,    //!< The packet buffer is too small to host the packet.
        DS_IP_ADDRESS_BUFFER_SIZE_ERROR = 50034,    //!< The IP address buffer is too small to host the IP address.
        DS_PHP_NOT_DEFINED = 50035, //!< The referenced packet header processor was not defined.
        DS_CANNOT_ASSIGN_EXISTING_ALIAS_NAME = 50036,   //!< The alias name currently exists.
        DS_PHP_PACKET_SIZE_FIELD_ERROR = 50037, //!< The bit length of the packet size field cannot exceeds the maximum exceptable value.
        DS_PHP_SYNC_SIZE_ERROR = 50038, //!< The byte length of the sync pattern cannot exceeds the maximum exceptable value.
        DS_PHP_SYNC_BYTE_BOUNDARY_ERROR = 50039,    //!< The sync pattern must be on a byte boundary.
        DS_PHP_PACKET_SEQUENCE_COUNT_FIELD_ERROR = 50040,   //!< The bit length of the packet sequence count field cannot exceeds the maximum exceptable value.
        DS_PHP_PACKET_KEY_FIELD_ERROR = 50041,  //!< The bit length of the packet key field cannot exceeds the maximum exceptable value.
        DS_FILE_OPEN_ERROR = 50042, //!< Error opening the file.
        DS_FILE_CLOSE_ERROR = 50043,    //!< Error opening the file.
        DS_WRITE_TO_LOG_FILE_ERROR = 50044, //!< Error writing message to the log file.
        DS_NO_LOG_FILE_EXISTS = 50045,   //!< No log file exists.
        DS_ERROR_RENAMING_LOG_FILE = 50046, //!< Error renaming log file.
        DS_WINSOCK_DLL_VERSION_ERROR = 50047,   //!< Error calling WSAStartup.
        DS_BUFFER_SIZE_ERROR = 50048,   //!< The value is too large to fit in the associated buffer.
        DS_NULL_POINTER_ERROR = 50049,   //!< Invalid pointer value of NULL.
        DS_RECORDING_CURRENTLY_ACTIVATED = 50050,   //!< The start recording request cannot be executed because recording is already being performed.
        DS_INVALID_BP_LIFETIME = 50051,   //!< Invalid bundle protocol lifetime (lifetime must be greater than zero).
        DS_INVALID_BP_CLASS_OF_SERVICE = 50052,   //!< Invalid bundle protocol class of service.
        DS_INVALID_BP_TRANSMISSION_MODE = 50053,   //!< Invalid bundle protocol transmission mode.
        DS_INVALID_BP_EXPEDITED_PRIORITY_ORDINAL = 50054,   //!< Invalid bundle protocol expedited priority ordinal (ordinal must be less than 255).
        DS_INVALID_BP_CRITICALITY = 50055,   //!< Invalid bundle protocol criticality. 
        DS_INVALID_BP_SERVICE = 50056,   //!< Invalid bundle protocol service (\<lifespan\>/\<cos\>/\<ordinal\>/\<mode\>/\<custody\>).
        DS_FILE_RENAME_ERROR = 50057,   //!< Error renaming the file.
        DS_NAMED_PIPE_CREATION_ERROR = 50058,   //!< Error creating named pipe
        DS_DEVICE_HAS_EXISTING_CIPHER = 50059,   //!< Error device has an existing cipher
        DS_DEVICE_DOES_NOT_SUPPORT_CIPHER = 50060,   //!< Error device has an existing cipher
        DS_NO_CRYPT_LIBRARY_DEVICE_EXISTS = 50061,   //!< Error no crypt library device exists
        DS_SOCKET_OPTION_BROADCAST_ERROR = 50062,   //!< Error setting broadcast socket option

        CFDP_API_CODE_BASE = 51000,
        CFDP_OPEN_FILE_ERROR = 51001,   //!< Error opening a file.
        CFDP_WRITE_FILE_ERROR = 51002,  //!< Error writing to a file.
        CFDP_INVALID_EID = 51003,   //!< Invalid entity ID.
        CFDP_STATUS_REQUEST_ERROR = 51004,  //!< CFDP is not configured to support status requests.
        CFDP_TIMEOUT = 51005,   //!< The timer associated with the request expired.
        CFDP_ARRAY_SIZE_ERROR = 51006,  //!< The size of the array is not large enough to hold the requested values.
        CFDP_FAILED_TO_POPULATE_COLLECTION = 51007,   //!< Failed to populate the CFDP collection class.
        CFDP_INVALID_CLASS_OF_SERVICE = 51008,  //!< CFDP class of service may only be class1 or class2
        CFDP_INVALID_ACK_TIMEOUT = 51009,   //!< Invalid Ack timeout.
        CFDP_INVALID_ACK_LIMIT = 51010,  //!< Invalid Ack limit.
        CFDP_INVALID_NAK_TIMEOUT = 51011,   //!< Invalid Nak timeout.
        CFDP_INVALID_NAK_LIMIT = 51012,//!< Invalid Nak limit.
        CFDP_INVALID_INACTIVITY_TIMEOUT = 51013,    //!< Invalid Inactivity timeout.
        CFDP_INVALID_FILE_CHUNK_SIZE = 51014,   //!< Invalid file chunk size.
        CFDP_INVALID_AGGREGATE_FILE_TRANSFER_RATE = 51015,  //!< Invalid aggregate file transfer rate.
        CFDP_INVALID_SOCKET_QUEUE_SIZE = 51016,//!< Invalid socket queue size
        CFDP_INVALID_CONFIG_FILE = 51017,   //!< Invalid configuration file.
        CFDP_FAILED_TO_INIT_TOOLKIT_CFDP = 51018,   //!< Failed to call InitToolkitCfdp.
        CFDP_INVALID_PRIMITIVE_FILE = 51019,   //!< Invalid primitive file.
        CFDP_EMPTY_PRIMITIVE_LISTS = 51020,   //!< The lists of CFDP primitives are empty.
        CFDP_INVALID_PRIMITIVE_FILE_VERSION = 51021,   //!< Invalid primitive file version.
        CFDP_INVALID_PUT_PRIMITIVE = 51022,   //!< Invalid put primitive.
        CFDP_INVALID_GET_PRIMITIVE = 51023,   //!< Invalid get primitive.
        CFDP_INVALID_FILESTORE_PRIMITIVE = 51024,   //!< Invalid filestore primitive.
        CFDP_INVALID_MESSAGE_PRIMITIVE = 51025,   //!< Invalid message primitive.
        CFDP_INVALID_DROPBOX_PRIMITIVE = 51026,   //!< Invalid dropbox primitive.
        CFDP_INVALID_CONFIG_FILE_VERSION = 51027,   //!< Invalid configuration file version.
        CFDP_INVALID_TRANSACTION_CYCLE_TIME_INTERVAL = 51028,   //!< Invalid transaction cycle time interval.
        CFDP_INVALID_STEPS_PER_TRANSACTION_CYCLE = 51029,  //!< Invalid steps per transaction cycle.
        CFDP_INVALID_LIFESPAN_IN_SEC = 51030,   //!< Invalid lifespan.
        CFDP_INVALID_EXPEDITED_PRIORITY_ORDINAL = 51031,  //!< Invalid expedited priority ordinal.
        CFDP_INVALID_TRANSACTION_RESULT_MESSAGE_TIMEOUT = 51032,  //!< Invalid transaction result message timeout.
        CFDP_INVALID_CONFIG_FILE_LINE_ITEM = 51033,   //!< Invalid configuration file line item.
        CFDP_PRIMITIVE_FILE_DEVICE_MODE_INCOMPATIBILITY = 51034,   //!< Primitive file and device mode are not compatible.
        CFDP_PUT_PRIMITIVE_DEVICE_MODE_INCOMPATIBILITY = 51035,   //!< Put primitive and device mode are not compatible.
        CFDP_GET_PRIMITIVE_DEVICE_MODE_INCOMPATIBILITY = 51036,   //!< Get primitive and device mode are not compatible.
        CFDP_FILESTORE_PRIMITIVE_DEVICE_MODE_INCOMPATIBILITY = 51037,   //!< Filestore primitive and device mode are not compatible.
        CFDP_MESSAGE_PRIMITIVE_DEVICE_MODE_INCOMPATIBILITY = 51038,   //!< Message primitive and device mode are not compatible.
        CFDP_DROPBOX_PRIMITIVE_DEVICE_MODE_INCOMPATIBILITY = 51039,   //!< Dropbox primitive and device mode are not compatible.
        CFDP_INVALID_DEVICE_MODE = 51040,   //!< Invalid CFDP device mode.
        CFDP_INVALID_FILESTORE_ACTION = 51041,  //!< Invalid filestore action.
        CFDP_MESSAGE_LENGTH_ERROR = 51042,  //!< The message length is zero or exceeds the maximum allowed size.
        CFDP_INVALID_LOCAL_ENTITY_ID = 51043,//!< Invalid local entity ID.
        CFDP_INVALID_LOCAL_PORT = 51044,//!< Invalid local port.
        CFDP_INVALID_REMOTE_ENTITY_ID = 51045,//!< Invalid remote entity ID.
        CFDP_INVALID_REMOTE_PORT = 51046,//!< Invalid remote port.
        CFDP_INVALID_AUTO_PAUSE_AND_RESUME_MODE = 51047,  //!< Invalid auto pause and resume mode.
        CFDP_INVALID_AUTO_PAUSE_AND_RESUME_CT_LIMIT = 51048,  //!< Invalid auto pause and resume connection test limit.	
        CFDP_TRANSACTION_DOES_NOT_CURRENTLY_EXIST = 51049,  //!< Transaction does not currently exist.
        CFDP_INVALID_TRANSACTION_COUNT = 51050,  //!< Invalid transaction count.
        CFDP_INVALID_DIRECTORY_ACTION = 51051,  //!< Invalid directory action.
        CFDP_INVALID_CFDP_LIBRARY_IP_ADDRESS = 51052,//!< Invalid CFDP library IP address.
        CFDP_INVALID_CFDP_LIBRARY_PORT = 51053,//!< Invalid CFDP library port.
        CFDP_INVALID_CFDP_CRYPT_CLASS = 51054,//!< Invalid CFDP crypt class.
        CFDP_INVALID_CIPHER_CLASS = 51055,  //!< Invalid cipher class.
        CFDP_INVALID_PEER_PUB_KEY_PATH_AND_FILE_NAME = 51056   //!< Invalid peer public key path and file name.
    }
}