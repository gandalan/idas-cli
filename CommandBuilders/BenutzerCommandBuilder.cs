using System.CommandLine;

public static class BenutzerCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("benutzer", "Manage users and authentication");

        // login subcommand
        var loginCmd = new Command("login", "Login using Single Sign-On (SSO)");
        var timeoutOption = new Option<int>("--timeout", () => 60, "Timeout in seconds for SSO callback");
        loginCmd.AddOption(timeoutOption);

        loginCmd.SetHandler(async (timeout) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var handler = new BenutzerCommands();
                await handler.Login(timeout);
            });
        }, timeoutOption);

        cmd.AddCommand(loginCmd);

        // logout subcommand
        var logoutCmd = new Command("logout", "Logout and revoke the current session token");

        logoutCmd.SetHandler(async () =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var handler = new BenutzerCommands();
                await handler.Logout();
            });
        });

        cmd.AddCommand(logoutCmd);

        // list subcommand
        var listCmd = new Command("list", "Get the list of own users");

        listCmd.SetHandler(async (format, filename) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var commonParams = new CommonParameters(format, filename);
                var handler = new BenutzerCommands();
                await handler.List(commonParams);
            });
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(listCmd);

        // password-reset subcommand
        var passwordResetCmd = new Command("password-reset", "Reset password for a user by email");
        var emailArgument = new Argument<string>("email", "User's email address");
        passwordResetCmd.AddArgument(emailArgument);

        passwordResetCmd.SetHandler(async (email) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var handler = new BenutzerCommands();
                await handler.PasswordReset(email);
            });
        }, emailArgument);

        cmd.AddCommand(passwordResetCmd);

        // change-password subcommand
        var changePasswordCmd = new Command("change-password", "Change password for the current user");
        var usernameArgument = new Argument<string>("username", "Username or email");
        var oldPasswordArgument = new Argument<string>("old-password", "Current password");
        var newPasswordArgument = new Argument<string>("new-password", "New password");
        changePasswordCmd.AddArgument(usernameArgument);
        changePasswordCmd.AddArgument(oldPasswordArgument);
        changePasswordCmd.AddArgument(newPasswordArgument);

        changePasswordCmd.SetHandler(async (username, oldPassword, newPassword) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var handler = new BenutzerCommands();
                await handler.ChangePassword(username, oldPassword, newPassword);
            });
        }, usernameArgument, oldPasswordArgument, newPasswordArgument);

        cmd.AddCommand(changePasswordCmd);

        // get subcommand - Get user by GUID with roles
        var getCmd = new Command("get", "Get user by GUID with roles");
        var benutzerGuidArg = new Argument<Guid>("benutzerGuid", "User GUID");
        getCmd.AddArgument(benutzerGuidArg);

        getCmd.SetHandler(async (format, filename, benutzerGuid) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var commonParams = new CommonParameters(format, filename);
                var handler = new BenutzerCommands();
                await handler.Get(commonParams, benutzerGuid);
            });
        }, GlobalOptions.Format, GlobalOptions.FileName, benutzerGuidArg);

        cmd.AddCommand(getCmd);

        // add-role subcommand - Add a role to a user
        var addRoleCmd = new Command("add-role", "Add a role to a user");
        var addRoleUserArg = new Argument<Guid>("benutzerGuid", "User GUID");
        var addRoleRoleArg = new Argument<Guid>("rolleGuid", "Role GUID");
        addRoleCmd.AddArgument(addRoleUserArg);
        addRoleCmd.AddArgument(addRoleRoleArg);

        addRoleCmd.SetHandler(async (benutzerGuid, rolleGuid) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var handler = new BenutzerCommands();
                await handler.AddRole(benutzerGuid, rolleGuid);
            });
        }, addRoleUserArg, addRoleRoleArg);

        cmd.AddCommand(addRoleCmd);

        // remove-role subcommand - Remove a role from a user
        var removeRoleCmd = new Command("remove-role", "Remove a role from a user");
        var removeRoleUserArg = new Argument<Guid>("benutzerGuid", "User GUID");
        var removeRoleRoleArg = new Argument<Guid>("rolleGuid", "Role GUID");
        removeRoleCmd.AddArgument(removeRoleUserArg);
        removeRoleCmd.AddArgument(removeRoleRoleArg);

        removeRoleCmd.SetHandler(async (benutzerGuid, rolleGuid) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var handler = new BenutzerCommands();
                await handler.RemoveRole(benutzerGuid, rolleGuid);
            });
        }, removeRoleUserArg, removeRoleRoleArg);

        cmd.AddCommand(removeRoleCmd);

        // set-rollen subcommand - Set user roles from JSON file (replaces all)
        var setRollenCmd = new Command("set-rollen", "Set user roles from JSON file (replaces all)");
        var setRollenUserArg = new Argument<Guid>("benutzerGuid", "User GUID");
        var setRollenFileArg = new Argument<string>("file", "Path to JSON file with roles array");
        setRollenCmd.AddArgument(setRollenUserArg);
        setRollenCmd.AddArgument(setRollenFileArg);

        setRollenCmd.SetHandler(async (benutzerGuid, file) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var handler = new BenutzerCommands();
                await handler.SetRollen(benutzerGuid, file);
            });
        }, setRollenUserArg, setRollenFileArg);

        cmd.AddCommand(setRollenCmd);

        return cmd;
    }
}
