# Ryubing, banjo-nx test build

The Ryujinx (Ryubing) build used to test the banjo-nx Switch port (Banjo-Kazooie: Nuts & Bolts) in an emulator. Built
with:

    dotnet build src/Ryujinx/Ryujinx.csproj -c Release -o build

(.NET 10 SDK; `DOTNET_ROOT` set to it when running `build/Ryujinx.exe`). The built folder is attached to the
repository's release as a zip.

## Changes from upstream

- **PRMT** (Maxwell's byte permute): implemented in `src/Ryujinx.Graphics.Shader/Instructions/InstEmitPrmt.cs` (the
  default mode: each result byte is the selector nibble's byte of {C:A}, or its sign), the stubs removed from
  `InstEmit.cs`. banjo-nx's shader compiler (uam) writes byte swaps as PRMT; without it the characters vanished.
  `CodeGenVersion` in `DiskCacheHostStorage.cs` is 7355 so older translated shaders are not reused.
- **Quiet start** (2026-10-19): the console window and the main window start minimised (also when a game starts) and
  the audio volume starts at 0, for the automated test runs (`Program.QuietStart`; `ConsoleHelper.MinimizeConsoleWindow`;
  `ConfigurationState.CreateHleConfiguration`; `AppHost`; `MainWindow`). Set the environment variable
  `RYUBING_INTERACTIVE=1` for the normal behaviour.
- **A display opened again** (2026-10-19): `IApplicationDisplayService.OpenDisplayImpl` returns the same id for a
  display the session already holds open, as the console does, instead of `AlreadyOpened`: libnx's default window
  holds "Default" open, and a game's vsync thread opens it again for `viGetDisplayVsyncEvent` (banjo-nx's
  `display_sync`, futari-nx's vblank thread). Correction (2026-10-20): the console REFUSES the second open (banjo-nx
  package 36i's console logs: "cannot open the display"); banjo-nx now makes libnx's default window itself and uses its
  display. This leniency stays (harmless), but a port must not rely on it: test the vsync thread on the console.
- **The display's vsync event at 60 Hz with game vsync off** (2026-10-20): with swap interval 0 SurfaceFlinger
  signalled `VsyncEvent` after every composition, i.e. at the game's own presents, so a game pacing frames by
  `viGetDisplayVsyncEvent` (banjo-nx's `banjo_display_sync`) paced them by its own presents and stalled or hung (its
  runs 65, 67, 68). `HandleComposition` now signals it on a grid of the target refresh (`_nextVsyncTicks`, a late wake
  skipping the refreshes it missed), as the console's display does whatever the swap interval; it still composes as
  soon as a frame is queued. Emulator run 112: display sync on, refresh 16.67 ms, no stall.
- **Vertex colours' R/B swap** (2026-10-20, found by the Peggle 2 port): `StateUpdater.UpdateVertexAttribState`
  honours VERTEX_ATTRIB_FORMAT bit 31 (swap R and B: deko3d's `DkVtxAttribState.isBgra`, which plume sets for every
  D3DCOLOR vertex colour) by taking R8G8B8A8Unorm as B8G8R8A8Unorm (R10G10B10A2Unorm as B10G10R10A2Unorm);
  `UnpackFormat` masks the bit off, so vertex colours came out swapped in the emulator while the console honours it.
- **C++ exceptions** (2026-10-20, found by the Futari port): ARMeilleure's MRS reads GCSPR_EL0 (the guarded control
  stack pointer) as 0. GCC 16's libgcc unwinder reads it behind a `chkfeat` test that skips it without GCS; the
  translator reached the skipped block anyway and threw "Unknown MRS 0xD53B2521", so every C++ exception (even a caught
  one) stopped the emulator (`InstEmitSystem.Mrs`).
- Earlier fixes for homebrew with a big address space (from the first banjo-nx emulator runs): `AllocSpace` with a
  fixed offset returned the free block's start instead of the offset; `Map/UnmapProcess(Code)Memory` and
  `SetProcessMemoryPermission` accept the current-process pseudo-handle; DMA remap constant fills in 2D form; some
  diagnostic logging.

Ryujinx is MIT licensed (LICENSE.txt). No keys, firmware or game data are in this repository.
