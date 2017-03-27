# Commonly Asked Questions

## Stitches

**Can I use Stitch data messages for reliable or transactional communications?**

No. Stitch data messages are more like UDP than TCP. There's no guarantee about reliability and no hard limitations on performance. CrossStitch does have some mechanisms for marking a message Acknowledged or Failed by the recipient, but that's only if the recipient receives the message and responds to it correctly. These designations are used more for health monitoring and alerting than anything else.

Stitch data messages are not persisted at any point, so if there is a failure either internal to CrossStitch or anywhere along the network, the message can be lost. If you need reliability in the face of possible network failure, use a persisted communications channel like an ESB or a message queue. You can use stitch data messages to transmit connection information for these channels on startup.

A general pattern to follow for using stitch data messages is this:

1. Wait for some kind of event, such as startup or a heartbeat
2. Check to see if the stitch has the information it needs
3. If not, send a request for the necessary information to the stitch/group
4. Process incoming requests hoping to get the value you need.
5. Wait for the next trigger and make sure you have what you need, again.



