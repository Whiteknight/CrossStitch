# CrossStitch.Stitch

`CrossStitch.Stitch` is the C# reference library for creating Stitch programs in C# to host under CrossStitch. The protocol for creating a Stitch is relatively simple and straight-forward, and this library is **not required** to create a functioning Stitch. Because the Stitch protocol is based only on OS-level features (command-line arguments and STDIN/STDOUT streams), you can create your own interface or use a different programming language entirely with no side effects or repercussions.

## Adaptors

CrossStitch supports different adaptor types for different purposes. Depending on your use-case, you may decide on different types of adaptors:

