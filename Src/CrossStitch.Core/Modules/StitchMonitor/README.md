## Stitch Monitor Module

This is an internal module and will be automatically created unless a custom variant is provided.

The Stitch Monitor module performs monitoring tasks for Stitches to make sure all stitches are healthy and responsive. The `StitchMonitorModule` will send out heartbeat messages to the Stitches and record their responses, and will also periodically review the list of stitches to see which ones have not responded in a long time.