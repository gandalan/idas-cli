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
            var handler = new BenutzerCommands();
            await handler.Login(timeout);
        }, timeoutOption);

        cmd.AddCommand(loginCmd);

        // logout subcommand
        var logoutCmd = new Command("logout", "Logout and revoke the current session token");

        logoutCmd.SetHandler(async () =>
        {
            var handler = new BenutzerCommands();
            await handler.Logout();
        });

        cmd.AddCommand(logoutCmd);

        // list subcommand
        var listCmd = new Command("list", "Get the list of own users");

        listCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new BenutzerCommands();
            await handler.List(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(listCmd);

        // password-reset subcommand
        var passwordResetCmd = new Command("password-reset", "Reset password for a user by email");
        var emailArgument = new Argument<string>("email", "User's email address");
        passwordResetCmd.AddArgument(emailArgument);

        passwordResetCmd.SetHandler(async (email) =>
        {
            var handler = new BenutzerCommands();
            await handler.PasswordReset(email);
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
            var handler = new BenutzerCommands();
            await handler.ChangePassword(username, oldPassword, newPassword);
        }, usernameArgument, oldPasswordArgument, newPasswordArgument);

        cmd.AddCommand(changePasswordCmd);

        return cmd;
    }
}
