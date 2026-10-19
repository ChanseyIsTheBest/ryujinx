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
  `display_sync`, futari-nx's vblank thread).
- Earlier fixes for homebrew with a big address space (from the first banjo-nx emulator runs): `AllocSpace` with a
  fixed offset returned the free block's start instead of the offset; `Map/UnmapProcess(Code)Memory` and
  `SetProcessMemoryPermission` accept the current-process pseudo-handle; DMA remap constant fills in 2D form; some
  diagnostic logging.

Ryujinx is MIT licensed (LICENSE.txt). No keys, firmware or game data are in this repository.
