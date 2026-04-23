# How to generate code using Kiota for SnD.ApiClient.Nexus
This project was generated using [Kiota](https://github.com/microsoft/kiota), a tool to generate API clients from OpenAPI descriptions.

## Kiota installation
The kiota version is locked in the `.tool-versions` file in the root of the repository.
You can install this version using [asdf](https://github.com/asdf-community/asdf-kiota).


## Code generation
Assuming you have downloaded the OpenAPI specification file `swagger.yaml`, you can generate the C# client
code by running the following command in the repository root:

```bash
kiota generate -l CSharp -c NexusGeneratedClient -n KiotaPosts.Client -d ./swagger.yaml -o ./src/SnD.ApiClient.Nexus.Generated
```

