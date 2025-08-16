namespace UltraBINGO.Net;

// Message initially received by the server to indicate what action to take.
// The contents will be unpacked into a new class indicated by the header property.
public class EncapsulatedMessage {
    public required string MessageType;
    public required object Contents;
}