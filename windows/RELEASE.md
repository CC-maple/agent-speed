# Windows offline build

Compiled executables are distributed as GitHub Release assets in this fork:

https://github.com/CC-maple/agent-speed/releases

The local `build/` directory contains copies of the published executables and is
ignored by Git. Binary files are not stored in Git or Git LFS.

- `AgentSpeed.exe`: self-contained Windows x64 offline application with the
  optional Traffic Helper embedded. Run this file to start Agent Speed.
- `AgentSpeedTrafficHelper.exe`: standalone Windows x64 Traffic Helper 1.0.1.
  Installation requires administrator approval. Enable per-agent traffic and
  install the helper from the main application's interface when needed.

See [OFFLINE.md](OFFLINE.md) for build, installation and local exchange-rate
instructions, and [../CHANGELOG.md](../CHANGELOG.md) for changes from upstream.
