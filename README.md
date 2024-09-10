# `idas-cli` Utility

## Quick start

Check out and compile with .NET 8 installed, or download one of the releases.

### Authentication/Login

At least once you need to login to IDAS. After a successful login, a file named `token` will be created in your working directory containing the `AuthToken`. After the token is expired, you will need to login again.

```
idas login --user xxx --password yyy --appguid 123 --env dev
```

You can save a lot of time and hassle if you configure a `.env` file, a sample is included. That reduces the login command to 

```
idas login
```

## Usage

```
idas --help
```

lists all available commands, and --help after a command will list the command's arguments and options:

```
idas vorgang --help
idas vorgang get --help
```