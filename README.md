# Crany Repository Manager (Lightweight)

Crany Repository Manager is a lightweight NuGet repository manager that provides an efficient and secure way to host and
manage your own private NuGet packages. It's designed to be simple yet powerful, enabling seamless integration with your
CI/CD pipelines.

---

## Purpose

Crany Repository Manager was specifically developed to manage and version **Protobuf** files, ensuring that updated versions are always accessible and synchronized across all clients. This simplifies the distribution and usage of Protobuf files, enabling developers to:

- Easily **version Protobuf files** for different services or applications.
- Manage **dependencies** between Protobuf files seamlessly.
- Ensure that **all clients** have access to the latest Protobuf versions without manual intervention.

With Crany, you can also manage Protobuf dependencies alongside your Protobuf files, simplifying the setup process for all developers and services consuming these files.

---

## Features

- **Lightweight and Self-Contained**: No external dependencies, runs directly on your .NET setup.
- **Secure API Key Authentication**: Ensures only authorized clients can interact with your repository.
- **Flexible Hosting**: Supports self-hosted configurations, including HTTPS with self-signed certificates.
- **Protobuf Dependency Management**: Handles dependencies within Protobuf files to reduce manual configuration.
- **NuGet v3 Protocol**: Fully compatible with the latest NuGet clients.

---

## Getting Started
### Prerequisites

- **.NET SDK** (6.0 or later)
- **NuGet CLI** or `dotnet` command-line tool
- **Self-Signed SSL Certificate** (if using HTTPS)

---
## Installation

1. Clone the Crany Repository Manager repository:
   ```*bash*
   git clone https://github.com/your-repository/Crany-Repository-Manager.git
   cd Crany-Repository-Manager
   ```
2. Build and run the project:
   ```*bash*
   dotnet build
   dotnet run
   ```
3. Configure your client to trust the self-signed certificate (if applicable):
   ```*bash*
   dotnet dev-certs https --trust
   ```

## Usage

### Clearing the Local NuGet Cache

To ensure your client doesn’t use stale or cached package data:

   ```*bash*
   dotnet nuget locals http-cache --clear
   ```

### Pack a Nuspec File
Use the crany pack command to generate a NuGet package from a .nuspec file:

   ```*bash*
   crany pack CustomTypesProto.nuspec --output-dir ./packages
   ```

This will create a NuGet package in the specified output directory.

To push the generated package to a NuGet source, use the following command:

   ```*bash*
   crany pack CustomTypesProto.nuspec --output-dir ./packages --push --api-key 7e5e1274-b271-4ad3-85f0-a1e1925126b2 --source https://localhost:5001/api/v3/index.json
   ```

This will:
1.	Pack the .nuspec file into a NuGet package.
2.	Push the resulting .nupkg file to the specified NuGet source.

### Pushing a Package (Using dotnet)

Alternatively, use dotnet nuget push to manually push a package to the repository:

   ```*bash*
   dotnet nuget push GreetProtos.1.0.3.nupkg \
    --source https://localhost:5001/api/v2/package \
    --api-key 7e5e1274-b271-4ad3-85f0-a1e1925126b2
   ```   

### Consuming the Repository

Add the Crany Repository Manager as a source in your NuGet configuration:

   ```*bash*
   dotnet nuget add source https://localhost:5001/api/v3/index.json \
   --name Crany \
   --username <your-username> \
   --password <your-password>
   ```

### Install packages from the Crany Repository Manager:

   ```*bash*
   dotnet add package GreetProtos --version 1.0.3
   ```

## API Overview
The following endpoints are available in Crany Repository Manager:
1. Package Upload:
   * POST /api/v2/package
   * Requires API Key for authentication.
2. Package Query:
   * GET /api/v3-flatcontainer/{id}/{version}/{filename}.nupkg
   * Returns a specific version of a package.
3. Package Metadata:
   * GET /api/v3/registration5-semver1/{id-lower}/index.json
   * Fetches metadata for a package ID.

## Contributing
Contributions are welcome! Feel free to fork the repository, create issues, and submit pull requests.

## License
This project is licensed under the [MIT License](./LICENSE.MD).

## Questions?
If you encounter issues or have questions, please contact the repository maintainers or open an issue in the GitHub repository.

### Updates
- Added `crany pack` and `crany push` commands with explanations and examples.
- Structured the usage section for clarity.