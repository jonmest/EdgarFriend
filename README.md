# EdgarFriend 

## Overview

This C# project was created to quickly extract fundamental financial data for US companies. It downloads bulk data files from the SEC, parses the data, extracts certain interesting features (see `AcceptedLabels.cs`) and does some data mangling to get information for all quarters.

The bulk data being downloaded from the SEC is about 15 GB in size, so some free disk space is required. However, it should be more efficient than using the SEC EDGAR API since that API is rate limited to ten requests a second, The data is being downloaded to the user's temp folder and automatically deleted after a successful run.

I had to roll my own JSON deserializer (see `Converters` directory) to make the execution time of the program acceptable. It is minimal by design and only picks up the data I was interested in, so feel free to review and adjust it to your own liking. The deserialized data is then processed (see `Processors` directory), again in a way tailored to my liking. For example, companies don't report metrics specifically for the fourth quarter, only for the first, second and third quarter and the whole year, so the fourth quarter is calculated by subtracting the preceding quarters from the annual reporting.

The program then saves the final data in a Postgres database for easy lookup.

## Getting Started

### Prerequisites

- .NET 7 or later
- Access to the internet for downloading data from the SEC website

### Installation

1. Clone the repository to your local machine:
   ```
   git clone [repository URL]
   ```
2. Navigate to the project directory:
   ```
   cd [project directory]
   ```
3. Build the project:
   ```
   dotnet build
   ```

### Usage
Set the following environment variables:
- USER_AGENT (in order to identify to the SEC API)
- DB_HOST
- DB_DATABASE
- DB_USER
- DB_PWD

Create a migration and apply it to the database:
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Run the program using the .NET CLI:
```
dotnet run
```

The program will automatically start downloading and processing the latest financial data from the SEC.

## License

Distributed under the GNU AFFERO GENERAL PUBLIC LICENSE License.