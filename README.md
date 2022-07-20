# azdidate
`azdidate` is a command-line utility to easily validate the YAML pipelines in your Azure DevOps projects. It uses the Azure DevOps API to validate your YAML syntax, template references, variable group references and much more.

## Why?
In large environments with (cross-repository) YAML templates, things can get quite complex. A change in a YAML template can potentially break one or more pipelines. `azdidate` can be used to validate your pipelines. 

You can manually run `azdidate`, or create a PR validation pipeline and let it run when changes are made to your pipelines. This way, you'll no longer break your pipelines by accident.

## About
`azdidate` uses the Azure DevOps API to validate the pipelines. When you edit the YAML of a pipeline in the Azure DevOps GUI, you have the option to validate the pipeline. `azdidate` invokes that endpoint when checking the YAML of pipelines in order for the validations to be reliable. 

Using the Azure DevOps API, `azdidate` can validate things like:
- YAML syntax
- Pipelines YAML schema
- YAML [template](https://docs.microsoft.com/en-us/azure/devops/pipelines/process/templates?view=azure-devops) usage
- Used variable groups
- Used environments
- Used service connections
- And more!

## Usage
`azdidate` is available as a minimalistic Docker image, and can be run as follows:
```docker
docker run --rm sietsetro/azdidate:0.1.0 \
    --env ACCESS_TOKEN=<YourPersonalAccessToken> \
    --organisation <YourAzureDevOpsOrganisation> \
    --project <YourAzureDevOpsProject> \ 
    --repository <YourAzureDevOpsRepository>
```

`azdidate` needs a [Personal Access Token](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=Windows) with the scope: `Build (Read & Execute)`.

`azdidate` will return exit code `0` when all checked pipelines are valid, and exit code `-1` when any checked pipeline is invalid.

The following optional arguments are available:

| Argument                   | Description |
| -------------              | ----------- |
| --branch                   | Branch to validate against. Will use the default branch if not specified.                                                                      |
| --ignore-non-existing-yaml | (Default: false) Whether to return a non-zero exit code when the YAML file of a pipeline does not exist in the branch that is checked against. |
| --pipeline-id              | Id of pipeline to validate. When not given, validates all pipelines in the project.                                                            |
| --help                     | Displays the help screen.                                                                                                                      |

## Limitations
Due to limitations in the Azure DevOps REST API, it is not possible to:
- Validate pipelines in external repositories, like GitHub

## Version history
All versions can be found on [Docker Hub](https://hub.docker.com/r/sietsetro/azdidate).

### 0.1.0
- Initial version

## Roadmap
- Retry mechanism for API requests to the Azure DevOps API
- Better architecture
- CI/CD pipelines to build and distribute the Docker image itself
