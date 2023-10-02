namespace UsersApi.Commands
{
    public record ChangeUserName(Guid Id, string NewName);

    public record RegisterUser(
        Guid Id,
        string Name,
        string Email
        );

    public record UpdateAddress(Guid Id, string Address);
}
