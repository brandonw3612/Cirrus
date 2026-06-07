<h1 align='center'>
  <image src='assets/Application Icon.png' width='128' />
  <br/>
  Cirrus
  <br/>
  <a href="https://github.com/brandonw3612/Cirrus/blob/main/LICENSE">
    <img alt="GitHub" src="https://img.shields.io/github/license/brandonw3612/Cirrus?label=License">
  </a>
</h1>

Cirrus, previously known as LyricEase, is a third-party client for NetEase Cloud Music. Based on the
latest WinUI framework, Cirrus aims to provide a modern and native user experience on the Windows 11
platform.

## Build

Currently, the project is under active development. We don't provide official pre-built binaries yet, but
you can build the project yourself using this repository. Please note that the project is in an early
stage, and the codebase may be unstable.

### Prerequisites

Please ensure your machine is set up with the following tools and configurations, as suggested by the
[Microsoft documentation](https://learn.microsoft.com/en-us/windows/apps/get-started/start-here):

- **Developer Mode** is enabled.
- **[Visual Studio 2026](https://visualstudio.microsoft.com/downloads/)** is installed with the **Windows App SDK** workloads.
- **[.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)** is installed.

### Build Tips

- Build under the `Debug` configuration if you want to contribute to the project.
- Create an application package under the `Release` configuration if you simply want to use the software. This requires
  a signing certificate to sign the package, which can be created by yourself or obtained from a trusted certificate
  authority. We have made the project NativeAOT-ready and the package is expected to be smaller and deliver better
  performance.

## License and Commercial Use

This project is licensed under the Apache License 2.0 — see the [LICENSE](LICENSE) file for full terms.
Under Apache-2.0 you are allowed to use, copy, modify, and distribute the software, including for commercial purposes,
provided you comply with the license terms (for example, preserving copyright notices and including the required notices).

Author request: The project authors kindly ask that this software not be used for commercial products or services
without explicit permission. This request is not a legal restriction and does not change the Apache-2.0 license.
If you plan to use this project in a commercial product, please contact the maintainers to discuss licensing or to
obtain an explicit commercial license.

## Contributing

Contributions are welcome! Please read the repository's [guidelines for contributing](CONTRIBUTING.md) before opening
issues or submitting pull requests. The contributing guide explains how to report bugs, propose features, run the build
and tests, and prepare pull requests.

Quick contributing steps:

- Fork the repository and create a topic branch (e.g. `feat/<short>`, `fix/<short>`).
- Keep your branch up-to-date with the upstream default branch.
- Run the build and tests locally before submitting a PR (see `CONTRIBUTING.md` for commands).
- Open a PR and reference any related issues.
